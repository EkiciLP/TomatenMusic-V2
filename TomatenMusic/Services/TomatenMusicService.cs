using Lavalink4NET;
using TomatenMusic;
using TomatenMusic.Music;
using TomatenMusic_Api.Models;
using TomatenMusic_Api.Models.EventArgs;
using static TomatenMusic_Api.InProcessEventBus;

namespace TomatenMusic_Api
{
    public class TomatenMusicService : IHostedService
    {
		private readonly InProcessEventBus _inProcessEventBus;
		private readonly ILogger<TomatenMusicService> _logger;
        public TomatenMusicBot _bot { get; set; }
        public IAudioService _audioService { get; set; }

        public TomatenMusicService(InProcessEventBus inProcessEventBus, ILogger<TomatenMusicService> logger)
		{
			_inProcessEventBus = inProcessEventBus;
			_logger = logger;

			Initialize();
		}

		private void Initialize()
		{
            _inProcessEventBus.OnConnectRequest += _inProcessEventBus_OnConnectRequest;
            _inProcessEventBus.OnDisconnectRequest += _inProcessEventBus_OnDisconnectRequest;
            _inProcessEventBus.OnPlayRequest += _inProcessEventBus_OnPlayRequest;
		}

        private async Task _inProcessEventBus_OnPlayRequest(InProcessEventBus sender, TrackPlayArgs e)
        {
			GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(e.GuildId);

			if (e.Response.Tracks != null && e.Response.Tracks.Any())
            {
				if (e.Now)
					await player.PlayNowAsync(e.Response.Tracks);
				else
					await player.PlayItemAsync(e.Response.Tracks);

				return;
			}

			if (e.Response.IsPlaylist)
            {
				if (e.Now)
					await player.PlayPlaylistNowAsync(e.Response.Playlist);
				else
					await player.PlayPlaylistAsync(e.Response.Playlist);
			}else
            {
				if (e.Now)
					await player.PlayNowAsync(e.Response.Track, e.StartTime);
				else
					await player.PlayAsync(e.Response.Track, e.StartTime);
			}

		}

		private async Task _inProcessEventBus_OnDisconnectRequest(InProcessEventBus sender, ChannelDisconnectArgs e)
        {
            GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(e.GuildId);
			player.DisconnectAsync();
        }

        private async Task _inProcessEventBus_OnConnectRequest(InProcessEventBus sender, ChannelConnectArgs e)
        {
			GuildPlayer player = await _audioService.JoinAsync<GuildPlayer>(e.Guild_Id, e.Channel.Id, true);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting service...");
			_bot = new TomatenMusicBot();
			await _bot.InitBotAsync();
			_audioService = TomatenMusicBot.ServiceProvider.GetRequiredService<IAudioService>();
			_logger.LogInformation("Service started!");

		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Shutting down service...");
			await _bot.ShutdownBotAsync();
			_logger.LogInformation("Service shut down!");

		}
	}
}

