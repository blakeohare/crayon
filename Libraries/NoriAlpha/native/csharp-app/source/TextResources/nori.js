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
		'btn.text': true,
	},
	isPanelType: {
		'DockPanel': true,
		'StackPanel': true,
		'FlowPanel': true,
	},
	childrenLayoutFnByType: {},
};

function setFrameRoot(root) {
	ctx.noriHostDiv = root;
	sizer = document.createElement('div');
	content = document.createElement('div');
	ctx.uiRoot = content;
	ctx.textSizer = sizer;
	sizer.style.position = 'absolute';
	sizer.style.width = 'auto';
	sizer.style.height = 'auto';
	sizer.style.whiteSpace = 'nowrap';
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
	wrapper.NORI_align = ['S', 'S']; // [{ L | C | R | S }, { T | C | B | S }]
	wrapper.NORI_dock = 'N'; // { N | S | E | W }
	
	if (wrapper.NORI_isPanel) {
		wrapper.NORI_applyChildrenLayout = ctx.childrenLayoutFnByType[type];
	}
	
	wrapper.style.position = 'absolute';
	
	var inner;
	var s;
	switch (type) {
		case 'Button':
			inner = document.createElement('button');
			s = inner.style;
			s.width = '100%';
			s.height = '100%';
			inner.innerHTML = 'Button';
			break;
		
		case 'DockPanel':
			inner = document.createElement('div');
			s = inner.style;
			s.width = '100%';
			s.height = '100%';
			s.position = 'relative';
			break;
		
		default:
			throw "Element creator not defined for " + type;
	}
	
	wrapper.appendChild(inner);
	
	return wrapper;
}

function buildEventHandler(value, e, eventName, args) {
	if (value === 0) return undefined;
	
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
		
		case 'btn.text': e.firstChild.innerHTML = escapeHtml(value); break;
		case 'btn.onclick': e.firstChild.onclick = buildEventHandler(value, e, key, ''); break;
		
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

function syncChildIds(e, childIds, startIndex, endIndex) {
	var host = e.firstChild;
	var length = host.children.length;
	var i;
	var id;
	if (length == 0) {
		for (i = startIndex; i < endIndex; ++i) {
			id = childIds[i];
			appendChild(e, id);
			e.NORI_childrenIdList.push(id);
		}
	} else {
		var existingIds = {};
		for (i = 0; i < length; ++i) {
			existingIds[host.children[i].NORI_id] = true;
		}
		var j = 0;
		var existingElement;
		var existingElementId;
		e.NORI_childrenIdList = [];
		for (i = startIndex; i < endIndex; ++i) {
			id = childIds[i];
			e.NORI_childrenIdList.push(id);
			if (j == length) {
				// run off past of the end of the existing list? then append the element.
				appendChild(e, childIds[i]);
			} else {
				existingElement = host.children[j];
				existingElementId = existingElement.NORI_id;
				if (id == existingElementId) {
					// element is the same, move on.
					j++;
				} else {
					if (existingIds[existingElementId]) {
						// The element ID is already in the list (in the future)
						// so this means the new child ID needs to be inserted.
						// Don't increment j.
						host.insertBefore(ctx.elementById[id], existingElement);
					} else {
						// The element ID is not in the list so it needs to be deleted.
						// Don't increment j.
						host.remove(existingElement);
						gcElement(existingElement);
					}
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
		host.remove(removedElement);
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
	var items = data.split(',');
	var len = items.length;
	var instruction;
	var id;
	var type;
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
					items[i] = parseInt(items[i]);
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
				ctx.uiRoot.append(ctx.rootElement);
				break;
			
			default:
				throw "Unknown command";
		}
	}
	
	doLayoutPass();
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
	xOffset,
	yOffset,
	availableWidth,
	availableHeight,
	halignOverride,
	valignOverride) {
	
	var e = ctx.elementById[elementId];
	var halign = halignOverride == null ? e.NORI_align[0] : halignOverride;
	var valign = valignOverride == null ? e.NORI_align[1] : valignOverride;
	
	var x;
	var y;
	var width;
	var height;
	
	var reqWidth = e.NORI_requiredSize[0];
	var reqHeight = e.NORI_requiredSize[1];
	var margin = e.NORI_margin;
	
	switch (halign) {
		case 'S':
			width = availableWidth - margin[0] - margin[2];
			x = xOffset + margin[0];
			break;
		case 'L':
			width = availableWidth - margin[0];
			if (reqWidth < width) width = reqWidth;
			x = xOffset + margin[0];
			break;
		case 'R':
			width = availableWidth - margin[2];
			if (reqWidth < width) width = reqWidth;
			x = xOffset + availableWidth - margin[2] - width;
			break;
		case 'C':
			width = availableWidth - margin[0] - margin[2];
			var cx = xOffset + margin[0] + width / 2;
			if (reqWidth < width) width = reqWidth;
			x = Math.floor(cx - width / 2);
			break;
	}
	
	switch (valign) {
		case 'S':
			height = availableHeight - margin[1] - margin[3];
			y = yOffset + margin[1];
			break;
		case 'T':
			height = availableHeight - margin[1];
			if (reqHeight < height) height = reqHeight;
			y = yOffset + margin[1];
			break;
		case 'B':
			height = availableHeight - margin[3];
			if (reqHeight < height) height = reqHeight;
			y = yOffset + availableHeight - margin[3] - height;
			break;
		case 'C':
			height = availableHeight - margin[1] - margin[3];
			var cy = yOffset + margin[1] + width / 2;
			if (reqHeight < height) height = reqHeight;
			y = Math.floor(cy - height / 2);
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
		e.NORI_applyChildrenLayout(e, x, y, width, height);
	}
}

function doDockPanelChildrenLayout(panel, xOffset, yOffset, availableWidth, availableHeight) {
	var childrenIds = panel.NORI_childrenIdList;
	var length = childrenIds.length - 1; // the last one just takes what's left, regardless of dock direction.
	if (length == -1) return;
	var child;
	var dock;
	var margin;
	var reqSize;
	var t;
	var pixelsConsumed;
	for (var i = 0; i < length; ++i) {
		child = ctx.elementById[childrenIds[i]];
		margin = child.NORI_margin;
		reqSize = child.NORI_requiredSize;
		switch (child.NORI_dock) {
			case 'N':
				spaceAllocation(
					child.NORI_id, 
					xOffset + margin[0], yOffset + margin[1],
					availableWidth - margin[0] - margin[2], reqSize[1],
					'S', 'T');
				pixelsConsumed = margin[1] + margin[3] + child.NORI_allocatedSize[1];
				yOffset += pixelsConsumed;
				availableHeight -= pixelsConsumed;
				break;
			
			case 'S':
				spaceAllocation(
					child.NORI_id,
					xOffset + margin[0], yOffset + availableHeight - reqSize[1] - margin[3],
					availableWidth - margin[0] - margin[2], reqSize[1],
					'S', 'B');
				pixelsConsumed = margin[1] + margin[3] + child.NORI_allocatedSize[1];
				availableHeight -= pixelsConsumed;
				break;
			
			case 'W':
				spaceAllocation(
					child.NORI_id,
					xOffset + margin[0], yOffset + margin[1],
					reqSize[0], availableHeight - margin[1] - margin[3],
					'L', 'S');
				pixelsConsumed = margin[0] + margin[2] + child.NORI_allocatedSize[0];
				xOffset += pixelsConsumed;
				availableWidth -= pixelsConsumed;
				break;
			
			case 'E':
				spaceAllocation(
					child.NORI_id,
					xOffset + availableWidth - margin[2] - reqSize[0], yOffset + margin[1],
					reqSize[0], availableHeight - margin[0] - margin[2],
					'R', 'S');
				pixelsConsumed = margin[0] + margin[2] + child.NORI_allocatedSize[0];
				availableWidth -= pixelsConsumed;
				break;
		}
	}
	child = ctx.elementById[childrenIds[length]];
	margin = child.NORI_margin;
	spaceAllocation(
		child.NORI_id,
		xOffset + margin[0], yOffset + margin[1],
		availableWidth - margin[0] - margin[2], availableHeight - margin[1] - margin[3],
		'S', 'S');
}
ctx.childrenLayoutFnByType['DockPanel'] = doDockPanelChildrenLayout;

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
		for (i = length - 1; i >= 0; --i) {
			id = children[i];
			child = elementById[id];
			calculateRequiredSize(child);
			
			childWidth = child.NORI_requiredSize[0] + child.NORI_margin[0] + child.NORI_margin[2];
			childHeight = child.NORI_requiredSize[1] + child.NORI_margin[1] + child.NORI_margin[3];
			switch (e.NORI_type) {
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
				default:
					throw "calculateRequiredSize not implemented for " + e.NORI_type;
			}
		}
		e.NORI_requiredSize[0] = (e.NORI_size[0] > xSize) ? e.NORI_size[0] : xSize;
		e.NORI_requiredSize[1] = (e.NORI_size[1] > ySize) ? e.NORI_size[1] : ySize;
	} else {
		var w = e.NORI_size[0];
		var h = e.NORI_size[1];
		switch (e.NORI_type) {
			case 'Button':
				if (w == null || h == null) {
					var sz = calculateTextSize(e.firstChild.innerHTML);
					if (w == null) w = 15 + sz[0];
					if (h == null) h = 6 + sz[1];
				}
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

function calculateTextSize(html) {
	var sizer = ctx.textSizer;
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
