using Discord.Commands;
using NoseBot.Objects;
using NoseBot.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NoseBot.Modules
{
    [Name("Analytics")]
    public class Analytics : ModuleBase<SocketCommandContext>
    {
        //get overall server stats - top 10 words, how many times each word, who said it the most times
        //get top word for each user
        //get top words for particular user

        //[Command("stats")]
        //public async Task GetUserStats(string user)
        //{
        //    Console.WriteLine("getuserstats");
        //    //get the monthly word stats for a specific user

        //}


        [Group("stats"), Name("stats")]
        public class GetStats : ModuleBase
        {
            [Command("server", RunMode = RunMode.Async)]
            public async Task GetServerStats()
            {
                Console.WriteLine("Command: Stats server");
                //get the monthly stats for the server as a whole
                Discord.IUserMessage holdon = await ReplyAsync("Hold on whist I calculate the stats..");
                var aggregate = AggregateRecords(Context as SocketCommandContext);
                string message = GetTopServerStats(aggregate);
                await ReplyAsync(message);
                holdon.DeleteAsync();

            }

            [Command("all", RunMode = RunMode.Async)]
            public async Task GetAllStats()
            {
                Console.WriteLine("Command: Stats All");
                //show the stats for every user
                Discord.IUserMessage holdon = await ReplyAsync("Hold on whist I calculate the stats..");
                var aggregate = AggregateRecords(Context as SocketCommandContext);
                string message = PrintTopDictValues(aggregate);
                await ReplyAsync(message);
                holdon.DeleteAsync();

            }

            [Command("user", RunMode = RunMode.Async)]
            public async Task GetUserStats(string user)
            {
                Console.WriteLine("Getting stats for user "+user);
                string message = "";

                if (!user.StartsWith("<@"))
                {
                    message = "Please make sure you are tagging a user";
                }
                else
                {
                    string uid = user.Split('@')[1].Split('>')[0];
                    uid = uid.Replace("!", string.Empty);
                    Console.WriteLine("userid found: " + uid);
                    var aggregate = AggregateRecords(Context as SocketCommandContext);
                    message = PrintUserStats(aggregate, uid);
                }
                await ReplyAsync(message);
            }
        }

        //read in the log file csv format into a chatlog object
        public static ChatRecord FromCSV(string record)
        {

            // {date, userid, [word1:2, word3:7...]}
            ChatRecord crecord = new ChatRecord();
            string[] fields = record.Split(',');
            DateTime timestamp;
            try
            {
                timestamp = Convert.ToDateTime(fields[0]);
            }
            catch
            {
                timestamp = DateTime.Now;
            }
            
            string userid = fields[1];


            crecord.User = userid;
            crecord.TimeStamp = timestamp;

            List<string> msgwords = fields.ToList().GetRange(2, fields.Length - 2);

            //count the number of instances of this word in the record
            foreach(string word in msgwords)
            {
                if (crecord.Words.ContainsKey(word.ToLower()))
                {
                    crecord.Words[word.ToLower()] += 1;
                }
                else
                {
                    crecord.Words.Add(word.ToLower(), 1);
                }
                
            }

            return crecord;
        }

        public static Dictionary<string, Dictionary<string,int>> AggregateRecords(SocketCommandContext context)
        {
            Console.WriteLine("aggregating records..");

            string filepath = Path.Combine(FileDirUtil.GetGuildDir(context.Guild.Id.ToString()), FileDirUtil.PROCESSLOG);
            Console.WriteLine("reading from the csv file..");
            List <ChatRecord> records = File.ReadAllLines(filepath)
                                           .Select(v => FromCSV(v))
                                           .ToList();

            Dictionary<string, Dictionary<string, int>> aggregate = new Dictionary<string, Dictionary<string, int>>();
            // {user: {word1:0..}}
            Console.WriteLine("looking through records");
            foreach (ChatRecord record in records)
            {
                if (aggregate.ContainsKey(record.User))
                {
                    //Console.WriteLine("Contained key " + record.User);
                    foreach (string word in record.Words.Keys)
                    {
                        //Console.WriteLine("looking at word " + word);
                        if (aggregate[record.User].ContainsKey(word))
                        {
                            aggregate[record.User][word] += record.Words[word];
                        }
                        else
                        {
                            aggregate[record.User].Add(word, record.Words[word]);
                        }
                        
                    }
                }
                else
                {
                    aggregate.Add(record.User, record.Words);
                }
            }

            Console.WriteLine("sorting word counts");
            //sort the word collections for each user based on the number of instances of each word
            foreach(KeyValuePair<string, Dictionary<string,int>> pair in aggregate.ToList())
            {
                //var mylist = aggregate[pair.Key].ToList();

                //mylist.Sort((x, y) => x.Value.CompareTo(y.Value));

                var sortedDict = from entry in aggregate[pair.Key] orderby entry.Value descending select entry;
                aggregate[pair.Key] = sortedDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            Console.WriteLine("Aggregated orederd recrods");

            return aggregate;

        }

        public static string PrintTopDictValues(Dictionary<string, Dictionary<string,int>> dict)
        {
            Console.WriteLine("Printing top dict values..");
            string response = "";
            if (dict.Count < 1) { response = "No messages stored  yet"; Console.WriteLine("no values"); return response; };
            foreach (KeyValuePair<string, Dictionary<string, int>> kvp in dict)
            {
                Console.WriteLine("kvp with key :" + kvp.Key);

                string tempstr = "<@{0}> used **{1}** {2} times, wordlen: {3}";
                var max = kvp.Value.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                response += string.Format(tempstr, kvp.Key.TrimStart(), max, kvp.Value[max], max.Length) + Environment.NewLine;

            }
            Console.WriteLine("Final response: " + response);
            return response;
        }

        public static string PrintUserStats(Dictionary<string, Dictionary<string, int>> dict, string userid)
        {
            Console.WriteLine("printing stats for user " + userid);
            Dictionary<string, int> userdict = null;
            if (dict.Keys.Contains(" "+userid))
            {
                Console.WriteLine("Found userid in chat");
                userdict = dict[" " + userid];
            }

            if (userdict != null)
            {
                string message = "The top words for <@"+userid+"> :\n";

                int i = 1;
                foreach(KeyValuePair<string,int> kvp in userdict)
                {
                    if (i < 6)
                    {
                        string temp = "*{0}*.**{1}** - {2} time(s)\n";
                        message += String.Format(temp, i, kvp.Key, kvp.Value);
                        i += 1;
                    }
                    else
                    {
                        break;
                    }                  
                }
                return message;
            }
            return "No chat log found for this user, try another!";
            
        }

        public static string GetTopServerStats(Dictionary<string, Dictionary<string, int>> dict)
        {
            Console.WriteLine("Getting server stats..");
            string response = "";
            if (dict.Count < 1) { response = "No messages stored  yet"; Console.WriteLine("no values"); return response; };
            Dictionary<string, int> wordcountdict = new Dictionary<string, int>();

            foreach(KeyValuePair<string,Dictionary<string,int>> kvp in dict)
            {
                foreach(KeyValuePair<string,int> kp in kvp.Value)
                {
                    //for each word:count pair for that user
                    //if we already aggregated that word, then increase count
                    if (wordcountdict.ContainsKey(kp.Key))
                    {
                        wordcountdict[kp.Key] += kp.Value;
                    }
                    //else, add the word to the aggregate
                    else
                    {
                        wordcountdict.Add(kp.Key,kp.Value);
                    }
                }
            }

            var sortedDict = from entry in wordcountdict orderby entry.Value descending select entry;
            wordcountdict = sortedDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Console.WriteLine("Got the top words");
            response = "Here are the most used words in this server:\n";

            int i = 0;
            foreach(KeyValuePair<string,int> kvp in wordcountdict)
            {
                if(i < 10)
                {
                    response += $"*{i+1}*. **{kvp.Key}** - {kvp.Value} time(s)" + Environment.NewLine;
                    i += 1;
                }
            }
            
            return response;

        }
        
    }
}
