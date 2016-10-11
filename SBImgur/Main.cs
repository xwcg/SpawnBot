using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.Net;
using Newtonsoft.Json;

namespace SBImgur
{
    public class Main : SBPlugin
    {
        private SBPluginHost Host;
        private string ImgurApiClientID = "";

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "SBImgur";
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

                Host.eventPluginChannelCommandReceived += Host_eventPluginChannelCommandReceived;
                Dictionary<string, string> config = Host.PluginConfigManager.Load<string, string>(this.PluginName, "api.cfg");
                if (config != null && config.Count > 0 && config.ContainsKey("clientid"))
                {
                    this.ImgurApiClientID = config["clientid"];
                }
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        private void Host_eventPluginChannelCommandReceived(string name, string channel, string command, string[] parameters)
        {
            if (command.ToLower() == "r")
            {
                if (parameters.Length == 1)
                {
                    string result = fetchImageFromSub(parameters[0]);
                    Host.PluginResponse(channel, result);
                }
                else
                {
                    Host.PluginResponse(channel, "Usage: !r [SubredditName] - Ex.: !r kittens");
                }
            }
        }

        private string fetchImageFromSub(string sub)
        {
            string responseString = "YOU SHOULD NOT SEE THIS TEXT.";
            try
            {
                WebClient x = new WebClient();
                x.Headers.Add("Authorization", "Client-ID " + this.ImgurApiClientID);
                responseString = x.DownloadString(String.Format("https://api.imgur.com/3/gallery/r/{0}", sub));

                ImgurResponse response = JsonConvert.DeserializeObject<ImgurResponse>(responseString);
                responseString = "";

                if (response.data != null)
                {
                    if (response.data.Count > 0)
                    {
                        Random r = new Random();
                        ImgurImage winner = response.data[r.Next(0, response.data.Count - 1)];

                        responseString = (winner.nsfw ? "(NSFW) " : "") + String.Format("{0} - {1}", winner.title, winner.link);
                    }
                    else
                    {
                        responseString = "No images found in /r/" + sub;
                    }
                }
                else
                {
                    responseString = "Faulty response from Imgur.";
                }

            }
            catch (Exception ex)
            {
                responseString = "An error has occured: " + ex.Message;
            }
            //xLogger.Logger.WriteLine(responseString);

            //string source = x.DownloadString(new Uri(urls[command]));

            return responseString;
        }

        private struct ImgurResponse
        {
            public List<ImgurImage> data;
            public bool success;
            public int status;
        }

        private struct ImgurImage
        {
            /*
              "id":"WIgyxxs",
         "title":"Baby Otter In A Boot",
         "description":null,
         "datetime":1475194334,
         "type":"image\/jpeg",
         "animated":false,
         "width":500,
         "height":381,
         "size":28718,
         "views":15905,
         "bandwidth":456759790,
         "vote":null,
         "favorite":false,
         "nsfw":false,
         "section":"Otters",
         "account_url":null,
         "account_id":null,
         "is_ad":false,
         "in_gallery":false,
         "link":"http:\/\/i.imgur.com\/WIgyxxs.jpg",
         "comment_count":null,
         "ups":null,
         "downs":null,
         "points":null,
         "score":15409,
         "is_album":false
            */
            public string id;
            public string title;
            public bool nsfw;
            public string link;
        }
    }
}
