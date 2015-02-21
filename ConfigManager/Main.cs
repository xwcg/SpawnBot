using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

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

namespace ConfigManager
{
    public class Config
    {
        private string _Index;
        private string _Value;

        public Config ( string index, string value, bool isEscaped )
        {
            if ( isEscaped )
            {
                EscapedIndex = index;
                EscapedValue = value;
            }
            else
            {
                _Index = index;
                _Value = value;
            }
        }

        public Config ( string index, string value )
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

        public string EscapedIndex
        {
            get
            {
                return _Index.Replace( "=", "\\=" );
            }
            set
            {
                _Index = value.Replace( "\\=", "=" );
            }
        }

        public string Value
        {
            get
            {
                return _Value;
            }
        }

        public string EscapedValue
        {
            get
            {
                return _Value.Replace( "=", "\\=" );
            }
            set
            {
                _Value = value.Replace( "\\=", "=" );
            }
        }
    }

    public class Manager
    {
        private string CurrentDir;

        public Manager ()
        {
            string localDir = Environment.CurrentDirectory;
            if ( Directory.Exists( localDir ) )
            {
                if ( !Directory.Exists( Path.Combine( localDir, "config" ) ) )
                {
                    Directory.CreateDirectory( Path.Combine( localDir, "config" ) );
                }
            }
            else
            {
                throw new Exception( "Base Directory does not exist" );
            }

            CurrentDir = Path.Combine( localDir, "config" );
        }

        public Dictionary<T, T2> Load<T, T2> ( string PluginName, string FileName )
        {
            List<Config> cfgs = this.Load( PluginName, FileName );
            if ( cfgs == null )
                return null;

            Dictionary<T, T2> ret = new Dictionary<T, T2>();

            if ( typeof( T2 ) == ( new List<string>() ).GetType() )
            {
                foreach ( Config i in cfgs )
                {
                    T key = (T)Convert.ChangeType( i.Index, typeof( T ) );
                    List<string> vals = new List<string>( i.Value.Split( ';' ) );
                    T2 val = (T2)Convert.ChangeType( vals, typeof( T2 ) );
                    ret.Add( key, val );
                }
            }
            else
            {
                foreach ( Config i in cfgs )
                {
                    T key = (T)Convert.ChangeType( i.Index, typeof( T ) );
                    T2 val = (T2)Convert.ChangeType( i.Value, typeof( T2 ) );
                    ret.Add( key, val );
                }
            }
            return ret;
        }

        public List<T> Load<T> ( string PluginName, string FileName )
        {
            List<Config> cfgs = this.Load( PluginName, FileName );
            if ( cfgs == null )
                return null;

            List<T> ret = new List<T>();
            foreach ( Config i in cfgs )
            {
                ret.Add( (T)Convert.ChangeType( i.Value, typeof( T ) ) );
            }

            return ret;
        }

        public List<Config> Load ( string PluginName, string FileName )
        {
            string LoadPath = CurrentDir;

            if ( PluginName != null && PluginName.Length > 0 )
            {
                LoadPath = Path.Combine( LoadPath, PluginName );
            }

            if ( !Directory.Exists( LoadPath ) )
            {
                Directory.CreateDirectory( LoadPath );
            }

            if ( FileName != null && FileName.Length > 0 )
            {
                LoadPath = Path.Combine( LoadPath, FileName );
            }
            else
            {
                return null;
            }

            if ( !File.Exists( LoadPath ) )
            {
                FileStream newfile = File.Create( LoadPath );
                newfile.Flush();
                newfile.Close();
                //newfile.Dispose();
            }

            List<Config> Output = new List<Config>();
            StreamReader r = new StreamReader( LoadPath );

            string input;

            Regex configMatcher = new Regex( @"([^\\=]*(\\=)*\\*)+", RegexOptions.ECMAScript );

            while ( ( input = r.ReadLine() ) != null )
            {
                // Skip over comments
                if ( input.StartsWith( "//" ) )
                {
                    continue;
                }

                // Otherwise, check if valid line and add
                if ( input.Length > 3 && input.Contains( "=" ) )
                {
                    MatchCollection parts = configMatcher.Matches( input );
                    if ( parts.Count == 4 )
                    {
                        string key = parts[0].Value;
                        string value = parts[2].Value;
                        //Console.WriteLine( "{0} ::: {1}", key, value );
                        Output.Add( new Config( key, value, true ) );
                    }
                    //string[] parts = input.Split('=');
                    //string value = parts[1];
                    //if (parts.Length > 2)
                    //{
                    //    value = String.Join("=", parts, 1, parts.Length - 1);
                    //}
                    //
                    //Output.Add(new Config(parts[0], value));
                }
            }

            r.Close();
            //r.Dispose();

            return Output;
        }

        /// <summary>
        /// Save a String-list to a config file for a plugin and filename
        /// </summary>
        /// <param name="bits">List to save</param>
        /// <param name="PluginName">Name of the plugin</param>
        /// <param name="FileName">Name of the file</param>
        /// <returns>true if saved, false on error</returns>
        public bool Save ( List<string> bits, string PluginName, string FileName )
        {
            List<Config> newBits = new List<Config>();
            for ( int i = 0; i < bits.Count; i++ )
                newBits.Add( new Config( i.ToString(), bits[i] ) );

            return this.Save( newBits, PluginName, FileName );
        }

        /// <summary>
        /// Save a Dictionary with a string key and string value to a config file for a plugin and filename
        /// </summary>
        /// <param name="bits">Dictionary to save</param>
        /// <param name="PluginName">Name of the plugin</param>
        /// <param name="FileName">Name of the file</param>
        /// <returns>true if saved, false on error</returns>
        public bool Save ( Dictionary<string, string> bits, string PluginName, string FileName )
        {
            List<Config> newBits = new List<Config>();
            foreach ( KeyValuePair<string, string> b in bits )
                newBits.Add( new Config( b.Key, b.Value ) );

            return this.Save( newBits, PluginName, FileName );
        }

        /// <summary>
        /// Save a Dictionary with a string key and String-list value to a config file for a plugin and filename
        /// </summary>
        /// <param name="bits">Dictionary to save</param>
        /// <param name="PluginName">Name of the plugin</param>
        /// <param name="FileName">Name of the file</param>
        /// <returns>true if saved, false on error</returns>
        public bool Save ( Dictionary<string, List<string>> bits, string PluginName, string FileName )
        {
            List<Config> newBits = new List<Config>();
            foreach ( KeyValuePair<string, List<string>> b in bits )
                newBits.Add( new Config( b.Key, String.Join( ";", b.Value.ToArray() ) ) );

            return this.Save( newBits, PluginName, FileName );
        }

        /// <summary>
        /// Saves a Config item list to a config file for a plugin and filename
        /// </summary>
        /// <param name="bits">Config items to save</param>
        /// <param name="PluginName">Name of the plugin</param>
        /// <param name="FileName">Name of the file</param>
        /// <returns>true if saved, false on error</returns>
        public bool Save ( List<Config> bits, string PluginName, string FileName )
        {
            if ( bits == null )
            {
                return false;
            }

            string SavePath = CurrentDir;

            if ( PluginName != null && PluginName.Length > 0 )
            {
                SavePath = Path.Combine( SavePath, PluginName );
            }

            if ( !Directory.Exists( SavePath ) )
            {
                Directory.CreateDirectory( SavePath );
            }

            if ( FileName != null && FileName.Length > 0 )
            {
                SavePath = Path.Combine( SavePath, FileName );
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
                StreamWriter w = new StreamWriter( SavePath, false );

                foreach ( Config c in bits )
                {
                    w.WriteLine( "{0}={1}", c.EscapedIndex, c.EscapedValue );
                }

                w.Flush();
                w.Close();
                //w.Dispose();
            }
            catch ( Exception ex )
            {
                return false;
            }

            return true;

        }
    }
}
