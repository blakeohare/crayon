VERSION = '0.2.0'

import xml.etree.ElementTree as ET
import os

def fail(message):
	raise Exception(message)

def trim(string):
	if string == None: return ''
	string = string.replace('\r', ' ').replace('\n', ' ').replace('\t', ' ').replace('    ', ' ')
	while '  ' in string:
		string = string.replace('  ', ' ')
	return string
	

DISABLE_TRIM_TAGS = ['code']

# the contents of these nodes should be literally relayed to the output
def parse_content_element(element, buffer = None, trim_enabled = True):
	
	root = buffer == None
	if root:
		buffer = []
		if element == None: return ''
	
	if element != None:
		if element.text != None:
			text = element.text
			if trim_enabled:
				text = trim(text)
			buffer.append(text)
		for child in element.getchildren():
			tag = child.tag
			te = trim_enabled
			if te and tag in DISABLE_TRIM_TAGS:
				te = False
			buffer.append('<' + tag + '>')
			parse_content_element(child, buffer, te)
			buffer.append('</' + tag + '>')
			if child.tail != None:
				text = child.tail
				if trim_enabled:
					text = trim(text)
				buffer.append(text)
	
	if root:
		return ''.join(buffer).strip()

_CHILD_ORDER = {}
def get_children_lookup(tag):
	lookup = {}
	for child in tag.getchildren():
		tag = child.tag
		list = lookup.get(tag)
		if list == None:
			list = []
			lookup[tag] = list
		list.append(child)
		_CHILD_ORDER[child] = len(_CHILD_ORDER)
	return lookup

def parse_documentation(documentation):
	items = get_children_lookup(documentation)
	parse_children(None, items)

def parse_library(library):
	library_name = library.attrib.get('name')
	if library_name == None:
		fail("Surely this library has a name")
	print "Parsing", library_name + '...'
	library_key = library_name.lower()
	items = get_children_lookup(library)
	description = items.get('description', [None])[0]
	
	if description != None:
		ENTITIES[library_key] = {
			'xml-entity': library,
			'type': 'library',
			'name': library_name,
			'description': parse_content_element(description)
		}
	parse_children(None, items)
	
def parse_namespace(prefix, namespace):
	
	name = namespace.attrib.get('name')
	if name == None:
		fail("Namespace without a name")
	key = name.lower()
	if prefix != None:
		key = prefix.lower() + '.' + key
	print "Parsing " + key + '...'
	
	# Namespace names and library names will collide. Library entries are preferred.
	contents = get_children_lookup(namespace)
	description = contents.get('description', [None])[0]
	if not ENTITIES.has_key(key):
		ENTITIES[key] = {
			'xml-entity': namespace,
			'type': 'namespace',
			'name': name,
			'description': parse_content_element(description)
		}
	parse_children(key, contents)
	
def parse_children(prefix, contents):
	
	for li in contents.get('library', []):
		parse_library(li)
		
	for fi in contents.get('field', []):
		parse_field(prefix, fi)
	
	for fn in contents.get('function', []):
		parse_function(prefix, fn)
	
	for enum in contents.get('enum', []):
		parse_enum(prefix, enum)
	
	for cls in contents.get('class', []):
		parse_class(prefix, cls)
		
	for ns in contents.get('namespace', []):
		parse_namespace(prefix, ns)

def parse_class(prefix, cls):
	name = cls.attrib.get('name')
	if name == None:
		fail("Class requires a name.")
	print "Parsing", prefix + '.' + name + '...'
	path = prefix + '.' + name.lower()
	
	contents = get_children_lookup(cls)
	description = contents.get('description', [None])[0]
	
	ENTITIES[path] = {
		'xml-entity': cls,
		'type': 'class',
		'name': name,
		'description': parse_content_element(description),
	}
	
	parse_children(path, contents)
	

def parse_field(prefix, field):
	name = field.attrib.get('name')
	if name == None:
		fail("Fields require a name.")
	print "Parsing", prefix + '.' + name + '...'
	path = prefix + '.' + name.lower()
	
	contents = get_children_lookup(field)
	description = contents.get('description', [None])[0]
	
	ENTITIES[path] = {
		'xml-entity': field,
		'type': 'field',
		'name': name,
		'description': parse_content_element(description),
	}

def parse_function(prefix, fn):
	name = fn.attrib.get('name')
	is_constructor = False
	if fn.attrib.get('constructor') == "true":
		is_constructor = True
		name = 'constructor'
	
	if name == None:
		fail("Function requires a name.")
	
	path = prefix + '.' + name.lower()
	print "Parsing", prefix + '.' + name + '...'
	data = {
		'ARG_COUNT': '0',
	}
	items = get_children_lookup(fn)
	args = items.get('arg', [])
	header_data = [str(len(args))]
	for arg in args:
		arg_name = arg.attrib.get('name')
		arg_type = arg.attrib.get('type')
		arg_default = arg.attrib.get('default')
		if arg_default == None: arg_default = ''
		if ',' in arg_default: fail("Arg default value cannot contain a comma. Sorry.")
		if arg_name == None: fail("Argument must have a name.")
		if arg_type == None: fail("Argument must have a type.")
		
		header_data.append(arg_name)
		header_data.append(arg_type)
		header_data.append(arg_default) # possibly empty string
	
	data = [','.join(header_data)]
	for arg in args:
		description = parse_content_element(arg)
		data.append(description)
	
	description = parse_content_element(items.get('description', [None])[0])
	
	ENTITIES[path] = {
		'xml-entity': fn,
		'type': 'function',
		'name': name,
		'data': '~@#@~'.join(data),
		'description': description,
	}
		
		
def parse_enum(prefix, enum):
	name = enum.attrib.get('name')
	if name == None:
		fail("Enum requires a name.")
	print "Parsing", prefix + '.' + name + '...'
	path = prefix + '.' + name.lower()
	values = []
	order = enum.attrib.get('order', 'as-is')
	
	items = get_children_lookup(enum)
	values_raw = items.get('value')
	if values_raw == None: fail("Enum without any values")
	keys = []
	for value in values_raw:
		key = parse_enum_value(path, value)
		keys.append(key.split('.')[-1])
	description = items.get('description', [None])[0]
	if description == None: fail("Enum without a description")
	
	if order == 'alpha':
		keys.sort()
	
	ENTITIES[path] = {
		'xml-entity': enum,
		'type': 'enum',
		'name': name,
		'values': keys,
		'description': parse_content_element(description)
	}

def parse_enum_value(path, value_element):
	value = value_element.text
	items = get_children_lookup(value_element)
	description = items.get('description', [None])[0]
	if description == None:
		value = value_element.text.strip()
	else:
		value = value_element.get('name')
		if value == None:
			fail("Enum values that have a description must have the name attribute set on the <value> tag.")
	key = path + '.' + value.lower()
	
	ENTITIES[key] = {
		'xml-entity': value_element,
		'type': 'enum_value_simple' if description == None else 'enum_value',
		'name': value,
		'description': parse_content_element(description),
	}
	
	return key


ENTITIES = {}

for file in filter(lambda x:x.lower().endswith('.xml'), os.listdir('.')):
	print "Parsing", file + '...'
	tree = ET.parse(file)
	root = tree.getroot()
	if root.tag != 'documentation':
		fail("Expected documentation tag")
	parse_documentation(root)

ENTITIES['~'] = {
	'type': 'root',
	'name': VERSION,
	'key': '~'
}

for value in ENTITIES.values():
	value['children'] = []


for key in ENTITIES.keys():
	entity = ENTITIES[key]
	entity['key'] = key
	if key != '~':
		parts = key.split('.')
		
		if len(parts) > 1:
			parent = '.'.join(parts[:-1])
		else:
			parent = '~'
		ENTITIES[parent]['children'].append(entity)

_CHILD_PREFIX = {
	'library': 'li',
	'namespace': 'ns',
	'class': 'cl',
	'function': 'fn',
	'enum': 'en',
	'enum_value': 'ev',
	'enum_value_simple': 'es',
	'field': 'fi',
}

def serialize_children(entity):
	output = []
	children = entity.get('children', [])
	children.sort(key = lambda child:_CHILD_ORDER[child['xml-entity']])
	for child in children:
		prefix = _CHILD_PREFIX.get(child['type'])
		key = child['key'].split('.')[-1]
		if prefix == 'es':
			key = child['name']
		elif prefix == 'fn':
			key = child['name'].lower()
		
		if prefix == None:
			fail("unexpected child type: " + child['type'])
		output.append(prefix + ':' + key)
	return ','.join(output)
		

def sanitize_sql_value(string):
	return string.replace("'", "''")

def generate_sql():
	
	columns = 'type key version name path description children data'.split(' ')
	
	query = '\n'.join([
		"DELETE FROM `documentation` WHERE `version` = '" + VERSION + "';\n",
		"INSERT INTO `documentation` (`" + '`, `'.join(columns) + "`) VALUES "])
	
	queries = []
	
	for entity in ENTITIES.values():
		key = entity['key']
		type = entity['type']
		description = entity.get('description')
		if description == None:
			description = ''
		sql_row = {
			'type': type,
			'key': VERSION + ':' + key,
			'name': entity.get('name', ''),
			'version': VERSION,
			'path': key.replace('.', '/'),
			'description': sanitize_sql_value(description),
			'children': serialize_children(entity),
			'data': sanitize_sql_value(entity.get('data', '')),
		}
		
		row = []
		for column in columns:
			row.append("'" + sql_row[column] + "'")
		queries.append("  (" + ', '.join(row) + ")")
	
	return query + '\n' + ',\n'.join(queries)

c = open('sql.txt', 'wt')
c.write(generate_sql())
c.close()