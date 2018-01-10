using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot.Util
{
    public static class FileDirUtil
    {

        public static async Task EstablishGuildFiles(Guild guild)
        {
            Console.WriteLine($"Executing {MethodBase.GetCurrentMethod().Name}");
            try
            {
                Console.WriteLine("Trying to set up new Guild folder for " + guild.name);
                //create base dir for server
                string guilddir = GetGuildDir(guild.id);
                Directory.CreateDirectory(guilddir);

                string sourceDir = Path.Combine(GetCurDir(),@"templates\");
                string targetDir = Path.Combine(guilddir);

                Console.WriteLine("Copying files from "+sourceDir + " to "+targetDir);
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
                    Console.WriteLine("copied file: " + file);
                }                  
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to establish guild");
                Console.WriteLine(e);
            }
            

        }

        public static async Task VerifyGuildFiles(string id)
        {
            Console.WriteLine($"Executing {MethodBase.GetCurrentMethod().Name}");

            List<string> files = Constants.GetIteratorList(typeof(Constants.TemplateFiles));

            string directory = GetGuildDir(id);
            
            bool verified = false;
            foreach(string file in files)
            {
                string filepath = CombineNames(directory, file);
                if (FileExists(filepath))
                {
                    verified = verified & true;
                }
                else
                {
                    Console.WriteLine("could not find file " + file);
                    try
                    {
                        File.Copy(Path.Combine(GetCurDir(), @"templates\" + file), filepath, true);
                        Console.WriteLine("Copied file successfully");
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Failed to copy guild file\n" + e);
                    }
                    
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
