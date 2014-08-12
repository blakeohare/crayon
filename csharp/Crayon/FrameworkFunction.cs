namespace Crayon
{
	internal enum FrameworkFunction
	{
		CLOCK_TICK = 1,
		CURRENT_TIME = 2,
		DRAW_RECTANGLE = 3,
		FILL_SCREEN = 4,
		FLOOR = 5,
		GET_EVENTS = 6,
		INITIALIZE_GAME = 7,
		INITIALIZE_SCREEN = 8,
		INVALIDATE_DISPLAY = 9,
		PRINT = 10,
		RANDOM = 11,
		SET_TITLE = 12,
	}

	internal static class FrameworkFunctionUtil
	{
		public static bool PushesToStack(FrameworkFunction ff)
		{
			switch (ff)
			{
				case FrameworkFunction.CURRENT_TIME:
				case FrameworkFunction.FLOOR:
				case FrameworkFunction.GET_EVENTS:
				case FrameworkFunction.RANDOM:
					return true;
				default:
					return false;
			}
		}
	}
}
