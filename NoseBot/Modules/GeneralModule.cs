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
            string helpstring = "List of commands for this bot:\n**!!start** - tells the bot start monitoring for live streams (notifies in same channel)\n**!!add <twitch name or link>** - to add a streamer to watch out for\n**!!remove <twitch name or link>** - to remove a streamer from the watch list\n**!!list** - lists current watch list";
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
    }
}
