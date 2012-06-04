using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBPluginInterface;
using System.Xml;
using System.ServiceModel.Syndication;

namespace SBGameSales
{
    public class GameSaleData
    {
        public string Name = "";
        public string URL = "";
        public string Store = "";
        public decimal PriceNow = 0.0m;
        public decimal PriceOriginal = 0.0m;
        public decimal PercentOff = 0.0m;

        public GameSaleData( SyndicationItem i )
        {
            string temp = i.Title.Text;

            Name = temp.Substring(temp.IndexOf("% off ", StringComparison.CurrentCultureIgnoreCase) + 6);
            Name = Name.Substring(0, Name.LastIndexOf(" - Now only"));

            URL = i.Links[0].Uri.ToString();

            if ( URL.Contains("anrdoezrs") )
            {
                URL = URL.Substring(URL.IndexOf("?url=") + 5);
            }


            // Sale Price: $19.99 USD, List Price: $24.99 USD, Store: Green Man Gaming
            temp = i.Summary.Text;

            //Sale Price: $19.99 USD
            //List Price: $24.99 USD
            //Store: Green Man Gaming
            string[] parts = temp.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            //[Sale Price] [$19.99 USD]
            //[List Price] [$24.99 USD]
            //[Store]      [Green Man Gaming]

            temp = parts[0].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1];
            PriceNow = Convert.ToDecimal(temp.Substring(1, temp.IndexOf(" USD") - 1));

            temp = parts[1].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1];
            PriceOriginal = Convert.ToDecimal(temp.Substring(1, temp.IndexOf(" USD") - 1));

            Store = parts[2].Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1];

            PercentOff = 100 - ( ( PriceNow / PriceOriginal ) * 100 );
        }
    }

    public class Main : SBPlugin
    {
        private SBPluginHost Host;

        #region SBPlugin Members

        public string PluginName
        {
            get
            {
                return "Game Sales";
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
                return "Michael Schwarz";
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
                return "Gets sales on games.";
            }
        }

        public SBPluginHost PluginHost
        {
            set
            {
                Host = value;

                Host.eventPluginChannelCommandReceived += new ChannelCommand(Host_eventPluginChannelCommandReceived);
            }
        }

        public void Dispose()
        {
            return;
        }

        #endregion

        void Host_eventPluginChannelCommandReceived( string name, string channel, string command, string[] parameters )
        {
            switch ( command )
            {
                case "sale":
                    Host.PluginResponse(channel, "Poor Gamer's Sale of the Day: " + BestSale("*"));
                    break;
                case "game":
                    if ( parameters.Length > 0 )
                    {
                        string search = "";
                        foreach ( string s in parameters )
                        {
                            search = search + s + " ";
                        }
                        Host.PluginResponse(channel, SearchSale(search.Trim(), false));
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Usage: !game [name] - Example: !game diablo");
                    }
                    break;
                case "gameprice":
                    if ( parameters.Length > 0 )
                    {
                        string search = "";
                        foreach ( string s in parameters )
                        {
                            search = search + s + " ";
                        }
                        Host.PluginResponse(channel, SearchSale(search.Trim(), true));
                    }
                    else
                    {
                        Host.PluginResponse(channel, "Usage: !gameprice [name] - Example: !gameprice diablo");
                    }
                    break;
                case "poor":
                    Host.PluginResponse(channel, "Lowest Price of the Day: " + BestSale("*", true));
                    break;
                case "steam":
                    Host.PluginResponse(channel, "Steam Sale of the Day: " + BestSale("Steam"));
                    break;
                case "gog":
                    Host.PluginResponse(channel, "GOG Sale of the Day: " + BestSale("GOG"));
                    break;
                case "gmg":
                    Host.PluginResponse(channel, "Green Man Gaming Sale of the Day: " + BestSale("Green Man Gaming"));
                    break;
                case "gg":
                    Host.PluginResponse(channel, "GamersGate Sale of the Day: " + BestSale("GamersGate"));
                    break;
            }
        }

        private string SearchSale( string name, bool byprice )
        {
            List<GameSaleData> Sales = LoadSales();


            Sales = Sales.FindAll(delegate( GameSaleData d )
            {
                if ( d.Name.ToLower().Contains(name.ToLower()) )
                {
                    return true;
                }

                return false;
            });

            if ( Sales == null || Sales.Count == 0 )
            {
                return "Sorry, no sales for '" + name + "' found for today :(";
            }

            GameSaleData best = null;

            foreach ( GameSaleData d in Sales )
            {
                if ( best == null )
                {
                    best = d;
                    continue;
                }

                if ( byprice )
                {
                    if ( d.PriceNow < best.PriceNow )
                    {
                        best = d;
                    }
                }
                else
                {
                    if ( d.PercentOff > best.PercentOff )
                    {
                        best = d;
                    }
                }
            }

            object[] p = new object[] { best.Name, best.PriceNow, best.Store, best.URL };

            return String.Format("I found: {0} for {1} USD at {2} ({3})", p);
        }

        private string BestSale( string store )
        {
            return BestSale(store, false);
        }

        private string BestSale( string store, bool byprice )
        {
            List<GameSaleData> Sales = LoadSales();

            if ( store != "*" )
            {
                Sales = Sales.FindAll(delegate( GameSaleData d )
                {
                    if ( d.Store == store )
                    {
                        return true;
                    }

                    return false;
                });
            }

            if ( Sales == null || Sales.Count == 0 )
            {
                return "Sorry, no sales found for today :(";
            }

            GameSaleData best = null;

            foreach ( GameSaleData d in Sales )
            {
                if ( best == null )
                {
                    best = d;
                    continue;
                }

                if ( byprice )
                {
                    if ( d.PriceNow < best.PriceNow )
                    {
                        best = d;
                    }
                }
                else
                {
                    if ( d.PercentOff > best.PercentOff )
                    {
                        best = d;
                    }
                }
            }

            object[] p = new object[] { best.Name, best.PriceNow, best.Store, best.PercentOff.ToString("##.##"), best.PriceOriginal, best.URL };

            return String.Format("{0} for {1} USD at {2} ({3}% off {4} USD) - {5}", p);
        }

        private List<GameSaleData> LoadSales()
        {
            XmlReader r = XmlReader.Create("http://www.steamgamesales.com/rss/?region=us&stores=steam+direct2drive+gamersgate+greenmangaming+gog");
            SyndicationFeed f = SyndicationFeed.Load(r);
            List<GameSaleData> GameSales = new List<GameSaleData>();

            try
            {
                foreach ( SyndicationItem i in f.Items )
                {
                    if ( !i.Title.Text.Contains(" DLC ") )
                    {
                        GameSales.Add(new GameSaleData(i));
                    }
                }
            }
            catch
            {
                return null;
            }

            return GameSales;
        }
    }
}
