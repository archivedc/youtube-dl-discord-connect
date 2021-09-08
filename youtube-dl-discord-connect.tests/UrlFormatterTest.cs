using System;
using Xunit;
using youtube_dl_discord_connect;

namespace youtube_dl_discord_connect.tests
{
    public class UrlFormatterTest
    {
        [Fact]
        public void YoutuDotBeShortLink()
        {
            var check = "https://youtu.be/_5RW17PWB8E";
            var want = "https://www.youtube.com/watch?v=_5RW17PWB8E";
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be converted to {want}");
        }

        [Fact]
        public void YoutuDotBeShortLinkWithTimeCode()
        {
            var check = "https://youtu.be/YGVibctgLsg?t=5";
            var want = "https://www.youtube.com/watch?v=YGVibctgLsg";
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be converted to {want}. Extra parameters must be ignored.");
        }

        [Fact]
        public void YoutubeDotComWithoutWww()
        {
            var check = "https://youtube.com/watch?v=YGlAWg1YmQ8";
            var want = "https://www.youtube.com/watch?v=YGlAWg1YmQ8";
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be converted to {want}.");
        }

        [Fact]
        public void YoutubeDotComWithoutHttps()
        {
            var check = "http://www.youtube.com/watch?v=hcqXjcO791s";
            var want = "https://www.youtube.com/watch?v=hcqXjcO791s";
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be converted to {want}.");
        }

        [Fact]
        public void YoutubeChannelPage()
        {
            var check = "https://www.youtube.com/channel/UCah4_WVjmr8XA7i5aigwV-Q";
            string want = null;
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be error.");
        }

        [Fact]
        public void YoutubePlaylistPage()
        {
            var check = "https://www.youtube.com/playlist?list=PL_Y0U3KlPL0c_GRDoMbZsWlBNEoSb10Hh";
            string want = null;
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be error.");
        }

        [Fact]
        public void YoutubeDotComWithoutWwwPlaylistPage()
        {
            var check = "https://youtube.com/playlist?list=PL_Y0U3KlPL0c_GRDoMbZsWlBNEoSb10Hh";
            string want = null;
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be error.");
        }

        [Fact]
        public void YoutubeDotComWithAdditionalParameters()
        {
            var check = "https://www.youtube.com/watch?v=9dK3lA-GEc0&list=PL_Y0U3KlPL0c_GRDoMbZsWlBNEoSb10Hh&index=5&t=12s";
            string want = "https://www.youtube.com/watch?v=9dK3lA-GEc0";
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be converted to {want}. Extra parameters must be ignored.");
        }

        [Fact]
        public void YoutuDotBeWithAdditionalParameters()
        {
            var check = "https://youtu.be/9dK3lA-GEc0?list=PL_Y0U3KlPL0c_GRDoMbZsWlBNEoSb10Hh&t=12";
            string want = "https://www.youtube.com/watch?v=9dK3lA-GEc0";
            var res = UrlFormatter.FormatUrl(check);

            Assert.True(res == want, $"{check} must be converted to {want}. Extra parameters must be ignored.");
        }
    }
}
