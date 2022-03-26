namespace TomatenMusic_Api.Models
{
    public class TrackPlayRequest
    {
        public ulong GuildId { get; set; }
        public string TrackUri { get; set; }
        public bool Now { get; set; }
        public int StartTimeSeconds { get; set; }
    }
}
