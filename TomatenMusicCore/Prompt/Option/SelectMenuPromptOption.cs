using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TomatenMusic.Prompt.Model;
using System.Linq;


namespace TomatenMusic.Prompt.Option
{
    class SelectMenuPromptOption : IPromptOption
    {
        public string Content { get; set; } = " ";
        public string CustomID { get; set; }
        public int Row { get; set; } = 1;
        public bool Disabled { get; set; } = false;
        public List<SelectMenuOption> Options { get; set; } = new List<SelectMenuOption>();
        public int MinValues { get; set; } = 1;
        public int MaxValues { get; set; } = 1;
        public List<string> CurrentValues { get; set; } = new List<string>();


        public Func<IPromptOption, Task<IPromptOption>> UpdateMethod { get; set; } = async (prompt) =>
        {
            return prompt;
        };
        public Func<ComponentInteractionCreateEventArgs, DiscordClient, IPromptOption, Task> Run { get; set; } = async (args, sender, option) =>
        {
            SelectMenuPromptOption _option = (SelectMenuPromptOption)option;
            foreach (var item in _option.Options)
            {
                if (_option.CurrentValues.Contains(item.CustomID) && !args.Values.Contains(item.CustomID))
                {
                    await item.OnUnselected.Invoke(args, sender, item);
                }
                if (!_option.CurrentValues.Contains(item.CustomID) && args.Values.Contains(item.CustomID))
                {
                    await item.OnSelected.Invoke(args, sender, item);
                }
            }
            _option.CurrentValues = new List<string>(args.Values);

        };

    }
}
