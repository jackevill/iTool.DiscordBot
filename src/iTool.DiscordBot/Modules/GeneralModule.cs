﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace iTool.DiscordBot.Modules
{
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _cmdService;
        private readonly Settings _settings;

        public GeneralModule(CommandService service, Settings settings)
        {
            _cmdService = service;
            _settings = settings;
        }

        [Command("help")]
        [Summary("Returns all the enabled modules")]
        public async Task Help(string moduleName = null)
        {
            if (moduleName == null)
            {
                await ReplyAsync("", embed: new EmbedBuilder()
                {
                    Title = "Modules",
                    Color = _settings.GetColor(),
                    Description = string.Join(", ", _cmdService.Modules
                                                        .Select(x => x.Name)
                                                        .OrderBy(x => x)
                                            ),
                }.Build());
                return;
            }

            IReadOnlyList<CommandInfo> cmds = _cmdService.Modules
                                                .FirstOrDefault(x => x.Name.ToLower() == moduleName.ToLower())
                                                ?.Commands;
            if (!cmds.Any())
            {
                await ReplyAsync("", embed: new EmbedBuilder()
                {
                    Title = "Help",
                    Color = _settings.GetColor(),
                    Description = $"No module named {moduleName} found",
                }.Build());
                return;
            }

            EmbedBuilder b = new EmbedBuilder()
            {
                Title = "Module commands",
                Color = _settings.GetColor(),
                Description = $"All commands from the {moduleName} module.",
            };

            foreach (CommandInfo cmd in cmds)
            {
                b.AddField(f =>
                {
                    f.Name = cmd.Name;
                    f.Value = cmd.Summary ?? "No summary";
                });
            }
            await ReplyAsync("", embed: b.Build());
        }

        [Command("cmdinfo")]
        [Alias("commandinfo")]
        [Summary("Returns info about the command")]
        public async Task CommandInfo(string cmdName)
        {
            CommandInfo cmd = _cmdService.Commands
                                .FirstOrDefault(x => x.Aliases.Contains(cmdName));

            if (cmd == null)
            {
                await ReplyAsync("", embed: new EmbedBuilder()
                {
                    Title = "Command info",
                    Color = _settings.GetColor(),
                    Description = "No command found",
                }.Build());
                return;
            }

            EmbedBuilder b = new EmbedBuilder()
            {
                Title = "Command info",
                Color = _settings.GetColor(),
            }
            .AddField(f =>
            {
                f.Name = "Name";
                f.Value = cmd.Name;
            });

            if (string.IsNullOrEmpty(cmd.Summary))
            {
                b.AddField(f =>
                {
                    f.Name = "Summary";
                    f.Value = cmd.Summary;
                });
            }

            IEnumerable<string> aliases = cmd.Aliases.Where(x => x != cmd.Name);

            if (aliases.Any())
            {
                b.AddField(f =>
                {
                    f.Name = "Aliases";
                    f.Value = string.Join(", ", aliases);
                });
            }

            if (cmd.Parameters.Any())
            {
                b.AddField(f =>
                {
                    f.Name = "Parameters";
                    f.Value = string.Join(", ", cmd.Parameters);
                });
            }

            await ReplyAsync("", embed: b.Build());
        }

        [Command("info")]
        [Alias("information")]
        [Summary("Returns info about the bot")]
        public async Task Info()
        {
            IApplication app = await Context.Client.GetApplicationInfoAsync();

            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Color = _settings.GetColor(),
                Description = $"{app.Name} bot is a general purpose bot that has moderation commands, can check your CS:GO, Battlefield 3, 4, H and HOTS stats and a lot more! Coded by Bond_009#0253.",
                Author = new EmbedAuthorBuilder()
                {
                    Name = app.Name,
                    IconUrl = app.IconUrl
                }
            }
            .AddField(f =>
            {
                f.Name = "Info";
                f.Value = $"- **Owner**: {app.Owner} (ID {app.Owner.Id})\n" +
                            $"- **Library**: Discord.Net ({DiscordConfig.Version})\n" +
                            $"- **Runtime**: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                            $"- **Uptime**: {Utils.GetUptime().ToString(@"d\d\ hh\:mm\:ss")}";
            })
            .AddField(f =>
            {
                f.Name = "Stats";
                f.Value = $"- **Heap Size**: {Utils.GetHeapSize()} MB\n" +
                            $"- **Guilds**: {Context.Client.Guilds.Count}\n" +
                            $"- **Channels**: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
                            $"- **Users**: {Context.Client.Guilds.Sum(g => g.MemberCount)}";
            })
            .AddField(f =>
            {
                f.Name = "Links";
                f.Value = "[GitHub](https://github.com/Bond-009/iTool.DiscordBot)\n" +
                            "[Bonds Discord Guild](https://discord.gg/thKXwJb)";
            })
            .Build());
        }

        [Command("invite")]
        [Summary("Returns the OAuth2 Invite URL of the bot")]
        public async Task Invite()
            => await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = "Invite",
                Color = _settings.GetColor(),
                Description = "A user with the `MANAGE_SERVER` permission can invite with this link:\n" +
                            $"<https://discordapp.com/oauth2/authorize?client_id={(await Context.Client.GetApplicationInfoAsync()).Id}&scope=bot>"
            }.Build());

        // TODO: Improve embed
        [Command("leave")]
        [Summary("Instructs the bot to leave the guild")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task Leave()
        {
            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = "Leaving",
                Color = _settings.GetColor(),
                Url = $"https://discordapp.com/oauth2/authorize?client_id={(await Context.Client.GetApplicationInfoAsync()).Id}&scope=bot",
                Description = "Leaving, click the title to invite me back in."
            }.Build());
            await Context.Guild.LeaveAsync();
        }

        [Command("setgame")]
        [Summary("Sets the bots game")]
        [RequireTrustedUser]
        public async Task SetGame([Remainder] string input)
        {
            _settings.Game = input;
            await Context.Client.SetGameAsync(input);
        }

        [Command("ping")]
        [Summary("Returns the estimated round-trip latency, in milliseconds, to the gateway server")]
        public async Task Ping()
            => await ReplyAsync("", embed: new EmbedBuilder()
                {
                    Title = "Ping",
                    Color = _settings.GetColor(),
                    Description = $"Latency: {Context.Client.Latency}ms"
                }.Build());

        [Command("userinfo")]
        [Alias("user")]
        [Summary("Returns info about the user")]
        public async Task UserInfo(SocketUser user = null)
        {
            if (user == null) user = Context.User;

            SocketGuildUser gUser = user as SocketGuildUser;

            EmbedBuilder b = new EmbedBuilder()
            {
                Title = $"Info about {user}",
                Color = _settings.GetColor(),
                ThumbnailUrl = user.GetAvatarUrl()
            }
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Username";
                f.Value = user.Username;
            });
            if (gUser?.Nickname != null)
            {
                b.AddField(f =>
                {
                    f.IsInline = true;
                    f.Name = "Nickname";
                    f.Value = gUser.Nickname;
                });
            }
            b.AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Discriminator";
                f.Value = user.Discriminator;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Id";
                f.Value = user.Id.ToString();
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Status";
                f.Value = user.Status.ToString();
            });
            if (gUser != null)
            {
                b.AddField(f =>
                {
                    f.IsInline = true;
                    f.Name = "Voice status";
                    f.Value = $"- **Deafened**: {gUser.IsDeafened}\n" +
                        $"- **Musted**: {gUser.IsMuted}\n" +
                        $"- **SelfDeafened**: {gUser.IsSelfDeafened}\n" +
                        $"- **SelfMuted**: {gUser.IsSelfMuted}\n" +
                        $"- **Suppressed**: {gUser.IsSuppressed}";
                });
            }
            b.AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Bot/Webhook";
                f.Value = $"- **Bot**: {user.IsBot}\n" +
                    $"- **Webhook**: {user.IsWebhook}";
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Created at";
                f.Value = user.CreatedAt.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss");
            });
            if (gUser?.JoinedAt != null)
            {
                b.AddField(f =>
                {
                    f.IsInline = true;
                    f.Name = "Joined at";
                    f.Value = gUser.JoinedAt.Value.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss");
                });
            }
            await ReplyAsync("", embed: b.Build());
        }

        [Command("serverinfo")]
        [Alias("server", "guild", "quildinfo")]
        [Summary("Returns info about the guild")]
        [RequireContext(ContextType.Guild)]
        public async Task GuildInfo()
        {
            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = $"Info about {Context.Guild}",
                Color = _settings.GetColor(),
                ThumbnailUrl = Context.Guild.IconUrl
            }
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Name";
                f.Value = Context.Guild.Name;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Id";
                f.Value = Context.Guild.Id;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Owner";
                f.Value = Context.Guild.Owner.ToString();
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Members";
                f.Value = Context.Guild.MemberCount;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Voice region";
                f.Value = Context.Guild.VoiceRegionId;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Created at";
                f.Value = Context.Guild.CreatedAt.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss");
            })
            .Build());
        }
    }
}
