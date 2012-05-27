using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

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
