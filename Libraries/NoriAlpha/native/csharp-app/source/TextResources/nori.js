var Nori = function() {
	var Nori = {};

	Nori.getDomRoot = function () { return document.getElementById('html_root'); }

	Nori.flushData = function (data) {
		Nori.getDomRoot().innerHTML = data;
	};

	return Nori;
}();
