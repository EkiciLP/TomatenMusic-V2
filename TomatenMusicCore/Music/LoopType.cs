using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.SlashCommands;

namespace TomatenMusic.Music
{
    public enum LoopType
    {
        [ChoiceName("Track")]
        TRACK,
        [ChoiceName("Queue")]
        QUEUE,
        [ChoiceName("None")]
        NONE
    }
}
