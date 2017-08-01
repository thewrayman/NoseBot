using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
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
    [Name("TwitchMonitor")]
    public class TwitchMonitor : ModuleBase<SocketCommandContext>
    {
        public string BaseString = "https://api.twitch.tv/kraken/";

        [Command("list"),Alias("streams")]
        [Remarks("List the streams the bot is watching")]
        public async Task List()
        {
            Console.WriteLine("executing list command");
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);

            List<string> names = JSONUtil.GetJsonToList<string>(Path.Combine(guildpath, FileDirUtil.JSONNAMES));
            Console.WriteLine("2");
            string messagestring = "Here's the current list of streamers i'm looking out for!";
            Console.WriteLine("3");
            foreach (string name in names)
            {
                messagestring += ("\n<https://www.twitch.tv/" + name + ">");
            }
            await ReplyAsync(messagestring);
        }



        [Command("start"),Alias("monitor")]
        [Remarks("Starts to monitor the listed streams for their live status")]
        public async Task Start()
        {
            Console.WriteLine("Executing start command");
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            string startfile = Path.Combine(guildpath, FileDirUtil.JSONSTART);
            if (File.Exists(startfile))
            {
                await ReplyAsync("Bot already running, if it appears to be broken try to restart");
                return;
            }
            FileStream fs = File.Create(startfile);
            fs.Flush();
            fs.Close();
            await MessageUtil.GetChannelIDs(guildid);
            await Context.Client.SetGameAsync("with yer nan");
            await ReplyAsync("Starting up the bot!");
            Task monitask = Task.Run(async () =>
            {
                await StartMon(startfile);
            });
            await Context.Message.DeleteAsync();

        }

        [Command("stop")]
        [Remarks("Stops the stream monitoring service")]
        public async Task Stop()
        {
            Console.WriteLine("Executing stop command");
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            string startfile = Path.Combine(guildpath, FileDirUtil.JSONSTART);
            if (File.Exists(startfile))
            {
                try
                {
                    File.Delete(startfile);
                    await ReplyAsync("Monitor has been stopped! Please wait up to 1 minute before attempting a restart..");
                }
                catch (Exception e)
                {
                    Console.WriteLine("error trying to delete startfile " + e);
                    await ReplyAsync("Could not stop the monitoring service right now, please retry");
                    return;
                }
            }
            else
            {
                await ReplyAsync("Currently not monitoring the streams, use **start** to begin monitoring!");
            }
            await Context.Client.SetGameAsync("AFK");
            await Context.Message.DeleteAsync();
        }



        [Command("Add"), Alias("add")]
        [Remarks("Add a streamer to be monitored")]
        public async Task AddStream(string streamer)
        {
            Console.WriteLine("Executing add command for "+streamer);
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            string namepath = Path.Combine(guildpath, FileDirUtil.JSONNAMES);
            List<string> names = JSONUtil.GetJsonToList<string>(namepath);
            //"Link" can be twitch link or just the channel name
            string name = MessageUtil.GetChannelNameFromMessage(streamer);
            if(name == null)
            {
                await ReplyAsync("Invalid link, please check again");
                return;
            }
            if (!names.Contains(name))
            {
                names.Add(name);
                JSONUtil.WriteJsonToFile(names, namepath);
                await ReplyAsync("Successfully added streamer: " + name);
                //refresh IDS after adding
                await MessageUtil.GetChannelIDs(guildid, true);
            }
            else
            {
                await ReplyAsync("I already have that streamer stored, try !!list to check the current list");
            }
            await Context.Message.DeleteAsync();
            Console.WriteLine("leaving addstreamer");
        }

        [Command("remove"), Alias("Delete,delete,Remove")]
        [Remarks("Add a streamer to be monitored")]
        public async Task RemoveStream(string streamer)
        {
            string guildid = Context.Guild.Id.ToString();
            string guildpath = FileDirUtil.GetGuildDir(guildid);
            List<string> names = JSONUtil.GetJsonToList<string>(Path.Combine(guildpath, FileDirUtil.JSONNAMES));


            string name = MessageUtil.GetChannelNameFromMessage(streamer);

            if (names.Contains(name))
            {
                await MessageUtil.UpdateList(name, guildid, true);
                await ReplyAsync("Successfully deleted streamer: " + name);
            }
            else
            {
                await ReplyAsync("Please try again, streamer could not be found!");
            }
            await Context.Message.DeleteAsync();
            Console.WriteLine("leaving deletestreamer");
        }

        [Command("notify")]
        public async Task NotifyBase()
        {
            await ReplyAsync($"{Context.User.Mention} please try using **notify on/off** to toggle notifications for yourself");
        }

        [Group("notify"), Name("Notify")]
        public class NotifyMe : ModuleBase
        {
            [Command("on")]
            [Remarks("Add me to the list of people to be notified when a stream goes live")]
            public async Task AddNotification()
            {
                var guildUser = Context.User as SocketGuildUser;
                
                var userhasrole = guildUser.Roles.FirstOrDefault(x => x.Name == "notify");

                //if they already have the role assigned
                if(userhasrole != null)
                {
                    await ReplyAsync($"{guildUser.Mention} you already have notifications active!");
                    return;
                }

                var guildrole = guildUser.Guild.Roles.FirstOrDefault(x => x.Name == "notify");
                Discord.IRole role = null;
                SocketUserMessage outmessage = null;

                //if role doesn't exist
                if (guildrole == null)
                {
                    try
                    {
                        //try to create role and assign it
                        role = await Context.Guild.CreateRoleAsync("notify", color: Discord.Color.DarkOrange);
                        await ReplyAsync("Created a **notify** role as one did not already exist");
                    }
                    catch (Exception e)
                    {
                        await ReplyAsync("Could not create a role to notify, please check permissions or manually add");
                        return;
                    }
                }

                Console.WriteLine("adding role to user");
                await guildUser.AddRoleAsync(role);
                Console.WriteLine("Added notify role to user");

                outmessage = await ReplyAsync($"{guildUser.Mention} you will now be notified for upcoming streams!") as SocketUserMessage;

                await Context.Message.DeleteAsync();
            }

            [Command("off")]
            [Remarks("Remove me from the notify list")]
            public async Task DeleteNotification()
            {
                //check the user has the role
                var guildUser = Context.User as SocketGuildUser;
                var guildrole = guildUser.Roles.FirstOrDefault(x => x.Name == "notify");
                SocketUserMessage outmessage = null;
                Discord.IRole role = guildrole;
                if (guildrole == null)
                {
                    outmessage = await ReplyAsync("You don't have the notify role, wyd fam") as SocketUserMessage;
                }
                else
                {
                    await guildUser.RemoveRoleAsync(role);
                    outmessage = await ReplyAsync($"{guildUser.Mention} you will no longer be notified for upcoming streams!") as SocketUserMessage;
                }
                await Context.Message.DeleteAsync();
            }
        }


        public async Task StartMon(string startfile)
        {
            Console.WriteLine("start mon");
            while (true)
            {
                if (File.Exists(startfile))
                {
                    await CheckLive(Context.Message, Context.Guild.Id.ToString());
                    System.Threading.Thread.Sleep(60000);
                }
                else
                {
                    break;
                }
                
            }
            await ReplyAsync("**Monitor stopped**");
        }


        public async Task CheckLive(SocketUserMessage msg, string guildid)
        {
            string guildids = FileDirUtil.GetGuildFile(guildid, FileDirUtil.JSONIDS);
            string guildlive = FileDirUtil.GetGuildFile(guildid, FileDirUtil.JSONLIVE);
            string guildstream = FileDirUtil.GetGuildFile(guildid, FileDirUtil.JSONSTREAMS);

            //dic of users to check; dic of last known live, list of currently live
            Dictionary<string, string> users = JSONUtil.GetJsonToDic<string, string>(guildids);
            Dictionary<string, string> live = JSONUtil.GetJsonToDic<string, string>(guildlive);
            List<string> liveusers = new List<string>();

            string s = string.Join(",", users.Select(x => x.Value));

            string response = SendRequest(BaseString + "streams/?channel=", s, "Requesting live channels for " + s);

            JSONUtil.WriteJsonToFile(null, guildstream, response);

            Streams streams = JsonConvert.DeserializeObject<Streams>(response);
 
            //if any live streams, check if already noted as live, if not, set to "live" and notify
            if (streams._total > 0)
            {
                foreach (Stream x in streams.streams)
                {
                    liveusers.Add(x.channel.name);
                    if (live[x.channel.name] != "live")
                    {
                        live[x.channel.name] = "live";
                        SocketGuildUser guilduser = msg.Author as SocketGuildUser;
                        SocketRole guildrole =  guilduser.Guild.Roles.FirstOrDefault(y => y.Name == "notify");
                        string mention = "";
                        if(guildrole != null)
                        {
                            mention = guildrole.Mention;
                        }
                        await msg.Channel.SendMessageAsync($"{mention} {x.channel.name} has just gone live playing {x.channel.game}! \n" + "https://www.twitch.tv/" + x.channel.name);
                    }
                }
            }

            //check the current live against previous live
            //if set as live when not currently live, set to offline
            Dictionary<string,string> newDictionary = live.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
            foreach (KeyValuePair<string, string> entry in live)
            {
                if (entry.Value == "live")
                {
                    if (!liveusers.Any(x => x == entry.Key))
                    {
                        newDictionary[entry.Key] = "offline";
                    }
                }
            }
            live = newDictionary.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
            JSONUtil.WriteJsonToFile(live, guildlive);
            Console.WriteLine("5");

        }


        public static string SendRequest(string url, string suffix, string log)
        {
            var client = new RestClient(url + suffix);
            var request = new RestRequest(Method.GET);

            request.AddHeader("Client-ID", "uo6dggojyb8d6soh92zknwmi5ej1q2");
            request.AddHeader("Accept", "application/vnd.twitchtv.v5+json");
            request.AddParameter("application/vnd.twitchtv.v5+json", "bodykey=bodyval", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            Console.WriteLine(log + ": ");

            return response.Content;
        }


    }
}
