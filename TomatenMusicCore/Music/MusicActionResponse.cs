using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Text;
using TomatenMusic.Music.Entitites;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusic.Music
{
    public class MusicActionResponse
    {
        public ILavalinkPlaylist Playlist { get; }
        public TomatenMusicTrack Track { get; }
        public TrackList Tracks { get; }
        public bool IsPlaylist { get; }
        public MusicActionResponse(TomatenMusicTrack track = null, ILavalinkPlaylist playlist = null, TrackList tracks = null)
        {
            Playlist = playlist;
            Track = track;
            IsPlaylist = playlist != null;
            Tracks = tracks;
            if (track != null)
            {
                var list = new TrackList();
                list.Add(track);
                Tracks = list;
            }
        }
    }
}
