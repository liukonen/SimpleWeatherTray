using System;
using System.IO;
using System.Text;

namespace WeatherDesktop.Share
{
    internal class EventLog
    {
        public enum EventLogEntryType { Warning, Error}
        internal static bool SourceExists(string sSource)
        {
            if (!Directory.Exists("err")) { return false; }
            return File.Exists("err" + Path.DirectorySeparatorChar + sSource.GetHashCode() + ".log");
        }

        internal static void CreateEventSource(string sSource, string sLog)
        {
            if (!Directory.Exists("err")) { Directory.CreateDirectory("err"); }
            File.WriteAllText("err" + Path.DirectorySeparatorChar + sSource.GetHashCode() + ".log",
                sSource + Environment.NewLine + sLog + Environment.NewLine + "Source, Message, EventLogType, V1,V2, Raw" + Environment.NewLine);
        }

        internal static void WriteEntry(string sSource, string message, EventLogEntryType error, int v1, int v2, byte[] raw)
        {

            File.AppendAllText("err" + Path.DirectorySeparatorChar + sSource.GetHashCode(),
                String.Join(",", sSource, message, (error == EventLogEntryType.Error) ? "Error" : "Warning", v1, v2, Encoding.ASCII.GetString(raw, 0, raw.Length))); ;
        }
    }
}