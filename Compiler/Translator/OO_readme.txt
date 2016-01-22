(ﾉ◕ヮ◕)ﾉ*:･ﾟ✧ Description of the class definition stuff in the interpreter

=== Class Definitions Table ===
When a class definition is encountered, it is assigned added to this table. The length of the table
BEFORE you add it is the ID (the table starts with 1 null entry at index 0 so the first real ID is 1).
Each row in this table has multiple attributes in a list:
- 0 - Name ID
- 1 - PC of constructor or 0
- 2 - Maximum arg count of the constructor
- 3 - Base Class ID
- 4 - 1 or 0: indicates whether there is a need to call base constructors. Will be 0 if the rest of the parent chain is just default constructors or empty.
- 5 - 1 or 0: is the class usable yet? This starts as 0 but gets marked as 1 when the definition is encountered.

=== Method Information Table ===
Like the classDefinitions, the index of each entry is the class ID. The row is a dictionary of method name ID's to a list of method information
Each method information list is a list of numbers like this:
- 0 - name ID
- 1 - max args
- 2 - PC
When a class is created, a copy is created of this entry for the parent class if one exists. The new methods defined in that class are then applied to this dictionary
and overwrite any entries that are already there. 

=== FIRST PASS AND SPECIAL CAACHE POPULATION ===
These tables are populated in the cache passes.
The class ID's and definitions are built in the first pass.
In the 2nd cache pass, the class ID is inserted into the special cache row for commands that use them: CLASS_DEFINITION, CALL_CONSTRUCTOR, and CALL_BASE_CONSTRUCTOR

=== BYTE CODE ARGUMENTS ===
CLASS_DEFINITION:
- 0 - Name ID
- 1 - Base class Name ID or -1
- 2 - Arg count of constructor or -1 if no constructor is present.
- 3 - How many methods?
- followed by method data (set of 3 numbers each)
-- 4 + 3n: Name ID of the method
-- 5 + 3n: PC offset from current PC
-- 6 + 3n: Maximum number of args

- special cache: class ID

When a class definition is encountered, the class definition is marked as usable. This prevents using a class before it is defined.

INSTANTIATE:
- 0 - Name ID

