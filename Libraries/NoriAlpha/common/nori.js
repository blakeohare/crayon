var ctx = {
	elementsById: {},
	childIdListByParentId: {},
	rootElementId: null,
	dirtyElementIds: [],
	domRoot: null,
};

function refreshPage() {
	var i = 0;
	var j;
	var dirtyElementIds = ctx.dirtyElementIds;
	var length = dirtyElementIds.length;
	var id;
	var domElement;
	
	if (ctx.rootElementId === null) return;
	
	clearOutAndSetRootElement();
	
	var propLen;
	var newPropKeys;
	var newProps;
	var oldPropKeys;
	var oldProps;
	var key;
	var value;
	var type;
	var oldValue;
	for (i = 0; i < length; ++i) {
		id = dirtyElementIds[id];
		domElement = elementsById[id];
		type = domElement.nori_type;
		
		newPropKeys = domElement.nori_newPropertyKeys;
		newPops = domElement.nori_newProperties;
		oldPropKeys = domElement.nori_oldPropertyKeys;
		oldProps = domElement.nori_oldProperties;
		
		// clear out the old properties that are no longer set
		propLen = oldPropKeys.length;
		for (j = 0; j < propLen; ++j) {
			key = oldPropKeys[j];
			if (newProps[key] === undefined) {
				unsetElementProperty(domElement, type, key);
			}
		}
		
		// set the new properties if they have changed.
		propLen = newPropKeys.length;
		for (j = 0; j < propLen; ++j) {
			key = newPropKeys[j];
			oldValue = oldProps[key];
			value = newProps[key];
			if (oldValue !== value) {
				setElementProperty(domElement, type, key, value);
			}
		}
	}
	
	throw "Not implemented yet. :(";
}

function clearOutAndSetRootElement() {
	var domRoot = getDomRoot();
	var rootElementId = ctx.rootElementId;
	var rootElement = ctx.elementsById[rootElementId];

	while (domRoot.lastChild != rootElement) {
		domRoot.removeChild(domRoot.lastChild);
	}
	while (domRoot.firstChild != rootElemnt) {
		domRoot.removeChild(domRoot.firstChild);
	}
	if (domRoot.firstChild != rootElement) {
		domRoot.appendChild(rootElement);
		return true;
	}
	
	return false;
}

function getDomRoot() { 
	if (ctx.domRoot === null) {
		ctx.domRoot = document.getElementById('nori_root');
	}
	return ctx.domRoot;
}

function flushData(dataRaw) {
	var data = dataRaw.split(',');
	var length = data.length;
	
	var rootElementId = parseInt(data[0]);
	var elementRemovalCount = parseInt(data[1]);
	
	var i = 2;
	var elementId;
	var parentId;
	while (i < elementRemovalCount) {
		elementId = data[2 + i];
		parentId = data[2 + i + 1];
		i += 2;
		// TODO: process element removal
	}
	
	var type;
	var id;
	var childrenCount;
	var propertyCount;
	var j = 0;
	var propertyLookup;
	var childrenIdList;
	var key;
	var value;
	var domElement;
	while (i < length) {
		type = data[i];
		id = parseInt(data[i + 1]);
		
		domElement = ctx.elementsById[id];
		if (domElement === undefined) {
			domElement = createEmptyElement(type);
			ctx.elementsById[id] = domElement;
			
			domElement.nori_childrenIds = [];
			domElement.nori_newChildrenIds = [];
			domElement.nori_newProperties = {};
			domElement.nori_newPropertyKeys = [];
			domElement.nori_existingProperties = {};
			domElement.nori_existingPropertyKeys = [];
			domElement.nori_id = id;
			domElement.nori_type = type;
		}
		
		childrenCount = parseInt(data[i + 2]);
		propertyCount = parseInt(data[i + 3]);
		i += 4;
		
		for (j = 0; j < childrenCount; ++j) {
			childrenIdList.push(parseInt(data[i + j]));
		}
		domElement.nori_newChildrenIds = childrenIdList;
		i += childrenCount;
		
		propertyLookup = {};
		propertyKeys = [];
		for (j = 0; j < propertyCount; ++j) {
			key = data[i];
			value = data[i + 1];
			
			propertyLookup[key] = value;
			propertyKeys.push(key);
			i += 2;
		}
		domElement.nori_newProperties = propertyLookup;
		domElement.nori_newPropertyKeys = propertyKeys;
		
		ctx.dirtyElementIds.push(id);
	}
	
	refreshPage();
}

var HEX_LOOKUP = {};
var i;
for (i = 0; i < 10; ++i) {
	HEX_LOOKUP[i + ''] = i;
}
HEX_LOOKUP['A'] = 10;
HEX_LOOKUP['B'] = 11;
HEX_LOOKUP['C'] = 12;
HEX_LOOKUP['D'] = 13;
HEX_LOOKUP['E'] = 14;
HEX_LOOKUP['F'] = 15;

function decodeHex(value) {
	var sb = [];
	var len = value.length;
	var a;
	var b;
	var c;
	for (var i = 0; i < len; i += 2) {
		a = HEX_LOOKUP[value.charAt(i)];
		b = HEX_LOOKUP[value.charAt(i + 1)];
		c = chr(a * 16 + b);
		sb.push(c);
	}
	return sb.join('');
}

function createEmptyElement(type) {
	switch (type) {
		case 'Button':
			return document.createElement('button');
		case 'DockPanel':
			return document.createElement('div');
		default:
			throw "Unknown element type: " + type;
	}
}
