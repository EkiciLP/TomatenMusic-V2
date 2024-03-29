﻿using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Commands.Checks;
using TomatenMusic.Music;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Util;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusic.Commands
{

        [SlashCommandGroup("playnow", "Plays the specified Song now and prepends the Current song to the Queue.")]
        public class PlayNowGroup : ApplicationCommandModule
        {
            public IAudioService _audioService { get; set; }
            public ILogger<PlayNowGroup> _logger { get; set; }
            public TrackProvider _trackProvider { get; set; }

            public PlayNowGroup(IAudioService audioService, ILogger<PlayNowGroup> logger, TrackProvider trackProvider)
            {
                _audioService = audioService;
                _logger = logger;
                _trackProvider = trackProvider;
            }

            [SlashCommand("query", "Play a song with its youtube/spotify link. (or youtube search)")]
            [UserInVoiceChannelCheck]
            [UserInMusicChannelCheck(true)]
            [OnlyGuildCheck]
            public async Task PlayQueryCommand(InteractionContext ctx, [Option("query", "The song search query.")] string query)
            {
            var sw = Stopwatch.StartNew();

                await ctx.DeferAsync(true);

                GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);
                
                MusicActionResponse response;

                try
                {
                    response = await _trackProvider.SearchAsync(query);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ An error occured while resolving your query: ``{ex.Message}``, ```{ex.StackTrace}```")
                      );
                sw.Stop();
                _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                return;
                }

                try
                {
                    player = await _audioService.JoinAsync<GuildPlayer>(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id, true);
                }
                catch (Exception ex)
                {
                    player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);

                    if (player == null || player.VoiceChannelId == null)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                         .WithContent($"❌ An error occured while connecting to your Channel: ``{ex.Message}``")
                          );
                    sw.Stop();
                    _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                    return;
                    }
                }

                try
                {
                    if (response.IsPlaylist)
                    {
                        ILavalinkPlaylist playlist = response.Playlist;
                        await player.PlayNowAsync(playlist);

                        _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Now Playing:").AddEmbed(
                        Common.AsEmbed(playlist)
                        ));

                    }
                    else
                    {
                        TomatenMusicTrack track = response.Track;

                        _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Playing Now")
                            .AddEmbed(Common.AsEmbed(track, player.PlayerQueue.LoopType, 0)));

                        await player.PlayNowAsync(response.Track);
                    }
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ An error occured while playing your Query: ``{ex.Message}``")
                      );
                sw.Stop();
                _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                return;
                }
            sw.Stop();
            _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
        }

            [SlashCommand("file", "Play a song file. (mp3/mp4)")]
            [UserInVoiceChannelCheck]
            [UserInMusicChannelCheck(true)]
            [OnlyGuildCheck]
            public async Task PlayFileCommand(InteractionContext ctx, [Option("File", "The File that should be played.")] DiscordAttachment file)
            {
            var sw = Stopwatch.StartNew();

            await ctx.DeferAsync(true);

                GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);

                MusicActionResponse response;

                try
                {
                    response = await _trackProvider.SearchAsync(new Uri(file.Url));
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ An error occured while resolving your file: ``{ex.Message}``")
                      );
                sw.Stop();
                _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                return;
                }

                try
                {
                    player = await _audioService.JoinAsync<GuildPlayer>(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id, true);
                }
                catch (Exception ex)
                {
                    player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);
                    if (player == null || player.VoiceChannelId == null)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                         .WithContent($"❌ An error occured while connecting to your Channel: ``{ex.Message}``")
                          );
                    sw.Stop();
                    _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                    return;
                    }
                }

                LavalinkTrack track = response.Track;

                _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Playing Now")
                    .AddEmbed(Common.AsEmbed(track, player.PlayerQueue.LoopType, 0)));

                await player.PlayNowAsync(response.Track);
            sw.Stop();
            _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
        }
        }

        [SlashCommandGroup("play", "Queues or plays the Song")]
        public class PlayQueueGroup : ApplicationCommandModule
        {
            public IAudioService _audioService { get; set; }
            public ILogger<PlayQueueGroup> _logger { get; set; }
            public TrackProvider _trackProvider { get; set; }

            public PlayQueueGroup(IAudioService audioService, ILogger<PlayQueueGroup> logger, TrackProvider trackProvider)
            {
                _audioService = audioService;
                _logger = logger;
                _trackProvider = trackProvider;
            }


            [SlashCommand("query", "Play a song with its youtube/spotify link. (or youtube search)")]
            [UserInVoiceChannelCheck]
            [UserInMusicChannelCheck(true)]
            [OnlyGuildCheck]
            public async Task PlayQueryCommand(InteractionContext ctx, [Option("query", "The song search query.")] string query)
            {
            var sw = Stopwatch.StartNew();

            await ctx.DeferAsync(true);

                GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);

                MusicActionResponse response;

                try
                {
                    response = await _trackProvider.SearchAsync(query);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ An error occured while resolving your query: ``{ex.Message}``, ```{ex.StackTrace}```")
                      );
                sw.Stop();
                _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                return;
                }

                try
                {
                    player = await _audioService.JoinAsync<GuildPlayer>(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id, true);
                }
                catch (Exception ex)
                {
                    player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);
                    if (player == null || player.VoiceChannelId == null)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                         .WithContent($"❌ An error occured while connecting to your Channel: ``{ex.Message}``")
                          );
                    sw.Stop();
                    _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                    return;
                    }
                }

            try
                {
                    if (response.IsPlaylist)
                    {
                        ILavalinkPlaylist playlist = response.Playlist;
                        await player.PlayItemAsync(playlist);

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Now Playing:").AddEmbed(
                        Common.AsEmbed(playlist)
                        ));

                    }
                    else
                    {
                        LavalinkTrack track = response.Track;

                        _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(player.State == PlayerState.NotPlaying ? "Now Playing:" : "Added to Queue")
                            .AddEmbed(Common.AsEmbed(track, player.PlayerQueue.LoopType, player.State == PlayerState.NotPlaying ? 0 : player.PlayerQueue.Queue.Count + 1)));

                        await player.PlayItemAsync(response.Track);
                    }
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ An error occured while playing your Track: ``{ex.Message}``, ```{ex.StackTrace}```")
                      );
                sw.Stop();
                _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                return;
                }
            sw.Stop();
            _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
        }

            [SlashCommand("file", "Play a song file. (mp3/mp4)")]
            [UserInVoiceChannelCheck]
            [UserInMusicChannelCheck(true)]
            [OnlyGuildCheck]
            public async Task PlayFileCommand(InteractionContext ctx, [Option("File", "The File that should be played.")] DiscordAttachment file)
            {
            var sw = Stopwatch.StartNew();

            await ctx.DeferAsync(true);

                GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);

                MusicActionResponse response;

                try
                {
                    response = await _trackProvider.SearchAsync(new Uri(file.Url));
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                     .WithContent($"❌ An error occured while resolving your file: ``{ex.Message}``")
                      );
                sw.Stop();
                _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                return;
                }

                try
                {
                    player = await _audioService.JoinAsync<GuildPlayer>(ctx.Guild.Id, ctx.Member.VoiceState.Channel.Id, true);
                }
                catch (Exception ex)
                {
                    player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);
                    if (player == null || player.VoiceChannelId == null)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                         .WithContent($"❌ An error occured while connecting to your Channel: ``{ex.Message}``")
                          );
                    sw.Stop();
                    _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
                    return;
                    }
                }

                LavalinkTrack track = response.Track;

                _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(player.State == PlayerState.NotPlaying ? "Now Playing:" : "Added to Queue")
                    .AddEmbed(Common.AsEmbed(track, player.PlayerQueue.LoopType, player.State == PlayerState.NotPlaying ? 0 : player.PlayerQueue.Queue.Count + 1)));

                await player.PlayItemAsync(response.Track);

            sw.Stop();
            _logger.LogDebug($"Command {ctx.CommandName} took {sw.ElapsedMilliseconds}ms to execute.");
            }
        }
}
