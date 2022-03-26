using Emzi0767.Utilities;
using Lavalink4NET.Player;
using TomatenMusic.Music;

namespace TomatenMusic_Api.Models.EventArgs
{
    public class TrackPlayArgs : AsyncEventArgs
    {
        public MusicActionResponse Response { get; set; }
        public ulong GuildId { get; set; }
        public TimeSpan StartTime { get; set; }
        public bool Now { get; set; }

        public TrackPlayArgs(MusicActionResponse response, ulong guildId, TimeSpan startTime, bool now)
        {
            Response = response;
            GuildId = guildId;
            StartTime = startTime;
            Now = now;
        }
    }
}
