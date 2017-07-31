using RestSharp;
using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoseBot.Util;

namespace NoseBot.Services
{
    public class TwitchService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        //private SocketCommandContext _context;
        private IServiceProvider _services;
        private ModuleInfo module;
        //private SocketUserMessage _context;

        public string BaseString = "https://api.twitch.tv/kraken/";

        public TwitchService(DiscordSocketClient discord)
        {
            _discord = discord;
        }
        public async Task InitializeAsync(IServiceProvider services)
        {
            _services = services;
            
        }

    }
}
