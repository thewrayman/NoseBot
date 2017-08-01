using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot
{
    public class Settings
    {
        public string serverid { get; set; }
        public string prefix { get; set; }        
        public int commanddelay { get; set; }
        public Dictionary<string,string> customcommands { get; set; }


        public Settings() { }
        public Settings(string pref, string id, int del) {
            prefix = pref;
            serverid = id;
            commanddelay = del;
        }


        /*
         * 
         * returns true if adding new, returns false if modifying
         */ 
        public bool AddOrModifyCommand(string command, string reaction)
        {
            //if no commands exist, create new dict
            if (customcommands == null)
            {
                customcommands = new Dictionary<string, string>();
            }

            //if command exists, but they want to modify the reaction, change reaction
            if (customcommands.ContainsKey(command))
            {
                customcommands[command] = reaction;
                return false;
            }
            //if it doesn't exist, add both command and reaction
            else
            {
                customcommands.Add(command, reaction);
                return true;
            }
        }

        public bool DelCommand(string command)
        {
            if(customcommands == null)
            {
                return false;
            }
            if (customcommands.ContainsKey(command))
            {
                customcommands.Remove(command);
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
