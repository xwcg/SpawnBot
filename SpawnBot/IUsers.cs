using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xLogger;

namespace SpawnBot
{
    public class IrcUser
    {
        private bool isOp = false;
        private bool isVoice = false;
        private bool isBot = false;
        private string name;
        private string channel;

        public IrcUser( string UserName, string ChannelName )
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
        }

        public void ChangeName( string newname )
        {
            Logger.WriteLine(String.Format("* Changed '{0}' name to '{1}'", name, newname), ConsoleColor.DarkYellow);
            name = newname;
        }

        public void Op()
        {
            isVoice = true;
            isOp = true;

            Logger.WriteLine(String.Format("* Op'ped '{0}' in {1}", name, channel), ConsoleColor.DarkYellow);
        }

        public void DeOp()
        {
            isOp = false;

            Logger.WriteLine(String.Format("* DeOp'ped '{0}' in {1}", name, channel), ConsoleColor.DarkYellow);
        }

        public void Voice()
        {
            isVoice = true;

            Logger.WriteLine(String.Format("* Voiced '{0}' in {1}", name, channel), ConsoleColor.DarkYellow);
        }

        public void DeVoice()
        {
            isVoice = false;

            Logger.WriteLine(String.Format("* UnVoiced '{0}' in {1}", name, channel), ConsoleColor.DarkYellow);
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
    }

    public static class IUsers
    {
        private static List<IrcUser> Userlist = new List<IrcUser>();

        public static void RemoveUser( string name, string channel)
        {
            RemoveUser(name, channel, "null");
        }

        public static void RemoveUser( string name, string channel, string reason )
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
                Logger.WriteLine(String.Format("***** RemoveUser: Can't find user '{0}' in '{1}'", name, channel), ConsoleColor.DarkRed);
            }
            else
            {
                Userlist.RemoveAt(user);
                Logger.WriteLine(String.Format("* RemoveUser: Removed user '{0}' in '{1}' from Userlist ({2})", name, channel, reason), ConsoleColor.Yellow);
            }
        }

        public static void RemoveAllUser( string name )
        {
            RemoveAllUser(name, "null");
        }

        public static void RemoveAllUser( string name, string reason )
        {
            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                Logger.WriteLine(String.Format("***** RemoveAllUser: Can't find any user '{0}'", name), ConsoleColor.DarkRed);
                return;
            }

            foreach ( IrcUser u in users )
            {
                RemoveUser(u.Name, u.Channel, reason);
            }
        }

        public static void AddUser( string name, string channel )
        {
            AddUser(name, channel, true);
        }

        public static void AddUser( string name, string channel, bool noVerbose )
        {
            Userlist.Add(new IrcUser(name, channel));

            if ( !noVerbose )
            {
                Logger.WriteLine(String.Format("* AddUser: Added user '{0}' in '{1}' to Userlist", name, channel), ConsoleColor.Yellow);
            }
        }

        public static void ChangeUserName( string name, string newname )
        {
            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                Logger.WriteLine(String.Format("***** ChangeUserName: Can't find any user '{0}'", name), ConsoleColor.DarkRed);
                return;
            }

            foreach ( IrcUser u in users )
            {
                u.ChangeName(newname);
            }
        }

        public static void SetBotFlag( string name )
        {
            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                Logger.WriteLine(String.Format("***** SetBotFlag: Can't find any user '{0}'", name), ConsoleColor.DarkRed);
                return;
            }

            foreach ( IrcUser u in users )
            {
                u.FlagBot();
            }
        }

        public static bool IsBot( string name )
        {
            IrcUser[] users = GetUsers(name);

            if ( users == null )
            {
                Logger.WriteLine(String.Format("***** IsBot: Can't find any user '{0}'", name), ConsoleColor.DarkRed);
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
