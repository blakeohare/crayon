using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon.Translator.COpenGL
{
	class COpenGLSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		protected override void TranslateArcCos(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcSin(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArcTan(List<string> output, ParseTree.Expression dy, ParseTree.Expression dx)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArrayGet(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArrayLength(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateArraySet(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateAssert(List<string> output, ParseTree.Expression message)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBeginFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImage(List<string> output, ParseTree.Expression image, ParseTree.Expression x, ParseTree.Expression y)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImagePartial(List<string> output, ParseTree.Expression image, ParseTree.Expression targetX, ParseTree.Expression targetY, ParseTree.Expression targetWidth, ParseTree.Expression targetHeight, ParseTree.Expression sourceX, ParseTree.Expression sourceY, ParseTree.Expression sourceWidth, ParseTree.Expression sourceHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCast(List<string> output, ParseTree.StringConstant typeValue, ParseTree.Expression expression)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCastToList(List<string> output, ParseTree.StringConstant typeValue, ParseTree.Expression enumerableThing)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCharToString(List<string> output, ParseTree.Expression charValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateComment(List<string> output, ParseTree.StringConstant commentValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateConvertListToArray(List<string> output, ParseTree.StringConstant type, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCos(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCurrentTimeSeconds(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryContains(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetGuaranteed(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, ParseTree.Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryGetValues(List<string> output, ParseTree.Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionaryRemove(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySet(List<string> output, ParseTree.Expression dictionary, ParseTree.Expression key, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDictionarySize(List<string> output, ParseTree.Expression dictionary)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDotEquals(List<string> output, ParseTree.Expression root, ParseTree.Expression compareTo)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDownloadImage(List<string> output, ParseTree.Expression key, ParseTree.Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawEllipse(List<string> output, ParseTree.Expression left, ParseTree.Expression top, ParseTree.Expression width, ParseTree.Expression height, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue, ParseTree.Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawLine(List<string> output, ParseTree.Expression ax, ParseTree.Expression ay, ParseTree.Expression bx, ParseTree.Expression by, ParseTree.Expression lineWidth, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue, ParseTree.Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawRectangle(List<string> output, ParseTree.Expression left, ParseTree.Expression top, ParseTree.Expression width, ParseTree.Expression height, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue, ParseTree.Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateExponent(List<string> output, ParseTree.Expression baseNum, ParseTree.Expression powerNum)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateFillScreen(List<string> output, ParseTree.Expression red, ParseTree.Expression green, ParseTree.Expression blue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateForceParens(List<string> output, ParseTree.Expression expression)
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

		protected override void TranslateGetRawByteCodeString(List<string> output, string theString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGlLoadTexture(List<string> output, ParseTree.Expression platformBitmapResource)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGlMaxTextureSize(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageAsyncDownloadCompletedPayload(List<string> output, ParseTree.Expression asyncReferenceKey)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageCreateFlippedCopyOfNativeBitmap(List<string> output, ParseTree.Expression image, ParseTree.Expression flipX, ParseTree.Expression flipY)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageImagetteFlushToNativeBitmap(List<string> output, ParseTree.Expression imagette)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageInitiateAsyncDownloadOfResource(List<string> output, ParseTree.Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageNativeBitmapHeight(List<string> output, ParseTree.Expression bitmap)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateImageNativeBitmapWidth(List<string> output, ParseTree.Expression bitmap)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInitializeGameWithFps(List<string> output, ParseTree.Expression fps)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInitializeScreen(List<string> output, ParseTree.Expression gameWidth, ParseTree.Expression gameHeight, ParseTree.Expression screenWidth, ParseTree.Expression screenHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateInt(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoCurrentDirectory(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoDoesPathExist(List<string> output, ParseTree.Expression canonicalizedPath, ParseTree.Expression directoriesOnly, ParseTree.Expression performCaseCheck)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFileReadText(List<string> output, ParseTree.Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFilesInDirectory(List<string> output, ParseTree.Expression verifiedCanonicalizedPath)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIoFileWriteText(List<string> output, ParseTree.Expression path, ParseTree.Expression content)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIsValidInteger(List<string> output, ParseTree.Expression number)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateIsWindowsProgram(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListClear(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListConcat(List<string> output, ParseTree.Expression listA, ParseTree.Expression listB)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListGet(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListInsert(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListJoin(List<string> output, ParseTree.Expression list, ParseTree.Expression sep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListJoinChars(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListLastIndex(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListLength(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListPop(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListPush(List<string> output, ParseTree.Expression list, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListRemoveAt(List<string> output, ParseTree.Expression list, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListReverseInPlace(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListSet(List<string> output, ParseTree.Expression list, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListShuffleInPlace(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateMultiplyList(List<string> output, ParseTree.Expression list, ParseTree.Expression num)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewArray(List<string> output, ParseTree.StringConstant type, ParseTree.Expression size)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewDictionary(List<string> output, ParseTree.StringConstant keyType, ParseTree.StringConstant valueType)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewList(List<string> output, ParseTree.StringConstant type)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewListOfSize(List<string> output, ParseTree.StringConstant type, ParseTree.Expression length)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewStack(List<string> output, ParseTree.StringConstant type)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateParseInt(List<string> output, ParseTree.Expression rawString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateParseJson(List<string> output, ParseTree.Expression rawString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslatePrint(List<string> output, ParseTree.Expression message)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateRandomFloat(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalImageResource(List<string> output, ParseTree.Expression filePath)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalSoundResource(List<string> output, ParseTree.Expression filePath)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalTileResource(List<string> output, ParseTree.Expression tileGenName)
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

		protected override void TranslateResourceReadText(List<string> output, ParseTree.Expression path)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSetProgramData(List<string> output, ParseTree.Expression programData)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSetTitle(List<string> output, ParseTree.Expression title)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSin(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSortPrimitiveValues(List<string> output, ParseTree.Expression valueList, ParseTree.Expression isString)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSortedCopyOfIntArray(List<string> output, ParseTree.Expression list)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateSoundPlay(List<string> output, ParseTree.Expression soundInstance)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackGet(List<string> output, ParseTree.Expression stack, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackLength(List<string> output, ParseTree.Expression stack)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackPop(List<string> output, ParseTree.Expression stack)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackPush(List<string> output, ParseTree.Expression stack, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStackSet(List<string> output, ParseTree.Expression stack, ParseTree.Expression index, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringAsChar(List<string> output, ParseTree.StringConstant stringConstant)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCast(List<string> output, ParseTree.Expression thing, bool strongCast)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCharAt(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringCompare(List<string> output, ParseTree.Expression a, ParseTree.Expression b)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringContains(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringEndsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringEquals(List<string> output, ParseTree.Expression aNonNull, ParseTree.Expression b)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringFromCode(List<string> output, ParseTree.Expression characterCode)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringIndexOf(List<string> output, ParseTree.Expression haystack, ParseTree.Expression needle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLength(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringLower(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseFloat(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringParseInt(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReplace(List<string> output, ParseTree.Expression stringValue, ParseTree.Expression findMe, ParseTree.Expression replaceWith)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringReverse(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringSplit(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression sep)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringStartsWith(List<string> output, ParseTree.Expression stringExpr, ParseTree.Expression findMe)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringTrim(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateStringUpper(List<string> output, ParseTree.Expression stringValue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateTan(List<string> output, ParseTree.Expression value)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, ParseTree.Expression numerator, ParseTree.Expression denominator)
		{
			throw new NotImplementedException();
		}
	}
}
