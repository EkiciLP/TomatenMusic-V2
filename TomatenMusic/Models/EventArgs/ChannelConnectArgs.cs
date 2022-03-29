using DSharpPlus.Entities;
using Emzi0767.Utilities;

namespace TomatenMusic_Api.Models.EventArgs
{
	public class ChannelConnectArgs : AsyncEventArgs
	{
		public ulong Guild_Id { get; set; }

		public DiscordChannel Channel { get; set; }

		public ChannelConnectArgs(ulong guild_Id, DiscordChannel channel)
		{
			Guild_Id = guild_Id;
			Channel = channel;
		}
	}
}
