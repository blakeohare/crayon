
def lib_json_parse(raw):
  import json
  try:
    return lib_json_parse_json_thing(json.loads(raw))
  except:
    return None

def lib_json_parse_json_thing(item):
  if item == None: return v_VALUE_NULL
  if item == "": return v_VALUE_EMPTY_STRING
  t = str(type(item))
  if "'bool'" in t:
    if item == True:
      return v_VALUE_TRUE
    return v_VALUE_FALSE
  if "'int'" in t or "'long'" in t:
    return v_buildInteger(item)
  if "'float'" in t:
    return v_buildFloat(item)
  if "'string'" in t or "'unicode'" in t:
    return v_buildString(str(item))
  if "'list'" in t:
    output = []
    for o in item:
      output.append(lib_json_parse_json_thing(o))
    return v_buildList(output)
  if "'dict'" in t:
    keys = []
    values = []
    for key in item.keys():
      keys.append(key)
      values.append(lib_json_parse_json_thing(item[key]))
    return v_buildDictionary(keys, values)
  return v_VALUE_NULL;
