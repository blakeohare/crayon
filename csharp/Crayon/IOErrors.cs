using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crayon
{
	internal enum IOErrors
	{
		NONE = 0,
		INVALID_PATH = 1,
		PATH_STEPS_ABOVE_DOMAIN = 3, // e.g. C:\..\foo.txt or %APPDATA%\..\foo.txt in userdata-only context
		DOES_NOT_EXIST = 4,
		BAD_CASING = 5,
		NONEXISTENT_DRIVE = 6,
		IMPOSSIBLE_STATE = 7,
		READ_ONLY = 8,
		DISK_FULL = 9,
		RESERVED_NAME = 10,
		PATH_TOO_LONG = 11,
		UNKNOWN_ERROR = 12,
	}
}
