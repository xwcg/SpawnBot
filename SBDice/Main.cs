using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

namespace SBDice
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Dice and Randomizers";
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
                return "Provides dice and flip commands";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                Host.eventPluginChannelMessageReceived += new ChannelMessage(Host_eventPluginChannelMessageReceived);
                Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            switch ( command )
            {
                case "roll":
                    if ( parameters.Length == 1 )
                    {
                        Host.PluginResponse(channel, Roll(parameters[0]));
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Usage: !roll [dice OR number] - You can use a dice declaration like 2d20 or just a natural number like 20");
                    }
                    break;
                case "flip":
                    Host.PluginResponse(channel, Flip());
                    break;
                case "coin":
                    Host.PluginResponse(channel, Flip());
                    break;
            }
        }

        void Host_eventPluginChannelMessageReceived( string name, string message, string channel )
        {
        }

        private string Roll( string param )
        {
            string output = "I\'ve rolled: ";

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
                            int dice = Convert.ToInt32(p.Split('d')[1]) + 1;
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

                    output = output + ( basebonus + roll ).ToString();

                    return output;

                }
                else
                {
                    int var = Convert.ToInt32(param) + 1;

                    int result = r.Next(1, var);

                    output = output + result.ToString();

                    return output;
                }
            }
            catch
            {
                return "I'm sorry Dave, I can't do that.";
            }
        }

        private string Flip()
        {
            Random r = new Random();
            int result = r.Next(100);
            if ( result > 50 )
            {
                return "Heads!";
            }
            else
            {
                return "Tails";
            }
        }

    }
}
