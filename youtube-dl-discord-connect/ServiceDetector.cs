using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace youtube_dl_discord_connect
{
    public class ServiceDetector
    {
        public static Service DetectService(string formattedurl)
        {
            if (formattedurl.StartsWith("https://www.youtube.com/"))
                return Service.YouTube;
            else
                return Service.Other;
        }

    }

    public enum Service { 
        Other,
        YouTube,
    }
}
