using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using ConfigManager;
using xLogger;

namespace SBQuake
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        private List<string> QuakePlayers = new List<string>();
        private List<string> QuakePhrases = new List<string>();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBQuake";
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

                if ( LoadList() == true )
                {
                    Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);
                }
                else
                {
                    Logger.WriteLine("*** PLAYERS.TXT not found. Ignoring.", ConsoleColor.Yellow);
                }
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        private bool LoadList()
        {
            List<Config> list = Host.PluginConfigManager.Load("sbquake", "players.cfg");

            if ( list == null )
            {
                return false;
            }

            foreach ( Config l in list )
            {
                string[] p = l.Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach ( string x in p )
                {
                    QuakePlayers.Add(x);
                }
            }

            list = Host.PluginConfigManager.Load("sbquake", "phrases.cfg");
            
            if ( list == null )
            {
                return false;
            }

            foreach ( Config l in list )
            {
                QuakePhrases.Add(l.Value);
            }

            return true;
        }

        private void AddPhrase( string txt )
        {
            QuakePhrases.Add(txt);
            QuakePhrases.Sort();

            List<Config> cfg = new List<Config>();

            foreach ( string p in QuakePhrases )
            {
                cfg.Add(new Config(cfg.Count.ToString(), p));
            }

            Host.PluginConfigManager.Save(cfg, "sbquake", "phrases.cfg");
        }

        private bool SignUp( string name )
        {
            if ( QuakePlayers.Contains(name) )
            {
                return false;
            }

            QuakePlayers.Add(name);
            QuakePlayers.Sort();

            string playerlist = "";
            foreach ( string player in QuakePlayers )
            {
                playerlist += player + " ";
            }

            playerlist = playerlist.Trim();

            List<Config> c = new List<Config>();
            Config ci = new Config("players", playerlist);
            c.Add(ci);

            Host.PluginConfigManager.Save(c, "sbquake", "players.cfg");

            return true;
        }

        private bool SignDown( string name )
        {
            if ( !QuakePlayers.Contains(name) )
            {
                return false;
            }

            QuakePlayers.Remove(name);
            QuakePlayers.Sort();

            string playerlist = "";
            foreach ( string player in QuakePlayers )
            {
                playerlist += player + " ";
            }

            playerlist = playerlist.Trim();

            List<Config> c = new List<Config>();
            Config ci = new Config("players", playerlist);
            c.Add(ci);

            Host.PluginConfigManager.Save(c, "sbquake", "players.cfg");

            return true;
        }

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            if ( command.ToLower() == "ql" )
            {
                if ( parameters.Length == 0 )
                {
                    string response = "";

                    Random r = new Random();

                    response += QuakePhrases[r.Next(0, QuakePhrases.Count)] + " ";

                    foreach ( string player in QuakePlayers )
                    {
                        response += player + " ";
                    }

                    Host.PluginResponse(channel, response);
                }
                else
                {
                    switch ( parameters[0].ToLower() )
                    {
                        case "signup":
                            if ( SignUp(name) )
                            {
                                Host.PluginResponse(channel, "Added you to the QuakeLive list!");
                            }
                            else
                            {
                                Host.PluginResponse(channel, "You already are on the QuakeLive list!");
                            }
                            break;

                        case "remove":
                            if ( SignDown(name) )
                            {
                                Host.PluginResponse(channel, "You have been removed from the list! (Traitor)");
                            }
                            else
                            {
                                Host.PluginResponse(channel, "You aren't on the QuakeLive list! Use '!ql signup' to add yourself.");
                            }
                            break;

                        case "addtaunt":
                            if ( !Host.PluginUserManager.IsOperator(name, "#spawn") )
                            {
                                Host.PluginResponse(channel, "Only #spawn OPs can do this");
                                return;
                            }

                            if ( parameters.Length > 1 )
                            {
                                string taunt = "";
                                for ( int i = 1; i < parameters.Length; i++ )
                                {
                                    taunt += parameters[i] + " ";
                                }

                                AddPhrase(taunt.Trim());

                                Host.PluginResponse(channel, "Taunt added!");
                            }
                            else
                            {
                                Host.PluginResponse(channel, "There needs to be a phrase you dolt!");
                            }
                            break;

                        default:
                            Host.PluginResponse(channel, "!ql to sound the Quakehorn; !ql signup, to add yourself; !ql remove, to remove yourself");
                            break;
                    }
                }
            }
        }

    }
}
