using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace youtube_dl_discord_connect
{
    public class UrlFormatter
    {
        public static string FormatUrl(string originalurl)
        {
            Uri url;
            try
            {
                url = new Uri(originalurl);
            }
            catch
            {
                return null;
            }


            if (!url.IsAbsoluteUri) return null;

        Format:

            string scheme = url.GetLeftPart(UriPartial.Scheme);

            switch (url.Authority)
            {
                case "www.youtube.com":
                    if (scheme != "https://")
                    {
                        url = new Uri($"https://www.youtube.com/{url.AbsolutePath}");
                        goto Format;
                    }
                    break;

                case "youtube.com":
                    url = new Uri($"https://www.youtube.com{url.PathAndQuery}");
                    goto Format;

                case "youtu.be":
                    url = new Uri($"https://www.youtube.com/watch?v={url.AbsolutePath.TrimStart('/')}");
                    goto Format;
            }

            return url.AbsoluteUri;
        }
    }
}
