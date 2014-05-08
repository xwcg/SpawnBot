using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using xLogger;

/*
    Copyright 2012-2014 Michael Schwarz
  
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

namespace SBUserManager
{
    public class Main : SBUserPlugin
    {
        private SBPluginHost Host;
        private List<IrcUser> adminChallenges = new List<IrcUser>();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SpawnBot User Manager";
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
                return "Default SpawnBot User Manager";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                Host.eventPluginUserChangedNick += new UserChange( Host_eventPluginUserChangedNick );
                Host.eventPluginUserJoined += new UserJoin( Host_eventPluginUserJoined );
                Host.eventPluginUserJoinedHostname += new UserJoinHostname( Host_eventPluginUserJoinedHostname );
                Host.eventPluginUserKicked += new UserKick( Host_eventPluginUserKicked );
                Host.eventPluginUserLeft += new UserLeave( Host_eventPluginUserLeft );
                Host.eventPluginUserQuit += new UserQuit( Host_eventPluginUserQuit );
                Host.eventPluginUserModeSet += new UserMode( Host_eventPluginUserModeSet );

                Host.eventPluginChannelNameListGet += new ServerNames( Host_eventPluginChannelNameListGet );

                Host.eventPluginPrivateCommandReceived += Host_eventPluginPrivateCommandReceived;
            }
        }


        public void Dispose ()
        {
            return;
        }

        public bool Exists ( string name, string channel )
        {
            IrcUser u = IUsers.GetUser( name, channel );
            if ( u == null )
            {
                return false;
            }
            return true;
        }

        public bool IsOperator ( string name, string channel )
        {
            IrcUser u = IUsers.GetUser( name, channel );
            if ( u == null )
            {
                Logger.WriteLine( String.Format( "***** IsOperator: Can't find user '{0}' in '{1}'", name, channel ), ConsoleColor.DarkRed );
                return false;
            }

            return u.HasOp;
        }

        public bool IsVoiced ( string name, string channel )
        {
            IrcUser u = IUsers.GetUser( name, channel );
            if ( u == null )
            {
                Logger.WriteLine( String.Format( "***** IsVoiced: Can't find user '{0}' in '{1}'", name, channel ), ConsoleColor.DarkRed );
                return false;
            }

            return u.HasVoice;
        }

        public bool VerifyAdmin ( string name, string channel )
        {
            IrcUser user = IUsers.GetUser( name, channel );
            if ( user != null )
            {
                if ( user.HasAdminFlag )
                    return true;

                if ( name == Host.PluginBotAdmin )
                {
                    Host.PluginResponse( name, "What's the secret?" );
                    Host.PluginResponse( channel, String.Format( "{0}, I recognize you as a Superuser but you haven't verified yourself yet. Challenging you now.", name ) );
                }
            }

            return false;
        }

        public bool IsBot ( string name )
        {
            return IUsers.IsBot( name );
        }

        public string[] GetChannels ( string name )
        {
            return IUsers.GetChannelsForUser( name );
        }

        public string[] GetUsers ( string channel )
        {
            return UserListToArray( IUsers.GetUsersForChannel( channel ) );
        }

        public void SetBotFlag ( string name )
        {
            IUsers.SetBotFlag( name );
        }

        public void ClearAllUsers ()
        {
            IUsers.RemoveAllUser( "*" );
        }

        #endregion


        void Host_eventPluginPrivateCommandReceived ( string name, string command, string[] parameters )
        {
            if ( command == "verify" )
            {
                if ( parameters.Length == 2 )
                {
                    string type = parameters[0];
                    string secret = parameters[1];
                    //string channel = parameters[2];

                    if ( type == "admin" )
                    {
                        if ( secret == Host.PluginBotSecret )
                        {
                            IUsers.SetAdminFlag( name );
                            Host.PluginResponse( name, "You have been verified as admin" );
                        }
                    }
                }
            }
        }

        void Host_eventPluginChannelNameListGet ( string channel, string[] list )
        {
            foreach ( string u in list )
            {
                IUsers.AddUser( u, channel, "-" );
            }
        }

        void Host_eventPluginUserModeSet ( string name, string channel, string mode, string by )
        {
            if ( name == channel )
            {
                return;
            }

            if ( mode.Contains( "b" ) )
            {
                Logger.WriteLine( String.Format( "***** Ban handling not implemented! ( {0} {1} in {2})", mode, name, channel ), ConsoleColor.DarkRed );
            }

            IrcUser u = IUsers.GetUser( name, channel );

            if ( u == null )
            {
                Logger.WriteLine( String.Format( "***** User '{0}' not found in '{1}'", name, channel ), ConsoleColor.DarkRed );
                return;
            }

            if ( mode.StartsWith( "+" ) )
            {
                switch ( mode.Substring( 1 ) )
                {
                    case "o":
                        u.Op();
                        break;
                    case "v":
                        u.Voice();
                        break;
                    default:
                        Logger.WriteLine( "***** Unknown mode '" + mode + "'", ConsoleColor.DarkRed );
                        break;
                }
            }
            else if ( mode.StartsWith( "-" ) )
            {
                switch ( mode.Substring( 1 ) )
                {
                    case "o":
                        u.DeOp();
                        break;
                    case "v":
                        u.DeVoice();
                        break;
                    default:
                        Logger.WriteLine( "***** Unknown mode '" + mode + "'", ConsoleColor.DarkRed );
                        break;
                }
            }
            else
            {
                Logger.WriteLine( "***** Unknown mode '" + mode + "'", ConsoleColor.DarkRed );
            }

            Logger.WriteLine( String.Format( "[{0}] {1} sets {2} for {3} ", new object[] { channel, by, mode, name } ), ConsoleColor.DarkYellow );
        }

        void Host_eventPluginUserQuit ( string name, string message )
        {
            IUsers.RemoveAllUser( name );

            Logger.WriteLine( String.Format( "[*] {0} has quit ({1})", name, message ), ConsoleColor.Yellow );
        }

        void Host_eventPluginUserLeft ( string channel, string name, string reason )
        {
            IUsers.RemoveUser( name, channel );

            Logger.WriteLine( String.Format( "[{0}] {1} has left ({2})", channel, name, reason ), ConsoleColor.Yellow );
        }

        void Host_eventPluginUserKicked ( string channel, string name, string by, string reason )
        {
            IUsers.RemoveUser( name, channel );

            Logger.WriteLine( String.Format( "[{0}] {1} has been kicked by {2} ({3})", new object[] { channel, name, by, reason } ), ConsoleColor.Yellow );
        }

        void Host_eventPluginUserJoined ( string channel, string name )
        {
            //IUsers.AddUser(name, channel);

            //Logger.WriteLine(String.Format("[{0}] {1} has joined", channel, name), ConsoleColor.Yellow);
        }

        void Host_eventPluginUserJoinedHostname ( string channel, string name, string hostname )
        {
            IUsers.AddUser( name, channel, hostname );

            Logger.WriteLine( String.Format( "[{0}] {1} ({2}) has joined", channel, name, hostname ), ConsoleColor.Yellow );
        }

        void Host_eventPluginUserChangedNick ( string name, string newname )
        {
            IUsers.ChangeUserName( name, newname );
        }

        private string[] UserListToArray ( IrcUser[] list )
        {
            List<string> output = new List<string>();

            foreach ( IrcUser u in list )
            {
                output.Add( u.GetLegacyName() );
            }

            return output.ToArray();
        }
    }
}
