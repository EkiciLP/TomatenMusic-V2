using Emzi0767.Utilities;

namespace TomatenMusic_Api.Models.EventArgs
{
    public class ChannelDisconnectArgs : AsyncEventArgs
    {
        public ulong GuildId { get; set; }

        public ChannelDisconnectArgs(ulong guildId) { GuildId = guildId; }
    }

    
}
