
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
	content.style.position = 'absolute';
	content.style.width = '100%';
	content.style.height = '100%';
	root.appendChild(sizer);
	root.appendChild(content);
}

function setFrameSize(width, height) {
	ctx.frameSize = [width, height];
	let s = ctx.uiRoot.style;
	s.width = width + 'px';
	s.height = height + 'px';
	s = ctx.noriHostDiv.style;
	s.width = width + 'px';
	s.height = height + 'px';
}

function createElement(id, type) {
	let wrapper = document.createElement('div');
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
	
	let inner;
	let s;
	if (wrapper.NORI_isPanel) {
		
		inner = document.createElement('div');
		s = inner.style;
		s.width = '100%';
		s.height = '100%';
		s.position = 'relative';
		
		if (type == 'Border') wrapper.NORI_borders = [0, 0, 0, 0];
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
			break;
		
		case 'PasswordBox':
		case 'TextBox':
			inner = document.createElement('input');
			inner.type = type == 'TextBox' ? 'text' : 'password';
			inner.spellcheck = false;
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

function setProperty(e, key, value) {
	let t;
	switch (key) {
		case 'el.width': e.NORI_size[0] = value; break;
		case 'el.height': e.NORI_size[1] = value; break;
		case 'el.halign': e.NORI_align[0] = 'SLCR'.charAt(value); break;
		case 'el.valign': e.NORI_align[1] = 'STCB'.charAt(value); break;
		case 'el.margin': for (let i = 0; i < 4; ++i) e.NORI_margin[i] = value; break;
		case 'el.leftmargin': e.NORI_margin[0] = value; break;
		case 'el.topmargin': e.NORI_margin[1] = value; break;
		case 'el.rightmargin': e.NORI_margin[2] = value; break;
		case 'el.bottommargin': e.NORI_margin[3] = value; break;
		case 'el.dock': e.NORI_dock = 'WNES'.charAt(value); break;

		case 'el.onmousedown':
		case 'el.onmouseup':
		case 'el.onmousemove':
			// TODO: this may be inaccurate for certain types of elements. Need to thoroughly test this
			// on various element types.
			e.firstChild[key.split('.')[1]] = NoriEvents.buildEventHandler(value, e, key, ev => {
				let r = e.getBoundingClientRect();
				let x = ev.clientX - r.left;
				let y = ev.clientY - r.top;
				let btn = '';
				if (key !== 'el.onmousemove') {
					switch (ev.button) {
						case 0: btn = 'primary'; break;
						case 1: btn = 'aux'; break;
						case 2: btn = 'secondary'; break;
						default: btn = '?'; break;
					}
				}
				return x + '|' + y + '|' + (x / r.width) + '|' + (y / r.height) + '|' + btn;
			});
			break;
		
		case 'el.bold': 
			e.style.fontWeight = value ? 'bold' : 'normal';
			e.NORI_font = e.NORI_font || {};
			e.NORI_font.bold = e.style.fontWeight;
			break;
		case 'el.italic': 
			e.style.fontStyle = value ? 'italic' : 'normal';
			e.NORI_font = e.NORI_font || {};
			e.NORI_font.italic = e.style.fontStyle;
			break;
		case 'el.fontcolor': 
			e.firstChild.style.color = value;
			e.NORI_font = e.NORI_font || {};
			e.NORI_font.color = value;
			break;
		case 'el.font':
			t = '"' + value + '",sans-serif';
			e.firstChild.style.fontFamily = t;
			e.NORI_font = e.NORI_font || {};
			e.NORI_font.face = t;
			break;
		case 'txtblk.sz':
			t = (value / 1000) + 'pt';
			e.firstChild.style.fontSize = t;
			e.NORI_font = e.NORI_font || {};
			e.NORI_font.size = t;
			break;

		case 'el.bgcolor':
			switch (e.NORI_type) {
				case 'Button':
				case 'TextBox':
				case 'PasswordBox':
					e.firstChild.style.backgroundColor = value;
					break;
				default:
					e.style.backgroundColor = value;
					break;
			}
			break;
		
		case 'border.radius': e.style.borderRadius = value + 'px'; e.firstChild.style.borderRadius = value + 'px'; break;
		case 'border.leftcolor': e.firstChild.style.borderLeftColor = value; break;
		case 'border.topcolor': e.firstChild.style.borderTopColor = value; break;
		case 'border.rightcolor': e.firstChild.style.borderRightColor = value; break;
		case 'border.bottomcolor': e.firstChild.style.borderBottomColor = value; break;
		case 'border.leftthickness': e.NORI_borders[0] = value; e.firstChild.style.borderLeftWidth = value + 'px'; e.firstChild.style.borderLeftStyle = value > 0 ? 'solid' : 'none'; break;
		case 'border.topthickness': e.NORI_borders[1] = value; e.firstChild.style.borderTopWidth = value + 'px'; e.firstChild.style.borderTopStyle = value > 0 ? 'solid' : 'none'; break;
		case 'border.rightthickness': e.NORI_borders[2] = value; e.firstChild.style.borderRightWidth = value + 'px'; e.firstChild.style.borderRightStyle = value > 0 ? 'solid' : 'none'; break;
		case 'border.bottomthickness': e.NORI_borders[3] = value; e.firstChild.style.borderBottomWidth = value + 'px'; e.firstChild.style.borderBottomStyle = value > 0 ? 'solid' : 'none'; break;
		
		case 'cv.height': e.firstChild.height = value; break;
		case 'cv.width': e.firstChild.width = value; break;

		case 'btn.text': e.firstChild.innerHTML = NoriUtil.escapeHtml(value); break;
		case 'btn.onclick': e.firstChild.onclick = NoriEvents.buildEventHandler(value, e, key, ''); break;
		
		case 'txtblk.text': e.firstChild.innerHTML = NoriUtil.escapeHtml(value); break;
		case 'txtblk.wrap': e.NORI_wrap = value === 1; break;
		
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
		
		case 'tb.border':
			e.firstChild.style.borderWidth = value ? '' : '0px';
			break;

		case 'input.onfocus':
		case 'input.onblur':
			e.firstChild[key == 'input.onfocus' ? 'onfocus' : 'onblur'] = NoriEvents.buildEventHandler(value, e, key, '');
			break;

		case 'input.changed':
		
			let eh;
			if (value === 0) eh = NoriUtil.noopFn;
			else {
				eh = function() {
					let input = e.firstChild;
					let inputValue;
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
	let canvas = e.firstChild;
	let imgObj = new Image();
	imgObj.onload = () => {
		canvas.width = imgObj.width;
		canvas.height = imgObj.height;
		let ctx = canvas.getContext('2d');
		ctx.drawImage(imgObj, 0, 0);
	};
	imgObj.src = "data:image/png;base64," + rawValue;
}

function syncChildIds(e, childIds, startIndex, endIndex) {
	let host = e.firstChild;
	let length = host.children.length;
	let i;
	let id;
	let newId;
	if (length == 0) {
		for (i = startIndex; i < endIndex; ++i) {
			id = childIds[i];
			appendChild(e, id);
			e.NORI_childrenIdList.push(id);
		}
	} else {
		let preExistingIds = {};
		for (i = 0; i < length; ++i) {
			preExistingIds[host.children[i].NORI_id] = true;
		}
		let j = 0; // walks along the DOM
		let preExistingElement;
		let preExistingElementId;
		e.NORI_childrenIdList = [];
		let childIndex = 0;
		let childrenLength = endIndex - startIndex;
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
	let host = e.firstChild;
	host.appendChild(ctx.elementById[childId]);
	e.NORI_childrenIdList.push(childId);
}

function removeChildrenFromEnd(e, n) {
	let host = e.firstChild;
	while (n --> 0) {
		let removedElement = host.lastChild;
		host.removeChild(removedElement);
		gcElement(removedElement);
		e.NORI_childrenIdList.pop();
	}
}

function gcElement(e) {
	let lookup = ctx.elementById;
	delete lookup[e.NORI_id];
	let childIds = e.NORI_childrenIdList;
	for (let i = 0; i < childIds.length; ++i) {
		gcElement(ctx.elementById[childIds[i]]);
	}
}

function flushUpdates(data) {
	let items = data;
	let len = items.length;
	let instruction;
	let id;
	let type;
	let amount;
	let direction;
	let propertyCount;
	let propertyDeletionCount;
	let propertyKey;
	let propertyValue;
	let childrenCount;
	let childId;
	let j;
	let isTextProperty = ctx.isTextProperty;
	let elementById = ctx.elementById;
	let i = 0;

	let performLayoutPass = false;

	while (i < len) {
		instruction = items[i++];
		id = parseInt(items[i++]);
		switch (instruction) {

			// noop
			case 'NO': break;

			case 'FA': // Frame attribute
				propertyKey = items[i++];
				propertyValue = items[i++];
				switch (propertyKey) {
					case 'frame.keydown':
						ctx.hasKeyDownListener = true;
						break;
					case 'frame.keyup':
						ctx.hasKeyUpListener = true;
						break;
				}
				break;
			
			case 'FR': // Font resource
				propertyValue = items[i++];
				document.getElementById('font_loader').innerHTML += "\n@import url('" + propertyValue + "');"
				break;

			case 'PF': // property full state
				performLayoutPass = true;
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
						propertyValue = NoriUtil.decodeHex(propertyValue);
					} else {
						propertyValue = parseInt(propertyValue);
					}
					setProperty(element, propertyKey, propertyValue);
				}
				i += propertyCount * 2;
				break;
			
			case 'PI': // property incremental updates
				performLayoutPass = true;
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
						propertyValue = NoriUtil.decodeHex(propertyValue);
					} else {
						propertyValue = parseInt(propertyValue);
					}
					setProperty(element, propertyKey, propertyValue);
				}
				i += propertyCount * 2;
				break;
			
			case 'CF': // children full state
				performLayoutPass = true;
				element = elementById[id];
				childrenCount = parseInt(items[i++]);
				for (j = 0; j < childrenCount; ++j) {
					items[i + j] = parseInt(items[i + j]);
				}
				syncChildIds(element, items, i, i + childrenCount);
				i += childrenCount;
				break;
			
			case 'CI': // children incremental updates
				performLayoutPass = true;
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
				performLayoutPass = true;
				
				ctx.rootElementId = id;
				let re = ctx.elementById[id];
				let f = {
					bold: 'normal',
					italic: 'normal',
					size: '10pt',
					face: '"Arial",sans-serif',
				};
				ctx.rootElement = re;
				re.NORI_font = f;
				re.style.fontWeight = f.bold;
				re.style.fontStyle = f.italic;
				re.style.fontFamily = f.face;
				re.style.fontSize = f.size;

				while (ctx.uiRoot.lastChild) {
					gcElement(ctx.uiRoot.lastChild);
					ctx.uiRoot.remove(ctx.uiRoot.lastChild);
				}
				ctx.uiRoot.appendChild(ctx.rootElement);
				break;
			
			case 'TO': // Queue a TimeOut
				let toId = id;
				let millis = items[i++];
				setTimeout(
					function() {
						platformSpecificHandleEvent(-1, 'on-timeout-callback', toId);
					}, millis);
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
				NoriCanvas.handleCanvasData(element.firstChild, items, i, j);
				i += j;
				break;

			case 'GS': // declare a canvas as a game surface.
				element = elementById[id];
				NoriEvents.enableBatchMode();
				(canvas => {
					let handleMouseEvent = (type, ev) => {
						let rect = canvas.getBoundingClientRect();
						let px = ev.clientX - rect.left;
						let py = ev.clientY - rect.top;
						let x = Math.floor(canvas.firstChild.width * px / rect.width);
						let y = Math.floor(canvas.firstChild.height * py / rect.height);
						let btn = -1;
						NoriEvents.addBatchEvent(type, [x, y, btn]);
					};
					canvas.addEventListener('mousemove', e => { handleMouseEvent('mousemove', e); });
					canvas.addEventListener('mousedown', e => { handleMouseEvent('mousedown', e); });
					canvas.addEventListener('mouseup', e => { handleMouseEvent('mouseup', e); });
				})(element);
				break;

			case 'FE': // flush events
				NoriEvents.flushEventBatches();
				break;

			default:
				throw "Unknown command";
		}
	}
	
	if (performLayoutPass) {
		NoriLayout.doLayoutPass();
		platformSpecificHandleEvent(-1, 'on-render-pass', '');
	}
}
