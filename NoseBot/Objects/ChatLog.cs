using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot.Objects
{
    class ChatLog
    {
    /*
     * the chat log, a list of individual chat records
     * 
     */ 
        public ChatRecord[] Log { get; set; }
        public int total { get; set; }
        public ChatLog() { }


        //public void AddRecord(string user, string[] words, )
    }
}
