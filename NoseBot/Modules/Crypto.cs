using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        [Command("pc")]
        public async Task PriceCheck(string coin, string cur="USD", string expanded=null)
        {
            Console.WriteLine("Command: crypto pricecheck");
            //get the api-correct name for the coin


            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            Dictionary<string,string> coindict = JSONUtil.GetJsonToDic<string,string>(Path.Combine(guildpath, FileDirUtil.COINS));

            string coinname = GetFullCoinName(coindict, coin);
            string message = "Could not find the coin mentioned, try updating my record!";
            if (coinname != null)
            {
                if (cur == "full") { cur = "usd"; expanded = "full"; }
                string response = Request(coinname, cur);

                message = FormatCoinPriceResponse(response, cur.ToLower(), expanded);
            }
            
            await ReplyAsync(message);



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
                if (kvp.Key == name.ToLower()) { return kvp.Value; }
                if (kvp.Value == name.ToLower()) { return kvp.Value;  }
            }
            return null;

        }

        private static string FormatCoinPriceResponse(string response, string cur, string expanded)
        {
            Console.WriteLine("formatting..");
            var json = JArray.Parse(response)[0];
            Console.WriteLine("got obj..");
            if ((string)json["error"] != null) { return "There was an error with the response, try again later"; }
            Console.WriteLine("getting message..");
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

        private static string Request(string coin, string cur)
        {
            Console.WriteLine("Crypto request for {0} in {1}", coin, cur);
            string url = String.Format(ticketstr, coin, cur);
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            IRestResponse response = client.Execute(request);
            Console.WriteLine("response: " + response.Content);
            return response.Content;
        }
    }
}
