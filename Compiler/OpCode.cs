namespace Crayon
{
	internal enum OpCode
	{
		ADD_LITERAL, // 1: type, 2: value (0 or 1 for false/true or an integer. String values are in the string arg. Float values are as well, and are parsed.)
		ADD_NAME, // name is string arg. ID is the order in which this was encountered.
		ASSIGN_FUNCTION_ARG, // 1: scope ID, 2: function arg value index to assign to variable
		ASSIGN_FUNCTION_ARG_AND_JUMP, // 1: scope ID, 2: function arg value index to assign to variable, 3: PC offset to jump if arg is present (otherwise don't assign and keep going)
		ASSIGN_INDEX, // 1: 0 or 1 for whether to push the assigned value back on the stack when done. value stack: [root, index, value]
		ASSIGN_STEP, // name ID of step. value stack: [root, value]
		ASSIGN_THIS_STEP, // name ID of step. value stack: [value]
		ASSIGN_VAR, // 1: local scope ID
		BINARY_OP,
		BOOLEAN_NOT, // no args.
		BREAK, // no ops. This should be resolved into a jump before actually being run.
		BUILD_SWITCH_INT, // 2n args: (1: integer key, 2: offset value) <- repeat, the order that these appear indicates the switch ID
		BUILD_SWITCH_STRING, // 1: switch ID, 2: offset, string arg: value
		CALL_BASE_CONSTRUCTOR, // 1: num args passed
		CALL_CONSTRUCTOR, // 1: arg count, 2: class name ID, 3: output used | special cache 1: Class ID
		CALL_FUNCTION, // 1: num args passed, 2: 1|0 is value used?
		CALL_FUNCTION2, // 1: type (see FunctionInvoationType enum), 2: num args passed, 3: function ID (if known), 4: output used, 5: class ID (if available)
		CALL_FUNCTION_ON_GLOBAL, // 1: global scope ID, 2: num args passed, 3: 1|0 is value used?
		CALL_LIB_FUNCTION, // 1: lib function ID, 2: num args passed, 3: 1|0 is value used?
		CLASS_DEFINITION, // Super complicated. See documentation in OO_readme.txt.
		CLASS_DEFINITION2, // It's complicated. See initializeClass method in MetadataInitializer.cry
		CONTINUE, // no ops. This should be resolved into a jump before actually being run.
		DEF_ORIGINAL_CODE, // 1: file ID, string arg: source code of that file with a preceding line for the file name.
		DEF_DICTIONARY, // 1: size
		DEF_LIST, // 1: size
		DEREF_DOT, // 1: step ID
		DEREF_DOT_ON_BASE, // 1: step ID
		DUPLICATE_STACK_TOP, // 1: how many stack items should be duplicated?. get the top n of the stack, and just duplicate it
		FINALIZE_INITIALIZATION, // no ops. This indicates that builder data (e.g. List<Value> literalTableBuilder) should be converted into final static data (Value[] literalTable).
		FUNCTION_DEFINITION, // 1: function name ID, 2: PC offset where first line of function is, 3: total number of args; check to make sure not exceeded
		FUNCTION_DEFINITION2, // 1: function ID, 2: function name ID (or 0 for constructors), 3: min args, 4: max args, 5: type (0 - function, 1 - method, 2 - static method, 3 - constructor, 4 - static constructor), 6: class ID (if applicable), 7: Jump (skip function body)
		INDEX,
		ITERATION_STEP, // stack is in the following state: [index, local scope ID, list]. If the index exceeds the length of the list, the loop stops and jumps over the body of the loop, which is arg 1.
		JUMP,
		JUMP_IF_FALSE,
		JUMP_IF_FALSE_NO_POP,
		JUMP_IF_TRUE,
		JUMP_IF_TRUE_NO_POP,
		LIST_SLICE, // 1: begin slice index is present, 2: end slice index is present, 3: step is present
		LITERAL, // 1: literal ID in the literal table
		LITERAL_STREAM, // repeated version of the LITERAL op. Literals are listed in reverse order.
		NEGATIVE_SIGN, // no args. pop, flip, push.
		POP, // no args. pop value from value stack.
		POP_IF_NULL_OR_JUMP, // if the last item on the value stack is null, pop it. If it isn't, then jump. 1: jump distance.
		PUSH_FUNC_REF, // push a verified function pointer to the stack, 1: function ID, 2: type, 3: class ID for static initialization check (or 0 if no check is necessary)
		RETURN,
		RETURN_NULL,
		SPRITE_SHEET_BUILDER, // See SpriteSheetBuilder.GenerateManifestAndProduceSheetNameIdMapping() for documentation.
		STACK_INSERTION_FOR_INCREMENT, // duplicates the top element of the stack but pushes it 3 spots back. [..., a, b, c] --> [..., c, a, b, c]
		STACK_SWAP_POP, // swaps the last 2 items on the stack and then pops the (new) last one.
		SWITCH_INT, // 1: integer switch ID, 2: offset for default case
		SWITCH_STRING, // 1: string switch ID, 2: offset for default case
		THIS, // pushes the current object context onto the stack.
		TOKEN_DATA, // 1: PC of where this token data applies (you must add the value of USER_CODE_START at runtime), 2: line, 3: col, 4: file ID
		USER_CODE_START, // 1: PC of where the user-compiled code begins. PC in token information will add this number.
		VARIABLE, // 1: name id, 2: local scope ID, [optional] 3: global scope ID
		VARIABLE_GLOBAL, // 1: name id, 2: global scope ID
		VERIFY_TYPE_IS_ITERABLE, // verifies the last item on the stack is a list
	}
}
