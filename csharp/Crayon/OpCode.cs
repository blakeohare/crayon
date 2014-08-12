namespace Crayon
{
	internal enum OpCode
	{
		ASSIGN_FUNCTION_ARG, // 1: variable name ID, 2: function arg value index to assign to variable
		ASSIGN_FUNCTION_ARG_AND_JUMP, // 1: variable name ID, 2: function arg value index to assign to variable, 3: PC offset to jump if arg is present (otherwise don't assign and keep going)
		ASSIGN_INDEX, // no args. value stack: [root, index, value]
		ASSIGN_STEP,
		ASSIGN_VAR, // 1: variable name ID
		BINARY_OP,
		BREAK, // no ops. This should be resolved into a jump before actually being run.
		CALL_FUNCTION, // 1: num args passed
		CALL_FRAMEWORK_FUNCTION, // 1: function ID (not identifier ID). Since these are defined by the framework, function count is verified at compile time and popped accordingly at runtime by the op itself.
		CALL_FUNCTION_ON_VARIABLE, // 1: function ID, 2: num args passed
		CONTINUE, // no ops. This should be resolved into a jump before actually being run.
		DEF_ORIGINAL_CODE,
		DEF_LIST, // 1: size
		DEF_DICTIONARY,
		DEREF_DOT, // 1: step ID
		DUPLICATE_STACK_TOP, // no args. get the top of the stack, and push it again.
		FUNCTION_DEFINITION, // 1: function name ID, 2: PC offset where first line of function is, 3: total number of args; check to make sure not exceeded
		INDEX,
		INDEX_INT,
		INDEX_STRING,
		JUMP,
		JUMP_IF_FALSE,
		JUMP_IF_FALSE_NO_POP,
		JUMP_IF_TRUE,
		JUMP_IF_TRUE_NO_POP,
		LITERAL, // populates the special cache with the entry this refers to. 1: type ID, 2: value ID (value for int, 1/0 for bool, ignored for null, lookup table ID for float/string)
		LITERAL_STREAM, // repeated version of the LITERAL op. Literals are listed in reverse order.
		NEGATIVE_SIGN, // no args. pop, flip, push.
		POP, // no args. pop value from value stack.
		RETURN,
		RETURN_NULL,
		STEP,
		STREAM_LITERALS, // 2n: See detailed comment in the STREAM_LITLERALS implementation in interpreter.cry
		STREAM_VARIABLES, // n: push all these variable values to the stack. 
		VARIABLE, // 1: id
		VARIABLE_STREAM, // first n: id, followed by -1, followed by triplets of token data (line, col, fileID)
	}
}
