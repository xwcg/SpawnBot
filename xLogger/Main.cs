using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace xLogger
{
    /// <summary>
    /// Provides a logging class. Creates a timestamped log file.
    /// </summary>
    public static class Logger
    {
        private static object[] DateContent = { DateTime.Now.Year.ToString("0000"), 
                                                  DateTime.Now.Month.ToString("00"), 
                                                  DateTime.Now.Day.ToString("00"), 
                                                  DateTime.Now.Hour.ToString("00"), 
                                                  DateTime.Now.Minute.ToString("00"), 
                                                  DateTime.Now.Second.ToString("00")};

        private static StreamWriter LogFile = new StreamWriter(String.Format("{0}-{1}-{2}_{3}-{4}-{5}.txt", DateContent));
        
        /// <summary>
        /// Write text to console and logfile
        /// </summary>
        /// <param name="txt">Text</param>
        /// <param name="clr">Text color (for console window)</param>
        public static void Write( string txt, ConsoleColor clr )
        {
            Console.ForegroundColor = clr;
            Console.Write(txt);
            Console.ForegroundColor = ConsoleColor.Gray;

            LogFile.Write(txt);
            LogFile.Flush();
        }

        /// <summary>
        /// Write text to console and logfile
        /// </summary>
        /// <param name="txt">Text</param>
        public static void Write( string txt )
        {
            Write(txt, ConsoleColor.Gray);
        }

        /// <summary>
        /// Write line to console and logfile
        /// </summary>
        /// <param name="txt">Line text</param>
        /// <param name="clr">Line color (for console window)</param>
        public static void WriteLine( string txt, ConsoleColor clr )
        {
            Console.ForegroundColor = clr;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.Gray;

            LogFile.WriteLine(txt);
            LogFile.Flush();
        }

        /// <summary>
        /// Write line to console and logfile
        /// </summary>
        /// <param name="txt">Line text</param>
        public static void WriteLine( string txt )
        {
            WriteLine(txt, ConsoleColor.Gray);
        }
    }
}
