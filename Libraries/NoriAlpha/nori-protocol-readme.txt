
DOMElement {

	# All properties have a NORI_ prefix to them to prevent collision
	id: int
	type: string
	childrenIdList: Array<int>
	properties: Dictionary<string, obj>
	
	# the following 2 properties are nullable
	newChildrenIds: List<int> # the full list
	newProperties: List<string, obj, ...>
	
	allocatedSize: [int, int]
	requiredSize: [int, int]
	
	# the following are caches for common layout properties.
	# when the corresponding properties are set, these fields are cleared to null
	# when a layout pass occurs, these are cached.
	sizeCache: [int, int] or null
	marginCache: [int, int, int, int] or null
	alignCache: [char, char] {L, C, R, S} | {T, C, B, S} 
	
}

FrameContext {
	rootId: int (-1 is sentinel value)
	elementsById: Dictionary<int, DOMElement>
	rootElement: DOMElement
	
	# a lookup of property setters by element by type
	propertySetters: Dictionary<string, Dictionary<string, function>>
	
	# a lookup of each types parent type. Used to lazily populate the propertySetters
	parentByType: Dictionary<string, string> 
}


The Crayon code should determine which frame should receive an update and then call the corresponding jsbridge function to send serialized updates to.
When a frame is created, it's given an update string as well to populate the basics

Bridge protocol

The protocol is a single string that is a list of items that are either strings or integers.
Strings can be hex encoded, typically done for user-set property values
Booleans also exist, but are expressed as just 0's or 1's in the serialized protocol

RE: the root element has changed. This is sent at the end.
"RE",
	{int: root element id or 0 if empty}
	
PF: sends full property state of an element across the wire
"PF",
	{int: id},
	{string: type},
	{int: property count}
	{list of n string property keys}
	{list of n int or hex-string property values}

PI: sends incremental property updates of an element across the wire.
"PI",
	{int: id},
	{int: properties being deleted n}
	{list of n string property keys for deletion}
	{int: properties being set n}
	{list of n string property keys being set}
	{list of n int or hex-string property values}

CF: sends full children updates for an element
"CF",
	{int: id},
	{int: number of children}
	{list of n int IDs}

CI: sends incremental changes of panel children changes, removes are performed first
"CI",
	{int: id},
	{int: number of children to remove from end of children list}
	{int: number of children to add}
	{list of n int IDs}

Update pass
Visual changes phase:
- loop through updates, apply all property changes first, this ensures that the elements with any ID that is mentioned actually exists
Parentage phase:
- loop through children removals and remove them from the DOM and bookkeeping
- loop through children additions and add them to the DOM and bookkeeping
- loop through full children specifications. In this case first create a lookup table of existing children and then loop through the new ID list. Use the lookup to determine if an element that doesn't match the next existing child in the list should be inserted before it or if the element already there should be removed. Also, if the element already has no elements, then use the code for pure additions.
Layout phase:
- do a descent through everything and determine the minimum amount of space required for all elements
- do a descent through everything and actually allocate sizes to them