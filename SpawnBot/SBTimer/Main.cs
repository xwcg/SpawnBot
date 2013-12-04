using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

namespace SBTimer
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private SBTimegiver.Main Timegiver;
        private Dictionary<string, DateTime> Timers = new Dictionary<string, DateTime>();


        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBTimer";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public string Author
        {
            get
            {
                return "Watashi";
            }
        }

        public string Website
        {
            get
            {
                return "";
            }
        }

        public string Description
        {
            get
            {
                return "Description";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;
                
                this.Timegiver = (SBTimegiver.Main)Host.FindPlugin("SBTimegiver");

                if (this.Timegiver == null)
                    xLogger.Logger.WriteLine("Timegiver not found", ConsoleColor.Red);
                else
                {
                    xLogger.Logger.WriteLine("Timegiver found!", ConsoleColor.Green);

                    Timegiver.onSecond += Timegiver_onSecond;
                    Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
                }
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Timegiver_onSecond()
        {
            DateTime now = DateTime.Now;
            foreach (KeyValuePair<string, DateTime> t in Timers)
            {
                if (t.Value <= now)
                {
                    string[] vals = t.Key.Split(';');

                    Host.PluginResponse(vals[0], String.Format("{0}, your timer '{1}' has expired!", vals[1], vals[2]));

                    Timers.Remove(t.Key);
                }
            }
        }

        void Host_eventPluginChannelCommandReceived(string name, string channel, string command, string[] parameters)
        {
            if (command == "timer")
            {
                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "add":
                            if (parameters.Length >= 2)
                            {
                                string makeKey = parameters[1];

                                string seconds = makeKey.Split('s')[0];
                                int secs = Convert.ToInt32(seconds);

                                Timers.Add(String.Format("{0};{1};{2}", channel, name, "testdummyname"), DateTime.Now.AddSeconds(secs));

                                Host.PluginResponse(channel, String.Format("Timer with duration of {0} seconds added!", seconds));

                            }
                            break;
                    }
                }
            }
        }


    }
}
