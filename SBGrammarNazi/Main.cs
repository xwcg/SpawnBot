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
            if ( message.Contains("would of ") )
            {
                Host.PluginResponse(channel, "*would've/would have");
            }

            if ( message.Contains("should of ") )
            {
                Host.PluginResponse(channel, "*should've/should have");
            }

            if ( message.Contains("could of ") )
            {
                Host.PluginResponse(channel, "*could've/could have");
            }
        }
    }
}
