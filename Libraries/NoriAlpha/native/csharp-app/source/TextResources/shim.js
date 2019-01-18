function winFormsNoriHandleNewUiData(data) {
	var output = document.getElementById('msg');
	flushData(data);
}

function winFormsNoriHandleEvent(id, args) {
	window.external.SendEventToCSharp(id, args);
}
