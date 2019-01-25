
function platformSpecificHandleEvent(id, args) {
	window.external.SendEventToCSharp(id, args);
}
