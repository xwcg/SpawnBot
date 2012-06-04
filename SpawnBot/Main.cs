﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Twitterizer;
using xLogger;
using SBPluginInterface;
using xIrcNet;

namespace SpawnBot
{
    class Bot : SBPluginHost
    {
        #region Properties
        private static IRC IrcService;

        private static bool DebugMode = true;
        private bool Active = true;
        private string ActiveBot = "";

        private static string Botname = "";
        private static string DefaultChannelName = "";
        private static string BotConfirmationSecret = "";

        private static List<string> Channels = new List<string>();

        private static PluginService plugins = new PluginService();
        private static SBUserPlugin UserManager;

        public static string TwitterAccessToken = "";
        public static string TwitterAccessTokenSecret = "";
        public static string TwitterConsumerKey = "";
        public static string TwitterConsumerKeySecret = "";
        #endregion

        static void Main( string[] args )
        {
            ReadConfig();

            Bot BotAbstractor = new Bot();

            // Command interface

            string cmd;

            while ( ( cmd = Console.ReadLine() ) != null )
            {
                string[] cmdparts = cmd.Split(' ');

                switch ( cmdparts[0] )
                {
                    case "quit":
                        if ( cmdparts.Length > 1 )
                        {
                            IrcService.Disconnect(cmd.Substring(5));
                        }
                        else
                        {
                            IrcService.Disconnect();
                        }
                        Environment.Exit(0);
                        break;
                    case "connect":
                        IrcService.Connect();
                        break;
                    case "disconnect":
                        if ( cmdparts.Length > 1 )
                        {
                            IrcService.Disconnect(cmd.Substring(11));
                        }
                        else
                        {
                            IrcService.Disconnect();
                        }
                        break;
                    case "ping":
                        IrcService.SendCommand("PING :test");
                        break;
                    case "say":
                        if ( cmdparts.Length > 1 )
                        {
                            string channel = cmdparts[1];
                            string message = cmd.Substring(cmd.IndexOf(channel) + channel.Length + 1);

                            SendMessage(message, channel);
                        }
                        break;
                    case "join":
                        if ( cmdparts.Length > 1 )
                        {
                            IrcService.SendCommand("JOIN " + cmdparts[1]);
                        }
                        break;
                    default:
                        Console.WriteLine("Unknown command '" + cmd + "'");
                        break;
                }
            }
        }

        private Bot()
        {
            // Load Plugins
            plugins.FindPlugins(Environment.CurrentDirectory + "\\Plugins\\");

            foreach ( AvailablePlugin p in plugins.AvailablePlugins )
            {
                p.Instance.PluginHost = this;
                Logger.WriteLine("* Loaded Plugin: " + p.Instance.PluginName + " - v" + p.Instance.Version, ConsoleColor.DarkGreen);
            }

            UserManager = (SBUserPlugin)plugins.AvailablePlugins.FindUserManager().Instance;

            // Start Service

            IrcService = new IRC(Botname, "SpawnBot", "Spawnbot", "irc.quakenet.org", 6667);

            IrcService.eventNameListReceived += new IrcNameListReceived(IrcService_eventNameListReceived);
            IrcService.eventTopicReceived += new IrcTopicReceived(IrcService_eventTopicReceived);
            IrcService.eventTopicOwnerReceived += new IrcTopicOwnerRecevied(IrcService_eventTopicOwnerReceived);

            IrcService.eventUserJoined += new IrcUserJoin(IrcService_eventUserJoined);
            IrcService.eventUserKicked += new IrcUserKicked(IrcService_eventUserKicked);
            IrcService.eventUserChangedNick += new IrcUserChange(IrcService_eventUserChangedNick);
            IrcService.eventUserModeChanged += new IrcUserMode(IrcService_eventUserModeChanged);
            IrcService.eventUserPart += new IrcUserPart(IrcService_eventUserPart);
            IrcService.eventUserQuit += new IrcUserQuit(IrcService_eventUserQuit);

            IrcService.eventRawBotModeReceived += new IrcRawBotModeGet(IrcService_eventRawBotModeReceived);

            IrcService.eventMessageReceived += new IrcMessage(IrcService_eventMessageReceived);

            IrcService.eventDisconnected += new IrcDisconnected(IrcService_eventDisconnected);

            IrcService.Connect();
        }

        void IrcService_eventDisconnected( string msg )
        {
            if ( msg == "Disconnected" || msg == "Timeout" )
            {
                UserManager.ClearAllUsers();
                IrcService.Connect();
            }
        }

        #region IRC Events

        void IrcService_eventRawBotModeReceived( string mode )
        {
            if ( mode == "+i" )
            {
                foreach ( string channel in Channels )
                {
                    SendCommand("JOIN " + channel);
                }

                if ( !DebugMode )
                {
                    SendMessage("Robot reporting for duty", DefaultChannelName);
                }
            }
        }

        void IrcService_eventUserQuit( string name, string message )
        {
            if ( name == ActiveBot )
            {
                SendMessage("I'll take over " + ActiveBot + "'s work now. The King is dead. Long live the King!", DefaultChannelName);
                Active = true;
                ActiveBot = Botname;
            }

            if ( eventPluginUserQuit != null )
            {
                eventPluginUserQuit(name, message);
            }
        }

        void IrcService_eventUserPart( string channel, string name, string message )
        {
            if ( eventPluginUserLeft != null )
            {
                eventPluginUserLeft(channel, name, message);
            }
        }

        void IrcService_eventUserModeChanged( string channel, string name, string mode, string by )
        {
            if ( eventPluginUserModeSet != null )
            {
                eventPluginUserModeSet(name, channel, mode, by);
            }
        }

        void IrcService_eventUserChangedNick( string name, string newName )
        {
            if ( eventPluginUserChangedNick != null )
            {
                eventPluginUserChangedNick(name, newName);
            }
        }

        void IrcService_eventUserKicked( string channel, string name, string by, string message )
        {
            if ( eventPluginUserKicked != null )
            {
                eventPluginUserKicked(channel, name, by, message);
            }
        }

        void IrcService_eventUserJoined( string channel, string name )
        {
            if ( eventPluginUserJoined != null )
            {
                eventPluginUserJoined(channel, name);
            }
        }

        void IrcService_eventMessageReceived( string channel, string name, string message )
        {
            Logger.WriteLine(String.Format("[{0}] {1}: {2}", channel, name, message));

            if ( channel == IrcService.Nick )
            {
                PrivateMessageResponse(name, message);
            }
            else
            {
                ChannelMessageResponse(channel, name, message);
            }
        }

        void IrcService_eventTopicOwnerReceived( string channel, string name, string date )
        {
            if ( eventPluginChannelTopicOwnerGet != null )
            {
                eventPluginChannelTopicOwnerGet(channel, name, date);
            }
        }

        void IrcService_eventTopicReceived( string channel, string topic )
        {
            if ( eventPluginChannelTopicSet != null )
            {
                eventPluginChannelTopicSet(channel, topic);
            }
        }

        void IrcService_eventNameListReceived( string channel, string[] list )
        {
            if ( eventPluginChannelNameListGet != null )
            {
                eventPluginChannelNameListGet(channel, list);
            }
        }

        #endregion

        private static void ReadConfig()
        {
            string input;
            StreamReader r;

            // Read config file
            if ( File.Exists("config.txt") )
            {
                r = new StreamReader("config.txt");

                Botname = r.ReadLine();
                BotConfirmationSecret = r.ReadLine();


                while ( ( input = r.ReadLine() ) != null )
                {
                    if ( DefaultChannelName == "" )
                    {
                        DefaultChannelName = input;
                    }

                    Channels.Add(input);
                }

                r.Close();
                r.Dispose();
            }
            else
            {
                Logger.WriteLine("****** CONFIG.TXT not found ******", ConsoleColor.Red);
                Environment.Exit(0);
            }

            // Read Twitter Keys

            input = "";

            if ( File.Exists("twitterkeys.txt") )
            {
                r = new StreamReader("twitterkeys.txt");

                TwitterAccessToken = r.ReadLine();
                TwitterAccessTokenSecret = r.ReadLine();
                TwitterConsumerKey = r.ReadLine();
                TwitterConsumerKeySecret = r.ReadLine();

                r.Close();
                r.Dispose();
            }
            else
            {
                Logger.WriteLine("*** TWITTERKEYS.TXT not found. Ignoring.", ConsoleColor.Yellow);
            }
        }

        public static string GetSHA1Hash( string text )
        {
            var SHA1 = new SHA1CryptoServiceProvider();

            byte[] arrayData;
            byte[] arrayResult;
            string result = null;
            string temp = null;

            arrayData = Encoding.ASCII.GetBytes(text);
            arrayResult = SHA1.ComputeHash(arrayData);
            for ( int i = 0; i < arrayResult.Length; i++ )
            {
                temp = Convert.ToString(arrayResult[i], 16);
                if ( temp.Length == 1 )
                    temp = "0" + temp;
                result += temp;
            }
            return result;
        }

        public static void SendMessage( string message, string channel )
        {
            string correctedMessage = message.Replace("\n", "").Replace("\r", "");

            IrcService.SendCommand(String.Format("PRIVMSG {0} :{1}", channel, correctedMessage));

            Logger.WriteLine(String.Format("-> ({0}) {1}", channel, correctedMessage), ConsoleColor.DarkCyan);
        }

        public static void SendCommand( string command )
        {
            Logger.WriteLine("-> " + command, ConsoleColor.DarkCyan);
            IrcService.SendCommand(command);
        }

        private void PrivateMessageResponse( string name, string message )
        {
            if ( message.Trim().StartsWith("DO YOU LOVE BEES?") )
            {
                if ( message.Substring(message.IndexOf(":") + 1).Trim() == GetSHA1Hash(DefaultChannelName + "I really love bees" + Botname + BotConfirmationSecret) )
                {
                    UserManager.SetBotFlag(name);

                    SendMessage(BotConfirmationSecret, name);
                }
                else
                {
                    SendMessage("Dirty liar!", name);
                }
                return;
            }

            if ( message.Trim().StartsWith(BotConfirmationSecret) && name != Botname && Active )
            {
                UserManager.SetBotFlag(name);

                ActiveBot = name;
                SendMessage("Alright, I'll sit on the bench.", DefaultChannelName);

                Active = false;
                return;
            }

            if ( eventPluginPrivateMessageReceived != null )
            {
                eventPluginPrivateMessageReceived(name, message);
            }
        }

        private void ChannelMessageResponse( string channel, string name, string message )
        {
            #region Bot Communication
            if ( message.Trim() == "Robot reporting for duty" && Active )
            {
                SendMessage("I'm already on duty.", channel);
            }

            if ( message.Trim() == "Alright, I'll sit on the bench." && Active )
            {
                if ( UserManager.IsBot(name) )
                {
                    SendMessage("<3", channel);
                }
                else
                {
                    SendMessage("</3 Lies! Thou art no bot!", channel);
                }
            }

            if ( message.Trim() == "I'm already on duty." && name != Botname && Active )
            {
                SendMessage("DO YOU LOVE BEES? :" + GetSHA1Hash(DefaultChannelName + "I really love bees" + name + BotConfirmationSecret), name);
            }

            if ( !Active )
            {
                return;
            }

            #endregion

            if ( message.StartsWith("!") )
            {
                if ( name.StartsWith("Mariondoe", StringComparison.CurrentCultureIgnoreCase) )
                {
                    Random r = new Random();
                    if ( r.Next(0, 100) <= 25 )
                    {
                        SendMessage("Unexpected error 0x0001D107", channel);
                        return;
                    }
                }

                string[] parts = message.Split(' ');

                if ( parts[0].Length == 1 )
                {
                    return;
                }

                parts[0] = parts[0].Substring(1).ToLower();

                string Command = parts[0];
                string[] Parameters = new string[parts.Length - 1];

                for ( int i = 1; i < parts.Length; i++ )
                {
                    Parameters[i - 1] = parts[i];
                }

                if ( Command == "version" )
                {
                    SendMessage("SpawnBot v" + Assembly.GetCallingAssembly().GetName().Version.ToString(), channel);
                    return;
                }

                if ( eventPluginChannelCommandReceived != null )
                {
                    eventPluginChannelCommandReceived(name, channel, Command, Parameters);
                }
            }
            else
            {
                if ( eventPluginChannelMessageReceived != null )
                {
                    eventPluginChannelMessageReceived(name, message, channel);
                }
            }
        }

        #region SBPluginHost Members

        public void PluginResponse( string channel, string message )
        {
            SendMessage(message, channel);
        }

        public event UserJoin eventPluginUserJoined;

        public event UserLeave eventPluginUserLeft;

        public event UserKick eventPluginUserKicked;

        public event UserChange eventPluginUserChangedNick;

        public event UserQuit eventPluginUserQuit;

        public event UserMode eventPluginUserModeSet;

        public event ChannelMessage eventPluginChannelMessageReceived;

        public event PrivateMessage eventPluginPrivateMessageReceived;

        public event ServerTopicSet eventPluginChannelTopicSet;

        public event ServerTopicOwner eventPluginChannelTopicOwnerGet;

        public event ServerNames eventPluginChannelNameListGet;

        public event ChannelCommand eventPluginChannelCommandReceived;

        public event PrivateCommand eventPluginPrivateCommandReceived;

        public string PluginTwitterAccessToken
        {
            get
            {
                return TwitterAccessToken;
            }
        }

        public string PluginTwitterAccessTokenSecret
        {
            get
            {
                return TwitterAccessTokenSecret;
            }
        }

        public string PluginTwitterConsumerKey
        {
            get
            {
                return TwitterConsumerKey;
            }
        }

        public string PluginTwitterConsumerKeySecret
        {
            get
            {
                return TwitterConsumerKeySecret;
            }
        }

        public string PluginBotname
        {
            get
            {
                return Botname;
            }
        }

        public string PluginBotFolder
        {
            get
            {
                return Environment.CurrentDirectory;
            }
        }

        public SBUserPlugin PluginUserManager
        {
            get
            {
                return UserManager;
            }
        }

        #endregion
    }
}
