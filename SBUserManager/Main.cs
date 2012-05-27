using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using xLogger;

namespace SBUserManager
{
    public class Main : SBUserPlugin
    {
        private SBPluginHost Host;

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

                Host.eventPluginUserChangedNick += new UserChange(Host_eventPluginUserChangedNick);
                Host.eventPluginUserJoined += new UserJoin(Host_eventPluginUserJoined);
                Host.eventPluginUserKicked += new UserKick(Host_eventPluginUserKicked);
                Host.eventPluginUserLeft += new UserLeave(Host_eventPluginUserLeft);
                Host.eventPluginUserQuit += new UserQuit(Host_eventPluginUserQuit);
                Host.eventPluginUserModeSet += new UserMode(Host_eventPluginUserModeSet);

                Host.eventPluginChannelNameListGet += new ServerNames(Host_eventPluginChannelNameListGet);
            }
        }

        public void Dispose()
        {
            return;
        }

        public bool IsOperator( string name, string channel )
        {
            IrcUser u = IUsers.GetUser(name, channel);
            if ( u == null )
            {
                Logger.WriteLine(String.Format("***** IsOperator: Can't find user '{0}' in '{1}'", name, channel), ConsoleColor.DarkRed);
                return false;
            }

            return u.HasOp;
        }

        public bool IsVoiced( string name, string channel )
        {
            IrcUser u = IUsers.GetUser(name, channel);
            if ( u == null )
            {
                Logger.WriteLine(String.Format("***** IsVoiced: Can't find user '{0}' in '{1}'", name, channel), ConsoleColor.DarkRed);
                return false;
            }

            return u.HasVoice;
        }

        public bool IsBot( string name )
        {
            return IUsers.IsBot(name);
        }

        public string[] GetChannels( string name )
        {
            return IUsers.GetChannelsForUser(name);
        }

        public string[] GetUsers( string channel )
        {
            return UserListToArray(IUsers.GetUsersForChannel(channel));
        }

        public void SetBotFlag( string name )
        {
            IUsers.SetBotFlag(name);
        }

        public void ClearAllUsers()
        {
            IUsers.RemoveAllUser("*");
        }

        #endregion

        void Host_eventPluginChannelNameListGet( string channel, string[] list )
        {
            foreach ( string u in list )
            {
                IUsers.AddUser(u, channel);
            }
        }

        void Host_eventPluginUserModeSet( string name, string channel, string mode, string by )
        {
            IrcUser u = IUsers.GetUser(name, channel);

            if ( u == null )
            {
                Logger.WriteLine(String.Format("***** User '{0}' not found in '{1}'", name, channel), ConsoleColor.DarkRed);
                return;
            }

            if ( mode.StartsWith("+") )
            {
                switch ( mode.Substring(1) )
                {
                    case "o":
                        u.Op();
                        break;
                    case "v":
                        u.Voice();
                        break;
                    default:
                        Logger.WriteLine("***** Unknown mode '" + mode + "'", ConsoleColor.DarkRed);
                        break;
                }
            }
            else if ( mode.StartsWith("-") )
            {
                switch ( mode.Substring(1) )
                {
                    case "o":
                        u.DeOp();
                        break;
                    case "v":
                        u.DeVoice();
                        break;
                    default:
                        Logger.WriteLine("***** Unknown mode '" + mode + "'", ConsoleColor.DarkRed);
                        break;
                }
            }
            else
            {
                Logger.WriteLine("***** Unknown mode '" + mode + "'", ConsoleColor.DarkRed);
            }
        }

        void Host_eventPluginUserQuit( string name, string message )
        {
            IUsers.RemoveAllUser(name);
        }

        void Host_eventPluginUserLeft( string channel, string name )
        {
            IUsers.RemoveUser(name, channel);
        }

        void Host_eventPluginUserKicked( string channel, string name, string by, string reason )
        {
            IUsers.RemoveUser(name, channel);
        }

        void Host_eventPluginUserJoined( string channel, string name )
        {
            IUsers.AddUser(name, channel);

            Logger.WriteLine(String.Format("* User '{0}' joined '{1}'", name, channel), ConsoleColor.Yellow);
        }

        void Host_eventPluginUserChangedNick( string name, string newname )
        {
            IUsers.ChangeUserName(name, newname);
        }

        private string[] UserListToArray( IrcUser[] list )
        {
            List<string> output = new List<string>();

            foreach ( IrcUser u in list )
            {
                output.Add(u.GetLegacyName());
            }

            return output.ToArray();
        }
    }
}
