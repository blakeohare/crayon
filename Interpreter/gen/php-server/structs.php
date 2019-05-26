<?php
class BreakpointInfo {
	var $breakpointId;
	var $isTransient;
	var $token;
	function __construct($a0, $a1, $a2) {
		$this->breakpointId = $a0;
		$this->isTransient = $a1;
		$this->token = $a2;
	}
}
class ClassInfo {
	var $id;
	var $nameId;
	var $baseClassId;
	var $assemblyId;
	var $staticInitializationState;
	var $staticFields;
	var $staticConstructorFunctionId;
	var $constructorFunctionId;
	var $memberCount;
	var $functionIds;
	var $fieldInitializationCommand;
	var $fieldInitializationLiteral;
	var $fieldAccessModifiers;
	var $globalIdToMemberId;
	var $localeScopedNameIdToMemberId;
	var $typeInfo;
	var $fullyQualifiedName;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7, $a8, $a9, $a10, $a11, $a12, $a13, $a14, $a15, $a16) {
		$this->id = $a0;
		$this->nameId = $a1;
		$this->baseClassId = $a2;
		$this->assemblyId = $a3;
		$this->staticInitializationState = $a4;
		$this->staticFields = $a5;
		$this->staticConstructorFunctionId = $a6;
		$this->constructorFunctionId = $a7;
		$this->memberCount = $a8;
		$this->functionIds = $a9;
		$this->fieldInitializationCommand = $a10;
		$this->fieldInitializationLiteral = $a11;
		$this->fieldAccessModifiers = $a12;
		$this->globalIdToMemberId = $a13;
		$this->localeScopedNameIdToMemberId = $a14;
		$this->typeInfo = $a15;
		$this->fullyQualifiedName = $a16;
	}
}
class ClassValue {
	var $isInterface;
	var $classId;
	function __construct($a0, $a1) {
		$this->isInterface = $a0;
		$this->classId = $a1;
	}
}
class ClosureValuePointer {
	var $value;
	function __construct($a0) {
		$this->value = $a0;
	}
}
class Code {
	var $ops;
	var $args;
	var $stringArgs;
	var $integerSwitchesByPc;
	var $stringSwitchesByPc;
	var $debugData;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5) {
		$this->ops = $a0;
		$this->args = $a1;
		$this->stringArgs = $a2;
		$this->integerSwitchesByPc = $a3;
		$this->stringSwitchesByPc = $a4;
		$this->debugData = $a5;
	}
}
class DebugStepTracker {
	var $uniqueId;
	var $originatingFileId;
	var $originatingLineIndex;
	function __construct($a0, $a1, $a2) {
		$this->uniqueId = $a0;
		$this->originatingFileId = $a1;
		$this->originatingLineIndex = $a2;
	}
}
class DictImpl {
	var $size;
	var $keyType;
	var $keyClassId;
	var $valueType;
	var $intToIndex;
	var $stringToIndex;
	var $keys;
	var $values;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7) {
		$this->size = $a0;
		$this->keyType = $a1;
		$this->keyClassId = $a2;
		$this->valueType = $a3;
		$this->intToIndex = $a4;
		$this->stringToIndex = $a5;
		$this->keys = $a6;
		$this->values = $a7;
	}
}
class ExecutionContext {
	var $id;
	var $stackTop;
	var $currentValueStackSize;
	var $valueStackCapacity;
	var $valueStack;
	var $localsStack;
	var $localsStackSet;
	var $localsStackSetToken;
	var $executionCounter;
	var $activeExceptionHandled;
	var $activeException;
	var $executionStateChange;
	var $executionStateChangeCommand;
	var $activeInterrupt;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7, $a8, $a9, $a10, $a11, $a12, $a13) {
		$this->id = $a0;
		$this->stackTop = $a1;
		$this->currentValueStackSize = $a2;
		$this->valueStackCapacity = $a3;
		$this->valueStack = $a4;
		$this->localsStack = $a5;
		$this->localsStackSet = $a6;
		$this->localsStackSetToken = $a7;
		$this->executionCounter = $a8;
		$this->activeExceptionHandled = $a9;
		$this->activeException = $a10;
		$this->executionStateChange = $a11;
		$this->executionStateChangeCommand = $a12;
		$this->activeInterrupt = $a13;
	}
}
class FunctionInfo {
	var $id;
	var $nameId;
	var $pc;
	var $minArgs;
	var $maxArgs;
	var $type;
	var $associatedClassId;
	var $localsSize;
	var $pcOffsetsForOptionalArgs;
	var $name;
	var $closureIds;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7, $a8, $a9, $a10) {
		$this->id = $a0;
		$this->nameId = $a1;
		$this->pc = $a2;
		$this->minArgs = $a3;
		$this->maxArgs = $a4;
		$this->type = $a5;
		$this->associatedClassId = $a6;
		$this->localsSize = $a7;
		$this->pcOffsetsForOptionalArgs = $a8;
		$this->name = $a9;
		$this->closureIds = $a10;
	}
}
class FunctionPointer {
	var $type;
	var $context;
	var $classId;
	var $functionId;
	var $closureVariables;
	function __construct($a0, $a1, $a2, $a3, $a4) {
		$this->type = $a0;
		$this->context = $a1;
		$this->classId = $a2;
		$this->functionId = $a3;
		$this->closureVariables = $a4;
	}
}
class HttpRequest {
	var $statusCode;
	var $status;
	var $headers;
	var $body;
	function __construct($a0, $a1, $a2, $a3) {
		$this->statusCode = $a0;
		$this->status = $a1;
		$this->headers = $a2;
		$this->body = $a3;
	}
}
class InterpreterResult {
	var $status;
	var $errorMessage;
	var $reinvokeDelay;
	var $executionContextId;
	var $isRootContext;
	var $loadAssemblyInformation;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5) {
		$this->status = $a0;
		$this->errorMessage = $a1;
		$this->reinvokeDelay = $a2;
		$this->executionContextId = $a3;
		$this->isRootContext = $a4;
		$this->loadAssemblyInformation = $a5;
	}
}
class Interrupt {
	var $type;
	var $exceptionType;
	var $exceptionMessage;
	var $sleepDurationSeconds;
	var $debugStepData;
	function __construct($a0, $a1, $a2, $a3, $a4) {
		$this->type = $a0;
		$this->exceptionType = $a1;
		$this->exceptionMessage = $a2;
		$this->sleepDurationSeconds = $a3;
		$this->debugStepData = $a4;
	}
}
class LibRegObj {
	var $functionPointers;
	var $functionNames;
	var $argCounts;
	function __construct($a0, $a1, $a2) {
		$this->functionPointers = $a0;
		$this->functionNames = $a1;
		$this->argCounts = $a2;
	}
}
class ListImpl {
	var $type;
	var $size;
	var $list;
	function __construct($a0, $a1, $a2) {
		$this->type = $a0;
		$this->size = $a1;
		$this->list = $a2;
	}
}
class MagicNumbers {
	var $coreExceptionClassId;
	var $coreGenerateExceptionFunctionId;
	var $totalLocaleCount;
	function __construct($a0, $a1, $a2) {
		$this->coreExceptionClassId = $a0;
		$this->coreGenerateExceptionFunctionId = $a1;
		$this->totalLocaleCount = $a2;
	}
}
class NamedCallbackStore {
	var $callbacksById;
	var $callbackIdLookup;
	function __construct($a0, $a1) {
		$this->callbacksById = $a0;
		$this->callbackIdLookup = $a1;
	}
}
class ObjectInstance {
	var $classId;
	var $objectId;
	var $members;
	var $nativeData;
	var $nativeObject;
	function __construct($a0, $a1, $a2, $a3, $a4) {
		$this->classId = $a0;
		$this->objectId = $a1;
		$this->members = $a2;
		$this->nativeData = $a3;
		$this->nativeObject = $a4;
	}
}
class PlatformRelayObject {
	var $type;
	var $iarg1;
	var $iarg2;
	var $iarg3;
	var $farg1;
	var $sarg1;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5) {
		$this->type = $a0;
		$this->iarg1 = $a1;
		$this->iarg2 = $a2;
		$this->iarg3 = $a3;
		$this->farg1 = $a4;
		$this->sarg1 = $a5;
	}
}
class ResourceDB {
	var $filesPerDirectory;
	var $fileInfo;
	var $dataList;
	function __construct($a0, $a1, $a2) {
		$this->filesPerDirectory = $a0;
		$this->fileInfo = $a1;
		$this->dataList = $a2;
	}
}
class ResourceInfo {
	var $userPath;
	var $internalPath;
	var $isText;
	var $type;
	var $manifestParam;
	function __construct($a0, $a1, $a2, $a3, $a4) {
		$this->userPath = $a0;
		$this->internalPath = $a1;
		$this->isText = $a2;
		$this->type = $a3;
		$this->manifestParam = $a4;
	}
}
class StackFrame {
	var $pc;
	var $localsStackSetToken;
	var $localsStackOffset;
	var $localsStackOffsetEnd;
	var $previous;
	var $returnValueUsed;
	var $objectContext;
	var $valueStackPopSize;
	var $markClassAsInitialized;
	var $depth;
	var $postFinallyBehavior;
	var $returnValueTempStorage;
	var $closureVariables;
	var $debugStepTracker;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7, $a8, $a9, $a10, $a11, $a12, $a13) {
		$this->pc = $a0;
		$this->localsStackSetToken = $a1;
		$this->localsStackOffset = $a2;
		$this->localsStackOffsetEnd = $a3;
		$this->previous = $a4;
		$this->returnValueUsed = $a5;
		$this->objectContext = $a6;
		$this->valueStackPopSize = $a7;
		$this->markClassAsInitialized = $a8;
		$this->depth = $a9;
		$this->postFinallyBehavior = $a10;
		$this->returnValueTempStorage = $a11;
		$this->closureVariables = $a12;
		$this->debugStepTracker = $a13;
	}
}
class SymbolData {
	var $tokenData;
	var $sourceCode;
	var $sourceCodeBuilder;
	var $fileNameById;
	var $fileIdByName;
	var $localVarNamesById;
	var $closureVarNamesById;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6) {
		$this->tokenData = $a0;
		$this->sourceCode = $a1;
		$this->sourceCodeBuilder = $a2;
		$this->fileNameById = $a3;
		$this->fileIdByName = $a4;
		$this->localVarNamesById = $a5;
		$this->closureVarNamesById = $a6;
	}
}
class SystemMethod {
	var $context;
	var $id;
	function __construct($a0, $a1) {
		$this->context = $a0;
		$this->id = $a1;
	}
}
class Token {
	var $lineIndex;
	var $colIndex;
	var $fileId;
	function __construct($a0, $a1, $a2) {
		$this->lineIndex = $a0;
		$this->colIndex = $a1;
		$this->fileId = $a2;
	}
}
class Value {
	var $type;
	var $internalValue;
	function __construct($a0, $a1) {
		$this->type = $a0;
		$this->internalValue = $a1;
	}
}
class VmContext {
	var $executionContexts;
	var $lastExecutionContextId;
	var $byteCode;
	var $symbolData;
	var $metadata;
	var $instanceCounter;
	var $initializationComplete;
	var $classStaticInitializationStack;
	var $funcArgs;
	var $resourceDatabase;
	var $shutdownHandlers;
	var $environment;
	var $namedCallbacks;
	var $globals;
	var $globalNull;
	var $globalTrue;
	var $globalFalse;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7, $a8, $a9, $a10, $a11, $a12, $a13, $a14, $a15, $a16) {
		$this->executionContexts = $a0;
		$this->lastExecutionContextId = $a1;
		$this->byteCode = $a2;
		$this->symbolData = $a3;
		$this->metadata = $a4;
		$this->instanceCounter = $a5;
		$this->initializationComplete = $a6;
		$this->classStaticInitializationStack = $a7;
		$this->funcArgs = $a8;
		$this->resourceDatabase = $a9;
		$this->shutdownHandlers = $a10;
		$this->environment = $a11;
		$this->namedCallbacks = $a12;
		$this->globals = $a13;
		$this->globalNull = $a14;
		$this->globalTrue = $a15;
		$this->globalFalse = $a16;
	}
}
class VmDebugData {
	var $hasBreakpoint;
	var $breakpointInfo;
	var $breakpointIdToPc;
	var $nextBreakpointId;
	var $nextStepId;
	function __construct($a0, $a1, $a2, $a3, $a4) {
		$this->hasBreakpoint = $a0;
		$this->breakpointInfo = $a1;
		$this->breakpointIdToPc = $a2;
		$this->nextBreakpointId = $a3;
		$this->nextStepId = $a4;
	}
}
class VmEnvironment {
	var $commandLineArgs;
	var $showLibStack;
	var $stdoutPrefix;
	var $stacktracePrefix;
	function __construct($a0, $a1, $a2, $a3) {
		$this->commandLineArgs = $a0;
		$this->showLibStack = $a1;
		$this->stdoutPrefix = $a2;
		$this->stacktracePrefix = $a3;
	}
}
class VmGlobals {
	var $valueNull;
	var $boolTrue;
	var $boolFalse;
	var $intZero;
	var $intOne;
	var $intNegativeOne;
	var $floatZero;
	var $floatOne;
	var $stringEmpty;
	var $positiveIntegers;
	var $negativeIntegers;
	var $commonStrings;
	var $booleanType;
	var $intType;
	var $stringType;
	var $floatType;
	var $classType;
	var $anyInstanceType;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7, $a8, $a9, $a10, $a11, $a12, $a13, $a14, $a15, $a16, $a17) {
		$this->valueNull = $a0;
		$this->boolTrue = $a1;
		$this->boolFalse = $a2;
		$this->intZero = $a3;
		$this->intOne = $a4;
		$this->intNegativeOne = $a5;
		$this->floatZero = $a6;
		$this->floatOne = $a7;
		$this->stringEmpty = $a8;
		$this->positiveIntegers = $a9;
		$this->negativeIntegers = $a10;
		$this->commonStrings = $a11;
		$this->booleanType = $a12;
		$this->intType = $a13;
		$this->stringType = $a14;
		$this->floatType = $a15;
		$this->classType = $a16;
		$this->anyInstanceType = $a17;
	}
}
class VmMetadata {
	var $identifiers;
	var $identifiersBuilder;
	var $invIdentifiers;
	var $literalTable;
	var $literalTableBuilder;
	var $integerSwitchLookups;
	var $integerSwitchLookupsBuilder;
	var $stringSwitchLookups;
	var $stringSwitchLookupsBuilder;
	var $classTable;
	var $functionTable;
	var $lambdaTable;
	var $globalNameIdToPrimitiveMethodName;
	var $cniFunctionsById;
	var $lengthId;
	var $primitiveMethodFunctionIdFallbackLookup;
	var $userCodeStart;
	var $projectId;
	var $esfData;
	var $magicNumbers;
	var $invFunctionNameLiterals;
	var $classMemberLocalizerBuilder;
	var $mostRecentFunctionDef;
	function __construct($a0, $a1, $a2, $a3, $a4, $a5, $a6, $a7, $a8, $a9, $a10, $a11, $a12, $a13, $a14, $a15, $a16, $a17, $a18, $a19, $a20, $a21, $a22) {
		$this->identifiers = $a0;
		$this->identifiersBuilder = $a1;
		$this->invIdentifiers = $a2;
		$this->literalTable = $a3;
		$this->literalTableBuilder = $a4;
		$this->integerSwitchLookups = $a5;
		$this->integerSwitchLookupsBuilder = $a6;
		$this->stringSwitchLookups = $a7;
		$this->stringSwitchLookupsBuilder = $a8;
		$this->classTable = $a9;
		$this->functionTable = $a10;
		$this->lambdaTable = $a11;
		$this->globalNameIdToPrimitiveMethodName = $a12;
		$this->cniFunctionsById = $a13;
		$this->lengthId = $a14;
		$this->primitiveMethodFunctionIdFallbackLookup = $a15;
		$this->userCodeStart = $a16;
		$this->projectId = $a17;
		$this->esfData = $a18;
		$this->magicNumbers = $a19;
		$this->invFunctionNameLiterals = $a20;
		$this->classMemberLocalizerBuilder = $a21;
		$this->mostRecentFunctionDef = $a22;
	}
}
?>