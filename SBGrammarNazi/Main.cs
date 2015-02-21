using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.Text.RegularExpressions;
using ConfigManager;
using xLogger;

/*
    Copyright 2012-2015 Michael Schwarz
  
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
        private Dictionary<string, string> PeeveDict = new Dictionary<string, string>();

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
                Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);

                if ( this.LoadDictionary() == false )
                {
                    Logger.WriteLine("*** Grammar Pet Peeve Dictionary not found. Ignoring.", ConsoleColor.Yellow);
                }
            }
        }


        public void Dispose()
        {
            return;
        }

        #endregion

        private bool LoadDictionary()
        {
            List<Config> list = Host.PluginConfigManager.Load("sbgrammarnazi", "dictionary.cfg");

            PeeveDict.Clear();

            if ( list == null )
            {
                return false;
            }

            foreach ( Config l in list )
            {
                PeeveDict.Add(l.Index, l.Value);
            }

            return true;
        }

        private void SaveDictionary()
        {
            List<Config> list = new List<Config>();

            foreach ( KeyValuePair<string, string> Peeve in PeeveDict )
            {
                list.Add(new Config(Peeve.Key, Peeve.Value));
            }

            Host.PluginConfigManager.Save(list, "sbgrammarnazi", "dictionary.cfg");
        }

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            if ( command.ToLower() == "addgrammar" )
            {
                if (Host.PluginUserManager.VerifyAdmin(name, channel))
                {
                    if ( parameters.Length >= 3 )
                    {
                        string[] parts = String.Join(" ", parameters).Split('=');
                        PeeveDict.Add(parts[0].Trim(), parts[1].Trim());
                        SaveDictionary();
                        Host.PluginResponse(channel, "Grammar Rule added!");
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Usage: !addgrammar [badspelling] = [goodspelling]");
                    }
                }
                else
                {
                    Host.PluginResponse(channel, "Only x can do this.");
                }
            }

            if ( command.ToLower() == "delgrammar" )
            {
                if (Host.PluginUserManager.VerifyAdmin(name, channel))
                {
                    if ( parameters.Length >= 1 )
                    {
                        string parts = String.Join(" ", parameters).Trim();
                        PeeveDict.Remove(parts);
                        SaveDictionary();
                        Host.PluginResponse(channel, "Grammar Rule removed!");
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Usage: !delgrammar [badspelling]");
                    }
                }
                else
                {
                    Host.PluginResponse(channel, "Only x can do this.");
                }
            }


            

        }

        void Host_eventPluginChannelMessageReceived( string name, string message, string channel )
        {
            string response = "";

            foreach ( KeyValuePair<string, string> Peeve in PeeveDict )
            {
                if ( Regex.Match(message, @"\b" + Peeve.Key + @"\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Success )
                {
                    response += String.Format("*{0} ", Peeve.Value);
                }
            }

            if ( response.Trim().Length > 0 )
            {
                Host.PluginResponse(channel, response);
            }

        }
    }
}
