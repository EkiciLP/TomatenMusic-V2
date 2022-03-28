using Lavalink4NET;
using Lavalink4NET.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Services;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusic.Music
{
    public class TrackProvider
    {
        public ISpotifyService _spotifyService { get; set; }
        public IAudioService _audioService { get; set; }
        public YoutubeService _youtubeService { get; set; }

        public TrackProvider(ISpotifyService spotify, IAudioService audioService, YoutubeService youtubeService)
        {
            _audioService = audioService;
            _spotifyService = spotify;
            _youtubeService = youtubeService;
        }

        public async Task<MusicActionResponse> SearchAsync(string query, bool withSearchResults = false)
        {

            Uri uri;
            TrackLoadResponsePayload loadResult;
            bool isSearch = true;

            if (query.StartsWith("https://open.spotify.com"))
            {
                return await _spotifyService.ConvertURL(query);
            }

            if (Uri.TryCreate(query, UriKind.Absolute, out uri))
            {
                loadResult = await _audioService.LoadTracksAsync(uri.ToString());
                isSearch = false;
            }
            else
                loadResult = await _audioService.LoadTracksAsync(query, SearchMode.YouTube);

            if (uri != null && uri.AbsolutePath.Contains("."))
                return await SearchAsync(uri);

            if (loadResult.LoadType == TrackLoadType.LoadFailed) throw new ArgumentException("Track loading failed");

            if (loadResult.LoadType == TrackLoadType.NoMatches) throw new FileNotFoundException("Query resulted in no Matches");

            if (withSearchResults && loadResult.LoadType == TrackLoadType.SearchResult)
            {
                return new MusicActionResponse(tracks: await FullTrackContext.PopulateTracksAsync(new TrackList(loadResult.Tracks)));
            }

            if (loadResult.LoadType == TrackLoadType.PlaylistLoaded && !isSearch)
                return new MusicActionResponse(
                    playlist: await _youtubeService.PopulatePlaylistAsync(
                        new YoutubePlaylist(loadResult.PlaylistInfo.Name, await FullTrackContext.PopulateTracksAsync(new TrackList(loadResult.Tracks)), ParseListId(query))));
            else
                return new MusicActionResponse(await FullTrackContext.PopulateAsync(new TomatenMusicTrack(loadResult.Tracks.First())));

        }

        public async Task<MusicActionResponse> SearchAsync(Uri fileUri)
        {

            var loadResult = new TomatenMusicTrack(await _audioService.GetTrackAsync(fileUri.ToString()));
            loadResult.Context = new FullTrackContext
            {
                IsFile = true
            };

            if (loadResult == null)
                throw new FileNotFoundException("The file was not found");

            return new MusicActionResponse(loadResult);

        }

        public string ParseListId(string url)
        {
            var uri = new Uri(url, UriKind.Absolute);

            // you can check host here => uri.Host <= "www.youtube.com"

            var query = HttpUtility.ParseQueryString(uri.Query);

            var videoId = string.Empty;

            if (query.AllKeys.Contains("list"))
            {
                videoId = query["list"];
            }
            else
            {
                videoId = uri.Segments.Last();
            }

            return videoId;
        }

    }
}
