using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xLogger;

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

namespace SBUserManager
{
    public class IrcUser
    {
        private bool isOp = false;
        private bool isVoice = false;
        private bool isBot = false;
        private string name;
        private string channel;
        private string hostname;

        public IrcUser( string UserName, string ChannelName, string HostName )
        {
            if ( UserName.StartsWith("+") )
            {
                isVoice = true;
                name = UserName.Substring(1);
            }
            else if ( UserName.StartsWith("@") )
            {
                isVoice = true;
                isOp = true;
                name = UserName.Substring(1);
            }
            else
            {
                name = UserName;
            }

            if ( ChannelName.StartsWith("#") )
            {
                channel = ChannelName;
            }
            else
            {
                channel = "#" + ChannelName;
            }

            hostname = HostName;
        }

        public string GetLegacyName()
        {
            if ( isOp )
            {
                return "@" + name;
            }
            else if ( isVoice )
            {
                return "+" + name;
            }
            else
            {
                return name;
            }
        }

        public void ChangeName( string newname )
        {
            Logger.WriteLine(String.Format("* '{0}' changed name to '{1}'", name, newname), ConsoleColor.DarkYellow);
            name = newname;
        }

        public void Op()
        {
            isVoice = true;
            isOp = true;
        }

        public void DeOp()
        {
            isOp = false;
        }

        public void Voice()
        {
            isVoice = true;
        }

        public void DeVoice()
        {
            isVoice = false;
        }

        public void FlagBot()
        {
            isBot = true;

            Logger.WriteLine(String.Format("* Flagged '{0}' as bot", name), ConsoleColor.DarkYellow);
        }

        public bool HasOp
        {
            get
            {
                return isOp;
            }
        }

        public bool HasVoice
        {
            get
            {
                return isVoice;
            }
        }

        public bool HasBotFlag
        {
            get
            {
                return isBot;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Channel
        {
            get
            {
                return channel;
            }
        }

        public string Hostname
        {
            get
            {
                return hostname;
            }
        }
    }

    public static class IUsers
    {
        private static List<IrcUser> Userlist = new List<IrcUser>();

        public static bool RemoveUser( string name, string channel )
        {
            int user = Userlist.FindIndex(delegate( IrcUser n )
            {
                if ( n.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) )
                {
                    if ( n.Channel.Equals(channel, StringComparison.CurrentCultureIgnoreCase) )
                    {
                        return true;
                    }
                }

                return false;
            });

            if ( user < 0 )
            {
                return false;
            }
            else
            {
                Userlist.RemoveAt(user);
                return true;
            }
        }

        public static bool[] RemoveAllUser( string name )
        {
            if ( name == "*" )
            {
                Userlist.Clear();
                return new bool[] { true };
            }

            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                return new bool[] { false };
            }

            List<bool> successes = new List<bool>();

            foreach ( IrcUser u in users )
            {
                successes.Add(RemoveUser(u.Name, u.Channel));
            }

            return successes.ToArray();
        }

        public static void AddUser( string name, string channel, string hostname )
        {
            Userlist.Add(new IrcUser(name, channel, hostname));
        }

        public static bool ChangeUserName( string name, string newname )
        {
            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                return false;
            }

            foreach ( IrcUser u in users )
            {
                u.ChangeName(newname);
            }

            return true;
        }

        public static bool SetBotFlag( string name )
        {
            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                return false;
            }

            foreach ( IrcUser u in users )
            {
                u.FlagBot();
            }

            return true;
        }

        public static bool IsBot( string name )
        {
            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                return false;
            }

            return users[0].HasBotFlag;
        }

        /// <summary>
        /// Get a user with a complete name match. (Ignores case)
        /// </summary>
        /// <param name="name">Complete username</param>
        /// <param name="channel">Channel to look for user</param>
        /// <returns>User; null when not found</returns>
        public static IrcUser GetUser( string name, string channel )
        {
            IrcUser user = Userlist.Find(delegate( IrcUser n )
            {
                if ( n.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) )
                {
                    if ( n.Channel.Equals(channel, StringComparison.CurrentCultureIgnoreCase) )
                    {
                        return true;
                    }
                }

                return false;
            });

            if ( user == null )
            {
                return null;
            }
            else
            {
                return user;
            }
        }

        public static IrcUser[] GetUsers( string name )
        {
            List<IrcUser> users = Userlist.FindAll(delegate( IrcUser n )
            {
                return n.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase);
            });

            if ( users == null || users.Count == 0 )
            {
                return null;
            }
            else
            {
                return users.ToArray();
            }
        }

        public static string[] GetChannelsForUser( string name )
        {
            List<IrcUser> users = Userlist.FindAll(delegate( IrcUser n )
            {
                return n.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase);
            });

            if ( users == null || users.Count == 0 )
            {
                return null;
            }
            else
            {
                List<string> chlist = new List<string>();

                foreach ( IrcUser user in users )
                {
                    if ( !chlist.Contains(user.Channel) )
                    {
                        chlist.Add(user.Channel);
                    }
                }

                return chlist.ToArray();
            }
        }

        public static IrcUser[] GetUsersForChannel( string channel )
        {
            List<IrcUser> users = Userlist.FindAll(delegate( IrcUser n )
            {
                return n.Channel.Equals(channel, StringComparison.CurrentCultureIgnoreCase);
            });

            if ( users == null || users.Count == 0 )
            {
                return null;
            }
            else
            {
                return users.ToArray();
            }
        }

        /// <summary>
        /// Get a user with a partial name match. (Ignores case)
        /// </summary>
        /// <param name="name">Partial username</param>
        /// <param name="channel">Channel to look for user</param>
        /// <returns>User; null when not found</returns>
        public static IrcUser GetUserPartial( string name, string channel )
        {
            IrcUser user = Userlist.Find(delegate( IrcUser n )
            {
                if ( n.Name.Contains(name) )
                {
                    if ( n.Channel.Equals(channel, StringComparison.CurrentCultureIgnoreCase) )
                    {
                        return true;
                    }
                }

                return false;
            });

            if ( user == null )
            {
                return null;
            }
            else
            {
                return user;
            }
        }
    }
}
