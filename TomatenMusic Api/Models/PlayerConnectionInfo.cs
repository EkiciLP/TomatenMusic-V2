using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Player;
using TomatenMusic;
using TomatenMusic.Music;

namespace TomatenMusic_Api.Models
{
    public class PlayerConnectionInfo
    {

        public static async Task<PlayerConnectionInfo> Create(GuildPlayer player)
        {
            PlayerConnectionInfo response = new PlayerConnectionInfo();

            response.PlaybackPosition = player.TrackPosition;
            response.Channel_Id = (ulong)player.VoiceChannelId;
            response.Guild_Id = player.GuildId;
            response.Paused = player.State == PlayerState.Paused;
            response.CurrentTrack = new BasicTrackInfo(player.CurrentTrack);
            response.LoopType = player.PlayerQueue.LoopType;

            response.Queue = player.PlayerQueue.Queue.ToList().ConvertAll(x => new BasicTrackInfo(x));
            response.PlayedTracks = player.PlayerQueue.PlayedTracks.ToList().ConvertAll(x => new BasicTrackInfo(x));
            response.State = player.State;

            return response;
        }

        // Summary:
        //     Gets the current playback position.
        public TimeSpan PlaybackPosition
        {
            get;
            internal set;
        }
        public PlayerState State { get; set; }
        //
        // Summary:
        //     Gets the voice channel associated with this connection.
        public ulong Channel_Id { get; set; }

        //
        // Summary:
        //     Gets the guild associated with this connection.
        public ulong Guild_Id {get; set; }

        public bool Paused { get; set; }

        public BasicTrackInfo CurrentTrack { get; set; }

        public LoopType LoopType { get; set; }

        public List<BasicTrackInfo> Queue { get; set; }

        public List<BasicTrackInfo> PlayedTracks { get; set; }


    }


}
