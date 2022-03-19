using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using TomatenMusic.Music;

namespace TomatenMusic.Commands.Checks
{
    public class OnlyGuildCheck : SlashCheckBaseAttribute
    {
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (ctx.Guild == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("This Command is only available on Guilds.").AsEphemeral(true));
                return false;
            }

            return true;
        }
    }
}
