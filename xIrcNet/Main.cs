using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;


/*
    Copyright 2012 Michael Schwarz
 
    xIrcNet is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    xIrcNet is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with xIrcNet.  If not, see <http://www.gnu.org/licenses/>.
 */


namespace xIrcNet
{
    #region Delegates

    public delegate void IrcCommandSent( string cmd );
    public delegate void IrcCommandReceived( string cmd );
    public delegate void IrcServerMessageReceived( string cmd );
    public delegate void IrcServerPingReceived( string hash );
    public delegate void IrcServerPongReceived();
    public delegate void IrcIdentChallenge( string msg );

    public delegate void IrcMessage( string channel, string name, string message );
    public delegate void IrcNotice( string name, string message );

    public delegate void IrcTopicReceived( string channel, string topic );
    public delegate void IrcTopicOwnerRecevied( string channel, string name, string date );
    public delegate void IrcNameListReceived( string channel, string[] list );

    public delegate void IrcUserJoin( string channel, string name );
    public delegate void IrcUserPart( string channel, string name, string message );
    public delegate void IrcUserMode( string channel, string name, string mode, string by );
    public delegate void IrcUserChange( string name, string newName );
    public delegate void IrcUserKicked( string channel, string name, string by, string message );
    public delegate void IrcUserQuit( string name, string message );

    public delegate void IrcConnected();
    public delegate void IrcDisconnected( string msg , bool connectionError);
    public delegate void IrcConnectingError( string error );

    public delegate void IrcRawMotdStart();
    public delegate void IrcRawMotdLineGet( string line );
    public delegate void IrcRawMotdEnd();
    public delegate void IrcRawBotModeGet( string mode );
    #endregion

    public class IRC
    {
        #region Events

        public event IrcUserJoin eventUserJoined;
        public event IrcUserKicked eventUserKicked;
        public event IrcUserMode eventUserModeChanged;
        public event IrcUserPart eventUserPart;
        public event IrcUserQuit eventUserQuit;
        public event IrcUserChange eventUserChangedNick;

        public event IrcCommandReceived eventCommandReceived;
        public event IrcCommandSent eventCommandSent;
        public event IrcMessage eventMessageReceived;
        public event IrcNotice eventNoticeReceived;

        public event IrcServerMessageReceived eventServerMessageReceived;
        public event IrcServerPingReceived eventServerPingReceived;
        public event IrcServerPongReceived eventServerPongReceived;

        public event IrcIdentChallenge eventServerIdentChallenge;

        public event IrcTopicReceived eventTopicReceived;
        public event IrcTopicOwnerRecevied eventTopicOwnerReceived;

        public event IrcNameListReceived eventNameListReceived;

        public event IrcConnected eventConnected;
        public event IrcDisconnected eventDisconnected;
        public event IrcConnectingError eventConnectingError;

        #endregion

        #region RawEvents

        public event IrcRawMotdStart eventRawMotdStarted;
        public event IrcRawMotdLineGet eventRawMotdLineReceived;
        public event IrcRawMotdEnd eventRawMotdEnded;

        public event IrcRawBotModeGet eventRawBotModeReceived;

        #endregion

        #region Private

        private string _Server;
        private int _Port;
        private string _Nick;
        private string _User;
        private string _RealName;
        private TcpClient _Connection;
        private NetworkStream _Stream;
        private StreamWriter _Out;
        private StreamReader _In;

        private Thread _ReadThread;
        private Thread _TimeoutThread;
        private string _Host;
        private DateTime _LastPing;
        private DateTime _PongChallenge;
        private bool _PongChallenging = false;
        private int _NameIncrementor = 0;
        private string _OriginalNick;

        #endregion

        #region Properties
        public string Server
        {
            get
            {
                return _Server;
            }
        }

        public int Port
        {
            get
            {
                return _Port;
            }
        }

        public string Nick
        {
            get
            {
                return _Nick;
            }
            set
            {
                _Nick = value;

                if ( _Connection != null && _Connection.Connected )
                {
                    SendCommand(String.Format("NICK {0}", _Nick));
                }
            }
        }

        public string User
        {
            get
            {
                return _User;
            }
            set
            {
                _User = value;
            }
        }

        public string RealName
        {
            get
            {
                return _RealName;
            }
            set
            {
                _RealName = value;
            }
        }

        public TcpClient Connection
        {
            get
            {
                return _Connection;
            }
        }

        public NetworkStream Stream
        {
            get
            {
                return _Stream;
            }
        }

        public StreamWriter StreamOut
        {
            get
            {
                return _Out;
            }
        }

        public StreamReader StreamIn
        {
            get
            {
                return _In;
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new IRC Connection
        /// </summary>
        /// <param name="name">Nickname/Handle</param>
        /// <param name="server">The server to connect to</param>
        /// <param name="port">The port where to connect to</param>
        public IRC( string name, string server, int port )
        {
            _Nick = name;
            _Server = server;
            _Port = port;

            _User = "Arthur";
            _RealName = "Arthur Dent";

            eventConnected += new IrcConnected(IRC_eventConnected);
        }

        /// <summary>
        /// Creates a new IRC Connection
        /// </summary>
        /// <param name="name">Nickname/Handle</param>
        /// <param name="user">Username</param>
        /// <param name="realname">Real Name</param>
        /// <param name="server">The server to connect to</param>
        /// <param name="port">The port where to connect to</param>
        public IRC( string name, string user, string realname, string server, int port )
        {
            _Nick = name;
            _User = user;
            _RealName = realname;
            _Server = server;
            _Port = port;

            eventConnected += new IrcConnected(IRC_eventConnected);
        }

        void IRC_eventConnected()
        {
            SendCommand(String.Format("USER {0} 8 * :{1}", _User, _RealName));
            SendCommand(String.Format("NICK {0}", _Nick));
        }

        #endregion

        #region Methods

        public void Disconnect()
        {
            Disconnect("So long and thanks for all the fish", false);
        }

        public void Disconnect( string msg , bool connectionError)
        {
            if ( _Connection != null && _Connection.Connected )
            {
                if ( _ReadThread.ThreadState == ThreadState.Running || _ReadThread.ThreadState == ThreadState.Background )
                {
                    _ReadThread.Abort();
                }

                if ( _TimeoutThread.ThreadState == ThreadState.Running || _TimeoutThread.ThreadState == ThreadState.Background )
                {
                    _TimeoutThread.Abort();
                }

                SendCommand(String.Format("QUIT :{0}", msg));
            }

            if ( _In != null )
            {
                _In.Close();
                _In.Dispose();
                _In = null;
            }

            if ( _Out != null )
            {
                _Out.Close();
                _Out.Dispose();
                _Out = null;
            }

            if ( _Stream != null )
            {
                _Stream.Close();
                _Stream.Dispose();
                _Stream = null;
            }

            if ( _Connection != null )
            {
                _Connection.Close();
                _Connection = null;
            }

            if ( eventDisconnected != null )
            {
                eventDisconnected(msg, connectionError);
            }
        }

        public void Connect()
        {
            if ( _Connection != null && _Connection.Connected == true )
            {
                if ( eventConnectingError != null )
                {
                    eventConnectingError("Already connected.");
                }

                return;
            }

            try
            {
                _Connection = new TcpClient(_Server, _Port);
                _Stream = _Connection.GetStream();
                _In = new StreamReader(_Stream);
                _Out = new StreamWriter(_Stream);

                if ( _Connection.Connected )
                {
                    if ( eventConnected != null )
                    {
                        eventConnected();
                    }

                    _ReadThread = new Thread(new ThreadStart(ReceiveLoop));
                    _ReadThread.IsBackground = true;
                    _ReadThread.Start();

                    _TimeoutThread = new Thread(new ThreadStart(TimeoutLoop));
                    _TimeoutThread.IsBackground = true;
                    _TimeoutThread.Start();
                }
                else
                {
                    if ( eventConnectingError != null )
                    {
                        eventConnectingError("Tcp Client could not connect");
                    }
                }
            }
            catch ( Exception e )
            {
                if ( eventConnectingError != null )
                {
                    eventConnectingError(e.Message);
                }
            }
        }

        public void SendCommand( string command )
        {
            try
            {
                if ( _Out != null && _Connection.Connected )
                {
                    _Out.WriteLine(command);
                    _Out.Flush();

                    if ( eventCommandSent != null )
                    {
                        eventCommandSent(command);
                    }
                }
            }
            catch
            {
            }
        }
        #endregion

        #region Input Handling

        private void ReceiveLoop()
        {
            try
            {
                while ( _In != null && _Connection.Connected )
                {
                    string RawIn;

                    if ( ( RawIn = _In.ReadLine() ) != null )
                    {
                        if ( eventCommandReceived != null )
                        {
                            eventCommandReceived(RawIn);
                        }

                        string[] RawParts = RawIn.Split(' ');

                        if ( RawParts[0].StartsWith(":") )
                        {
                            RawParts[0] = RawParts[0].Substring(1);
                        }

                        switch ( RawParts[0] )
                        {
                            case "PING":
                                HandlePing(RawParts[1]);
                                continue;
                            case "NOTICE":
                                if ( RawParts[1] == "AUTH" )
                                {
                                    if ( eventServerIdentChallenge != null )
                                    {
                                        eventServerIdentChallenge(RawIn.Substring(RawIn.IndexOf(":") + 1));
                                    }
                                }
                                continue;
                        }
                        switch ( RawParts[1] )
                        {
                            // WELCOME
                            case "001":
                                _Host = RawParts[0];
                                break;

                            // YOUR HOST
                            case "002":
                                break;

                            // CREATED
                            case "003":
                                break;

                            // MY INFO
                            case "004":
                                break;

                            // I SUPPORT
                            case "005":
                                break;

                            // U(R) MODE IS
                            case "221":
                                if ( eventRawBotModeReceived != null )
                                {
                                    eventRawBotModeReceived(RawParts[3]);
                                }
                                break;

                            // TOPIC
                            case "332":
                                HandleTopic(RawGetOnlyText(RawIn));
                                break;

                            // TOPIC WHO TIME
                            case "333":
                                HandleTopicInfo(RawGetOnlyText(RawIn));
                                break;

                            // NAME REPLY
                            case "353":
                                HandleNameList(RawGetOnlyText(RawIn));
                                break;

                            // END OF NAMES
                            case "366":
                                break;

                            // MOTD START
                            case "375":
                                if ( eventRawMotdStarted != null )
                                {
                                    eventRawMotdStarted();
                                }
                                break;

                            // MOTD
                            case "372":
                                if ( eventRawMotdLineReceived != null )
                                {
                                    eventRawMotdLineReceived(RawGetOnlyText(RawIn));
                                }
                                break;

                            // MOTD END
                            case "376":
                                if ( eventRawMotdEnded != null )
                                {
                                    eventRawMotdEnded();
                                }
                                break;

                            // NICKNAME IN USE
                            case "433":
                                if ( _NameIncrementor == 0 )
                                {
                                    _OriginalNick = _Nick;

                                    eventUserChangedNick += new IrcUserChange(IRC_eventUserChangedNick);
                                    eventUserQuit += new IrcUserQuit(IRC_eventUserQuit);
                                }
                                _NameIncrementor++;
                                Nick = _Nick + _NameIncrementor.ToString();
                                break;

                            ////// Normal command

                            case "MODE":
                                HandleMode(RawParts);
                                break;

                            case "PRIVMSG":
                                HandlePrivMsg(RawGetSourceNick(RawParts[0]), RawIn);
                                break;

                            case "NOTICE":
                                HandleNoticeMsg(RawGetSourceNick(RawParts[0]), RawIn);
                                break;

                            case "NICK":
                                HandleNick(RawGetSourceNick(RawParts[0]), RawParts[2]);
                                break;

                            case "KICK":
                                HandleKick(RawParts);
                                break;

                            case "JOIN":
                                if ( eventUserJoined != null )
                                {
                                    eventUserJoined(RawParts[2], RawGetSourceNick(RawParts[0]));
                                }
                                break;

                            case "PART":
                                HandlePart(RawGetSourceNick(RawParts[0]), RawIn);
                                break;

                            case "QUIT":
                                HandleQuit(RawGetSourceNick(RawParts[0]), RawIn);
                                break;

                            case "PONG":
                                if ( eventServerPongReceived != null )
                                {
                                    eventServerPongReceived();
                                }
                                break;
                        }
                    }
                }
            }
            catch ( ThreadStartException e )
            {
                return;
            }
            catch ( Exception e )
            {
                Disconnect(e.Message, true);
            }

            Disconnect("Disconnected", false);
        }

        private void TimeoutLoop()
        {
            while ( _In != null && _Connection.Connected )
            {
                if ( _Connection != null && _Connection.Connected == false )
                {
                    Disconnect("Timeout", true);
                    return;
                }

                if ( _Connection != null && _Connection.Connected == true && _PongChallenging == false )
                {
                    TimeSpan t = new TimeSpan(DateTime.Now.Ticks - _LastPing.Ticks);

                    if ( t.TotalSeconds > 200 && t.TotalSeconds < 5000 )
                    {
                        _PongChallenging = true;

                        eventServerPongReceived += new IrcServerPongReceived(IRC_eventServerPongReceived);

                        _PongChallenge = DateTime.Now;
                        SendCommand("PING :42");
                    }
                }

                if ( _PongChallenging == true )
                {
                    TimeSpan t = new TimeSpan(DateTime.Now.Ticks - _PongChallenge.Ticks);

                    if ( t.TotalSeconds > 10 )
                    {
                        Disconnect("Timeout", true);
                        return;
                    }
                }

                Thread.Sleep(1000);
            }
        }

        void IRC_eventUserQuit( string name, string message )
        {
            if ( name == _OriginalNick )
            {
                Nick = _OriginalNick;

                eventUserChangedNick -= IRC_eventUserChangedNick;
                eventUserQuit -= IRC_eventUserQuit;
            }
        }

        void IRC_eventUserChangedNick( string name, string newName )
        {
            if ( name == _OriginalNick )
            {
                Nick = _OriginalNick;

                eventUserChangedNick -= IRC_eventUserChangedNick;
                eventUserQuit -= IRC_eventUserQuit;
            }
        }

        void IRC_eventServerPongReceived()
        {
            _LastPing = DateTime.Now;
            _PongChallenging = false;
            eventServerPongReceived -= IRC_eventServerPongReceived;
        }

        /// <summary>
        /// Cleans a raw command from all junk and returns only the text.
        /// Example: :irc.server.com 333 botname #channel user 123 -> #channel user 123
        /// </summary>
        /// <param name="rawcmd">Raw command to clean</param>
        /// <returns>Only the command text</returns>
        private string RawGetOnlyText( string rawcmd )
        {
            string text = rawcmd.Substring(rawcmd.IndexOf(_Nick) + _Nick.Length + 1);
            if ( text.StartsWith(":") )
            {
                text = text.Substring(1);
            }

            return text;
        }

        private string RawGetSourceNick( string rawpart )
        {
            if ( rawpart.Contains("!") )
            {
                return rawpart.Substring(0, rawpart.IndexOf("!"));
            }
            else if ( rawpart.Contains(_Host) )
            {
                return "Server";
            }
            else
            {
                return null;
            }
        }

        private void HandlePing( string rawHash )
        {
            _LastPing = DateTime.Now;

            string hash = rawHash.Substring(1);

            if ( eventServerPingReceived != null )
            {
                eventServerPingReceived(hash);
            }

            SendCommand(String.Format("PONG :{0}", hash));

        }

        private void HandleTopic( string CleanText )
        {
            string channel = CleanText.Substring(0, CleanText.IndexOf(" :"));
            string topic = CleanText.Substring(CleanText.IndexOf(":") + 1);

            if ( eventTopicReceived != null )
            {
                eventTopicReceived(channel, topic);
            }
        }

        private void HandleTopicInfo( string CleanText )
        {
            string[] Parts = CleanText.Split(' ');

            if ( eventTopicOwnerReceived != null )
            {
                eventTopicOwnerReceived(Parts[0], Parts[1], Parts[2]);
            }
        }

        private void HandleNameList( string CleanText )
        {
            string[] Parts = CleanText.Split(' ');
            string channel = Parts[1];
            List<string> names = new List<string>();

            for ( int i = 2; i < Parts.Length; i++ )
            {
                string name = Parts[i];

                if ( name.StartsWith(":") )
                {
                    name = name.Substring(1);
                }

                names.Add(name);
            }

            if ( eventNameListReceived != null )
            {
                eventNameListReceived(channel, names.ToArray());
            }
        }

        private void HandleMode( string[] rawparts )
        {
            if ( rawparts[2].StartsWith("#") )
            {
                string by = RawGetSourceNick(rawparts[0]);
                string channel = rawparts[2];

                string modeprefix = rawparts[3].Substring(0, 1);

                char[] modes = rawparts[3].Substring(1).ToCharArray();

                for ( int i = 0; i < modes.Length; i++ )
                {
                    if ( eventUserModeChanged != null )
                    {
                        eventUserModeChanged(channel, rawparts[i + 4], modeprefix + modes[i], by);
                    }
                }
            }
            else
            {
                string by = RawGetSourceNick(rawparts[0]);
                string channel = rawparts[2];

                if ( eventUserModeChanged != null )
                {
                    eventUserModeChanged(channel, by, rawparts[3], by);
                }
            }

        }

        private void HandlePrivMsg( string CleanSourceNick, string rawcmd )
        {
            string[] parts = rawcmd.Split(' ');

            string name = CleanSourceNick;
            string channel = parts[2];
            string message = rawcmd.Substring(rawcmd.IndexOf(channel) + channel.Length + 2);

            if ( eventMessageReceived != null )
            {
                eventMessageReceived(channel, name, message);
            }
        }

        private void HandleNoticeMsg( string CleanSourceNick, string rawcmd )
        {
            string[] parts = rawcmd.Split(' ');

            string name = CleanSourceNick;
            string message = rawcmd.Substring(rawcmd.IndexOf(_Nick) + _Nick.Length + 2);

            if ( eventNoticeReceived != null )
            {
                eventNoticeReceived(name, message);
            }
        }

        private void HandleNick( string CleanSourceNick, string NewNick )
        {
            string nick = NewNick;
            if ( nick.StartsWith(":") )
            {
                nick = nick.Substring(1);
            }

            if ( eventUserChangedNick != null )
            {
                eventUserChangedNick(CleanSourceNick, nick);
            }
        }

        private void HandleKick( string[] rawparts )
        {
            string by = RawGetSourceNick(rawparts[0]);
            string channel = rawparts[2];
            string name = rawparts[3];

            string message = "";

            for ( int i = 4; i < rawparts.Length; i++ )
            {
                if ( i == 4 )
                {
                    if ( rawparts[i].StartsWith(":") )
                    {
                        message = message + rawparts[i].Substring(1) + " ";
                    }
                }
                else
                {
                    message = message + rawparts[i] + " ";
                }
            }

            message = message.Trim();

            if ( eventUserKicked != null )
            {
                eventUserKicked(channel, name, by, message);
            }
        }

        private void HandlePart( string CleanSourceNick, string rawcmd )
        {
            string name = CleanSourceNick;
            string channel = rawcmd.Split(' ')[2];
            string message;

            if ( rawcmd.Split(' ').Length > 3 )
            {
                message = rawcmd.Substring(rawcmd.IndexOf(channel) + channel.Length + 2);
            }
            else
            {
                message = "PART";
            }

            if ( eventUserPart != null )
            {
                eventUserPart(channel, name, message);
            }
        }

        private void HandleQuit( string CleanSourceNick, string rawcmd )
        {
            string name = CleanSourceNick;
            string message = rawcmd.Substring(rawcmd.IndexOf("QUIT") + 6);

            if ( eventUserQuit != null )
            {
                eventUserQuit(name, message);
            }
        }

        #endregion
    }
}
