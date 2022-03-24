using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;

namespace TomatenMusic.Music.Entitites
{
    public class SpotifyPlaylist : ILavalinkPlaylist
    {
        public string Name { get; }
        public IEnumerable<LavalinkTrack> Tracks { get; }
        public Uri Url { get; set; }
        public string AuthorName { get; set; }
        public Uri AuthorUri { get; set; }
        public string Description { get; set; }
        public int Followers { get; set; }
        public string Identifier { get; }
        public Uri AuthorThumbnail { get; set; }


        public SpotifyPlaylist(string name, string id, IEnumerable<LavalinkTrack> tracks, Uri uri)
        {
            Name = name;
            Identifier = id;
            Tracks = tracks;
            Url = uri;
        }
    }
}
