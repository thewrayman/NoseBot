using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using RestSharp;
using System.Linq;
using NoseBot.Util;
using Microsoft.Extensions.DependencyInjection;
using NoseBot.Services;

namespace NoseBot
{
    public class Program
    {
        //https://discordapp.com/oauth2/authorize?client_id=339505551115419648&scope=bot&permissions=515136
        //https://discordapp.com/oauth2/authorize?client_id=339505551115419648&scope=bot&permissions=268958784
        private DiscordSocketClient _client;
        private CommandHandler _commands;
        private IServiceProvider _services;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

      
        public async Task MainAsync()
        {

            _client = new DiscordSocketClient();
            _services = new ServiceCollection().BuildServiceProvider();
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;
            _client.JoinedGuild += JoinedGuild;
            await _client.SetGameAsync("AFK");
            string token = "MzM5NTA1NTUxMTE1NDE5NjQ4.DFlETQ.AWXndihnyvC0FSnDktqyRzUB0is"; // Remember to keep this private!
            
            var services = ConfigureServices();
            //services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandler>().InitializeAsync(services, _client);
            await services.GetRequiredService<TwitchService>().InitializeAsync(services);


            //_commands = new CommandHandler();                // Initialize the command handler service
            //await _commands.InstallAsync(_client);

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await CleanGuilds();

            // Block this task until the program is closed.
        await Task.Delay(-1);

        }
        private async Task CleanGuilds()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Guilds guilds = JsonConvert.DeserializeObject<Guilds>(File.ReadAllText(FileDirUtil.JSONGUILDS), settings);

            if (guilds.guilds != null)
            {
                foreach(Guild guild in guilds.guilds)
                {
                    string startfile = FileDirUtil.GetGuildFile(guild.id, FileDirUtil.JSONSTART);
                    string stopfile = FileDirUtil.GetGuildFile(guild.id, FileDirUtil.JSONSTOP);
                    if (File.Exists(startfile))
                    {
                        try
                        {
                            File.Delete(FileDirUtil.GetGuildFile(guild.id, FileDirUtil.JSONSTART));
                            Console.WriteLine("Cleaned up start files for " + guild.name);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("couldn't clean up start files " + e);
                        }
                    }
                    else
                    {
                        Console.WriteLine("no start file present, carry on");
                    }

                    if (File.Exists(stopfile))
                    {
                        try
                        {
                            File.Delete(FileDirUtil.GetGuildFile(guild.id, FileDirUtil.JSONSTOP));
                            Console.WriteLine("Cleaned up stop files for " + guild.name);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("couldn't clean up stop files " + e);
                        }
                    }
                    else
                    {
                        Console.WriteLine("no stop file present, carry on");
                    }

                }
            }
        }
        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandHandler>()
                //logs
                //.AddLogging()
                //.AddSingleton<LogService>()
                // Extra
                .AddSingleton<TwitchService>()
                .BuildServiceProvider();
        }


        private async Task JoinedGuild(SocketGuild guild)
        {
            Console.WriteLine("Entered guild ");
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            Guilds guilds = JsonConvert.DeserializeObject<Guilds>(File.ReadAllText(FileDirUtil.JSONGUILDS), settings);//checks json containing all guilds + their settings
            Console.WriteLine("got guilds json");
            string id = guild.Id.ToString();
            string name = guild.Name;
            
            List<string> ids = guilds.GetIds();
            Console.WriteLine("Entered guild " + name + " "+id);
            //if this is a brand new guild
            if ((guilds.guilds == null) || !ids.Contains(id))
            {
                string guildpath = FileDirUtil.GetGuildDir(id);
                string settingfile = Path.Combine(guildpath, FileDirUtil.JSONSETTINGS);

                Console.WriteLine("new guild!");

                Settings gdst = new Settings("!!",id,0);
                Console.WriteLine("settings!");

                Guild newguilld = new Guild(id, name, DateTime.Now, DateTime.Now);    //create the new guild with info
                Console.WriteLine("newguild!");

                guilds.AddGuild(newguilld);
                Console.WriteLine("addedguild!");

                await FileDirUtil.EstablishGuildFiles(newguilld);
                Console.WriteLine("writing guilds back to file");
                JSONUtil.WriteJsonToFile(guilds, FileDirUtil.JSONGUILDS);
                JSONUtil.WriteJsonToFile(gdst, settingfile);
                Console.WriteLine("completed setting up for new guild!");
            }
            else
            {
                Console.WriteLine("existing guild");
                await FileDirUtil.VerifyGuildFiles(id);
            }
            //write back the json
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
        }



    }

}
