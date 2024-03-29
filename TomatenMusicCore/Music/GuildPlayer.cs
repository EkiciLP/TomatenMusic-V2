﻿using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using System.Linq;
using TomatenMusic.Music.Entitites;
using Microsoft.Extensions.Logging;
using TomatenMusic.Services;
using TomatenMusic.Prompt.Implementation;
using Lavalink4NET.Player;
using Lavalink4NET.Events;
using Lavalink4NET;
using Lavalink4NET.Rest;
using Microsoft.Extensions.DependencyInjection;
using Lavalink4NET.Decoding;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusic.Music
{
    public class GuildPlayer : LavalinkPlayer
    {

        ILogger<GuildPlayer> _logger { get; set; }
        public PlayerQueue PlayerQueue { get;} = new PlayerQueue();
        public DiscordClient _client { get; set; }
        public ISpotifyService _spotify { get; set; }
        public IAudioService _audioService { get; set; }

        public bool Autoplay { get; set; } = false;

        public GuildPlayer()
        {
            IServiceProvider serviceProvider = TomatenMusicBot.ServiceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<GuildPlayer>>();
            var client = serviceProvider.GetRequiredService<DiscordShardedClient>();
            _client = client.GetShard(GuildId);

            _spotify = serviceProvider.GetRequiredService<ISpotifyService>();
            _audioService = serviceProvider.GetRequiredService<IAudioService>();
        }
        public async Task PlayItemAsync(IPlayableItem item, TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = true)
        {

            EnsureConnected();
            EnsureNotDestroyed();

            _ =  item.Play(this, startTime, endTime, noReplace);
            _logger.LogInformation("Started playing Item {0} on Guild {1}", item.Title, (await GetGuildAsync()).Name);

            QueuePrompt.UpdateFor(GuildId);
        }

        public async Task PlayNowAsync(IPlayableItem item, TimeSpan? startTime = null, TimeSpan? endTime = null, bool withoutQueuePrepend = false)
        {
            EnsureConnected();
            EnsureNotDestroyed();

            _ = item.PlayNow(this, startTime, endTime, withoutQueuePrepend);
            _logger.LogInformation("Started playing Item {0} now on Guild {1}", item.Title, (await GetGuildAsync()).Name);

            QueuePrompt.UpdateFor(GuildId);
        }

        public async Task RewindAsync()
        {
            EnsureNotDestroyed();
            EnsureConnected();
            if (Position.Position.Seconds > 5)
            {
                await ReplayAsync();
                return;
            }

            MusicActionResponse response = PlayerQueue.Rewind();
            
            _logger.LogInformation($"Rewinded Track {CurrentTrack.Title} for Track {response.Track.Title}");
            await base.PlayAsync(response.Track);
            QueuePrompt.UpdateFor(GuildId);
        }

        public async Task SkipAsync()
        {
            EnsureNotDestroyed();
            EnsureConnected();

            MusicActionResponse response;
            try
            {
                response = PlayerQueue.NextTrack(true, Autoplay);
            }catch (Exception ex)
            {
                if (Autoplay)
                {
                    _ = OnAutoPlay(CurrentTrack);
                    return;
                }
                throw ex;
            }

            _logger.LogInformation($"Skipped Track {CurrentTrack.Title} for Track {response.Track.Title}");
            await PlayNowAsync(response.Track, withoutQueuePrepend: true);
            QueuePrompt.UpdateFor(GuildId);
        }

        public async Task TogglePauseAsync()
        {
            EnsureNotDestroyed();
            EnsureConnected();

            if (State == PlayerState.NotPlaying) throw new InvalidOperationException("Cant pause Song! Nothing is Playing.");


            if (State == PlayerState.Paused)
                await ResumeAsync();
            else
                await PauseAsync();

            QueuePrompt.UpdateFor(GuildId);
        }

        public async Task SetLoopAsync(LoopType type)
        {

            EnsureNotDestroyed();
            EnsureConnected();

            if (State == PlayerState.NotPlaying) throw new InvalidOperationException("Cant change LoopType! Nothing is Playing.");

            _ = PlayerQueue.SetLoopAsync(type);
            QueuePrompt.UpdateFor(GuildId);

        }

        public async Task ShuffleAsync()
        {

            EnsureNotDestroyed();
            EnsureConnected();

            await PlayerQueue.ShuffleAsync();

            QueuePrompt.UpdateFor(GuildId);
        }
        public async override Task ConnectAsync(ulong voiceChannelId, bool selfDeaf = true, bool selfMute = false)
        {
            EnsureNotDestroyed();

            DiscordChannel channel = await _client.GetChannelAsync(voiceChannelId);

            if (channel.Type != ChannelType.Voice && channel.Type != ChannelType.Stage) throw new ArgumentException("The channel Id provided was not a voice channel");

            if (State != PlayerState.NotConnected)
                throw new InvalidOperationException("The Bot is already connected.");

            await base.ConnectAsync(voiceChannelId, selfDeaf, selfMute);

            if (channel.Type == ChannelType.Stage)
            {
                DiscordStageInstance stageInstance;
                try
                {
                    stageInstance = await channel.GetStageInstanceAsync();

                }catch (Exception ex)
                {
                    stageInstance = await channel.CreateStageInstanceAsync("Music");
                }
                
                await stageInstance.Channel.UpdateCurrentUserVoiceStateAsync(false);
            }

            _logger.LogInformation("Connected to Channel {0} on Guild {1}", channel.Name, channel.Guild.Name);
        }
        public override Task DisconnectAsync()
        {
            _logger.LogInformation("Disconnected from Channel {0} on Guild {1}", VoiceChannelId, GuildId);

            QueuePrompt.InvalidateFor(GuildId);
            return base.DisconnectAsync();
        }

        public override async Task SeekPositionAsync(TimeSpan timeSpan)
        {
            EnsureNotDestroyed();
            EnsureConnected();

            if (State == PlayerState.NotPlaying) throw new InvalidOperationException("Cant change LoopType! Nothing is Playing.");

            if (timeSpan.CompareTo(CurrentTrack.Duration) == 1) throw new ArgumentException("Please specify a TimeSpan shorter than the Track");

            await base.SeekPositionAsync(timeSpan);
            QueuePrompt.UpdateFor(GuildId);
        }
        protected override void Dispose(bool disposing)
        {
            QueuePrompt.InvalidateFor(GuildId);

            base.Dispose(disposing);
        }

        public async override Task OnTrackEndAsync(TrackEndEventArgs eventArgs)
        {
            DisconnectOnStop = false;
            var oldTrack = CurrentTrack;

            if (eventArgs.Reason != TrackEndReason.Finished)
                return;

            if (eventArgs.MayStartNext)
            {
                try
                {
                    MusicActionResponse response = PlayerQueue.NextTrack();
                    _ = PlayNowAsync(response.Track, withoutQueuePrepend: true);
                }
                catch (Exception ex)
                {
                    if (!Autoplay)
                    {
                        _logger.LogInformation("Track has ended and Queue was Empty... Idling");
                        QueuePrompt.UpdateFor(GuildId);
                        await base.OnTrackEndAsync(eventArgs);
                        return;
                    }

                    await base.OnTrackEndAsync(eventArgs);
                    _ = OnAutoPlay(oldTrack);

                }
            }

            
        }

        public async Task OnAutoPlay(LavalinkTrack oldTrack)
        {
            YoutubeService youtube = TomatenMusicBot.ServiceProvider.GetRequiredService<YoutubeService>();

            TomatenMusicTrack newTrack;
            if (oldTrack.Provider != StreamProvider.YouTube)
                newTrack = await youtube.GetRelatedTrackAsync(PlayerQueue.PlayedTracks.First(x => x.Provider == StreamProvider.YouTube).TrackIdentifier, PlayerQueue.PlayedTracks.Take(5).ToList().ConvertAll(x => x.TrackIdentifier));
            else
                newTrack = await youtube.GetRelatedTrackAsync(oldTrack.TrackIdentifier, PlayerQueue.PlayedTracks.Take(5).ToList().ConvertAll(x => x.TrackIdentifier));
            _logger.LogInformation($"Autoplaying for track {oldTrack.TrackIdentifier} with Track {newTrack.TrackIdentifier}");
            PlayerQueue.LastTrack = newTrack;
            await newTrack.PlayNow(this, withoutQueuePrepend: true);
            QueuePrompt.UpdateFor(GuildId);
        }

        public async Task<DiscordChannel> GetChannelAsync()
        {
            EnsureConnected();
            EnsureNotDestroyed();
            DiscordGuild guild = await GetGuildAsync();

            return guild.GetChannel((ulong) VoiceChannelId);
        }

        public async Task<DiscordGuild> GetGuildAsync()
        {
            return await _client.GetGuildAsync(GuildId);
        }
        public async Task<bool> AreActionsAllowedAsync(DiscordMember member)
        {
            if (member.VoiceState == null || member.VoiceState.Channel == null)
            {
                return false;
            }

            if (await GetChannelAsync() != null && await GetChannelAsync() != member.VoiceState.Channel)
            {
                return false;
            }

            return true;
        }

    }
}
