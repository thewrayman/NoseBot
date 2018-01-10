using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot.Util
{
    public static class Constants
    {
        /// <summary>
        /// Base files to be copied from the base folder for each disc server
        /// </summary>
        public static class TemplateFiles
        {
            //stream/twitch files
            public static string JSONNAMES      { get { return @"Names.json";} }
            public static string JSONIDS        { get { return @"IDs.json";} }
            public static string JSONUSERS      { get { return @"Users.json";} }
            public static string JSONSTREAMS    { get { return @"Streams.json";} }
            public static string JSONLIVE       { get { return @"Live.json";} }
            public static string JSONSTART      { get { return @"Start.json";} }
            public static string JSONSTOP       { get { return @"Stop.json";} }

            //general server files
            public static string JSONSETTINGS   { get { return @"Settings.json";} }
            public static string PROCESSLOG     { get { return @"Log.txt";} }
            //crypto service files
            public static string COINS          { get { return @"coins.json";} }
            public static string PORTFOLOIOS    { get { return @"portfolios.json";} }
        }

        /// <summary>
        /// Files stored in the root folder for general bot settings
        /// </summary>
        public static class BaseFiles
        {
            public static string JSONGUILDS     { get { return @"Guilds.json";} }
            public static string TEMPLATEFOLDER { get { return @"templates\"; } }
        }

        public static class URLs
        {
            public static string CMCBASE        { get { return "https://api.coinmarketcap.com/v1/ticker/"; } }
            public static string TWITCHBASE     { get { return "https://api.twitch.tv/kraken/"; } }

            public static string INVITEURL      { get { return "https://discordapp.com/oauth2/authorize?client_id=339505551115419648&scope=bot&permissions=268958784"; } }
        }

        public static List<string> GetIteratorList(Type t)
        {
            Type type;
            if(t == typeof(TemplateFiles))
            {
                type = typeof(TemplateFiles);
            }
            else if(t == typeof(BaseFiles))
            {
                type = typeof(BaseFiles);
            }
            else
            {
                return null;
            }
            Console.WriteLine($"Executing {MethodBase.GetCurrentMethod().Name}");
            var files = type.GetProperties();

            List<string> filelist = new List<string>();
            foreach (var p in files)
            {
                Console.WriteLine("getting value..");
                var v = p.GetValue(null); // static classes cannot be instanced, so use null...
                filelist.Add((string)v);
            }
            return filelist;

        }

    }
}
