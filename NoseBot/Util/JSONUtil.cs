using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NoseBot.Util
{
    public static class JSONUtil
    {
        /*
         * jsonobj - data object to read the data from (pass as null if jsonstring = true)
         * filename - json file to write object to
         * jsonstring - if already serialised as a string, pass as true with a null jsonobj
         * 
         */ 
        public static void WriteJsonToFile(object jsonobj, string filename, string jsonstring = null)
        {
            string truefilename = Path.Combine(FileDirUtil.GetCurDir(),filename);
            string json = jsonstring;
            if (jsonobj != null)
            {
                try
                {
                    json = JsonConvert.SerializeObject(jsonobj);
                }
                catch (Exception e)
                {
                    Console.WriteLine("exception in serialize " + e);
                }
                
            }
            using (StreamWriter file = File.CreateText(truefilename))
            {
                try
                {
                    file.Write(json);
                    file.Close();
                    Console.WriteLine("written "+truefilename);
                }
                catch (Exception e)
                {
                    Console.WriteLine("failed to write to file.. retrying");
                    System.Threading.Thread.Sleep(1000);
                    file.Write(json);
                    file.Close();
                    Console.WriteLine("success");
                }
                
            }
        }

        public static object ReadJsonFileToObject<T>(T objtype, string filename)
        {
            Type jsonobject = JsonConvert.DeserializeObject<Type>(File.ReadAllText(filename));
            return jsonobject;
        }



        /*
         * Given the Guild id and the json file to check
         * Check the guild folder for the file mentioned
         */
        public static bool JsonExistsId(string filename, string id)
        {
            bool exists = false;
            string filepath = GetGuildPath(id) + filename;
            if (File.Exists(filepath))
            {
                exists = true;
            }
            return exists;
        }

        /*
         * Given the Guild id, return the relative path for that folder
         */ 
        public static string GetGuildPath(string id)
        {
            return Path.Combine(Directory.GetCurrentDirectory(),(@"\" + id + @"\"));
        }

        public static Dictionary<TKey, TElement> GetJsonToDic<TKey, TElement>(string file)
        {
            //Console.WriteLine("trying to produce dictionary..");
            var thisdic = JsonConvert.DeserializeObject<Dictionary< TKey, TElement>>(File.ReadAllText(file));
            return thisdic;
        }

        public static List<T> GetJsonToList<T>(string file)
        {
            //Console.WriteLine("trying to produce list..");
            var thislist = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(file));
            return thislist;
        }

        public static object GetJsonToType(string file)
        {
            Console.WriteLine("trying to produce object.. ");
            var thisobj = JsonConvert.DeserializeObject<object>(File.ReadAllText(file));
            return thisobj;
        }

        public static Settings GetSettingsObj(string id)
        {
            string settingsfile = FileDirUtil.GetGuildFile(id, FileDirUtil.JSONSETTINGS);
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsfile));
        }

    }
}
