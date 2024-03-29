﻿using DSharpPlus.Entities;
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
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;
using TomatenMusicCore.Prompt.Buttons;

namespace TomatenMusic.Prompt.Implementation
{
    class SongListActionPrompt : ButtonPrompt
    {
        //TODO
        public TrackList Tracks { get; private set; }

        public SongListActionPrompt(TrackList tracks, DiscordMember requestMember, DiscordPromptBase lastPrompt = null) : base(lastPrompt)
        {
            Tracks = tracks;

            AddOption(new AddToQueueButton(tracks, 1, requestMember));
            AddOption(new PlayNowButton(tracks, 1, requestMember));

        }

        protected override Task<DiscordMessageBuilder> GetMessageAsync()
        {

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle("What do you want to do with these Tracks?");

            builder.WithDescription(Common.TrackListString(Tracks, 1000));

            return Task.FromResult(new DiscordMessageBuilder().WithEmbed(builder.Build()));
        }
    }
}
