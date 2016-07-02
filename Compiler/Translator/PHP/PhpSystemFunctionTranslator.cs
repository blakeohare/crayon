using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Php
{
	internal class PhpSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		protected override void TranslateAppDataRoot(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcCos(List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcSin(List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcTan(List<string> output, Expression dy, Expression dx)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArrayGet(List<string> output, Expression list, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArrayLength(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArraySet(List<string> output, Expression list, Expression index, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateAssert(List<string> output, Expression message)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateAsyncMessageQueuePump(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBeginFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCharToString(List<string> output, Expression charValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateChr(List<string> output, Expression asciiValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateComment(List<string> output, StringConstant commentValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCos(List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCurrentTimeSeconds(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDownloadImage(List<string> output, Expression key, Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateForceParens(List<string> output, Expression expression)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetEventsRawList(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetProgramData(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetRawByteCodeString(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGlMaxTextureSize(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageAsyncDownloadCompletedPayload(List<string> output, Expression asyncReferenceKey)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageCreateFlippedCopyOfNativeBitmap(List<string> output, Expression image, Expression flipX, Expression flipY)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageImagetteFlushToNativeBitmap(List<string> output, Expression imagette)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageInitiateAsyncDownloadOfResource(List<string> output, Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageNativeBitmapHeight(List<string> output, Expression bitmap)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageNativeBitmapWidth(List<string> output, Expression bitmap)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIncrement(List<string> output, Expression expression, bool increment, bool prefix)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInitializeGameWithFps(List<string> output, Expression fps)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInt(List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoCurrentDirectory(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIsValidInteger(List<string> output, Expression number)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIsWindowsProgram(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListClear(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListGet(List<string> output, Expression list, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListJoinChars(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListLastIndex(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListLength(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListPop(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListPush(List<string> output, Expression list, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListReverseInPlace(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewList(List<string> output, StringConstant type)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateOrd(List<string> output, Expression character)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateParseInt(List<string> output, Expression rawString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateParseJson(List<string> output, Expression rawString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateRandomFloat(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalImageResource(List<string> output, Expression filePath)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateResourceGetManifest(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateResourceReadText(List<string> output, Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSetProgramData(List<string> output, Expression programData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSetTitle(List<string> output, Expression title)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSin(List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSortedCopyOfStringArray(List<string> output, Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCompare(List<string> output, Expression a, Expression b)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringContains(List<string> output, Expression haystack, Expression needle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLength(List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLower(List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseInt(List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReverse(List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringTrim(List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringUpper(List<string> output, Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateTan(List<string> output, Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
		{
			throw new NotImplementedException();
		}
	}
}

