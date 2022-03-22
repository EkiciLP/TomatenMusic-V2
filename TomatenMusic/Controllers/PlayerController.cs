using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;
using TomatenMusic;
using TomatenMusic_Api;
using TomatenMusic_Api.Auth.Helpers;
using TomatenMusic_Api.Models;
using TomatenMusic_Api.Models.EventArgs;
using static TomatenMusic_Api.InProcessEventBus;

namespace TomatenMusic_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayerController : ControllerBase
{


	private readonly ILogger<PlayerController> _logger;
	private readonly InProcessEventBus _eventBus;
	private readonly TomatenMusicDataService _tomatenMusicDataService;

	public PlayerController(
		ILogger<PlayerController> logger,
		InProcessEventBus eventBus, TomatenMusicDataService dataService)
		
	{
		_logger = logger;
		_eventBus = eventBus;
		_tomatenMusicDataService = dataService;
	}

	[HttpGet("{guild_id}")]
	public async Task<IActionResult> Get(ulong guild_Id)
	{
        Models.PlayerConnectionInfo response = await _tomatenMusicDataService.GetConnectionInfoAsync(guild_Id);

		if (response == null)
        {
			return BadRequest("The Bot is not connected or the guild is unknown");
        }

		return Ok(response);
	}
	[HttpGet]
	public async Task<IActionResult> Get()
    {
        List<Models.PlayerConnectionInfo> response = await _tomatenMusicDataService.GetAllGuildPlayersAsync();

		if (response == null)
        {
			return BadRequest("An Error occured while parsing the Guilds, Guilds were Empty");
        }

		return Ok(response);
    }

	[HttpPost("connect")]
	public async Task<IActionResult> PostConnect(ChannelConnectRequest request)
	{

		try
		{
			await _tomatenMusicDataService.GetGuildAsync(request.Guild_Id);
		}catch (Exception ex)
		{
			return NotFound("That Guild was not found");
		}


		Boolean? playing = await _tomatenMusicDataService.IsPlayingAsync(request.Guild_Id);

		DiscordChannel channel;

		if (playing == true)
			return BadRequest("The Bot is already playing");

		if (await _tomatenMusicDataService.IsConnectedAsync(request.Guild_Id) == true)
			return BadRequest("The Bot is already connected");

		try
        {
			channel = await _tomatenMusicDataService.GetDiscordChannelAsync(request.Guild_Id, request.Channel_Id);
		}catch (Exception ex)
        {
			return NotFound("Channel was not Found");
		}



		_eventBus.OnConnectRequestEvent(new ChannelConnectArgs(request.Guild_Id, channel));

		return Ok();
	}

	[HttpPost("disconnect")]
	public async Task<IActionResult> PostDisconnect(ChannelDisconnectRequest request)
    {
		try
		{
			await _tomatenMusicDataService.GetGuildAsync(request.GuildId);
		}
		catch (Exception ex)
		{
			return NotFound("That Guild was not found");
		}

		if (!await _tomatenMusicDataService.IsConnectedAsync(request.GuildId) == true)
			return BadRequest("The Bot is not connected.");

		_eventBus.OnDisconnectRequestEvent(new ChannelDisconnectArgs(request.GuildId));
		return Ok();

	}
}
