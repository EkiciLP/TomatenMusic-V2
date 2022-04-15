using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using TomatenMusic.Music.Entitites;
using System.Threading.Tasks;
using System.Linq;
using TomatenMusic.Util;
using Microsoft.Extensions.Logging;
using Lavalink4NET.Player;
using Microsoft.Extensions.DependencyInjection;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusic.Music
{
    public class PlayerQueue
    {
        
        public Queue<TomatenMusicTrack> Queue { get; set; } = new Queue<TomatenMusicTrack>();
        public Queue<TomatenMusicTrack> PlayedTracks { get; set; } = new Queue<TomatenMusicTrack>();
        public ILogger<PlayerQueue> _logger { get; set; } = TomatenMusicBot.ServiceProvider.GetRequiredService<ILogger<PlayerQueue>>();
        public ILavalinkPlaylist CurrentPlaylist { get; set; }

        public LoopType LoopType { get; private set; } = LoopType.NONE;

        public TomatenMusicTrack LastTrack { get; set; }

        public List<TomatenMusicTrack> QueueLoopList { get; private set; }

        public void QueueTrack(TomatenMusicTrack track)
        {
            CurrentPlaylist = null;
            Queue.Enqueue(track);
            _logger.LogInformation("Queued Track {0}", track.Title);

            if (LoopType == LoopType.QUEUE)
                QueueLoopList.Add(track);
        }

        public Task QueuePlaylistAsync(ILavalinkPlaylist playlist)
        {
            return Task.Run(() =>
            {
                if (CurrentPlaylist == null && Queue.Count == 0)
                    CurrentPlaylist = playlist;
                else
                    CurrentPlaylist = null;

                _logger.LogInformation("Queued Playlist {0}", playlist.Title);
                foreach (var track in playlist.Tracks)
                {
                    Queue.Enqueue(track);
                }



                if (LoopType == LoopType.QUEUE)
                    QueueLoopList.AddRange(playlist.Tracks);
            });

        }

        public Task QueueTracksAsync(TrackList tracks)
        {
            return Task.Run(() =>
            {
                CurrentPlaylist = null;
                _logger.LogInformation("Queued TrackList {0}", tracks.ToString());
                foreach (var track in tracks)
                {
                    Queue.Enqueue(track);
                }
                if (LoopType == LoopType.QUEUE)
                    QueueLoopList.AddRange(tracks);
            });

        }

        public void Clear()
        {
            Queue.Clear();
            PlayedTracks.Clear();
        }

        public void RemoveAt(int index)
        {
            if (Queue.Count == 0) throw new InvalidOperationException("Queue was Empty");
            List<TomatenMusicTrack> tracks = Queue.ToList();
            tracks.RemoveAt(index);
            Queue = new Queue<TomatenMusicTrack>(tracks);

        }

        public MusicActionResponse NextTrack(bool ignoreLoop = false, bool autoplay = false)
        {
            if (LastTrack != null)
                if (LoopType != LoopType.NONE && Queue.Count == 0 || autoplay)
                    PlayedTracks = new Queue<TomatenMusicTrack>(PlayedTracks.Prepend(new TomatenMusicTrack(LastTrack.WithPosition(TimeSpan.Zero))));

            switch (LoopType)
            {
                case LoopType.NONE:
                    if (Queue.Count == 0) throw new InvalidOperationException("Queue was Empty");

                    LastTrack = Queue.Dequeue();

                    return new MusicActionResponse(LastTrack);
                case LoopType.TRACK:
                    if (ignoreLoop)
                    {
                        LastTrack = Queue.Dequeue();
                        return new MusicActionResponse(LastTrack);
                    }

                    return new MusicActionResponse(LastTrack);
                case LoopType.QUEUE:
                    if (!Queue.Any())
                    {
                        if (CurrentPlaylist != null)
                            Queue = new Queue<TomatenMusicTrack>(CurrentPlaylist.Tracks);
                        else
                            Queue = new Queue<TomatenMusicTrack>(QueueLoopList);
                    }

                    LastTrack = Queue.Dequeue();

                    return new MusicActionResponse(LastTrack);
                default:
                    throw new NullReferenceException("LoopType was null");
            }
        }

        public MusicActionResponse Rewind()
        {

            if (!PlayedTracks.Any()) throw new InvalidOperationException("There are no songs that could be rewinded to yet.");

            Queue = new Queue<TomatenMusicTrack>(Queue.Prepend(LastTrack));
            LastTrack = PlayedTracks.Dequeue();

            return new MusicActionResponse(LastTrack);
        }

        public Task ShuffleAsync()
        {
            if (Queue.Count == 0) throw new InvalidOperationException("Queue is Empty");

            List<TomatenMusicTrack> tracks = new List<TomatenMusicTrack>(Queue);
            tracks.Shuffle();
            Queue = new Queue<TomatenMusicTrack>(tracks);
            return Task.CompletedTask;
        }

        public async Task SetLoopAsync(LoopType type)
        {
            LoopType = type;

            if (type == LoopType.QUEUE)
            {
                QueueLoopList = new List<TomatenMusicTrack>(QueueLoopList.Prepend(LastTrack));
            }
        }
    }
}
