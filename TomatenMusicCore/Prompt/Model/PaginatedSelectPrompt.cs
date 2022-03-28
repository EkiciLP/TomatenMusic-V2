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
using System.Linq;

namespace TomatenMusic.Prompt.Model
{
    abstract class PaginatedSelectPrompt<T> : CombinedPrompt
    {
        protected PageManager<T> PageManager { get; set; }
        protected int CurrentPage { get; set; } = 1;

        public string Title { get; set; }
        public List<T> SelectedItems { get; set; } = new List<T>();


        public PaginatedSelectPrompt(string title, List<T> items, DiscordPromptBase lastPrompt = null, List<DiscordEmbed> embeds = null) : base(lastPrompt)
        {
            Embeds = embeds;
            PageManager = new PageManager<T>(items, 10);
            Title = title;
            AddOption(new SelectMenuPromptOption
            {
                Row = 1,
                MinValues = 1,
                MaxValues = PageManager.GetPage(CurrentPage).Count,
                Content = "Select a Value",
                UpdateMethod = async (option) =>
                {
                    SelectMenuPromptOption _option = (SelectMenuPromptOption)option;
                    
                    _option.MaxValues = PageManager.GetPage(CurrentPage).Count;
                    _option.Options.Clear();
                    foreach (var item in PageManager.GetPage(CurrentPage))
                    {
                        _option.Options.Add(await GetOption(item));
                    }
                    foreach (var item in _option.Options)
                    {
                        foreach (var sOption in SelectedItems)
                        {
                            PaginatedSelectMenuOption<T> _item = (PaginatedSelectMenuOption<T>)item;
                            if (_item.Item.Equals(sOption))
                            {
                                _option.CurrentValues.Add(_item.CustomID);
                            }
                        }
                        
                    }

                    return _option;
                }
            });


            AddOption(new ButtonPromptOption
            {
                Style = ButtonStyle.Secondary,
                Emoji = new DiscordComponentEmoji("⬅️"),
                Row = 2,
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
                Row = 2,
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

        private async Task<PaginatedSelectMenuOption<T>> GetOption(T item)
        {
            var option = await ConvertToOption(item);
            option.Item = item;
            option.CustomID = RandomUtil.GenerateGuid();
            option.Default = SelectedItems.Contains(item);
            option.OnSelected = async (args, sender, option) =>
            {
                PaginatedSelectMenuOption<T> _option = (PaginatedSelectMenuOption<T>)option;
                if (!SelectedItems.Contains(_option.Item))
                    SelectedItems.Add(_option.Item);
                await OnSelect(_option.Item, args, sender);
                
            };
            option.OnUnselected = async (args, sender, option) =>
            {
                PaginatedSelectMenuOption<T> _option = (PaginatedSelectMenuOption<T>)option;
                SelectedItems.Remove(_option.Item);
                await OnUnselect(_option.Item, args, sender);
            };


            return option;
        }
        public abstract Task<PaginatedSelectMenuOption<T>> ConvertToOption(T item);

        public abstract Task OnSelect(T item, ComponentInteractionCreateEventArgs args, DiscordClient sender);

        public abstract Task OnUnselect(T item, ComponentInteractionCreateEventArgs args, DiscordClient sender);

        protected int GetTotalPages()
        {
            return PageManager.GetTotalPages();
        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            DiscordEmbedBuilder builder;
            if (Embeds != null)
            {
                builder = new DiscordEmbedBuilder(Embeds[0]);
            }else
            {
                builder = new DiscordEmbedBuilder();
            }

            builder
                .WithTitle(Title)
                .WithFooter($"Page {CurrentPage} of {GetTotalPages()}");

            return PopulateMessage(builder);

        }

        protected abstract DiscordMessageBuilder PopulateMessage(DiscordEmbedBuilder builder);

        public class PaginatedSelectMenuOption<I> : SelectMenuOption
        {
            public I Item { get; set; }
        }
    }
}
