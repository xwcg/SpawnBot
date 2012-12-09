using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using xLogger;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

/*
    Copyright 2012 Michael Schwarz
  
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
 */

namespace SBTitler
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Website Title Parser";
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
                return "Michael Schwarz";
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
                return "Returns website titles on demand and automatically on youtube/vimeo links";
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

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            if ( name.ToLower().Trim() == "eru" )
            {
                return;
            }

            switch ( command )
            {
                case "title":
                    if ( parameters.Length == 1 )
                    {
                        string title = PollWebsiteTitle(parameters[0]);
                        if ( title == null )
                        {
                            Host.PluginResponse(channel, "Error getting website title. (Probably a timeout)");
                            break;
                        }
                        Host.PluginResponse(channel, "Title: " + title);
                    }
                    break;
            }
        }

        void Host_eventPluginChannelMessageReceived( string name, string message, string channel )
        {
            if ( name.ToLower().Trim() == "eru" )
            {
                return;
            } 

            if ( message.Contains("youtube.com") || message.Contains("youtu.be") || message.Contains("vimeo.com") )
            {
                string[] Parameters = message.Split(' ');
                foreach ( string p in Parameters )
                {
                    if ( p.Contains("youtube.com/watch?") || p.Contains("youtu.be/") || p.Contains("vimeo.com/") )
                    {
                        string title = PollWebsiteTitle(p);
                        if ( title == null )
                        {
                            break;
                        }
                        Host.PluginResponse(channel, "Title: " + title);
                        break;
                    }

                    if ( p.Contains("youtube.com/playlist?list=") )
                    {
                        string title = PollWebsiteTitle(p);
                        if ( title == null )
                        {
                            break;
                        }
                        Host.PluginResponse(channel, "Playlist Title: " + title);
                        break;
                    }
                }
            }
        }

        private string PollWebsiteTitle( string url )
        {
            try
            {
                Logger.WriteLine("* Polling Title for " + url, ConsoleColor.Yellow);

                string cUrl = url.Trim();
                if ( ( !cUrl.StartsWith("http://") && !cUrl.StartsWith("https://") ) && !cUrl.StartsWith("www.") )
                {
                    cUrl = "http://www." + cUrl;
                }

                if ( cUrl.StartsWith("www.") )
                {
                    cUrl = "http://" + cUrl;
                }

                WebClient x = new WebClient();
                string source = x.DownloadString(new Uri(cUrl));
                string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                x.Dispose();

                title = HttpUtility.HtmlDecode(title);

                return title;
            }
            catch ( Exception e )
            {
                Logger.WriteLine("***** " + e.Message, ConsoleColor.DarkRed);
                return null;
            }
        }
    }
}
