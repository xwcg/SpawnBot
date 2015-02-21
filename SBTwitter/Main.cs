using ConfigManager;
using SBPluginInterface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using Tweetinvi;
using xLogger;

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

namespace SBTwitter
{
    public class Main : SBPlugin
    {
        private string AccessSecret;
        private string AccessToken;
        private string ConsumerKey;
        private string ConsumerSecret;
        private Dictionary<string, List<string>> follows = new Dictionary<string, List<string>>();
        private SBPluginHost Host;
        private Dictionary<string, string> lastSaid = new Dictionary<string, string>();

        #region SBPlugin Members

        public string Author
        {
            get
            {
                return "Michael Schwarz";
            }
        }

        public string Description
        {
            get
            {
                return "Parses links to Twitter statuses and verboses message to channel";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                List<Config> Cfgs = Host.PluginConfigManager.Load( "sbtwitter", "keys.cfg" );

                if ( Cfgs == null )
                {
                    Logger.WriteLine( "****** (SBTwitter) KEYS.CFG not found, Disabling. ******", ConsoleColor.Red );
                }
                else
                {
                    foreach ( Config c in Cfgs )
                    {
                        switch ( c.Index )
                        {
                            case "accesstoken":
                                AccessToken = c.Value;
                                break;

                            case "accesssecret":
                                AccessSecret = c.Value;
                                break;

                            case "consumerkey":
                                ConsumerKey = c.Value;
                                break;

                            case "consumersecret":
                                ConsumerSecret = c.Value;
                                break;
                        }
                    }

                    Tweetinvi.TwitterCredentials.SetCredentials( AccessToken, AccessSecret, ConsumerKey, ConsumerSecret );
                    Host.eventPluginChannelMessageReceived += new ChannelMessage( Host_eventPluginChannelMessageReceived );
                    Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
                    Host.eventPluginPrivateCommandReceived += Host_eventPluginPrivateCommandReceived;

                    var twitterStream = Stream.CreateFilteredStream();
                    twitterStream.AddTrack( "HerobrineIRC" );
                    twitterStream.MatchingTweetReceived += twitterStream_MatchingTweetReceived;
                    twitterStream.StartStreamMatchingAllConditionsAsync();

                    LoadConfig();
                    InitFollow();
                }
            }
        }

        void Host_eventPluginPrivateCommandReceived ( string name, string command, string[] parameters )
        {
            if ( name == "xwcg" && command == "twitterdebug" )
            {
                if ( stream != null )
                {
                    try
                    {
                        if ( ExceptionHandler.GetLastException() == null )
                        {
                            Host.PluginResponse( name, "No errors so far!" );
                        }
                        else
                        {
                            var exceptionStatusCode = ExceptionHandler.GetLastException().StatusCode;
                            var exceptionDescription = ExceptionHandler.GetLastException().TwitterDescription;
                            Host.PluginResponse( name, exceptionStatusCode + ": " + exceptionDescription );
                        }
                    }
                    catch ( Exception e )
                    {
                        Host.PluginResponse( name, "Exception while getting status:" + e.Message );
                    }

                }
            }
        }

        public string PluginName
        {
            get
            {
                return "Twitter Message Parser";
            }
        }

        public string Version
        {
            get
            {
                return "1.0";
            }
        }

        public string Website
        {
            get
            {
                return "";
            }
        }

        public void Dispose ()
        {
            return;
        }

        private void twitterStream_MatchingTweetReceived ( object sender, Tweetinvi.Core.Events.EventArguments.MatchedTweetReceivedEventArgs e )
        {
            string name = e.Tweet.Creator.ScreenName;
            string text = WebUtility.HtmlDecode( e.Tweet.Text );

            Regex hashMatch = new Regex( @"#\w+" );

            if ( text.IndexOf( "#" ) != -1 )
            {
                MatchCollection matches = hashMatch.Matches( text );
                if ( matches.Count > 0 )
                {
                    Host.PluginResponse( matches[0].Value, String.Format( "@{0} tweets you: {1}", name, text ) );
                }
            }
        }

        #endregion SBPlugin Members

        private void Host_eventPluginChannelCommandReceived ( string name, string channel, string command, string[] parameters )
        {
            switch ( command.ToLower() )
            {
                case "twitter":
                    if ( parameters.Length > 0 )
                    {
                        if ( Host.PluginUserManager.IsOperator( name, channel ) || Host.PluginUserManager.VerifyAdmin( name, channel ) )
                        {
                            switch ( parameters[0].ToLower() )
                            {
                                case "follow":
                                    if ( parameters.Length > 1 )
                                    {
                                        this.AddFollow( channel, parameters[1] );
                                        this.InitFollow();
                                        Host.PluginResponse( channel, String.Format( "Now following @{0} on Twitter for you!", parameters[1] ) );
                                    }
                                    else
                                    {
                                        Host.PluginResponse( channel, "Usage: !twitter follow [username]" );
                                    }
                                    break;

                                case "unfollow":
                                    if ( parameters.Length > 1 )
                                    {
                                        this.DelFollow( channel, parameters[1] );
                                        this.InitFollow();
                                        Host.PluginResponse( channel, String.Format( "Not following @{0} on Twitter for you anymore!", parameters[1] ) );
                                    }
                                    else
                                    {
                                        Host.PluginResponse( channel, "Usage: !twitter follow [username]" );
                                    }
                                    break;
                                case "restart":
                                    Host.PluginResponse(channel, "Restarting twitter stream...");
                                    this.InitFollow();
                                    Host.PluginResponse(channel, "Twitter stream restarted!");
                                    break;
                            }
                        }
                        else
                        {
                            Host.PluginResponse( channel, "Only @'s can do this!" );
                        }
                    }
                    else
                    {
                        Host.PluginResponse( channel, "Usage: !twitter (follow/unfollow) [Twitter Screen Name]" );
                    }
                    break;

                case "tweet":
                    string msg = String.Join( " ", parameters );
                    var t = Tweetinvi.Tweet.PublishTweet( String.Format( "<{0}@{1}> {2}", name, channel, msg ) );
                    Host.PluginResponse( channel, "Tweet sent! https://twitter.com/HerobrineIRC/status/" + t.IdStr );
                    break;

                case "tweetthat":
                case "tweetdat":
                case "tweetlast":
                    if ( lastSaid.ContainsKey( channel ) )
                    {
                        var t2 = Tweetinvi.Tweet.PublishTweet( lastSaid[channel] );
                        Host.PluginResponse( channel, "Tweeted last line! https://twitter.com/HerobrineIRC/status/" + t2.IdStr );
                    }
                    break;
            }
        }

        private void Host_eventPluginChannelMessageReceived ( string name, string message, string channel )
        {
            if ( !lastSaid.ContainsKey( channel ) )
                lastSaid.Add( channel, String.Format( "<{0}@{1}> {2}", name, channel, message ) );
            else
                lastSaid[channel] = String.Format( "<{0}@{1}> {2}", name, channel, message );

            if ( message.Contains( "twitter.com" ) && ( message.Contains( "/status/" ) || message.Contains( "/statuses/" ) ) )
            {
                string[] Parameters = message.Split( ' ' );
                foreach ( string p in Parameters )
                {
                    if ( p.Contains( "twitter.com" ) && ( p.Contains( "/status/" ) || p.Contains( "/statuses/" ) ) )
                    {
                        try
                        {
                            string parse = p.Replace( "/statuses/", "/status/" );

                            parse = parse.Substring( parse.IndexOf( "/status/" ) );
                            parse = parse.Replace( "/status/", "" );

                            string text = PollTwitterStatus( parse );

                            if ( text == null )
                            {
                                return;
                            }
                            else
                            {
                                Host.PluginResponse( channel, text );
                                break;
                            }
                        }
                        catch ( Exception e )
                        {
                            Logger.WriteLine( String.Format( "***** Twitter error. Parsing: {0} ({1})", p, e.Message ), ConsoleColor.DarkRed );
                            return;
                        }
                    }
                }
            }
        }

        private bool LoadConfig ()
            {
            //List<Config> cfgfollows = Host.PluginConfigManager.Load("sbtwitter", "follows.cfg");
            //if (cfgfollows == null)
            //    return false;
            //follows.Clear();
            //foreach (Config f in cfgfollows)
            //{
            //    string channel = f.Index;
            //    string[] tweeters = f.Value.Split(';');

            //    follows.Add(channel, new List<string>(tweeters));
            //}
            //
            //return true;

            Dictionary<string, List<string>> cfgFollows = Host.PluginConfigManager.Load<string, List<string>>( "sbtwitter", "follows.cfg" );
            if ( cfgFollows == null )
                return false;

            this.follows = cfgFollows;
            return true;
        }

        private bool SaveConfig ()
            {
            //List<Config> cfgFollows = new List<Config>();
            //foreach (KeyValuePair<string, List<string>> kp in follows)
            //{
            //    cfgFollows.Add(new Config(kp.Key, String.Join(";", kp.Value.ToArray())));
            //}

            return Host.PluginConfigManager.Save( follows, "sbtwitter", "follows.cfg" );
        }

        private bool AddFollow ( string channel, string tweeter )
        {
            if ( !this.follows.ContainsKey( channel ) )
            {
                List<string> newlist = new List<string>();
                newlist.Add( tweeter );
                this.follows.Add( channel, newlist );
            }
            else
            {
                if ( !this.follows[channel].Contains( tweeter ) )
                    this.follows[channel].Add( tweeter );
            }

            return SaveConfig();
        }

        private bool DelFollow ( string channel, string tweeter )
        {
            if ( this.follows.ContainsKey( channel ) )
            {
                if ( this.follows[channel].Contains( tweeter ) )
                {
                    this.follows[channel].Remove( tweeter );
                }
            }

            return SaveConfig();
        }

        private Tweetinvi.Core.Interfaces.Streaminvi.IFilteredStream stream = null;


        private void InitFollow ()
        {
            this.InitFollow( false );
        }

        private void InitFollow ( bool verbose )
        {
            if ( stream != null )
            {
                if ( verbose )
                    Host.PluginResponse( "xwcg", "Stopping Twitter Stream" );
                stream.StopStream();
            }

            stream = Stream.CreateFilteredStream();
            stream.MatchingTweetReceived += stream_MatchingTweetReceived;
            stream.WarningFallingBehindDetected += stream_WarningFallingBehindDetected;
            stream.DisconnectMessageReceived += stream_DisconnectMessageReceived;
            foreach ( KeyValuePair<string, List<string>> kp in this.follows )
            {
                string channel = kp.Key;
                List<string> tweeters = kp.Value;

                foreach ( string tweeter in tweeters )
                {
                    if ( verbose )
                        Host.PluginResponse( "xwcg", "Adding '" + tweeter + "' to Twitter Stream" );
                    //stream.AddTrack(tweeter);
                    stream.AddFollow( User.GetUserFromScreenName( tweeter ) );
                }
                }

            if ( verbose )
                Host.PluginResponse( "xwcg", "Starting Twitter Stream" );

            stream.StartStreamMatchingAnyConditionAsync();
            }

        void stream_DisconnectMessageReceived ( object sender, Tweetinvi.Core.Events.EventArguments.DisconnectMessageEventArgs e )
        {
            InitFollow();
        }

        void stream_WarningFallingBehindDetected ( object sender, Tweetinvi.Core.Events.EventArguments.WarningFallingBehindEventArgs e )
        {
            Host.PluginResponse( "xwcg", "Twitter: Warning Falling Behind! " + e.WarningMessage.Message );
        }

        private void stream_MatchingTweetReceived ( object sender, Tweetinvi.Core.Events.EventArguments.MatchedTweetReceivedEventArgs e )
        {
            Tweetinvi.Core.Interfaces.ITweet t = e.Tweet;

            foreach ( KeyValuePair<string, List<string>> kp in this.follows )
            {
                string channel = kp.Key;
                List<string> tweeters = kp.Value;
                foreach ( string tweeter in tweeters )
                {
                    if ( tweeter.ToLower() == t.Creator.ScreenName.ToLower() )
                    {
                        Host.PluginResponse( channel, String.Format( "@{0} just tweeted: {1}", t.Creator.ScreenName, WebUtility.HtmlDecode( t.Text ) ) );
                    }
                }
            }
        }

        private string PollTwitterStatus ( string StatusId )
        {
            string returnString = "@{0}";
            var tweet = Tweetinvi.Tweet.GetTweet( Convert.ToInt64( StatusId ) );

            if ( tweet.IsRetweet )
                returnString += " retweeted: {1}";
            else
                returnString += " tweeted: {1}";

            return String.Format( returnString, tweet.Creator.ScreenName, WebUtility.HtmlDecode( tweet.Text ) );
            //OAuthTokens t = new OAuthTokens();
            //t.AccessToken = AccessToken;
            //t.AccessTokenSecret = AccessSecret;
            //t.ConsumerKey = ConsumerKey;
            //t.ConsumerSecret = ConsumerSecret;

            //TwitterResponse<TwitterStatus> status = TwitterStatus.Show(t, StatusId);

            //if ( status.Result == RequestResult.Success )
            //{
            //    return String.Format("@{0} posted: {1}", status.ResponseObject.User.ScreenName, status.ResponseObject.Text);
            //}
            //else
            //{
            //    return null;
            //}
        }
    }
}