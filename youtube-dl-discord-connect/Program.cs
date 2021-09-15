using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace youtube_dl_discord_connect
{
    class Program
    {
        private readonly DiscordSocketClient _client;

        private static string BotToken;
        private static ulong TargetChannel;

        private System.Timers.Timer QueueTimer;

        static void Main(string[] args)
        {
            Console.WriteLine("Trying to load configuration file...");
            if (File.Exists("cfg.cnf"))
            {
                string[] file = File.ReadAllLines("cfg.cnf");
                if (file.Length < 2)
                {
                    Console.WriteLine("Not valid config file.");
                    return;
                }
                BotToken = file[0];
                if (!ulong.TryParse(file[1], out TargetChannel))
                {
                    Console.WriteLine("Not valid config file.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Using System Environment");
                BotToken = Environment.GetEnvironmentVariable("BOTTOKEN");
                if (!ulong.TryParse(Environment.GetEnvironmentVariable("CHANNEL"), out TargetChannel))
                {
                    Console.WriteLine("Failed to load from System Environment");
                    return;
                }
            }

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, BotToken);
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private ISocketMessageChannel replyChannel;

        private async Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            replyChannel = (ISocketMessageChannel)_client.GetChannel(TargetChannel);

            await RefreshStatusAsync();

            QueueTimer = new System.Timers.Timer(5000)
            {
                AutoReset = true,
                Enabled = true
            };
            QueueTimer.Elapsed += QueueTimer_Elapsed;
        }

        private void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Downloading)
                return;

            if (DownloadQueue.Count < 1) return;

            Task.Run(() =>
            {
                Downloading = true;
                QueueDownload(DownloadQueue.Dequeue(), replyChannel);
                Downloading = false;
                RefreshStatusAsync().Wait();
            });
        }

        private async Task RefreshStatusAsync(string currentjob = null)
        {
            int left = DownloadQueue.Count;
            if (Downloading) left++;

            string job = currentjob != null ? $"({currentjob})" : "";
            await _client.SetGameAsync($"Queue: {left} {job}");
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Channel.Id != TargetChannel)
                return;

            string url = UrlFormatter.FormatUrl(message.Content);
            if (url == null)
            {
                await message.AddReactionAsync(new Emoji("\U0001F196" /* 🆖 */));
                return;
            }

            await message.AddReactionAsync(new Emoji("\U0001F197" /* 🆗 */));

            DownloadQueue.Enqueue(url);
            await RefreshStatusAsync();
        }

        private Queue<string> DownloadQueue = new Queue<string>();

        async void QueueDownload(string url, ISocketMessageChannel replyChannel)
        {
            (int, string) exitcode;
            await RefreshStatusAsync("Downloading Video");

            var outputfile = Path.Combine("output", string.Join("", url.Select(v => InvalidFilenameChars.Contains(v) ? '_' : v)));

            exitcode = DownloadVideo(url, outputfile);
            if (exitcode.Item1 != 0)
            {
                await replyChannel.SendMessageAsync($"Fail!: `{url}`\n```\n{exitcode.Item2}\n```");
                await SendLogFile(exitcode.Item2, replyChannel);
                return;
            }

            await RefreshStatusAsync("Downloading Livechat");
            exitcode = DownloadLiveChat(url, outputfile);
            if (exitcode.Item1 != 0)
            {
                await replyChannel.SendMessageAsync($"Success! (no Livechat): `{url}`");
                await SendLogFile(exitcode.Item2, replyChannel);
                return;
            }

            await replyChannel.SendMessageAsync($"Success!: `{url}`");
        }

        private async Task SendLogFile(string log, ISocketMessageChannel channel)
        {
            await channel.SendFileAsync(new MemoryStream(Encoding.UTF8.GetBytes(log)), "log.txt");
        }

        (int, string) DownloadVideo(string video, string outputfile)
        {
            // https://blog.yuki0311.com/how_to_use_youtube-dl/ How to write as mp4
            var args = $"-f bestvideo[ext=webm]+bestaudio[ext=webm] --merge-output-format webm --recode-video mp4 -o \"{outputfile}.%(ext)s\" --write-sub --all-subs --write-info-json --write-thumbnail --write-annotations --write-description {video}";
            Console.WriteLine($"$ youtube-dl: {args}");
            var psi = new System.Diagnostics.ProcessStartInfo("youtube-dl", args);
            psi.RedirectStandardError = true;
            var p = System.Diagnostics.Process.Start(psi);
            p.WaitForExit();
            return (p.ExitCode, p.StandardError.ReadToEnd());
        }

        private static char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();

        private bool Downloading = false;

        (int, string) DownloadLiveChat(string video, string outputfile)
        {
            var args = $"livechat_downloader/livechat.py {video} \"{outputfile}.chat.json\"";
            Console.WriteLine($"$ chat_download: {args}");
            var psi = new System.Diagnostics.ProcessStartInfo("python", args);
            psi.RedirectStandardError = true;
            var p = System.Diagnostics.Process.Start(psi);
            p.WaitForExit();
            return (p.ExitCode, p.StandardError.ReadToEnd());
        }
    }
}
