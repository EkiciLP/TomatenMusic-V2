using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music;
using TomatenMusic.Music.Entitites;
using Microsoft.Extensions.DependencyInjection;
using TomatenMusicCore.Music;
using TomatenMusicCore.Music.Entities;

namespace TomatenMusic.Prompt.Buttons
{
    class AddToQueueButton : ButtonPromptOption
    {
        public TrackList Tracks { get; set; }

        public AddToQueueButton(TrackList tracks, int row, DiscordMember requestMember)
        {
            Tracks = tracks;
            Emoji = new DiscordComponentEmoji("▶️");
                Row = row;
                Style = DSharpPlus.ButtonStyle.Primary;
                UpdateMethod = (prompt) =>
                {
                    if (requestMember.VoiceState == null || requestMember.VoiceState.Channel == null)
                        prompt.Disabled = true;

                    return Task.FromResult(prompt);
                };
            Run = async (args, sender, option) =>
            {
                IAudioService audioService = TomatenMusicBot.ServiceProvider.GetRequiredService<IAudioService>();
                GuildPlayer player;player = audioService.GetPlayer<GuildPlayer>(args.Guild.Id);

                try
                {
                    try
                    {
                        player = await audioService.JoinAsync<GuildPlayer>(args.Guild.Id, ((DiscordMember)args.User).VoiceState.Channel.Id, true);

                    }catch (Exception ex)
                    {
                        player = audioService.GetPlayer<GuildPlayer>(args.Guild.Id);
                    }
                    await player.PlayItemAsync(Tracks);
                }
                catch (Exception ex)
                {

                }
            };
        }
    }
}
