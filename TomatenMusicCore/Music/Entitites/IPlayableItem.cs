using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomatenMusic.Music;

namespace TomatenMusicCore.Music.Entities
{
    public interface IPlayableItem
    {
        public string Title { get; }
        Task Play(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool noReplace = true);
        Task PlayNow(GuildPlayer player, TimeSpan? startTime = null, TimeSpan? endTime = null, bool withoutQueuePrepend = false);

    }
}
