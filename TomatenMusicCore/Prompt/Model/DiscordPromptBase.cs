using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using TomatenMusic.Prompt.Option;
using TomatenMusic.Util;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus;

namespace TomatenMusic.Prompt.Model
{
    abstract class DiscordPromptBase
    {
        public static List<DiscordPromptBase> ActivePrompts { get; } = new List<DiscordPromptBase>();

        public PromptState State { get; protected set; }
        public DiscordMessage Message { get; private set; }
        public DiscordInteraction Interaction { get; private set; }
        public List<IPromptOption> Options { get; protected set; } = new List<IPromptOption>();
        public DiscordClient _client { get; set; }
        public DiscordPromptBase LastPrompt { get; protected set; }
        public System.Timers.Timer TimeoutTimer { get; set; }

        protected ILogger<DiscordPromptBase> _logger { get; set; }

        protected EventId eventId = new EventId(16, "Prompts");

        protected DiscordPromptBase(DiscordPromptBase lastPrompt)
        {
            LastPrompt = lastPrompt;
            Options = new List<IPromptOption>();
            IServiceProvider serviceProvider = TomatenMusicBot.ServiceProvider;

            _logger = serviceProvider.GetRequiredService<ILogger<DiscordPromptBase>>();
 

            if (lastPrompt != null)
            {
                Options.Add(new ButtonPromptOption
                {
                    Style = DSharpPlus.ButtonStyle.Danger,
                    Row = 5,
                    Emoji = new DiscordComponentEmoji("↩️"),
                    Run = async (args, sender, option) =>
                    {
                        _ = BackAsync();
                    }
                });
            }

            Options.Add(new ButtonPromptOption
            {
                Style = DSharpPlus.ButtonStyle.Danger,
                Row = 5,
                Emoji = new DiscordComponentEmoji("❌"),
                Run = async (args, sender, option) =>
                {
                    _ = InvalidateAsync();
                }
            });

        }

        public async Task InvalidateAsync(bool withEdit = true, bool destroyHistory = false)
        {
            foreach (var option in Options)
                option.UpdateMethod = (prompt) =>
                {
                    prompt.Disabled = true;
                    return Task.FromResult<IPromptOption>(prompt);
                };

            if (withEdit)
                await EditMessageAsync(new DiscordWebhookBuilder().WithContent("This Prompt is invalid!"));
            ActivePrompts.Remove(this);
            if (destroyHistory)
            {
                if (LastPrompt != null)
                    await LastPrompt.InvalidateAsync(false);
                await EditMessageAsync(new DiscordWebhookBuilder().WithContent("This Prompt is invalid!"));
            }

            if (State == PromptState.INVALID)
                return;
            State = PromptState.INVALID;
            

            _client.ComponentInteractionCreated -= Discord_ComponentInteractionCreated;

        }

        public async Task SendAsync(DiscordChannel channel)
        {
            if (State == PromptState.INVALID)
                return;

            IServiceProvider serviceProvider = TomatenMusicBot.ServiceProvider;
            var client = serviceProvider.GetRequiredService<DiscordShardedClient>();
            _client = client.GetShard( (ulong) channel.GuildId);

            _client.ComponentInteractionCreated += Discord_ComponentInteractionCreated;
            ActivePrompts.Add(this);
            AddGuids();
            DiscordMessageBuilder builder = await GetMessageAsync();
            builder = await AddComponentsAsync(builder);


            Message = await builder.SendAsync(channel);
            State = PromptState.OPEN;
        }

        public async Task SendAsync(DiscordInteraction interaction, bool ephemeral = false)
        {
            if (State == PromptState.INVALID)
                return;

            IServiceProvider serviceProvider = TomatenMusicBot.ServiceProvider;
            var client = serviceProvider.GetRequiredService<DiscordShardedClient>();
            _client = client.GetShard((ulong)interaction.GuildId);

            _client.ComponentInteractionCreated += Discord_ComponentInteractionCreated;
            ActivePrompts.Add(this);

            AddGuids();
            DiscordFollowupMessageBuilder builder = await GetFollowupMessageAsync();
            builder = await AddComponentsAsync(builder);
            builder.AsEphemeral(ephemeral);

            Interaction = interaction;
            Message = await interaction.CreateFollowupMessageAsync(builder);
            State = PromptState.OPEN;

            long timeoutTime = (Interaction.CreationTimestamp.ToUnixTimeMilliseconds() + 900000) - DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (TimeoutTimer != null)
                TimeoutTimer.Close();

            TimeoutTimer = new System.Timers.Timer(timeoutTime);
            TimeoutTimer.Elapsed += OnTimeout;
            TimeoutTimer.AutoReset = false;
            TimeoutTimer.Start();
        }

        public async Task UseAsync(DiscordMessage message)
        {
            if (State == PromptState.INVALID)
                return;

            IServiceProvider serviceProvider = TomatenMusicBot.ServiceProvider;
            var client = serviceProvider.GetRequiredService<DiscordShardedClient>();
            _client = client.GetShard((ulong)message.Channel.GuildId);

            _client.ComponentInteractionCreated += Discord_ComponentInteractionCreated;
            ActivePrompts.Add(this);

            AddGuids();
            DiscordWebhookBuilder builder = await GetWebhookMessageAsync();

            await EditMessageAsync(builder);
            State = PromptState.OPEN;

        }

        public async Task UseAsync(DiscordInteraction interaction, DiscordMessage message)
        {
            if (State == PromptState.INVALID)
                return;

            IServiceProvider serviceProvider = TomatenMusicBot.ServiceProvider;
            var client = serviceProvider.GetRequiredService<DiscordShardedClient>();
            _client = client.GetShard((ulong)interaction.GuildId);

            _client.ComponentInteractionCreated += Discord_ComponentInteractionCreated;
            ActivePrompts.Add(this);
            AddGuids();
            DiscordWebhookBuilder builder = await GetWebhookMessageAsync();
            Interaction = interaction;
            Message = message;
            await EditMessageAsync(builder);
            State = PromptState.OPEN;

            long timeoutTime = (Interaction.CreationTimestamp.ToUnixTimeMilliseconds() + 900000) - DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (TimeoutTimer != null)
                TimeoutTimer.Close();

            TimeoutTimer = new System.Timers.Timer(timeoutTime);
            TimeoutTimer.Elapsed += OnTimeout;
            TimeoutTimer.AutoReset = false;
            TimeoutTimer.Start();
            
        }

        private void OnTimeout(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _ = InvalidateAsync();
        }

        private void AddGuids()
        {
            foreach (var item in Options)
            {
                item.CustomID = RandomUtil.GenerateGuid();
                if (item is SelectMenuPromptOption)
                {
                    SelectMenuPromptOption menuItem = (SelectMenuPromptOption)item;
                    foreach (var option in menuItem.Options)
                    {
                        option.CustomID = RandomUtil.GenerateGuid();
                    }
                }

            }
            //this.Options = options;
        }

        protected abstract Task<DiscordComponent> GetComponentAsync(IPromptOption option);

        protected abstract Task<DiscordMessageBuilder> GetMessageAsync();

        private async Task<DiscordFollowupMessageBuilder> GetFollowupMessageAsync()
        {
            DiscordMessageBuilder oldBuilder = await GetMessageAsync();

            return new DiscordFollowupMessageBuilder()
                .WithContent(oldBuilder.Content)
                .AddEmbeds(oldBuilder.Embeds);
                
        }
        private async Task<DiscordWebhookBuilder> GetWebhookMessageAsync()
        {
            DiscordMessageBuilder oldBuilder = await GetMessageAsync();

            return new DiscordWebhookBuilder()
                .WithContent(oldBuilder.Content)
                .AddEmbeds(oldBuilder.Embeds);

        }

        public async Task UpdateAsync()
        {
           if (State == PromptState.INVALID)
                return;
           await EditMessageAsync(await GetWebhookMessageAsync());
           
        }

        private async Task UpdateOptionsAsync()
        {
            List<IPromptOption> options = new List<IPromptOption>();
            foreach (var option in this.Options)
                options.Add(await option.UpdateMethod.Invoke(option));
            this.Options = options;
        }

        protected async Task Discord_ComponentInteractionCreated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            if (State == PromptState.INVALID)
                return;

            foreach (var option in Options)
            {
                if (option.CustomID == e.Id)
                {

                    await e.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate);
                    _ = option.Run.Invoke(e, sender, option);
                }
            }
        }

        public async Task EditMessageAsync(DiscordWebhookBuilder builder)
        {
            try
            {
                if (Interaction != null)
                {
                    await AddComponentsAsync(builder);
                    try
                    {
                        Message = await Interaction.EditFollowupMessageAsync(Message.Id, builder);
                    }catch (Exception e)
                    {
                        Message = await Interaction.EditOriginalResponseAsync(builder);
                    }

                }
                else
                {
                    DiscordMessageBuilder msgbuilder = new DiscordMessageBuilder()
                        .AddEmbeds(builder.Embeds)
                        .WithContent(builder.Content);
                    await AddComponentsAsync(msgbuilder);
                    Message = await Message.ModifyAsync(msgbuilder);
                }
            }catch (BadRequestException e)
            {
                _logger.LogError(e.Errors);
            }

        }

        protected async Task<DiscordMessageBuilder> AddComponentsAsync(DiscordMessageBuilder builder)
        {
            await UpdateOptionsAsync();
            builder.ClearComponents();

            List<DiscordComponent> row1 = new List<DiscordComponent>(5);
            List<DiscordComponent> row2 = new List<DiscordComponent>(5);
            List<DiscordComponent> row3 = new List<DiscordComponent>(5);
            List<DiscordComponent> row4 = new List<DiscordComponent>(5);
            List<DiscordComponent> row5 = new List<DiscordComponent>(5);

            foreach (var option in Options)
            {
                switch (option.Row)
                {
                    case 1:
                        row1.Add(await GetComponentAsync(option));
                        break;
                    case 2:
                        row2.Add(await GetComponentAsync(option));
                        break;
                    case 3:
                        row3.Add(await GetComponentAsync(option));
                        break;
                    case 4:
                        row4.Add(await GetComponentAsync(option));
                        break;
                    case 5:
                        row5.Add(await GetComponentAsync(option));
                        break;
                    default:
                        throw new ArgumentException("Invalid Row! Must be between 1 and 5", "Row");
                }
            }
            if (row1.Count != 0)
            {
                builder.AddComponents(row1);
            }
            if (row2.Count != 0)
            {
                builder.AddComponents(row2);
            }
            if (row3.Count != 0)
            {
                builder.AddComponents(row3);
            }
            if (row4.Count != 0)
            {
                builder.AddComponents(row4);
            }
            if (row5.Count != 0)
            {
                builder.AddComponents(row5);
            }
            return builder;
        }

        protected async Task<DiscordFollowupMessageBuilder> AddComponentsAsync(DiscordFollowupMessageBuilder builder)
        {
            await UpdateOptionsAsync();
            builder.ClearComponents();

            List<DiscordComponent> row1 = new List<DiscordComponent>(5);
            List<DiscordComponent> row2 = new List<DiscordComponent>(5);
            List<DiscordComponent> row3 = new List<DiscordComponent>(5);
            List<DiscordComponent> row4 = new List<DiscordComponent>(5);
            List<DiscordComponent> row5 = new List<DiscordComponent>(5);

            foreach (var option in Options)
            {
                switch (option.Row)
                {
                    case 1:
                        row1.Add(await GetComponentAsync(option));
                        break;
                    case 2:
                        row2.Add(await GetComponentAsync(option));
                        break;
                    case 3:
                        row3.Add(await GetComponentAsync(option));
                        break;
                    case 4:
                        row4.Add(await GetComponentAsync(option));
                        break;
                    case 5:
                        row5.Add(await GetComponentAsync(option));
                        break;
                    default:
                        throw new ArgumentException("Invalid Row! Must be between 1 and 5", "Row");
                }
            }
            if (row1.Count != 0)
            {
                builder.AddComponents(row1);
            }
            if (row2.Count != 0)
            {
                builder.AddComponents(row2);
            }
            if (row3.Count != 0)
            {
                builder.AddComponents(row3);
            }
            if (row4.Count != 0)
            {
                builder.AddComponents(row4);
            }
            if (row5.Count != 0)
            {
                builder.AddComponents(row5);
            }
            return builder;
        }

        protected async Task<DiscordWebhookBuilder> AddComponentsAsync(DiscordWebhookBuilder builder)
        {
            await UpdateOptionsAsync();
            builder.ClearComponents();

            List<DiscordComponent> row1 = new List<DiscordComponent>(5);
            List<DiscordComponent> row2 = new List<DiscordComponent>(5);
            List<DiscordComponent> row3 = new List<DiscordComponent>(5);
            List<DiscordComponent> row4 = new List<DiscordComponent>(5);
            List<DiscordComponent> row5 = new List<DiscordComponent>(5);

            foreach (var option in Options)
            {
                switch (option.Row)
                {
                    case 1:
                        row1.Add(await GetComponentAsync(option));
                        break;
                    case 2:
                        row2.Add(await GetComponentAsync(option));
                        break;
                    case 3:
                        row3.Add(await GetComponentAsync(option));
                        break;
                    case 4:
                        row4.Add(await GetComponentAsync(option));
                        break;
                    case 5:
                        row5.Add(await GetComponentAsync(option));
                        break;
                    default:
                        throw new ArgumentException("Invalid Row! Must be between 1 and 5", "Row");
                }
            }
            if (row1.Count != 0)
            {
                builder.AddComponents(row1);
            }
            if (row2.Count != 0)
            {
                builder.AddComponents(row2);
            }
            if (row3.Count != 0)
            {
                builder.AddComponents(row3);
            }
            if (row4.Count != 0)
            {
                builder.AddComponents(row4);
            }
            if (row5.Count != 0)
            {
                builder.AddComponents(row5);
            }
            return builder;
        }

        public async Task BackAsync()
        {

            if (LastPrompt == null)
                return;
            _client.ComponentInteractionCreated -= LastPrompt.Discord_ComponentInteractionCreated;

            await InvalidateAsync(false);
            if (Interaction == null)
                await LastPrompt.UseAsync(Message);
            else
                await LastPrompt.UseAsync(Interaction, Message);
        }

        public void AddOption(IPromptOption option)
        {
            Options.Add(option);
        }

    }
}
