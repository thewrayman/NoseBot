﻿using Discord.Commands;
using Discord.WebSocket;
using NoseBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using NoseBot.Util;

namespace NoseBot
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _cmds;
        private TwitchService _twitch;
        private IServiceProvider _provider;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService commands)
        {
            _cmds = commands;
            _client = client;
            _provider = provider;
        }
        public CommandHandler() { }
        public async Task InitializeAsync(IServiceProvider provider, DiscordSocketClient c)
        {
            _client = c;                                                 // Save an instance of the discord client.
            _cmds = new CommandService();
            _provider = provider;
            _client.MessageReceived += HandleCommandAsync;
            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        //public async Task InstallAsync(DiscordSocketClient c)
        //{
        //    _client = c;                                                 // Save an instance of the discord client.
        //    _cmds = new CommandService();                                // Create a new instance of the commandservice.                              

        //    await _cmds.AddModulesAsync(Assembly.GetEntryAssembly());    // Load all modules from the assembly.

        //    _client.MessageReceived += HandleCommandAsync;               // Register the messagereceived event to handle commands.
        //}

        private async Task HandleCommandAsync(SocketMessage s)
        {


            var msg = s as SocketUserMessage;
            if (msg == null)
                return;

            var context = new SocketCommandContext(_client, msg);

            string id = context.Guild.Id.ToString();
            Settings guildsettings = JSONUtil.GetSettingsObj(id);
            Dictionary<string, string> customargs = guildsettings.customcommands;

            int argPos = 0;
            if (msg.HasStringPrefix(guildsettings.prefix, ref argPos) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                // Try and execute a command with the given context.

                Console.WriteLine("trying to execute with command" + context.Message.Content);
                var result = await _cmds.ExecuteAsync(context, argPos);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ToString());


                Console.WriteLine("Handling incoming command.." + msg.Content);
            }
            if (customargs != null)
            {
                if (!context.Message.Author.IsBot && !context.Message.Content.Contains(guildsettings.prefix))
                {
                    //string emote = "<:dampC:277086968905728000>";

                    //await SendAsync("PUT", () => $"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", ids, options: options).ConfigureAwait(false);
                    foreach (KeyValuePair<string, string> entry in customargs)
                    {
                        if (context.Message.Content.ToLower().Contains(entry.Key.ToLower()))
                        {
                            Console.WriteLine("contains command");
                            await context.Channel.SendMessageAsync(entry.Value);
                        }
                    }
                }


            }
        }
    }


}