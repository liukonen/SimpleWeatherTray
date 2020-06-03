using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using Newtonsoft.Json.Linq;


namespace ExternalService
{

    public class GovWeather3 : WeatherDesktop.Interface.ISharedWeatherinterface
    {
        const string Gov_User = "WeatherWallpaper / v1.0 (https://github.com/liukonen/WeatherWallpaper/; liukonen@gmail.com)";
        const int UpdateInterval = 60;
        private string httpResponseHourly;
        private string httpResponseDaily;
        private string _errors;
        private DateTime LastUpdated = DateTime.MinValue;
        latLong LatLong;
        private Exception _ThrownException = null;

        private latLong Grid;
        private string StationID;

        string ISharedInterface.Debug()
        {
            Dictionary<string, string> debugValues = new Dictionary<string, string>();
            debugValues.Add("Last updated", LastUpdated.ToString());

            return SharedObjects.CompileDebug(debugValues);
        }

        ISharedResponse ISharedInterface.Invoke()
        {
            WeatherResponse response = new WeatherResponse();
            try
            {

                httpResponseHourly = SharedObjects.CompressedCallSite($"https://api.weather.gov/gridpoints/{StationID}/{Grid.Latitude.ToString()},{Grid.Longitude.ToString()}/forecast/hourly", Gov_User);
                httpResponseDaily = SharedObjects.CompressedCallSite($"https://api.weather.gov/gridpoints/{StationID}/{Grid.Latitude.ToString()},{Grid.Longitude.ToString()}/forecast", Gov_User);
                response = Transform(httpResponseDaily, httpResponseHourly);
                LastUpdated = DateTime.Now;
                _ThrownException = null;
            }
            catch (Exception x) { _ThrownException = x; _errors = x.ToString(); }
            return response;
        }

        void ISharedInterface.Load()
        {
            LatLong = GetLocationProperty();
            Grid = LookUpPoints(LatLong, out StationID);
        }

        private WeatherResponse Transform(string Daily, string Hourly)
        {
            WeatherResponse response = new WeatherResponse();
            string shortDescription;
            JObject item = JObject.Parse(Hourly);
            var X = item["properties"]["periods"][0];
            shortDescription = X.Value<string>("shortForecast");
            response.Temp = X.Value<int>("temperature");
            response.ForcastDescription = DetailedForcast(Daily);
            response.WType = convert(shortDescription);
            response.OptionalLink = $"https://forecast.weather.gov/MapClick.php?lat={LatLong.Latitude}&lon={LatLong.Longitude}";
            return response;
        }
        private static string DetailedForcast(string DailyForcast)
        {
            System.Text.StringBuilder builder = new StringBuilder();

            JObject item = JObject.Parse(DailyForcast);

            foreach (JToken jitem in item["properties"]["periods"])
            {
                builder.Append(formatLineOut(jitem.Value<string>("name"), jitem.Value<string>("detailedForecast")));
                builder.Append(Environment.NewLine) ;
           }

            return builder.ToString();
        }


        private static string formatLineOut(string name, string Desc)
        {
            int Padding = name.Length + 4;
            return $"- {name}: {RecusiveSplit(Desc, Padding, true)}";
        }


        private static string RecusiveSplit(string LineToParse, int padding, bool first)
        {
            const int MaxLength = 120;
            if (!first) { string Space = new string(' ', padding); LineToParse = Space + LineToParse; }
            if (LineToParse.Length <= MaxLength) { return LineToParse; }
            string Sub120 = LineToParse.Substring(0, MaxLength);
            int index = Sub120.LastIndexOf(" ");
            string NewLineToParse = LineToParse.Substring(index);
            string itsSub = RecusiveSplit(NewLineToParse, padding, false);
            return Sub120.Substring(0, index) + Environment.NewLine + itsSub;
        }



        private Dictionary<string, SharedObjects.WeatherTypes> MappingItems = new Dictionary<string, SharedObjects.WeatherTypes>()
        {
            { "thunderstorm", SharedObjects.WeatherTypes.ThunderStorm},
            { "partly cloudy", SharedObjects.WeatherTypes.PartlyCloudy},
            { "snow", SharedObjects.WeatherTypes.Snow},
            { "cloudy", SharedObjects.WeatherTypes.Cloudy},
            { "rain", SharedObjects.WeatherTypes.Rain},
            { "dust", SharedObjects.WeatherTypes.Dust},
            { "fog", SharedObjects.WeatherTypes.Fog},
            { "frigid", SharedObjects.WeatherTypes.Frigid},
            { "haze", SharedObjects.WeatherTypes.Haze},
            { "hot", SharedObjects.WeatherTypes.Hot},
            { "smoke", SharedObjects.WeatherTypes.Smoke},
            { "windy", SharedObjects.WeatherTypes.Windy}
        };

        private SharedObjects.WeatherTypes convert(string description)
        {
            string lowerDescription = description.ToLower();
            foreach (var mappedItem in MappingItems)
            {
                if (lowerDescription.Contains(mappedItem.Key)) { return mappedItem.Value; }
            }
            return SharedObjects.WeatherTypes.Clear;
        }

        static latLong GetLocationProperty()
        {
            if (SharedObjects.LatLong.HasRecord() || IntialgetLatLong())
            { return new latLong(SharedObjects.LatLong.Lat, SharedObjects.LatLong.Lng); }
            return new latLong(0, 0);
        }

        static bool IntialgetLatLong()
        {
            Input inputbox = Input.Invoke();
            MessageBox mbox = MessageBox.Invoke();
           while (!SharedObjects.LatLong.HasRecord())
            { mbox.Show("Lat and Long not yet available, Manual enter");

                double lat = double.Parse(inputbox.Show("Please Enter your Latitude", "Latitude"));
                double lon = double.Parse(inputbox.Show("Please Enter your Longitude", "Longitude"));
                SharedObjects.LatLong.Set(lat, lon);
            }
            return true;
        }

        private static latLong LookUpPoints(latLong LatLong, out string station)
        {
            string Lookup = String.Join(",", "GovWeather3Points", LatLong.Latitude.ToString(), LatLong.Longitude.ToString());
            string Item = SharedObjects.AppSettings.ReadSetting(Lookup);
            if (string.IsNullOrWhiteSpace(Item))
            {
                JObject item = JObject.Parse(SharedObjects.CompressedCallSite($"https://api.weather.gov/points/{LatLong.Latitude},{LatLong.Longitude}", Gov_User));
                var jProperties = item["properties"];

                station = jProperties.Value<string>("cwa");
                latLong response = new latLong(jProperties.Value<float>("gridX"), jProperties.Value<float>("gridY"));
                SharedObjects.AppSettings.AddUpdateAppSettings(Lookup, String.Join(",", response.Latitude.ToString(), response.Longitude.ToString(), station));
                return response;
            }
            else
            {
                string[] SS = Item.Split(',');
                station = SS[2];
                return new latLong(float.Parse(SS[0]), float.Parse(SS[1]));
            }

        }

        Dictionary<string, EventHandler> ISharedInterface.SettingsItems()
        {
            return new Dictionary<string, EventHandler>() { { SharedObjects.ZipObjects.ZipMenuItem.Key, SharedObjects.ZipObjects.ZipMenuItem.Value } };
        }

        public Exception ThrownException()
        {
            throw new NotImplementedException();
        }
    }
}