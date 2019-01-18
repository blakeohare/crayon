var ctx = {
	elementsById: {},
	childIdListByParentId: {},
	rootElementId: null,
	dirtyElementIds: []
};

function getDomRoot() { return document.getElementById('html_root'); }

function flushData(dataRaw) {
	var data = dataRaw.split(',');
	var length = data.length;
	
	var rootElementId = parseInt(data[0]);
	var elementRemovalCount = parseInt(data[1]);
	
	var i = 0;
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
			domElement.nori_existingProperties = {};
			domElement.nori_id = id;
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
		for (j = 0; j < propertyCount; ++j) {
			key = data[i];
			value = data[i + 1];
			propertyLookup[key] = value;
			i += 2;
		}
		domElement.nori_newProperties = propertyLookup;
		
		ctx.dirtyElementIds.push(id);
	}
}

function createElement(type) {
	switch (type) {
		case 'Button':
			return document.createElement('button');
		case 'DockPanel':
			return document.createElement('div');
		default:
			throw "Unknown element type: " + type;
	}
}
