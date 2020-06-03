using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WeatherDesktop.Share
{
    public static class SharedObjects
    {
        public enum WeatherTypes { ThunderStorm, Rain, Snow, Dust, Fog, Haze, Smoke, Windy, Frigid, Cloudy, PartlyCloudy, Clear, Hot };

        public static class ZipObjects
        {
            public static KeyValuePair<string, EventHandler> ZipMenuItem
            {
                get
                {
                    return new KeyValuePair<string, EventHandler>("Change Zip Code", ChangeZipClick);
                }
            }

            public static void ChangeZipClick(object sender, EventArgs e)
            {
                GetZip();
            }

            /// <summary>
            /// Opens a dialog box, saves zip and returns value. if cancel, closes app gracefully... will retry if not a number
            /// </summary>
            /// <returns></returns>
            public static string GetZip()
            {
                Input inp = Input.Invoke();
                MessageBox box = MessageBox.Invoke();

                string zip = string.Empty;
                object locker = new object();
                lock (locker)
                {
                    while (!int.TryParse(zip, out int zipparse))
                    {
                        zip = inp.Show("Please Enter your zip code", "Zip Code", Rawzip);
                        if (string.IsNullOrWhiteSpace(zip)){box.Show("The application needs your zip code");}
                    }
                }
                Rawzip = zip;
                return zip;
            }

            public static string Rawzip
            {
                get
                {
                    return AppSettings.ReadSetting("zipcode");
                }
                set
                {
                    AppSettings.AddUpdateAppSettings("zipcode", value);
                }
            }

            public static string TryGetZip()
            {
                if (string.IsNullOrWhiteSpace(Rawzip)) return GetZip();
                return Rawzip;
            }
        }

        public static class AppSettings
        {
            #region App Config (No Encryption)

            private static Dictionary<string, string> Config {
                get {
                    Dictionary<string, string> configItems = new Dictionary<string, string>();
                    if (System.IO.File.Exists("config.json"))
                    {
                        var configFile = System.IO.File.ReadAllText("config.json");
                        if (!string.IsNullOrEmpty(configFile))
                        {
                            configItems = JsonConvert.DeserializeObject<Dictionary<string, string>>(configFile);
                        }
                    }
                    return configItems;
                }
                set {
                    System.IO.File.WriteAllText("config.json", JsonConvert.SerializeObject(value));
                }

        }

            public static void AddUpdateAppSettings(string key, string value)
            {
                    var configFile = Config;
                    if (configFile.ContainsKey(key)) { configFile[key] = value; }
                    else { configFile.Add(key, value); }
                    Config = configFile;
                }

            public static void RemoveAppSetting(string key)
            {
                var configFile = Config;
                if (configFile.ContainsKey(key)) { configFile.Remove(key); Config = configFile; }
                
            }
            public static string ReadSetting(string key)
            {
                var configFile = Config;
                if (configFile.ContainsKey(key)){ return configFile[key]; }
                return string.Empty;
            }

            #endregion


        }


        public class LatLong
        {
            const string csvEncryptedLatLongName = "LatLong";
            public static float Lat
            {
                get
                {
                    string value = WeatherDesktop.Share.SharedObjects.AppSettings.ReadSetting(csvEncryptedLatLongName);
                    if (value != null) return float.Parse(value.Split(',')[0].Replace(",", string.Empty));
                    return 0;
                }
            }
            public static float Lng
            {
                get
                {
                    string value = WeatherDesktop.Share.SharedObjects.AppSettings.ReadSetting(csvEncryptedLatLongName);
                    if (value != null) return float.Parse(value.Split(',')[1].Replace(",", string.Empty));
                    return 0;
                }
            }

            public static bool HasRecord() { return (!string.IsNullOrWhiteSpace(WeatherDesktop.Share.SharedObjects.AppSettings.ReadSetting(csvEncryptedLatLongName))); }
            public static void Set(double dLat, double dLng)
            {
                WeatherDesktop.Share.SharedObjects.AppSettings.AddUpdateAppSettings(csvEncryptedLatLongName, string.Join(",", dLat, dLng));
            }

        }

        #region Web Request
        public static string CompressedCallSite(string Url)
        {
            return CompressedCallSite(Url, string.Empty);

        }
        public static string CompressedCallSite(string Url, string UserAgent)
        {
            HttpWebRequest request = (System.Net.HttpWebRequest)HttpWebRequest.Create(Url);
            if (!string.IsNullOrWhiteSpace(UserAgent)) { request.UserAgent = UserAgent; }
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.AllowAutoRedirect = true;

            using (System.Net.WebResponse response = request.GetResponse())
            {
                StreamReader Reader = new System.IO.StreamReader(response.GetResponseStream());
                return Reader.ReadToEnd();
            }

        }

        #endregion

        public static bool BetweenTimespans(TimeSpan test, TimeSpan LowerValue, TimeSpan Highervalue) { return (LowerValue < test && test < Highervalue); }

        public static string CompileDebug(System.Collections.Generic.Dictionary<string, string> ItemsTodisplay)
        {
            StringBuilder SB = new StringBuilder();
            SB.Append(Environment.NewLine);
            foreach (var item in ItemsTodisplay)
            {
                SB.Append(item.Key).Append(": ").Append(item.Value).Append(Environment.NewLine);
            }
            SB.Append(Environment.NewLine);
            return SB.ToString();
        }

        /// <summary>
        /// Bitarray should be under 32 bits
        /// </summary>
        /// <param name="Array"></param>
        /// <returns></returns>
        public static int ConvertBitarrayToInt(System.Collections.BitArray Array)
        {
            int[] array = new int[1];
            Array.CopyTo(array, 0);
            return array[0];
        }

        public static System.Collections.BitArray ConverTIntToBitArray(int item)
        {
            return new System.Collections.BitArray(new int[] { item });
        }

    }
}
