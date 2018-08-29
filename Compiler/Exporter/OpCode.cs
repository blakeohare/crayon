namespace Exporter
{
    public enum OpCode
    {
        ADD_LITERAL, // 1: type, 2: value (0 or 1 for false/true or an integer. String values are in the string arg. Float values are as well, and are parsed.)
        ADD_NAME, // name is string arg. ID is the order in which this was encountered.
        ASSIGN_CLOSURE, // 1: closure variable ID
        ASSIGN_INDEX, // 1: 0 or 1 for whether to push the assigned value back on the stack when done. value stack: [root, index, value]
        ASSIGN_LOCAL, // 1: local scope ID
        ASSIGN_STATIC_FIELD, // 1: class ID, 2: field ID
        ASSIGN_STEP, // name ID of step. value stack: [root, value]
        ASSIGN_THIS_STEP, // name ID of step. value stack: [value]
        BINARY_OP,
        BOOLEAN_NOT, // no args.
        BREAK, // 0: flag if this has been set, 1: PC offset to jump to (generally BREAK gets resolved into JUMP unless a finally block needs to run.)
        BUILD_SWITCH_INT, // 2n args: (1: integer key, 2: offset value) <- repeat, the order that these appear indicates the switch ID
        BUILD_SWITCH_STRING, // 1: switch ID, 2: offset, string arg: value
        CALL_FUNCTION, // 1: type (see FunctionInvoationType enum), 2: num args passed, 3: function ID (if known), 4: output used, 5: class ID (if available)
        CALL_LIB_FUNCTION, // 1: lib function ID, 2: num args passed, 3: 1|0 is value used?
        CALL_LIB_FUNCTION_DYNAMIC,
        CLASS_DEFINITION, // It's complicated. See initializeClass method in MetadataInitializer.cry
        CNI_INVOKE,
        CNI_REGISTER,
        COMMAND_LINE_ARGS, // pushes a list of the command line args to the stack.
        CONTINUE, // 0: flag if this has been set, 1: PC offset to jump to (generally BREAK gets resolved into JUMP unless a finally block needs to run.)
        CORE_FUNCTION, // 1: function ID
        DEF_ORIGINAL_CODE, // 1: file ID, string arg: source code of that file with a preceding line for the file name.
        DEF_DICTIONARY, // 1: size
        DEF_LIST, // 1: size
        DEREF_CLOSURE, // 1: var ID
        DEREF_DOT, // 1: step ID
        DEREF_INSTANCE_FIELD, // 1: member ID
        DEREF_STATIC_FIELD, // 1: class ID, 2: static member ID
        DUPLICATE_STACK_TOP, // 1: how many stack items should be duplicated?. get the top n of the stack, and just duplicate it
        EQUALS, // 1: 1 to reverse results
        ESF_LOOKUP, // [4n args, n = ESF token count] 4n+0: PC of try, 4n+1: PC of exception-sorter, 4n+2: PC of finally, 4n+3: value stack depth
        EXCEPTION_HANDLED_TOGGLE, // 1: boolean (0|1) indicating if the ExecutionContext's current exception should be marked as handled.
        FINALIZE_INITIALIZATION, // no ops. This indicates that builder data (e.g. List<Value> literalTableBuilder) should be converted into final static data (Value[] literalTable).
        FINALLY_END, // indicates the end of a finally block. Responsible for bubbling exceptions or returning from the function if appropriate 1: break JUMP offset, 2: continue JUMP offset, 3: 0|1 bool if break offset has been set, 4: 0|1 bool if continue offset has been set
        FUNCTION_DEFINITION, // 1: function ID, 2: function name ID (or 0 for constructors), 3: min args, 4: max args, 5: type (0 - function, 1 - method, 2 - static method, 3 - constructor, 4 - static constructor), 6: class ID (if applicable), 7: locals count, 8: Jump (skip function body)
        INDEX,
        IS_COMPARISON, // pops stack, checks if value is an instance of the given class, pushes a boolean. 1: class ID
        ITERATION_STEP, // stack is in the following state: [index, local scope ID, list]. If the index exceeds the length of the list, the loop stops and jumps over the body of the loop, which is arg 1.
        JUMP,
        JUMP_IF_EXCEPTION_OF_TYPE, // 1: offset to jump, 2+: list of class ID's
        JUMP_IF_FALSE,
        JUMP_IF_FALSE_NO_POP,
        JUMP_IF_TRUE,
        JUMP_IF_TRUE_NO_POP,
        LAMBDA,
        LIB_DECLARATION, // 1: library reference ID#, string: name of the library
        LIST_SLICE, // 1: begin slice index is present, 2: end slice index is present, 3: step is present
        LITERAL, // 1: literal ID in the literal table
        LITERAL_STREAM, // repeated version of the LITERAL op. Literals are listed in reverse order.
        LOCAL, // pushes a local value onto the stack. 1: local ID, 2: name ID
        LOC_TABLE, // 1: class ID, 2: member count n, [locale ID, n member name ID's] * k locales
        NEGATIVE_SIGN, // no args. pop, flip, push.
        POP, // no args. pop value from value stack.
        POP_IF_NULL_OR_JUMP, // if the last item on the value stack is null, pop it. If it isn't, then jump. 1: jump distance.
        PUSH_FUNC_REF, // push a verified function pointer to the stack, 1: function ID, 2: type, 3: class ID for static initialization check (or 0 if no check is necessary)
        RETURN,
        STACK_INSERTION_FOR_INCREMENT, // duplicates the top element of the stack but pushes it 3 spots back. [..., a, b, c] --> [..., c, a, b, c]
        STACK_SWAP_POP, // swaps the last 2 items on the stack and then pops the (new) last one.
        SWITCH_INT, // 1: integer switch ID, 2: offset for default case
        SWITCH_STRING, // 1: string switch ID, 2: offset for default case
        THIS, // pushes the current object context onto the stack.
        THROW, // throw an exception (exception is popped from the value stack)
        TOKEN_DATA, // 1: PC of where this token data applies (you must add the value of USER_CODE_START at runtime), 2: line, 3: col, 4: file ID
        USER_CODE_START, // 1: PC of where the user-compiled code begins. PC in token information will add this number.
        VALUE_STACK_DEPTH, // (2n args), 2n + 0: PC, 2n + 1: change in value stack depth typically as the result of a foreach loop. This is just the base depth outside of expressions. These are flattened into absolute depths and create a per-PC lookup for jumps such as breaks and try/catch/finally stuff.
        VERIFY_TYPE_IS_ITERABLE, // verifies the last item on the stack is a list
    }
}
