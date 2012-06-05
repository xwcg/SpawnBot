using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

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

namespace SBSpawnPlugins
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBSpawnPlugins";
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
                return "Provides some small info tidbits for #spawn";
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
            switch ( command )
            {
                case "ranks":
                    if ( channel.Contains("mhykol") )
                    {
                        Host.PluginResponse(channel, "http://goo.gl/88F58");
                    }
                    break;
                case "server":
                    Host.PluginResponse(channel, "The Spawn.net Server IP is: 127.0.0.1");
                    break;
                case "mumble":
                    Host.PluginResponse(channel, "Mumble Address: mumble.thespawn.net - Port: 64738");
                    break;
            }
        }

        void Host_eventPluginChannelMessageReceived( string name, string message, string channel )
        {
        }

    }
}
