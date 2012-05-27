using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.Xml;
using System.ServiceModel.Syndication;

namespace SBGameSales
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Game Sales";
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
                return "Gets sales on games.";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

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
            if ( command == "steam" )
            {
                string sale = PollSteamSale();
                if ( sale == null )
                {
                    Host.PluginResponse(channel, "Error getting Steam sales.");
                    return;
                }
                Host.PluginResponse(channel, "Poor Man's Steam Sale of the Day: " + sale);
                Host.PluginResponse(channel, "Game Sale Pulling is currently being overhauled");
            }
        }

        private string PollSteamSale()
        {
            XmlReader r = XmlReader.Create("http://steamsales.rhekua.com/feed.php");

            SyndicationFeed f = SyndicationFeed.Load(r);

            string result = "";

            foreach ( SyndicationItem i in f.Items )
            {
                result = i.Title.Text;
                foreach ( SyndicationLink l in i.Links )
                {
                    result = result + " (" + l.Uri.ToString() + ")";
                    break;
                }
                return result;
            }

            return null;
        }

        private string[] PollSales()
        {
            XmlReader r = XmlReader.Create("http://steamsales.rhekua.com/feed.php");
            SyndicationFeed f = SyndicationFeed.Load(r);

            List<string> Sales = new List<string>();

            string result = "";

            foreach ( SyndicationItem i in f.Items )
            {
                result = i.Title.Text;
                foreach ( SyndicationLink l in i.Links )
                {
                    result = result + " (" + l.Uri.ToString() + ")";
                    break;
                }

                Sales.Add(result);

                result = "";
            }

            if ( Sales.Count > 0 )
            {
                return Sales.ToArray();
            }

            return null;
        }
    }
}
