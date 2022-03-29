using System;
using System.Collections.Generic;
using System.Text;
using TomatenMusic.Prompt.Model;
using DSharpPlus;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using DSharpPlus.Entities;

namespace TomatenMusic.Prompt.Implementation
{
    class StringSelectorPrompt : PaginatedSelectPrompt<string>
    {
        public StringSelectorPrompt(string title, List<string> strings, DiscordPromptBase lastPrompt = null) : base(title, strings, lastPrompt)
        {
        }
        public async override Task<PaginatedSelectMenuOption<string>> ConvertToOption(string item)
        {
            return new PaginatedSelectMenuOption<string>
            {
                Label = item
            };
        }

        public async override Task OnSelect(string item, ComponentInteractionCreateEventArgs args, DiscordClient sender)
        {
        }

        public async override Task OnUnselect(string item, ComponentInteractionCreateEventArgs args, DiscordClient sender)
        {

        }

        protected override DiscordMessageBuilder PopulateMessage(DiscordEmbedBuilder builder)
        {
            foreach (var item in PageManager.GetPage(CurrentPage))
            {
                builder.AddField(item, item);
            }

            return new DiscordMessageBuilder().WithEmbed(builder);
        }
    }
}
