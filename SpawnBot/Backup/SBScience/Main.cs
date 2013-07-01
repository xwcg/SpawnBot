using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using ConfigManager;
using xLogger;

namespace SBScience
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private List<string> Links = new List<string>();
        string replacer = "<->equal<->";

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBScience";
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

                Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);

                if ( LoadScience() == false )
                {
                    Logger.WriteLine("*** Science not found. Ignoring common sense.", ConsoleColor.Yellow);
                }
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            if ( command == "science" )
            {
                if ( parameters.Length > 0 )
                {
                    switch ( parameters[0] )
                    {
                        case "add":
                            if ( parameters.Length < 2 )
                            {
                                Host.PluginResponse(channel, "You need to provide science to add!");
                                return;
                            }

                            string sci = String.Join(" ", parameters).Substring(4);
                            Links.Add(sci);
                            SaveScience();

                            Host.PluginResponse(channel, "Science added!");
                            break;

                        case "remove":
                        case "del":
                        case "delete":
                            if ( parameters.Length < 2 )
                            {
                                Host.PluginResponse(channel, "You need to provide a science number to delete!");
                                return;
                            }

                            int i = -1;
                            try
                            {
                                int u = Convert.ToInt32(parameters[1]);
                                Links.Remove(Links[u]);
                                SaveScience();
                                Host.PluginResponse(channel, "Science has been deleted!");
                            }
                            catch
                            {
                                Host.PluginResponse(channel, "You provided an incorrect science number :(");
                            }

                            break;
                    }
                }
                else
                {
                    if ( Links.Count > 0 )
                    {
                        Random r = new Random();
                        int i = r.Next(0, Links.Count);
                        string instantScience = Links[i];

                        Host.PluginResponse(channel, String.Format("Warm Cup of Instant Science No. {0}: {1}", i, instantScience));
                    }
                    else
                    {
                        Host.PluginResponse(channel, "There is no Science :(");
                    }
                }
            }
        }

        private bool LoadScience()
        {
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
        }

        private void SaveScience()
        {
            List<Config> list = new List<Config>();

            UInt16 i = 0;
            foreach ( string sci in Links )
            {
                list.Add(new Config(i.ToString(), sci.Replace("=", replacer)));
                i++;
            }

            Host.PluginConfigManager.Save(list, this.PluginName, "science.cfg");
        }

    }
}
