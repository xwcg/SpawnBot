using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.ServiceModel.Syndication;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using Twitterizer;

namespace SpawnBot
{
    class cIRC
    {
        private static IRC IrcObject;

        private static bool DebugMode = false;

        private bool Active = true;
        private string ActiveBot = "";

        private static string Botname = "";
        private static string ChannelName = "";
        private static string BotConfirmationSecret = "";
        private static string TwitterAccessToken = "";
        private static string TwitterAccessTokenSecret = "";
        private static string TwitterConsumerKey = "";
        private static string TwitterConsumerKeySecret = "";

        private static List<string> Channels = new List<string>();
        private static List<string> LPs = new List<string>();


        static void Main( string[] args )
        {
            ReadConfig();

            cIRC IrcApp = new cIRC("irc.quakenet.org", 6667, Botname, ChannelName);
        }

        private static void ReadConfig()
        {
            // Read config file
            if ( File.Exists("config.txt") )
            {
                StreamReader r = new StreamReader("config.txt");

                Botname = r.ReadLine();
                BotConfirmationSecret = r.ReadLine();

                string input;

                while ( ( input = r.ReadLine() ) != null )
                {
                    if ( ChannelName == "" )
                    {
                        ChannelName = input;
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

            // Read LP list

            input = "";

            if ( File.Exists("lp.txt") )
            {
                r = new StreamReader("lp.txt");

                while ( ( input = r.ReadLine() ) != null )
                {
                    LPs.Add(input);
                }

                r.Close();
                r.Dispose();
            }
            else
            {
                Logger.WriteLine("*** LP.TXT not found. Ignoring.", ConsoleColor.Yellow);
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

        private void SendMessage( string message, string channel )
        {
            IrcObject.IrcWriter.WriteLine(String.Format("PRIVMSG {0} :{1}", channel, message));
            IrcObject.IrcWriter.Flush();

            Logger.WriteLine(String.Format("-> ({0}) {1}", channel, message), ConsoleColor.DarkCyan);

            //Logger.WriteLine(String.Format("PRIVMSG {0} :{1}", channel, message));
        }

        private void SendCommand( string command )
        {
            Logger.WriteLine("-> " + command, ConsoleColor.DarkCyan);
            IrcObject.IrcWriter.WriteLine(command);
            IrcObject.IrcWriter.Flush();
        }

        private cIRC( string IrcServer, int IrcPort, string IrcUser, string IrcChan )
        {
            IrcObject = new IRC(IrcUser, IrcChan);

            // Assign events
            IrcObject.eventReceiving += new CommandReceived(IrcCommandReceived);
            IrcObject.eventTopicSet += new TopicSet(IrcTopicSet);
            IrcObject.eventTopicOwner += new TopicOwner(IrcTopicOwner);
            IrcObject.eventNamesList += new NamesList(IrcNamesList);
            IrcObject.eventServerMessage += new ServerMessage(IrcServerMessage);
            IrcObject.eventJoin += new Join(IrcJoin);
            IrcObject.eventPart += new Part(IrcPart);
            IrcObject.eventMode += new Mode(IrcMode);
            IrcObject.eventNickChange += new NickChange(IrcNickChange);
            IrcObject.eventKick += new Kick(IrcKick);
            IrcObject.eventQuit += new Quit(IrcQuit);
            IrcObject.eventMessage += new Message(IrcObject_eventMessage);

            // Connect to server	
            IrcObject.Connect(IrcServer, IrcPort);
        }

        private void PrivateMessageResponse( string user, string message )
        {
            if ( message.Contains("steam") || message.Contains("sale") || message.Contains("sales") )
            {
                string[] Sales = PollSales();

                for ( int i = 0; i < 5; i++ )
                {
                    SendMessage(( i + 1 ).ToString() + ". " + Sales[i], user);
                    Thread.Sleep(500);
                }
            }

            if ( message.Trim().StartsWith("DO YOU LOVE BEES?") )
            {
                if ( message.Substring(message.IndexOf(":") + 1).Trim() == GetSHA1Hash(ChannelName + "I really love bees" + Botname + BotConfirmationSecret) )
                {
                    IUsers.SetBotFlag(user);

                    SendMessage(BotConfirmationSecret, user);
                }
                else
                {
                    SendMessage("Dirty liar!", user);
                }
            }

            if ( message.Trim().StartsWith(BotConfirmationSecret) && user != Botname && Active )
            {
                IUsers.SetBotFlag(user);

                ActiveBot = user;
                SendMessage("Alright, I'll sit on the bench.", ChannelName);

                Active = false;
            }
        }

        private void ChannelMessageResponse( string User, string Message, string Channel )
        {
            if ( Message.Trim() == "Robot reporting for duty" && Active )
            {
                SendMessage("I'm already on duty.", Channel);
            }

            if ( Message.Trim() == "Alright, I'll sit on the bench." && Active )
            {
                if ( IUsers.IsBot(User) )
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
                SendMessage("DO YOU LOVE BEES? :" + GetSHA1Hash(ChannelName + "I really love bees" + User + BotConfirmationSecret), User);
                //ActiveBot = User;
                //SendMessage("Alright, I'll sit on the bench.", Channel);
                //Active = false;
            }

            if ( !Active )
            {
                return;
            }

            if ( Message.Contains("would of ") )
            {
                SendMessage("*would've/would have", Channel);
            }

            if ( Message.Contains("should of ") )
            {
                SendMessage("*should've/should have", Channel);
            }

            if ( Message.Contains("could of ") )
            {
                SendMessage("*could've/could have", Channel);
            }


            if ( Message.StartsWith("!") )
            {
                if ( User == "Mariondoe" )
                {
                    Random r = new Random();
                    if ( r.Next(0, 100) <= 25 )
                    {
                        SendMessage("Unexpected error 0x0001D107", Channel);
                        return;
                    }
                }

                string[] Parameters = Message.Split(' ');

                if ( Parameters[0].Length == 1 )
                {
                    return;
                }

                Parameters[0] = Parameters[0].Substring(1);

                ///////////////////////////

                switch ( Parameters[0] )
                {
                    case "roll":
                        if ( Parameters.Length > 1 )
                        {
                            Roll(Parameters[1], Channel);
                        }
                        else
                        {
                            SendMessage("Usage: !roll [dice OR number] - You can use a dice declaration like 2d20 or just a natural number like 20", Channel);
                        }
                        break;
                    case "ranks":
                        if ( Channel.Contains("mhykol") )
                        {
                            SendMessage("http://goo.gl/88F58", Channel);
                        }
                        break;
                    case "test":
                        SendMessage("Stop using !test, Marion.", Channel);
                        break;
                    case "flip":
                        Flip(Channel);
                        break;
                    case "coin":
                        Flip(Channel);
                        break;
                    case "server":
                        SendMessage("The Spawn.net Server IP is: 127.0.0.1", Channel);
                        break;
                    case "mumble":
                        SendMessage("Mumble Address: mumble.thespawn.net - Port: 64738", Channel);
                        break;
                    case "steam":
                        string sale = PollSteamSale();
                        if ( sale == null )
                        {
                            SendMessage("Error getting Steam sales.", Channel);
                            break;
                        }
                        SendMessage("Poor Man's Steam Sale of the Day: " + sale, Channel);
                        SendMessage("You can also ask me about steam sales privately and I'll tell you more", Channel);
                        break;
                    case "insult":
                        if ( Parameters.Length == 1 )
                        {
                            Insult(User, Channel);
                        }
                        else if ( Parameters[1] != Botname )
                        {
                            Insult(Parameters[1], Channel);
                        }
                        else
                        {
                            Insult(User, Channel);
                        }
                        break;
                    case "title":
                        if ( Parameters.Length > 1 )
                        {
                            string title = PollWebsiteTitle(Parameters[1]);
                            if ( title == null )
                            {
                                SendMessage("Error polling website title.", Channel);
                                break;
                            }
                            SendMessage("Title: " + title, Channel);
                        }
                        break;
                    case "lp":
                        if ( Parameters.Length > 1 )
                        {
                            if ( Parameters[1].Trim() == "add" )
                            {
                                if ( !IsOp(User, Channel) )
                                {
                                    SendMessage("Only @'s can do that.", Channel);
                                    return;
                                }

                                if ( Parameters.Length == 4 )
                                {
                                    AddLp(Parameters[2], Parameters[3], Channel);
                                }
                                else
                                {
                                    SendMessage("Invalid parameters. Usage: !lp add [name] [youtube url]", Channel);
                                }
                            }
                            else if ( Parameters[1].Trim() == "del" || Parameters[1].Trim() == "remove" )
                            {
                                if ( !IsOp(User, Channel) )
                                {
                                    SendMessage("Only @'s can do that.", Channel);
                                    return;
                                }

                                if ( Parameters.Length == 3 )
                                {
                                    DelLp(Parameters[2], Channel);
                                }
                                else
                                {
                                    SendMessage("Invalid parameters. Usage: !lp remove|del [name]", Channel);
                                }
                            }
                            else
                            {
                                GetLp(Parameters[1], Channel);
                            }
                        }
                        else
                        {
                            LpList(Channel);
                        }
                        break;
                }
            }

            if ( Message.Contains("youtube.com") || Message.Contains("youtu.be") || Message.Contains("vimeo.com") )
            {
                string[] Parameters = Message.Split(' ');
                foreach ( string p in Parameters )
                {
                    if ( p.Contains("youtube.com/watch?") || p.Contains("youtu.be/") || p.Contains("vimeo.com/") )
                    {
                        string title = PollWebsiteTitle(p);
                        if ( title == null )
                        {
                            break;
                        }
                        SendMessage("Title: " + title, Channel);
                        break;
                    }

                    if ( p.Contains("youtube.com/playlist?list=") )
                    {
                        string title = PollWebsiteTitle(p);
                        if ( title == null )
                        {
                            break;
                        }
                        SendMessage("Playlist Title: " + title, Channel);
                        break;
                    }
                }
            }

            if ( Message.Contains("twitter.com") && ( Message.Contains("/status/") || Message.Contains("/statuses/") ) )
            {
                string[] Parameters = Message.Split(' ');
                foreach ( string p in Parameters )
                {
                    if ( p.Contains("twitter.com") && ( p.Contains("/status/") || p.Contains("/statuses/") ) )
                    {
                        try
                        {
                            string parse = p.Replace("/statuses/", "/status/");

                            parse = parse.Substring(parse.IndexOf("/status/"));
                            parse = parse.Replace("/status/", "");

                            string text = PollTwitterStatus(Convert.ToDecimal(parse));

                            if ( text == null )
                            {
                                return;
                            }
                            else
                            {
                                SendMessage(text, Channel);
                                break;
                            }
                        }
                        catch ( Exception e )
                        {
                            Logger.WriteLine("***** Twitter error. Parsing: " + p, ConsoleColor.DarkRed);
                            return;
                        }
                    }
                }
            }

        }

        void IrcObject_eventMessage( string User, string Message, string Channel )
        {
            if ( !User.Equals("MMC", StringComparison.CurrentCultureIgnoreCase) )
            {
                Logger.WriteLine(String.Format("[{2}] {0}: {1}", User, Message, Channel));
            }

            if ( DebugMode )
            {
                return;
            }

            if ( Channel == Botname )
            {
                PrivateMessageResponse(User, Message);
            }
            else
            {
                ChannelMessageResponse(User, Message, Channel);
            }
        }

        private bool IsOp( string name, string channel )
        {
            IrcUser u = IUsers.GetUser(name, channel);
            if ( u == null )
            {
                Logger.WriteLine(String.Format("***** IsOp(Program.cs): Can't find user '{0}' in '{1}'", name, channel), ConsoleColor.DarkRed);
                return false;
            }

            return u.HasOp;
        }

        private void LpList( string Channel )
        {
            string response = "List of Let's Plays: ";

            foreach ( string lp in LPs )
            {
                string[] slp = lp.Split(' ');
                response = response + slp[0] + " ";
            }

            response = response + "( !lp [name] for more info )";

            SendMessage(response, Channel);
        }

        private void GetLp( string name, string Channel )
        {
            string lp = LPs.Find(delegate( string l )
            {
                return l.StartsWith(name, StringComparison.CurrentCultureIgnoreCase);
            });

            if ( lp == null || lp.Trim().Length == 0 )
            {
                SendMessage("I don't have any information on " + name, Channel);
            }
            else
            {
                SendMessage(lp, Channel);
            }
        }

        private void AddLp( string name, string ur, string Channel )
        {
            try
            {
                string newlp = String.Format("{0} {1}", name, ur);
                LPs.Add(newlp);
                LPs.Sort();

                SaveLP();

                SendMessage(name + " has been added", Channel);
            }
            catch ( Exception e )
            {
                Logger.WriteLine(e.Message);
                SendMessage("Error adding new LP", Channel);
            }
        }

        private void DelLp( string name, string Channel )
        {
            try
            {
                int i = LPs.FindIndex(delegate( string l )
                {
                    return l.StartsWith(name, StringComparison.CurrentCultureIgnoreCase);
                });

                if ( i < 0 )
                {
                    SendMessage("I couldn't find " + name + ". Did you check your spelling?", Channel);
                    return;
                }

                LPs.RemoveAt(i);
                SaveLP();

                SendMessage(name + " has been removed", Channel);
            }
            catch ( Exception e )
            {
                Logger.WriteLine("***** " + e.Message, ConsoleColor.DarkRed);
                SendMessage("Something has gone horribly wrong.", Channel);
            }
        }

        private void SaveLP()
        {
            StreamWriter w = new StreamWriter("lp.txt", false);
            foreach ( string lp in LPs )
            {
                w.WriteLine(lp);
            }

            w.Flush();
            w.Close();
            w.Dispose();
        }

        private void Roll( string param, string channel )
        {
            try
            {
                Random r = new Random();

                if ( param.Contains("d") )
                {
                    string[] parts = param.Split('+');
                    int basebonus = 0;
                    int roll = 0;

                    foreach ( string p in parts )
                    {
                        if ( p.Contains("d") )
                        {
                            int dice = Convert.ToInt32(p.Split('d')[1]);
                            int count = Convert.ToInt32(p.Split('d')[0]);

                            for ( int i = 0; i < count; i++ )
                            {
                                roll += r.Next(1, dice);
                            }
                        }
                        else
                        {
                            basebonus = Convert.ToInt32(p);
                        }
                    }

                    SendMessage("I\'ve rolled: " + ( basebonus + roll ).ToString(), channel);

                }
                else
                {
                    int var = Convert.ToInt32(param);

                    int result = r.Next(1, var);

                    SendMessage("I\'ve rolled: " + result.ToString(), channel);
                }
            }
            catch
            {
                SendMessage("I'm sorry Dave, I can't do that.", channel);
            }
        }

        private void Flip( string channel )
        {
            Random r = new Random();
            int result = r.Next(100);
            if ( result > 50 )
            {
                SendMessage("Heads!", channel);
            }
            else
            {
                SendMessage("Tails!", channel);
            }
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

            SendMessage(String.Format("{0}! You {1}, {2} {3}!", target, word1, word2, word3), channel);
        }

        private string PollSteamSale()
        {
            XmlReader r = XmlReader.Create("http://steamsales.rhekua.com/feed.php");

            SyndicationFeed f = SyndicationFeed.Load(r);

            string result = "";

            foreach ( SyndicationItem i in f.Items )
            {
                result = i.Title.Text;
                foreach ( SyndicationLink l in i.Links )
                {
                    result = result + " (" + l.Uri.ToString() + ")";
                    break;
                }
                return result;
            }

            return null;
        }

        private string[] PollSales()
        {
            XmlReader r = XmlReader.Create("http://steamsales.rhekua.com/feed.php");
            SyndicationFeed f = SyndicationFeed.Load(r);

            List<string> Sales = new List<string>();

            string result = "";

            foreach ( SyndicationItem i in f.Items )
            {
                result = i.Title.Text;
                foreach ( SyndicationLink l in i.Links )
                {
                    result = result + " (" + l.Uri.ToString() + ")";
                    break;
                }

                Sales.Add(result);

                result = "";
            }

            if ( Sales.Count > 0 )
            {
                return Sales.ToArray();
            }

            return null;
        }

        private string PollWebsiteTitle( string url )
        {
            try
            {
                Logger.WriteLine("* Polling Title for " + url, ConsoleColor.Yellow);

                string cUrl = url.Trim();
                if ( ( !cUrl.StartsWith("http://") && !cUrl.StartsWith("https://") ) && !cUrl.StartsWith("www.") )
                {
                    cUrl = "http://www." + cUrl;
                }

                if ( cUrl.StartsWith("www.") )
                {
                    cUrl = "http://" + cUrl;
                }

                WebClient x = new WebClient();
                string source = x.DownloadString(new Uri(cUrl));
                string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                x.Dispose();

                //HttpUtility.HtmlEncode // using System.Web

                if ( CapsCap(title) )
                {
                    return title.ToLower();
                }
                else
                {
                    return title;
                }

            }
            catch ( Exception e )
            {
                Logger.WriteLine("***** " + e.Message, ConsoleColor.DarkRed);
                return null;
            }
        }

        private string PollTwitterStatus( decimal StatusId )
        {
            OAuthTokens t = new OAuthTokens();
            t.AccessToken = TwitterAccessToken;
            t.AccessTokenSecret = TwitterAccessTokenSecret;
            t.ConsumerKey = TwitterConsumerKey;
            t.ConsumerSecret = TwitterConsumerKeySecret;

            TwitterResponse<TwitterStatus> status = TwitterStatus.Show(t, StatusId);

            if ( status.Result == RequestResult.Success )
            {
                return String.Format("@{0} posted: {1}", status.ResponseObject.User.ScreenName, status.ResponseObject.Text);
            }
            else
            {
                return null;
            }
        }

        private bool CapsCap( string s )
        {
            int count = 0;
            for ( int i = 0; i < s.Length; i++ )
            {
                if ( char.IsUpper(s[i]) )
                    count++;
            }

            if ( count >= ( s.Length / 2 ) )
            {
                return true;
            }

            return false;

        }

        #region Junk
        private void IrcCommandReceived( string IrcCommand )
        {
            //Logger.WriteLine(IrcCommand);
        }

        private void IrcTopicSet( string IrcChan, string IrcTopic )
        {
            Logger.WriteLine(String.Format("* Topic of {0} is: {1}", IrcChan, IrcTopic), ConsoleColor.DarkGreen);
        }

        private void IrcTopicOwner( string IrcChan, string IrcUser, string TopicDate )
        {
            Logger.WriteLine(String.Format("* Topic of {0} set by {1} on {2} (unixtime)", IrcChan, IrcUser, TopicDate), ConsoleColor.DarkGreen);
        }

        private void IrcNamesList( string UserNames, string IrcChannel )
        {
            //Logger.WriteLine(String.Format("Names List: {0}", UserNames));
            Logger.WriteLine("<- Name list received for " + IrcChannel, ConsoleColor.DarkMagenta);

            string[] list = UserNames.Split(' ');

            foreach ( string n in list )
            {
                IUsers.AddUser(n, IrcChannel);
                //Logger.WriteLine(n);
            }
        }

        private void IrcServerMessage( string ServerMessage )
        {
            Logger.WriteLine(String.Format("** {0}", ServerMessage), ConsoleColor.Green);
        }

        private void IrcJoin( string IrcChan, string IrcUser )
        {
            //IrcObject.IrcWriter.WriteLine(String.Format("NOTICE {0} :Hello {0}, welcome to {1}!", IrcUser, IrcChan));
            //IrcObject.IrcWriter.Flush ();

            IUsers.AddUser(IrcUser, IrcChan, false);
        }

        private void IrcPart( string IrcChan, string IrcUser )
        {
            //Logger.WriteLine(String.Format("{0} parts {1}", IrcUser, IrcChan));

            if ( IrcUser == ActiveBot )
            {
                //SendMessage("Ping", ChannelName);
                //VacantSearch = true;
                //vacancystart = DateTime.Now;
                SendMessage("I'll take over " + ActiveBot + "'s work now. The King is dead. Long live the King!", IrcChan);
                Active = true;
                ActiveBot = Botname;
            }

            IUsers.RemoveUser(IrcUser, IrcChan, "Part");
        }

        private void IrcMode( string IrcChan, string IrcUser, string UserMode )
        {
            //if ( IrcUser != IrcChan )
            //{
            //Logger.WriteLine(String.Format("* {0} sets {1} in {2}", IrcUser, UserMode, IrcChan), ConsoleColor.DarkGreen);
            //}

            if ( IrcUser == Botname && IrcUser == IrcChan )
            {
                foreach ( string channel in Channels )
                {
                    SendCommand("JOIN " + channel);
                }

                if ( !DebugMode )
                {
                    SendMessage("Robot reporting for duty", ChannelName);
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

            try
            {
                for ( int i = 0; i < modes.Length; i++ )
                {
                    switch ( modes[i] )
                    {
                        case 'o':
                            if ( add )
                            {
                                IUsers.GetUser(modeparts[i + 1], IrcChan).Op();
                            }
                            else
                            {
                                IUsers.GetUser(modeparts[i + 1], IrcChan).DeOp();
                            }
                            break;

                        case 'v':
                            if ( add )
                            {
                                IUsers.GetUser(modeparts[i + 1], IrcChan).Voice();
                            }
                            else
                            {
                                IUsers.GetUser(modeparts[i + 1], IrcChan).DeVoice();
                            }
                            break;
                        default:
                            Logger.WriteLine(String.Format("***** Unrecognized mode. ({0} sets {1} in {2})", IrcUser, UserMode, IrcChan), ConsoleColor.DarkRed);
                            break;
                    }
                }
            }
            catch ( Exception e )
            {
                Logger.WriteLine("***** Error setting mode (" + e.Message + ")", ConsoleColor.DarkRed);
            }
        }

        private void IrcNickChange( string UserOldNick, string UserNewNick )
        {
            //Logger.WriteLine(String.Format("{0} changes nick to {1}", UserOldNick, UserNewNick));

            IUsers.ChangeUserName(UserOldNick, UserNewNick);
        }

        private void IrcKick( string IrcChannel, string UserKicker, string UserKicked, string KickMessage )
        {
            //Logger.WriteLine(String.Format("{0} kicks {1} out {2} ({3})", UserKicker, UserKicked, IrcChannel, KickMessage));

            IUsers.RemoveUser(UserKicked, IrcChannel, String.Format("Kicked by {0} ({1})", UserKicker, KickMessage));
        }

        private void IrcQuit( string UserQuit, string QuitMessage )
        {
            //Logger.WriteLine(String.Format("{0} has quit IRC ({1})", UserQuit, QuitMessage));

            if ( UserQuit == ActiveBot )
            {
                //SendMessage("Ping", ChannelName);
                //VacantSearch = true;
                //vacancystart = DateTime.Now;
                SendMessage("I'll take over " + ActiveBot + "'s work now. The King is dead. Long live the King!", ChannelName);
                Active = true;
                ActiveBot = Botname;
            }

            IUsers.RemoveAllUser(UserQuit, String.Format("Quit ({0})", QuitMessage));
        }
        #endregion
    }
}
