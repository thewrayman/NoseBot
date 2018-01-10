using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NoseBot.Objects;
using NoseBot.Services;
using NoseBot.Util;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot.Modules
{
    [Name("Crypto")]
    public class Crypto : ModuleBase<SocketCommandContext>
    {

        static string ticketstr = "https://api.coinmarketcap.com/v1/ticker/{0}/?convert={1}";

        [Command("pc", RunMode = RunMode.Async)]
        public async Task PriceCheck(string coin, string cur="USD", string expanded=null)
        {
            Console.WriteLine("Command: crypto pricecheck");
            //get the api-correct name for the coin


            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            Dictionary<string,string> coindict = JSONUtil.GetJsonToDic<string,string>(Path.Combine(guildpath, Constants.TemplateFiles.COINS));

            string coinname = GetFullCoinName(coindict, coin);
            string message = "Could not find the coin mentioned, double check the code";
            if (coinname != null)
            {
                if (cur == "full") { cur = "usd"; expanded = "full"; }
                string response = RequestCoin(coinname, cur);

                message = FormatCoinPriceResponse(response, cur.ToLower(), expanded);
            }
            
            await ReplyAsync(message);



        }

        [Command("up")]
        public async Task UpdateCryptoProfile(params string[] options)
        {
            Console.WriteLine("Updating crypto profile with "+options.Length +" coins");
            if(options.Length < 1)
            {
                await ReplyAsync("Please supply a list of coins you want to add to your portfoloio " + Context.Message.Author.Mention);
                return;
            }

            Dictionary<string,List<string>> userprofiles = new Dictionary<string, List<string>>();
            //{"userid":[coin1,coin2,coin3], "userid2"...}
            string uid = Context.Message.Author.Id.ToString();
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            string portfile = Path.Combine(guildpath, Constants.TemplateFiles.PORTFOLOIOS);
            userprofiles = JSONUtil.GetJsonToDic<string, List<string>>(portfile);
            Console.WriteLine("Got existing profile file");
            string message;

            if (userprofiles.ContainsKey(uid))
            {
                userprofiles[uid] = options.ToList();
                message = "Updated your coin profile " + Context.Message.Author.Mention + "  Check it out by using `cp`";
            }
            else
            {
                userprofiles.Add(uid, options.ToList());
                message = "Added your coin profile " + Context.Message.Author.Mention+"  Check it out by using `cp`";
            }
            Console.WriteLine("writing profiles..");
            JSONUtil.WriteJsonToFile(userprofiles, portfile);
            Console.WriteLine("written");
            await ReplyAsync(message);
        }


        [Command("cp")]
        public async Task GetPortfolio(string cur="usd")
        {
            Dictionary<string, List<string>> userprofiles = new Dictionary<string, List<string>>();
            //{"userid":[coin1,coin2,coin3], "userid2"...}
            string uid = Context.Message.Author.Id.ToString();
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            string portfile = Path.Combine(guildpath, Constants.TemplateFiles.PORTFOLOIOS);
            userprofiles = JSONUtil.GetJsonToDic<string, List<string>>(portfile);

            if(!userprofiles.ContainsKey(uid) || userprofiles.Count < 1)
            {
                await ReplyAsync("No existing portfolio found for " + Context.Message.Author.Mention + " please make one!");
                return;
            }

            await ReplyAsync(Context.Message.Author.Mention + " here is your portfolio standing:");
            foreach(var coin in userprofiles[uid])
            {
                PriceCheck(coin, cur);
            }
        }

        [Command("updatecoins", RunMode = RunMode.Async)]
        public async Task UpdateCoins(int count=500)
        {
            /*
             * Takes in an optional number of coins to look for, will take the top x coins from coinmarketcap
             * reads in existing list, updates with anything new from request
             * 
             */ 
            Discord.IUserMessage startmsg = await ReplyAsync($"Fetching top {count} coins from CoinMarketCap..");

            string updatelink = $"https://api.coinmarketcap.com/v1/ticker/?start=0&limit={count}";
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            string response = Request(updatelink);


            var json = JsonConvert.DeserializeObject<List<Coin>>(response);
            json.RemoveAll(r => r.id == "bitmark");
            Dictionary<string, string> coindict = JSONUtil.GetJsonToDic<string, string>(Path.Combine(guildpath, Constants.TemplateFiles.COINS));

            foreach (var coin in json)
            {
                string symbol = coin.symbol;
                string id = coin.id;
                if (!coindict.ContainsKey(symbol)) { coindict.Add(symbol, id); }
            }
            
            JSONUtil.WriteJsonToFile(coindict, Path.Combine(guildpath, Constants.TemplateFiles.COINS));
            Console.WriteLine("updated coins json file");
            await startmsg.ModifyAsync(msg => msg.Content = "Updated coin reference");

        }

        [Command("coins")]
        public async Task CoinHelp()
        {
            
        }

        private static string GetFullCoinName(Dictionary<string, string> dict, string coin)
        {
            string name = coin;

            foreach(KeyValuePair<string,string> kvp in dict)
            {
                if (kvp.Key.ToLower() == name.ToLower()) { return kvp.Value; }
                if (kvp.Value.ToLower() == name.ToLower()) { return kvp.Value;  }
            }
            return coin;

        }

        private static string FormatCoinPriceResponse(string response, string cur, string expanded)
        {
            Console.WriteLine("formatting..");
            try
            {
                var result = JObject.Parse(response);
                if((string)result["error"]!= null)
                {
                    return (string)result["error"];
                }
            }
            catch
            {
                Console.WriteLine("caught");
            }

            var json = JArray.Parse(response)[0];

            if ((string)json["error"] != null) { return "There was an error with the response, try again later"; }

            string message = $"**{(string)json["name"]}({(string)json["symbol"]})/{cur.ToUpper()}**: **`{(string)json[$"price_{cur}"]}`**, Daily change: **`{(string)json["percent_change_24h"]}%`**\n";

            if (expanded != null)
            {
                message += $"**Rank**: **`{(string)json["rank"]}`**\n";
                message += $"**Price BTC**: **`{(string)json["price_btc"]}`**\n";
                message += $"**Market Cap USD**: **`${(string)json["market_cap_usd"]}`**\n";
                message += $"**7-Day change**: **`{(string)json["percent_change_7d"]}%`**\n";
                //add extra info to string

            }

            return message;

        }

        private static string RequestCoin(string coin, string cur)
        {
            Console.WriteLine("Crypto request for {0} in {1}", coin, cur);
            string url = String.Format(ticketstr, coin, cur);
            return Request(url);
        }

        private static string Request(string url)
        {
            Console.WriteLine("sending request for " + url);
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            IRestResponse response = client.Execute(request);
            Console.WriteLine($"[{response.StatusCode}]response: " + response.Content);
            return response.Content;
        }
    }
}
