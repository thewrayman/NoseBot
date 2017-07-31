using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot
{
    class Guilds
    {
        public List<Guild> guilds { get; set; }
        public int total {get;set;}
        public Guilds() { }


        public List<string> GetIds()
        {
            if (guilds == null) return null;
            List<string> ids = new List<string>();

            //for each guild known, grab their id
            if (guilds.Count() > 0)
            {
                foreach(Guild gd in guilds)
                {
                    ids.Add(gd.id);
                }
            }
            //retunrs a list of guilds that the bot has joined
            return ids;

        }


        public void AddGuild(Guild gd)
        {
            if(guilds == null)
            {
                this.guilds = new List<Guild>();
            }
            guilds.Add(gd);
            total = guilds.Count();
        }
    }
}
