
def lib_json_parse_impl(g, raw):
  import json
  try:
    return lib_json_parse_json_thing(g, json.loads(raw))
  except:
    return None

def lib_json_parse_json_thing(g, item):
  if item == None: return buildNull(g)
  t = str(type(item))
  if "'bool'" in t:
    return buildBoolean(g, item)
  if "'int'" in t or "'long'" in t:
    return buildInteger(g, item)
  if "'float'" in t:
    return buildFloat(g, item)
  if "'str'" in t or "'unicode'" in t:
    return buildString(g, str(item))
  if "'list'" in t:
    output = []
    for o in item:
      output.append(lib_json_parse_json_thing(g, o))
    return buildList(output)
  if "'dict'" in t:
    keys = []
    values = []
    for key in item.keys():
      keys.append(key)
      values.append(lib_json_parse_json_thing(g, item[key]))
    return buildStringDictionary(g, keys, values)
  return buildNull(g);
