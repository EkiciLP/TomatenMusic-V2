using DSharpPlus.Entities;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Prompt.Buttons;
using TomatenMusic.Prompt.Model;
using TomatenMusic.Util;
using TomatenMusicCore.Music;

namespace TomatenMusic.Prompt.Implementation
{
    class SongActionPrompt : ButtonPrompt
    {
        public LavalinkTrack Track { get; set; }
        public SongActionPrompt(TomatenMusicTrack track, DiscordMember requestMember, List<DiscordEmbed> embeds = null)
        {
            Embeds = embeds == null ? new List<DiscordEmbed>() : embeds;
            Track = track;

            AddOption(new AddToQueueButton(new TrackList() { track }, 1, requestMember));
        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            return new DiscordMessageBuilder().AddEmbed(Common.AsEmbed(Track)).AddEmbeds(Embeds);
        }
    }
}
