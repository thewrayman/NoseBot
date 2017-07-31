using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoseBot
{
    /*
     *el": {
            "_id": 121059319,
            "broadcaster_language": "en",
            "created_at": "2016-04-06T04:12:40Z",
            "display_name": "MOONMOON_OW",
            "followers": 251220,
            "game": "Overwatch",
            "language": "en",
            "logo": "https://static-cdn.jtvnw.net/jtv_user_pictures/moonmoon_ow-profile_image-0fe586039bb28259-300x300.png",
            "mature": true,
            "name": "moonmoon_ow",
            "partner": true,
            "profile_banner": "https://static-cdn.jtvnw.net/jtv_user_pictures/moonmoon_ow-profile_banner-13fbfa1ba07bcd8a-480.png",
            "profile_banner_background_color": null,
            "status": "KKona where my Darryl subs at KKona",
            "updated_at": "2016-12-15T20:04:53Z",
            "url": "https://www.twitch.tv/moonmoon_ow",
            "video_banner": "https://static-cdn.jtvnw.net/jtv_user_pictures/moonmoon_ow-channel_offline_image-2b3302e20384eee8-1920x1080.png",
            "views": 9869754
     */
    class Channel
    {

        public Channel() { }

        public string name { get; set; }
        public string game { get; set; }
        public string url { get; set; }
    }
}
