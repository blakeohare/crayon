The context in which all these methods are included have the following local variables set:
argCount -> number of args set
funcArgs -> an array of those args

The primitive value is set in the following:
List<Value> -> list1
string -> string1
dictImpl -> dictionary
value -> the actual value struct, if needed

The following values can be set:
Value output -> this is set to null before code is included. If it is still null after the imported code runs, 
	it will count as a not found function. Use this for incorrect arg lengths.


bool primitiveMethodToCoreLibraryFallback -> This is false. If you want the primitive method to instead invoke a function,
	then set this to true and provide the function ID in the functionId variable.
	One example where this is used is list.sort(userfunction) where the userfunction needs to be invoked in the interpreter, not 
	synchronously in one command.
int functionId -> see above