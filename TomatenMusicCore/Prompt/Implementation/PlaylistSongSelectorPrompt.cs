using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Prompt;
using TomatenMusic.Prompt.Model;
using TomatenMusic.Util;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusicCore.Prompt.Implementation
{
    class PlaylistSongSelectorPrompt : PaginatedSelectPrompt<TomatenMusicTrack>
    {
        public bool IsConfirmed { get; set; }
        public Func<TrackList, Task> ConfirmCallback { get; set; } = (tracks) =>
        {
            return Task.CompletedTask;
        };

        public ILavalinkPlaylist Playlist { get; private set; }

        public PlaylistSongSelectorPrompt(ILavalinkPlaylist playlist, DiscordPromptBase lastPrompt = null, List<DiscordEmbed> embeds = null) : base(playlist.Title, playlist.Tracks.ToList(), lastPrompt, embeds)
        {
            Playlist = playlist;
            AddOption(new ButtonPromptOption
            {
                Emoji = new DiscordComponentEmoji("✔️"),
                Row = 3,
                Style = ButtonStyle.Success,
                Run = async (args, client, option) =>
                {
                    if (SelectedItems.Count == 0)
                    {
                        await args.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Please Select a Song!").AsEphemeral(true));
                        return;
                    }
                    IsConfirmed = true;
                    _ = ConfirmCallback.Invoke(new TrackList(SelectedItems));
                }
            });
        }
        public override Task<PaginatedSelectMenuOption<TomatenMusicTrack>> ConvertToOption(TomatenMusicTrack item)
        {
            return Task.FromResult<PaginatedSelectMenuOption<TomatenMusicTrack>>(new PaginatedSelectMenuOption<TomatenMusicTrack>
            {
                Label = item.Title,
                Description = item.Author
            });

        }

        public override Task OnSelect(TomatenMusicTrack item, ComponentInteractionCreateEventArgs args, DiscordClient sender)
        {
            _logger.LogDebug($"Added {item.Title}, {SelectedItems}");
            return Task.CompletedTask;
        }

        public override Task OnUnselect(TomatenMusicTrack item, ComponentInteractionCreateEventArgs args, DiscordClient sender)
        {
            _logger.LogDebug($"Removed {item.Title}");
            return Task.CompletedTask;

        }

        public async Task<TrackList> AwaitSelectionAsync()
        {
            return await Task.Run(() =>
            {
                while (!IsConfirmed)
                {
                    if (State == PromptState.INVALID)
                        throw new InvalidOperationException("Prompt has been Invalidated");
                }
                IsConfirmed = false;
                return new TrackList(SelectedItems);
            });
        }

        protected override DiscordMessageBuilder PopulateMessage(DiscordEmbedBuilder builder)
        {

            builder.WithTitle(Title);
            builder.WithDescription(Common.TrackListString(PageManager.GetPage(CurrentPage), 4000));
            builder.WithUrl(Playlist.Url);
            builder.WithAuthor(Playlist.AuthorName, Playlist.AuthorUri.ToString(), Playlist.AuthorThumbnail.ToString());
            
            List<DiscordEmbed> embeds = new List<DiscordEmbed>();
            embeds.Add(builder.Build());

            if (Embeds != null)
                embeds.AddRange(Embeds);

            return new DiscordMessageBuilder().AddEmbeds(embeds);
        }
    }
}
