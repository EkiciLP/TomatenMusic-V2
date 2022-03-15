﻿using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;
using TomatenMusic.Music.Entitites;

namespace TomatenMusic.Music
{
    public class MusicActionResponse
    {
        public LavalinkPlaylist Playlist { get; }
        public LavalinkTrack Track { get; }
        public IEnumerable<LavalinkTrack> Tracks { get; }
        public bool isPlaylist { get; }
        public MusicActionResponse(LavalinkTrack track = null, LavalinkPlaylist playlist = null, IEnumerable<LavalinkTrack> tracks = null)
        {
            Playlist = playlist;
            Track = track;
            isPlaylist = playlist != null;
            Tracks = tracks;
        }
    }
}