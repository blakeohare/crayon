namespace Crayon
{
	internal enum OpCode
	{
		ADD_LITERAL, // 1: type, 2: value (0 or 1 for false/true or an integer. String values are in the string arg. Float values are as well, and are parsed.)
		ADD_NAME, // name is string arg. ID is the order in which this was encountered.
		ASSIGN_FUNCTION_ARG, // 1: variable name ID, 2: function arg value index to assign to variable
		ASSIGN_FUNCTION_ARG_AND_JUMP, // 1: variable name ID, 2: function arg value index to assign to variable, 3: PC offset to jump if arg is present (otherwise don't assign and keep going)
		ASSIGN_INDEX, // 1: 0 or 1 for whether to push the assigned value back on the stack when done. value stack: [root, index, value]
		ASSIGN_STEP, // name ID of step. value stack: [root, value]
		ASSIGN_THIS_STEP, // name ID of step. value stack: [value]
		ASSIGN_VAR, // 1: variable name ID
		BINARY_OP,
		BOOLEAN_NOT, // no args.
		BREAK, // no ops. This should be resolved into a jump before actually being run.
		BUILD_SWITCH_INT, // 2n args: (1: integer key, 2: offset value) <- repeat, the order that these appear indicates the switch ID
		BUILD_SWITCH_STRING, // 1: switch ID, 2: offset, string arg: value
		CALL_BASE_CONSTRUCTOR, // 1: num args passed
		CALL_CONSTRUCTOR, // 1: arg count, 2: class name ID, 3: output used | special cache 1: Class ID
		CALL_FUNCTION, // 1: num args passed
		CALL_FRAMEWORK_FUNCTION, // 1: function ID (not identifier ID). Since these are defined by the framework, function count is verified at compile time and popped accordingly at runtime by the op itself.
		CALL_FUNCTION_ON_VARIABLE, // 1: function ID, 2: num args passed
		CLASS_DEFINITION, // Super complicated. See documentation in OO_readme.txt.
		CONTINUE, // no ops. This should be resolved into a jump before actually being run.
		DEF_ORIGINAL_CODE, // 1: file ID, string arg: source code of that file with a preceding line for the file name.
		DEF_DICTIONARY, // 1: size
		DEF_LIST, // 1: size
		DEREF_DOT, // 1: step ID
		DUPLICATE_STACK_TOP, // 1: how many stack items should be duplicated?. get the top n of the stack, and just duplicate it
		FINALIZE_INITIALIZATION, // no ops. This indicates that builder data (e.g. List<Value> literalTableBuilder) should be converted into final static data (Value[] literalTable).
		FUNCTION_DEFINITION, // 1: function name ID, 2: PC offset where first line of function is, 3: total number of args; check to make sure not exceeded
		INDEX,
		ITERATION_STEP, // stack is in the following state: [index, variable ID, list]. If the index exceeds the length of the list, the loop stops and jumps over the body of the loop, which is arg 1.
		JUMP,
		JUMP_IF_FALSE,
		JUMP_IF_FALSE_NO_POP,
		JUMP_IF_TRUE,
		JUMP_IF_TRUE_NO_POP,
		LITERAL, // 1: literal ID in the literal table
		LITERAL_STREAM, // repeated version of the LITERAL op. Literals are listed in reverse order.
		NEGATIVE_SIGN, // no args. pop, flip, push.
		POP, // no args. pop value from value stack.
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
		VARIABLE, // 1: id
		VARIABLE_STREAM, // first n: id, followed by -1, followed by triplets of token data (line, col, fileID)
		VERIFY_TYPE_IS_ITERABLE, // verifies the last item on the stack is a list
	}
}
