using Lavalink4NET;
using Lavalink4NET.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Services;

namespace TomatenMusic.Music
{
    public class TrackProvider
    {
        public ISpotifyService _spotifyService { get; set; }
        public IAudioService _audioService { get; set; }

        public TrackProvider(ISpotifyService spotify, IAudioService audioService)
        {
            _audioService = audioService;
            _spotifyService = spotify;
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


            if (loadResult.LoadType == TrackLoadType.LoadFailed) throw new ArgumentException("Track loading failed");

            if (loadResult.LoadType == TrackLoadType.NoMatches) throw new FileNotFoundException("Query resulted in no Matches");

            if (withSearchResults && loadResult.LoadType == TrackLoadType.SearchResult)
            {
                return new MusicActionResponse(tracks: await FullTrackContext.PopulateTracksAsync(loadResult.Tracks));
            }

            if (loadResult.LoadType == TrackLoadType.PlaylistLoaded && !isSearch)
                return new MusicActionResponse(
                    playlist: new YoutubePlaylist(loadResult.PlaylistInfo.Name, await FullTrackContext.PopulateTracksAsync(loadResult.Tracks), uri));
            else
                return new MusicActionResponse(await FullTrackContext.PopulateAsync(loadResult.Tracks.First()));

        }

        public async Task<MusicActionResponse> SearchAsync(Uri fileUri)
        {

            var loadResult = await _audioService.GetTrackAsync(fileUri.ToString());
            loadResult.Context = new FullTrackContext
            {
                IsFile = true
            };

            if (loadResult == null)
                throw new FileNotFoundException("The file was not found");

            return new MusicActionResponse(loadResult);

        }

    }
}
