using TomatenMusic.Music;
using DSharpPlus;
using DSharpPlus.Entities;
using TomatenMusic_Api.Models;
using Lavalink4NET.Player;
using TomatenMusic;
using Lavalink4NET;

namespace TomatenMusic_Api
{
    public class TomatenMusicDataService : IHostedService
    {
        private ILogger<TomatenMusicDataService> _logger;
        private IServiceProvider _serviceProvider { get; set; } = TomatenMusicBot.ServiceProvider;
        public IAudioService _audioService { get; set; }
        public TrackProvider TrackProvider { get; set; }
        public TomatenMusicDataService(ILogger<TomatenMusicDataService> logger)
        {
            _logger = logger;
            _audioService = _serviceProvider.GetRequiredService<IAudioService>();
            TrackProvider = _serviceProvider.GetRequiredService<TrackProvider>();
        }

        public async Task<PlayerConnectionInfo> GetConnectionInfoAsync(ulong guild_id)
        {
            GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(guild_id);
            if (player == null)
                return null;
            return await PlayerConnectionInfo.Create(player);
        }

        public async Task<Boolean?> IsPlayingAsync(ulong guild_id)
        {
            GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(guild_id);

            if (player == null)
                return false;
            return player.State == PlayerState.Playing;
        }
        public async Task<Boolean?> IsConnectedAsync(ulong guild_id)
        {
            GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(guild_id);

            if (player == null)
                return false;

            return player.State != PlayerState.NotConnected;
        }

        public async Task<List<PlayerConnectionInfo>> GetAllGuildPlayersAsync()
        {
            List<PlayerConnectionInfo> list = new List<PlayerConnectionInfo>();
            foreach (var guild in _audioService.GetPlayers<GuildPlayer>())
            {
                list.Add(await PlayerConnectionInfo.Create(guild));
            }
            if (list.Count == 0)
                return null;              

            return list;
        }

        public Task<DiscordChannel> GetDiscordChannelAsync(ulong guild_id, ulong channel_id)
        {
            var client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            var guildClient = client.GetShard(guild_id);
            return guildClient.GetChannelAsync(channel_id);
        }

        public Task<DiscordGuild> GetGuildAsync(ulong guild_id)
        {
            var client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            var guildClient = client.GetShard(guild_id);

            return guildClient.GetGuildAsync(guild_id);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TomatenMusicDataService starting...");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TomatenMusicDataService stopping...");
            return Task.CompletedTask;

        }
    }
}
