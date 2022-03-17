using Lavalink4NET.Player;
using System.Text.Json.Serialization;
using TomatenMusic.Music.Entitites;

namespace TomatenMusic_Api.Models
{
    public class BasicTrackInfo
    {
        public string Name { get; set; }

        public TrackPlatform Platform { get; set; }

        public string YoutubeId { get; set; }

        public string SpotifyId { get; set; }

        public Uri URL { get; set; }

        public BasicTrackInfo(LavalinkTrack track)
        {
            if (track == null)
                return;
            FullTrackContext ctx = (FullTrackContext)track.Context;

            if (ctx == null)
                return;

            Name = track.Title;
            Platform = ctx.SpotifyIdentifier == null ? TrackPlatform.YOUTUBE : TrackPlatform.SPOTIFY;
            YoutubeId = track.Identifier;
            SpotifyId = ctx.SpotifyIdentifier;
            URL = ctx.YoutubeUri;
        }
    }

    public enum TrackPlatform
    {
        YOUTUBE,
        SPOTIFY,
        FILE
    }
}
