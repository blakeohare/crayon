var ctx = {
	noriHostDiv: null,
	uiRoot: null,
	rootElementId: null,
	rootElement: null,
	elementById: {},
	frameSize: [0, 0],
	textSizer: null,
	
	// some static data:
	isTextProperty: {
		// Note: color properties count as text properties.
		'btn.text': true,
		'txtblk.text': true,
		'border.leftcolor': true,
		'border.topcolor': true,
		'border.rightcolor': true,
		'border.bottomcolor': true,
		'el.bgcolor': true,
		'img.src': true,
		'input.value': true
	},
	isPanelType: {
		'Border': true,
		'DockPanel': true,
		'FlowPanel': true,
		'ScrollPanel': true,
		'StackPanel': true
	},
	scrollEnumLookup: ['none', 'auto', 'scroll', 'crop'],
	scrollToCss: {
		none: 'visible',
		auto: 'auto',
		scroll: 'scroll',
		crop: 'hidden'
	},
	childrenLayoutFnByType: {}
};

function setFrameRoot(root) {
	ctx.noriHostDiv = root;
	let sizer = document.createElement('div');
	let content = document.createElement('div');
	ctx.uiRoot = content;
	ctx.textSizer = sizer;
	sizer.style.position = 'absolute';
	sizer.style.width = 'auto';
	sizer.style.height = 'auto';
	sizer.style.whiteSpace = 'nowrap';
	sizer.style.textAlign = 'left';
	sizer.style.fontFamily = 'Arial,sans-serif';
	content.style.position = 'absolute';
	content.style.width = '100%';
	content.style.height = '100%';
	root.appendChild(sizer);
	root.appendChild(content);
}

function setFrameSize(width, height) {
	ctx.frameSize = [width, height];
	var s = ctx.uiRoot.style;
	s.width = width + 'px';
	s.height = height + 'px';
	s = ctx.noriHostDiv.style;
	s.width = width + 'px';
	s.height = height + 'px';
}

function createElement(id, type) {
	var wrapper = document.createElement('div');
	wrapper.NORI_id = id;
	wrapper.NORI_type = type;
	wrapper.NORI_childrenIdList = [];
	wrapper.NORI_isPanel = !!ctx.isPanelType[type];
	
	wrapper.NORI_requiredSize = [0, 0];
	wrapper.NORI_allocatedSize = [0, 0];
	
	// The following properties are set when the corresponding property is set
	// and are faster lookups than the string keys in NORI_properties.
	// The layout engine is the computational bottleneck.
	wrapper.NORI_size = [null, null]; // width and height, if set
	wrapper.NORI_margin = [0, 0, 0, 0]; // [left, top, right, bottom] margin properties, if set
	wrapper.NORI_align = ['L', 'T']; // [{ L | C | R | S }, { T | C | B | S }]
	wrapper.NORI_dock = 'N'; // { N | S | E | W }
	
	var inner;
	var s;
	if (wrapper.NORI_isPanel) {
		
		inner = document.createElement('div');
		s = inner.style;
		s.width = '100%';
		s.height = '100%';
		s.position = 'relative';
		
		if (type == 'Border') wrapper.NORI_borders = [0, 0, 0, 0];
		wrapper.NORI_applyChildrenLayout = ctx.childrenLayoutFnByType[type];
	}
	
	wrapper.style.position = 'absolute';
	
	switch (type) {
		case 'Button':
			inner = document.createElement('button');
			s = inner.style;
			s.width = '100%';
			s.height = '100%';
			inner.innerHTML = 'Button';
			break;
		
		case 'Canvas':
		case 'Image':
			inner = document.createElement('canvas');
			s = inner.style;
			s.width = '100%';
			s.height = '100%';
			break;
		
		case 'TextBlock':
			inner = document.createElement('div');
			s = inner.style;
			s.width = '100%';
			s.height = '100%';
			s.textAlign = 'left';
			s.fontFamily = 'Arial,sans-serif';
			break;
		
		case 'PasswordBox':
		case 'TextBox':
			inner = document.createElement('input');
			inner.type = type == 'TextBox' ? 'text' : 'password';
			s = inner.style;
			s.width = '100%';
			s.height = '100%';
			break;
		
		case 'CheckBox':
			inner = document.createElement('input');
			inner.type = 'checkbox';
			s = inner.style;
			s.width = '100%';
			s.height = '100%';
			s.margin = '0px';
			s.padding = '0px';
			break;
		
		case 'ScrollPanel':
			wrapper.NORI_scrollpanel = ['none', 'none'];
			break;
			
		case 'Border':
		case 'DockPanel':
		case 'FlowPanel':
		case 'StackPanel':
			break;
		
		default:
			throw "Element creator not defined for " + type;
	}
	
	wrapper.appendChild(inner);
	
	return wrapper;
}

function noopFn() { }

function buildEventHandler(value, e, eventName, args) {
	if (value === 0) return noopFn;
	
	return function() {
		platformSpecificHandleEvent(e.NORI_id, eventName, args);
	};
}

function setProperty(e, key, value) {
	switch (key) {
		case 'el.width': e.NORI_size[0] = value; break;
		case 'el.height': e.NORI_size[1] = value; break;
		case 'el.halign': e.NORI_align[0] = 'SLCR'.charAt(value); break;
		case 'el.valign': e.NORI_align[1] = 'STCB'.charAt(value); break;
		case 'el.margin': for (var i = 0; i < 4; ++i) e.NORI_margin[i] = value; break;
		case 'el.leftmargin': e.NORI_margin[0] = value; break;
		case 'el.topmargin': e.NORI_margin[1] = value; break;
		case 'el.rightmargin': e.NORI_margin[2] = value; break;
		case 'el.bottommargin': e.NORI_margin[3] = value; break;
		case 'el.dock': e.NORI_dock = 'WNES'.charAt(value); break;
		
		case 'el.bgcolor':
			switch (e.NORI_type) {
				case 'Button':
					e.firstChild.style.backgroundColor = value;
					break;
				default:
					e.style.backgroundColor = value;
					break;
			}
			break;
		
		case 'border.leftcolor': e.firstChild.style.borderLeftColor = value; break;
		case 'border.topcolor': e.firstChild.style.borderTopColor = value; break;
		case 'border.rightcolor': e.firstChild.style.borderRightColor = value; break;
		case 'border.bottomcolor': e.firstChild.style.borderBottomColor = value; break;
		case 'border.leftthickness': e.NORI_borders[0] = value; e.firstChild.style.borderLeftThickness = value + 'px'; e.firstChild.style.borderLeftStyle = value > 0 ? 'solid' : 'none'; break;
		case 'border.topthickness': e.NORI_borders[1] = value; e.firstChild.style.borderTopThickness = value + 'px'; e.firstChild.style.borderTopStyle = value > 0 ? 'solid' : 'none'; break;
		case 'border.rightthickness': e.NORI_borders[2] = value; e.firstChild.style.borderRightThickness = value + 'px'; e.firstChild.style.borderRightStyle = value > 0 ? 'solid' : 'none'; break;
		case 'border.bottomthickness': e.NORI_borders[3] = value; e.firstChild.style.borderBottomThickness = value + 'px'; e.firstChild.style.borderBottomStyle = value > 0 ? 'solid' : 'none'; break;
		
		case 'cv.height': e.firstChild.height = value; break;
		case 'cv.width': e.firstChild.width = value; break;

		case 'btn.text': e.firstChild.innerHTML = escapeHtml(value); break;
		case 'btn.onclick': e.firstChild.onclick = buildEventHandler(value, e, key, ''); break;
		
		case 'txtblk.text': e.firstChild.innerHTML = escapeHtml(value); break;
		case 'txtblk.wrap': e.NORI_wrap = value === 1; break;
		case 'txtblk.sz': e.firstChild.style.fontSize = (value / 1000) + 'pt'; break;
		
		case 'scroll.x': e.NORI_scrollpanel[0] = ctx.scrollEnumLookup[value]; break;
		case 'scroll.y': e.NORI_scrollpanel[1] = ctx.scrollEnumLookup[value]; break;
		
		case 'img.src': setImageSource(e, value); break;
		
		case 'input.value':
			switch (e.NORI_type) {
				case 'PasswordBox':
				case 'TextBox':
				case 'CheckBox':
					if (e.firstChild.value != value) {
						e.firstChild.value = value;
					}
					break;
					
				default:
					throw "value setter not defined: " + e.NORI_type;
			}
			break;
		
		case 'input.changed':
		
			var eh;
			if (value === 0) eh = noopFn;
			else {
				eh = function() {
					var input = e.firstChild;
					var inputValue;
					switch (e.NORI_type) {
						case 'PasswordBox':
						case 'TextBox':
							inputValue = input.value;
							break;
						case 'CheckBox':
							inputValue = input.checked ? '1' : '';
							break;
						default:
							throw "unknown: " + e.NORI_type;
					}
					if (!inputValue) inputValue = '';
					if (e.NORI_prev_input_value !== inputValue) {
						e.NORI_prev_input_value = inputValue;
						platformSpecificHandleEvent(e.NORI_id, 'input.changed', inputValue);
					}
				};
			}
			switch (e.NORI_type) {
				case 'PasswordBox':
				case 'TextBox':
					e.firstChild.onchange = eh;
					e.firstChild.oninput = eh;
					break;
					
				case 'CheckBox':
					e.firstChild.onchange = eh;
					e.firstChild.onclick = eh;
					break;
					
				default:
					throw e.NORI_type;
			}
			break;
		
		default:
			throw "property setter not implemented: " + key;
	}
}

function unsetProperty(e, key) {
	// Note that there are a lot of missing properties from this switch.
	// That is because they cannot be unset, as they have some default value
	// and the VM code will only set explicit values by sending the default
	// value when the user intention is to unset.
	switch (key) {
		case 'el.width': e.NORI_size[0] = null; break;
		case 'el.height': e.NORI_size[1] = null; break;
		
		default:
			throw "property un-setter not implemented: " + key;
	}
}

function setImageSource(e, rawValue) {
	var id = parseInt(rawValue.split(':')[1]);
	var img = getImageInHoldingArea(id);
	var domCanvas = e.firstChild;
	domCanvas.width = img.width;
	domCanvas.height = img.height;
	// Some platforms cannot transmit the image data synchronously.
	// However, in those cases, the canvas width and height will be set properly so layout can occur.
	if (img.NORI_canvas_needs_loading) {
		img.NORI_canvas_load_callback_here = function () {
			var ctxA = domCanvas.getContext('2d');
			ctxA.drawImage(img, 0, 0);
		};
	} else {
		var ctx = domCanvas.getContext('2d');
		ctx.drawImage(img, 0, 0);
	}
}

function syncChildIds(e, childIds, startIndex, endIndex) {
	var host = e.firstChild;
	var length = host.children.length;
	var i;
	var id;
	var newId;
	if (length == 0) {
		for (i = startIndex; i < endIndex; ++i) {
			id = childIds[i];
			appendChild(e, id);
			e.NORI_childrenIdList.push(id);
		}
	} else {
		var preExistingIds = {};
		for (i = 0; i < length; ++i) {
			preExistingIds[host.children[i].NORI_id] = true;
		}
		var j = 0; // walks along the DOM
		var preExistingElement;
		var preExistingElementId;
		e.NORI_childrenIdList = [];
		var childIndex = 0;
		var childrenLength = endIndex - startIndex;
		for (i = 0; i < childrenLength; ++i) {
			childIndex = startIndex + i;
			newId = childIds[childIndex];
			if (j == host.children.length) {
				// If you get to the end of the pre-existing DOM elements, then everything
				// else in the new children list is new. Just append them to the end.
				appendChild(e, childIds[childIndex]);
				j++;
				e.NORI_childrenIdList.push(newId);
			} else {
				preExistingElement = host.children[j];
				preExistingElementId = preExistingElement.NORI_id;
				if (newId == preExistingElementId) {
					// element is the same, move on.
					j++;
					e.NORI_childrenIdList.push(newId);
				} else if (preExistingIds[newId]) {
					// The next element that needs to be here is already in the DOM.
					// That means the pre-existing element you have here has been removed.
					// Don't increment j.
					// Decrement i so that you can try inserting it again without this
					// removed element in the way.
					host.removeChild(preExistingElement);
					gcElement(preExistingElement);
					i--;
				} else {
					// The new element ID is not in the DOM yet so it needs to be inserted.
					host.insertBefore(ctx.elementById[newId], preExistingElement);
					j++;
					e.NORI_childrenIdList.push(newId);
				}
			}
		}
	}
}

function appendChild(e, childId) {
	var host = e.firstChild;
	host.appendChild(ctx.elementById[childId]);
	e.NORI_childrenIdList.push(childId);
}

function removeChildrenFromEnd(e, n) {
	var host = e.firstChild;
	while (n --> 0) {
		var removedElement = host.lastChild;
		host.removeChild(removedElement);
		gcElement(removedElement);
		e.NORI_childrenIdList.pop();
	}
}

function gcElement(e) {
	var lookup = ctx.elementById;
	delete lookup[e.NORI_id];
	var childIds = e.NORI_childrenIdList;
	for (var i = 0; i < childIds.length; ++i) {
		gcElement(ctx.elementById[childIds[i]]);
	}
}

function flushUpdates(data) {
	var items = data;
	var len = items.length;
	var instruction;
	var id;
	var type;
	var amount;
	var direction;
	var propertyCount;
	var propertyDeletionCount;
	var propertyKey;
	var propertyValue;
	var childrenCount;
	var childId;
	var j;
	var isTextProperty = ctx.isTextProperty;
	var elementById = ctx.elementById;
	var i = 0;
	while (i < len) {
		instruction = items[i++];
		id = parseInt(items[i++]);
		switch (instruction) {

			// noop
			case 'NO': break;
			
			case 'PF': // property full state
				type = items[i++];
				element = elementById[id];
				if (element === undefined) {
					element = createElement(id, type);
					elementById[id] = element;
				}
				
				propertyCount = parseInt(items[i++]);
				for (j = 0; j < propertyCount; ++j) {
					propertyKey = items[i + j];
					propertyValue = items[i + j + propertyCount];
					if (isTextProperty[propertyKey]) {
						propertyValue = decodeHex(propertyValue);
					} else {
						propertyValue = parseInt(propertyValue);
					}
					setProperty(element, propertyKey, propertyValue);
				}
				i += propertyCount * 2;
				break;
			
			case 'PI': // property incremental updates
				element = elementById[id];
				
				// delete properties
				propertyDeletionCount = parseInt(items[i++]);
				for (j = 0; j < propertyDeletionCount; ++j) {
					propertyKey = items[i + j];
					unsetProperty(element, propertyKey);
				}
				i += propertyDeletionCount;
				
				propertyCount = parseInt(items[i++]);
				for (j = 0; j < propertyCount; ++j) {
					propertyKey = items[i + j];
					propertyValue = items[i + j + propertyCount];
					if (isTextProperty[propertyKey]) {
						propertyValue = decodeHex(propertyValue);
					} else {
						propertyValue = parseInt(propertyValue);
					}
					setProperty(element, propertyKey, propertyValue);
				}
				i += propertyCount * 2;
				break;
			
			case 'CF': // children full state
				element = elementById[id];
				childrenCount = parseInt(items[i++]);
				for (j = 0; j < childrenCount; ++j) {
					items[i + j] = parseInt(items[i + j]);
				}
				syncChildIds(element, items, i, i + childrenCount);
				i += childrenCount;
				break;
			
			case 'CI': // children incremental updates
				element = elementById[id];
				
				// removals occur first
				childrenCount = parseInt(items[i++]);
				removeChildrenFromEnd(element, childrenCount);
				
				// followed by additions
				childrenCount = parseInt(items[i++]);
				for (j = 0; j < childrenCount; ++j) {
					childId = parseInt(items[i++]);
					appendChild(element, childId);
				}
				break;
			
			case 'RE': // Root element change
				
				ctx.rootElementId = id;
				ctx.rootElement = ctx.elementById[id];
				while (ctx.uiRoot.lastChild) {
					gcElement(ctx.uiRoot.lastChild);
					ctx.uiRoot.remove(ctx.uiRoot.lastChild);
				}
				ctx.uiRoot.appendChild(ctx.rootElement);
				break;
			
			case 'TO': // Queue a TimeOut
				queueTimeout(id, items[i++]);
				break;
			
			case 'SC': // Scroll an element
				element = elementById[id];
				type = items[i++];
				direction = items[i++]; // { W, N, E, S }
				amount = parseInt(items[i++]);
				if (element) {
					switch (type) {
						case 'E': // Scroll all the way to the 'E'nd in this direction
							switch (direction) {
								case 'W': element.scrollLeft = 0; break;
								case 'N': element.scrollTop = 0; break;
								case 'E': element.scrollLeft = element.scrollWidth; break;
								case 'S': element.scrollTop = element.scrollHeight; break;
							}
							break;
							
						case 'I': // 'I'ncremental. Scroll this amount in this direction
							
							break;
							
						case 'A': // 'A'bsolute. Scroll to this location.
							
							break;
							
						default:
							break;
					}
				}
				
				break;
			
			case 'CV':
				j = items[i++];
				element = elementById[id];
				handleCanvasData(element.firstChild, items, i, j);
				i += j;
				break;

			default:
				throw "Unknown command";
		}
	}
	
	doLayoutPass();
	platformSpecificHandleEvent(-1, 'on-render-pass', '');
}

let TO_HEX = (() => {
	let h = '0123456789abcdef'.split('');
	let arr = [];
	for (let a of h) {
		for (let b of h) {
			arr.push(a + b);
		}
	}
	return arr;
})();
let TO_HEX_HASH = TO_HEX.map(t => '#' + t);

function handleCanvasData(canvas, buffer, start, len) {
	let ctx = canvas.getContext('2d');
	let cvWidth = canvas.width;
	let cvHeight = canvas.height;
	let end = start + len;
	let r, g, b, a, w, h, x, y, lineWidth, x2, y2, hex;
	let i = start;
	while (i < end) {
		r = buffer[i + 1];
		g = buffer[i + 2];
		b = buffer[i + 3];
		hex = TO_HEX_HASH[r] + TO_HEX[g] + TO_HEX[b];
		switch (buffer[i]) {
			case 'F':
				i += 4;
				ctx.fillStyle = hex;
				ctx.fillRect(0, 0, cvWidth, cvHeight);
				break;
			
			case 'R':
				a = buffer[i + 4];
				x = buffer[i + 5];
				y = buffer[i + 6];
				w = buffer[i + 7];
				h = buffer[i + 8];
				i += 9;
				if (a !== 255) {
					ctx.globalAlpha = a / 255;
					ctx.fillStyle = hex;
					ctx.fillRect(x, y, w, h);
					ctx.globalAlpha = 1;
				} else {
					ctx.fillStyle = hex;
					ctx.fillRect(x, y, w, h);
				}
				break;
			
			case 'E':
				r = buffer[i + 1];
				g = buffer[i + 2];
				b = buffer[i + 3];
				a = buffer[i + 4];
				x = buffer[i + 5];
				y = buffer[i + 6];
				w = buffer[i + 7];
				h = buffer[i + 8];
				i += 9;
				// TODO: this
				break;
			
			case 'L':
				r = buffer[i + 1];
				g = buffer[i + 2];
				b = buffer[i + 3];
				a = buffer[i + 4];
				x = buffer[i + 5];
				y = buffer[i + 6];
				x2 = buffer[i + 7];
				y2 = buffer[i + 8];
				lineWidth = buffer[i + 9];
				i += 10;
				// TODO: this
				break;
			
			default:
				throw new Error("Unknown draw instruction: " + buffer[i]);
		}
	}
}

function queueTimeout(id, millis) {
	setTimeout(
		function() {
			platformSpecificHandleEvent(-1, 'on-timeout-callback', id);
		}, millis);
}

function doLayoutPass() {
	if (ctx.rootElementId === null) return;
	calculateRequiredSize(ctx.rootElement);
	var width = ctx.frameSize[0];
	var height = ctx.frameSize[1];
	spaceAllocation(ctx.rootElementId, 0, 0, width, height, 'S', 'S');
}

function spaceAllocation(
	elementId,
	xOffset, yOffset, // this includes the top-left margin
	usableWidth, usableHeight, // these already have margins taken out
	halign, valign) { // these contain either the original element's alignment or are overridden by the panel logic.
	
	var e = ctx.elementById[elementId];

	var x;
	var y;
	var width;
	var height;
	
	// Raw size value applied from bridge (could be pixels or negative percent notation)
	var originalWidth = e.NORI_size[0];
	var originalHeight = e.NORI_size[1];
	
	// minimum required explicit size.
	// Note that requiredSize is always non-0 and so pxWidth/Height gets set to null only in
	// the event of a percent setting.
	var pxWidth = e.NORI_requiredSize[0];
	var pxHeight = e.NORI_requiredSize[1];

	// 0 to 1 ratio of size consumption of available space
	var ratioX = null;
	var ratioY = null;
	
	// ratio of size consumption converted to final pixels
	var perWidth = null;
	var perHeight = null;
	if (originalWidth !== null && originalWidth < 0) {
		pxWidth = null;
		ratioX = originalWidth / -100000.0;
		perWidth = Math.floor(ratioX * usableWidth);
	}
	if (originalHeight !== null && originalHeight < 0) {
		pxHeight = null;
		ratioY = originalHeight / -100000.0;
		perHeight = Math.floor(ratioY * usableHeight);
	}
	var margin = e.NORI_margin;
	
	if (e.NORI_flexibleText) {
		var sz;
		var text = e.firstChild.innerHTML;
		if (e.NORI_flexibleText == '%') {
			var percentWidth = Math.floor(usableWidth * ratioX);
			sz = calculateTextSize(text, percentWidth);
			sz[0] = percentWidth; // even if you don't wrap and don't reach the end, this is the logical width of the element.
		} else {
			sz = calculateTextSize(text, usableWidth);
			// if it doesn't reach the end, don't use the full width.
		}
		
		// regardless of how the width was determined, treat the new measurement as required width.
		pxWidth = sz[0];
		if (pxHeight !== null) {
			pxHeight = sz[1];
			
		} else {
			
		}
		ratioX = null;
		
	}
	
	// calculated width and height if there is no stretch
	var calcWidth = pxWidth === null ? perWidth : pxWidth;
	var calcHeight = pxHeight === null ? perHeight : pxHeight;
	
	switch (halign) {
		case 'S':
			width = usableWidth;
			x = xOffset;
			break;
		case 'L':
			width = calcWidth;
			x = xOffset;
			break;
		case 'R':
			width = calcWidth;
			x = xOffset + usableWidth - width;
			break;
		case 'C':
			var centerX = xOffset + calcWidth / 2;
			width = calcWidth;
			x = Math.floor(centerX - width / 2);
			break;
	}
	
	switch (valign) {
		case 'S':
			height = usableHeight;
			y = yOffset;
			break;
		case 'T':
			height = calcHeight;
			y = yOffset;
			break;
		case 'B':
			height = calcHeight;
			y = yOffset + usableHeight - height;
			break;
		case 'C':
			var centerY = yOffset + calcHeight / 2;
			height = calcHeight;
			y = Math.floor(centerY - height / 2);
			break;
	}
	
	var s = e.style;
	s.width = width + 'px';
	s.height = height + 'px';
	s.left = x + 'px';
	s.top = y + 'px';
	
	e.NORI_allocatedSize[0] = width;
	e.NORI_allocatedSize[1] = height;
	
	if (e.NORI_isPanel) {
		
		if (e.NORI_borders) {
			var b = e.NORI_borders;
			width -= b[0] + b[2];
			height -= b[1] + b[3];
		}
		
		if (e.NORI_scrollpanel) {
			s.overflowX = ctx.scrollToCss[e.NORI_scrollpanel[0]];
			s.overflowY = ctx.scrollToCss[e.NORI_scrollpanel[1]];
		}
		
		e.NORI_applyChildrenLayout(e, 0, 0, width, height);
	}
}

function doDockPanelChildrenLayout(
	panel,
	xOffset,
	yOffset,
	availableWidth,
	availableHeight,
	overrideChildDock,
	useTransverseStretch) {
		
	var childrenIds = panel.NORI_childrenIdList;
	var length = childrenIds.length - 1; // the last one just takes what's left, regardless of dock direction.
	if (length == -1) return;
	var child;
	var dock;
	var margin;
	var reqSize;
	var t;
	var pixelsConsumed;
	var usableWidth;
	var usableHeight;
	for (var i = 0; i < length; ++i) {
		child = ctx.elementById[childrenIds[i]];
		margin = child.NORI_margin;
		reqSize = child.NORI_requiredSize;
		dock = overrideChildDock == null ? child.NORI_dock : overrideChildDock;
		usableWidth = availableWidth - margin[0] - margin[2];
		usableHeight = availableHeight - margin[1] - margin[3];
		switch (dock) {
			case 'N':
				spaceAllocation(
					child.NORI_id, 
					xOffset + margin[0], yOffset + margin[1],
					usableWidth, usableHeight,
					useTransverseStretch ? 'S' : child.NORI_align[0], 'T');
				pixelsConsumed = margin[1] + margin[3] + child.NORI_allocatedSize[1];
				yOffset += pixelsConsumed;
				availableHeight -= pixelsConsumed;
				break;
			
			case 'S':
				spaceAllocation(
					child.NORI_id,
					xOffset + margin[0], yOffset + margin[1],
					usableWidth, usableHeight,
					useTransverseStretch ? 'S' : child.NORI_align[0], 'B');
				pixelsConsumed = margin[1] + margin[3] + child.NORI_allocatedSize[1];
				availableHeight -= pixelsConsumed;
				break;
			
			case 'W':
				spaceAllocation(
					child.NORI_id,
					xOffset + margin[0], yOffset + margin[1],
					usableWidth, usableHeight,
					'L', useTransverseStretch ? 'S' : child.NORI_align[1]);
				pixelsConsumed = margin[0] + margin[2] + child.NORI_allocatedSize[0];
				xOffset += pixelsConsumed;
				availableWidth -= pixelsConsumed;
				break;
			
			case 'E':
				spaceAllocation(
					child.NORI_id,
					xOffset + margin[0], yOffset + margin[1],
					usableWidth, usableHeight,
					'R', useTransverseStretch ? 'S' : child.NORI_align[1]);
				pixelsConsumed = margin[0] + margin[2] + child.NORI_allocatedSize[0];
				availableWidth -= pixelsConsumed;
				break;
		}
	}
	child = ctx.elementById[childrenIds[length]];
	margin = child.NORI_margin;
	t = panel.NORI_type;
	var ha = useTransverseStretch ? 'S' : t == 'FlowPanel' ? 'L' : null;
	var va = useTransverseStretch ? 'S' : t == 'StackPanel' ? 'T' : null;
	
	if (panel.NORI_scrollpanel) {
		var xscroll = panel.NORI_scrollpanel[0];
		var yscroll = panel.NORI_scrollpanel[1];
		if (xscroll == 'none') ha = 'S';
		else ha = 'L';
		if (yscroll == 'none') va = 'S';
		else va = 'T';
	}
	usableWidth = availableWidth - margin[0] - margin[2];
	usableHeight = availableHeight - margin[1] - margin[3];

	if (ha === null) {
		ha = child.NORI_align[0];
		if (ha === null) ha = 'L';
	}
	if (va === null) {
		va = child.NORI_align[1];
		if (va === null) va = 'T';
	}

	spaceAllocation(
		child.NORI_id,
		xOffset + margin[0], yOffset + margin[1],
		usableWidth, usableHeight,
		ha, va);
}

var docLayoutFn = function(panel, xOffset, yOffset, availableWidth, availableHeight) {
	doDockPanelChildrenLayout(panel, xOffset, yOffset, availableWidth, availableHeight, null, true);
};
ctx.childrenLayoutFnByType['DockPanel'] = docLayoutFn;
ctx.childrenLayoutFnByType['Border'] = docLayoutFn;
ctx.childrenLayoutFnByType['ScrollPanel'] = docLayoutFn;

ctx.childrenLayoutFnByType['StackPanel'] = function(panel, xOffset, yOffset, availableWidth, availableHeight) {
	doDockPanelChildrenLayout(panel, xOffset, yOffset, availableWidth, availableHeight, 'N', false);
};
ctx.childrenLayoutFnByType['FlowPanel'] = function(panel, xOffset, yOffset, availableWidth, availableHeight) {
	doDockPanelChildrenLayout(panel, xOffset, yOffset, availableWidth, availableHeight, 'W', false);
};

function calculateRequiredSize(e) {
	if (e.NORI_isPanel) {
		var elementById = ctx.elementById;
		var children = e.NORI_childrenIdList;
		var child;
		var i;
		var id;
		var length = children.length;
		var xSize = 0;
		var ySize = 0;
		var childWidth;
		var childHeight;
		var t;
		for (i = length - 1; i >= 0; --i) {
			id = children[i];
			child = elementById[id];
			calculateRequiredSize(child);
			
			childWidth = child.NORI_requiredSize[0] + child.NORI_margin[0] + child.NORI_margin[2];
			childHeight = child.NORI_requiredSize[1] + child.NORI_margin[1] + child.NORI_margin[3];
			switch (e.NORI_type) {
				
				case 'Border':
					xSize = childWidth + e.NORI_borders[0] + e.NORI_borders[2];
					ySize = childHeight + e.NORI_borders[1] + e.NORI_borders[3];
					break;
				
				case 'DockPanel':
					if (i == length - 1) {
						xSize = childWidth;
						ySize = childHeight;
					} else {
						switch (child.NORI_dock) {
							case 'N':
							case 'S':
								xSize = Math.max(xSize, childWidth);
								ySize += childHeight;
								break;
							default:
								xSize += childWidth;
								ySize = Math.max(ySize, childHeight);
								break;
						}
					}
					break;
				
				case 'ScrollPanel':
					xSize = e.NORI_scrollpanel[0] == 'none' ? childWidth : 0;
					ySize = e.NORI_scrollpanel[1] == 'none' ? childHeight : 0;
					break;
				
				case 'StackPanel':
					xSize = Math.max(xSize, childWidth);
					ySize += childHeight;
					break;
				
				case 'FlowPanel':
					xSize += childWidth;
					ySize = Math.max(ySize, childHeight);
					break;
					
				default:
					throw "calculateRequiredSize not implemented for " + e.NORI_type;
			}
		}
		e.NORI_requiredSize[0] = (e.NORI_size[0] > xSize) ? e.NORI_size[0] : xSize;
		e.NORI_requiredSize[1] = (e.NORI_size[1] > ySize) ? e.NORI_size[1] : ySize;
	} else {
		var w = e.NORI_size[0];
		var h = e.NORI_size[1];
		var sz;
		switch (e.NORI_type) {
			case 'Button':
				if (w == null || h == null) {
					sz = calculateTextSize(e.firstChild.innerHTML, null);
					if (w == null) w = 15 + sz[0];
					if (h == null) h = 6 + sz[1];
				}
				break;
				
			case 'TextBlock':
				t = e.firstChild.innerHTML;
				e.NORI_flexibleText = false;
				if (!e.NORI_wrap) {
					sz = calculateTextSize(t, null);
				} else if (w != null) {
					if (w > 0) {
						sz = calculateTextSize(t, w);
					} else {
						// the element has a fixed width, but this is based on a percent
						// and can only be determined at spaceAllocation time.
						sz = [0, h == null ? 0 : h];
						e.NORI_flexibleText = '%';
					}
				} else {
					// The element has no fixed width. The width of the element ought to be
					// determined during the space allocation phase based on what's available,
					// but could possibly wrap if there's not enough space.
					sz = [0, h == null ? 0 : h];
					e.NORI_flexibleText = 'wrap';
				}
				if (w == null || sz[0] > w) w = sz[0];
				if (h == null || sz[1] > h) h = sz[1];
				break;
				
			default:
				if (w == null) w = 0;
				if (h == null) h = 0;
				break;
		}
		
		e.NORI_requiredSize[0] = w;
		e.NORI_requiredSize[1] = h;
	}
}

function calculateTextSize(html, width) {
	var sizer = ctx.textSizer;
	if (width === null) {
		sizer.style.whiteSpace = 'nowrap';
		sizer.style.width = 'auto';
	} else {
		sizer.style.whiteSpace = 'normal';
		sizer.style.width = width + 'px';
	}
	sizer.innerHTML = html;
	var sz = [Math.max(1, sizer.clientWidth), Math.max(1, sizer.clientHeight)];
	sizer.innerHTML = '';
	return sz;
}

function escapeHtml(text) {
	var o = [];
	var len = text.length;
	var c;
	for (var i = 0; i < len; ++i) {
		c = text.charAt(i);
		switch (c) {
			case '<': c ='&lt;'; break;
			case '>': c = '&gt;'; break;
			case '&': c = '&amp;'; break;
			case '"': c = '&quot;'; break;
		}
		o.push(c);
	}
	return o.join('');
}

var HEX_LOOKUP = {};
for (var i = 0; i < 10; ++i) {
	HEX_LOOKUP[i + ''] = i;
}
for (var i = 0; i < 6; ++i) {
	HEX_LOOKUP['abcdef'.charAt(i)] = i + 10;
	HEX_LOOKUP['ABCDEF'.charAt(i)] = i + 10;
}
function decodeHex(s) {
	var o = [];
	var len = s.length;
	var a;
	var b;
	for (var i = 0; i < len; i += 2) {
		a = HEX_LOOKUP[s.charAt(i)];
		b = HEX_LOOKUP[s.charAt(i + 1)];
		o.push(String.fromCharCode(a * 16 + b));
	}
	return o.join('');
}
