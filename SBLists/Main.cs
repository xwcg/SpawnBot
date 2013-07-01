using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using ConfigManager;

namespace SBLists
{
    public class Main : SBPlugin
    {
        private List<string> lists_index = new List<string>();
        private Dictionary<string, List<string>> lists = new Dictionary<string, List<string>>();

        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBLists";
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
        }

        bool LoadLists()
        {
            List<Config> cLists = Host.PluginConfigManager.Load(this.PluginName, "index_lists.cfg");

            lists_index.Clear();

            if (cLists == null)
                return false;

            foreach (Config l in cLists)
            {
                List<Config> thisList = Host.PluginConfigManager.Load(this.PluginName, String.Format("{0}.cfg", l.Value));

                if (thisList == null)
                    continue;
            }

            return true;

            /*
             List<Config> list = Host.PluginConfigManager.Load(this.PluginName, "science.cfg");

            Links.Clear();

            if ( list == null )
            {
                return false;
            }

            foreach ( Config l in list )
            {
                Links.Add(l.Value.Replace(replacer, "="));
            }

            return true;
             */
        }

    }
}
