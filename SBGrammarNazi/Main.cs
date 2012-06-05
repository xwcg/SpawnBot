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

namespace SBGrammarNazi
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Grammar Nazi";
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
                return "Corrects my grammar pet peeves";
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

        void Host_eventPluginChannelMessageReceived( string name, string message, string channel )
        {
            string response = "";

            if ( message.ToLower().Contains("would of") || message.ToLower().Contains("woud of") )
            {
                //Host.PluginResponse(channel, "*would've/would have");
                response += "*would've/would have ";
            }

            if ( message.ToLower().Contains("should of") || message.ToLower().Contains("shoud of") )
            {
                //Host.PluginResponse(channel, "*should've/should have");
                response += "*should've/should have ";
            }

            if ( message.ToLower().Contains("could of") || message.ToLower().Contains("coud of") )
            {
                //Host.PluginResponse(channel, "*could've/could have");
                response += "*could've/could have";
            }

            if ( response.Length > 5 )
            {
                Host.PluginResponse(channel, response);
            }
        }
    }
}
