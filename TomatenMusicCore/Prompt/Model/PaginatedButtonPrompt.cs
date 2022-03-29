using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Util;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using TomatenMusic.Prompt.Option;

namespace TomatenMusic.Prompt.Model
{
    abstract class PaginatedButtonPrompt<T> : ButtonPrompt
    {
        protected PageManager<T> PageManager { get; set; }
        protected int CurrentPage { get; set; } = 1;
        public string Title { get; set; }

        public PaginatedButtonPrompt(string title, List<T> items, DiscordPromptBase lastPrompt = null) : base(lastPrompt)
        {
            PageManager = new PageManager<T>(items, 9);
            Title = title;

            for (int i = 0; i < 9; i++)
            {
                int currentNumber = i + 1;
                
                ButtonPromptOption option = new ButtonPromptOption()
                {
                    Style = DSharpPlus.ButtonStyle.Primary,
                    Row = i < 5 ? 1 : 2,
                    UpdateMethod = async (option) =>
                    {
                        option.Disabled = PageManager.GetPage(CurrentPage).Count < currentNumber;
                        return option;
                    },
                    Run = async (args, sender, prompt) =>
                    {
                        List<T> items = PageManager.GetPage(CurrentPage);
                        await OnSelect(items[currentNumber-1], args, sender);
                    }
                };

                switch (i)
                {
                    case 0:
                        option.Emoji = new DiscordComponentEmoji("1️⃣");
                        break;
                    case 1:
                        option.Emoji = new DiscordComponentEmoji("2️⃣");
                        break;
                    case 2:
                        option.Emoji = new DiscordComponentEmoji("3️⃣");
                        break;
                    case 3:
                        option.Emoji = new DiscordComponentEmoji("4️⃣");
                        break;
                    case 4:
                        option.Emoji = new DiscordComponentEmoji("5️⃣");
                        break;
                    case 5:
                        option.Emoji = new DiscordComponentEmoji("6️⃣");
                        break;
                    case 6:
                        option.Emoji = new DiscordComponentEmoji("7️⃣");
                        break;
                    case 7:
                        option.Emoji = new DiscordComponentEmoji("8️⃣");
                        break;
                    case 8:
                        option.Emoji = new DiscordComponentEmoji("9️⃣");
                        break;
                }

                AddOption(option);
            }

            AddOption(new ButtonPromptOption
            {
                Style = ButtonStyle.Secondary,
                Emoji= new DiscordComponentEmoji("⬅️"),
                Row = 3,
                UpdateMethod = async (prompt) =>
                {
                    prompt.Disabled = CurrentPage - 1 == 0;
                    return prompt;
                },
                Run = async (args, sender, prompt) =>
                {
                    CurrentPage--;
                    await UpdateAsync();

                }
            });
            AddOption(new ButtonPromptOption
            {
                Style = ButtonStyle.Secondary,
                Emoji = new DiscordComponentEmoji("➡️"),
                Row = 3,
                UpdateMethod = async (prompt) =>
                {
                    prompt.Disabled = PageManager.GetTotalPages() == CurrentPage;
                    return prompt;
                },
                Run = async (args, sender, prompt) =>
                {
                    CurrentPage++;
                    await UpdateAsync();
                }
            });

        }

        public abstract Task OnSelect(T item, ComponentInteractionCreateEventArgs args, DiscordClient sender);

        protected int GetTotalPages()
        {
            return PageManager.GetTotalPages();
        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle(Title)
                .WithFooter($"Page {CurrentPage} of {GetTotalPages()}")
                .WithDescription("Select your desired Tracks");

            return PopulateMessage(builder);

        }

        protected abstract DiscordMessageBuilder PopulateMessage(DiscordEmbedBuilder builder);



    }
}
