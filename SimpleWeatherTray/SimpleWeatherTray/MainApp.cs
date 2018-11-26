using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using System.Linq;

namespace SimpleWeatherTray
{
    public sealed class MainApp: IDisposable
    {
        #region global Objects
        private NotifyIcon notifyIcon;
        private ContextMenu notificationMenu;
        private ISharedWeatherinterface g_Weather;
        private IsharedSunRiseSetInterface g_SunRiseSet;
        private string g_CurrentWeatherType;
        private CompositionContainer _container;
        readonly string PluginPaths = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "Plugins";

        #endregion

        #region Initialize icon and menu
        public MainApp()
        {
            LazyLoader();
            DeclareGlobals();
            notifyIcon = new NotifyIcon();
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.DoubleClick += IconDoubleClick;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainApp));
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.ContextMenu = notificationMenu;
            CreateTimer();
            UpdateScreen();
        }
        #endregion

        #region Launch Functions

        private void LazyLoader()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(SimpleWeatherTray.MainApp).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory));

            if (System.IO.Directory.Exists(PluginPaths))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(PluginPaths));
                foreach (var item in System.IO.Directory.EnumerateDirectories(PluginPaths)) { catalog.Catalogs.Add(new DirectoryCatalog(item)); }
            }
            _container = new CompositionContainer(catalog);
            try { this._container.ComposeParts(this); }
            catch (CompositionException compositionException) { MessageBox.Show(compositionException.Message); }
        }

        private void DeclareGlobals()
        {
            try
            {
                //get weather type
                string weatherType = SharedObjects.AppSettings.ReadSetting(Properties.Resources.cWeather);
                if (!string.IsNullOrWhiteSpace(weatherType)) { GetWeatherByName(weatherType); }
                if (g_Weather == null)
                {
                    g_Weather = (from X in WeatherObjects.AsEnumerable()
                                 where (!(X.Metadata.ClassName.StartsWith("Mock")))
                                 select (ISharedWeatherinterface)X.Value).FirstOrDefault() ?? GetWeatherByName("Mock_Weather");
                }
                g_Weather.Load();

                //try get latlong if you can
                if (!SharedObjects.LatLong.HasRecord())
                {
                    if (LatLongObjects != null)
                    {
                        double[] i = (from X in LatLongObjects.AsEnumerable()
                                      where X.Value.worked()
                                      select new double[] { X.Value.Latitude(), X.Value.Longitude() }).FirstOrDefault() ?? new double[] { 0, 0 };
                        SharedObjects.LatLong.Set(i[0], i[1]);
                    }
                }

                //get SRS
                string srs = SharedObjects.AppSettings.ReadSetting(Properties.Resources.cSRS);
                if (!string.IsNullOrWhiteSpace(srs)) { g_SunRiseSet = GetSRSByName(srs); }
                if (g_SunRiseSet == null)
                {
                    g_SunRiseSet = (from X in SRSObjects.AsEnumerable()
                                    where (!X.Metadata.ClassName.StartsWith("Mock"))
                                    select (IsharedSunRiseSetInterface)X.Value).First() ?? GetSRSByName("Mock_SunRiseSet");
                }
                g_SunRiseSet.Load();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);

            }
        }

        private void CreateTimer()
        {
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer { Interval = 3600000};/*(60 minutes)*/
            t.Tick += new EventHandler(OnTimedEvent);
            t.Start();
        }

        private void UpdateScreen()
        {
            var weather = (WeatherResponse)g_Weather.Invoke();
            var sunriseSet = g_SunRiseSet.Invoke();
            string currentTime;
            currentTime = (SharedObjects.BetweenTimespans(DateTime.Now.TimeOfDay, ((SunRiseSetResponse)sunriseSet).SunRise.TimeOfDay, ((SunRiseSetResponse)sunriseSet).SunSet.TimeOfDay)) ? Properties.Resources.cDay : Properties.Resources.cNight;
            string weatherType = Enum.GetName(typeof(SharedObjects.WeatherTypes), weather.WType);
            notifyIcon.Text = weatherType + " " + weather.Temp.ToString();
            string currentWeatherType = currentTime + weatherType;
            if (string.IsNullOrWhiteSpace(g_CurrentWeatherType) || currentWeatherType != g_CurrentWeatherType)
            {
                g_CurrentWeatherType = currentWeatherType;
                notifyIcon.Icon = GetWeatherIcon(weather.WType, (currentTime == Properties.Resources.cDay));
            }
        }
        #endregion

        #region Menus

        private MenuItem[] InitializeMenu()
        {
            return new MenuItem[] { new MenuItem("Settings", GetSettings()), new MenuItem("About", MenuAboutClick), new MenuItem("Exit", MenuExitClick) };
        }

        private MenuItem[] GetSettings()
        {
            return new MenuItem[] { new MenuItem("Global", GlobalMenuSettings()), new MenuItem(g_SunRiseSet.GetType().Name, g_SunRiseSet.SettingsItems()), new MenuItem(g_Weather.GetType().Name, g_Weather.SettingsItems()) };
        }

        private MenuItem[] GlobalMenuSettings()
        {
            string SelectedItem = SharedObjects.AppSettings.ReadSetting(Properties.Resources.cWeather);
            string SelectedSRS = SharedObjects.AppSettings.ReadSetting(Properties.Resources.cSRS);

            return new MenuItem[] {
                new MenuItem("Plugin Folder", PluginFolder_Event),
                new MenuItem("Weather", (from item in WeatherObjects
                                               select new MenuItem(item.Metadata.ClassName, UpdateGlobalObjecttype) { Checked = (item.Metadata.ClassName == SelectedItem) }).ToArray()),
                new MenuItem("SunRiseSet", (from item in SRSObjects
                                                  select new MenuItem(item.Metadata.ClassName, UpdateGlobalObjecttype) { Checked = (item.Metadata.ClassName == SelectedSRS) }).ToArray())
            };
        }

        #endregion

        #region Events

        private void OnTimedEvent(object sender, EventArgs e) { UpdateScreen(); }

        private void MenuAboutClick(object sender, EventArgs e)
        {
            string Name = this.GetType().Assembly.GetName().Name;

            System.Text.StringBuilder message = new System.Text.StringBuilder();
            //Description
            //
            //Version: XXXX
            //Copyright:XXXXX
            //
            //Others
            var CustomDescriptionAttributes = this.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
            if (CustomDescriptionAttributes.Length > 0) { message.Append(((System.Reflection.AssemblyDescriptionAttribute)CustomDescriptionAttributes[0]).Description).Append(Environment.NewLine); }
            message.Append(Environment.NewLine);
            message.Append("Version: ").Append(this.GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
            var CustomInfoCopyrightCall = this.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
            if (CustomInfoCopyrightCall.Length > 0) { message.Append("Copyright: ").Append(((System.Reflection.AssemblyCopyrightAttribute)CustomInfoCopyrightCall[0]).Copyright).Append(Environment.NewLine); }
            message.Append(Environment.NewLine);
            message.Append(ExtractInfo(g_Weather));
            message.Append(ExtractInfo(g_SunRiseSet));
            MessageBox.Show(message.ToString(), Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MenuExitClick(object sender, EventArgs e) { Application.Exit(); }

        private void UpdateGlobalObjecttype(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            if (((MenuItem)Current.Parent).Text == "Weather")
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(Properties.Resources.cWeather, Name);
                g_Weather = GetWeatherByName(Name);
                g_Weather.Load();
            }
            else if (((MenuItem)Current.Parent).Text == "SunRiseSet")
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(Properties.Resources.cSRS, Name);

                g_SunRiseSet = GetSRSByName(Name);
                g_SunRiseSet.Load();
            }
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.ContextMenu = notificationMenu;
        }

        private void IconDoubleClick(object sender, EventArgs e) { MessageBox.Show(((WeatherResponse)(g_Weather).Invoke()).ForcastDescription); }

        private void PluginFolder_Event(object sender, EventArgs e) { if (System.IO.Directory.Exists(PluginPaths)) { System.Diagnostics.Process.Start(PluginPaths); } }

        #endregion

        #region Methods and functions

        private IsharedSunRiseSetInterface GetSRSByName(string name) { return (from X in SRSObjects.AsEnumerable() where X.Metadata.ClassName == name select X.Value).FirstOrDefault(); }

        private ISharedWeatherinterface GetWeatherByName(string name) { return (from X in WeatherObjects.AsEnumerable() where X.Metadata.ClassName == name select X.Value).FirstOrDefault(); }

        private string ExtractInfo(ISharedInterface Item)
        {
            string Name = Item.GetType().Assembly.GetName().Name + " " + Item.ToString();
            var CustomInfoCopyrightCall = g_Weather.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append(Name).Append(Environment.NewLine);
            sb.Append('_', Name.Length).Append(Environment.NewLine);
            if (CustomInfoCopyrightCall.Length > 0) { sb.Append("Copyright: ").Append(((System.Reflection.AssemblyCopyrightAttribute)CustomInfoCopyrightCall[0]).Copyright).Append(Environment.NewLine); }
            sb.Append("Version: ").Append(g_Weather.GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
            sb.Append("Debug Info: ").Append(Item.Debug()).Append(Environment.NewLine).Append(Environment.NewLine);
            return sb.ToString();
        }

        private static System.Drawing.Icon GetWeatherIcon(SharedObjects.WeatherTypes weather, bool isDaytime)
        {
            switch (weather)
            {
                case SharedObjects.WeatherTypes.Clear:
                    return (isDaytime) ? Properties.Resources.Clear_day : Properties.Resources.Clear_night;
                case SharedObjects.WeatherTypes.Cloudy:
                    return Properties.Resources.cloudy;
                case SharedObjects.WeatherTypes.Fog:
                    return Properties.Resources.fog;
                case SharedObjects.WeatherTypes.Frigid:
                    return Properties.Resources.Frigid;
                case SharedObjects.WeatherTypes.Hot:
                    return Properties.Resources.hot;
                case SharedObjects.WeatherTypes.PartlyCloudy:
                    return (isDaytime) ? Properties.Resources.PartlyCloudy_day : Properties.Resources.PartlyCloudy_night;
                case SharedObjects.WeatherTypes.Rain:
                    return Properties.Resources.raindrop;
                case SharedObjects.WeatherTypes.Snow:
                    return Properties.Resources.snowflake;
                case SharedObjects.WeatherTypes.ThunderStorm:
                    return Properties.Resources.Thunderstorm;
                case SharedObjects.WeatherTypes.Windy:
                    return Properties.Resources.wind;
                case SharedObjects.WeatherTypes.Dust:
                case SharedObjects.WeatherTypes.Haze:
                case SharedObjects.WeatherTypes.Smoke:
                default:
                    return Properties.Resources.windsock;
            }

        }

        #endregion

        #region Main - Program entry point
        /// <summary>Program entry point.</summary>
        /// <param name="args">Command Line Arguments</param>
        [STAThread]
        public static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isFirstInstance = false;
            // Please use a unique name for the mutex to prevent conflicts with other programs

            using (Mutex mtx = new Mutex(true, "SimpleWeatherTray", out isFirstInstance))
            {
                if (isFirstInstance)
                {
                    try
                    {
                        MainApp notificationIcon = new MainApp();
                        notificationIcon.notifyIcon.Visible = true;
                        GC.Collect();
                        Application.Run();                
                        notificationIcon.notifyIcon.Visible=false;
                    }
                    catch (Exception x)
                    {
                        MessageBox.Show("Error: " + x.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    mtx.ReleaseMutex();
                }
                else
                {
                    GC.Collect();
                    MessageBox.Show("App appears to be running. if not, you may have to restart your machine to get it to work.");
                }
            }

        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) {  _container.Dispose(); }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose() { Dispose(true); }
        #endregion

        #region Lazy Load objects
#pragma warning disable 0649
        [ImportMany]
        IEnumerable<Lazy<ISharedWeatherinterface, IClassName>> WeatherObjects;

        [ImportMany]
        IEnumerable<Lazy<IsharedSunRiseSetInterface, IClassName>> SRSObjects;

        [ImportMany]
        IEnumerable<Lazy<ILatLongInterface, IClassName>> LatLongObjects;
#pragma warning restore 0649
        #endregion
     
    }
}
