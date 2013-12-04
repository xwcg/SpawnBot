using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
