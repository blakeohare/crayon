function noriInit(noriRoot, uiData, shownCallback, eventBatchSender) {
    NoriEvents.setBatchMessageSender(eventBatchSender);
    setFrameRoot(noriRoot);
    let sz = getWindowSize();
    setFrameSize(sz[0], sz[1]);
    window.addEventListener('resize', () => {
        let newSize = getWindowSize();
        setFrameSize(newSize[0], newSize[1]);
        platformSpecificHandleEvent(-1, 'frame.onresize', newSize[0] + ',' + newSize[1]);
        flushUpdates(['NO', 0]);
    });
    let fontUpdates = [];
    let otherUpdates = [];
    for (let i = 0; i < uiData.length; ++i) {
        if (uiData[i] === 'FR') {
            for (let j = 0; j < 4; ++j) {
                fontUpdates.push(uiData[i + j]);
            }
            i += 3;
        } else {
            otherUpdates = uiData.slice(i);
            break;
        }
    }
    uiData = otherUpdates;

    let finishInit = () => {
        flushUpdates(uiData);
        shownCallback();
    };

    if (fontUpdates.length === 0) {
        finishInit();
    } else {
        flushUpdates(fontUpdates);
        waitForFonts().then(finishInit);
    }
}

function setFrameRoot(root) {
    ctx.noriHostDiv = root;
    let sizer = document.createElement('div');
    let fontLoader = document.createElement('style');
    let content = document.createElement('div');
    ctx.textSizer = sizer;
    ctx.fontLoader = fontLoader;
    ctx.uiRoot = content;
    sizer.style.position = 'absolute';
    sizer.style.width = 'auto';
    sizer.style.height = 'auto';
    sizer.style.whiteSpace = 'nowrap';
    sizer.style.textAlign = 'left';
    content.style.position = 'absolute';
    content.style.width = '100%';
    content.style.height = '100%';
    content.style.fontFamily = '"Arial", sans-serif;';
    content.style.fontSize = '12pt';
    root.appendChild(sizer);
    root.appendChild(fontLoader);
    root.appendChild(content);
    ctx.uiRoot.NORI_font = { size: '12pt', bold: 'normal', italic: 'normal', color: '#000', face: 'Arial' };

    root.addEventListener('click', () => {
        NoriAudio.interactionOccurred();
    });
}

function setFrameSize(width, height) {
    ctx.frameSize = [width, height];
    let s = ctx.uiRoot.style;
    s.width = width + 'px';
    s.height = height + 'px';
    s = ctx.noriHostDiv.style;
    s.width = width + 'px';
    s.height = height + 'px';
    NoriLayout.doLayoutPass();
}

function createElement(id, type) {
    let wrapper = document.createElement('div');
    wrapper.NORI_id = id;
    wrapper.NORI_type = type;
    wrapper.NORI_childrenIdList = [];
    wrapper.NORI_isPanel = !!ctx.isPanelType[type];

    wrapper.NORI_requiredSize = [0, 0];
    wrapper.NORI_allocatedSize = [0, 0];
    wrapper.NORI_handlers = { outer: {}, inner: {}};

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

        if (type == 'Border') {
            wrapper.NORI_borders = [0, 0, 0, 0];
        }

        if (type == 'ScrollPanel') {
            // nothing special
        } else if (type == 'FloatPanel') {
            wrapper.style.position = 'absolute';
            wrapper.style.width = '0px';
            wrapper.style.height = '0px';
            wrapper.NORI_floatAnchors = [null, null, null, null];
        } else {
            // s.overflow = 'hidden';
        }
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
            // Any properties set here must also be added to the multiline property setter, which toggles
            // between Textarea and input[type=text] elements for the inner element.
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
            wrapper.NORI_scrollHandlerChildren = [];
            wrapper.addEventListener('scroll', () => {
                let listeners = wrapper.NORI_scrollHandlerChildren;
                if (listeners.length > 0) {
                    let scrollRect = wrapper.getBoundingClientRect();
                    for (let item of listeners) {
                        let rect = item.getBoundingClientRect();
                        let isInView = !(scrollRect.left > rect.right ||
                            scrollRect.right < rect.left ||
                            scrollRect.top > rect.bottom ||
                            scrollRect.bottom < rect.top);
                        if (item.NORI_scrollHandlerTriggered !== isInView) {
                            item.NORI_scrollIntoViewHandler(isInView ? '1' : '0');
                            item.NORI_scrollHandlerTriggered = isInView;
                        }
                    }
                }
            });
            break;

        case 'Border':
        case 'DockPanel':
        case 'FlowPanel':
        case 'StackPanel':
        case 'FloatPanel':
            break;

        default:
            throw "Element creator not defined for " + type;
    }

    wrapper.appendChild(inner);

    return wrapper;
}

function updateShadow(e) {
    let shadow = e.NORI_shadow;
    let color = shadow.color;
    let blur = shadow.blur === undefined ? 0 : shadow.blur;
    let { x, y } = shadow;
    e.style.boxShadow = [
        x || 0, 'px ',
        y || 0, 'px ',
        blur, 'px ',
        'rgba(', color[0], ',', color[1], ',', color[2], ',', color[3] / 255, ')',
    ].join('');
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

        case 'el.shadowcolor':
            e.NORI_shadow = e.NORI_shadow || {};
            e.NORI_shadow.color = value.split(',').map(n => parseInt(n));
            updateShadow(e);
            break;
        case 'el.shadowblur':
            e.NORI_shadow = e.NORI_shadow || {};
            e.NORI_shadow.blur = value;
            updateShadow(e);
            break;
        case 'el.shadowx':
        case 'el.shadowy':
            e.NORI_shadow = e.NORI_shadow || {};
            e.NORI_shadow[key == 'el.shadowx' ? 'x' : 'y'] = value;
            updateShadow(e);
            break;

        case 'el.cursor':
            t = e.firstChild.style;
            switch (value) {
                case 0: t.cursor = 'auto'; break;
                case 1: t.cursor = 'pointer'; break;
                case 2: t.cursor = 'ew-resize'; break;
                case 3: t.cursor = 'ns-resize'; break;
                case 4: t.cursor = 'not-allowed'; break;
                case 5: t.cursor = 'grab'; break;
                case 6: t.cursor = 'text'; break;
                case 7: t.cursor = 'wait'; break;
                case 8: t.cursor = 'crosshair'; break;
                case 9: t.cursor = 'zoom-in'; break;
                case 10: t.cursor = 'zoom-out'; break;
                case 11: t.cursor = 'help'; break;
            }
            break;

        case 'rtb.onlink':
            NoriEvents.applyEventHandler(e, key, NoriEvents.buildEventHandler(value, e, key, arg => arg));
            break;

        case 'el.onmousedown':
        case 'el.onmouseup':
        case 'el.onmousemove':
        case 'el.onmouseenter':
        case 'el.onmouseleave':
            NoriEvents.applyEventHandler(e, key, NoriEvents.buildEventHandler(value, e, key, ev => {
                let r = e.getBoundingClientRect();
                let x = ev.clientX - r.left;
                let y = ev.clientY - r.top;
                let btn = '';
                switch (ev.button) {
                    case 0: btn = 'primary'; break;
                    case 1: btn = 'aux'; break;
                    case 2: btn = 'secondary'; break;
                    default: btn = '?'; break;
                }
                return x + '|' + y + '|' + (x / r.width) + '|' + (y / r.height) + '|' + btn;
            }));
            break;

        case 'el.onscrollintoview':
            NoriEvents.applyEventHandler(e, key, NoriEvents.buildEventHandler(value, e, key, arg => arg));
            e.NORI_scrollHandlerTriggered = false;
            break;

        case 'el.bold':
            (e.NORI_type == 'Button' ? e.firstChild : e).style.fontWeight = value ? 'bold' : 'normal';
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
        case 'el.fontsize':
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

        case 'el.opacity':
            e.style.opacity = Math.min(1, Math.max(0, value / 1024.0));
            break;

        case 'el.onclick':
            NoriEvents.applyEventHandler(e, key, NoriEvents.buildEventHandler(value, e, key, ''));
            break;

        case 'el.focus':
            if (value) e.firstChild.focus();
            break;

        case 'float.anchorleft': e.NORI_floatAnchors[0] = value; break;
        case 'float.anchortop': e.NORI_floatAnchors[1] = value; break;
        case 'float.anchorright': e.NORI_floatAnchors[2] = value; break;
        case 'float.anchorbottom': e.NORI_floatAnchors[3] = value; break;

        case 'border.crop': e.style.overflow = value ? 'hidden' : 'auto'; break;
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

        case 'img.nn':
        case 'cv.nn':
            e.firstChild.style.imageRendering = value ? 'pixelated' : 'auto';
            break;

        case 'btn.text': e.firstChild.innerHTML = NoriUtil.escapeHtml(value); break;
        case 'btn.bevel': e.firstChild.style.borderWidth = value ? 'auto' : '0px'; break;
        case 'btn.radius': e.firstChild.style.borderRadius = value + 'px'; break;
        case 'btn.disabled': e.firstChild.disabled = !!value; break;

        case 'btn.gradtop':
        case 'btn.gradbottom':
            if (!e.NORI_gradient) e.NORI_gradient = {};
            e.NORI_gradient[key === 'btn.gradtop' ? 'top' : 'bottom'] = value;
            if (e.NORI_gradient.top && e.NORI_gradient.bottom) {
                e.firstChild.style.background = 'linear-gradient(' + e.NORI_gradient.top + ',' + e.NORI_gradient.bottom + ')';
            }
            break;

        case 'txtblk.text':
            switch (value.charAt(0)) {
                case '0':
                    e.firstChild.innerHTML = NoriUtil.escapeHtml(value.substr(1), true);
                    break;
                case '1':
                    while (e.firstChild.firstChild) e.firstChild.removeChild(e.firstChild.firstChild);
                    e.firstChild.append(NoriUtil.convertRichText(value.substr(1), e));
                    break;
            }

            break;
        case 'txtblk.wrap': e.NORI_wrap = value === 1; break;

        case 'scroll.x': e.NORI_scrollpanel[0] = ctx.scrollEnumLookup[value]; break;
        case 'scroll.y': e.NORI_scrollpanel[1] = ctx.scrollEnumLookup[value]; break;

        case 'img.src': setImageSource(e, value); break;

        case 'input.value':
            switch (e.NORI_type) {
                case 'PasswordBox':
                case 'TextBox':
                    if (e.firstChild.value != value) {
                        e.firstChild.value = value;
                    }
                    break;
                case 'CheckBox':
                    if (!e.firstChild.checked != !value) {
                        e.firstChild.checked = !!value;
                    }
                    break;

                default:
                    throw "value setter not defined: " + e.NORI_type;
            }
            break;

        case 'tb.border':
            e.firstChild.style.borderWidth = value ? '' : '0px';
            break;
        case 'tb.multiline':
            t = e.firstChild;
            e.removeChild(e.firstChild);
            if (value) {
                e.append(document.createElement('textarea'));
                e.firstChild.style.resize = 'none';
                e.firstChild.style.fontFamily = 'sans-serif';
            } else {
                e.append(document.createElement('input'));
                e.firstChild.type = 'text';
            }
            e.firstChild.spellcheck = false;
            e.firstChild.style.width = '100%';
            e.firstChild.style.height = '100%';

            e.firstChild.value = t.value;

            // Transfer visual properties
            e.firstChild.style.borderWidth = t.style.borderWidth;
            e.firstChild.style.backgroundColor = t.style.backgroundColor;

            for (let jsEv of Object.keys(e.NORI_handlers.inner)) {
                e.firstChild.addEventListener(jsEv, e.NORI_handlers.inner[jsEv]);
            }

            break;

        case 'input.onfocus':
        case 'input.onblur':
            NoriEvents.applyEventHandler(e, key, NoriEvents.buildEventHandler(value, e, key, ''))
            break;

        case 'input.changed':
            NoriEvents.applyEventHandler(e, key, NoriEvents.buildEventHandler(value, e, key, 'ignored'));
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
                propertyValue = NoriUtil.decodeB64(items[i++]);
                ctx.fontsLoading.push(NoriUtil.decodeB64(items[i++]));
                ctx.fontLoader.innerHTML += "\n@import url('" + propertyValue + "');"
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
                        propertyValue = NoriUtil.decodeB64(propertyValue);
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
                    if (!performLayoutPass && !ctx.layoutExemptProperties[propertyKey]) {
                        performLayoutPass = true;
                    }
                }
                i += propertyDeletionCount;

                propertyCount = parseInt(items[i++]);
                for (j = 0; j < propertyCount; ++j) {
                    propertyKey = items[i + j];
                    propertyValue = items[i + j + propertyCount];
                    if (!performLayoutPass && !ctx.layoutExemptProperties[propertyKey]) {
                        performLayoutPass = true;
                    }
                    if (isTextProperty[propertyKey]) {
                        propertyValue = NoriUtil.decodeB64(propertyValue);
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

            case 'AU': // Audio event
                propertyKey = items[i++];
                j = NoriAudio.handleAudioEvent(id, propertyKey, items, i);
                i += j;
                break;

            case 'GP':
                NoriGamepad.init();
                break;

            default:
                throw new Error("Unknown command: " + instruction);
        }
    }

    if (performLayoutPass) {
        NoriLayout.doLayoutPass();
        platformSpecificHandleEvent(-1, 'on-render-pass', '');
    }
}

const waitForFonts = () => {
    let root = ctx.noriHostDiv;
    if (ctx.fontsLoading.length === 0) {
        return Promise.resolve(true);
    }
    const font = ctx.fontsLoading.pop();
    let temp = document.createElement('div'); // font will not load until something uses it.
    root.append(temp);
    let style = temp.style;
    style.fontSize = '1pt';
    style.color = '#fff';
    temp.append('.');
    style.fontFamily = '"' + font + '"';
    let waitForFont = () => {
        if (document.fonts.check('8pt "' + font + '"')) {
            root.removeChild(temp);
            return waitForFonts();
        }
        return NoriUtil.promiseWait(10).then(() => waitForFont());
    };
    return waitForFont();
};
