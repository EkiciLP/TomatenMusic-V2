using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using TomatenMusic.Music;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Commands.Checks;
using TomatenMusic.Util;
using Microsoft.Extensions.Logging;
using TomatenMusic.Prompt;
using TomatenMusic.Prompt.Model;
using TomatenMusic.Prompt.Implementation;
using TomatenMusic.Prompt.Option;
using System.Linq;
using Lavalink4NET;
using Lavalink4NET.Player;
using TomatenMusicCore.Prompt.Implementation;

namespace TomatenMusic.Commands
{
    public class MusicCommands : ApplicationCommandModule
    {
        public IAudioService _audioService { get; set; }
        public ILogger<MusicCommands> _logger { get; set; }
        public TrackProvider _trackProvider { get; set; }

        public MusicCommands(IAudioService audioService, ILogger<MusicCommands> logger, TrackProvider trackProvider)
        {
            _audioService = audioService;
            _logger = logger;
            _trackProvider = trackProvider;
        }

        [SlashCommand("stop", "Stops the current Playback and clears the Queue")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task StopCommand(InteractionContext ctx)
        {

            GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);
            try
            {
                await player.DisconnectAsync();
            }catch (Exception ex)
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder
                {
                    Content = $"❌ An Error occured : ``{ex.Message}``",
                    IsEphemeral = true
                });
                return;
            }
            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder
            {
                Content = $"✔️ The Bot was stopped successfully",
                IsEphemeral = true
            });

        }


        [SlashCommand("skip", "Skips the current song and plays the next one in the queue")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task SkipCommand(InteractionContext ctx)
        {

            GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);

            LavalinkTrack oldTrack = player.CurrentTrack;
            try
            {
                await player.SkipAsync();
            }
            catch (Exception e) 
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"⛔ Could not Skip Song, Queue Empty!").AsEphemeral(true));
                return;
            }

            _ = ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Skipped From Song ``{oldTrack.Title}`` To Song:")
                .AddEmbed(Common.AsEmbed(player.CurrentTrack, loopType: player.PlayerQueue.LoopType)).AsEphemeral(true));
        }

        [SlashCommand("fav", "Shows the favorite Song Panel")]
        [OnlyGuildCheck]
        public async Task FavCommand(InteractionContext ctx)
        {
            
        }

        [SlashCommand("search", "Searches for a specific query")]
        [OnlyGuildCheck]
        public async Task SearchCommand(InteractionContext ctx, [Option("query", "The Search Query")] string query)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);
            MusicActionResponse response;
            try
            {
                response = await _trackProvider.SearchAsync(query, true);
            }catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"❌ Search failed: ``{e.Message}``, ```{e.StackTrace}```"));
                return;
            }

            DiscordPromptBase prompt;

            if (!response.IsPlaylist && response.Tracks.Count() == 1) 
            {
                var sPrompt = new SongActionPrompt(response.Tracks.First(), ctx.Member);
                prompt = sPrompt;
            }
            else if (response.IsPlaylist)
            {
                var sPrompt = new PlaylistSongSelectorPrompt(response.Playlist);
                sPrompt.ConfirmCallback = async (tracks) =>
                {
                    var selectPrompt = new SongListActionPrompt(tracks, ctx.Member, sPrompt);
                    await selectPrompt.UseAsync(sPrompt.Interaction, sPrompt.Message);
                };
                prompt = sPrompt;
            }
            else
            {
                var sPrompt = new SongSelectorPrompt($"Search results for {query}", response.Tracks);
                sPrompt.ConfirmCallback = async (tracks) =>
                {
                    var selectPrompt = new SongListActionPrompt(tracks, ctx.Member, sPrompt);
                    await selectPrompt.UseAsync(sPrompt.Interaction, sPrompt.Message);
                };
                prompt = sPrompt;
            }


            await prompt.UseAsync(ctx.Interaction, await ctx.GetOriginalResponseAsync());
        }

        [SlashCommand("time", "Sets the playing position of the current Song.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task TimeCommand(InteractionContext ctx, [Option("time", "The time formatted like this: Hours: 1h, Minutes: 1m, Seconds 1s")] string time)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);

            TimeSpan timeSpan;

            try
            {
                timeSpan = TimeSpan.Parse(time);
            }
            catch (Exception e)
            {
                try
                {
                    timeSpan = Common.ToTimeSpan(time);
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ An Error occured when parsing your input."));
                    return;
                }
            }

            try
            {
                await player.SeekPositionAsync(timeSpan);
            }catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"❌ An Error occured while Seeking the Track: ``{ex.Message}``"));
                return;
            }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"✔️ You successfully set the Song to ``{Common.GetTimestamp(timeSpan)}``."));
        }

        [SlashCommand("pause", "Pauses or Resumes the current Song.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task PauseCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = (GuildPlayer)_audioService.GetPlayer(ctx.Guild.Id);
            try
            {
                await player.TogglePauseAsync();
            }catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"❌ An Error occured changing the pause state of the Song: ``{ex.Message}``"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"✔️ You {(player.State == PlayerState.Paused ? "successfully paused the Track" : "successfully resumed the Track")}"));

        }

        [SlashCommand("shuffle", "Shuffles the Queue.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task ShuffleCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);
            try
            {
                await player.ShuffleAsync();
            }
            catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"❌ An error occured while shuffling the Queue: ``{ex.Message}``"));
                return;
            }
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"😀 You shuffled the Queue."));

        }

        [SlashCommand("loop", "Sets the loop type of the current player.")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task LoopCommand(InteractionContext ctx, [Option("Looptype", "The loop type which the player should be set to")] LoopType type)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);
            
            try
            {
                await player.SetLoopAsync(type);
            }catch (Exception ex)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"❌ An error occured while change the Queue Loop: ``{ex.Message}``"));

            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"😀 You have set the Loop to ``{type.ToString()}``."));

        }

        [SlashCommand("autoplay", "Enables/Disables Autoplay")]
        [OnlyGuildCheck]
        [UserInMusicChannelCheck]
        public async Task AutoplayCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);

            player.Autoplay = !player.Autoplay;

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have set Autoplay to ``{(player.Autoplay ? "Enabled" : "Disabled")}``"));

        }

        [SlashCommand("queue", "Shows the Queue")]
        [OnlyGuildCheck]
        public async Task QueueCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);
            GuildPlayer player = _audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ ``Theres currently nothing playing``"));
                return;
            }

            LavalinkTrack track = player.CurrentTrack;

            if (track == null)
            {
                _ = ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("❌ ``Theres currently nothing playing``"));
                return;
            }

            QueuePrompt prompt = new QueuePrompt(player);

            _ = prompt.UseAsync(ctx.Interaction, await ctx.GetOriginalResponseAsync());
        }

    }
}
