using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using TomatenMusic.Prompt.Option;
using TomatenMusic.Prompt.Model;


namespace TomatenMusic.Prompt
{
    class ButtonPromptOption : IPromptOption
    {

        public ButtonStyle Style { get; set; } = ButtonStyle.Primary;
        public string Content { get; set; } = " ";
        public DiscordComponentEmoji Emoji { get; set; }
        public bool Disabled { get; set; } = false;
        public string CustomID { get; set; }
        public string Link { get; set; }
        public int Row { get; set; }
        public Func<Option.IPromptOption, Task<Option.IPromptOption>> UpdateMethod { get; set; } = async prompt =>
        {
            return prompt;
        };
        public Func<DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs, DiscordClient, IPromptOption, Task> Run { get; set; } = async (args, sender, prompt) =>
        {

        };
    }
}
