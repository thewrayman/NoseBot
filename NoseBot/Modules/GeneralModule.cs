using Discord.Commands;
using NoseBot.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace NoseBot.Modules
{
    [Name("GeneralModule")]
    public class  GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Command("commands"), Alias("help,commands")]
        [Remarks("Get a list of commands")]
        public async Task GetHelp()
        {
            Console.WriteLine("executing help command");
            string id = Context.Guild.Id.ToString();
            string settingsfile = FileDirUtil.GetGuildFile(id, FileDirUtil.JSONSETTINGS);
            Settings guildsettings = JSONUtil.GetSettingsObj(id);   //get the settings for this guild

            string helpstring = "";
            helpstring += "List of commands for this bot:\n**{0}start** - tells the bot start monitoring for live streams (notifies in same channel)\n";
            helpstring += "**{0}stop** - Tells the bot to stop monitoring (NOTE this may take up to 1 minute to fufil)\n";
            helpstring += "**{0}add <twitch name or link>** - to add a streamer to watch out for\n";
            helpstring += "**{0}remove <twitch name or link>** - to remove a streamer from the watch list\n";
            helpstring += "**{0}notify <on/off>** - Adds the “notify” role to the user invoking the command. (Creates the role if it doesn’t already exist)\n";
            helpstring += "**{0}list** - lists current watch list";
            helpstring += "\n";
            helpstring += "**{0}prefix <new prefix>** - Changes the default or existing prefix to the new one entered\n";
            helpstring += "**{0}addresponse <trigger> <response>** - The bot will watch out for the trigger word and reply with a response when seen\n";
            helpstring += "**{0}deleteresponse <trigger>** - Deletes the response for that trigger\n";
            helpstring += "**{0}responses** - Lists the current stored responses for this server";
            helpstring += "**{0}stats <user>** - Returns the most used words from that user\n";
            helpstring += "**{0}stats all** - Returns the top word for each user in the chat log\n";
            helpstring += "**{0}stats server** - Returns the top words in the server\n";
            helpstring += "\n";
            helpstring += "**{0}pc <coin> <currency>(opt) full(opt)** - Returns basic or full price info for a coin\n";
            helpstring = String.Format(helpstring, guildsettings.prefix);
            await ReplyAsync(helpstring);
        }

        [Command("addresponse")]
        [Remarks("Automated response to a certain word")]
        public async Task AddReponse(string word, string reaction)
        {
            string id = Context.Guild.Id.ToString();
            string settingsfile = FileDirUtil.GetGuildFile(id, FileDirUtil.JSONSETTINGS);
            Settings guildsettings = JSONUtil.GetSettingsObj(id);

            bool addnew = guildsettings.AddOrModifyCommand(word.ToLower(), reaction);
            if (addnew)
            {
                await ReplyAsync($"Successfully added a new response for {word}!");
            }
            else
            {
                await ReplyAsync($"Successfully modified the response {word}");

            }

            JSONUtil.WriteJsonToFile(guildsettings, settingsfile);
        }

        [Command("deleteresponse")]
        [Remarks("Deletes a specified command from the bot")]
        public async Task DeleteResponse(string word)
        {
            string id = Context.Guild.Id.ToString();
            string settingsfile = FileDirUtil.GetGuildFile(id, FileDirUtil.JSONSETTINGS);
            Settings guildsettings = JSONUtil.GetSettingsObj(id);

            bool remove = guildsettings.DelCommand(word.ToLower());
            if (remove)
            {
                await ReplyAsync($"Successfully deleted the response for {word}!");
            }
            else
            {
                await ReplyAsync($"Oops, looks like {word} didn't exist anyway!");
            }


            JSONUtil.WriteJsonToFile(guildsettings, settingsfile);
        }

        [Command("responses")]
        [Remarks("Lists the current responses for this server")]
        public async Task ListResponses()
        {
            string id = Context.Guild.Id.ToString();
            Settings guildsettings = JSONUtil.GetSettingsObj(id);   //get the settings for this guild
            Dictionary<string, string> cmddict = guildsettings.customcommands;
            if(cmddict == null)
            {
                cmddict = new Dictionary<string, string>();
            }

            StringBuilder outputstr = new StringBuilder();

            if (cmddict.Count > 0)
            {
                outputstr.Append("Here are a list of command responses for this server\n");
                foreach (KeyValuePair<string, string> entry in cmddict)
                {
                    outputstr.Append($"**{entry.Key}** - {entry.Value}\n");
                }
            }
            else
            {
                outputstr.Append("No commands stored yet, try adding one with the addresponse command");
            }

            await ReplyAsync(outputstr.ToString());

        }

        [Command("prefix")]
        [Remarks("Changes the current prefix to the value entered")]
        public async Task ChangePrefix(string newpref)
        {
            string id = Context.Guild.Id.ToString();
            string settingsfile = FileDirUtil.GetGuildFile(id, FileDirUtil.JSONSETTINGS);
            Settings guildsettings = JSONUtil.GetSettingsObj(id);   //get the settings for this guild

            guildsettings.prefix = newpref;

            JSONUtil.WriteJsonToFile(guildsettings, settingsfile);

            await ReplyAsync($"The prefix for the bot has been changed to **{newpref}**");
        }

        [Command("restartbot")]
        [Remarks("Restarts the server instance of the bot")]
        public async Task RestartBot()
        {
            await ReplyAsync("**Attempting to restart the bot instance..**");
            MessageUtil.AttemptGuildsMessage(Context, "**NoseBot is current restarting, apologies for the inconvenience. Monitors will be stopped**");
            ProgramUtil.RestartBot();
        }
    }
}
