using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SpawnBot
{
    public static class Logger
    {
        private static object[] DateContent = { DateTime.Now.Year.ToString("0000"), 
                                                  DateTime.Now.Month.ToString("00"), 
                                                  DateTime.Now.Day.ToString("00"), 
                                                  DateTime.Now.Hour.ToString("00"), 
                                                  DateTime.Now.Minute.ToString("00"), 
                                                  DateTime.Now.Second.ToString("00")};

        private static StreamWriter LogFile = new StreamWriter(String.Format("{0}-{1}-{2}_{3}-{4}-{5}.txt", DateContent));

        public static void Write( string txt, ConsoleColor clr )
        {
            Console.ForegroundColor = clr;
            Console.Write(txt);
            Console.ForegroundColor = ConsoleColor.Gray;

            LogFile.Write(txt);
            LogFile.Flush();
        }

        public static void Write( string txt )
        {
            Write(txt, ConsoleColor.Gray);
        }

        /// <summary>
        /// Write line to console
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="clr">DarkCyan for outgoing, DarkMagenta for incoming, DarkRed for error, (dark)Yellow for log</param>
        public static void WriteLine( string txt, ConsoleColor clr )
        {
            Console.ForegroundColor = clr;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.Gray;

            LogFile.WriteLine(txt);
            LogFile.Flush();
        }

        public static void WriteLine( string txt )
        {
            WriteLine(txt, ConsoleColor.Gray);
        }
    }
}
