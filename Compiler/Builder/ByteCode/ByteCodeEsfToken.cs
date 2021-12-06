namespace Builder.ByteCode
{
    /*
        This doc comment explains the exceptions system as a whole since ESF Tokens are a critical piece of that and cannot be
        explained without this context.

        An ESF token is a token that goes in the ByteBuffer at the PC where the try block begins. It indicates how far the
        catch and the finally blocks are from the beginning of the try block.

        Technically the offset to the "catch" is ambiguous because there can be multiple catch blocks per try block. In the byte
        code, there is a block of code that sorts out the exception types, called an Exception Sort block which precedes the actual
        catch block code. The beginning of this Exception sorting block is the location stored in ESF tokens, hence the name.
        F of course stands for the finally block.

        Consider the following code:

        A();
        B();
        try {
            C();
            D();
            try {
                E();
                F();
            } catch (Exception e1) {
                G();
                H();
            }
            I();
        } catch (FooException e2) {
            J();
            K();
        } finally {
            L();
            M();
        }
        N();
        O();

        This creates some byte code. This byte code has EsfTokens which indicate how far the exception sort block and
        finally are from the beginning of the try block...

        Also note that JUMPs use offsets that assume +1 will be added on the next VM cycle, whereas the EsfTokens are based
        on positional data and so their offsets are not 1-off.

        Assuming some ops get compiled before this snippet (I arbitrarily chose 0x120), this is the byte code that gets generated:

        ...
        0x120:   A
        0x121:   B
        0x122:   C   [ESF TOKEN] Catch is +11, Finally is +17
        0x123:   D
        0x124:   E   [ESF TOKEN] Catch: +3, Finally: +6
        0x125:   F
        0x126:   Jump +4 (to 0x12A)
        0x127:   EX_HANDLE_TOGGLE true
        0x128:   G
        0x129:   H
        0x12A:   FINAL_END (note that there's always a final block that ends with FINAL_END. If there is no final block in the original code, one is added with just this op
        0x12B:   I
        0x12C:   Jump +7 (to 0x133)
        0x12D:   EX_HANDLE_TOGGLE true
        0x12E:   JUMP_IF_EX_OF_TYPE: FooException
        0x12F:   EX_HANDLE_TOGGLE false
        0x130:   JUMP +3 (to 0x133)
        0x131:   J
        0x132:   K
        0x133:   L
        0x134:   M
        0x135:   FINAL_END
        0x136:   N
        0x137:   O
        ...

        After these offsets are determined and the final byte code size is also determined, there is an ESF_INIT op
        somewhere before FINALIZE_INIT in the byte code with lots of arguments. These arguments are sets of 3 numbers
        representing the PC of the ESF token location (beginning of the try) followed by the 2 values in the ESF Token.

        An EsfInfo array is generated when the ESF_INIT op runs. This array's indices are PC's and include information
        for where the catch and finally blocks are. These tokens are applied in a stack where the topmost token in the
        stack is applied. The tokens are pushed in order and popped when they are no longer applicable.

        ...
        0x120:   A   Catch: NONE, Finally: NONE
        0x121:   B   Catch: NONE, Finally: NONE
        0x122:   C   Catch: +11, Finally +17 <-- ESF token was declared here
        0x123:   D   Catch: +10, Finally +16
        0x124:   E   Catch: +3, Finally: +6 <-- a new ESF token was declared here and overshadows the other one
        0x125:   F   Catch: +2, Finally: +5
        0x126:   Jump +4
        0x127:   EX_HANDLE_TOGGLE true
        0x128:   G   Catch: NONE, Finally: +2 <-- note that the outer catch does not get inherited here. Execution must go to the finally first, at which point, the outer catch will pick it up
        0x129:   H   Catch: NONE, Finally: +1
        0x12A:   FINAL_END   Catch: +3, Finally: +9 <-- the FINAL_END indicates that the ESF token should be popped and the previously shadowed values are now used.
        0x12B:   I   Catch: +2, Finally: +8
        0x12C:   Jump +7 (to 0x133)
        0x12D:   EX_HANDLE_TOGGLE true
        0x12E:   JUMP_IF_EX_OF_TYPE: FooException
        0x12F:   EX_HANDLE_TOGGLE false
        0x130:   JUMP +3 (to 0x133)
        0x131:   J   Catch: NONE, Finally: +2 <-- once the catch is passed from the current token, catch is set to NONE
        0x132:   K   Catch: NONE, Finally: +1
        0x133:   L   Catch: NONE, Finally: NONE
        0x134:   M   Catch: NONE, Finally: NONE
        0x135:   FINAL_END
        0x136:   N   Catch: NONE, Finally: NONE
        0x137:   O   Catch: NONE, Finally: NONE
        ...

        When an exception is thrown, the VM will jump to the catch if one is present, otherwise it'll go to the final offset
        if one is present. If neither are present, then it will pop the stack until it finds one of those offsets declared
        on its current PC. If it gets to the root stack frame, then that's an uncaught exception. Uncaught exceptions
        will still get caught in Exception sort blocks even if there is no type that applies and go on to finally blocks
        when present, but the FINAL_END op will ensure that it continues to bubble after the final block code runs.

        Returning from a function will also check to see if there is a final block. If there is, it will set the stack frame's
        return value, but not return and instead jump to the final block. FINAL_END will act like a return if there is a
        return value set. Alternatively, if the final block contains a return itself, it will overwrite the previously set return
        value.

        // This code returns false.
        try {
            return true; // true is set as the return value and then
        } catch (Exception e) {
            return 42; // not run
        } finally {
            return false; // overwrites the true in the stack frame's return value.
        }

        It is also important to note that return has a responsibility to check if there is an uncaught exception in addition
        to the presence of a final block, in which case it must be treated as a bubble instead of a return.

        A bubbling exception and its caught/uncaught status is stored on the current ExecutionContext.
    */
    internal class ByteCodeEsfToken
    {
        // while the actual offset to the exception-sort block and the final block from specifically the
        // beginning of the try block, it gives relative position that can be used at runtime that can be
        // used to individually set the ESF for each row.
        public int ExceptionSortPcOffsetFromTry { get; set; }
        public int FinallyPcOffsetFromTry { get; set; }
    }
}
