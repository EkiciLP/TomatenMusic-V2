using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TomatenMusic.Util;
using DSharpPlus.Entities;
using Lavalink4NET.Player;
using TomatenMusicCore.Music;

namespace TomatenMusic.Music.Entitites
{
    public interface ILavalinkPlaylist : IPlayableItem
    {
        public string Title { get; }
        public TrackList Tracks { get; }
        public Uri Url { get; }
        public string AuthorName { get; set; }
        public Uri AuthorUri { get; set; }
        public string Description { get; set; }
        public string Identifier { get; }
        public Uri AuthorThumbnail { get; set; }

        public TimeSpan GetLength()
        {
            TimeSpan timeSpan = TimeSpan.FromTicks(0);

            foreach (var track in Tracks)
            {
                timeSpan = timeSpan.Add(track.Duration);
            }

            return timeSpan;
        }
    }
}
