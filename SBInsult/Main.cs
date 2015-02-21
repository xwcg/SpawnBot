using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

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

namespace SBInsult
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Profanity-Safe Insulter";
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
                return "Generates Profanity-Safe insults";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                Host.eventPluginChannelMessageReceived += new ChannelMessage(Host_eventPluginChannelMessageReceived);
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
            switch ( command )
            {
                case "insult":
                    if ( parameters.Length == 0 )
                    {
                        Insult(name, channel);
                    }
                    else if ( parameters[0] != Host.PluginBotname )
                    {
                        Insult(parameters[0], channel);
                    }
                    else
                    {
                        Insult(name, channel);
                    }
                    break;
            }
        }

        void Host_eventPluginChannelMessageReceived( string name, string message, string channel )
        {
        }

        private void Insult( string target, string channel )
        {
            string[] col1 = { "artless", "bawdy", "beslubbering", "bootless", "churlish", "cockered", "clouted", "craven", "currish", "dankish", "dissembling", "droning", "errant", 
                                "fawning", "fobbing", "froward", "frothy", "gleeking", "goatish", "gorbellied", "impertinent", "infectious", "jarring", "loggerheaded", "lumpish", 
                                "mammering", "mangled", "mewling", "paunchy", "pribbling", "puking", "puny", "qualling", "rank", "reeky", "roguish", "ruttish", "saucy", "spleeny", 
                                "spongy", "surly", "tottering", "unmuzzled", "vain", "venomed", "villainous", "warped", "wayward", "weedy", "yeasty" };

            string[] col2 = { "base-court", "bat-fowling", "beef-witted", "beetle-headed", "boil-brained", "clapper-clawed", "clay-brained", "common-kissing", "crook-pated", 
                                "dismal-dreaming", "dizzy-eyed", "doghearted", "dread-bolted", "earth-vexing", "elf-skinned", "fat-kidneyed", "fen-sucked", "flap-mouthed", "fly-bitten", 
                                "folly-fallen", "fool-born", "full-gorged", "guts-griping", "half-faced", "hasty-witted", "hedge-born", "hell-hated", "idle-headed", "ill-breeding", 
                                "ill-nurtured", "knotty-pated", "milk-livered", "motley-minded", "onion-eyed", "plume-plucked", "pottle-deep", "pox-marked", "reeling-ripe", "rough-hewn",                               "rude-growing", "rump-fed", "shard-borne", "sheep-biting", "spur-galled", "swag-bellied", "tardy-gaited", "tickle-brained", "toad-spotted", "unchin-snouted", "weather-bitten"};

            string[] col3 = { "apple-john", "baggage", "barnacle", "bladder", "boar-pig", "bugbear", "bum-bailey", "canker-blossom", "clack-dish", "clotpole", "coxcomb", "codpiece", "death-token",
                                "dewberry", "flap-dragon", "flax-wench", "flirt-gill", "foot-licker", "fustilarian", "giglet", "gudgeon", "haggard", "harpy", "hedge-pig", "horn-beast", 
                                "hugger-mugger", "joithead", "lewdster", "lout", "maggot-pie", "malt-worm", "mammet", "measle", "minnow", "miscreant", "moldwarp", "mumble-news", "nut-hook", 
                                "pigeon-egg", "pignut", "puttock", "pumpion", "ratsbane", "scut", "skainsmate", "strumpet", "varlet", "vassal", "whey-face", "wagtail" };

            Random r = new Random();

            string word1 = col1[r.Next(0, col1.Length)];
            string word2 = col2[r.Next(0, col2.Length)];
            string word3 = col3[r.Next(0, col3.Length)];

            Host.PluginResponse(channel, String.Format("{0}! You {1}, {2} {3}!", target, word1, word2, word3));
        }
    }
}
