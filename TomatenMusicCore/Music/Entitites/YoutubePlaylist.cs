using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using Lavalink4NET.Player;

namespace TomatenMusic.Music.Entitites
{
    public class YoutubePlaylist : LavalinkPlaylist
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

        public YoutubePlaylist(string name, IEnumerable<LavalinkTrack> tracks, Uri uri)
        {
            Identifier = uri.ToString().Replace("https://www.youtube.com/playlist?list=", "");
            Name = name;
            Tracks = tracks;
            Url = uri;
            TrackCount = tracks.Count();
        }
    }
}
