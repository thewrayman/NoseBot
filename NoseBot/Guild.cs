using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot
{
    public class Guild
    {
        public string id { get; set; }
        public string name { get; set; }
        public DateTime TimeAdded { get; set; }
        public DateTime LastAccessed { get; set; }

        public Guild() { }
        public Guild(string id, string name, DateTime added, DateTime accessed)
        {
            this.id = id;
            this.name = name;
            this.TimeAdded = added;
            this.LastAccessed = accessed;
        }
    }
}
