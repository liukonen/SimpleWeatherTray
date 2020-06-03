using System;
using System.Xml;
using WeatherDesktop.Share;
using System.Xml.Serialization;
using System.Collections.Generic;
using WeatherDesktop.Interface;
using System.Linq;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace ExternalService
{

    public class OpenDataFlatFileLookup : ILatLongInterface
    {
        //private bool _worked;
        private string Cache;

        public double Latitude()
        {
            string[] LatLong = Cache.Split(',');
            return double.Parse(LatLong[0].Replace(",", string.Empty));
        }

        public double Longitude()
        {
            string[] LatLong = Cache.Split(',');
            return double.Parse(LatLong[1].Replace(",", string.Empty));
        }

        public bool worked()
        {
            throw new NotImplementedException();
        }

        public OpenDataFlatFileLookup()
        {
            const string OriginalFile = "us-zip-code-latitude-and-longitude.csv";
            const string CompressedFile = "us-zip-code-latitude-and-longitude.csv.gz";

            string strFile = string.Empty;

            //Reload the DB if we need to
            if (System.IO.File.Exists(OriginalFile))
            {
                strFile = System.IO.File.ReadAllText(OriginalFile);
                compress(CompressedFile, strFile);
            }
            if (string.IsNullOrWhiteSpace(strFile))
            {
                strFile = decompress(CompressedFile);
            }

            var zipcode = SharedObjects.ZipObjects.GetZip();

            foreach (var item in strFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var ziprow = new ZipRowItem(item);
                if (ziprow.Zipcode == zipcode) { Cache =  string.Join(",", ziprow.Latitude, ziprow.Longitude);break; }
            }
            

            //Cache = (from string Row in strFile.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            //         let zipRow = new ZipRowItem(Row)
            //         where zipRow.Zipcode == zipcode
            //         select string.Join(",", zipRow.Latitude, zipRow.Longitude)).First();





            string Zip = SharedObjects.ZipObjects.Rawzip;
            if (string.IsNullOrEmpty(Zip)) { Zip = SharedObjects.ZipObjects.GetZip(); }

            //Since this should be a one and done, no point in building an entire colleciton... iterate, find, and exit

        }

        private static string decompress(string fileName)
        {
            byte[] Bytes;
            using (var gZipBuffer = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var gZipStream = new GZipStream(gZipBuffer, CompressionMode.Decompress))
            using (var Mem = new System.IO.MemoryStream())
            {
                gZipStream.CopyTo(Mem);
                Bytes = Mem.ToArray();
            }
            return Encoding.UTF8.GetString(Bytes);
        }


        private static void compress(string fileName, string context)
        {
            using (var FileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(context)))
            using (var compressorStream = new GZipStream(FileStream, CompressionMode.Compress))
            {
                uncompressedStream.CopyTo(compressorStream);
            }


        }

        private class ZipRowItem
        {
            public string Zipcode = string.Empty;
            public string CityName = string.Empty;
            public string State = string.Empty;
            public double Latitude = 0;
            public double Longitude = 0;
            public short Timezone = 0;
            public Boolean DaylightSavings = false;

            public ZipRowItem(string item)
            {
                if (!item.StartsWith("Zip") && !string.IsNullOrWhiteSpace(item))
                {
                    //Example string 71937;Cove;AR;34.398483;-94.39398;-6;1;34.398483,-94.39398
                    string[] items = item.Split(';');
                    Zipcode = items[0];
                    CityName = items[1];
                    State = items[2];
                    Latitude = double.Parse(items[3]);
                    Longitude = double.Parse(items[4]);
                    Timezone = short.Parse(items[5]);
                    DaylightSavings = (items[6] == "1");
                }
            }
        }

    }
}

