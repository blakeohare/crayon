# Xml Library

General purpose schema-free XML parsing library. Supports both namespace-based XML and basic XML.

Crayon has its own parser implementation (as opposed to using the underlying platforms XML parser of choice) guaranteeing that all parsing quirks are consistent across all platform deployments (e.g. a slight malformation won't be silently ignored when testing locally in a Python-based project and then subsequently cause a parse error when deploying to an Android app).

### parse

`Xml.parse(string, options = 0)`

Parses the string as XML and returns a tree of XML nodes.

| Argument | Type | Description |
| --- | --- | --- |
| **string** | _string_ | The XML string to be parsed. |
| **options** | _mask of XmlOptions_ | A bitmask of enum values from [XmlOptions](#enum-xmloptions) |

There are 3 possible return value types.
- If **XmlOption.FAIL_SILENTLY** is passed in as an option, the function will return `null` if there is an error.
- If **XmlOption.NAMESPACES** is passed in as an option, the function will return an [XmlNamespacedElement](#class-xmlnamespacedelement) instance.
- Otherwise, an [XmlElement](#class-xmlelement) instance is returned.

---

# Enum: XmlOption

An enum of option flags for the XML parser. These are bitmasked values and can be combined using the bitwise or operator `|`.

| Value | Description |
| --- | --- |
| TRIM_WHITESPACE | Text content of elements will have their whitespace trimmed. Notably this will also exclude whitespace text content (i.e. spaces between nodes). |
| STRICT_ENTITIES | Malformed entity codes will be treated as parse errors. Otherwise, they will fall back to uninterpreted text content. |
| ALLOW_INCOMPLETE_DOCUMENT | If a document is only partial, parsing will stop and return the document as is without an error. |
| FAIL_SILENTLY | If an error is encountered, return `null` instead of generating an error. |
| NAMESPACES | If set, the Xml.parse will parse while taking into consideration namespaces and alias prefixes. The result will be an [XmlNamespacedElement](#class-xmlnamespacedelement) document tree. Without this, elements such as `<foo:bar />` will be parsed as having an element name of "foo:bar". |

---

# Class: XmlElement

A class that represents an element in an XML document.

## Fields

### .name

`xmlElement.name`

The name of the element.

---

### .attributes

`xmlElement.attributes`

Attributes of the element. This is a dictionary of string keys mapping to string values.

---

### .children

`xmlElement.children`

A list of children nodes. A child node can be any of 3 possible types:
- [XmlText](#class-xmltext)
- [XmlComment](#class-xmlcomment)
- More [XmlElement](#class-xmlelement)s

---

### .type

`xmlElement.type`

A value from the [NodeType](#enum-nodetype) enum indicating what kind of node this is. For XmlElements, this is always **NodeType.ELEMENT**.

---

# Class: XmlNamespacedElement

Represents an element in an XML document. Includes namespace information for itself and its attributes.

## Fields

### .name

`xmlElement.name`

Name of this element not including the namespace.

---

### .alias

`xmlElement.alias`

The original alias used for this element or empty string if there is none.

---

### .xmlns

`xmlElement.xmlns`

The XML namespace value for this element based on its alias and current set of inherited xmlns attribute definitions.

---

### .children

`xmlElement.children`

List of child nodes of this element.

---

### .attributes

`xmlElement.attributes`

List of XML attributes on this element in the order they appear. Supports multiple attributes with the same name.

Each attribute is an [XmlNamespacedAttribute](#class-xmlnamespacedattribute) instance.

---

# Class XmlNamespacedAttribute

Represents an attribute on an XML element when namespaces are used to parse. Attributes on XmlElements when namespaces are not used are represented by string-to-string dictionaries.

## Fields

### .name

`attribute.name`

Name of the XML attribute without its namespace alias.

---

### .alias

`attribute.alias`

The alias on this attribute or empty string if there is none.

---

### .xmlns

`attribute.xmlns`

The XML namespace that the alias indicates.

---

### .value

`attribute.value`

The value of the XML attribute as a string.

---

# Class: XmlText

Represents text inside an element.

## Fields

### .value

`node.value`

The value of the text as a string.
