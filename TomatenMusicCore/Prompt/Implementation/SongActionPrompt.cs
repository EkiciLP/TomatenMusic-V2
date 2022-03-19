using DSharpPlus.Entities;
using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Prompt.Buttons;
using TomatenMusic.Prompt.Model;

namespace TomatenMusic.Prompt.Implementation
{
    class SongActionPrompt : ButtonPrompt
    {
        public LavalinkTrack Track { get; set; }
        public SongActionPrompt(LavalinkTrack track, DiscordMember requestMember, List<DiscordEmbed> embeds = null)
        {
            Embeds = embeds;
            Track = track;

            AddOption(new AddToQueueButton(new List<LavalinkTrack>() { track }, 1, requestMember));
        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            return new DiscordMessageBuilder().AddEmbeds(Embeds);
        }
    }
}
