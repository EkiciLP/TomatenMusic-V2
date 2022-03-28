using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using TomatenMusic.Prompt.Option;
using System.Linq;
using System.Threading.Tasks;

namespace TomatenMusic.Prompt.Model
{
    class ButtonPrompt : DiscordPromptBase
    {
        public string Content { get; protected set; } = "";
        public List<DiscordEmbed> Embeds { get; protected set; } = new List<DiscordEmbed>();

        public ButtonPrompt(DiscordPromptBase lastPrompt = null, string content = " ", List<DiscordEmbed> embeds = null) : base(lastPrompt)
        {
            this.Content = content;
            this.Embeds = embeds == null ? new List<DiscordEmbed>() : embeds;
        }

        protected override Task<DiscordComponent> GetComponentAsync(IPromptOption option)
        {
            var myOption = (ButtonPromptOption)option;
            DiscordComponent component;

            if (myOption.Link != null)
                component = new DiscordLinkButtonComponent(myOption.Link, myOption.Content, myOption.Disabled, myOption.Emoji);
            else
                component = new DiscordButtonComponent(myOption.Style, myOption.CustomID, myOption.Content, myOption.Disabled, myOption.Emoji);
            return Task.FromResult<DiscordComponent>(component);
        }

        protected override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            return Task.FromResult<DiscordMessageBuilder>(new DiscordMessageBuilder()
            .WithContent(Content)
            .AddEmbeds(Embeds));
        }
    }
}
