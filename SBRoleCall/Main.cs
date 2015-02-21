using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;

namespace SBRoleCall
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private Dictionary<string, List<string>> Roles = new Dictionary<string, List<string>>();

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBRoleCall";
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

                Roles = Host.PluginConfigManager.Load<string, List<string>>(PluginName, "roles.cfg");

                Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelCommandReceived(string name, string channel, string command, string[] parameters)
        {
            if (command == "rolecall")
            {
                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "create":
                            if (parameters.Length > 1)
                            {
                                string roleName = parameters[1].ToLower();
                                if (!Roles.ContainsKey(roleName))
                                {
                                    Roles.Add(roleName, new List<string>());
                                    SaveRoles();

                                    Host.PluginResponse(channel, String.Format("Rolecall {0} added! Join with '!{0} join' and leave with '!{0} leave'. Call everyone with !{0}", roleName));
                                }
                                else
                                {
                                    Host.PluginResponse(channel, String.Format("Rolecall {0} already exists!", roleName));
                                }
                            }
                            else
                            {
                                Host.PluginResponse(channel, "Usage: !rolecall create [RoleName]");
                            }
                            break;
                        case "remove":
                            if (parameters.Length > 1)
                            {
                                string roleName = parameters[1].ToLower();

                                if (Host.PluginUserManager.IsOperator(name, channel) || Host.PluginUserManager.VerifyAdmin(name, channel))
                                {
                                    if (Roles.ContainsKey(roleName))
                                    {
                                        Roles.Remove(roleName);
                                        SaveRoles();

                                        Host.PluginResponse(channel, String.Format("Rolecall {0} has been removed!", roleName));
                                    }
                                    else
                                    {
                                        Host.PluginResponse(channel, String.Format("Rolecall {0} does not exist!", roleName));
                                    }
                                }
                                else
                                {
                                    Host.PluginResponse(channel, String.Format("Only operators can do this (This command deletes the entire list, to leave the list use !{0} leave)", roleName));
                                }
                            }
                            else
                            {
                                Host.PluginResponse(channel, "Usage: !rolecall remove [RoleName]");
                            }
                            break;
                    }
                }
            }
            else if (Roles.ContainsKey(command))
            {
                if (parameters.Length > 0)
                {
                    switch (parameters[0])
                    {
                        case "join":
                            if (!Roles[command].Contains(name))
                            {
                                Roles[command].Add(name);
                                SaveRoles();

                                Host.PluginResponse(channel, String.Format("You have now joined {0} :)", command));
                            }
                            else
                            {
                                Host.PluginResponse(channel, String.Format("You have already joined {0}!", command));
                            }
                            break;

                        case "leave":
                            if (Roles[command].Contains(name))
                            {
                                Roles[command].Remove(name);
                                SaveRoles();

                                Host.PluginResponse(channel, String.Format("You have now left {0} :(", command));
                            }
                            else
                            {
                                Host.PluginResponse(channel, String.Format("You are not in {0}!", command));
                            }
                            break;

                        default:
                            Host.PluginResponse(channel, String.Format("Usage: !{0} (join|leave)", command));
                            break;
                    }
                }
                else
                {
                    Host.PluginResponse(channel, String.Format("Calling all {0}-ers! {1}", command, String.Join(", ", Roles[command].ToArray())));
                }
            }
        }

        private void SaveRoles()
        {
            Host.PluginConfigManager.Save(Roles, PluginName, "roles.cfg");
        }

    }
}
