using DSharpPlus.Entities;
using Emzi0767.Utilities;
using Microsoft.AspNetCore.Mvc;
using TomatenMusic_Api.Models;

namespace TomatenMusic_Api;

public class InProcessEventBus
{
	public event AsyncEventHandler<InProcessEventBus, ChannelConnectEventArgs>? OnConnectRequest;

	public void OnConnectRequestEvent(ChannelConnectEventArgs e)
	{
		_ = OnConnectRequest?.Invoke(this, e);
	}

	public class ChannelConnectEventArgs : AsyncEventArgs
    {
		public ulong Guild_Id { get; set; }

		public DiscordChannel Channel { get; set; }

        public ChannelConnectEventArgs(ulong guild_Id, DiscordChannel channel)
        {
			Guild_Id = guild_Id;
			Channel = channel;
        }
	}
}

