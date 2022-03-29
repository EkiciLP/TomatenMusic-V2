using DSharpPlus.Entities;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic;
using TomatenMusic.Music;
using TomatenMusic.Prompt;
using TomatenMusic.Prompt.Option;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusicCore.Prompt.Buttons
{
    class PlayNowButton : ButtonPromptOption
    {
        public TrackList Tracks { get; set; }

        public PlayNowButton(TrackList tracks, int row, DiscordMember requestMember)
        {
            Tracks = tracks;
            Emoji = new DiscordComponentEmoji("▶");
            Content = "Now";
            Row = row;
            Style = DSharpPlus.ButtonStyle.Secondary;
            UpdateMethod = (prompt) =>
            {
                if (requestMember.VoiceState == null || requestMember.VoiceState.Channel == null)
                    prompt.Disabled = true;

                return Task.FromResult(prompt);
            };
            Run = async (args, sender, option) =>
            {
                IAudioService audioService = TomatenMusicBot.ServiceProvider.GetRequiredService<IAudioService>();
                GuildPlayer player;

                try
                {
                    try
                    {
                        player = await audioService.JoinAsync<GuildPlayer>(args.Guild.Id, ((DiscordMember)args.User).VoiceState.Channel.Id, true);

                    }
                    catch (Exception ex)
                    {
                        player = audioService.GetPlayer<GuildPlayer>(args.Guild.Id);
                    }
                    await player.PlayNowAsync(Tracks);
                }
                catch (Exception ex)
                {

                }
            };
        }
    }
}
