using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xIrcNet;
using xLogger;

namespace NetTestApp
{
    class Program
    {
        static IRC test;

        static void Main( string[] args )
        {
            test = new IRC("testbot", "irc.quakenet.org", 6667);

            test.eventCommandReceived += new IrcCommandReceived(test_eventCommandReceived);

            test.eventConnected += new IrcConnected(test_eventConnected);
            test.eventDisconnected += new IrcDisconnected(test_eventDisconnected);
            test.eventConnectingError += new IrcConnectingError(test_eventConnectingError);
            test.eventCommandSent += new IrcCommandSent(test_eventCommandSent);
            test.eventRawBotModeReceived += new IrcRawBotModeGet(test_eventRawBotModeReceived);
            test.eventUserModeChanged += new IrcUserMode(test_eventUserModeChanged);
            test.eventMessageReceived += new IrcMessage(test_eventMessageReceived);
            test.eventNameListReceived += new IrcNameListReceived(test_eventNameListReceived);
            test.eventUserKicked += new IrcUserKicked(test_eventUserKicked);

            string cmd;

            while ( ( cmd = Console.ReadLine() ) != null )
            {
                if ( cmd.StartsWith("say") )
                {
                    string[] parts = cmd.Split(' ');

                    string channel = parts[1];
                    string message = cmd.Substring(cmd.IndexOf(channel) + channel.Length + 1);

                    test.SendCommand(String.Format("PRIVMSG {0} :{1}", channel, message));
                    continue;
                }

                switch ( cmd.ToLower() )
                {
                    case "quit":
                        test.Disconnect();
                        Environment.Exit(0);
                        break;
                    case "connect":
                        test.Connect();
                        break;
                    case "disconnect":
                        test.Disconnect();
                        break;
                    default:
                        Console.WriteLine("Unknown command '" + cmd + "'");
                        break;
                }
            }
        }

        static void test_eventUserKicked( string channel, string name, string by, string message )
        {
            Logger.WriteLine(name + " was kicked by " + by + " in " + channel + " for " + message, ConsoleColor.Yellow);
        }

        static void test_eventNameListReceived( string channel, string[] list )
        {
            Logger.WriteLine("USER LIST FOR " + channel, ConsoleColor.Blue);
            foreach ( string name in list )
            {
                Logger.WriteLine(name, ConsoleColor.DarkBlue);
            }
            Logger.WriteLine("END OF LIST");
        }

        static void test_eventMessageReceived( string channel, string name, string message )
        {
            Logger.WriteLine(String.Format("({0}) {1}: {2}", channel, name, message), ConsoleColor.White);
        }

        static void test_eventUserModeChanged( string channel, string name, string mode, string by )
        {
            Logger.WriteLine(String.Format("({0}) MODE {1} TO {2} BY {3}", new object[] { channel, mode, name, by }), ConsoleColor.Yellow);
        }

        static void test_eventRawBotModeReceived( string mode )
        {
            if ( mode == "+i" )
            {
                test.SendCommand("JOIN #thepretz");
            }
        }

        static void test_eventRawMotdLineReceived( string line )
        {
            Logger.WriteLine("MOTD: " + line, ConsoleColor.Yellow);
        }

        static void test_eventCommandReceived( string cmd )
        {
            Logger.WriteLine("<- " + cmd, ConsoleColor.DarkMagenta);
        }

        static void test_eventConnectingError( string error )
        {
            Logger.WriteLine("Connecting Error! : " + error, ConsoleColor.Red);
        }

        static void test_eventCommandSent( string cmd )
        {
            Logger.WriteLine("-> " + cmd, ConsoleColor.DarkCyan);
        }

        static void test_eventConnected()
        {
            Logger.WriteLine("Connected!", ConsoleColor.Green);
        }

        static void test_eventDisconnected( string msg )
        {
            Logger.WriteLine("Disconnected!", ConsoleColor.Green);
        }
    }
}
