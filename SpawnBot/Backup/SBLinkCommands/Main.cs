using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using ConfigManager;

namespace SBLinkCommands
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private Dictionary<string, string> cmds = new Dictionary<string, string>();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBLinkCommands";
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

                LoadDictionary();
                Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        private bool LoadDictionary()
        {
            List<Config> list = Host.PluginConfigManager.Load("sblinkcommands", "cmds.cfg");

            cmds.Clear();

            if ( list == null )
            {
                return false;
            }

            foreach ( Config l in list )
            {
                cmds.Add(l.Index, l.Value);
            }

            return true;
        }

        private void SaveDictionary()
        {
            List<Config> list = new List<Config>();

            foreach ( KeyValuePair<string, string> Peeve in cmds )
            {
                list.Add(new Config(Peeve.Key, Peeve.Value));
            }

            Host.PluginConfigManager.Save(list, "sblinkcommands", "cmds.cfg");
        }

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            switch ( command )
            {
                case "link":
                    if ( Host.PluginUserManager.IsOperator(name, channel) )
                    {
                        if ( parameters.Length == 2 )
                        {
                            if ( !cmds.ContainsKey(parameters[0]) )
                            {
                                cmds.Add(parameters[0], parameters[1]);
                                SaveDictionary();
                                Host.PluginResponse(channel, String.Format("'!{0}' has been added. To unlink: !unlink {0}", parameters[0]));
                            }
                            else
                            {
                                Host.PluginResponse(channel, String.Format("{0} is already linked!", parameters[0]));
                            }
                        }
                        else
                        {
                            Host.PluginResponse(channel, "Usage: !link [cmd] [msg]");
                        }
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Only @'s can do that");
                    }

                    break;

                case "unlink":
                    if ( Host.PluginUserManager.IsOperator(name, channel) )
                    {
                        if ( parameters.Length == 1 )
                        {
                            if ( cmds.ContainsKey(parameters[0]) )
                            {
                                cmds.Remove(parameters[0]);
                                SaveDictionary();
                                Host.PluginResponse(channel, "Removed!");
                            }
                            else
                            {
                                Host.PluginResponse(channel, String.Format("{0} doesn't exist!", parameters[0]));
                            }
                        }
                        else
                        {
                            Host.PluginResponse(channel, "Usage: !unlink [cmd]");
                        }
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Only @'s can do that");
                    }
                    break;
                default:
                    if ( cmds.ContainsKey(command) )
                    {
                        Host.PluginResponse(channel, cmds[command]);
                    }
                    break;
            }
        }

    }
}
