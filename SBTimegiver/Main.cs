using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.Timers;

namespace SBTimegiver
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private Timer timer = new Timer();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBTimegiver";
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


        public delegate void Tick();

        public event Tick onSecond;
        public event Tick onMinute;
        public event Tick onHour;

    }
}
