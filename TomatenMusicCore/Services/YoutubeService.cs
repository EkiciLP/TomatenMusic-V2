using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music.Entitites;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Logging;
using static TomatenMusic.TomatenMusicBot;
using Lavalink4NET.Player;
using Microsoft.Extensions.DependencyInjection;
using Lavalink4NET;
using TomatenMusicCore.Music;

namespace TomatenMusic.Services
{
    public class YoutubeService
    {
        public YouTubeService Service { get; }
        public ILogger<YoutubeService> _logger { get; set; }
        public YoutubeService(ILogger<YoutubeService> logger, ConfigJson config)
        {
            Service = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = config.YoutubeAPIKey,
                ApplicationName = "TomatenMusic"
            });
            _logger = logger;
        }

        public async Task<LavalinkTrack> PopulateTrackInfoAsync(LavalinkTrack track)
        {
            var video = await GetVideoAsync(track.TrackIdentifier);
            var channel = await GetChannelAsync(video.Snippet.ChannelId);
            FullTrackContext context = track.Context == null ? new FullTrackContext() : (FullTrackContext)track.Context;

            if (channel.Statistics.SubscriberCount != null)
                context.YoutubeAuthorSubs = (ulong) channel.Statistics.SubscriberCount;
            context.YoutubeAuthorThumbnail = new Uri(channel.Snippet.Thumbnails.Default__.Url);
            context.YoutubeAuthorUri = new Uri($"https://www.youtube.com/channel/{channel.Id}");
            string desc = video.Snippet.Description;

            context.YoutubeDescription = desc.Substring(0, Math.Min(desc.Length, 1024)) + (desc.Length > 1020 ? "..." : " ");
            if (video.Statistics.LikeCount != null)
                context.YoutubeLikes = (ulong) video.Statistics.LikeCount;
            context.YoutubeTags = video.Snippet.Tags;

            try
            {
                context.YoutubeThumbnail = new Uri(video.Snippet.Thumbnails.High.Url);
            }catch (Exception ex) { }

            context.YoutubeUploadDate = (DateTime)video.Snippet.PublishedAt;
            context.YoutubeViews = (ulong)video.Statistics.ViewCount;
            context.YoutubeCommentCount = video.Statistics.CommentCount;
            track.Context = context;
            return track;
        }

        public async Task<List<LavalinkTrack>> PopulateTrackListAsync(IEnumerable<LavalinkTrack> tracks)
        {
            List<LavalinkTrack> newTracks = new List<LavalinkTrack>();
            foreach (var track in tracks)
                newTracks.Add(await PopulateTrackInfoAsync(track));

            return newTracks;
        }
        public async Task<ILavalinkPlaylist> PopulatePlaylistAsync(YoutubePlaylist playlist)
        {
            var list = await GetPlaylistAsync(playlist.Identifier);
            var channel = await GetChannelAsync(list.Snippet.ChannelId);

            string desc = list.Snippet.Description;

            playlist.Description = desc.Substring(0, Math.Min(desc.Length, 1024)) + (desc.Length > 1020 ? "..." : " ");
            if (playlist.Description.Length < 2)
                playlist.Description = "None";

            try
            {
                playlist.Thumbnail = new Uri(list.Snippet.Thumbnails.Maxres.Url);
            }catch (Exception ex) { }
            playlist.AuthorName = channel.Snippet.Title;
            playlist.CreationTime = (DateTime)list.Snippet.PublishedAt;
            playlist.YoutubeItem = list;
            playlist.AuthorThumbnail = new Uri(channel.Snippet.Thumbnails.Default__.Url);
            playlist.AuthorUri = new Uri($"https://www.youtube.com/channels/{channel.Id}");

            return playlist;
        }

        public async Task<Video> GetVideoAsync(string id)
        {
            var search = Service.Videos.List("contentDetails,id,liveStreamingDetails,localizations,player,recordingDetails,snippet,statistics,status,topicDetails");
            search.Id = id;
            var response = await search.ExecuteAsync();
            return response.Items.First();
        }

        public async Task<Channel> GetChannelAsync(string id)
        {
            var search = Service.Channels.List("brandingSettings,contentDetails,contentOwnerDetails,id,localizations,snippet,statistics,status,topicDetails");
            search.Id = id;
            var response = await search.ExecuteAsync();

            return response.Items.First();
        }
        public async Task<Playlist> GetPlaylistAsync(string id)
        {
            var search = Service.Playlists.List("snippet,contentDetails,status");
            search.Id = id;
            var response = await search.ExecuteAsync();

            return response.Items.First();
        }

        public async Task<IEnumerable<SearchResult>> GetRelatedVideosAsync(string id)
        {
            var search = Service.Search.List("snippet");
            search.RelatedToVideoId = id;
            search.Type = "video";
            var response = await search.ExecuteAsync();
            return response.Items.Where(x => x.Snippet != null);
        }

        public async Task<LavalinkTrack> GetRelatedTrackAsync(string id, List<string> excludeIds)
        {
            var audioService = TomatenMusicBot.ServiceProvider.GetRequiredService<IAudioService>();

            var videos = await GetRelatedVideosAsync(id);
            SearchResult video = null;
            foreach (var vid in videos)
                video = videos.First(x => !excludeIds.Contains(x.Id.VideoId));
            
            if (video == null)
                video = videos.FirstOrDefault();

            var loadResult = new TomatenMusicTrack(await audioService.GetTrackAsync($"https://youtu.be/{video.Id.VideoId}"));

            if (loadResult == null)
                throw new Exception("An Error occurred while processing the Request");

            return await FullTrackContext.PopulateAsync(loadResult);
        }

    }
}
