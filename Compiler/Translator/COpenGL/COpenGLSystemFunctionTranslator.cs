using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crayon.ParseTree;

namespace Crayon.Translator.COpenGL
{
	class COpenGLSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		protected override void TranslateAppDataRoot(List<string> output)
		{
			output.Add("TODO_app_data_root()");
		}

		protected override void TranslateAsyncMessageQueuePump(List<string> output)
		{
			output.Add("TODO_async_message_queue_pump()");
		}

		protected override void TranslateArcCos(List<string> output, Expression value)
		{
			output.Add("TODO_arccos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcSin(List<string> output, Expression value)
		{
			output.Add("TODO_arcsin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcTan(List<string> output, Expression dy, Expression dx)
		{
			output.Add("TODO_arctan(");
			this.Translator.TranslateExpression(output, dy);
			output.Add(", ");
			this.Translator.TranslateExpression(output, dx);
			output.Add(")");
		}

		protected override void TranslateArrayGet(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateArrayLength(List<string> output, Expression list)
		{
			// need to slowly convert these into int + pointer instead of just pointers where possible.
			// Otherwise create a $_new_array_with_length that allocates an extra integer before the pointer.
			output.Add("TODO_array_length(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateArraySet(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateAssert(List<string> output, Expression message)
		{
			output.Add("TODO_assert(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateBeginFrame(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImageAlpha(List<string> output, Expression image, Expression x, Expression y, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression targetWidth, Expression targetHeight, Expression sourceX, Expression sourceY, Expression sourceWidth, Expression sourceHeight)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateBlitImageRotated(List<string> output, Expression image, Expression centerX, Expression centerY, Expression angle)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
		{
			output.Add("((");
			output.Add(((COpenGLPlatform)this.Platform).GetTypeStringFromAnnotation(typeValue.FirstToken, typeValue.Value, false, true));
			output.Add(") ");
			this.Translator.TranslateExpression(output, expression);
			output.Add(")");
		}

		protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
		{
			output.Add("TODO_cast_to_list(");
			this.Translator.TranslateExpression(output, enumerableThing);
			output.Add(")");
		}

		protected override void TranslateCharToString(List<string> output, Expression charValue)
		{
			output.Add("TODO_char_to_string(");
			this.Translator.TranslateExpression(output, charValue);
			output.Add(")");

		}

		protected override void TranslateChr(List<string> output, Expression asciiValue)
		{
			output.Add("TODO_int_to_string(");
			this.Translator.TranslateExpression(output, asciiValue);
			output.Add(")");
		}

		protected override void TranslateComment(List<string> output, StringConstant commentValue)
		{
			output.Add("// ");
			output.Add(commentValue.Value);
		}

		protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
		{
			output.Add("TODO_convert_list_to_array(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateCos(List<string> output, Expression value)
		{
			output.Add("TODO_cos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateCurrentTimeSeconds(List<string> output)
		{
			output.Add("TODO_current_time_seconds_float()");
		}

		protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
		{
			output.Add("TODO_translate_dictionary_contains(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(", ");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
		{
			output.Add("TODO_dictionary_get_guaranteed(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(", ");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary)
		{
			output.Add("TODO_dictionary_get_keys(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
		{
			output.Add("TODO_dictionary_get_value(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
		{
			output.Add("TODO_dictionary_remove(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(", ");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionarySet(List<string> output, Expression dictionary, Expression key, Expression value)
		{
			output.Add("TODO_dictionary_set(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(", ");
			this.Translator.TranslateExpression(output, key);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
		{
			output.Add("TODO_dictionary_size(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
		{
			output.Add("TODO_dot_equals(");
			this.Translator.TranslateExpression(output, root);
			output.Add(", ");
			this.Translator.TranslateExpression(output, compareTo);
			output.Add(")");
		}

		protected override void TranslateDownloadImage(List<string> output, Expression key, Expression path)
		{
			output.Add("TODO_download_image(");
			this.Translator.TranslateExpression(output, key);
			output.Add(", ");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateDrawEllipse(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawLine(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression lineWidth, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawRectangle(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateDrawTriangle(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression cx, Expression cy, Expression red, Expression green, Expression blue, Expression alpha)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
		{
			output.Add("TODO_exponent(");
			this.Translator.TranslateExpression(output, baseNum);
			output.Add(", ");
			this.Translator.TranslateExpression(output, powerNum);
			output.Add(")");
		}

		protected override void TranslateFillScreen(List<string> output, Expression red, Expression green, Expression blue)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateForceParens(List<string> output, Expression expression)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, expression);
			output.Add(")");
		}

		protected override void TranslateGamepadEnableDevice(List<string> output, Expression device)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadGetAxisValue(List<string> output, Expression device, Expression axisIndex)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadGetAxisCount(List<string> output, Expression device)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadGetButtonCount(List<string> output, Expression device)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadGetDeviceCount(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadGetDeviceName(List<string> output, Expression device)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadGetHatCount(List<string> output, Expression device)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadGetRawDevice(List<string> output, Expression index)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGamepadIsButtonPressed(List<string> output, Expression device, Expression buttonIndex)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateGetEventsRawList(List<string> output)
		{
			output.Add("TODO_get_events_raw_list()");
		}

		protected override void TranslateGetProgramData(List<string> output)
		{
			output.Add("TODO_get_program_data()");
		}

		protected override void TranslateGetRawByteCodeString(List<string> output, string theString)
		{
			output.Add("\"");
			output.Add(theString);
			output.Add("\"");
		}

		protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
		{
			output.Add("TODO_http_request(");
			this.Translator.TranslateExpression(output, httpRequest);
			output.Add(", ");
			this.Translator.TranslateExpression(output, method);
			output.Add(", ");
			this.Translator.TranslateExpression(output, url);
			output.Add(", ");
			this.Translator.TranslateExpression(output, body);
			output.Add(", ");
			this.Translator.TranslateExpression(output, userAgent);
			output.Add(", ");
			this.Translator.TranslateExpression(output, contentType);
			output.Add(", ");
			this.Translator.TranslateExpression(output, contentLength);
			output.Add(", ");
			this.Translator.TranslateExpression(output, headerNameList);
			output.Add(", ");
			this.Translator.TranslateExpression(output, headerValueList);
			output.Add(")");
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
			output.Add("TODO_image_flush_imagette_to_native_bitmap(");
			this.Translator.TranslateExpression(output, imagette);
			output.Add(")");
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
			string op = increment ? "++" : "--";
			if (prefix) output.Add(op);
			this.Translator.TranslateExpression(output, expression);
			if (!prefix) output.Add(op);
		}

		protected override void TranslateInitializeGameWithFps(List<string> output, Expression fps)
		{
			output.Add("TODO_initialize_game_with_fps(");
			this.Translator.TranslateExpression(output, fps);
			output.Add(")");
		}

		protected override void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight)
		{
			output.Add("TODO_initialize_screen(");
			this.Translator.TranslateExpression(output, gameWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, gameHeight);
			output.Add(", ");
			this.Translator.TranslateExpression(output, screenWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, screenHeight);
			output.Add(")");
		}

		protected override void TranslateInt(List<string> output, Expression value)
		{
			output.Add("TODO_int(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
		{
			output.Add("TODO_io_create_directory(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateIoCurrentDirectory(List<string> output)
		{
			output.Add("TODO_io_current_directory()");
		}

		protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
		{
			output.Add("TODO_io_delete_file(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isUserData);
			output.Add(")");
		}

		protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
		{
			output.Add("TODO_io_does_path_exist(");
			this.Translator.TranslateExpression(output, canonicalizedPath);
			output.Add(", ");
			this.Translator.TranslateExpression(output, directoriesOnly);
			output.Add(", ");
			this.Translator.TranslateExpression(output, performCaseCheck);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isUserData);
			output.Add(")");
		}

		protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
		{
			output.Add("TODO_io_file_read_text(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isUserData);
			output.Add(")");
		}

		protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
		{
			output.Add("TODO_io_files_in_directory(");
			this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isUserData);
			output.Add(")");
		}

		protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
		{
			output.Add("TODO_io_file_write_text(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, content);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isUserData);
			output.Add(")");
		}

		protected override void TranslateIsValidInteger(List<string> output, Expression number)
		{
			output.Add("TODO_is_valid_integer(");
			this.Translator.TranslateExpression(output, number);
			output.Add(")");
		}

		protected override void TranslateIsWindowsProgram(List<string> output)
		{
			output.Add("1");
		}

		protected override void TranslateLaunchBrowser(List<string> output, Expression url)
		{
			output.Add("TODO_launch_browser(");
			this.Translator.TranslateExpression(output, url);
			output.Add(")");
		}

		protected override void TranslateListClear(List<string> output, Expression list)
		{
			output.Add("TODO_list_clear(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateListGet(List<string> output, Expression list, Expression index)
		{
			output.Add("TODO_list_get(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
		{
			output.Add("TODO_list_insert(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
		{
			output.Add("TODO_list_join(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateListJoinChars(List<string> output, Expression list)
		{
			output.Add("TODO_list_join_chars(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListLastIndex(List<string> output, Expression list)
		{
			output.Add("TODO_list_last_index(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListLength(List<string> output, Expression list)
		{
			output.Add("TODO_list_length(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListPop(List<string> output, Expression list)
		{
			output.Add("TODO_list_pop(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListPush(List<string> output, Expression list, Expression value)
		{
			output.Add("TODO_list_push(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
		{
			output.Add("TODO_list_remove_at(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");

		}

		protected override void TranslateListReverseInPlace(List<string> output, Expression list)
		{
			output.Add("TODO_list_reverse_in_place(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
		{
			output.Add("TODO_list_set(");
			this.Translator.TranslateExpression(output, list);
			output.Add(", ");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");

		}

		protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
		{
			output.Add("TODO_shuffle_list_in_place(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
		{
			output.Add("TODO_new_array(");
			this.Translator.TranslateExpression(output, size);
			output.Add(")");
		}

		protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
		{
			switch (keyType.Value)
			{
				case "int": output.Add("TODO_new_int_dictionary()"); break;
				case "string": output.Add("TODO_new_string_dictionary()"); break;
				default: throw new NotImplementedException();
			}
		}

		protected override void TranslateNewList(List<string> output, StringConstant type)
		{
			output.Add("TODO_new_list()");
		}

		protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateOrd(List<string> output, Expression character)
		{
			output.Add("TODO_ord(");
			this.Translator.TranslateExpression(output, character);
			output.Add(")");
		}

		protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
		{
			output.Add("TODO_parse_float(");
			this.Translator.TranslateExpression(output, outParam);
			output.Add(", ");
			this.Translator.TranslateExpression(output, rawString);
			output.Add(")");
		}

		protected override void TranslateParseInt(List<string> output, Expression rawString)
		{
			output.Add("TODO_parse_int(");
			this.Translator.TranslateExpression(output, rawString);
			output.Add(")");
		}

		protected override void TranslateParseJson(List<string> output, Expression rawString)
		{
			output.Add("TODO_parse_json(");
			this.Translator.TranslateExpression(output, rawString);
			output.Add(")");
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			output.Add("TODO_pause_for_frame()");
		}

		protected override void TranslatePrint(List<string> output, Expression message)
		{
			output.Add("printf(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateRandomFloat(List<string> output)
		{
			output.Add("TODO_random_float()");
		}

		protected override void TranslateReadLocalImageResource(List<string> output, Expression filePath)
		{
			output.Add("TODO_read_local_image_resource(");
			this.Translator.TranslateExpression(output, filePath);
			output.Add(")");
		}

		protected override void TranslateReadLocalSoundResource(List<string> output, Expression filePath)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateReadLocalTileResource(List<string> output, Expression tileGenName)
		{
			output.Add("TODO_read_local_tile_resource(");
			this.Translator.TranslateExpression(output, tileGenName);
			output.Add(")");
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateResourceReadText(List<string> output, Expression path)
		{
			output.Add("TODO_resource_read_text(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateSetProgramData(List<string> output, Expression programData)
		{
			output.Add("TODO_set_program_data(");
			this.Translator.TranslateExpression(output, programData);
			output.Add(")");
		}

		protected override void TranslateSetTitle(List<string> output, Expression title)
		{
			output.Add("TODO_set_title(");
			this.Translator.TranslateExpression(output, title);
			output.Add(")");
		}

		protected override void TranslateSfxPlay(List<string> output, Expression soundInstance)
		{
			output.Add("TODO_sfx_play(");
			this.Translator.TranslateExpression(output, soundInstance);
			output.Add(")");
		}

		protected override void TranslateSin(List<string> output, Expression value)
		{
			output.Add("TODO_sin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateSortPrimitiveValues(List<string> output, Expression valueList, Expression isString)
		{
			output.Add("TODO_sort_primitive_list(");
			this.Translator.TranslateExpression(output, valueList);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isString);
			output.Add(")");
		}

		protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
		{
			output.Add("TODO_sorted_copy_of_int_array(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
		{
			output.Add("TODO_string_as_char(");
			this.Translator.TranslateExpression(output, stringConstant);
			output.Add(")");
		}

		protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
		{
			output.Add("TODO_string_cast_");
			output.Add(strongCast ? "strong" : "weak");
			output.Add("(");
			this.Translator.TranslateExpression(output, thing);
			output.Add(")");
		}

		protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
		{
			output.Add("TODO_string_char_at(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(", ");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateStringCompare(List<string> output, Expression a, Expression b)
		{
			output.Add("TODO_string_compare(");
			this.Translator.TranslateExpression(output, a);
			output.Add(", ");
			this.Translator.TranslateExpression(output, b);
			output.Add(")");
		}

		protected override void TranslateStringContains(List<string> output, Expression haystack, Expression needle)
		{
			output.Add("TODO_string_contains(");
			this.Translator.TranslateExpression(output, haystack);
			output.Add(", ");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			output.Add("TODO_string_endswith(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(", ");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
		{
			output.Add("TODO_string_equals(");
			this.Translator.TranslateExpression(output, aNonNull);
			output.Add(", ");
			this.Translator.TranslateExpression(output, b);
			output.Add(")");
		}

		protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
		{
			output.Add("TODO_string_from_code(");
			this.Translator.TranslateExpression(output, characterCode);
			output.Add(")");
		}

		protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle)
		{
			output.Add("TODO_string_index_of(");
			this.Translator.TranslateExpression(output, haystack);
			output.Add(", ");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringLength(List<string> output, Expression stringValue)
		{
			output.Add("TODO_string_length(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringLower(List<string> output, Expression stringValue)
		{
			output.Add("TODO_string_lower(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
		{
			output.Add("TODO_parse_float(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseInt(List<string> output, Expression value)
		{
			output.Add("TODO_parse_int(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
		{
			output.Add("TODO_string_replace(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(", ");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(", ");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
		}

		protected override void TranslateStringReverse(List<string> output, Expression stringValue)
		{
			output.Add("TODO_string_reverse(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
		{
			output.Add("TODO_string_split(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			output.Add("TODO_string_starts_with(");
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(", ");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringTrim(List<string> output, Expression stringValue)
		{
			output.Add("TODO_string_trim(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringUpper(List<string> output, Expression stringValue)
		{
			output.Add("TODO_string_upper(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateTan(List<string> output, Expression value)
		{
			output.Add("TODO_tan(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			throw new NotImplementedException();
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
		{
			output.Add("TODO_unsafe_float_division(");
			this.Translator.TranslateExpression(output, numerator);
			output.Add(", ");
			this.Translator.TranslateExpression(output, denominator);
			output.Add(")");
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
		{
			output.Add("TODO_unsafe_integer_division(");
			this.Translator.TranslateExpression(output, numerator);
			output.Add(", ");
			this.Translator.TranslateExpression(output, denominator);
			output.Add(")");
		}

		protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
		{
			output.Add("TODO_io_delete_directory(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isRecursive);
			output.Add(")");
		}

		protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename, Expression intOutStatus)
		{
			output.Add("TODO_music_load_from_resource(");
			this.Translator.TranslateExpression(output, filename);
			output.Add(", ");
			this.Translator.TranslateExpression(output, intOutStatus);
			output.Add(")");
		}

		protected override void TranslateMusicPause(List<string> output)
		{
			output.Add("TODO_music_pause()");
		}

		protected override void TranslateMusicPlayNow(List<string> output, Expression musicNativeObject, Expression musicRealPath, Expression isLooping)
		{
			output.Add("TODO_music_play_now(");
			this.Translator.TranslateExpression(output, musicNativeObject);
			output.Add(", ");
			this.Translator.TranslateExpression(output, musicRealPath);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isLooping);
			output.Add(")");
		}

		protected override void TranslateMusicResume(List<string> output)
		{
			output.Add("TODO_music_resume()");
		}

		protected override void TranslateMusicSetVolume(List<string> output, Expression musicNativeObject, Expression ratio)
		{
			output.Add("TODO_musci_set_volume(");
			this.Translator.TranslateExpression(output, musicNativeObject);
			output.Add(", ");
			this.Translator.TranslateExpression(output, ratio);
			output.Add(")");
		}
	}
}
