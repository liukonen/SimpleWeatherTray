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

namespace SimpleWeatherTrayGtkEdition
{
	internal class tray
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
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected O, but got Unknown
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected O, but got Unknown
			Application.Init();
			Load();
			trayIcon = (StatusIcon)(object)new StatusIcon((Pixbuf)(object)new Pixbuf("icons" + Path.DirectorySeparatorChar + "fog.png"));
			trayIcon.set_Visible(true);
			trayIcon.add_PopupMenu((PopupMenuHandler)(object)new PopupMenuHandler(OnTrayIconPopup));
			trayIcon.set_Tooltip("Hello World Icon");
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
			if (((Widget)notificationMenu).get_Visible())
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
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			InputArgs inputArgs = (InputArgs)e;
			Dialog val = null;
			try
			{
				Entry val2 = (Entry)(object)new Entry();
				Dialog val3 = new Dialog();
				((Window)val3).set_Title(inputArgs.Title);
				val = (Dialog)(object)val3;
				val.AddButton("ok", (ResponseType)(-5));
				((Container)val.get_VBox()).Add((Widget)(object)new Label(inputArgs.Message));
				((Window)val).SetPosition((WindowPosition)3);
				((Container)val.get_VBox()).Add((Widget)(object)val2);
				((Widget)val).ShowAll();
				val.Run();
				inputArgs.Response = val2.get_Text();
			}
			finally
			{
				if (val != null)
				{
					((Object)val).Destroy();
				}
			}
		}

		private void MBox_msgShow(object sender, EventArgs e)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			msgBoxEventArg msgBoxEventArg = (msgBoxEventArg)e;
			Dialog val = null;
			try
			{
				Dialog val2 = new Dialog();
				((Window)val2).set_Title(msgBoxEventArg.Title);
				val = (Dialog)(object)val2;
				val.AddButton("ok", (ResponseType)(-5));
				((Window)val).SetPosition((WindowPosition)3);
				((Container)val.get_VBox()).Add((Widget)(object)new Label(msgBoxEventArg.Message));
				((Widget)val).ShowAll();
				val.Run();
			}
			finally
			{
				if (val != null)
				{
					((Object)val).Destroy();
				}
			}
		}

		private void MenuAboutClick(object sender, EventArgs e)
		{
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Expected O, but got Unknown
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
			((Window)val).SetPosition((WindowPosition)3);
			((Window)val).set_Title("Simple Weather Tray Gtk Edition");
			val.set_Authors(new string[1]
			{
				"Luke Liukonen"
			});
			val.set_Comments(stringBuilder.ToString());
			((Widget)val).Show();
		}

		private void MenuExitClick(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void GetForcast(object sender, EventArgs e)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			Dialog val = null;
			try
			{
				LinkButton val2 = (LinkButton)(object)new LinkButton(weatherResponse.OptionalLink);
				((Button)val2).add_Clicked((EventHandler)Weather_Clicked);
				Dialog val3 = new Dialog();
				((Window)val3).set_Title("Forcast");
				val = (Dialog)(object)val3;
				val.AddButton("ok", (ResponseType)(-5));
				((Window)val).SetPosition((WindowPosition)3);
				((Container)val.get_VBox()).Add((Widget)(object)new Label(weatherResponse.ForcastDescription));
				((Container)val.get_VBox()).Add((Widget)(object)val2);
				((Widget)val).ShowAll();
				val.Run();
			}
			finally
			{
				if (val != null)
				{
					((Object)val).Destroy();
				}
			}
		}

		private void Weather_Clicked(object sender, EventArgs e)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			Process.Start(((LinkButton)sender).get_Uri());
		}

		private Menu InitializeMenu()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			Menu val = new Menu();
			((Container)val).Add((Widget)(object)CreateMenuItem("Forcast", GetForcast));
			((Container)val).Add((Widget)(object)CreateMenuItem("About", MenuAboutClick));
			((Container)val).Add((Widget)(object)CreateMenuItem("Exit", MenuExitClick));
			return (Menu)(object)val;
		}

		private Menu GetSettings()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			Menu val = new Menu();
			((Container)val).Add((Widget)(object)SubMenu("Internal Sun Rise Set", g_SunRiseSet.SettingsItems()));
			((Container)val).Add((Widget)(object)SubMenu("Gov Weather", g_Weather.SettingsItems()));
			return (Menu)(object)val;
		}

		private static MenuItem SubMenu(string text, Dictionary<string, EventHandler> dict)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			MenuItem val = (MenuItem)(object)new MenuItem(text);
			Menu val2 = (Menu)(object)new Menu();
			foreach (KeyValuePair<string, EventHandler> item in dict)
			{
				((Container)val2).Add((Widget)(object)CreateMenuItem(item.Key, item.Value));
			}
			val.set_Submenu((Widget)(object)val2);
			return val;
		}

		private static MenuItem SubMenu(string text, Menu menu)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			MenuItem val = new MenuItem(text);
			val.set_Submenu((Widget)(object)menu);
			return (MenuItem)(object)val;
		}

		private static MenuItem CreateMenuItem(string text, EventHandler @event)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			MenuItem val = new MenuItem(text);
			val.add_Activated(@event);
			return (MenuItem)(object)val;
		}

		private void updateScreen()
		{
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Expected O, but got Unknown
			WeatherResponse weatherResponse = this.weatherResponse = (WeatherResponse)g_Weather.Invoke();
			ISharedResponse sharedResponse = g_SunRiseSet.Invoke();
			string text = SharedObjects.BetweenTimespans(DateTime.Now.TimeOfDay, ((SunRiseSetResponse)sharedResponse).SunRise.TimeOfDay, ((SunRiseSetResponse)sharedResponse).SunSet.TimeOfDay) ? "Day" : "Night";
			string name = Enum.GetName(typeof(SharedObjects.WeatherTypes), weatherResponse.WType);
			trayIcon.set_Tooltip($"{name} {weatherResponse.Temp}");
			_ = text + name;
			trayIcon.set_Pixbuf((Pixbuf)(object)new Pixbuf(GetWeatherIconPath(weatherResponse.WType, text == "Day")));
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
