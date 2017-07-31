using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot
{
    /*
     *    "_total": 2,
   "users": [
      {
         "_id": "44322889",
         "bio": "Just a gamer playing games and chatting. :)",
         "created_at": "2013-06-03T19:12:02.580593Z",
         "display_name": "dallas",
         "logo": "https://static-cdn.jtvnw.net/jtv_user_pictures/dallas-profile_image-1a2c906ee2c35f12-300x300.png",
         "name": "dallas",
         "type": "staff",
         "updated_at": "2017-02-09T16:32:06.784398Z"
      },
     * 
     */

    class Users
    {
        public int _total { get; set; }
        public User[] users { get; set; }
        public Users()
        {

        }
    }
}
