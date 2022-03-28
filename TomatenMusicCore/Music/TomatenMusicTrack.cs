using Lavalink4NET.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music;
using TomatenMusic.Prompt.Implementation;

namespace TomatenMusicCore.Music
{
    public class TomatenMusicTrack : LavalinkTrack, IPlayableItem
    {
        

        public TomatenMusicTrack
            (LavalinkTrack track)
            : base(track.Identifier, track.Author, track.Duration, track.IsLiveStream, track.IsSeekable, track.Source, track.Title, track.TrackIdentifier, track.Provider)
        {
            Context = track.Context;
        }

        public string Title => base.Title;

        public async Task Play(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = true)
        {
            
            if (player.State == PlayerState.NotPlaying)
            {
                player.PlayerQueue.LastTrack = this;
                await player.PlayAsync(this, startTime, endTime, noReplace);
            }
            else
                player.PlayerQueue.QueueTrack(this);

        }

        public async Task PlayNow(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool withoutQueuePrepend = false)
        {
            if (!withoutQueuePrepend)
                player.PlayerQueue.Queue = new Queue<TomatenMusicTrack>(player.PlayerQueue.Queue.Prepend(player.PlayerQueue.LastTrack));

            player.PlayerQueue.LastTrack = this;
            await player.PlayAsync(this, startTime, endTime);
        }
    }
}
