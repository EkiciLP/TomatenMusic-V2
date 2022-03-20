using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Services;
using System.Linq;
using SpotifyAPI.Web;
using Lavalink4NET.Player;
using Microsoft.Extensions.DependencyInjection;
using Lavalink4NET;

namespace TomatenMusic.Music.Entitites
{
    public class FullTrackContext
    {
        public bool IsFile { get; set; }
        public string YoutubeDescription { get; set; }
        public Uri YoutubeUri { get; set; }
        public IEnumerable<string> YoutubeTags { get; set; }
        public ulong YoutubeViews { get; set; }
        public ulong YoutubeLikes { get; set; }
        public Uri YoutubeThumbnail { get; set; }
        public DateTime YoutubeUploadDate { get; set; }
        //
        // Summary:
        //     Gets the author of the track.
        public Uri YoutubeAuthorThumbnail { get; set; }
        public ulong YoutubeAuthorSubs { get; set; }
        public Uri YoutubeAuthorUri { get; set; }
        public ulong? YoutubeCommentCount { get; set; }
        public string SpotifyIdentifier { get; set; }
        public SimpleAlbum SpotifyAlbum { get; set; }
        public List<SimpleArtist> SpotifyArtists { get; set; }
        public int SpotifyPopularity { get; set; }
        public Uri SpotifyUri { get; set; }

        public static async Task<LavalinkTrack> PopulateAsync(LavalinkTrack track, string spotifyIdentifier = null)
        {
            FullTrackContext context = (FullTrackContext)track.Context;

            if (context == null)
                context = new FullTrackContext();

            var spotifyService = TomatenMusicBot.ServiceProvider.GetRequiredService<ISpotifyService>();
            var youtubeService = TomatenMusicBot.ServiceProvider.GetRequiredService<YoutubeService>();
            context.SpotifyIdentifier = spotifyIdentifier;
            context.YoutubeUri = new Uri($"https://youtu.be/{track.TrackIdentifier}");
            track.Context = context;
            await youtubeService.PopulateTrackInfoAsync(track);
            await spotifyService.PopulateTrackAsync(track);
            
            return track;
        }

        public static async Task<IEnumerable<LavalinkTrack>> PopulateTracksAsync(IEnumerable<LavalinkTrack> tracks)
        {
            foreach (var trackItem in tracks)
            {
                await PopulateAsync(trackItem);
            }

            return tracks;
        }


        
    }
}
