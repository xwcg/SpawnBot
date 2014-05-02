using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using ConfigManager;

namespace SBLists
{
    public class Main : SBPlugin
    {
        private List<string> lists_index = new List<string>();
        private string replacer = "<->equal<->";

        /// <summary>
        /// Dictionary with all lists. Key is list name and value is list with contents
        /// </summary>
        private Dictionary<string, List<string>> lists = new Dictionary<string, List<string>>();

        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBLists";
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

                if (this.LoadLists() == true)
                    Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
                else
                    xLogger.Logger.WriteLine("*** Lists could not be loaded! Ignoring", ConsoleColor.DarkRed);
            }
        }


        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelCommandReceived(string name, string channel, string command, string[] parameters)
        {
            if (lists_index.Contains(command.ToLower()) && lists.ContainsKey(command.ToLower()))
            {
                if (parameters.Length > 0)
                {
                    switch (parameters[0].ToLower())
                    {
                        case "add":
                            if (parameters.Length < 2)
                            {
                                Host.PluginResponse(channel, String.Format("Usage: !{0} add [content]", command.ToLower()));
                            }
                            else
                            {
                                string cupContent = String.Join(" ", parameters, 1, parameters.Length - 1);
                                lists[command.ToLower()].Add(cupContent);

                                if (SaveLists() == false)
                                    Host.PluginResponse(channel, "Problem adding to list :(");
                                else
                                    Host.PluginResponse(channel, String.Format("Added to {0} list!  Cup No. '{1}' (Important for removal)", command.ToLower(), lists[command.ToLower()].Count));
                            }
                            break;

                        case "del":
                        case "delete":
                        case "rem":
                        case "remove":
                            if (lists[command.ToLower()].Count == 0)
                            {
                                Host.PluginResponse(channel, String.Format("There is nothing in {0} to be removed :(", command.ToLower()));
                            }
                            else
                            {
                                if (parameters.Length < 2)
                                {
                                    Host.PluginResponse(channel, String.Format("Usage: !{0} del [Cup No.]", command.ToLower()));
                                }
                                else
                                {
                                    if (Host.PluginUserManager.IsOperator(name, "#spawn"))
                                    {
                                        try
                                        {
                                            lists[command.ToLower()].RemoveAt(Convert.ToInt32(parameters[1]) - 1);

                                            if (SaveLists() == false)
                                                Host.PluginResponse(channel, "Problem removing from list :(");
                                            else
                                                Host.PluginResponse(channel, String.Format("Removed from list!", command.ToLower()));
                                        }
                                        catch (Exception ex)
                                        {
                                            Host.PluginResponse(channel, "Error while removing: " + ex.Message);
                                        }
                                    }
                                    else
                                    {
                                        Host.PluginResponse(channel, "Only OPs can remove from lists!");
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    if (lists[command.ToLower()].Count == 0)
                    {
                        Host.PluginResponse(channel, String.Format("There is no {0} :(", command.ToLower()));
                    }
                    else
                    {
                        Random r = new Random();
                        int i = r.Next(0, lists[command.ToLower()].Count);
                        string instantScience = lists[command.ToLower()][i];

                        Host.PluginResponse(channel, String.Format("Warm Cup of Instant {0} No. {1}: {2}", command.ToLower(), i + 1, instantScience));
                    }
                }
            }
            else
            {
                if (command.ToLower() == "list")
                {
                    if (Host.PluginUserManager.IsOperator(name, "#spawn"))
                    {
                        if (parameters.Length <= 1)
                        {
                            Host.PluginResponse(channel, "Usage: !list (add|del|rem) [listname]");
                        }
                        else
                        {

                            switch (parameters[0].ToLower())
                            {
                                case "add":
                                    lists_index.Add(parameters[1].ToLower());
                                    lists.Add(parameters[1].ToLower(), new List<string>());

                                    if (SaveLists() == false)
                                    {
                                        Host.PluginResponse(channel, "There was a problem creating a new list!");
                                    }
                                    else
                                    {
                                        Host.PluginResponse(channel, String.Format("Your list '{0}' has been added! ('!{0}' for random link, '!{0} add [content]' to add to list)", parameters[1].ToLower()));
                                    }
                                    break;
                                case "del":
                                case "delete":
                                case "rem":
                                case "remove":
                                    if (!lists_index.Contains(parameters[1].ToLower()))
                                        Host.PluginResponse(channel, String.Format("The list {0} does not exist!", parameters[1].ToLower()));
                                    else
                                    {
                                        lists_index.Remove(parameters[1].ToLower());
                                        lists.Remove(parameters[1].ToLower());

                                        if (SaveLists() == false)
                                        {
                                            Host.PluginResponse(channel, "There was a problem removing the list!");
                                        }
                                        else
                                        {
                                            Host.PluginResponse(channel, String.Format("The list '{0}' has been removed!", parameters[1].ToLower()));
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Only OPs can manage lists!");
                    }
                }
                else if (command.ToLower() == "lists")
                {
                    Host.PluginResponse(channel, "I currently have the following lists: " + String.Join(", ", lists_index.ToArray()));
                }
            }
        }

        bool LoadLists()
        {
            List<Config> cLists = Host.PluginConfigManager.Load(this.PluginName, "index_lists.cfg");

            lists_index.Clear();

            if (cLists == null)
                return false;

            foreach (Config l in cLists)
            {
                List<Config> thisList = Host.PluginConfigManager.Load(this.PluginName, String.Format("list_{0}.cfg", l.Value));

                if (thisList == null)
                    continue;

                lists_index.Add(l.Value);

                if (!lists.ContainsKey(l.Value))
                    lists.Add(l.Value, new List<string>());

                foreach (Config link in thisList)
                {
                    lists[l.Value].Add(link.Value.Replace(this.replacer, "="));
                }
            }

            return true;
        }

        bool SaveLists()
        {
            try
            {

                List<Config> cLists = new List<Config>();

                for (int i = 0; i < lists_index.Count; i++)
                {
                    cLists.Add(new Config(i.ToString(), lists_index[i]));
                }

                if (Host.PluginConfigManager.Save(cLists, this.PluginName, "index_lists.cfg") == false)
                    return false;

                foreach (KeyValuePair<string, List<string>> entry in lists)
                {
                    List<Config> thisList = new List<Config>();

                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        thisList.Add(new Config(i.ToString(), entry.Value[i]));
                    }

                    if (Host.PluginConfigManager.Save(thisList, this.PluginName, String.Format("list_{0}.cfg", entry.Key)) == false)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Host.PluginResponse("#spawn", "Error while saving:" + ex.Message);
                return false;
            }
        }


    }
}
