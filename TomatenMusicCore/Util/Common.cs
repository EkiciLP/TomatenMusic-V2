using DSharpPlus.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TomatenMusic.Music.Entitites;
using TomatenMusic.Util;
using Microsoft.Extensions.Logging;
using TomatenMusic.Music;
using System.Threading.Tasks;
using System.Linq;
using Lavalink4NET.Player;

namespace TomatenMusic.Util
{
    class Common
    {

        public static DiscordEmbed AsEmbed(LavalinkTrack track, LoopType loopType, int position = -1)
        {
            FullTrackContext context = (FullTrackContext)track.Context;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle(track.Title)
                .AddField("Length", Common.GetTimestamp(track.Duration), true);
            if (context.IsFile)
            {
                builder.WithAuthor(track.Author);
            }
            else
                builder
                    .WithAuthor(track.Author, context.YoutubeAuthorUri.ToString(), context.YoutubeAuthorThumbnail.ToString())
                    .WithUrl(context.YoutubeUri)
                    .WithImageUrl(context.YoutubeThumbnail)
                    .WithDescription(context.YoutubeDescription);



            if (position != -1)
            {
                builder.AddField("Position", (position == 0 ? "Now Playing" : position.ToString()), true);
            }
            builder.AddField("Current Queue Loop", loopType.ToString(), true);
            if (!context.IsFile)
            {
                builder.AddField("Views", $"{context.YoutubeViews:N0} Views", true);
                builder.AddField("Rating", $"{context.YoutubeLikes:N0} 👍", true);
                builder.AddField("Upload Date", $"{context.YoutubeUploadDate.ToString("dd. MMMM, yyyy")}", true);
                builder.AddField("Comments", context.YoutubeCommentCount == null ? "Comments Disabled" : $"{context.YoutubeCommentCount:N0} Comments", true);
                builder.AddField("Channel Subscriptions", $"{context.YoutubeAuthorSubs:N0} Subscribers", true);
            }

            return builder;
        }

        public static DiscordEmbed AsEmbed(LavalinkTrack track, int position = -1)
        {
            FullTrackContext context = (FullTrackContext)track.Context;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle(track.Title)
                .WithUrl(context.YoutubeUri)
                .WithImageUrl(context.YoutubeThumbnail)
                .WithDescription(context.YoutubeDescription)
                .AddField("Length", Common.GetTimestamp(track.Duration), true);

            if (context.IsFile)
            {
                builder.WithAuthor(track.Author);
            }
            else
                builder
                    .WithAuthor(track.Author, context.YoutubeAuthorUri.ToString(), context.YoutubeAuthorThumbnail.ToString())
                    .WithUrl(context.YoutubeUri);

            if (position != -1)
            {
                builder.AddField("Position", (position == 0 ? "Now Playing" : position.ToString()), true);
            }

            if (!context.IsFile)
            {
                builder.AddField("Views", $"{context.YoutubeViews:N0} Views", true);
                builder.AddField("Rating", $"{context.YoutubeLikes:N0} 👍", true);
                builder.AddField("Upload Date", $"{context.YoutubeUploadDate.ToString("dd. MMMM, yyyy")}", true);
                builder.AddField("Comments", context.YoutubeCommentCount == null ? "Comments Disabled" : $"{context.YoutubeCommentCount:N0} Comments", true);
                builder.AddField("Channel Subscriptions", $"{context.YoutubeAuthorSubs:N0} Subscribers", true);
            }

            return builder;
        }

        public static DiscordEmbed AsEmbed(ILavalinkPlaylist playlist)
        {

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            if (playlist is YoutubePlaylist)
            {
                YoutubePlaylist youtubePlaylist = (YoutubePlaylist)playlist;
                Console.WriteLine($"{playlist.AuthorName}, {playlist.AuthorUri.ToString()}, {playlist.AuthorThumbnail.ToString()}");
                builder.WithAuthor(playlist.AuthorName, playlist.AuthorUri.ToString(), youtubePlaylist.AuthorThumbnail.ToString());
                builder.WithTitle(playlist.Name);
                builder.WithUrl(playlist.Url);
                builder.WithDescription(TrackListString(playlist.Tracks, 4000));
                builder.WithImageUrl(youtubePlaylist.Thumbnail);
                builder.AddField("Description", playlist.Description, false);
                builder.AddField("Track Count", $"{playlist.Tracks.Count()} Tracks", true);
                builder.AddField("Length", $"{Common.GetTimestamp(playlist.GetLength())}", true);
                builder.AddField("Create Date", $"{youtubePlaylist.CreationTime:dd. MMMM, yyyy}", true);
                
            }else if (playlist is SpotifyPlaylist)
            {
                SpotifyPlaylist spotifyPlaylist = (SpotifyPlaylist)playlist;

                builder.WithTitle(playlist.Name);
                builder.WithUrl(playlist.Url);
                builder.WithDescription(TrackListString(playlist.Tracks, 4000));
                builder.AddField("Description", playlist.Description, false);
                builder.AddField("Track Count", $"{playlist.Tracks.Count()} Tracks", true);
                builder.AddField("Length", $"{Common.GetTimestamp(playlist.GetLength())}", true);
                builder.AddField("Spotify Followers", $"{spotifyPlaylist.Followers:N0}", true);
                if (spotifyPlaylist.AuthorThumbnail != null)
                {
                    builder.WithAuthor(playlist.AuthorName, playlist.AuthorUri.ToString(), spotifyPlaylist.AuthorThumbnail.ToString());
                }else
                    builder.WithAuthor(playlist.AuthorName, playlist.AuthorUri.ToString());
            }

            return builder.Build();
        }

        public static DiscordEmbed GetQueueEmbed(GuildPlayer player)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            builder.WithDescription(TrackListString(player.PlayerQueue.Queue, 4000));
            builder.WithTitle("Current Queue");
            builder.WithAuthor($"{player.PlayerQueue.Queue.Count} Songs");

            TimeSpan timeSpan = TimeSpan.FromTicks(0);

            foreach (var track in player.PlayerQueue.Queue)
            {
                timeSpan = timeSpan.Add(track.Duration);
            }

            builder.AddField("Length", GetTimestamp(timeSpan), true);
            builder.AddField("Loop Type", player.PlayerQueue.LoopType.ToString(), true);
            builder.AddField("Autoplay", player.Autoplay ? "✅" : "❌", true);
            if (player.PlayerQueue.CurrentPlaylist != null)
                builder.AddField("Current Playlist", $"[{player.PlayerQueue.CurrentPlaylist.Name}]({player.PlayerQueue.CurrentPlaylist.Url})", true);

            if (player.PlayerQueue.PlayedTracks.Any())
                builder.AddField("History", TrackListString(player.PlayerQueue.PlayedTracks, 1000), true);

            return builder;
        }

        public static string TrackListString(IEnumerable<LavalinkTrack> tracks, int maxCharacters)
        {
            StringBuilder builder = new StringBuilder();
            string lastString = " ";
            int count = 1;
            foreach (LavalinkTrack track in tracks)
            {
                if (builder.ToString().Length > maxCharacters)
                {
                    builder = new StringBuilder(lastString);
                    builder.Append(String.Format("***And {0} more...***", tracks.Count() - count));
                    break;
                }

                FullTrackContext context = (FullTrackContext)track.Context;

                lastString = builder.ToString();
                builder.Append(count).Append(": ").Append($"[{track.Title}]({context.YoutubeUri})").Append(" [").Append(Common.GetTimestamp(track.Duration)).Append("] | ");
                builder.Append($"[{track.Author}]({context.YoutubeAuthorUri})").Append("\n\n");
                count++;
            }
            builder.Append(" ");
            return builder.ToString();
        }

        public static string GetTimestamp(TimeSpan timeSpan)
        {
            if (timeSpan.Hours > 0)
                return String.Format("{0:hh\\:mm\\:ss}", timeSpan);
            else
                return String.Format("{0:mm\\:ss}", timeSpan);
        }

        public static TimeSpan ToTimeSpan(string text)
        {
            string[] input = text.Split(" ");
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(0);

            foreach (var item in input)
            {
                var l = item.Length - 1;
                var value = item.Substring(0, l);
                var type = item.Substring(l, 1);

                switch (type)
                {
                    case "d": 
                        timeSpan = timeSpan.Add(TimeSpan.FromDays(double.Parse(value)));
                        break;
                    case "h":
                        timeSpan = timeSpan.Add(TimeSpan.FromHours(double.Parse(value))); 
                        break;
                    case "m":
                        timeSpan = timeSpan.Add(TimeSpan.FromMinutes(double.Parse(value)));
                        break;
                    case "s":
                        timeSpan = timeSpan.Add(TimeSpan.FromSeconds(double.Parse(value)));
                        break;
                    case "f":
                        timeSpan = timeSpan.Add(TimeSpan.FromMilliseconds(double.Parse(value))); 
                        break;
                    case "z":
                        timeSpan = timeSpan.Add(TimeSpan.FromTicks(long.Parse(value)));
                        break;
                    default:
                        timeSpan = timeSpan.Add(TimeSpan.FromDays(double.Parse(value)));
                        break;
                }
            }

            return timeSpan;
        }

       public static string ProgressBar(int current, int max)
        {
            int percentage = (current * 100) / max;
            int rounded = (int) Math.Round(((double) percentage / 10));

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i <= 10; i++)
            {
                if (i == rounded)
                    builder.Append("🔘");
                else
                    builder.Append("─");
            }

            return builder.ToString();
        }

        public async static Task<DiscordEmbed> CurrentSongEmbedAsync(GuildPlayer player)
        {

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            LavalinkTrack track = player.CurrentTrack;

            if (track == null)
            {
                builder.WithColor(DiscordColor.Red);
                builder.WithTitle("Nothing Playing");
                builder.WithImageUrl("https://media.tomatentum.net/TMBanner.gif");
                return builder;
            }

            FullTrackContext context = (FullTrackContext)track.Context;

            string progressBar = $"|{ProgressBar((int)player.Position.Position.TotalSeconds, (int)track.Duration.TotalSeconds)}|\n [{Common.GetTimestamp(player.Position.Position)}/{Common.GetTimestamp(track.Duration)}]";
            
            builder.WithAuthor(track.Author);
            builder.WithTitle(track.Title);
            builder.WithUrl(track.Source);
            builder.WithColor(player.State == PlayerState.Paused ? DiscordColor.Orange : DiscordColor.Green);
            builder.AddField("Length", Common.GetTimestamp(track.Duration), true);
            builder.AddField("Loop", player.PlayerQueue.LoopType.ToString(), true);
            builder.AddField("Progress", progressBar, true);
            if (!context.IsFile)
            {
                builder.WithAuthor(track.Author, context.YoutubeAuthorUri.ToString(), context.YoutubeAuthorThumbnail.ToString());
                builder.WithImageUrl(context.YoutubeThumbnail);
                builder.AddField("Views", $"{context.YoutubeViews:N0} Views", true);
                builder.AddField("Rating", $"{context.YoutubeLikes:N0} 👍", true);
                builder.AddField("Upload Date", $"{context.YoutubeUploadDate.ToString("dd. MMMM, yyyy")}", true);
                builder.AddField("Comments", $"{context.YoutubeCommentCount:N0} Comments", true);
                builder.AddField("Channel Subscriptions", $"{context.YoutubeAuthorSubs:N0} Subscribers", true);

            }


            return builder;
        }
    }
}
