using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.EventArgs;
using DSharpPlus;
using TomatenMusic.Music;
using Emzi0767.Utilities;
using Lavalink4NET;
using Microsoft.Extensions.DependencyInjection;

namespace TomatenMusic.Commands.Checks
{
    public class UserInMusicChannelCheck : SlashCheckBaseAttribute
    {
        public bool _passIfNull { get; set; }
        public UserInMusicChannelCheck(bool passIfNull = false)
        {
            _passIfNull = passIfNull;
        }
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            IAudioService audioService = TomatenMusicBot.ServiceProvider.GetRequiredService<IAudioService>();

            GuildPlayer player = audioService.GetPlayer<GuildPlayer>(ctx.Guild.Id);
            bool allowed;
            //TODO
            if (player != null)
            {
                allowed = ctx.Member.VoiceState.Channel != null && ctx.Member.VoiceState.Channel.Id == player.VoiceChannelId;
            }
            else
                allowed = _passIfNull;

            if (!allowed)
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("❌ Please connect to the Bots Channel to use this Command").AsEphemeral(true));
            return allowed;
        }
    }
}
