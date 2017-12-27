using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot.Objects
{
    public class ChatRecord
    {
        public DateTime TimeStamp { get; set; }
        public string User { get; set; }
        public Dictionary<string,int> Words { get; set; }

        public ChatRecord()
        {
            Words = new Dictionary<string, int>();
        }
    }
}
