using System;
using System.Collections.Generic;
using Crayon.ParseTree;

namespace Crayon.Translator.Python
{
	internal class PythonSystemFunctionTranslator : AbstractSystemFunctionTranslator
	{
		public PythonSystemFunctionTranslator()
			: base()
		{ }

		protected override void TranslateAppDataRoot(List<string> output)
		{
			output.Add("get_app_data_root()");
		}

		protected override void TranslateAsyncMessageQueuePump(List<string> output)
		{
			output.Add("_pump_async_message_queue()");
		}

		protected override void TranslateArcCos(List<string> output, Expression value)
		{
			output.Add("math.acos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcSin(List<string> output, Expression value)
		{
			output.Add("math.asin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateArcTan(List<string> output, Expression dy, Expression dx)
		{
			output.Add("math.atan2(");
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
			output.Add("len(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateArraySet(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add(this.Shorten("] = "));
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateAssert(List<string> output, Expression message)
		{
			output.Add("create_assertion(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateBeginFrame(List<string> output)
		{
			throw new Exception("This code path should be optimized out of the python translation.");
		}

		protected override void TranslateBlitImage(List<string> output, Expression image, Expression x, Expression y)
		{
			output.Add("_global_vars['virtual_screen'].blit(");
			this.Translator.TranslateExpression(output, image);
			output.Add("[1], (");
			this.Translator.TranslateExpression(output, x);
			output.Add(", ");
			this.Translator.TranslateExpression(output, y);
			output.Add("))");
		}

		protected override void TranslateBlitImagePartial(List<string> output, Expression image, Expression targetX, Expression targetY, Expression targetWidth, Expression targetHeight, Expression sourceX, Expression sourceY, Expression sourceWidth, Expression sourceHeight)
		{
			output.Add("blit_partial(");
			this.Translator.TranslateExpression(output, image);
			output.Add("[1], ");
			this.Translator.TranslateExpression(output, targetX);
			output.Add(", ");
			this.Translator.TranslateExpression(output, targetY);
			output.Add(", ");
			this.Translator.TranslateExpression(output, targetWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, targetHeight);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceX);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceY);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, sourceHeight);
			output.Add(")");
		}

		protected override void TranslateCast(List<string> output, StringConstant typeValue, Expression expression)
		{
			this.Translator.TranslateExpression(output, expression);
		}

		protected override void TranslateCastToList(List<string> output, StringConstant typeValue, Expression enumerableThing)
		{
			this.Translator.TranslateExpression(output, enumerableThing);
		}

		protected override void TranslateCharToString(List<string> output, Expression charValue)
		{
			this.Translator.TranslateExpression(output, charValue);
		}

		protected override void TranslateChr(List<string> output, Expression asciiValue)
		{
			output.Add("chr(");
			this.Translator.TranslateExpression(output, asciiValue);
			output.Add(")");
		}

		protected override void TranslateComment(List<string> output, StringConstant commentValue)
		{
#if DEBUG
			output.Add("# " + commentValue.Value);
#endif
		}

		protected override void TranslateConvertListToArray(List<string> output, StringConstant type, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
		}

		protected override void TranslateCos(List<string> output, Expression value)
		{
			output.Add("math.cos(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateCurrentTimeSeconds(List<string> output)
		{
			output.Add("time.time()");
		}

		// Not safe for dictionaries that can contain a value of None.
		protected override void TranslateDictionaryContains(List<string> output, Expression dictionary, Expression key)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".get(");
			this.Translator.TranslateExpression(output, key);
			output.Add(", None) != None)");
		}

		protected override void TranslateDictionaryGetGuaranteed(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("]");
		}

		protected override void TranslateDictionaryGetKeys(List<string> output, string keyType, Expression dictionary)
		{
			output.Add("list(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".keys())");
		}

		protected override void TranslateDictionaryGetValues(List<string> output, Expression dictionary)
		{
			output.Add("list(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".values())");
		}

		protected override void TranslateDictionaryRemove(List<string> output, Expression dictionary, Expression key)
		{
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(".pop(");
			this.Translator.TranslateExpression(output, key);
			output.Add(")");
		}

		protected override void TranslateDictionarySet(List<string> output, Expression dict, Expression key, Expression value)
		{
			this.Translator.TranslateExpression(output, dict);
			output.Add("[");
			this.Translator.TranslateExpression(output, key);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateDictionarySize(List<string> output, Expression dictionary)
		{
			output.Add("len(");
			this.Translator.TranslateExpression(output, dictionary);
			output.Add(")");
		}

		protected override void TranslateDotEquals(List<string> output, Expression root, Expression compareTo)
		{
			throw new Exception("This should have been optimized out.");
		}

		protected override void TranslateDownloadImage(List<string> output, Expression key, Expression path)
		{
			output.Add("download_image_from_internet(");
			this.Translator.TranslateExpression(output, key);
			output.Add(", ");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateDrawEllipse(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			// TODO: use alpha
			output.Add("_PDE(_global_vars['virtual_screen'], (");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add("), _PR(");
			this.Translator.TranslateExpression(output, left);
			output.Add(", ");
			this.Translator.TranslateExpression(output, top);
			output.Add(", ");
			this.Translator.TranslateExpression(output, width);
			output.Add(", ");
			this.Translator.TranslateExpression(output, height);
			output.Add("))");
		}

		protected override void TranslateDrawLine(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression lineWidth, Expression red, Expression green, Expression blue, Expression alpha)
		{
			output.Add("_PDL(_global_vars['virtual_screen'], (");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add("), (");
			this.Translator.TranslateExpression(output, ax);
			output.Add(", ");
			this.Translator.TranslateExpression(output, ay);
			output.Add("), (");
			this.Translator.TranslateExpression(output, bx);
			output.Add(", ");
			this.Translator.TranslateExpression(output, by);
			output.Add("), ");
			this.Translator.TranslateExpression(output, lineWidth);
			output.Add(")");
		}

		protected override void TranslateDrawRectangle(List<string> output, Expression left, Expression top, Expression width, Expression height, Expression red, Expression green, Expression blue, Expression alpha)
		{
			output.Add("draw_rectangle(");
			this.Translator.TranslateExpression(output, left);
			output.Add(", ");
			this.Translator.TranslateExpression(output, top);
			output.Add(", ");
			this.Translator.TranslateExpression(output, width);
			output.Add(", ");
			this.Translator.TranslateExpression(output, height);
			output.Add(", ");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add(", ");
			this.Translator.TranslateExpression(output, alpha);
			output.Add(")");
		}

		protected override void TranslateDrawTriangle(List<string> output, Expression ax, Expression ay, Expression bx, Expression by, Expression cx, Expression cy, Expression red, Expression green, Expression blue, Expression alpha)
		{
			output.Add("draw_triangle(");
			this.Translator.TranslateExpression(output, ax);
			output.Add(", ");
			this.Translator.TranslateExpression(output, ay);
			output.Add(", ");
			this.Translator.TranslateExpression(output, bx);
			output.Add(", ");
			this.Translator.TranslateExpression(output, by);
			output.Add(", ");
			this.Translator.TranslateExpression(output, cx);
			output.Add(", ");
			this.Translator.TranslateExpression(output, cy);
			output.Add(", ");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add(", ");
			this.Translator.TranslateExpression(output, alpha);
			output.Add(")");
		}

		protected override void TranslateExponent(List<string> output, Expression baseNum, Expression powerNum)
		{
			output.Add("float(");
			this.Translator.TranslateExpression(output, baseNum);
			output.Add(" ** ");
			this.Translator.TranslateExpression(output, powerNum);
			output.Add(")");
		}

		protected override void TranslateFillScreen(List<string> output, Expression red, Expression green, Expression blue)
		{
			output.Add("_global_vars['virtual_screen'].fill((");
			this.Translator.TranslateExpression(output, red);
			output.Add(", ");
			this.Translator.TranslateExpression(output, green);
			output.Add(", ");
			this.Translator.TranslateExpression(output, blue);
			output.Add("))");
		}

		protected override void TranslateForceParens(List<string> output, Expression expression)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, expression);
			output.Add(")");
		}

		protected override void TranslateGamepadEnableDevice(List<string> output, Expression device)
		{
			output.Add("_gamepad_enable_device(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetAxisCount(List<string> output, Expression device)
		{
			output.Add("_gamepad_axis_count(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetAxisValue(List<string> output, Expression device, Expression axisIndex)
		{
			output.Add("_gamepad_axis_value(");
			this.Translator.TranslateExpression(output, device);
			output.Add(", ");
			this.Translator.TranslateExpression(output, axisIndex);
			output.Add(")");
		}

		protected override void TranslateGamepadGetButtonCount(List<string> output, Expression device)
		{
			output.Add("_gamepad_button_count(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetDeviceCount(List<string> output)
		{
			output.Add("_get_gamepad_count()");
		}

		protected override void TranslateGamepadGetDeviceName(List<string> output, Expression device)
		{
			output.Add("_get_gamepad_name(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetHatCount(List<string> output, Expression device)
		{
			output.Add("_game_gamepad_hat_count(");
			this.Translator.TranslateExpression(output, device);
			output.Add(")");
		}

		protected override void TranslateGamepadGetRawDevice(List<string> output, Expression index)
		{
			output.Add("_get_gamepad_device(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateGamepadIsButtonPressed(List<string> output, Expression device, Expression buttonIndex)
		{
			output.Add("_gamepad_is_button_pressed(");
			this.Translator.TranslateExpression(output, device);
			output.Add(", ");
			this.Translator.TranslateExpression(output, buttonIndex);
			output.Add(")");
		}

		protected override void TranslateGetEventsRawList(List<string> output)
		{
			output.Add("_pygame_pump_events()");
		}

		protected override void TranslateGetProgramData(List<string> output)
		{
			output.Add("program_data[0]");
		}

		protected override void TranslateGetRawByteCodeString(List<string> output, string theString)
		{
			output.Add("\"");
			output.Add(theString);
			output.Add("\"");
		}

		protected override void TranslateHttpRequest(List<string> output, Expression httpRequest, Expression method, Expression url, Expression body, Expression userAgent, Expression contentType, Expression contentLength, Expression headerNameList, Expression headerValueList)
		{
			output.Add("_http_request_impl(");
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
			// Python loads resources synchronously.
			throw new InvalidOperationException();
		}

		protected override void TranslateImageCreateFlippedCopyOfNativeBitmap(List<string> output, Expression image, Expression flipX, Expression flipY)
		{
			output.Add("_pygame_flip_image(");
			this.Translator.TranslateExpression(output, image);
			output.Add(", ");
			this.Translator.TranslateExpression(output, flipX);
			output.Add(", ");
			this.Translator.TranslateExpression(output, flipY);
			output.Add(")");
		}

		protected override void TranslateImageImagetteFlushToNativeBitmap(List<string> output, Expression imagette)
		{
			output.Add("flush_imagette(");
			this.Translator.TranslateExpression(output, imagette);
			output.Add(")");
		}

		protected override void TranslateImageInitiateAsyncDownloadOfResource(List<string> output, Expression path)
		{
			// Python loads resources synchronously.
			throw new InvalidOperationException();
		}

		protected override void TranslateImageNativeBitmapHeight(List<string> output, Expression bitmap)
		{
			this.Translator.TranslateExpression(output, bitmap);
			output.Add(".get_height()");
		}

		protected override void TranslateImageNativeBitmapWidth(List<string> output, Expression bitmap)
		{
			this.Translator.TranslateExpression(output, bitmap);
			output.Add(".get_width()");
		}

		protected override void TranslateImageScaleNativeResource(List<string> output, Expression bitmap, Expression width, Expression height)
		{
			output.Add("pygame.transform.scale(");
			this.Translator.TranslateExpression(output, bitmap);
			output.Add(", (");
			this.Translator.TranslateExpression(output, width);
			output.Add(", ");
			this.Translator.TranslateExpression(output, height);
			output.Add("))");
		}

		protected override void TranslateInitializeGameWithFps(List<string> output, Expression fps)
		{
			output.Add("platform_begin(");
			this.Translator.TranslateExpression(output, fps);
			output.Add(")");
		}

		protected override void TranslateInitializeScreen(List<string> output, Expression gameWidth, Expression gameHeight, Expression screenWidth, Expression screenHeight)
		{
			output.Add("_pygame_initialize_screen(");
			this.Translator.TranslateExpression(output, gameWidth);
			output.Add(", ");
			this.Translator.TranslateExpression(output, gameHeight);
			output.Add(", ");
			if (screenWidth is NullConstant)
			{
				output.Add("None");
			}
			else
			{
				output.Add("(");
				this.Translator.TranslateExpression(output, screenWidth);
				output.Add(this.Shorten(", "));
				this.Translator.TranslateExpression(output, screenHeight);
				output.Add(")");
			}
			output.Add(")");
		}

		protected override void TranslateInt(List<string> output, Expression value)
		{
			output.Add("int(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateIoCreateDirectory(List<string> output, Expression path)
		{
			output.Add("io_create_directory(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateIoCurrentDirectory(List<string> output)
		{
			output.Add("io_helper_current_directory()");
		}

		protected override void TranslateIoDeleteDirectory(List<string> output, Expression path, Expression isRecursive)
		{
			output.Add("io_delete_directory(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isRecursive);
			output.Add(")");
		}

		protected override void TranslateIoDeleteFile(List<string> output, Expression path, Expression isUserData)
		{
			output.Add("io_delete_file(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateIoDoesPathExist(List<string> output, Expression canonicalizedPath, Expression directoriesOnly, Expression performCaseCheck, Expression isUserData)
		{
			output.Add("io_helper_check_path(");
			this.Translator.TranslateExpression(output, canonicalizedPath);
			output.Add(", ");
			this.Translator.TranslateExpression(output, directoriesOnly);
			output.Add(", ");
			this.Translator.TranslateExpression(output, performCaseCheck);
			output.Add(")");
		}

		protected override void TranslateIoFileReadText(List<string> output, Expression path, Expression isUserData)
		{
			output.Add("io_helper_read_text(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateIoFilesInDirectory(List<string> output, Expression verifiedCanonicalizedPath, Expression isUserData)
		{
			output.Add("os.listdir(");
			this.Translator.TranslateExpression(output, verifiedCanonicalizedPath);
			output.Add(")");
		}

		protected override void TranslateIoFileWriteText(List<string> output, Expression path, Expression content, Expression isUserData)
		{
			output.Add("io_helper_write_text(");
			this.Translator.TranslateExpression(output, path);
			output.Add(", ");
			this.Translator.TranslateExpression(output, content);
			output.Add(")");
		}

		protected override void TranslateIsValidInteger(List<string> output, Expression number)
		{
			output.Add("_is_valid_integer(");
			this.Translator.TranslateExpression(output, number);
			output.Add(")");
		}

		protected override void TranslateIsWindowsProgram(List<string> output)
		{
			output.Add("(sys.platform == 'win32')");
		}

		protected override void TranslateLaunchBrowser(List<string> output, Expression url)
		{
			output.Add("_launch_browser(");
			this.Translator.TranslateExpression(output, url);
			output.Add(")");
		}

		protected override void TranslateListClear(List<string> output, Expression list)
		{
			throw new Exception("This should have been optimized out.");
		}

		protected override void TranslateListConcat(List<string> output, Expression listA, Expression listB)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, listA);
			output.Add(" + ");
			this.Translator.TranslateExpression(output, listB);
			output.Add(")");
		}

		protected override void TranslateListGet(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateListInsert(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".insert(");
			this.Translator.TranslateExpression(output, index);
			output.Add(", ");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListJoin(List<string> output, Expression list, Expression sep)
		{
			this.Translator.TranslateExpression(output, sep);
			output.Add(".join(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListJoinChars(List<string> output, Expression list)
		{
			output.Add("''.join(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListLastIndex(List<string> output, Expression list)
		{
			output.Add("-1");
		}

		protected override void TranslateListLength(List<string> output, Expression list)
		{
			output.Add("len(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateListPop(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".pop()");
		}

		protected override void TranslateListPush(List<string> output, Expression list, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".append(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateListRemoveAt(List<string> output, Expression list, Expression index)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".pop(");
			this.Translator.TranslateExpression(output, index);
			output.Add(")");
		}

		protected override void TranslateListReverseInPlace(List<string> output, Expression list)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(".reverse()");
		}

		protected override void TranslateListSet(List<string> output, Expression list, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateListShuffleInPlace(List<string> output, Expression list)
		{
			output.Add("random.shuffle(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateMultiplyList(List<string> output, Expression list, Expression num)
		{
			this.Translator.TranslateExpression(output, list);
			output.Add(this.Shorten(" * "));
			this.Translator.TranslateExpression(output, num);
		}

		protected override void TranslateMusicLoadFromResource(List<string> output, Expression filename, Expression intOutStatus)
		{
			output.Add("_music_load(");
			this.Translator.TranslateExpression(output, filename);
			output.Add(", ");
			this.Translator.TranslateExpression(output, intOutStatus);
			output.Add(", True)");
		}

		protected override void TranslateMusicPlayNow(List<string> output, Expression musicNativeObject, Expression musicRealPath, Expression isLooping)
		{
			output.Add("_music_play_now(");
			this.Translator.TranslateExpression(output, musicRealPath);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isLooping);
			output.Add(")");
		}

		protected override void TranslateMusicSetVolume(List<string> output, Expression musicNativeObject, Expression ratio)
		{
			output.Add("_music_set_volume(");
			this.Translator.TranslateExpression(output, ratio);
			output.Add(")");
		}

		private void TranslateNewPythonList(List<string> output, Expression size)
		{
			if (size is IntegerConstant)
			{
				int length = ((IntegerConstant)size).Value;
				if (length == 0)
				{
					output.Add("[]");
				}
				else if (length == 1)
				{
					output.Add("[None]");
				}
				else
				{
					output.Add("[None]");
					output.Add(this.Shorten(" * "));
					output.Add("" + length);
				}
			}
			else
			{
				output.Add("[None] * ");
				this.Translator.TranslateExpression(output, size);
			}
		}

		protected override void TranslateNewArray(List<string> output, StringConstant type, Expression size)
		{
			this.TranslateNewPythonList(output, size);
		}

		protected override void TranslateNewDictionary(List<string> output, StringConstant keyType, StringConstant valueType)
		{
			output.Add("{}");
		}

		protected override void TranslateNewList(List<string> output, StringConstant type)
		{
			output.Add("[]");
		}

		protected override void TranslateNewListOfSize(List<string> output, StringConstant type, Expression length)
		{
			this.TranslateNewPythonList(output, length);
		}

		protected override void TranslateNewStack(List<string> output, StringConstant type)
		{
			output.Add("[]");
		}

		protected override void TranslateOrd(List<string> output, Expression character)
		{
			output.Add("ord(");
			this.Translator.TranslateExpression(output, character);
			output.Add(")");
		}

		protected override void TranslatePauseForFrame(List<string> output)
		{
			output.Add("_pygame_end_of_frame()");
		}

		protected override void TranslateParseFloat(List<string> output, Expression outParam, Expression rawString)
		{
			output.Add("_parse_float_helper(");
			this.Translator.TranslateExpression(output, outParam);
			output.Add(", ");
			this.Translator.TranslateExpression(output, rawString);
			output.Add(")");
		}

		protected override void TranslateParseInt(List<string> output, Expression rawString)
		{
			output.Add("int(");
			this.Translator.TranslateExpression(output, rawString);
			output.Add(")");
		}

		protected override void TranslateParseJson(List<string> output, Expression rawString)
		{
			output.Add("_parse_json(");
			this.Translator.TranslateExpression(output, rawString);
			output.Add(")");
		}

		protected override void TranslatePrint(List<string> output, Expression message)
		{
			output.Add("print(");
			this.Translator.TranslateExpression(output, message);
			output.Add(")");
		}

		protected override void TranslateRandomFloat(List<string> output)
		{
			output.Add("random.random()");
		}

		protected override void TranslateReadLocalImageResource(List<string> output, Expression filePath)
		{
			output.Add("load_local_image_resource(");
			this.Translator.TranslateExpression(output, filePath);
			output.Add(")");
		}

		protected override void TranslateReadLocalSoundResource(List<string> output, Expression filePath)
		{
			output.Add("readLocalSoundResource(");
			this.Translator.TranslateExpression(output, filePath);
			output.Add(")");
		}

		protected override void TranslateReadLocalTileResource(List<string> output, Expression tileGenName)
		{
			output.Add("load_local_tile_resource(");
			this.Translator.TranslateExpression(output, tileGenName);
			output.Add(")");
		}

		protected override void TranslateRegisterTicker(List<string> output)
		{
			throw new Exception("This code path should be optimized out of the python translation.");
		}

		protected override void TranslateRegisterTimeout(List<string> output)
		{
			throw new Exception("This code path should be optimized out of the python translation.");
		}

		protected override void TranslateResourceReadText(List<string> output, Expression path)
		{
			output.Add("_read_resource_text(");
			this.Translator.TranslateExpression(output, path);
			output.Add(")");
		}

		protected override void TranslateSetProgramData(List<string> output, Expression programData)
		{
			output.Add("program_data[0] = ");
			this.Translator.TranslateExpression(output, programData);
		}

		protected override void TranslateSetTitle(List<string> output, Expression title)
		{
			output.Add("pygame.display.set_caption(");
			this.Translator.TranslateExpression(output, title);
			output.Add(")");
		}

		protected override void TranslateSin(List<string> output, Expression value)
		{
			output.Add("math.sin(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateSortPrimitiveValues(List<string> output, Expression valueList, Expression isString)
		{
			output.Add("sort_primitive_value_list(");
			this.Translator.TranslateExpression(output, valueList);
			output.Add(", ");
			this.Translator.TranslateExpression(output, isString);
			output.Add(")");
		}

		protected override void TranslateSortedCopyOfIntArray(List<string> output, Expression list)
		{
			output.Add("create_sorted_copy_of_list(");
			this.Translator.TranslateExpression(output, list);
			output.Add(")");
		}

		protected override void TranslateSoundPlay(List<string> output, Expression soundInstance)
		{
			output.Add("playSoundImpl(");
			this.Translator.TranslateExpression(output, soundInstance);
			output.Add(")");
		}

		protected override void TranslateStackGet(List<string> output, Expression stack, Expression index)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateStackLength(List<string> output, Expression stack)
		{
			output.Add("len(");
			this.Translator.TranslateExpression(output, stack);
			output.Add(")");
		}

		protected override void TranslateStackPop(List<string> output, Expression stack)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".pop()");
		}

		protected override void TranslateStackPush(List<string> output, Expression stack, Expression value)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add(".append(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStackSet(List<string> output, Expression stack, Expression index, Expression value)
		{
			this.Translator.TranslateExpression(output, stack);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("] = ");
			this.Translator.TranslateExpression(output, value);
		}

		protected override void TranslateStringAsChar(List<string> output, StringConstant stringConstant)
		{
			this.Translator.TranslateExpression(output, stringConstant);
		}

		protected override void TranslateStringCast(List<string> output, Expression thing, bool strongCast)
		{
			output.Add("str(");
			this.Translator.TranslateExpression(output, thing);
			output.Add(")");
		}

		protected override void TranslateStringCharAt(List<string> output, Expression stringValue, Expression index)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add("[");
			this.Translator.TranslateExpression(output, index);
			output.Add("]");
		}

		protected override void TranslateStringCompare(List<string> output, Expression a, Expression b)
		{
			output.Add("cmp(");
			this.Translator.TranslateExpression(output, a);
			output.Add(", ");
			this.Translator.TranslateExpression(output, b);
			output.Add(")");
		}

		protected override void TranslateStringContains(List<string> output, Expression haystack, Expression needle)
		{
			output.Add("(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(" in ");
			this.Translator.TranslateExpression(output, haystack);
			output.Add(")");
		}

		protected override void TranslateStringEndsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".endswith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringEquals(List<string> output, Expression aNonNull, Expression b)
		{
			this.Translator.TranslateExpression(output, aNonNull);
			output.Add(" == ");
			this.Translator.TranslateExpression(output, b);
		}

		protected override void TranslateStringFromCode(List<string> output, Expression characterCode)
		{
			output.Add("wrappedChr(");
			this.Translator.TranslateExpression(output, characterCode);
			output.Add(")");
		}

		protected override void TranslateStringIndexOf(List<string> output, Expression haystack, Expression needle)
		{
			this.Translator.TranslateExpression(output, haystack);
			output.Add(".find(");
			this.Translator.TranslateExpression(output, needle);
			output.Add(")");
		}

		protected override void TranslateStringLength(List<string> output, Expression stringValue)
		{
			output.Add("len(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringLower(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".lower()");
		}

		protected override void TranslateStringParseFloat(List<string> output, Expression stringValue)
		{
			output.Add("float(");
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(")");
		}

		protected override void TranslateStringParseInt(List<string> output, Expression value)
		{
			output.Add("int(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateStringReplace(List<string> output, Expression stringValue, Expression findMe, Expression replaceWith)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".replace(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(", ");
			this.Translator.TranslateExpression(output, replaceWith);
			output.Add(")");
		}

		protected override void TranslateStringReverse(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add("[::-1]");
		}

		protected override void TranslateStringSplit(List<string> output, Expression stringExpr, Expression sep)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".split(");
			this.Translator.TranslateExpression(output, sep);
			output.Add(")");
		}

		protected override void TranslateStringStartsWith(List<string> output, Expression stringExpr, Expression findMe)
		{
			this.Translator.TranslateExpression(output, stringExpr);
			output.Add(".startswith(");
			this.Translator.TranslateExpression(output, findMe);
			output.Add(")");
		}

		protected override void TranslateStringTrim(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".strip()");
		}

		protected override void TranslateStringUpper(List<string> output, Expression stringValue)
		{
			this.Translator.TranslateExpression(output, stringValue);
			output.Add(".upper()");
		}

		protected override void TranslateTan(List<string> output, Expression value)
		{
			output.Add("math.tan(");
			this.Translator.TranslateExpression(output, value);
			output.Add(")");
		}

		protected override void TranslateUnregisterTicker(List<string> output)
		{
			throw new Exception("This code path should be optimized out of the python translation.");
		}

		protected override void TranslateUnsafeFloatDivision(List<string> output, Expression numerator, Expression denominator)
		{
			output.Add("1.0 * ");
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" / ");
			this.Translator.TranslateExpression(output, denominator);
		}

		protected override void TranslateUnsafeIntegerDivision(List<string> output, Expression numerator, Expression denominator)
		{
			this.Translator.TranslateExpression(output, numerator);
			output.Add(" // ");
			this.Translator.TranslateExpression(output, denominator);
		}
	}
}
