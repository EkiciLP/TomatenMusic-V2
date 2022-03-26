using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using Lavalink4NET.Player;
using Microsoft.Extensions.DependencyInjection;
using TomatenMusic.Services;

namespace TomatenMusic.Music.Entitites
{
    public class YoutubePlaylist : ILavalinkPlaylist
    {
        public string Name { get; }

        public IEnumerable<LavalinkTrack> Tracks { get; }

        public int TrackCount { get; }

        public Uri Url { get; }

        public string AuthorName { get; set; }
        public Uri AuthorUri { get; set; }
        public string Description { get; set; }
        public Uri Thumbnail { get; set; }
        public DateTime CreationTime { get; set; }
        public string Identifier { get; }
        public Playlist YoutubeItem { get; set; }
        public Uri AuthorThumbnail { get; set; }

        public YoutubePlaylist(string name, IEnumerable<LavalinkTrack> tracks, string id)
        {
            Identifier = id;
            Name = name;
            Tracks = tracks;
            Url = new Uri($"https://youtube.com/playlist?list={id}");
            TrackCount = tracks.Count();

        }
    }
}
