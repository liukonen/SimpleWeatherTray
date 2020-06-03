using Gtk;
using System;
using System.IO;
using System.Threading;

namespace SimpleWeatherTrayGtkEdition
{
	internal class MainClass
	{
		[STAThread]
		public static void Main(string[] args)
		{
			bool createdNew = false;
			using (Mutex mutex = new Mutex(initiallyOwned: true, "SimpleWeatherTrayGTK", out createdNew))
			{
				if (createdNew)
				{
					try
					{
						GC.Collect();
						Application.Init();
						new tray();
						Application.Run();
					}
					catch (Exception ex)
					{
						File.WriteAllText("Err.log", ex.Message);
					}
					mutex.ReleaseMutex();
				}
				else
				{
					GC.Collect();
				}
			}
		}
	}
}
