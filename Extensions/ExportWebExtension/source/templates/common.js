
function C$runInterpreter(execId) {
	
	throw new Error("Not implemented");
	/*
	let vm = C$common$programData;
	return C$handleVmResult(runInterpreter(vm, execId));
	*/
}

function C$runInterpreterWithFunctionPointer(fp, args) {
	throw new Error("Not implemented");
	/*
	let vm = C$common$programData;
	return C$handleVmResult(runInterpreterWithFunctionPointer(vm, fp, args));
	*/
}

let C$convertNativeArgToValue = value => {
	
	throw new Error("Not implemented");
	/*
	let globals = vmGetGlobals(C$common$programData);
	if (value === null || value === undefined) return buildNull(globals);
	if (value === true || value === false) return buildBoolean(globals, value);
	switch (typeof(value)) {
		case 'string': return buildString(globals, value);
		case 'number': return value % 1 === 0 ? buildInteger(globals, value) : buildFloat(globals, value);
		case 'object':
			if (Array.isArray(value)) {
				let arr = [];
				for (let item of value) {
					arr.push(C$convertNativeArgToValue(item));
				}
				return buildList(arr);
			}
			let keys = Object.keys(value);
			let values = keys.map(k => C$convertNativeArgToValue(value[k]));
			return buildStringDictionary(globals, keys, values);
		default: return buildNull(globals);
	}
	*/
};

function C$runInterpreterWithFunctionPointerNativeArgs(fp, args) {
	
	throw new Error("Not implemented");
	//return C$runInterpreterWithFunctionPointer(fp, args.map(C$convertNativeArgToValue));
}

function C$handleVmResult(res) {
	throw new Error("Not implemented");
	/*
	let vm = C$common$programData;
	let status = getVmResultStatus(res);
	if (status == 5) { // REINVOKE
		let delayMillis = Math.floor(1000 * getVmReinvokeDelay(res));
		let execId = getVmResultExecId(res);
		window.setTimeout(function() { C$runInterpreter(execId); }, delayMillis);
	} else {
		if (status == 1 || status == 2 || status == 3) return;
		throw "Unknown status";
	}
	//*/
}

const COMMON = (() => {

	let runtime = null; // set externally

	let alwaysTrue = () => true;
	let alwaysFalse = () => false;
	
	let print = v => {
		console.log(v);
	};

	let timedCallback = (vm, fn, delay) => { 
		// setTimeout(() => C$runInterpreterWithFunctionPointer(fn, []), Math.floor(delay * 1000 + .5)); 
		throw new Error("Not implemented");
	};
	
	let res = (() => {
		let readBytes = (reader, path, isText, useB64, bytesOut, posInts) => {
			throw new Error("Not implemented");
		};
		
		let readText = (reader, path, isText) => {
			throw new Error("Not implemented");
		};
		return { readBytes, readText };
	})();

	let dateTime = (() => {
		
		let utcTimeZone = {
			name: "utc",
			isLocal: false,
			observesDst: false
		};
		
		let extractTimeZone = (nullableTz) => {
			if (nullableTz === null) return utcTimeZone;
			return nullableTz;
		};
		
		let getDataForLocalTimeZone = (strOut, intOut) => {
			let name = Intl.DateTimeFormat().resolvedOptions().timeZone;
			strOut[0] = name;
			strOut[1] = name;
			intOut[0] = -1; // TODO: remove this
			let now = new Date();
			let janSeconds = new Date(now.getFullYear(), 1, 1, 0, 0, 0).getTime() / 1000;
			let junSeconds = new Date(now.getFullYear(), 6, 1, 0, 0, 0).getTime() / 1000;
			let janToJunHours = (junSeconds - janSeconds) / 3600;
			let dstOccurs = (janToJunHours % 24) != 0;
			intOut[1] = dstOccurs ? 1 : 0;
			return {
				name: name,
				isLocal: true,
				observesDst: dstOccurs
			};
		};
		
		let unixToStructured = (intOut, nullableTimeZone, unixTime) => {
			let unixTimeInt = Math.floor(unixTime);
			let micros = Math.floor(1000000 * (unixTime - unixTimeInt));
			let millis = Math.floor(micros / 1000);
			let tz = extractTimeZone(nullableTimeZone);
			let d = new Date(Math.floor(unixTime * 1000));
			intOut[6] = millis;
			intOut[7] = micros;
			if (tz.isLocal) {
				intOut[0] = d.getFullYear();
				intOut[1] = d.getMonth() + 1;
				intOut[2] = d.getDate();
				intOut[3] = d.getHours();
				intOut[4] = d.getMinutes();
				intOut[5] = d.getSeconds();
				intOut[8] = d.getDay() + 1;
			} else {
				let p = d.toISOString().split('Z')[0].split('T');
				let pd = p[0].split('-');
				let pt = p[1].split('.')[0].split(':');
				
				intOut[0] = parseInt(pd[0]);
				intOut[1] = parseInt(pd[1]);
				intOut[2] = parseInt(pd[2]);
				intOut[3] = parseInt(pt[0]);
				intOut[4] = parseInt(pt[1]);
				intOut[5] = parseInt(pt[2]);
				intOut[8] = new Date(intOut[0], intOut[1] - 1, intOut[2], 12, 0, 0).getDay() + 1;
			}
			return true;
		};
		
		let pad = (n, length) => {
			n = n + '';
			while (n.length < length) {
				n = '0' + n;
			}
			return n;
		};
		let parseDate = (intOut, nullableTimeZone, year, month, day, hour, minute, microseconds) => {
			let tz = extractTimeZone(nullableTimeZone);
			intOut[2] = microseconds % 1000000;
			intOut[0] = 1;
			let seconds = Math.floor((microseconds - intOut[2]) / 1000000);
			if (tz.isLocal) {
				let d = new Date(year, month - 1, day, hour, minute, seconds);
				intOut[1] = Math.floor(d.getTime() / 1000);
			} else {
				intOut[1] = new Date([
					year, "-",
					pad(month, 2), "-",
					pad(day, 2), "T",
					pad(hour, 2), ":",
					pad(minute, 2), ":",
					pad(seconds, 2), "Z"].join('')).getTime();
			}
		};
		
		let getUtcOffsetAt = (tz, unixTime) => {
			let d = new Date(Math.floor(unixTime * 1000));
			return -60 * d.getTimezoneOffset();
		};
		
		let isDstOccurringAt = (tz, unixTime) => { throw new Error("Not implemented"); };
		let initializeTimeZoneList = () => { throw new Error("Not implemented"); };

		return { parseDate, unixToStructured, getDataForLocalTimeZone, getUtcOffsetAt, isDstOccurringAt, initializeTimeZoneList };
	})();

	let interop = (() => {
		
		let getInteropObj = () => {
			if (!window.CrayonInterop) window.CrayonInterop = {};
			return window.CrayonInterop;
		};
		
		let invoke = (fnName, argsStr, cbValue) => {
			let fn = getInteropObj()[fnName];
			if (!fn) return false;
			let args = JSON.parse(argsStr).args;
			setTimeout(() => {
				let result = fn.apply(null, args);
				if (cbValue !== null) {
					Promise.resolve(result).then(value => {
						let valueStr = JSON.stringify({ value });
						C$runInterpreterWithFunctionPointerNativeArgs(cbValue, [valueStr]);
					});
				}
			}, 0);
			return true;
		};
		
		let callbacksById = {};
		
		let registerCallback = (funcName, useReturn, funcValue) => {
			let ci = getInteropObj();
			if (!ci.INTERNAL_ID_ALLOC) ci.INTERNAL_ID_ALLOC = 0;
			ci[funcName] = function() {
				let args = [];
				for (let i = 0; i < arguments.length; ++i) {
					args.push(arguments[i]);
				}
				let argStr = JSON.stringify({ args });
				let id = 0;
				let resolve = null;
				let p = null;
				if (useReturn) {
					p = new Promise(res => {resolve = res; });
					id = ++ci.INTERNAL_ID_ALLOC;
					callbacksById[id] = resolve;
				} else {
					p = Promise.resolve(null);
				}
				setTimeout(() => {
					C$runInterpreterWithFunctionPointerNativeArgs(funcValue, [id, argStr]);
				}, 0);
				
				return p;
			};
		};
		
		let callbackReturn = (id, valueStr) => {
			if (id === 0) return;
			let fn = callbacksById[id];
			delete callbacksById[id];
			let retVal = JSON.parse(valueStr).value;
			fn(retVal);
		};
		
		return { callbackReturn, invoke, registerCallback };
	})();

	let b64Decode = (() => { // It's late 2020, but in Android WebView it's still 2009!
		let lu = {};
		lu['+'] = 62;
		lu['/'] = 63;
		lu['-'] = 62;
		lu['_'] = 63;
		let letters = 'abcdefghijklmnopqrstuvwxyz';
		for (let i = 0; i < 26; ++i) {
			lu[letters.charAt(i).toUpperCase()] = i;
			lu[letters.charAt(i)] = i + 26;
			lu[i + ''] = 52 + i;
		}
		return s => {
			let pairs = [];
			let len = s.length;
			let c;
			for (let i = 0; i < len; ++i) {
				c = lu[s.charAt(i)];
				if (c !== '=') pairs.push(c >> 4, (c >> 2) & 3, c & 3);
			}
			let buf = [];
			for (let i = 0; i + 3 < pairs.length; i += 4) {
				buf.push((pairs[i] << 6) + (pairs[i + 1] << 4) + (pairs[i + 2] << 2) + (pairs[i + 3]));
			}
			return buf;
		};
	})();
	
	let httpSend = (vm, cb, failCb, url, method, contentType, binaryContent, textContent, headersKvp, bytesObj, bytesObjNativeData) => {
		let req = new XMLHttpRequest();
		
		req.onload = (e, e2) => {
			let headers = [];
			let ct = '';
			for (let rawHeader of req.getAllResponseHeaders().trim().split('\r\n')) {
				let t = rawHeader.split(':', 2);
				let name = t[0].trim();
				let value = t.length > 1 ? t[1].trim() : '';
				headers.push(name);
				headers.push(value);
				if (name.toLowerCase() == 'content-type') ct = value;
			}
	
			let byteArrayPr = req.response.arrayBuffer ?
				req.response.arrayBuffer().then(buf => {
					return new Uint8Array(buf);
				}) : (async () => {
					let res = null;
					let p = new Promise(r => { res = r; });
					let fileReader = new FileReader();
					fileReader.readAsDataURL(req.response);
					fileReader.onloadend = () => {
						let b64Value = fileReader.result.split('base64,')[1];
						let bytes = b64Decode(b64Value);
						res(new Uint8Array(bytes));
					};
					return p;
				})();
	
			byteArrayPr.then(arr => {
				let nd = [];
				let bl = arr.length;
				for (let i = 0; i < bl; ++i) {
					nd.push(arr[i]);
				}
				bytesObjNativeData[0] = nd;
				let text = null;
				try {
					let textArr = nd.slice(0);
					while (textArr.length && textArr[textArr.length - 1] === 0) textArr.pop();
					text = new TextDecoder().decode(new Uint8Array(textArr));
				} catch (_) { }
				
				C$runInterpreterWithFunctionPointer(cb, [
					C$convertNativeArgToValue(req.status),
					C$convertNativeArgToValue(req.statusText),
					C$convertNativeArgToValue(ct),
					C$convertNativeArgToValue(headers),
					bytesObj,
					C$convertNativeArgToValue(text)
				]);
			});
		};
		
		req.onerror = (e1, e2) => {
			C$runInterpreterWithFunctionPointer(failCb, []);
		};
		
		req.open(method, url, true);
	
		if (textContent !== null || binaryContent !== null) {
			req.setRequestHeader('Content-Type', contentType);
		}
	
		req.responseType = 'blob';
		
		for (let i = 0; i < headersKvp.length; i += 2) {
			let n = headersKvp[i];
			let v = headersKvp[i + 1];
			switch (n.toLowerCase()) {
				// These are forbidden by the browser.
				case 'content-length':
				case 'user-agent':
					break;
				default:
					req.setRequestHeader(n, v);
					break;
			}
		}
		
		if (textContent !== null) {
			req.send(textContent);
		} else if (binaryContent !== null) {
			req.send(Int8Array.from(binaryContent));
		} else {
			req.send(null);
		}
	};
	
	let imageUtil = (() => {
		
		let chunkLoadAsync = (vm, loadedChunks, chunkId, chunkIds, cb) => {
			let resLoader = runtime.vmGetResourceReaderObj(vm);
			let evLoop = runtime.vmGetEventLoopObj(vm);
			let path = 'resources/ch_' + chunkId + '.png';
			resLoader.cbxBundle.getImageResource(path).then(canvas => {
				loadedChunks[chunkId] = { canvas, ctx: canvas.getContext('2d'), b64: null };
				let total = chunkIds.length;
				let loaded = chunkIds.filter(id => loadedChunks[id]).length;
				evLoop.runVmWithNativeArgs(cb, [chunkId, loaded, total]);
			});
		};
		
		let scale = (bmp, w, h, algo) => {
			if (algo !== 1) throw new Error();
			let canvas = document.createElement('canvas');
			canvas.width = w;
			canvas.height = h;
			let ctx = canvas.getContext('2d');
			ctx.drawImage(bmp.canvas, 0, 0, w, h);
			return { canvas, ctx, b64: null };
		};
		
		let newBitmap = (w, h) => {
			let cnv = document.createElement('canvas');
			cnv.width = w;
			cnv.height = h;
			return {
				canvas: cnv,
				ctx: cnv.getContext('2d'),
				b64: null,
			};
		};
		
		let fromBytes = (data, szOut, ndOut, cb) => {
			if (data.length < 2) return null;
			let isPng = data.substr(0, 2) == 'iV';
			let loader = new Image();
			loader.onload = () => {
				let cnv = document.createElement('canvas');
				let [w, h] = [loader.width, loader.height];
				cnv.width = w;
				cnv.height = h;
				let ctx = cnv.getContext('2d');
				ctx.drawImage(loader, 0, 0);
				ndOut[0] = {
					canvas: cnv,
					ctx,
					b64: data,
				};
				C$runInterpreterWithFunctionPointerNativeArgs(cb, [0, w, h]);
			};
			loader.onerror = () => {
				C$runInterpreterWithFunctionPointerNativeArgs(cb, [2, 0, 0]);
			};
			loader.src = ('data:image/' + (isPng ? 'png' : 'jpeg') + ';base64,') + data;
			return false;
		};
		
		let getPixel = (bmp, optEdit, x, y, cOut) => {
			let img = bmp;
			if (optEdit !== null) {
				img = optEdit;
			}
			let c = img.canvas;
			let w = c.width;
			let h = c.height;
			if (x < 0 || y < 0 || x >= w || y >= h) {
				cOut[4] = 0;
				return;
			}
			cOut[4] = 1;
			let px = bmp.ctx.getImageData(x, y, 1, 1).data;
			cOut[0] = px[0];
			cOut[1] = px[1];
			cOut[2] = px[2];
			cOut[3] = px[3];
		};
		
		let singlePixelImgData = null;
		let hexLookup = (() => {
			let o = [];
			let H = '0123456789abcdef'.split('');
			for (let a of H) for (let b of H) {
				o.push(a + b);
			}
			return o;
		})();
		let hexLookupHash = hexLookup.map(_ => '#' + _);
		
		let setPixel = (session, x1, y1, x2, y2, r, g, b, a) => {
			let w = session.width;
			let h = session.height;
			if (x1 < 0 || y1 < 0 || x1 >= w || y1 >= h || x2 < 0 || y2 < 0 || x2 >= w || y2 >= h) return true;
			let ctx = session.ctx;
			if (a == 255) {
				ctx.fillStyle = hexLookupHash[r] + hexLookup[g] + hexLookup[b];
				if (x1 == x2 && y1 == y2) {
					ctx.fillRect(x1, y1, 1, 1);
				} else {
					let x = Math.min(x1, x2);
					let y = Math.min(y1, y2);
					w = Math.abs(x1 - x2) + 1;
					h = Math.abs(y1 - y2) + 1;
					ctx.fillRect(x, y, w, h);
				}
			} else {
				if (singlePixelImgData === null) {
					singlePixelImgData = session.ctx.getImageData(x1, y1, 1, 1);
				}
				let d = singlePixelImgData.data;
				d[0] = r;
				d[1] = g;
				d[2] = b;
				d[3] = a;
		
				if (x1 == x2 && y1 == y2) {
					session.ctx.putImageData(singlePixelImgData, x1, y1);
				} else {
					let xLeft = Math.min(x1, x2);
					let yTop = Math.min(y1, y2);
					let xRight = Math.max(x1, x2);
					let yBottom = Math.max(y1, y2);
					for (let y = yTop; y <= yBottom; ++y) {
						for (let x = xLeft; x <= xRight; ++x) {
							session.ctx.putImageData(singlePixelImgData, x, y);
						}
					}
				}
			}
			return false;
		};
		
		let startEditSession = bmp => {
			let canvas = document.createElement('canvas');
			canvas.width = bmp.canvas.width;
			canvas.height = bmp.canvas.height;
			let ctx = canvas.getContext('2d');
			ctx.drawImage(bmp.canvas, 0, 0);
			return {
				width: canvas.width,
				height: canvas.height,
				canvas,
				ctx,
				parent: bmp,
			};
		};
		
		let endEditSession = (session, bmp) => {
			bmp.canvas = session.canvas;
			bmp.ctx = session.ctx;
			bmp.b64 = null;
			session.canvas = null;
			session.ctx = null;
		};
		
		let encode = (bmp, format, formatOut) => {
			let d = bmp.canvas.toDataURL(format == 1 ? 'image/png' : 'image/jpeg');
			let b64 = d.split(',')[1];
			formatOut[0] = true;
			return b64;
		};
		
		let blit = (target, src, sx, sy, sw, sh, tx, ty, tw, th) => {
			if (tw === sw && th === sh && sx == 0 && sy === 0) {
				target.ctx.drawImage(src.canvas, tx, ty);
			} else {
				target.ctx.imageSmoothingEnabled = false;
				target.ctx.drawImage(src.canvas, sx, sy, sw, sh, tx, ty, tw, th);
			}
		};
		
		return { blit, chunkLoadAsync, encode, endEditSession, fromBytes, getPixel, newBitmap, scale, setPixel, startEditSession };
	})();

	let textEncoding = (() => {
		
		let bytesToText = (bytes, format, strOut) => {
			let i;
			let length = bytes.length;
			let sb = [];
			let a;
			let b;
			let c;
			let cp;
			let t;
			let isBigEndian = format == 5 || format == 7;
			switch (format) {
				case 1:
				case 2:
					let isAscii = format == 1;
					for (i = 0; i < length; ++i) {
						c = bytes[i];
						if (isAscii && c > 127) return 1;
						sb.push(String.fromCharCode(c));
					}
					break;
				case 3:
					i = 0;
					while (i < length) {
						c = bytes[i];
						if ((c & 0x80) == 0) {
							cp = c;
							i++;
						} else if ((c & 0xE0) == 0xC0) {
							if (i + 1 >= length) return 1;
							cp = c & 0x1FF;
							c = bytes[i + 1];
							if ((c & 0xC0) != 0x80) return 1;
							cp = (cp << 6) | (c & 0x3F);
							i += 2;
						} else if ((c & 0xF0) == 0xE0) {
							if (i + 2 >= length) return 1;
							cp = c & 0x0F;
							c = bytes[i + 1];
							if ((c & 0xC0) != 0x80) return 1;
							cp = (cp << 6) | (c & 0x3F);
							c = bytes[i + 2];
							if ((c & 0xC0) != 0x80) return 1;
							cp = (cp << 6) | (c & 0x3F);
							i += 3;
						} else if ((c & 0xF8) == 0xF0) {
							if (i + 3 >= length) return 1;
							cp = c & 0x07;
							c = bytes[i + 1];
							if ((c & 0xC0) != 0x80) return 1;
							cp = (cp << 6) | (c & 0x3F);
							c = bytes[i + 2];
							if ((c & 0xC0) != 0x80) return 1;
							cp = (cp << 6) | (c & 0x3F);
							c = bytes[i + 3];
							if ((c & 0xC0) != 0x80) return 1;
							cp = (cp << 6) | (c & 0x3F);
							i += 4;
						}
						sb.push(String.fromCodePoint(cp));
					}
					break;
				
				case 4:
				case 5:
					for (i = 0; i < length; i += 2) {
						if (isBigEndian) {
							a = bytes[i];
							b = bytes[i + 1];
						} else {
							a = bytes[i + 1];
							b = bytes[i];
						}
						c = (a << 8) | b;
						if (c < 0xD800 || c > 0xDFFF) {
							cp = c;
						} else if (c < 0xD800 && c >= 0xDC00) {
							return 1;
						} else if (i + 3 >= length) {
							return 1;
						} else {
							if (isBigEndian) {
								a = bytes[i + 2];
								b = bytes[i + 3];
							} else {
								a = bytes[i + 3];
								b = bytes[i + 2];
							}
							b = (a << 8) | b;
							a = c;
							// a and b are now 16 bit words
							if (b < 0xDC00 || b > 0xDFFF) return 1;
							cp = (((a & 0x03FF) << 10) | (b & 0x03FF)) + 0x10000;
							i += 2;
						}
						sb.push(String.fromCodePoint(cp));
					}
					break;
				
				case 6:
				case 7:
					for (i = 0; i < length; i += 4) {
						if (isBigEndian) {
							cp = bytes[i + 3] | (bytes[i + 2] << 8) | (bytes[i + 1] << 16) | (bytes[i] << 24);
						} else {
							cp = bytes[i] | (bytes[i + 1] << 8) | (bytes[i + 2] << 16) | (bytes[i + 3] << 24);
						}
						sb.push(String.fromCodePoint(cp));
					}
					break;
			}
			
			strOut[0] = sb.join('');
			return 0;
		};
		
		let stringToCodePointList = (s) => {
			let codePoints = Array.from(s);
			let length = codePoints.length;
			let n;
			for (let i = 0; i < length; ++i) {
				n = codePoints[i].codePointAt(0);
				codePoints[i] = n;
			}
			return codePoints;
		};
		
		let textToBytes = (value, includeBom, format, byteList, positiveIntegers, intOut) => {
			intOut[0] = 0;
			let codePoints = stringToCodePointList(value);
			let length = codePoints.length;
			let maxValue = 0;
			let n = 0;
			let i;
			for (i = 0; i < length; ++i) {
				if (codePoints[i] > maxValue) {
					maxValue = codePoints[i];
				}
			}
			if (maxValue > 127 && format == 1) return 1;
		
			// TODO: this is slightly wrong. Unicode characters should be converted into extended ASCII chars, if possible.
			if (maxValue > 255 && format == 2) return 1;
		
			let bytes;
			if (format <= 2) {
				bytes = codePoints;
			} else {
				bytes = [];
				if (includeBom) {
					switch (format) {
						case 3: bytes = [239, 187, 191]; break;
						case 4: bytes = [255, 254]; break;
						case 5: bytes = [254, 255]; break;
						case 6: bytes = [255, 254, 0, 0]; break;
						case 7: bytes = [0, 0, 254, 255]; break;
					}
				}
				length = codePoints.length;
				switch (format) {
					case 3:
						codePointsToUtf8(codePoints, bytes);
						break;
					case 4:
					case 5:
						codePointsToUtf16(codePoints, bytes, format == 5);
						break;
					case 6:
					case 7:
						codePointsToUtf32(codePoints, bytes, format == 7);
						break;
				}
			}
			
			for (i = 0; i < bytes.length; ++i) {
				byteList.push(positiveIntegers[bytes[i]]);
			}
			
			return 0;
		};
		
		let codePointsToUtf8 = (points, buffer) => {
			let length = points.length;
			let p;
			for (let pIndex = 0; pIndex < length; ++pIndex) {
				p = points[pIndex];
				if (p < 0x80) {
					buffer.push(p);
				} else if (p < 0x0800) {
					buffer.push(0xC0 | ((p >> 6) & 0x1F));
					buffer.push(0x80 | (p & 0x3F));
				} else if (p < 0x10000) {
					buffer.push(0xE0 | ((p >> 12) & 0x0F));
					buffer.push(0x80 | ((p >> 6) & 0x3F));
					buffer.push(0x80 | (p & 0x3F));
				} else {
					buffer.push(0xF0 | ((p >> 18) & 3));
					buffer.push(0x80 | ((p >> 12) & 0x3F));
					buffer.push(0x80 | ((p >> 6) & 0x3F));
					buffer.push(0x80 | (p & 0x3F));
				}
			}
		};
		
		let codePointsToUtf16 = (points, buffer, isBigEndian) => {
			let length = points.length;
			let p;
			let a;
			let b;
			
			for (let pIndex = 0; pIndex < length; ++pIndex) {
				p = points[pIndex];
				if (p < 0x10000) {
					if (isBigEndian) {
						buffer.push((p >> 8) & 255);
						buffer.push(p & 255);
					} else {
						buffer.push(p & 255);
						buffer.push((p >> 8) & 255);
					}
				} else {
					p -= 0x10000;
					a = 0xD800 | ((p >> 10) & 0x03FF);
					b = 0xDC00 | (p & 0x03FF);
					if (isBigEndian) {
						buffer.push((a >> 8) & 255);
						buffer.push(a & 255);
						buffer.push((b >> 8) & 255);
						buffer.push(b & 255);
					} else {
						buffer.push(a & 255);
						buffer.push((a >> 8) & 255);
						buffer.push(b & 255);
						buffer.push((b >> 8) & 255);
					}
				}
			}
		};
		
		let codePointsToUtf32 = (points, buffer, isBigEndian) => {
			let i = 0;
			let length = points.length;
			let p;
			while (i < length) {
				p = points[i++];
				if (isBigEndian) {
					buffer.push((p >> 24) & 255);
					buffer.push((p >> 16) & 255);
					buffer.push((p >> 8) & 255);
					buffer.push(p & 255);
				} else {
					buffer.push(p & 255);
					buffer.push((p >> 8) & 255);
					buffer.push((p >> 16) & 255);
					buffer.push((p >> 24) & 255);
				}
			}
		};
		
		return {
			textToBytes,
			bytesToText,
		};
	
	})();
	
	/////// fakedisk.js
	// TODO: conditionally include fake disk if FileIOCommon is used.
	// TODO: rename these functions and wrap things more nicely
	// TODO: go through and update things from arcaic IE-compatible JS
	/*
		Fake disk is a disk implemented over a simple string-to-string dictionary.
		There are two scenarios for this:
		- Creating a temporary fully sandboxed file system using an empty dictionary {}
		- Creating a persistent data store using the localStorage dictionary.
		
		There are 4 types of keys. All keys have a special prefix to prevent collisions
		with other apps in the case of using localStorage. 
		
		- {prefix}:version - A number. Each time a change occurs to the fakedisk, increment the version
		- {prefix}:nextId - An ID allocator. Used for generating new file ID#'s
		- {prefix}:disk - A serialized string containing all directory and file metadata
		- {prefix}:F{file ID#} - the contents of files with the given ID#. This can either be a string or a hex-encoded byte string 
		
		Fake disk is not optimized for vast and complex virtual disks.
		The most common scenario is storing a couple of relatively small user-preference files.
		
		The disk value is a JSON string.
		Read this string when you are about to make a change and the current version is old.
		Serialize your version back to a string and increment the version in localStorage / the dictionary.
		
		Local disk is a series of nested objects that represent files and directories.
		
		Directory:
		{
			d --> is a directory (boolean, always true)
			c --> created timestamp (int)
			m --> modified timestamp (int)
			f --> files (dictionary keyed by name string)
			root --> true or undefined if this is the root directory
		}
		
		File:
		{
			d --> is a directory (boolean, always false)
			c --> created timestamp (int)
			m --> created timestamp (int)
			i --> id (int)
			s --> size (int)
			b --> is stored as hex string (bool)
		}
		
		Fake Disk object:
		{
			v --> version
			r --> local cache of root object
			s --> string-based storage, either a JS object or localStorage
			u --> uses local storage? (bool)
			p --> prefix
			d --> recently deleted file ID's
		}
			
	*/
	
	let fakedisk_createDefault = () => {
		return {d:true,c:0,m:0,f:{}};
	};
	
	let fakedisk_create = (useLocalStorage) => {
		return {
			v: -1,
			r: fakedisk_createDefault(), // root directory
			u: useLocalStorage, // distinction is necessary to invoke .removeItem(key)
			s: useLocalStorage ? window.localStorage : {},
			p: 'PREFIX:', // TODO get the real file prefix
			d: [], // recently deleted file ID's
		};
	};
	
	let fakedisk_ensureLatest = (disk) => {
		let version = parseInt(disk.s[disk.p + 'version']);
		if (!(version > 0)) { // could be NaN
			fakedisk_format(disk);
			return fakedisk_ensureLatest(disk);
		}
		if (version != disk.v) {
			let json = disk.s[disk.p + 'disk'];
			let success = false;
			if (!!json && json.length > 0) {
				try {
					disk.r = JSON.parse(json);
					disk.v = version;
					success = true;
				} catch (e) {}
			}
			if (!success) {
				fakedisk_format(disk);
				disk.r = fakedisk_createDefault();
				disk.v = 0;
				disk.s[disk.p + 'version'] = disk.v + '';
			}
		}
		disk.r.root = true;
	};
	
	let fakedisk_pushChanges = (disk) => {
		disk.s[disk.p + 'disk'] = JSON.stringify(disk.r);
		disk.s[disk.p + 'version'] = ++disk.v;
		for (let i = 0; i < disk.d.length; ++i) {
			disk.s.removeItem(prefix + 'F' + disk.d[i]);
		}
		disk.d = [];
	};
	
	let fakedisk_format = (disk) => {
		let keys = [];
		for (let key in disk.s) {
			if (key.startsWith(disk.p)) keys.push(key);
		}
		for (let i = 0; i < keys.length; ++i) {
			if (disk.u) disk.s.removeItem(keys[i]);
			else delete disk.s[keys[i]];
		}
		disk.s[disk.p + 'version'] = '1';
		disk.s[disk.p + 'nextId'] = '1';
		disk.s[disk.p + 'disk'] = JSON.stringify(fakedisk_createDefault());
	};
	
	let fakedisk_getNormalizedPath = (path) => {
		let rawParts = path.split('/');
		let parts = [];
		for (let i = 0; i < rawParts.length; ++i) {
			if (rawParts[i].length > 0) {
				parts.push(rawParts[i]);
			}
		}
		return parts;
	}
	
	let fakedisk_getFileName = (path) => {
		let pathParts = fakedisk_getNormalizedPath(path);
		if (pathParts.length == 0) return null;
		return pathParts[pathParts.length - 1];
	}
	
	let fakedisk_now = () => {
		return Math.floor(new Date().getTime());
	};
	
	let fakedisk_getNode = (disk, path, getParent) => {
		let parts = fakedisk_getNormalizedPath(path);
		if (getParent && parts.length == 0) return null;
		if (getParent) parts.pop();
		
		fakedisk_ensureLatest(disk);
		
		let current = disk.r;
		for (let i = 0; i < parts.length; ++i) {
			if (!current.d) return null;
			current = current.f[parts[i]];
			if (current === undefined) return null;
		}
		
		return current;
	};
	
	/*
		status codes:
			0 -> success
			4 -> not found
	*/
	let fakedisk_listdir = (disk, path, includeFullPath, filesOut) => {
		let dir = fakedisk_getNode(disk, path, false);
		if (dir == null || !dir.d) return 4;
		let prefix = '';
		if (includeFullPath) {
			let parts = fakedisk_getNormalizedPath(path);
			if (parts.length == 0) prefix = '/';
			else {
				prefix = '/' + parts.join('/') + '/';
			}
		}
		for (let file in dir.f) {
			filesOut.push(prefix + file);
		}
		filesOut.sort();
		return 0;
	};
	
	/*
		status codes:
		0 -> OK
		1 -> unknown -- occurs when name is invalid or there's something already there
		4 -> directory not found
		8 -> access denied
		11 -> target parent doesn't exist
	*/
	let fakedisk_movedir = (disk, fromPath, toPath) => {
		let toName = fakedisk_getFileName(toPath);
		let fromName = fakedisk_getFileName(fromPath);
		if (!fakedisk_isFileNameValid(toName)) return 1;
		let fromDir = fakedisk_getNode(disk, fromPath, false);
		if (fromDir == null || !fromDir.d) return 4;
		if (!!fromDir.root) return 8;
		let toDir = fakedisk_getNode(disk, toPath, true);
		if (toDir == null || !toDir.d) return 11;
		let toItem = fakedisk_getNode(disk, toPath, false);
		if (toItem != null) return 1;
		let fromParent = fakedisk_getNode(disk, fromPath, true);
		delete fromParent.f[fromName];
		toDir.f[toName] = fromDir;
		fakedisk_pushChanges(disk);
		return 0;
	};
	
	let fakedisk_isFileNameValid = (name) => {
		if (name.length == 0) return false;
		for (let i = 0; i < name.length; ++i) {
			switch (name[i]) {
				case ':':
				case '\\':
				case '\n':
				case '\r':
				case '\0':
				case '\t':
				case '>':
				case '<':
				case '|':
				case '?':
				case '*':
					return false;
				default: break;
			}
		}
		return true;
	};
	
	/*
		status codes:
			0 -> success
			1 -> unknown error
			
			// TODO: need to update translated code to expect better error conditions
			
			1 -> parent does not exist
			2 -> something already exists there
			3 -> invalid directory name
	*/
	let fakedisk_mkdir = (disk, path) => {
		let dir = fakedisk_getNode(disk, path, true);
		if (dir == null || !dir.d) return 1; //1;
		let file = fakedisk_getNode(disk, path, false);
		if (file != null) return 1; // 2;
		let name = fakedisk_getFileName(path);
		if (!fakedisk_isFileNameValid(name)) return 1; //3;
		let now = fakedisk_now();
		dir.f[name] = {
			d: true,
			c: now,
			m: now,
			f: {}
		};
		fakedisk_pushChanges(disk);
		return 0;
	};
	
	/*
		status codes:
			0 -> success
			1 -> directory not found
			2 -> access denied (tried to delete root)
			3 -> unknown error (file seemingly disappeared during the middle of the dir update)
	*/
	let fakedisk_rmdir = (disk, path) => {
		let dir = fakedisk_getNode(disk, path, false);
		if (dir == null || !dir.d) return 1;
		let parent = fakedisk_getNode(disk, path, true);
		if (parent == null) return 2;
		let name = fakedisk_getFileName(path);
		if (parent.f[name] === undefined) return 3;
		delete parent.f[name];
		// TODO: go through and unlink all affected file ID's
		fakedisk_pushChanges(disk);
	};
	
	/*
		status codes:
			0 -> success
			1 -> unknownn -- occurs when file name was invalid
			3 -> bad encoding
			4 -> parent directory not found
	*/
	let fakedisk_fileWrite = (disk, path, format, contentString, contentBytes) => {
		let isBytes = format == 0;
		if (format < 0 || format > 5) return 3;
		let content = isBytes ? contentBytes : contentString;
		let size = content.length;
		let encodedContent = content;
		if (isBytes) {
			let sb = [];
			let b;
			for (let i = 0; i < encodedContent.length; ++i) {
				b = encodedContent[i].toString(16);
				if (b.length == 1) b = '0' + b;
				sb.push(b);
			}
			encodedContent = sb.join('');
		}
		
		let dir = fakedisk_getNode(disk, path, true);
		if (dir == null || !dir.d) return 4;
		let file = fakedisk_getNode(disk, path, false);
		let name = fakedisk_getFileName(path);
		if (!fakedisk_isFileNameValid(name)) return 1;
		let id;
		let now = fakedisk_now();
		if (file == null) {
			// new file. get new ID. set created and modified times.
			let id = fakedisk_getNextId(disk);
			file = {
				d: false,
				i: id,
				c: now,
				m: now,
				b: isBytes,
				s: size
			};
		} else {
			// old file. use old ID. set modified time. don't change created time.
			id = file.i;
			file.m = now;
			file.s = size;
			file.b = isBytes;
		}
		dir.f[name] = file;
		disk.s[disk.p + 'F' + id] = encodedContent;
		fakedisk_pushChanges(disk);
		return 0;
	};
	
	let fakedisk_getNextId = (disk) => {
		let k = disk.p + 'nextId';
		let id = parseInt(disk.s[k]);
		if (!(id == id)) id = 1;
		disk.s[k] = '' + (id + 1);
		return id;
	};
	
	let fakedisk_removeBom = (s) => {
		return (s.length > 2 && s.charCodeAt(0) == 239 && s.charCodeAt(1) == 187 && s.charCodeAt(2) == 191)
			? s.substring(3)
			: s;
	};
	
	/*
		status codes:
			0 -> OK
			4 -> file not found
		
		if reading as text, set stringOut[0] as the contents
		if reading as bytes, add integerObjectCache[byteValue]'s to the them to bytesOut
	*/
	let fakedisk_fileRead = (disk, path, readAsBytes, stringOut, integerObjectCache, bytesOut) => {
		let file = fakedisk_getNode(disk, path, false);
		if (file == null) return 4;
		let content = disk.s[disk.p + 'F' + file.i];
		if (content === undefined) content = [];
		
		// is a string and read as string...
		if (!file.b && !readAsBytes) {
			stringOut[0] = fakedisk_removeBom(content);
			return 0;
		}
		
		// file is encoded as bytes
		if (file.b) {
			// get the bytes
			let sb = [];
			let b;
			for (let i = 0; i < content.length; i += 2) {
				b = parseInt(content[i], 16) * 16 + parseInt(content[i + 1], 16);
				if (b == b) {
					if (readAsBytes) {
						bytesOut.push(integerObjectCache[b]);
					} else {
						sb.push(String.fromCharCode(b));
					}
				} // drop invalid bytes
			}
			
			if (!readAsBytes) {
				stringOut[0] = fakedisk_removeBom(sb.join(''));
			}
			return 0;
		}
		
		// otherwise it's a string and we want bytes.
		let bytes = [];
		for (let i = 0; i < content.length; ++i) {
			bytesOut.push(integerObjectCache[content.charCodeAt(i)]);
		}
		return 0;
	};
	
	/*
		status codes
			0 -> OK
			1 -> unknown error -- occurs if there is a directory OR if topath is not a valid name
			4 -> from path doesn't exist or to directory doesn't exist or isn't a directory
			9 -> file exists and allowOverwrite isn't true
	*/
	let fakedisk_fileMove = (disk, fromPath, toPath, isCopy, allowOverwrite) => {
		let toName = fakedisk_getFileName(toPath);
		let fromName = fakedisk_getFileName(fromPath);
		if (!fakedisk_isFileNameValid(toName)) return 1;
		let file = fakedisk_getNode(disk, fromPath, false);
		if (file == null || file.d) return 4;
		let fromDir = fakedisk_getNode(disk, fromPath, true);
		let toDir = fakedisk_getNode(disk, toPath, true);
		if (toDir == null || !toDir.d) return 4;
		let toFile = fakedisk_getNode(disk, toPath, false);
		let isOverwrite = toFile != null;
		let deletedId = null;
		if (isOverwrite) {
			if (toFile.d) return 1;
			if (!allowOverwrite) return 9;
			disk.d.push(toFile.i);
		}
		
		let deleteFrom = true;
		let newFile = file;
		if (isCopy) {
			newFile = {
				d: false,
				c: file.c,
				m: file.m,
				i:  fakedisk_getNextId(disk),
				s: file.s,
				b: file.b
			};
			disk.s[disk.p + 'F' + newFile.i] = disk.s[disk.p + 'F' + file.i];
			deleteFrom = false;
		}
		
		toDir.f[toName] = newFile;
		if (deleteFrom && fromDir.f[fromname] !== undefined) {
			delete fromDir.f[fromName];
		}
		
		fakedisk_pushChanges(disk);
		return 0;
	};
	
	// Called safely (won't try to traverse above root)
	let fakedisk_getPathParent = (path, stringOut) => {
		parts = fakedisk_getNormalizedPath(path);
		parts.pop();
		stringOut[0] = '/' + parts.join('/');
		return 0;
	};
	
	/*
		status codes:
			0 -> OK
			1 -> unknown error -- occurs if for some odd reason the file is found but the parent isn't
			4 -> not found
	*/
	let fakedisk_fileDelete = (disk, path) => {
		let file = fakedisk_getNode(disk, path, false);
		if (file == null || file.d) return 4;
		let parent = fakedisk_getNode(disk, path, true);
		if (parent == null || !parent.d) return 1; // not sure how that would happen
		let name = fakedisk_getFileName(path);
		
		if (parent.f[name] !== undefined) {
			delete parent.f[name];
		}
		
		let key = disk.p + 'F' + file.i;
		
		if (disk.s[key] !== undefined) {
			if (disk.u) disk.s.removedItem(key);
			else { delete disk.s[key]; }
			disk.d.push(file.i);
		}
		return 0;
	};
	
	/*
		[
			exists?,
			is directory?,
			file size?
			created timestamp?
			modified timestamp?
		]
	*/
	fakedisk_pathInfo = (disk, path) => {
		let item = fakedisk_getNode(disk, path, false);
		if (item == null) return [false, false, 0, 0, 0];
		return [true, item.d, item.s, item.c, item.m];
	};
	
	let fakedisk_dirExists = (disk, path) => {
		return fakedisk_pathInfo(disk, path)[1];
	};
	
	let fakedisk_getPathInfoExt = (disk, path, mask, intOut, floatOut) => {
		let info = fakedisk_pathInfo(disk, path)
		intOut[0] = info[0] ? 1 : 0;
		intOut[1] = info[1] ? 1 : 0;
		if (info[0] && !info[1]) {
			intOut[2] = info[2];
		}
		intOut[3] = 0;
		floatOut[0] = info[3] / 1000;
		floatOut[1] = info[4] / 1000;
	};
	
	let envIsMobile = (() => {
		let mob = null;
		let check = () => {
			let ua = navigator.userAgent;
			for (let v of 'Android,iPhone,iPod,iPad,iPod,Nintendo'.split(',')) {
				if (ua.includes(v)) return 1;
			}
			return { BlackBerry: 1, webOS: 1, PSP: 1 }[navigator.platform];
		};
		return () => {
			if (mob === null) mob = !!check();
			return mob;
		};
	})();
	
	let setUrlPath = (path) => { history.pushState(path, '', path); };
	let getUrlPath = () => document.location.pathname;
	let launchBrowser = (url, fr, name) => {
		window.open(url, ['_parent', '_blank', '_self', '_top', name][fr - 1])
	};
	
	return {
		environment: {
			descriptor: 'web|javascript',
			isMobile: false,
			fullscreen: false,
			aspectRatio: null,
			isAndroid: false,
			isIos: false,
		},
		alwaysTrue,
		alwaysFalse,
		setUrlPath,
		getUrlPath,
		launchBrowser,
		timedCallback,
		print, // TODO: this is not actually used. Pastel uses console.log directly. Use this instead so it can be overwritten.

		textEncoding,
		httpSend,
		interop,
		dateTime,
		res,
		imageUtil,

		setRuntime: (rt) => { runtime = rt; },

		fakeDisk: {
			create: fakedisk_create,
			listdir: fakedisk_listdir,
			movedir: fakedisk_movedir,
			mkdir: fakedisk_mkdir,
			rmdir: fakedisk_rmdir,
			fileWrite: fakedisk_fileWrite,
			getPathInfoExt: fakedisk_getPathInfoExt,
			fileRead: fakedisk_fileRead,
			fileMove: fakedisk_fileMove,
			getPathParent: fakedisk_getPathParent,
			fileDelete: fakedisk_fileDelete,
			dirExists: fakedisk_dirExists,
		},
	};
})();
