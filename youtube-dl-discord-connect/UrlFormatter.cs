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

            var query = System.Web.HttpUtility.ParseQueryString(url.Query);

            switch (url.Authority)
            {
                case "www.youtube.com":
                    // If not "/watch", Return error.
                    if (url.AbsolutePath != "/watch") return null;

                    // Return error if no params or no "v" params.
                    if (!query.HasKeys() || !query.AllKeys.Contains("v")) return null;

                    // If there is parameters not "v", delete that
                    if (query.Count > 1)
                    {
                        url = new Uri($"https://www.youtube.com{url.AbsolutePath}?v={query.Get("v")}");
                        goto Format;
                    }

                    // Rewrite https
                    if (scheme != "https://")
                    {
                        url = new Uri($"https://www.youtube.com{url.PathAndQuery}");
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
