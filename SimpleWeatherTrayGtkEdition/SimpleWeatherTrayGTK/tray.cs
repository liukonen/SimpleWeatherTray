using ExternalService;
using Gdk;
using Gtk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Timers;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using Gtk;
using Gdk;

namespace SimpleWeatherTrayGTK
{
	public class tray
	{
		private const string DayString = "Day";
		private const string NightString = "Night";
		private static string iconPath = "icons" + Path.DirectorySeparatorChar;
		private static StatusIcon trayIcon;
		private Menu notificationMenu = (Menu)(object)new Menu();
		private ISharedWeatherinterface g_Weather;
		private IsharedSunRiseSetInterface g_SunRiseSet;
		private MessageBox mBox = MessageBox.Invoke();
		private WeatherDesktop.Share.Input input = WeatherDesktop.Share.Input.Invoke();
		private string g_CurrentWeatherType;
		private Timer refreshControls = new Timer();
		private WeatherResponse weatherResponse = new WeatherResponse();

		public tray()
		{

			Application.Init();
			Load();

			trayIcon =(StatusIcon)(object)new StatusIcon((Pixbuf)(object)new Pixbuf("icons" + Path.DirectorySeparatorChar + "fog.png"));
			trayIcon.Visible = true;
			//trayIcon.PopupMenu  = 
			//trayIcon.set_Visible(true);
			//trayIcon.add_PopupMenu((PopupMenuHandler)(object)new PopupMenuHandler(OnTrayIconPopup));
			//trayIcon.set_Tooltip("Hello World Icon");
			updateScreen();
		}

		private void Load()
		{
			mBox.msgShow += MBox_msgShow;
			input.msgShow += Input_msgShow;
			refreshControls.Elapsed += RefreshControls_Elapsed;
			g_Weather = new GovWeather3();
			g_SunRiseSet = new InternalSunRiseSet();
			LoadLatLongs();
			g_Weather.Load();
			g_SunRiseSet.Load();
			refreshControls.Interval = 3600000.0;
			refreshControls.Start();
			notificationMenu = InitializeMenu();
		}

		private void OnTrayIconPopup(object o, EventArgs args)
		{
			if (notificationMenu.Visible)
			{
				notificationMenu.Popdown();
				return;
			}
			((Widget)notificationMenu).ShowAll();
			notificationMenu.Popup();
		}

		private void RefreshControls_Elapsed(object sender, ElapsedEventArgs e)
		{
			updateScreen();
		}

		private void Input_msgShow(object sender, EventArgs e)
		{

		 WeatherDesktop.Share.InputArgs inputArgs = (WeatherDesktop.Share.InputArgs)e;
			Dialog val = null;
			try
			{
				Entry val2 = new Entry();
				val = new Dialog()
				{
					Title = inputArgs.Title
				};

				val.AddButton("ok", (ResponseType)(-5));
				val.VBox.Add(new Label(inputArgs.Message));
				val.SetPosition(WindowPosition.CenterAlways);
				val.VBox.Add(val2);
				val.Show();
				val.Run();
				inputArgs.Response = val2.Text;
			}
			finally
			{
				if (val != null)
				{
					val.Dispose();
				}
			}
		}

		private void MBox_msgShow(object sender, EventArgs e)
		{
			msgBoxEventArg msgBoxEventArg = (msgBoxEventArg)e;
			Dialog val = null;
			try
			{
				val = new Dialog()
				{
					Title = msgBoxEventArg.Title
				};

				val.AddButton("ok", (ResponseType)(-5));
				val.SetPosition((WindowPosition)3);
				val.VBox.Add(new Label(msgBoxEventArg.Message));
				((Widget)val).ShowAll();
				val.Run();
			}
			finally
			{
				if (val != null)
				{
					val.Dispose();
				}
			}
		}

		private void MenuAboutClick(object sender, EventArgs e)
		{
			_ = GetType().Assembly.GetName().Name;
			StringBuilder stringBuilder = new StringBuilder();
			object[] customAttributes = GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				stringBuilder.Append(((AssemblyDescriptionAttribute)customAttributes[0]).Description).Append(Environment.NewLine);
			}
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("Version: ").Append(GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
			object[] customAttributes2 = GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
			if (customAttributes2.Length != 0)
			{
				stringBuilder.Append("Copyright: ").Append(((AssemblyCopyrightAttribute)customAttributes2[0]).Copyright).Append(Environment.NewLine);
			}
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(ExtractInfo(g_Weather));
			stringBuilder.Append(ExtractInfo(g_SunRiseSet));
			AboutDialog val = (AboutDialog)(object)new AboutDialog();
			val.SetPosition((WindowPosition)3);
			val.Title = "Simple Weather Tray Gtk Edition";
			val.Authors = new string[1]{"Luke Liukonen"};
			val.Comments = stringBuilder.ToString();
			val.Show();
		}

		private void MenuExitClick(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void GetForcast(object sender, EventArgs e)
		{
			Dialog val = null;
			try
			{
				LinkButton val2 = (LinkButton)(object)new LinkButton(weatherResponse.OptionalLink);
				val2.Clicked += Weather_Clicked;
				val = new Dialog() { Title = "Forcast" };
				val.AddButton("ok", (ResponseType)(-5));
				val.SetPosition((WindowPosition)3);
				val.VBox.Add(new Label(weatherResponse.ForcastDescription));
				val.VBox.Add(val2);
				((Widget)val).ShowAll();
				val.Run();
			}
			finally
			{
				if (val != null)
				{
					val.Dispose();
				}
			}
		}

		private void Weather_Clicked(object sender, EventArgs e)
		{
			Process.Start(((LinkButton)sender).Uri);
		}

		private Menu InitializeMenu()
		{
			Menu val = new Menu();
			val.Add(CreateMenuItem("Forcast", GetForcast));
			val.Add(CreateMenuItem("About", MenuAboutClick));
			val.Add(CreateMenuItem("Exit", MenuExitClick));
			return (Menu)(object)val;
		}

		private Menu GetSettings()
		{
			Menu val = new Menu();
			val.Add(SubMenu("Internal Sun Rise Set", g_SunRiseSet.SettingsItems()));
			val.Add(SubMenu("Gov Weather", g_Weather.SettingsItems()));
			return (Menu)(object)val;
		}

		private static MenuItem SubMenu(string text, Dictionary<string, EventHandler> dict)
		{

			MenuItem val = (MenuItem)(object)new MenuItem(text);
			Menu val2 = (Menu)(object)new Menu();
			foreach (KeyValuePair<string, EventHandler> item in dict)
			{
				((Container)val2).Add((Widget)(object)CreateMenuItem(item.Key, item.Value));
			}
			val.Submenu =val2;
			return val;
		}

		private static MenuItem SubMenu(string text, Menu menu)
		{

			MenuItem val = new MenuItem(text);
			val.Submenu = menu;
			return (MenuItem)(object)val;
		}

		private static MenuItem CreateMenuItem(string text, EventHandler @event)
		{

			MenuItem val = new MenuItem(text);
			val.Activated += @event;
			//val.add_Activated(@event);
			return (MenuItem)(object)val;
		}

		private void updateScreen()
		{

			WeatherResponse weatherResponse = this.weatherResponse = (WeatherResponse)g_Weather.Invoke();
			ISharedResponse sharedResponse = g_SunRiseSet.Invoke();
			string text = SharedObjects.BetweenTimespans(DateTime.Now.TimeOfDay, ((SunRiseSetResponse)sharedResponse).SunRise.TimeOfDay, ((SunRiseSetResponse)sharedResponse).SunSet.TimeOfDay) ? "Day" : "Night";
			string name = Enum.GetName(typeof(SharedObjects.WeatherTypes), weatherResponse.WType);
			trayIcon.Tooltip = ($"{name} {weatherResponse.Temp}");
			_ = text + name;
			trayIcon.Pixbuf = ((Pixbuf)(object)new Pixbuf(GetWeatherIconPath(weatherResponse.WType, text == "Day")));
		}

		private void LoadLatLongs()
		{
			if (!SharedObjects.LatLong.HasRecord())
			{
				OpenDataFlatFileLookup openDataFlatFileLookup = new OpenDataFlatFileLookup();
				SharedObjects.LatLong.Set(openDataFlatFileLookup.Latitude(), openDataFlatFileLookup.Longitude());
			}
		}

		private static string GetWeatherIconPath(SharedObjects.WeatherTypes weather, bool isDaytime)
		{
			switch (weather)
			{
				case SharedObjects.WeatherTypes.Clear:
					if (!isDaytime)
					{
						return iconPath + "Clear_night.png";
					}
					return iconPath + "Clear_day.png";
				case SharedObjects.WeatherTypes.Cloudy:
					return iconPath + "cloudy.png";
				case SharedObjects.WeatherTypes.Fog:
					return iconPath + "fog.png";
				case SharedObjects.WeatherTypes.Frigid:
					return iconPath + "Frigid.png";
				case SharedObjects.WeatherTypes.Hot:
					return iconPath + "hot.png";
				case SharedObjects.WeatherTypes.PartlyCloudy:
					if (!isDaytime)
					{
						return iconPath + "PartlyCloudy_night.png";
					}
					return iconPath + "PartlyCloudy_day.png";
				case SharedObjects.WeatherTypes.Rain:
					return iconPath + "raindrop.png";
				case SharedObjects.WeatherTypes.Snow:
					return iconPath + "snowflake.png";
				case SharedObjects.WeatherTypes.ThunderStorm:
					return iconPath + "Thunderstorm.png";
				case SharedObjects.WeatherTypes.Windy:
					return iconPath + "wind.png";
				default:
					return iconPath + "windsock.png";
			}
		}

		private string ExtractInfo(ISharedInterface Item)
		{
			string text = Item.GetType().Assembly.GetName().Name + " " + Item.ToString();
			object[] customAttributes = g_Weather.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(text).Append(Environment.NewLine);
			stringBuilder.Append('_', text.Length).Append(Environment.NewLine);
			if (customAttributes.Length != 0)
			{
				stringBuilder.Append("Copyright: ").Append(((AssemblyCopyrightAttribute)customAttributes[0]).Copyright).Append(Environment.NewLine);
			}
			stringBuilder.Append("Version: ").Append(g_Weather.GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
			stringBuilder.Append("Debug Info: ").Append(Item.Debug()).Append(Environment.NewLine)
				.Append(Environment.NewLine);
			return stringBuilder.ToString();
		}
	}
}
