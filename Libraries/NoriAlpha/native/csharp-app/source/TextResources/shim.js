function winFormsNoriHandleNewUiData(data) {
	var output = document.getElementById('msg');
	Nori.flushData(data);
}

function winFormsNoriHandleEvent(id, args) {
	window.external.SendEventToCSharp(id, args);
}
