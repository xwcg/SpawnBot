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
        private List<TimerInfo> Timers = new List<TimerInfo>();

        private class TimerInfo
        {
            public string Channel;
            public string Username;
            public string Name;
            public DateTime EndTime;
            public Boolean Expired;
        }

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

                if (Host.PluginTimegiver != null)
                {
                    Host.PluginTimegiver.onSecond += PluginTimegiver_onSecond;
                    Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
                }

                //this.Timegiver = (SBTimegiver.Main)Host.FindPlugin("SBTimegiver");

                //if (this.Timegiver == null)
                //    xLogger.Logger.WriteLine("Timegiver not found", ConsoleColor.Red);
                //else
                //{
                //    xLogger.Logger.WriteLine("Timegiver found!", ConsoleColor.Green);

                //    Timegiver.onSecond += Timegiver_onSecond;
                //    Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
                //}
            }
        }


        public void Dispose()
        {
            return;
        }

        #endregion

        void PluginTimegiver_onSecond()
        {
            DateTime now = DateTime.Now;

            for (int i = 0; i < Timers.Count; i++)
            {
                TimerInfo timer = Timers[i];
                if (!timer.Expired && timer.EndTime.Ticks <= now.Ticks)
                {
                    Host.PluginResponse(timer.Channel, String.Format("{0}, your timer '{1}' has elapsed!", timer.Username, timer.Name));
                    Timers[i].Expired = true;
                }
            }
        }
        
        void Host_eventPluginChannelCommandReceived(string name, string channel, string command, string[] parameters)
        {
            // !timer add "Tea Timer" 1h10m10s
            if (command == "timer")
            {
                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "add":
                            if (parameters.Length >= 3)
                            {
                                try
                                {
                                    string timerName = parameters[1].Trim(new char[] { '"' });
                                    string timeString = parameters[2];

                                    string[] hourBits = timeString.Split('h');
                                    string minuteString = hourBits.Length == 1 ? hourBits[0] : hourBits[1];

                                    string[] minuteBits = minuteString.Split('m');
                                    string secondString = minuteBits.Length == 1 ? minuteBits[0] : minuteBits[1];

                                    string[] secondBits = secondString.Split('s');

                                    string sHour = hourBits.Length == 1 ? null : hourBits[0];
                                    string sMinute = minuteBits.Length == 1 ? null : minuteBits[0];
                                    string sSecond = secondBits.Length == 1 ? null : secondBits[0];

                                    DateTime endTime = DateTime.Now;

                                    if (sHour != null)
                                        endTime = endTime.AddHours(Convert.ToInt32(sHour));
                                    if (sMinute != null)
                                        endTime = endTime.AddMinutes(Convert.ToInt32(sMinute));
                                    if (sSecond != null)
                                        endTime = endTime.AddSeconds(Convert.ToInt32(sSecond));

                                    TimerInfo newTimer = new TimerInfo();
                                    newTimer.Channel = channel;
                                    newTimer.Username = name;
                                    newTimer.Name = timerName;
                                    newTimer.EndTime = endTime;
                                    newTimer.Expired = false;

                                    xLogger.Logger.WriteLine(String.Format("Now: {0}, Expiry: {1}", DateTime.Now.ToLongTimeString(), endTime.ToLongTimeString()));

                                    Timers.Add(newTimer);

                                    Host.PluginResponse(channel, String.Format("Your timer \"{0}\" with a duration of{1}{2}{3} has been added!", timerName,
                                        (sHour != null ? String.Format(" {0} hours", sHour) : ""),
                                        (sMinute != null ? String.Format(" {0} minutes", sMinute) : ""),
                                        (sSecond != null ? String.Format(" {0} seconds", sSecond) : "")));
                                }
                                catch (Exception ex)
                                {
                                    Host.PluginResponse(channel, String.Format("Error while adding timer: {0}", ex.Message));
                                }

                            }
                            break;
                    }
                }
            }
        }


    }
}
