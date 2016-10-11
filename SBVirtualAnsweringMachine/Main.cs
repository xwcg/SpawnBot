using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

namespace SBVirtualAnsweringMachine
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        private Dictionary<string, Dictionary<string, string>> Messages = new Dictionary<string, Dictionary<string, string>>();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBVirtualAnsweringMachine";
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

                Host.eventPluginChannelMessageReceived += Host_eventPluginChannelMessageReceived;
                Host.eventPluginUserJoined += Host_eventPluginUserJoined;
                Host.eventPluginUserChangedNick += Host_eventPluginUserChangedNick;

                Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelCommandReceived(string name, string channel, string command, string[] parameters)
        {
            if (command == "tell")
            {
                if (parameters.Length > 1)
                {
                    string target = parameters[0].ToLower();
                    string message = String.Join(" ", parameters, 1, parameters.Length - 1);

                    if (!Messages.ContainsKey(target))
                    {
                        Messages.Add(target, new Dictionary<string, string>());
                    }

                    if (!Messages[target].ContainsKey(name))
                    {
                        Messages[target].Add(name, message);
                    }
                    else
                    {
                        Messages[target][name] = message;
                    }

                    Host.PluginResponse(channel, String.Format("I will tell {0} about that the next time they are active.", target));
                }
                else
                {
                    Host.PluginResponse(channel, "Usage: !tell [Username] [Message]");
                }
            }
        }

        void Host_eventPluginChannelMessageReceived(string name, string message, string channel)
        {
            Activity(name);
        }

        void Host_eventPluginUserJoined(string channel, string name)
        {
            Activity(name);
        }

        void Host_eventPluginUserChangedNick(string name, string newname)
        {
            Activity(newname);
        }


        private void Activity(string name)
        {
            if (Messages.ContainsKey(name.ToLower()))
            {
                Dictionary<string, string> msgs = Messages[name.ToLower()];

                Host.PluginResponse(name, String.Format("You have {0} new messages!", msgs.Count));

                foreach (KeyValuePair<string, string> msg in msgs)
                {
                    Host.PluginResponse(name, String.Format("{0}: {1}", msg.Key, msg.Value));
                }

                Messages.Remove(name.ToLower());
            }
        }
    }
}
