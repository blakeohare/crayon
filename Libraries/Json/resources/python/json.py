
def lib_json_parse(g, raw):
  import json
  try:
    return lib_json_parse_json_thing(g, json.loads(raw))
  except:
    return None

def lib_json_parse_json_thing(g, item):
  if item == None: return v_buildNull(g)
  t = str(type(item))
  if "'bool'" in t:
    return v_buildBoolean(g, item)
  if "'int'" in t or "'long'" in t:
    return v_buildInteger(g, item)
  if "'float'" in t:
    return v_buildFloat(g, item)
  if "'string'" in t or "'unicode'" in t:
    return v_buildString(g, str(item))
  if "'list'" in t:
    output = []
    for o in item:
      output.append(lib_json_parse_json_thing(g, o))
    return v_buildList(output)
  if "'dict'" in t:
    keys = []
    values = []
    for key in item.keys():
      keys.append(key)
      values.append(lib_json_parse_json_thing(g, item[key]))
    return v_buildDictionary(keys, values)
  return v_buildNull(g);
