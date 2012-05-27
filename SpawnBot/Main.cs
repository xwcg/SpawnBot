using System;
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
using xIRC;
using xLogger;
using SBPluginInterface;

namespace SpawnBot
{
    class Bot : SBPluginHost
    {
        private static IRC IrcObject;

        private static bool DebugMode = true;

        private bool Active = true;
        private string ActiveBot = "";

        private static string Botname = "";
        private static string DefaultChannelName = "";
        private static string BotConfirmationSecret = "";

        public static string TwitterAccessToken = "";
        public static string TwitterAccessTokenSecret = "";
        public static string TwitterConsumerKey = "";
        public static string TwitterConsumerKeySecret = "";

        private static List<string> Channels = new List<string>();

        private static PluginService plugins = new PluginService();

        private SBUserPlugin UserManager;


        static void Main( string[] args )
        {
            ReadConfig();

            Bot IrcApp = new Bot("irc.quakenet.org", 6667, Botname, DefaultChannelName);
        }

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

            IrcObject.IrcWriter.WriteLine(String.Format("PRIVMSG {0} :{1}", channel, correctedMessage));
            IrcObject.IrcWriter.Flush();

            Logger.WriteLine(String.Format("-> ({0}) {1}", channel, correctedMessage), ConsoleColor.DarkCyan);

            //Logger.WriteLine(String.Format("PRIVMSG {0} :{1}", channel, message));
        }

        public static void SendCommand( string command )
        {
            Logger.WriteLine("-> " + command, ConsoleColor.DarkCyan);
            IrcObject.IrcWriter.WriteLine(command);
            IrcObject.IrcWriter.Flush();
        }

        private Bot( string IrcServer, int IrcPort, string IrcUser, string IrcChan )
        {
            plugins.FindPlugins(Environment.CurrentDirectory + "\\Plugins\\");

            foreach ( AvailablePlugin p in plugins.AvailablePlugins )
            {
                p.Instance.PluginHost = this;
                Logger.WriteLine("* Loaded Plugin: " + p.Instance.PluginName + " - v" + p.Instance.Version, ConsoleColor.DarkGreen);
            }

            UserManager = (SBUserPlugin)plugins.AvailablePlugins.FindUserManager().Instance;

            IrcObject = new IRC(IrcUser, IrcChan);

            // Assign events
            IrcObject.eventReceiving += new CommandReceivedRaw(IrcCommandReceived);
            IrcObject.eventTopicSet += new TopicSetRaw(IrcTopicSet);
            IrcObject.eventTopicOwner += new TopicOwnerRaw(IrcTopicOwner);
            IrcObject.eventNamesList += new NamesListRaw(IrcNamesList);
            IrcObject.eventServerMessage += new ServerMessageRaw(IrcServerMessage);
            IrcObject.eventJoin += new JoinRaw(IrcJoin);
            IrcObject.eventPart += new PartRaw(IrcPart);
            IrcObject.eventMode += new ModeRaw(IrcMode);
            IrcObject.eventNickChange += new NickChangeRaw(IrcNickChange);
            IrcObject.eventKick += new KickRaw(IrcKick);
            IrcObject.eventQuit += new QuitRaw(IrcQuit);
            IrcObject.eventMessage += new MessageRaw(IrcObject_eventMessage);

            // Connect to server	
            IrcObject.Connect(IrcServer, IrcPort);
        }

        private void PrivateMessageResponse( string user, string message )
        {
            if ( message.Trim().StartsWith("DO YOU LOVE BEES?") )
            {
                if ( message.Substring(message.IndexOf(":") + 1).Trim() == GetSHA1Hash(DefaultChannelName + "I really love bees" + Botname + BotConfirmationSecret) )
                {
                    UserManager.SetBotFlag(user);

                    SendMessage(BotConfirmationSecret, user);
                }
                else
                {
                    SendMessage("Dirty liar!", user);
                }
            }

            if ( message.Trim().StartsWith(BotConfirmationSecret) && user != Botname && Active )
            {
                UserManager.SetBotFlag(user);

                ActiveBot = user;
                SendMessage("Alright, I'll sit on the bench.", DefaultChannelName);

                Active = false;
            }
        }

        private void ChannelMessageResponse( string User, string Message, string Channel )
        {
            #region Bot Communication
            if ( Message.Trim() == "Robot reporting for duty" && Active )
            {
                SendMessage("I'm already on duty.", Channel);
            }

            if ( Message.Trim() == "Alright, I'll sit on the bench." && Active )
            {
                if ( UserManager.IsBot(User) )
                {
                    SendMessage("<3", Channel);
                }
                else
                {
                    SendMessage("</3 Lies! Thou art no bot!", Channel);
                }
            }

            if ( Message.Trim() == "I'm already on duty." && User != Botname && Active )
            {
                SendMessage("DO YOU LOVE BEES? :" + GetSHA1Hash(DefaultChannelName + "I really love bees" + User + BotConfirmationSecret), User);
            }

            if ( !Active )
            {
                return;
            }

            #endregion

            if ( Message.StartsWith("!") )
            {
                if ( User.StartsWith("Mariondoe", StringComparison.CurrentCultureIgnoreCase) )
                {
                    Random r = new Random();
                    if ( r.Next(0, 100) <= 25 )
                    {
                        SendMessage("Unexpected error 0x0001D107", Channel);
                        return;
                    }
                }

                string[] parts = Message.Split(' ');

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
                    SendMessage("SpawnBot v" + Assembly.GetCallingAssembly().GetName().Version.ToString(), Channel);
                    return;
                }

                if ( eventPluginChannelCommandReceived != null )
                {
                    eventPluginChannelCommandReceived(User, Channel, Command, Parameters);
                }
            }
            else
            {
                if ( eventPluginChannelMessageReceived != null )
                {
                    eventPluginChannelMessageReceived(User, Message, Channel);
                }
            }
        }

        void IrcObject_eventMessage( string User, string Message, string Channel )
        {
            //if ( !User.Equals("MMC", StringComparison.CurrentCultureIgnoreCase) && !User.Equals("MMMC", StringComparison.CurrentCultureIgnoreCase) )
            //{
            Logger.WriteLine(String.Format("[{2}] {0}: {1}", User, Message, Channel));
            //}

            //if ( DebugMode )
            //{
            //    return;
            //}

            if ( Channel == Botname )
            {
                PrivateMessageResponse(User, Message);
            }
            else
            {
                ChannelMessageResponse(User, Message, Channel);
            }
        }

        #region Junk
        private void IrcCommandReceived( string IrcCommand )
        {
            //Logger.WriteLine(IrcCommand);
        }

        private void IrcTopicSet( string IrcChan, string IrcTopic )
        {
            if ( eventPluginChannelTopicSet != null )
            {
                eventPluginChannelTopicSet(IrcChan, IrcTopic);
            }
        }

        private void IrcTopicOwner( string IrcChan, string IrcUser, string TopicDate )
        {
            if ( eventPluginChannelTopicOwnerGet != null )
            {
                eventPluginChannelTopicOwnerGet(IrcChan, IrcUser, TopicDate);
            }

        }

        private void IrcNamesList( string UserNames, string IrcChannel )
        {
            string[] list = UserNames.Split(' ');

            if ( eventPluginChannelNameListGet != null )
            {
                eventPluginChannelNameListGet(IrcChannel, list);
            }
        }

        private void IrcServerMessage( string ServerMessage )
        {
            Logger.WriteLine(String.Format("** {0}", ServerMessage), ConsoleColor.Green);
        }

        private void IrcJoin( string IrcChan, string IrcUser )
        {
            if ( eventPluginUserJoined != null )
            {
                eventPluginUserJoined(IrcChan, IrcUser);
            }
        }

        private void IrcPart( string IrcChan, string IrcUser )
        {
            if ( IrcUser == ActiveBot )
            {
                SendMessage("I'll take over " + ActiveBot + "'s work now. The King is dead. Long live the King!", IrcChan);
                Active = true;
                ActiveBot = Botname;
            }

            if ( eventPluginUserLeft != null )
            {
                eventPluginUserLeft(IrcChan, IrcUser);
            }
        }

        private void IrcMode( string IrcChan, string IrcUser, string UserMode )
        {
            if ( IrcUser == Botname && IrcUser == IrcChan )
            {
                foreach ( string channel in Channels )
                {
                    SendCommand("JOIN " + channel);
                }

                if ( !DebugMode )
                {
                    SendMessage("Robot reporting for duty", DefaultChannelName);
                }

                return;
            }

            string[] modeparts = UserMode.Split(' ');

            bool add = false;

            if ( modeparts[0].StartsWith("+") )
            {
                add = true;
            }
            else
            {
                add = false;
            }

            char[] modes = modeparts[0].Substring(1).ToCharArray();

            for ( int i = 0; i < modes.Length; i++ )
            {
                if ( add )
                {
                    if ( eventPluginUserModeSet != null )
                    {
                        eventPluginUserModeSet(modeparts[i + 1], IrcChan, "+" + modes[i], IrcUser);
                    }
                }
                else
                {
                    if ( eventPluginUserModeSet != null )
                    {
                        eventPluginUserModeSet(modeparts[i + 1], IrcChan, "-" + modes[i], IrcUser);
                    }
                }
            }
        }

        private void IrcNickChange( string UserOldNick, string UserNewNick )
        {
            if ( eventPluginUserChangedNick != null )
            {
                eventPluginUserChangedNick(UserOldNick, UserNewNick);
            }
        }

        private void IrcKick( string IrcChannel, string UserKicker, string UserKicked, string KickMessage )
        {
            if ( eventPluginUserKicked != null )
            {
                eventPluginUserKicked(IrcChannel, UserKicked, UserKicker, KickMessage);
            }
        }

        private void IrcQuit( string UserQuit, string QuitMessage )
        {
            if ( UserQuit == ActiveBot )
            {
                SendMessage("I'll take over " + ActiveBot + "'s work now. The King is dead. Long live the King!", DefaultChannelName);
                Active = true;
                ActiveBot = Botname;
            }

            if ( eventPluginUserQuit != null )
            {
                eventPluginUserQuit(UserQuit, QuitMessage);
            }
        }
        #endregion

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
