using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xLogger;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace xIRC
{
    #region Delegates
    public delegate void CommandReceivedRaw( string IrcCommand );
    public delegate void TopicSetRaw( string IrcChannel, string IrcTopic );
    public delegate void TopicOwnerRaw( string IrcChannel, string IrcUser, string TopicDate );
    public delegate void NamesListRaw( string UserNames, string IrcChannel );
    public delegate void ServerMessageRaw( string ServerMessage );
    public delegate void JoinRaw( string IrcChannel, string IrcUser );
    public delegate void PartRaw( string IrcChannel, string IrcUser );
    public delegate void ModeRaw( string IrcChannel, string IrcUser, string UserMode );
    public delegate void NickChangeRaw( string UserOldNick, string UserNewNick );
    public delegate void KickRaw( string IrcChannel, string UserKicker, string UserKicked, string KickMessage );
    public delegate void QuitRaw( string UserQuit, string QuitMessage );
    public delegate void MessageRaw( string User, string Message, string Channel );
    #endregion

    public class IRC
    {
        #region Events
        public event CommandReceivedRaw eventReceiving;
        public event TopicSetRaw eventTopicSet;
        public event TopicOwnerRaw eventTopicOwner;
        public event NamesListRaw eventNamesList;
        public event ServerMessageRaw eventServerMessage;
        public event JoinRaw eventJoin;
        public event PartRaw eventPart;
        public event ModeRaw eventMode;
        public event NickChangeRaw eventNickChange;
        public event KickRaw eventKick;
        public event QuitRaw eventQuit;
        public event MessageRaw eventMessage;
        #endregion

        #region Private Variables
        private string ircServer;
        private int ircPort;
        private string ircNick;
        private string ircUser;
        private string ircRealName;
        private string ircChannel;
        private bool isInvisible;
        private TcpClient ircConnection;
        private NetworkStream ircStream;
        private StreamWriter ircWriter;
        private StreamReader ircReader;
        #endregion

        #region Properties
        public string IrcServer
        {
            get
            {
                return this.ircServer;
            }
            set
            {
                this.ircServer = value;
            }
        }

        public int IrcPort
        {
            get
            {
                return this.ircPort;
            }
            set
            {
                this.ircPort = value;
            }
        }

        public string IrcNick
        {
            get
            {
                return this.ircNick;
            }
            set
            {
                this.ircNick = value;
            }
        }

        public string IrcUser
        {
            get
            {
                return this.ircUser;
            }
            set
            {
                this.ircUser = value;
            }
        }

        public string IrcRealName
        {
            get
            {
                return this.ircRealName;
            }
            set
            {
                this.ircRealName = value;
            }
        }

        public string IrcChannel
        {
            get
            {
                return this.ircChannel;
            }
            set
            {
                this.ircChannel = value;
            }
        }

        public bool IsInvisble
        {
            get
            {
                return this.isInvisible;
            }
            set
            {
                this.isInvisible = value;
            }
        }

        public TcpClient IrcConnection
        {
            get
            {
                return this.ircConnection;
            }
            set
            {
                this.ircConnection = value;
            }
        }

        public NetworkStream IrcStream
        {
            get
            {
                return this.ircStream;
            }
            set
            {
                this.ircStream = value;
            }
        }

        public StreamWriter IrcWriter
        {
            get
            {
                return this.ircWriter;
            }
            set
            {
                this.ircWriter = value;
            }
        }

        public StreamReader IrcReader
        {
            get
            {
                return this.ircReader;
            }
            set
            {
                this.ircReader = value;
            }
        }
        #endregion

        #region Constructor
        public IRC( string IrcNick, string IrcChannel )
        {
            this.IrcNick = IrcNick;
            this.IrcUser = "Spawnbot";
            this.IrcRealName = "SpawnBot Alpha";
            this.IrcChannel = IrcChannel;
            this.IsInvisble = false;
        }
        #endregion

        #region Public Methods

        private void SendCommand( string command )
        {
            Logger.WriteLine("-> " + command, ConsoleColor.DarkCyan);
            IrcWriter.WriteLine(command);
            IrcWriter.Flush();
        }

        public void Connect( string IrcServer, int IrcPort )
        {
            Thread.Sleep(1000);

            this.IrcServer = IrcServer;
            this.IrcPort = IrcPort;

            Logger.WriteLine("* Connecting...", ConsoleColor.White);

            // Connect
            try
            {
                this.IrcConnection = new TcpClient(this.IrcServer, this.IrcPort);
                this.IrcStream = this.IrcConnection.GetStream();
                this.IrcReader = new StreamReader(this.IrcStream);
                this.IrcWriter = new StreamWriter(this.IrcStream);
            }
            catch
            {
                Logger.WriteLine("**** Error, retrying...", ConsoleColor.Red);
                Connect(IrcServer, IrcPort);

                return;
            }

            Logger.WriteLine("* Connected!", ConsoleColor.White);

            // Authenticate
            string isInvisible = this.IsInvisble ? "8" : "0";
            this.SendCommand(String.Format("USER {0} {1} * :{2}", this.IrcUser, isInvisible, this.IrcRealName));

            this.SendCommand(String.Format("NICK {0}", this.IrcNick));

            this.SendCommand(String.Format("JOIN {0}", this.IrcChannel));

            // Listen
            while ( true )
            {
                string ircCommand;
                try
                {
                    while ( ( ircCommand = this.IrcReader.ReadLine() ) != null )
                    {
                        if ( !IrcConnection.Connected )
                        {
                            break;
                        }
                        //if ( !expired )
                        //{
                        //    TimeSpan s = DateTime.Now.Subtract(t);
                        //    if ( Math.Abs(s.Seconds) >= 10 )
                        //    {
                        //        Console.WriteLine("Joining");
                        //        this.IrcWriter.WriteLine(String.Format("JOIN {0}", this.IrcChannel));
                        //        this.IrcWriter.Flush();

                        //        expired = true;
                        //    }
                        //    else
                        //    {
                        //        Console.WriteLine(( Math.Abs(s.Seconds) - 10 ).ToString() + " to join");
                        //    }
                        //}

                        if ( eventReceiving != null )
                        {
                            this.eventReceiving(ircCommand);
                        }

                        //if ( ircCommand.Contains("Imperialistic") )
                        //{
                        //}

                        string[] commandParts = new string[ircCommand.Split(' ').Length];
                        commandParts = ircCommand.Split(' ');
                        if ( commandParts[0].Substring(0, 1) == ":" )
                        {
                            commandParts[0] = commandParts[0].Remove(0, 1);
                        }

                        if ( commandParts[0].Contains("quakenet") && !commandParts[0].Contains("!~") )
                        {
                            // Server message
                            switch ( commandParts[1] )
                            {
                                case "332":
                                    this.IrcTopic(commandParts);
                                    break;
                                case "333":
                                    this.IrcTopicOwner(commandParts);
                                    break;
                                case "353":
                                    this.IrcNamesList(commandParts);
                                    break;
                                case "366": /*this.IrcEndNamesList(commandParts);*/
                                    break;
                                case "372": /*this.IrcMOTD(commandParts);*/
                                    break;
                                case "376": /*this.IrcEndMOTD(commandParts);*/
                                    //Console.WriteLine("End of MOTD");
                                    break;
                                case "MODE":
                                    this.IrcModeServer(commandParts);
                                    break;
                                case "PRIVMSG":
                                    //Logger.WriteLine("*** " + ircCommand, ConsoleColor.Green);
                                    this.IrcMessage(commandParts);
                                    break;

                                case "JOIN":
                                    this.IrcJoin(commandParts);
                                    break;
                                case "PART":
                                    this.IrcPart(commandParts);
                                    break;
                                case "NICK":
                                    this.IrcNickChange(commandParts);
                                    break;
                                case "KICK":
                                    this.IrcKick(commandParts);
                                    break;
                                case "QUIT":
                                    this.IrcQuit(commandParts);
                                    break;
                                default:
                                    this.IrcServerMessage(commandParts);
                                    break;
                            }
                        }
                        else if ( commandParts[0] == "PING" )
                        {
                            // Server PING, send PONG back
                            Logger.WriteLine("<- PING", ConsoleColor.DarkMagenta);
                            this.IrcPing(commandParts);
                        }
                        else
                        {
                            // Normal message
                            string commandAction = commandParts[1];
                            switch ( commandAction )
                            {
                                case "JOIN":
                                    this.IrcJoin(commandParts);
                                    break;
                                case "PART":
                                    this.IrcPart(commandParts);
                                    break;
                                case "MODE":
                                    this.IrcMode(commandParts);
                                    break;
                                case "NICK":
                                    this.IrcNickChange(commandParts);
                                    break;
                                case "KICK":
                                    this.IrcKick(commandParts);
                                    break;
                                case "QUIT":
                                    this.IrcQuit(commandParts);
                                    break;
                                case "PRIVMSG":
                                    //Logger.WriteLine("*** " + ircCommand, ConsoleColor.Green);
                                    this.IrcMessage(commandParts);
                                    break;
                                default:
                                    Logger.WriteLine("*** Unrecognized: " + ircCommand, ConsoleColor.DarkRed);
                                    break;
                            }
                        }
                    }
                }
                catch ( Exception e )
                {
                    Logger.WriteLine("***** Error in command loop or connection. Exception Message:", ConsoleColor.DarkRed);
                    Logger.WriteLine(e.Message, ConsoleColor.DarkRed);
                }

                Logger.WriteLine("* Disconnected", ConsoleColor.White);
                this.IrcWriter.Close();
                this.IrcReader.Close();
                this.IrcConnection.Close();

                this.Connect(IrcServer, IrcPort);
            }
        }
        #endregion

        #region Private Methods
        #region Server Messages
        private void IrcTopic( string[] IrcCommand )
        {
            string IrcChannel = IrcCommand[3];
            string IrcTopic = "";
            for ( int intI = 4; intI < IrcCommand.Length; intI++ )
            {
                IrcTopic += IrcCommand[intI] + " ";
            }
            if ( eventTopicSet != null )
            {
                this.eventTopicSet(IrcChannel, IrcTopic.Remove(0, 1).Trim());
            }
        }

        private void IrcTopicOwner( string[] IrcCommand )
        {
            string IrcChannel = IrcCommand[3];
            string IrcUser = IrcCommand[4].Split('!')[0];
            string TopicDate = IrcCommand[5];
            if ( eventTopicOwner != null )
            {
                this.eventTopicOwner(IrcChannel, IrcUser, TopicDate);
            }
        }

        private void IrcNamesList( string[] IrcCommand )
        {
            string UserNames = "";

            for ( int intI = 5; intI < IrcCommand.Length; intI++ )
            {
                UserNames += IrcCommand[intI] + " ";
            }
            if ( eventNamesList != null )
            {
                this.eventNamesList(UserNames.Remove(0, 1).Trim(), IrcCommand[4]);
            }
        }

        private void IrcServerMessage( string[] IrcCommand )
        {
            string ServerMessage = "";
            for ( int intI = 0; intI < IrcCommand.Length; intI++ )
            {
                ServerMessage += IrcCommand[intI] + " ";
            }
            if ( eventServerMessage != null )
            {
                this.eventServerMessage(ServerMessage.Trim());
            }
        }
        #endregion

        #region Ping
        private void IrcPing( string[] IrcCommand )
        {
            string PingHash = "";
            for ( int intI = 1; intI < IrcCommand.Length; intI++ )
            {
                PingHash += IrcCommand[intI] + " ";
            }
            this.IrcWriter.WriteLine("PONG " + PingHash);
            this.IrcWriter.Flush();
            Logger.WriteLine("-> PONG!", ConsoleColor.DarkCyan);
        }
        #endregion

        #region User Messages
        private void IrcJoin( string[] IrcCommand )
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            if ( eventJoin != null )
            {
                this.eventJoin(IrcChannel.Remove(0, 1), IrcUser);
            }
        }

        private void IrcPart( string[] IrcCommand )
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            if ( eventPart != null )
            {
                this.eventPart(IrcChannel, IrcUser);
            }
        }

        private void IrcMode( string[] IrcCommand )
        {
            string IrcChannel = IrcCommand[2];
            string IrcUser = IrcCommand[0].Split('!')[0];
            string UserMode = "";
            for ( int intI = 3; intI < IrcCommand.Length; intI++ )
            {
                UserMode += IrcCommand[intI] + " ";
            }
            if ( UserMode.Substring(0, 1) == ":" )
            {
                UserMode = UserMode.Remove(0, 1);
            }
            if ( eventMode != null )
            {
                this.eventMode(IrcChannel, IrcUser, UserMode.Trim());
            }
        }

        private void IrcModeServer( string[] IrcCommand )
        {
            if ( eventMode != null )
            {
                //this.eventMode(IrcCommand[2], IrcCommand[4], IrcCommand[3]);
                this.eventMode(IrcCommand[2], "Q", IrcCommand[3] + " " + IrcCommand[4]);
            }
        }

        private void IrcNickChange( string[] IrcCommand )
        {
            string UserOldNick = IrcCommand[0].Split('!')[0];
            string UserNewNick = IrcCommand[2].Remove(0, 1);
            if ( eventNickChange != null )
            {
                this.eventNickChange(UserOldNick, UserNewNick);
            }
        }

        private void IrcKick( string[] IrcCommand )
        {
            string UserKicker = IrcCommand[0].Split('!')[0];
            string UserKicked = IrcCommand[3];
            string IrcChannel = IrcCommand[2];
            string KickMessage = "";
            for ( int intI = 4; intI < IrcCommand.Length; intI++ )
            {
                KickMessage += IrcCommand[intI] + " ";
            }
            if ( eventKick != null )
            {
                this.eventKick(IrcChannel, UserKicker, UserKicked, KickMessage.Remove(0, 1).Trim());
            }
        }

        private void IrcQuit( string[] IrcCommand )
        {
            string UserQuit = IrcCommand[0].Split('!')[0];
            string QuitMessage = "";
            for ( int intI = 2; intI < IrcCommand.Length; intI++ )
            {
                QuitMessage += IrcCommand[intI] + " ";
            }
            if ( eventQuit != null )
            {
                this.eventQuit(UserQuit, QuitMessage.Remove(0, 1).Trim());
            }
        }

        private void IrcMessage( string[] IrcCommand )
        {
            string UserQuit = IrcCommand[0].Split('!')[0];
            string channel = IrcCommand[2];
            string message = "";

            for ( int i = 3; i < IrcCommand.Length; i++ )
            {
                message += IrcCommand[i] + " ";
            }

            message = message.Substring(1).Trim();

            this.eventMessage(UserQuit, message, channel);
        }

        #endregion
        #endregion
    }
}
