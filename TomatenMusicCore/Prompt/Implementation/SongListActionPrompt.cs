using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Prompt.Model;
using System.Linq;
using TomatenMusic.Util;
using TomatenMusic.Music;
using Microsoft.Extensions.Logging;
using TomatenMusic.Prompt.Buttons;
using Lavalink4NET.Player;

namespace TomatenMusic.Prompt.Implementation
{
    class SongListActionPrompt : ButtonPrompt
    {
        //TODO
        public List<LavalinkTrack> Tracks { get; private set; }

        public SongListActionPrompt(List<LavalinkTrack> tracks, DiscordMember requestMember, DiscordPromptBase lastPrompt = null) : base(lastPrompt)
        {
            Tracks = tracks;

            AddOption(new AddToQueueButton(tracks, 1, requestMember));
        }

        protected override Task<DiscordMessageBuilder> GetMessageAsync()
        {

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle("What do you want to do with these Tracks?");

            builder.WithDescription(Common.TrackListString(Tracks));

            return Task.FromResult(new DiscordMessageBuilder().WithEmbed(builder.Build()));
        }
    }
}
