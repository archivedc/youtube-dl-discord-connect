using System;
using Xunit;
using youtube_dl_discord_connect;

namespace youtube_dl_discord_connect.tests
{
    public class UrlFormatterTest
    {
        [Fact]
        public void YoutuDotBeShortLink1()
        {
            var check = "https://youtu.be/_5RW17PWB8E";
            var want = "https://www.youtube.com/watch?v=_5RW17PWB8E";
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be converted to {want}");
        }
    }
}
