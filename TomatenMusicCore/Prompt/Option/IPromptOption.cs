using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using TomatenMusic.Prompt.Option;
using TomatenMusic.Prompt.Model;


namespace TomatenMusic.Prompt.Option
{
    interface IPromptOption
    {
        public string Content { get; set; }
        public string CustomID { get; set; }
        public int Row { get; set; }
        public bool Disabled { get; set; }
        public Func<IPromptOption, Task<IPromptOption>> UpdateMethod { get; set; }
        public Func<DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs, DiscordClient, IPromptOption, Task> Run { get; set; }




    }
}
