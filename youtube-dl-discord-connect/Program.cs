using Discord;
using Discord.WebSocket;
using System;
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
                Console.WriteLine("No Config");
                return;
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

        private async Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");

            await RefreshStatusAsync();
        }

        private async Task RefreshStatusAsync(string currentjob = null)
        {
            string job = currentjob != null ? $"({currentjob})" : "";
            await _client.SetGameAsync($"Queue: {CurrentQueuedCount} {job}");
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Channel.Id != TargetChannel)
                return;

            await message.AddReactionAsync(new Emoji("\U0001F197"));

            CurrentQueuedCount++;
            await RefreshStatusAsync();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => QueueDownload(message.Content, message.Channel));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private int CurrentQueuedCount = 0;

        async void QueueDownload(string url, ISocketMessageChannel replyChannel)
        {
            (int, string) exitcode;
            await RefreshStatusAsync("Downloading Video");

            var outputfile = string.Join("", url.Select(v => InvalidFilenameChars.Contains(v) ? '_' : v));

            exitcode = DownloadVideo(url, outputfile);
            if (exitcode.Item1 != 0)
            {
                await replyChannel.SendMessageAsync($"Fail!: `{url}`\n```\n{exitcode.Item2}\n```");
                await SendLogFile(exitcode.Item2, replyChannel);
                await RefreshStatusAsync();
                CurrentQueuedCount--;
                return;
            }

            await RefreshStatusAsync("Downloading Livechat");
            exitcode = DownloadLiveChat(url, outputfile);
            if (exitcode.Item1 != 0)
            {
                await replyChannel.SendMessageAsync($"Success! (no Livechat): `{url}`");
                await SendLogFile(exitcode.Item2, replyChannel);
                await RefreshStatusAsync();
                CurrentQueuedCount--;
                return;
            }

            CurrentQueuedCount--;
            await RefreshStatusAsync();
            await replyChannel.SendMessageAsync($"Success!: `{url}`");
        }

        private async Task SendLogFile(string log, ISocketMessageChannel channel)
        {
            await channel.SendFileAsync(new MemoryStream(Encoding.UTF8.GetBytes(log)), "log.txt");
        }

        (int, string) DownloadVideo(string video, string outputfile)
        {
            var args = $"-f bestvideo+bestaudio -o \"{outputfile}.%(ext)s\" --write-sub --all-subs --write-info-json --write-thumbnail --write-annotations --write-description {video}";
            Console.WriteLine($"$ youtube-dl: {args}");
            var psi = new System.Diagnostics.ProcessStartInfo("youtube-dl", args);
            psi.RedirectStandardError = true;
            var p = System.Diagnostics.Process.Start(psi);
            p.WaitForExit();
            return (p.ExitCode, p.StandardError.ReadToEnd());
        }

        private static char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();

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
