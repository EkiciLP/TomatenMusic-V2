using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using Lavalink4NET.Player;
using Microsoft.Extensions.DependencyInjection;
using TomatenMusic.Services;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusic.Music.Entitites
{
    public class YoutubePlaylist : ILavalinkPlaylist
    {
        public string Title { get; }

        public TrackList Tracks { get; }

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

        public YoutubePlaylist(string name, TrackList tracks, string id)
        {
            Identifier = id;
            Title = name;
            Tracks = tracks;
            Url = new Uri($"https://youtube.com/playlist?list={id}");
            TrackCount = tracks.Count();

        }

        public async Task Play(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = true)
        {
            await player.PlayerQueue.QueuePlaylistAsync(this);


            if (player.State == PlayerState.NotPlaying)
            {
                LavalinkTrack nextTrack = player.PlayerQueue.NextTrack().Track;
                await player.PlayAsync(nextTrack);
            }
        }

        public async Task PlayNow(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool withoutQueuePrepend = false)
        {
            if (!player.PlayerQueue.Queue.Any())
                player.PlayerQueue.CurrentPlaylist = this;

            if (!withoutQueuePrepend && player.State == PlayerState.Playing)
                player.PlayerQueue.Queue = new Queue<TomatenMusicTrack>(player.PlayerQueue.Queue.Prepend(new TomatenMusicTrack(player.PlayerQueue.LastTrack.WithPosition(player.TrackPosition))));



            Queue<TomatenMusicTrack> reversedTracks = new Queue<TomatenMusicTrack>(Tracks);

            TomatenMusicTrack track = reversedTracks.Dequeue();
            player.PlayerQueue.LastTrack = track;
            await player.PlayAsync(track);

            reversedTracks.Reverse();

            foreach (var item in reversedTracks)
            {
                player.PlayerQueue.Queue = new Queue<TomatenMusicTrack>(player.PlayerQueue.Queue.Prepend(item));
            }
        }
    }
}
