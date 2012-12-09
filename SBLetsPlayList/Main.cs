using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.IO;
using xLogger;
using ConfigManager;

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

namespace SBLetsPlayList
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private static List<string> LPs = new List<string>();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Let's Play List Manager";
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
                return "List for managing Let's Play links";
            }
        }

        public void Dispose()
        {
            return;
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
                    Logger.WriteLine("*** LP.TXT not found. Ignoring.", ConsoleColor.Yellow);
                }

            }
        }

        #endregion

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            if ( command == "lp" )
            {
                if ( parameters.Length > 0 )
                {
                    if ( parameters[0].Trim() == "add" )
                    {
                        if ( !Host.PluginUserManager.IsOperator(name, "#spawn") )
                        {
                            Host.PluginResponse(channel, "Only @'s can do that.");
                            return;
                        }

                        if ( parameters.Length == 3 )
                        {
                            if ( AddLp(parameters[1], parameters[2]) == true )
                            {
                                Host.PluginResponse(channel, String.Format("{0} has been added!", parameters[1]));
                            }
                            else
                            {
                                Host.PluginResponse(channel, String.Format("Error while adding {0}", parameters[1]));
                            }
                        }
                        else
                        {
                            Host.PluginResponse(channel, "Invalid parameters. Usage: !lp add [name] [youtube url]");
                        }
                    }
                    else if ( parameters[0].Trim() == "del" || parameters[0].Trim() == "remove" )
                    {
                        if ( !Host.PluginUserManager.IsOperator(name, "#spawn") )
                        {
                            Host.PluginResponse(channel, "Only @'s can do that.");
                            return;
                        }

                        if ( parameters.Length == 2 )
                        {
                            DelLp(parameters[1], channel);
                        }
                        else
                        {
                            Host.PluginResponse(channel, "Invalid parameters. Usage: !lp remove|del [name]");
                        }
                    }
                    else if ( parameters[0].Trim() == "list" )
                    {
                        Host.PluginResponse(channel, LpList());
                    }
                    else
                    {
                        Host.PluginResponse(channel, GetLp(parameters[0]));
                    }
                }
                else
                {
                    Host.PluginResponse(channel, LpList());
                }
            }
        }

        private bool LoadList()
        {
            List<Config> list = Host.PluginConfigManager.Load("sbletsplaylist", "list.cfg");

            if ( list == null )
            {
                return false;
            }

            foreach ( Config l in list )
            {
                LPs.Add(String.Format("{0} {1}", l.Index, l.Value));
            }

            return true;
        }

        private string LpList()
        {
            string response = "List of Let's Plays: ";

            foreach ( string lp in LPs )
            {
                string[] slp = lp.Split(' ');
                response = response + slp[0] + " ";
            }

            response = response + "( !lp [name] for more info )";

            return response;
        }

        private string GetLp( string name )
        {
            string lp = LPs.Find(delegate( string l )
            {
                return l.StartsWith(name, StringComparison.CurrentCultureIgnoreCase);
            });

            if ( lp == null || lp.Trim().Length == 0 )
            {
                return "I don't have any information on " + name;
            }
            else
            {
                return lp;
            }
        }

        private bool AddLp( string name, string ur )
        {
            try
            {
                string newlp = String.Format("{0} {1}", name, ur);
                LPs.Add(newlp);
                LPs.Sort();

                SaveLP();

                return true;
            }
            catch ( Exception e )
            {
                Logger.WriteLine(e.Message);
                return false;
            }
        }

        private void DelLp( string name, string Channel )
        {
            try
            {
                int i = LPs.FindIndex(delegate( string l )
                {
                    return l.StartsWith(name, StringComparison.CurrentCultureIgnoreCase);
                });

                if ( i < 0 )
                {
                    Host.PluginResponse(Channel, "I couldn't find " + name + ". Did you check your spelling?");
                    return;
                }

                LPs.RemoveAt(i);
                SaveLP();

                Host.PluginResponse(Channel, name + " has been removed");
            }
            catch ( Exception e )
            {
                Logger.WriteLine("***** " + e.Message, ConsoleColor.DarkRed);
                Host.PluginResponse(Channel, "Something has gone horribly wrong.");
            }
        }

        private void SaveLP()
        {
            List<Config> cfg = new List<Config>();

            foreach ( string lp in LPs )
            {
                string[] p = lp.Split(' ');
                cfg.Add(new Config(p[0], p[1]));
            }

            Host.PluginConfigManager.Save(cfg, "sbletsplaylist", "list.cfg");
        }

    }
}
