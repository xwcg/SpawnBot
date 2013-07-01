using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

namespace ConfigManager
{
    public class Config
    {
        private string _Index;
        private string _Value;

        public Config( string index, string value )
        {
            _Index = index;
            _Value = value;
        }

        public string Index
        {
            get
            {
                return _Index;
            }
        }

        public string Value
        {
            get
            {
                return _Value;
            }
        }
    }

    public class Manager
    {
        private string CurrentDir;

        public Manager()
        {
            if ( Directory.Exists(Environment.CurrentDirectory) )
            {
                if ( !Directory.Exists(Environment.CurrentDirectory + "\\config\\") )
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\config\\");
                }
            }
            else
            {
                throw new Exception("Base Directory does not exist");
            }

            CurrentDir = Environment.CurrentDirectory + "\\config\\";
        }

        public List<Config> Load( string PluginName, string FileName )
        {
            string LoadPath = CurrentDir;

            if ( PluginName != null && PluginName.Length > 0 )
            {
                LoadPath += PluginName + "\\";
            }

            if ( !Directory.Exists(LoadPath) )
            {
                Directory.CreateDirectory(LoadPath);
            }

            if ( FileName != null && FileName.Length > 0 )
            {
                LoadPath += FileName;
            }
            else
            {
                return null;
            }

            if ( !File.Exists(LoadPath) )
            {
                FileStream newfile = File.Create(LoadPath);
                newfile.Flush();
                newfile.Close();
                newfile.Dispose();
            }

            List<Config> Output = new List<Config>();
            StreamReader r = new StreamReader(LoadPath);

            string input;

            while ( ( input = r.ReadLine() ) != null )
            {
                // Skip over comments
                if ( input.StartsWith("//") )
                {
                    continue;
                }

                // Otherwise, check if valid line and add
                if ( input.Length > 2 && input.Contains("=") )
                {
                    string[] parts = input.Split('=');
                    Output.Add(new Config(parts[0], parts[1]));
                }
            }

            r.Close();
            r.Dispose();

            return Output;
        }

        public bool Save( List<Config> bits, string PluginName, string FileName )
        {
            if ( bits == null || bits.Count == 0 )
            {
                return false;
            }

            string SavePath = CurrentDir;

            if ( PluginName != null && PluginName.Length > 0 )
            {
                SavePath += PluginName + "\\";
            }

            if ( !Directory.Exists(SavePath) )
            {
                Directory.CreateDirectory(SavePath);
            }

            if ( FileName != null && FileName.Length > 0 )
            {
                SavePath += FileName;
            }
            else
            {
                return false;
            }

            //if ( !File.Exists(SavePath) )
            //{
            //    File.Create(SavePath);
            //}

            try
            {
                StreamWriter w = new StreamWriter(SavePath, false);

                foreach ( Config c in bits )
                {
                    w.WriteLine("{0}={1}", c.Index, c.Value);
                }

                w.Flush();
                w.Close();
                w.Dispose();
            }
            catch
            {
                return false;
            }

            return true;

        }
    }
}
