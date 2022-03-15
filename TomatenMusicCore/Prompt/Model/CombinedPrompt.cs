using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using TomatenMusic.Prompt.Option;
using System.Linq;
using System.Threading.Tasks;
using TomatenMusic.Util;
using Microsoft.Extensions.Logging;


namespace TomatenMusic.Prompt.Model
{
    class CombinedPrompt : DiscordPromptBase
    {
        public string Content { get; protected set; } = "";
        public List<DiscordEmbed> Embeds { get; protected set; } = new List<DiscordEmbed>();

        public CombinedPrompt(DiscordPromptBase lastPrompt = null, string content = "Example Content", List<DiscordEmbed> embeds = null) : base(lastPrompt)
        {
            this.LastPrompt = lastPrompt;

            this.Content = content;
            this.Embeds = embeds == null ? new List<DiscordEmbed>() : embeds;
        }

        protected async override Task<DiscordComponent> GetComponentAsync(IPromptOption option)
        {
            if (option is SelectMenuPromptOption)
            {
                SelectMenuPromptOption selectOption = (SelectMenuPromptOption)option;
                List<DiscordSelectComponentOption> options = new List<DiscordSelectComponentOption>();
                foreach (var item in selectOption.Options)
                {
                    options.Add(new DiscordSelectComponentOption(item.Label, item.CustomID, item.Description, item.Default, item.Emoji));
                }

                return new DiscordSelectComponent(selectOption.CustomID, selectOption.Content, options, selectOption.Disabled, selectOption.MinValues, selectOption.MaxValues);
            }
            else
            {
                var myOption = (ButtonPromptOption)option;
                DiscordComponent component;

                if (myOption.Link != null)
                    component = new DiscordLinkButtonComponent(myOption.Link, myOption.Content, myOption.Disabled, myOption.Emoji);
                else
                    component = new DiscordButtonComponent(myOption.Style, myOption.CustomID, myOption.Content, myOption.Disabled, myOption.Emoji);
                return component;
            }


        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            return new DiscordMessageBuilder()
            .WithContent(Content)
            .AddEmbeds(Embeds);
        }
    }
}
