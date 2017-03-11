﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace iTool.DiscordBot
{
    public class CommandHandler
    {
        public CommandService CommandService { get; private set; }

        private DiscordSocketClient client;
        private IDependencyMap map;

        public async Task Install(IDependencyMap _map, CommandServiceConfig config)
        {
            // Create Command Service, inject it into Dependency Map
            client = _map.Get<DiscordSocketClient>();
            CommandService = new CommandService(config);
            _map.Add(CommandService);
            map = _map;

            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());

            // HACK: Loads all commands and than unloads the disabled modules
            if (File.Exists(Common.SettingsDir + Path.DirectorySeparatorChar + "disabled_modules.txt"))
            {
                IEnumerable<string> disabledModules = File.ReadAllText(Common.SettingsDir + Path.DirectorySeparatorChar + "disabled_modules.txt")
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

                foreach (ModuleInfo moduleInfo in CommandService.Modules.Where(x => disabledModules.Contains(x.Name)).ToArray())
                {
                    await CommandService.RemoveModuleAsync(moduleInfo);
                }
            }

            client.MessageReceived += HandleCommand;
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            // Don't handle the command if it is a system message
            SocketUserMessage message = parameterMessage as SocketUserMessage;
            if (message == null) { return; }

            // Mark where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message has a valid prefix, adjust argPos 
            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) 
                || message.HasStringPrefix(Program.Settings.Prefix, ref argPos)))
            { return; }

            // Execute the Command, store the result
            IResult result = await CommandService.ExecuteAsync(new CommandContext(client, message), argPos, map);

            // If the command failed, notify the user
            if (!result.IsSuccess)
            {
                if (result.ErrorReason.ToLower() != "unknown command.")
                {
                    await Program.Log(new LogMessage(LogSeverity.Error, "CommandHandler", result.ErrorReason));

                    await message.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                    {
                        Title = "Error",
                        Color = new Color(204, 0, 0),
                        Description = result.ErrorReason
                    });
                }
            }
        }
    }
}
