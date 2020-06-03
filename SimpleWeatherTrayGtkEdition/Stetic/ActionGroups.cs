using Gtk;
using System;

namespace Stetic
{
	internal class ActionGroups
	{
		public static ActionGroup GetActionGroup(Type type)
		{
			return GetActionGroup(type.FullName);
		}

		public static ActionGroup GetActionGroup(string name)
		{
			return null;
		}
	}
}
