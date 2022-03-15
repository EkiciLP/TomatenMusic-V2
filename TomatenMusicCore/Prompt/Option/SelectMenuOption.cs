using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Prompt.Model;
using DSharpPlus.Entities;

namespace TomatenMusic.Prompt.Option
{
    class SelectMenuOption
    {
        public string Label { get; set; }
        public string CustomID { get; set; }
        public string Description { get; set; }
        public bool Default { get; set; }
        public DiscordComponentEmoji Emoji { get; set; }
        public Func<ComponentInteractionCreateEventArgs, DiscordClient, SelectMenuOption, Task> OnSelected { get; set; }
        public Func<ComponentInteractionCreateEventArgs, DiscordClient, SelectMenuOption, Task> OnUnselected { get; set; }

    }
}
