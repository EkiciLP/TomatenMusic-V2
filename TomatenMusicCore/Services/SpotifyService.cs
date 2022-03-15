using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using System.Linq;
using Microsoft.Extensions.Logging;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Services;
using TomatenMusic.Music;
using Lavalink4NET;
using Lavalink4NET.Player;

namespace TomatenMusic.Services
{

    public interface ISpotifyService
    {
        public Task<MusicActionResponse> ConvertURL(string url);
        public Task<SpotifyPlaylist> PopulateSpotifyPlaylistAsync(SpotifyPlaylist playlist);
        public Task<SpotifyPlaylist> PopulateSpotifyAlbumAsync(SpotifyPlaylist playlist);
        public Task<LavalinkTrack> PopulateTrackAsync(LavalinkTrack track);

    }

    public class SpotifyService : SpotifyClient, ISpotifyService
    {
        public ILogger _logger { get; set; }
        public IAudioService _audioService { get; set; }

        public SpotifyService(SpotifyClientConfig config, ILogger<SpotifyService> logger, IAudioService audioService) : base(config)
        {
            _logger = logger;
            _audioService = audioService;
        }

        public async Task<MusicActionResponse> ConvertURL(string url)
        {
            string trackId = url
                .Replace("https://open.spotify.com/track/", "")
                .Replace("https://open.spotify.com/album/", "")
                .Replace("https://open.spotify.com/playlist/", "")
                .Substring(0, 22);

            if (url.StartsWith("https://open.spotify.com/track"))
            {
                FullTrack sTrack = await Tracks.Get(trackId);

                _logger.LogInformation($"Searching youtube from spotify with query: {sTrack.Name} {String.Join(" ", sTrack.Artists)}");

                var track = await _audioService.GetTrackAsync($"{sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}", Lavalink4NET.Rest.SearchMode.YouTube);

                if (track == null) throw new ArgumentException("This Spotify Track was not found on Youtube");

                return new MusicActionResponse(await FullTrackContext.PopulateAsync(track, sTrack.Uri));

            }
            else if (url.StartsWith("https://open.spotify.com/album"))
            {
                List<LavalinkTrack> tracks = new List<LavalinkTrack>();

                FullAlbum album = await Albums.Get(trackId);

                foreach (var sTrack in await PaginateAll(album.Tracks))
                {
                    _logger.LogInformation($"Searching youtube from spotify with query: {sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}");

                    var track = await _audioService.GetTrackAsync($"{sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}", Lavalink4NET.Rest.SearchMode.YouTube);

                    if (track == null) throw new ArgumentException("This Spotify Track was not found on Youtube");

                    tracks.Add(await FullTrackContext.PopulateAsync(track, sTrack.Uri));
                }
                Uri uri;
                Uri.TryCreate(url, UriKind.Absolute, out uri);

                SpotifyPlaylist playlist = new SpotifyPlaylist(album.Name, album.Id, tracks, uri);
                await PopulateSpotifyAlbumAsync(playlist);

                return new MusicActionResponse(playlist: playlist);

            }
            else if (url.StartsWith("https://open.spotify.com/playlist"))
            {
                List<LavalinkTrack> tracks = new List<LavalinkTrack>();

                FullPlaylist spotifyPlaylist = await Playlists.Get(trackId);
                
                foreach (var sTrack in await PaginateAll(spotifyPlaylist.Tracks))
                {
                    if (sTrack.Track is FullTrack)
                    {
                        FullTrack fullTrack = (FullTrack)sTrack.Track;
                        _logger.LogInformation($"Searching youtube from spotify with query: {fullTrack.Name} {String.Join(" ", fullTrack.Artists.ConvertAll(artist => artist.Name))}");

                        var track = await _audioService.GetTrackAsync($"{fullTrack.Name} {String.Join(" ", fullTrack.Artists.ConvertAll(artist => artist.Name))}", Lavalink4NET.Rest.SearchMode.YouTube);

                        if (track == null) throw new ArgumentException("This Spotify Track was not found on Youtube");

                        tracks.Add(await FullTrackContext.PopulateAsync(track, fullTrack.Uri));
                    }

                }
                Uri uri;
                Uri.TryCreate(url, UriKind.Absolute, out uri);
                SpotifyPlaylist playlist = new SpotifyPlaylist(spotifyPlaylist.Name, spotifyPlaylist.Id, tracks, uri);
                await PopulateSpotifyPlaylistAsync(playlist);

                return new MusicActionResponse(playlist: playlist);
            }
            return null;
        }
   
        public async Task<SpotifyPlaylist> PopulateSpotifyPlaylistAsync(SpotifyPlaylist playlist)
        {
            var list = await this.Playlists.Get(playlist.Identifier);
            playlist.Description = list.Description;
            playlist.AuthorUri = new Uri(list.Owner.Uri);
            playlist.AuthorName = list.Owner.DisplayName;
            playlist.Followers = list.Followers.Total;
            playlist.Url = new Uri(list.Uri);
            playlist.AuthorThumbnail = new Uri(list.Owner.Images.First().Url);
            return playlist;
        }

        public async Task<SpotifyPlaylist> PopulateSpotifyAlbumAsync(SpotifyPlaylist playlist)
        {
            var list = await this.Albums.Get(playlist.Identifier);
            playlist.Description = list.Label;
            playlist.AuthorUri = new Uri(list.Artists.First().Uri);
            playlist.AuthorName = list.Artists.First().Name;
            playlist.Followers = list.Popularity;
            playlist.Url = new Uri(list.Uri);

            return playlist;
        }

        public async Task<LavalinkTrack> PopulateTrackAsync(LavalinkTrack track)
        {
            FullTrackContext context = (FullTrackContext)track.Context;
            if (context.SpotifyIdentifier == null)
                return track;

            var spotifyTrack = await this.Tracks.Get(context.SpotifyIdentifier);

            context.SpotifyAlbum = spotifyTrack.Album;
            context.SpotifyArtists = spotifyTrack.Artists;
            context.SpotifyPopularity = spotifyTrack.Popularity;
            context.SpotifyUri = new Uri(spotifyTrack.Uri);
            track.Context = context;

            return track;
        }
    }
}
