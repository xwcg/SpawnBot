using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.Timers;

namespace SBTimegiver
{
    public class Main : SBTimePlugin
    {
        private SBPluginHost Host;
        private Timer timer = new Timer();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SpawnBot TimeGiver";
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
                return "Watashi";
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
                return "Description";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                timer.Interval = 1000;
                timer.AutoReset = true;
                timer.Elapsed += timer_Elapsed;

                timer.Enabled = true;
            }
        }
        public void Dispose()
        {
            return;
        }

        #endregion

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (onSecond != null)
                onSecond();
        }

        public event TimeTick onSecond;
    }
}
