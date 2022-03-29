using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Prompt.Option;
using System.Linq;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using TomatenMusic.Util;


namespace TomatenMusic.Prompt.Model
{
    class SelectPrompt : DiscordPromptBase
    {
        public List<DiscordEmbed> Embeds { get; protected set; } = new List<DiscordEmbed>();
        public string Content { get; protected set; } = "";
        public SelectPrompt(DiscordPromptBase lastPrompt = null, string content = " Example", List<DiscordEmbed> embeds = null) : base(lastPrompt)
        {

            this.Content = content;
            this.Embeds = embeds == null ? new List<DiscordEmbed>() : embeds;
        }

        protected async override Task<DiscordComponent> GetComponentAsync(IPromptOption option)
        {

            SelectMenuPromptOption selectOption = (SelectMenuPromptOption)option;
            List<DiscordSelectComponentOption> options = new List<DiscordSelectComponentOption>();
            foreach ( var item in selectOption.Options)
            {
                options.Add(new DiscordSelectComponentOption(item.Label, item.CustomID, item.Description, item.Default, item.Emoji));
            }

                return new DiscordSelectComponent(selectOption.CustomID, selectOption.Content, options, selectOption.Disabled, selectOption.MinValues, selectOption.MaxValues);
        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            return new DiscordMessageBuilder()
                .WithContent(Content)
                .AddEmbeds(Embeds);
        }

    }
}
