using DSharpPlus.Entities;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TomatenMusic.Music;
using TomatenMusic.Prompt.Model;
using TomatenMusic.Prompt.Option;
using TomatenMusic.Util;

namespace TomatenMusic.Prompt.Implementation
{
    class QueuePrompt : ButtonPrompt
    {

        public static void InvalidateFor(ulong guildId)
        {
            foreach (var prompt in ActivePrompts)
            {
                if (prompt.State != PromptState.OPEN)
                    continue;
                if (!(prompt is QueuePrompt))
                    continue;
                if (((QueuePrompt)prompt).Player.GuildId != guildId)
                    continue;
                _ = prompt.InvalidateAsync();

            }
        }
        public static void UpdateFor(ulong guildId)
        {
            _ = Task.Delay(400).ContinueWith(async (task) =>
            {
                foreach (var prompt in ActivePrompts)
                {
                    if (prompt.State != PromptState.OPEN)
                        continue;
                    if (!(prompt is QueuePrompt))
                        continue;
                    if (((QueuePrompt)prompt).Player.GuildId != guildId)
                        continue;
                    _ = prompt.UpdateAsync();
                }
            });
        }

        public GuildPlayer Player { get; private set; }

        public QueuePrompt(GuildPlayer player, DiscordPromptBase lastPrompt = null, List<DiscordEmbed> embeds = null) : base(lastPrompt, embeds: embeds)
        {
            Player = player;

            AddOption(
                new ButtonPromptOption()
                {
                    Emoji = new DiscordComponentEmoji("⏯️"),
                    Row = 1,
                    UpdateMethod = (option) =>
                    {
                        ButtonPromptOption button = (ButtonPromptOption)option;
                        if (player.State == PlayerState.Paused)
                            button.Style = DSharpPlus.ButtonStyle.Danger;
                        else
                            button.Style = DSharpPlus.ButtonStyle.Success;

                        return Task.FromResult((IPromptOption) button);
                    },
                    Run = async (args, sender, option) =>
                    {
                        if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                        {
                            _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                            return;
                        }

                        await Player.TogglePauseAsync();
                    }
                }
                );

            AddOption(new ButtonPromptOption()
                {
                Emoji = new DiscordComponentEmoji("⏮️"),
                Row = 1,
                Style = DSharpPlus.ButtonStyle.Secondary,
                Run = async (args, sender, option) =>
                {
                    if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                    {
                        _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                        return;
                    }
                    try
                    {
                        await Player.RewindAsync();
                    }catch (Exception ex)
                    {

                    }
                }
            }
            );
            AddOption(new ButtonPromptOption()
            {
                Emoji = new DiscordComponentEmoji("⏹️"),
                Row = 1,
                Style = DSharpPlus.ButtonStyle.Secondary,
                Run = async (args, sender, option) =>
                {
                    if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                    {
                        _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                        return;
                    }

                    await Player.DisconnectAsync();
                }
            });
            AddOption(new ButtonPromptOption()
            {
                Emoji = new DiscordComponentEmoji("⏭️"),
                Row = 1,
                Style = DSharpPlus.ButtonStyle.Secondary,
                Run = async (args, sender, option) =>
                {
                    if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                    {
                        _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                        return;
                    }

                    await Player.SkipAsync();

                    System.Timers.Timer timer = new System.Timers.Timer(800);
                    timer.Elapsed += (s, args) =>
                    {
                        _ = UpdateAsync();
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            );


            AddOption(
                new ButtonPromptOption()
                {
                    Row = 1,
                    UpdateMethod = (option) =>
                    {
                        ButtonPromptOption button = (ButtonPromptOption)option;

                        if (player.PlayerQueue.LoopType == LoopType.TRACK)
                        {
                            button.Style = DSharpPlus.ButtonStyle.Success;
                            button.Emoji = new DiscordComponentEmoji("🔂");
                        }
                        else if (player.PlayerQueue.LoopType == LoopType.QUEUE)
                        {
                            button.Style = DSharpPlus.ButtonStyle.Success;
                            button.Emoji = new DiscordComponentEmoji("🔁");
                        }
                        else
                        {
                            button.Style = DSharpPlus.ButtonStyle.Danger;
                            button.Emoji = null;
                            button.Content = "Loop";
                        }


                        return Task.FromResult((IPromptOption)button);
                    },
                    Run = async (args, sender, option) =>
                    {
                        if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                        {
                            _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                            return;
                        }

                        switch (player.PlayerQueue.LoopType)
                        {
                            case LoopType.NONE:
                                _ = Player.SetLoopAsync(LoopType.QUEUE);
                                break;
                            case LoopType.QUEUE:
                                _ = Player.SetLoopAsync(LoopType.TRACK);
                                break;
                            case LoopType.TRACK:
                                _ = Player.SetLoopAsync(LoopType.NONE);
                                break;
                        }
                    }
                }
                );

            AddOption(new ButtonPromptOption()
            {
                Emoji = new DiscordComponentEmoji("🔀"),
                Row = 2,
                Style = DSharpPlus.ButtonStyle.Secondary,
                Run = async (args, sender, option) =>
                {
                    if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                    {
                        _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                        return;
                    }

                    await Player.ShuffleAsync();

                }
            });

            AddOption(new ButtonPromptOption()
            {
                Emoji = new DiscordComponentEmoji("🚫"),
                Row = 2,
                Style = DSharpPlus.ButtonStyle.Secondary,
                Run = async (args, sender, option) =>
                {
                    if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                    {
                        _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                        return;
                    }

                    Player.PlayerQueue.Queue.Clear();

                    _ = UpdateAsync();
                }
            });

            AddOption(
                new ButtonPromptOption()
                {
                    Emoji = new DiscordComponentEmoji("➡️"),
                    Content = "AutoPlay",
                    Row = 2,
                    UpdateMethod = (option) =>
                    {
                        ButtonPromptOption button = (ButtonPromptOption)option;
                        if (player.Autoplay)
                            button.Style = DSharpPlus.ButtonStyle.Success;
                        else
                            button.Style = DSharpPlus.ButtonStyle.Danger;

                        return Task.FromResult((IPromptOption)button);
                    },
                    Run = async (args, sender, option) =>
                    {
                        if (!await Player.AreActionsAllowedAsync((DiscordMember)args.User))
                        {
                            _ = args.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Please connect to the bots Channel to use this Interaction"));
                            return;
                        }

                        Player.Autoplay = !Player.Autoplay;

                        _ = UpdateAsync();
                    }
                }
                );
        }

        protected async override Task<DiscordMessageBuilder> GetMessageAsync()
        {
            return new DiscordMessageBuilder()
                .AddEmbed(Common.GetQueueEmbed(Player))
                .AddEmbed(await Common.CurrentSongEmbedAsync(Player))
                .AddEmbeds(Embeds);
        }
    }
}
