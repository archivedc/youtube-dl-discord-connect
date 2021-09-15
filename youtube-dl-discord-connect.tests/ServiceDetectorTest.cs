using System;
using Xunit;
using youtube_dl_discord_connect;

namespace youtube_dl_discord_connect.tests
{
    public class ServiceDetectorTest
    {
        [Fact]
        public void ServiceDetectorYouTube()
        {
            var check = "https://www.youtube.com/watch?v=3mULuTlo8D0";
            Service want = Service.YouTube;
            var res = ServiceDetector.DetectService(check);

            Assert.True(res == want, $"{check} must be YouTUbe.");
        }
    }
}
