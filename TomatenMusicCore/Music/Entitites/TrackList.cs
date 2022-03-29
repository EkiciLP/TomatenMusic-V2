

using Lavalink4NET.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TomatenMusic.Music;

namespace TomatenMusicCore.Music.Entities
{
    //
    // Summary:
    //     A thread-safe queue for Lavalink4NET.Player.LavalinkTrack.
    public sealed class TrackList : IList<TomatenMusicTrack>, ICollection<TomatenMusicTrack>, IEnumerable<TomatenMusicTrack>, IEnumerable, IPlayableItem
    {
        private readonly List<TomatenMusicTrack> _list;

        private readonly object _syncRoot;

        //
        // Summary:
        //     Gets the number of queued tracks.
        //
        // Remarks:
        //     This property is thread-safe, so it can be used from multiple threads at once
        //     safely.
        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _list.Count;
                }
            }
        }

        //
        // Summary:
        //     Gets a value indicating whether the queue is empty.
        //
        // Remarks:
        //     This property is thread-safe, so it can be used from multiple threads at once
        //     safely.
        public bool IsEmpty => Count == 0;

        //
        // Summary:
        //     Gets a value indicating whether the queue is read-only.
        //
        // Remarks:
        //     This property is thread-safe, so it can be used from multiple threads at once
        //     safely.
        public bool IsReadOnly => false;

        //
        // Summary:
        //     Gets or sets the enqueued tracks.
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public IReadOnlyList<TomatenMusicTrack> Tracks
        {
            get
            {
                lock (_syncRoot)
                {
                    return _list.ToArray();
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    _list.Clear();
                    _list.AddRange(value);
                }
            }
        }

        public string Title => $"Track List with {Count} Tracks";

        //
        // Summary:
        //     Gets or sets the track at the specified index.
        //
        // Parameters:
        //   index:
        //     the zero-based position
        //
        // Returns:
        //     the track at the specified index
        //
        // Remarks:
        //     This indexer property is thread-safe, so it can be used from multiple threads
        //     at once safely.
        public TomatenMusicTrack this[int index]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _list[index];
                }
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                lock (_syncRoot)
                {
                    _list[index] = value;
                }
            }
        }


        public TrackList()
        {
            _list = new List<TomatenMusicTrack>();
            _syncRoot = new object();
        }

        public TrackList(IEnumerable<LavalinkTrack> tracks)
        {
            _list = new List<TomatenMusicTrack>();
            _syncRoot = new object();

            foreach (var track in tracks)
                Add(new TomatenMusicTrack(track));
        }

        //
        // Summary:
        //     Adds a track at the end of the queue.
        //
        // Parameters:
        //   track:
        //     the track to add
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     thrown if the specified track is null.
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void Add(TomatenMusicTrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            lock (_syncRoot)
            {
                _list.Add(track);
            }
        }

        //
        // Summary:
        //     Adds all specified tracks to the queue.
        //
        // Parameters:
        //   tracks:
        //     the tracks to enqueue
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     thrown if the specified tracks enumerable is null.
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void AddRange(IEnumerable<TomatenMusicTrack> tracks)
        {
            if (tracks == null)
            {
                throw new ArgumentNullException("tracks");
            }

            lock (_syncRoot)
            {
                _list.AddRange(tracks);
            }
        }

        //
        // Summary:
        //     Clears all tracks from the queue.
        //
        // Returns:
        //     the number of tracks removed
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public int Clear()
        {
            lock (_syncRoot)
            {
                int count = _list.Count;
                _list.Clear();
                return count;
            }
        }

        //
        // Summary:
        //     Gets a value indicating whether the specified track is in the queue.
        //
        // Parameters:
        //   track:
        //     the track to find
        //
        // Returns:
        //     a value indicating whether the specified track is in the queue
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public bool Contains(TomatenMusicTrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            lock (_syncRoot)
            {
                return _list.Contains(track);
            }
        }

        //
        // Summary:
        //     Copies all tracks to the specified array at the specified index.
        //
        // Parameters:
        //   array:
        //     the array to the tracks to
        //
        //   index:
        //     the zero-based writing start index
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void CopyTo(TomatenMusicTrack[] array, int index)
        {
            lock (_syncRoot)
            {
                _list.CopyTo(array, index);
            }
        }

        //
        // Summary:
        //     Dequeues a track using the FIFO method.
        //
        // Returns:
        //     the dequeued track
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     thrown if no tracks were in the queue
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public TomatenMusicTrack Dequeue()
        {
            lock (_syncRoot)
            {
                if (_list.Count <= 0)
                {
                    throw new InvalidOperationException("No tracks in to dequeue.");
                }

                TomatenMusicTrack result = _list[0];
                _list.RemoveAt(0);
                return result;
            }
        }

        //
        // Summary:
        //     Deletes all duplicate tracks from the queue.
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void Distinct()
        {
            lock (_syncRoot)
            {
                if (_list.Count > 1)
                {
                    TomatenMusicTrack[] collection = (from track in _list
                                                  group track by track.Identifier into s
                                                  select s.First()).ToArray();
                    _list.Clear();
                    _list.AddRange(collection);
                }
            }
        }

        //
        // Summary:
        //     Gets the track enumerator.
        //
        // Returns:
        //     the track enumerator
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public IEnumerator<TomatenMusicTrack> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _list.ToList().GetEnumerator();
            }
        }

        //
        // Summary:
        //     Gets the zero-based index of the specified track.
        //
        // Parameters:
        //   track:
        //     the track to locate
        //
        // Returns:
        //     the zero-based index of the specified track
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     thrown if the specified track is null.
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public int IndexOf(TomatenMusicTrack track)
        {
            if (track == null)
            {
                throw new ArgumentNullException("track");
            }

            lock (_syncRoot)
            {
                return _list.IndexOf(track);
            }
        }

        //
        // Summary:
        //     Inserts the specified track at the specified index.
        //
        // Parameters:
        //   index:
        //     the zero-based index to insert (e.g. 0 = top)
        //
        //   track:
        //     the track to insert
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void Insert(int index, TomatenMusicTrack track)
        {
            lock (_syncRoot)
            {
                _list.Insert(index, track);
            }
        }

        //
        // Summary:
        //     Tries to remove the specified track from the queue.
        //
        // Parameters:
        //   track:
        //     the track to remove
        //
        // Returns:
        //     a value indicating whether the track was found and removed from the queue
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public bool Remove(TomatenMusicTrack track)
        {
            lock (_syncRoot)
            {
                return _list.Remove(track);
            }
        }

        //
        // Summary:
        //     Removes all tracks that matches the specified predicate.
        //
        // Parameters:
        //   predicate:
        //     the track predicate
        //
        // Returns:
        //     the number of tracks removed
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public int RemoveAll(Predicate<TomatenMusicTrack> predicate)
        {
            lock (_syncRoot)
            {
                return _list.RemoveAll(predicate);
            }
        }

        //
        // Summary:
        //     Removes a track at the specified index.
        //
        // Parameters:
        //   index:
        //     the index to remove the track
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void RemoveAt(int index)
        {
            lock (_syncRoot)
            {
                _list.RemoveAt(index);
            }
        }

        //
        // Summary:
        //     Removes all count tracks from the specified index.
        //
        // Parameters:
        //   index:
        //     the start index (zero-based)
        //
        //   count:
        //     the number of tracks to remove
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void RemoveRange(int index, int count)
        {
            lock (_syncRoot)
            {
                _list.RemoveRange(index, count);
            }
        }

        //
        // Summary:
        //     Shuffles / mixes all tracks in the queue.
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public void Shuffle()
        {
            lock (_syncRoot)
            {
                if (_list.Count > 2)
                {
                    TomatenMusicTrack[] collection = _list.OrderBy((TomatenMusicTrack s) => Guid.NewGuid()).ToArray();
                    _list.Clear();
                    _list.AddRange(collection);
                }
            }
        }

        //
        // Summary:
        //     Tries to dequeue a track using the FIFO method.
        //
        // Parameters:
        //   track:
        //     the dequeued track; or default is the result is false.
        //
        // Returns:
        //     a value indicating whether a track was dequeued.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     thrown if no tracks were in the queue
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        public bool TryDequeue(out TomatenMusicTrack? track)
        {
            lock (_syncRoot)
            {
                if (_list.Count <= 0)
                {
                    track = null;
                    return false;
                }

                track = _list[0];
                _list.RemoveAt(0);
                return true;
            }
        }

        //
        // Summary:
        //     Clears the queue.
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        void ICollection<TomatenMusicTrack>.Clear()
        {
            lock (_syncRoot)
            {
                _list.Clear();
            }
        }

        //
        // Summary:
        //     Gets the track enumerator.
        //
        // Returns:
        //     the track enumerator
        //
        // Remarks:
        //     This method is thread-safe, so it can be used from multiple threads at once safely.
        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _list.ToArray().GetEnumerator();
            }
        }

        public async Task Play(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = true)
        {
            await player.PlayerQueue.QueueTracksAsync(this);

            if (player.State == PlayerState.NotPlaying)
            {
                LavalinkTrack nextTrack = player.PlayerQueue.NextTrack().Track;
                await player.PlayAsync(nextTrack, startTime, endTime, noReplace);
            }
        }

        public async Task PlayNow(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool withoutQueuePrepend = false)
        {
            Queue<TomatenMusicTrack> reversedTracks = new Queue<TomatenMusicTrack>(this);

            player.PlayerQueue.Queue = new Queue<TomatenMusicTrack>(player.PlayerQueue.Queue.Prepend(new TomatenMusicTrack(player.PlayerQueue.LastTrack.WithPosition(player.TrackPosition))));

            TomatenMusicTrack track = reversedTracks.Dequeue();
            player.PlayerQueue.LastTrack = track;
            await player.PlayAsync(track, startTime, endTime);

            reversedTracks.Reverse();

            foreach (var item in reversedTracks)
            {
                player.PlayerQueue.Queue = new Queue<TomatenMusicTrack>(player.PlayerQueue.Queue.Prepend(item));
            }
        }
    }
}
