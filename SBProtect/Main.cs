using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

namespace SBProtect
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        List<UserCache> msgCache = new List<UserCache>();
        List<ProtectedChannels> protChans = new List<ProtectedChannels>();


        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBProtect";
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
                return "alefnull";
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

                Host.eventPluginChannelMessageReceived += new ChannelMessage(Host_eventPluginChannelMessageReceived);
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelMessageReceived(string name, string message, string channel)
        {
            if (channel.Contains("spawn"))
            {
                msgCache.Add(new UserCache
                            {
                                Nick = name,
                                Channel = channel,
                                Time = DateTime.Now
                            });
                msgCache.RemoveAll(x => (DateTime.Now - x.Time).TotalSeconds > 5);
                var c = msgCache.Count(x => x.Nick == name && x.Channel == channel);
                if (c > 4) Host.PluginKick(channel, name, "Slow your roll! (No more flooding, please.)");
            }
        }

    }

    internal class UserCache
    {
        public string Nick { get; set; }
        public string Channel { get; set; }
        public DateTime Time { get; set; }
    }

    internal class ProtectedChannels
    {
        public string Channel { get; set; }
    }
}
