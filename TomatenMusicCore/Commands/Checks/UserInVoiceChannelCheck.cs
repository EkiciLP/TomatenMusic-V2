using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using TomatenMusic.Music;

namespace TomatenMusic.Commands.Checks
{
    class UserInVoiceChannelCheck : SlashCheckBaseAttribute
    {

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("You are not in a Voice Channel.").AsEphemeral(true));
                return false;
            }

            return true;
            
        }
    }
}
