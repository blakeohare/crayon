const ctx = {
	noriHostDiv: null,
	uiRoot: null,
	rootElementId: null,
	rootElement: null,
	elementById: {},
	frameSize: [0, 0],
	textSizer: null,
	
	// some static data:
	isTextProperty: {
		// Note: color properties count as text properties.
		'btn.text': true,
		'txtblk.text': true,
		'border.leftcolor': true,
		'border.topcolor': true,
		'border.rightcolor': true,
		'border.bottomcolor': true,
		'el.bgcolor': true,
		'el.fontcolor': true,
		'el.font': true,
		'img.src': true,
		'input.value': true,
	},
	isPanelType: {
		'Border': true,
		'DockPanel': true,
		'FlowPanel': true,
		'ScrollPanel': true,
		'StackPanel': true
	},
	scrollEnumLookup: ['none', 'auto', 'scroll', 'crop'],

	hasKeyDownListener: false,
	hasKeyUpListener: false,
};
