using DSharpPlus.Entities;
using Emzi0767.Utilities;
using Microsoft.AspNetCore.Mvc;
using TomatenMusic_Api.Models;
using TomatenMusic_Api.Models.EventArgs;

namespace TomatenMusic_Api;

public class InProcessEventBus
{
	public event AsyncEventHandler<InProcessEventBus, ChannelConnectArgs>? OnConnectRequest;

	public event AsyncEventHandler<InProcessEventBus, ChannelDisconnectArgs>? OnDisconnectRequest;


	public void OnConnectRequestEvent(ChannelConnectArgs e)
	{
		_ = OnConnectRequest?.Invoke(this, e);
	}

	public void OnDisconnectRequestEvent(ChannelDisconnectArgs e)
	{
		_ = OnDisconnectRequest?.Invoke(this, e);
	}
}

