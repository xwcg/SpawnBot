using SBPluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xLogger;

/*
    Copyright 2013 Eric Socia
  
    This file is part of SpawnBot.

    SpawnBot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    SpawnBot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with SpawnBot.  If not, see <http://www.gnu.org/licenses/>.
 * 
*/

namespace SBProtect
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        List<UserCache> msgCache = new List<UserCache>();
        List<String> protChans = new List<String>();
        public string chanToAdd;

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
                Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelCommandReceived(string name, string channel, string command, string[] parameters)
        {
            #region !command system
            if (command.ToLower() == "protect" || command.ToLower() == "protection")
            {
                if (Host.PluginUserManager.IsOperator(Host.PluginBotname, channel))
                {
                    if (Host.PluginUserManager.IsOperator(name, channel))
                    {
                        if (parameters.Length > 0)
                        {
                            string subCommand = parameters[0];
                            switch (subCommand)
                            {
                                case "start":
                                case "add":
                                    {
                                        #region !protect add
                                        protChans.Add(channel);
                                        Host.PluginResponse(channel, channel + " is now under my protection.");
                                        #endregion
                                    }
                                    break;

                                case "stop":
                                case "del":
                                case "remove":
                                    {
                                        #region !protect remove
                                        protChans.Remove(channel);
                                        Host.PluginResponse(channel, channel + " is no longer under my protection.");
                                        #endregion
                                    }
                                    break;

                                case "list":
                                    {
                                        #region !protect list
                                        string chanList = string.Join(", ", protChans.ToArray());
                                        Host.PluginResponse(channel, "Channels under my protection: " + chanList);
                                        #endregion
                                    }
                                    break;

                                default:
                                    {
                                        #region !protect <usage>
                                        Host.PluginResponse(channel, "Usage: !protect <add|remove|list>");
                                        #endregion
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            Host.PluginResponse(channel, "Usage: !protect <add|remove|list>");
                        }
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Only @'s can do that");
                    }
                }
                else
                {
                    Host.PluginResponse(channel, "No changes made. I need @ in a channel to manage it's protection.");
                }
            }
            #endregion
        }

        void Host_eventPluginChannelMessageReceived(string name, string message, string channel)
        {
            if (Host.PluginUserManager.IsOperator(Host.PluginBotname, channel))
            {
                if (!Host.PluginUserManager.IsOperator(name, channel))
                {
                    if (protChans.Contains(channel))
                    {
                        msgCache.Add(new UserCache
                            {
                                Nick = name,
                                Channel = channel,
                                Time = DateTime.Now
                            });

                        int c = msgCache.Count(x => x.Nick == name && x.Channel == channel);

                        if (c > 4)
                        {
                            Host.PluginKick(channel, name, "Slow your roll! (No more flooding, please.)");
                            msgCache.RemoveAll(x => x.Nick == name);
                        }
                    }
                    msgCache.RemoveAll(x => (DateTime.Now - x.Time).TotalSeconds >= 5);
                }
            }
        }



        internal class UserCache
        {
            public string Nick { get; set; }
            public string Channel { get; set; }
            public DateTime Time { get; set; }
        }

    }
}
