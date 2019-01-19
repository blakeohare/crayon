function winFormsNoriHandleNewUiData(data) {
	var output = document.getElementById('msg');
	flushData(data);
}

function platformSpecificHandleEvent(id, args) {
	window.external.SendEventToCSharp(id, args);
}
