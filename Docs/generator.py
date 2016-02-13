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


def get_children_lookup(tag):
	lookup = {}
	for child in tag.getchildren():
		tag = child.tag
		list = lookup.get(tag)
		if list == None:
			list = []
			lookup[tag] = list
		list.append(child)
	return lookup

def parse_documentation(documentation):
	for library in documentation.getchildren():
		if library.tag != 'library':
			fail("Expected library tag")
		parse_library(library)

def parse_library(library):
	library_name = library.attrib.get('name')
	if library_name == None:
		fail("Surely this library has a name")
	print library_name
	library_key = library_name.lower()
	items = get_children_lookup(library)
	description = items.get('description', [None])[0]
	
	if description != None:
		ENTITIES[library_key] = {
			'type': 'library',
			'name': library_name,
			'description': parse_content_element(description)
		}
	
	for ns in items.get('namespace', []):
		name = ns.attrib.get('name')
		if name == None:
			fail("Namespace without a name")
		parse_namespace(library_key, ns)

def parse_namespace(name, namespace):
	key = name.lower()
	
	# Namespace names and library names will collide. Library entries are preferred.
	contents = get_children_lookup(namespace)
	description = contents.get('description', [None])[0]
	if not ENTITIES.has_key(key):
		ENTITIES[key] = {
			'type': 'namespace',
			'name': name,
			'description': parse_content_element(description)
		}
	
	for enum in contents.get('enum', []):
		parse_enum(name, enum)

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
		keys.append(key)
	description = items.get('description', [None])[0]
	if description == None: fail("Enum without a description")
	
	if order == 'alpha':
		keys.sort()
	
	ENTITIES[path] = {
		'type': 'enum',
		'name': name,
		'values': keys,
		'description': parse_content_element(description)
	}

def parse_enum_value(path, value_element):
	value = value_element.text
	items = get_children_lookup(value_element)
	description = items.get('description', [None])[0]
	
	key = path + '.' + value.lower()
	
	ENTITIES[key] = {
		'type': 'enum_value_simple' if description == None else 'enum_value',
		'name': value,
		'description': description,
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
	'enum': 'en',
	'enum_value': 'ev',
	'enum_value_simple': 'es',
}

def serialize_children(entity):
	output = []
	for child in entity.get('children', []):
		prefix = _CHILD_PREFIX.get(child['type'])
		key = child['key']
		if prefix == 'es':
			key = child['name']
		if prefix == None:
			fail("unexpected child type: " + child['type'])
		output.append(prefix + ':' + key)
	return ','.join(output)
		

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
		else:
			print key, description
		sql_row = {
			'type': type,
			'key': VERSION + ':' + key,
			'name': entity.get('name', ''),
			'version': VERSION,
			'path': key.replace('.', '/'),
			'description': description,
			'children': serialize_children(entity),
			'data': ''
		}
		
		row = []
		for column in columns:
			print column
			row.append("'" + sql_row[column] + "'")
		queries.append("  (" + ', '.join(row) + ")")
	
	return query + '\n' + ',\n'.join(queries)

c = open('sql.txt', 'wt')
c.write(generate_sql())
c.close()