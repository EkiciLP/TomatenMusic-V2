using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;
using TomatenMusic.Music.Entitites;

namespace TomatenMusic.Music
{
    public class MusicActionResponse
    {
        public ILavalinkPlaylist Playlist { get; }
        public LavalinkTrack Track { get; }
        public IEnumerable<LavalinkTrack> Tracks { get; }
        public bool IsPlaylist { get; }
        public MusicActionResponse(LavalinkTrack track = null, ILavalinkPlaylist playlist = null, IEnumerable<LavalinkTrack> tracks = null)
        {
            Playlist = playlist;
            Track = track;
            IsPlaylist = playlist != null;
            Tracks = tracks;
        }
    }
}
