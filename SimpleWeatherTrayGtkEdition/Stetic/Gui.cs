using Gtk;

namespace Stetic
{
	internal class Gui
	{
		private static bool initialized;

		internal static void Initialize(Widget iconRenderer)
		{
			if (!initialized)
			{
				initialized = true;
			}
		}
	}
}
