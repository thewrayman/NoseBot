using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using NoseBot.Modules;
using Discord.Commands;

namespace NoseBot.Util
{
    public static class MessageUtil
    {
        public static string BaseString = "https://api.twitch.tv/kraken/";
        public static async Task NotifyChannelLive(string user, string game, SocketMessage msg)
        {
            Console.WriteLine("sending live notification..");
            //ow channel id 239069517605502977
            await msg.Channel.SendMessageAsync(user + " has now gone live playing " + game + "! \n" + "https://www.twitch.tv/" + user);
        }


        public static async Task NotifyChannelMessage(string message, SocketMessage msg)
        {
            Console.WriteLine("sending message..");
            await msg.Channel.SendMessageAsync(message);
        }

        public static string GetChannelNameFromMessage(string message, string split = "twitch.tv/")
        {
            Console.WriteLine("Getting channel name from " + message + "\nUsing split: " + split);
            //in the case of no "twitch.tv", add the channel name provided
            string name = message;

            //if twitch.tv is present, grab name from the link
            if (message.Contains(split))
            {
                name = message.ToLower().Substring(message.IndexOf(split) + split.Length);
            }
            if (name.Contains("/")) name = null;
            Console.WriteLine("Found channel: " + name);
            return name;
        }

        public static async Task GetChannelIDs(string id, bool refresh = false)
        {
            string guildpath = FileDirUtil.GetGuildDir(id);
            string namepath = Path.Combine(guildpath, FileDirUtil.JSONNAMES);
            string livepath = Path.Combine(guildpath, FileDirUtil.JSONLIVE);
            string idpath = Path.Combine(guildpath, FileDirUtil.JSONIDS);


            Console.WriteLine("getting channel ids");
            Dictionary<string, string> nameid = new Dictionary<string, string>();
            Dictionary<string, string> live = JSONUtil.GetJsonToDic<string, string>(livepath);
            List<string> names = JSONUtil.GetJsonToList<string>(namepath);
            string searchstring = "tinietinie";
            if (names.Count > 1)
            {
                searchstring = string.Join("&login=", names.Select(x => x));
            }


            string response = TwitchMonitor.SendRequest (BaseString + "users?login=" + searchstring, null, "Channels");
            Console.WriteLine("loginresponse"+ response);
            JSONUtil.WriteJsonToFile(null, Path.Combine(guildpath,FileDirUtil.JSONUSERS), response);
            Console.WriteLine("written users to file");
            Users users = JsonConvert.DeserializeObject<Users>(response);
            Console.WriteLine("made user obj");
            foreach (User u in users.users)
            {
                Console.WriteLine("user: " + u.name);
                nameid[u.name] = u._id;
                if (refresh)
                {
                    Console.WriteLine("is refresh");
                    if (!live.ContainsKey(u.name))
                    {
                        Console.WriteLine("new key");
                        live[u.name] = "offline";
                    }
                }
                else
                {
                    Console.WriteLine("not refresh, set to offline");
                    live[u.name] = "offline";
                }

            }

            JSONUtil.WriteJsonToFile(nameid, idpath);
            JSONUtil.WriteJsonToFile(live, livepath);
        }

        public static async Task UpdateList(string name, string id, bool remove = true)
        {
            string guildpath = FileDirUtil.GetGuildDir(id);
            string namepath = Path.Combine(guildpath, FileDirUtil.JSONNAMES);
            string livepath = Path.Combine(guildpath, FileDirUtil.JSONLIVE);
            string idpath = Path.Combine(guildpath, FileDirUtil.JSONIDS);

            List<string> names = JSONUtil.GetJsonToList<string>(namepath);
            Dictionary<string, string> live = JSONUtil.GetJsonToDic<string, string>(livepath);
            Dictionary<string, string> ids = JSONUtil.GetJsonToDic<string, string>(idpath);

            if (remove)
            {
                names.Remove(name);
                live.Remove(name);
                ids.Remove(name);
                JSONUtil.WriteJsonToFile(names, namepath);

                JSONUtil.WriteJsonToFile(live, livepath);

                JSONUtil.WriteJsonToFile(ids, idpath);

                Console.WriteLine("updated name removal for " + name);
            }
        }

        public static bool CheckRoleExists(List<SocketRole> roles, string name)
        {
            Console.WriteLine($"Checking if role {name} exists");
            return roles.Any(x => x.Name == name);
        }
        
        public async static void AttemptGuildsMessage(SocketCommandContext context, string message)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            IReadOnlyCollection<SocketGuild> guilds = context.Client.Guilds;

            foreach(SocketGuild tguild in guilds)
            {
                try
                {
                    await tguild.DefaultChannel.SendMessageAsync(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to send message to " + tguild.DefaultChannel.Name);
                }              
            }
        }

    }
}
