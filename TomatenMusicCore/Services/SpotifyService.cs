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
using System.Runtime.Caching;
using TomatenMusicCore.Music;

namespace TomatenMusic.Services
{

    public interface ISpotifyService
    {
        public Task<MusicActionResponse> ConvertURL(string url);
        public Task<SpotifyPlaylist> PopulateSpotifyPlaylistAsync(SpotifyPlaylist playlist, FullPlaylist spotifyPlaylist = null);
        public Task<SpotifyPlaylist> PopulateSpotifyAlbumAsync(SpotifyPlaylist playlist, FullAlbum spotifyAlbum = null);
        public Task<LavalinkTrack> PopulateTrackAsync(LavalinkTrack track, FullTrack spotifyFullTrack = null);

    }

    public class SpotifyService : SpotifyClient, ISpotifyService
    {
        public ILogger _logger { get; set; }
        public IAudioService _audioService { get; set; }

        public ObjectCache Cache { get; set; }

        public SpotifyService(SpotifyClientConfig config, ILogger<SpotifyService> logger, IAudioService audioService) : base(config)
        {
            _logger = logger;
            _audioService = audioService;
            Cache = MemoryCache.Default;
        }

        public async Task<MusicActionResponse> ConvertURL(string url)
        {
            string trackId = url
                .Replace("https://open.spotify.com/track/", "")
                .Replace("https://open.spotify.com/album/", "")
                .Replace("https://open.spotify.com/playlist/", "")
                .Substring(0, 22);

            _logger.LogDebug($"Starting spotify conversion for: {url}");

            if (url.StartsWith("https://open.spotify.com/track"))
            {
                FullTrack sTrack = Cache.Contains(trackId) ? Cache.Get(trackId) as FullTrack : await Tracks.Get(trackId);

                _logger.LogInformation($"Searching youtube from spotify with query: {sTrack.Name} {String.Join(" ", sTrack.Artists)}");

                var track = new TomatenMusicTrack(
                    await _audioService.GetTrackAsync($"{sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}"
                    , Lavalink4NET.Rest.SearchMode.YouTube));

                if (track == null) throw new ArgumentException("This Spotify Track was not found on Youtube");

                Cache.Add(trackId, sTrack, DateTimeOffset.MaxValue);

                return new MusicActionResponse(await FullTrackContext.PopulateAsync(track, sTrack));

            }
            else if (url.StartsWith("https://open.spotify.com/album"))
            {
                TrackList tracks = new TrackList();

                FullAlbum album = Cache.Contains(trackId) ? Cache.Get(trackId) as FullAlbum : await Albums.Get(trackId);

                foreach (var sTrack in await PaginateAll(album.Tracks))
                {
                    _logger.LogInformation($"Searching youtube from spotify with query: {sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}");
                    var track = new TomatenMusicTrack(
                        await _audioService.GetTrackAsync($"{sTrack.Name} {String.Join(" ", sTrack.Artists.ConvertAll(artist => artist.Name))}"
                        , Lavalink4NET.Rest.SearchMode.YouTube));

                    if (track == null) throw new ArgumentException("This Spotify Track was not found on Youtube");
                    
                    tracks.Add(await FullTrackContext.PopulateAsync(track, spotifyId: sTrack.Uri.Replace("spotify:track:", "")));
                }
                Uri uri;
                Uri.TryCreate(url, UriKind.Absolute, out uri);

                SpotifyPlaylist playlist = new SpotifyPlaylist(album.Name, album.Id, tracks, uri);
                await PopulateSpotifyAlbumAsync(playlist, album);

                Cache.Add(trackId, album, DateTimeOffset.MaxValue);

                return new MusicActionResponse(playlist: playlist);

            }
            else if (url.StartsWith("https://open.spotify.com/playlist"))
            {

                TrackList tracks = new TrackList();

                FullPlaylist spotifyPlaylist = Cache.Contains(trackId) ? Cache.Get(trackId) as FullPlaylist : await Playlists.Get(trackId);
                
                foreach (var sTrack in await PaginateAll(spotifyPlaylist.Tracks))
                {
                    if (sTrack.Track is FullTrack)
                    {
                        FullTrack fullTrack = (FullTrack)sTrack.Track;
                        _logger.LogInformation($"Searching youtube from spotify with query: {fullTrack.Name} {String.Join(" ", fullTrack.Artists.ConvertAll(artist => artist.Name))}");

                        var track = new TomatenMusicTrack(
                            await _audioService.GetTrackAsync($"{fullTrack.Name} {String.Join(" ", fullTrack.Artists.ConvertAll(artist => artist.Name))}"
                            , Lavalink4NET.Rest.SearchMode.YouTube));

                        if (track == null) throw new ArgumentException("This Spotify Track was not found on Youtube");

                        tracks.Add(await FullTrackContext.PopulateAsync(track, fullTrack));
                    }

                }
                Uri uri;
                Uri.TryCreate(url, UriKind.Absolute, out uri);
                SpotifyPlaylist playlist = new SpotifyPlaylist(spotifyPlaylist.Name, spotifyPlaylist.Id, tracks, uri);
                await PopulateSpotifyPlaylistAsync(playlist, spotifyPlaylist);

                Cache.Add(trackId, spotifyPlaylist, DateTimeOffset.MaxValue);

                return new MusicActionResponse(playlist: playlist);
            }
            return null;
        }
   
        public async Task<SpotifyPlaylist> PopulateSpotifyPlaylistAsync(SpotifyPlaylist playlist, FullPlaylist spotifyPlaylist = null)
        {
            FullPlaylist list = spotifyPlaylist;
            if (list == null) 
                list = await this.Playlists.Get(playlist.Identifier);

            string desc = list.Description;

            playlist.Description = desc.Substring(0, Math.Min(desc.Length, 1024)) + (desc.Length > 1020 ? "..." : " ");
            if (playlist.Description.Length < 2)
                playlist.Description = "None";

            playlist.AuthorUri = new Uri($"https://open.spotify.com/user/{list.Owner.Id}");
            playlist.AuthorName = list.Owner.DisplayName;
            playlist.Followers = list.Followers.Total;
            playlist.Url = new Uri($"https://open.spotify.com/playlist/{playlist.Identifier}");
            try
            {
                playlist.AuthorThumbnail = new Uri(list.Owner.Images.First().Url);
            }
            catch (Exception ex) { }

            return playlist;
        }

        public async Task<SpotifyPlaylist> PopulateSpotifyAlbumAsync(SpotifyPlaylist playlist, FullAlbum spotifyAlbum = null)
        {
            FullAlbum list = spotifyAlbum;
            if (list == null)
                list = await this.Albums.Get(playlist.Identifier);

            string desc = list.Label;

            playlist.Description = desc.Substring(0, Math.Min(desc.Length, 1024)) + (desc.Length > 1020 ? "..." : " ");
            playlist.AuthorUri = new Uri($"https://open.spotify.com/user/{list.Artists.First().Uri}");
            playlist.AuthorName = list.Artists.First().Name;
            playlist.Followers = list.Popularity;
            playlist.Url = new Uri($"https://open.spotify.com/album/{playlist.Identifier}");

            return playlist;
        }

        public async Task<LavalinkTrack> PopulateTrackAsync(LavalinkTrack track, FullTrack spotifyFullTrack)
        {
            FullTrackContext context = (FullTrackContext)track.Context;
            if (context.SpotifyIdentifier == null)
                return track;

            FullTrack spotifyTrack = spotifyFullTrack;
            if (spotifyTrack == null)
                spotifyTrack = await Tracks.Get(context.SpotifyIdentifier);

            context.SpotifyAlbum = spotifyTrack.Album;
            context.SpotifyArtists = spotifyTrack.Artists;
            context.SpotifyPopularity = spotifyTrack.Popularity;
            context.SpotifyUri = new Uri($"https://open.spotify.com/track/{context.SpotifyIdentifier}");
            track.Context = context;

            return track;
        }
    }
}
