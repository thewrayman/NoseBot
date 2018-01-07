using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot.Util
{
    public static class FileDirUtil
    {
        public const string JSONNAMES = @"Names.json";
        public const string JSONIDS = @"IDs.json";
        public const string JSONUSERS = @"Users.json";
        public const string JSONSTREAMS = @"Streams.json";
        public const string JSONLIVE = @"Live.json";
        public const string JSONGUILDS = @"Guilds.json";
        public const string JSONSETTINGS = @"Settings.json";
        public const string JSONSTART = @"Start.json";
        public const string JSONSTOP = @"Stop.json";
        public const string EVENTLOG = @"Events.txt";
        public const string PROCESSLOG = @"Log.txt";
        public const string COINS = @"coins.json";
        public const string PORTFOLOIOS = @"portfolios.json";


        public static async Task EstablishGuildFiles(Guild guild)
        {
            try
            {
                Console.WriteLine("Trying to set up new Guild folder for " + guild.name);
                //create base dir for server
                Directory.CreateDirectory(guild.id);
                string sourceDir = Path.Combine(GetCurDir(),@"templates\");
                string targetDir = Path.Combine(GetCurDir(), guild.id+@"\"); ;
                Console.WriteLine("Copying files from "+sourceDir + " to "+targetDir);
                foreach (var file in Directory.GetFiles(sourceDir))
                    File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to establish guild");
                Console.WriteLine(e);
            }
            

        }

        public static async Task VerifyGuildFiles(string id)
        {
            Console.WriteLine("verifying files for guild");
            string directory = GetGuildDir(id);
            string[] filestocheck = { JSONNAMES, JSONIDS, JSONUSERS, JSONSTREAMS, JSONLIVE, JSONSETTINGS };
            bool verified = false;
            foreach(string file in filestocheck)
            {
                string filepath = CombineNames(directory, file);
                if (FileExists(filepath))
                {
                    verified = verified & true;
                }
                else
                {
                    Console.WriteLine("could not find file " + file);
                    File.Copy(Path.Combine(GetCurDir(), @"templates\"+file), filepath, true);
                    Console.WriteLine("Made a new copy of "+file+" at "+filepath);
                }
                Console.WriteLine("verified file "+file);
            }
            Console.WriteLine("verification complete");

        }
        /*
 * Given a filename, check that the file exists
 */
        public static bool FileExists(string filename)
        {
            bool exists = false;
            if (File.Exists(filename))
            {
                exists = true;
            }
            return exists;
        }

        public static string GetCurDir()
        {
            return Directory.GetCurrentDirectory();
        }
        public static string GetGuildFile(string id, string filename)
        {
            return Path.Combine(GetGuildDir(id), filename);
        }
        public static string GetGuildDir(string id)
        {
            return Path.Combine(GetCurDir(), id);
        }


        public static string CombineNames(string s1, string s2)
        {
            return Path.Combine(s1,s2);
        }


        
    }
}
