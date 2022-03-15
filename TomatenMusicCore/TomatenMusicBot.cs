using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using System.Linq;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using TomatenMusic.Commands;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using TomatenMusic.Music;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web;
using DSharpPlus.Exceptions;
using Lavalink4NET;
using Lavalink4NET.DSharpPlus;
using Lavalink4NET.MemoryCache;
using TomatenMusic.Services;
using Lavalink4NET.Tracking;
using Lavalink4NET.Lyrics;
using Microsoft.Extensions.Hosting;
using Lavalink4NET.Logging;
using Lavalink4NET.Logging.Microsoft;
using TomatenMusic.Music.Entitites;

namespace TomatenMusic
{
    public class TomatenMusicBot
    {

        public class ConfigJson
        {
            [JsonProperty("TOKEN")]
            public string Token { get; private set; }
            [JsonProperty("LavaLinkPassword")]
            public string LavaLinkPassword { get; private set; }
            [JsonProperty("SpotifyClientId")]
            public string SpotifyClientId { get; private set; }
            [JsonProperty("SpotifyClientSecret")]
            public string SpotifyClientSecret { get; private set; }
            [JsonProperty("YoutubeApiKey")]
            public string YoutubeAPIKey { get; private set; }

        }

        public static IServiceProvider ServiceProvider { get; private set; }

        public IHost _host { get; set; }

        private async Task<IServiceProvider> BuildServiceProvider()
        {
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();
            ConfigJson config = JsonConvert.DeserializeObject<ConfigJson>(json);



            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                services
                    .AddSingleton(config)
                    .AddMicrosoftExtensionsLavalinkLogging()
                    .AddSingleton<TrackProvider>()
                    .AddSingleton<DiscordShardedClient>()
                    .AddSingleton( s => new DiscordConfiguration
                    {
                        Token = config.Token,
                        Intents = DiscordIntents.All,
                        LoggerFactory = s.GetRequiredService<ILoggerFactory>()

                    })

                    // Lavalink
                    .AddSingleton<IDiscordClientWrapper, DiscordShardedClientWrapper>()
                    .AddSingleton<IAudioService, LavalinkNode>()
                    .AddSingleton(new InactivityTrackingOptions
                    {
                        TrackInactivity = true
                    })
                    .AddSingleton<InactivityTrackingService>()

                    .AddSingleton(
                          new LavalinkNodeOptions
                          {
                              RestUri = "http://116.202.92.16:2333",
                              Password = config.LavaLinkPassword,
                              WebSocketUri = "ws://116.202.92.16:2333",
                              AllowResuming = true

                          })

                    .AddSingleton<ILavalinkCache, LavalinkCache>()
                    .AddSingleton<ISpotifyService, SpotifyService>()
                    .AddSingleton<YoutubeService>()
                    .AddSingleton<LyricsOptions>()
                    .AddSingleton<LyricsService>()
                    .AddSingleton(SpotifyClientConfig.CreateDefault().WithAuthenticator(new ClientCredentialsAuthenticator(config.SpotifyClientId, config.SpotifyClientSecret))))
                .Build();

            ServiceProvider = _host.Services;
            return ServiceProvider;
        }



        public async Task InitBotAsync()
        {
            await BuildServiceProvider();

            //_ = _host.StartAsync();

            _host.Start();
            var client = ServiceProvider.GetRequiredService<DiscordShardedClient>();
            var audioService = ServiceProvider.GetRequiredService<IAudioService>();
            var logger = ServiceProvider.GetRequiredService<ILogger<TomatenMusicBot>>();

            client.ClientErrored += Discord_ClientErrored;
            var slash = await client.UseSlashCommandsAsync(new SlashCommandsConfiguration
            {
                Services = ServiceProvider
            });

            slash.RegisterCommands<MusicCommands>(888493810554900491);
            slash.RegisterCommands<PlayCommandGroup>(888493810554900491);

            await client.StartAsync();
            client.Ready += Client_Ready;
            await audioService.InitializeAsync();

            var trackingService = ServiceProvider.GetRequiredService<InactivityTrackingService>();
            trackingService.ClearTrackers();
            trackingService.AddTracker(DefaultInactivityTrackers.UsersInactivityTracker);
            trackingService.AddTracker(DefaultInactivityTrackers.ChannelInactivityTracker);

        }

        private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            var slash = sender.GetSlashCommands();
            slash.SlashCommandInvoked += Slash_SlashCommandInvoked;
            slash.SlashCommandErrored += Slash_SlashCommandErrored;
            sender.UpdateStatusAsync(new DiscordActivity($"/ commands! Shard {sender.ShardId}", ActivityType.Watching));

            return Task.CompletedTask;
        }

        public async Task ShutdownBotAsync()
        {
            var audioService = ServiceProvider.GetRequiredService<IAudioService>();
            var client = ServiceProvider.GetRequiredService<DiscordShardedClient>();

            
            audioService.Dispose();
            await client.StopAsync();
        }


        private Task Discord_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<TomatenMusicBot>>();

            logger.LogDebug("Event {0} errored with Exception {3}", e.EventName, e.Exception);
            if (e.Exception is NotFoundException)
                logger.LogDebug($"{ ((NotFoundException)e.Exception).JsonMessage }");
            if (e.Exception is BadRequestException)
                logger.LogDebug($"{ ((BadRequestException)e.Exception).Errors }");
            return Task.CompletedTask;
        }

        private Task Slash_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<TomatenMusicBot>>();

            logger.LogInformation("Command {0} invoked by {1} on Guild {2} with Exception {3}", e.Context.CommandName, e.Context.Member, e.Context.Guild, e.Exception);
            if (e.Exception is NotFoundException)
                logger.LogDebug($"{ ((NotFoundException)e.Exception).JsonMessage }");
            if (e.Exception is BadRequestException)
                logger.LogDebug($"{ ((BadRequestException)e.Exception).JsonMessage }");
            return Task.CompletedTask;

        }

        private Task Slash_SlashCommandInvoked(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandInvokedEventArgs e)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<TomatenMusicBot>>();

            logger.LogInformation("Command {0} invoked by {1} on Guild {2}", e.Context.CommandName, e.Context.Member, e.Context.Guild);

            return Task.CompletedTask;
        }
    }
}
