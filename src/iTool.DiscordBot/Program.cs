﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using iTool.DiscordBot.Audio;
using OpenWeather;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iTool.DiscordBot
{
    public static class Program
    {
        public static void Main(string[] args) => Start().GetAwaiter().GetResult();

        public static List<ulong> BlacklistedUsers { get; set; }
        public static IUser Owner { get; private set; }
        public static Settings Settings { get; set; }
        public static List<ulong> TrustedUsers { get; set; }

        private static DiscordSocketClient discordClient;
        private static List<string> bannedWords;

        public static async Task Start()
        {
            try
            {
                Settings = SettingsManager.LoadSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                Environment.Exit(1);
            }

            BlacklistedUsers = Utils.LoadListFromFile(Common.SettingsDir + Path.DirectorySeparatorChar + "blacklisted_users.txt").Select(ulong.Parse).ToList();

            TrustedUsers = Utils.LoadListFromFile(Common.SettingsDir + Path.DirectorySeparatorChar + "trusted_users.txt").Select(ulong.Parse).ToList();

            bannedWords = Utils.LoadListFromFile(Common.SettingsDir + Path.DirectorySeparatorChar + "banned_words.txt").ToList();

            if (!File.Exists(Common.AudioIndexFile)) { AudioManager.ResetAudioIndex(); }

            if (string.IsNullOrEmpty(Settings.DiscordToken))
            {
                Console.WriteLine("No token");

                if (!Console.IsInputRedirected)
                { Console.ReadKey(); }

                Environment.Exit(0);
            }

            discordClient = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = Settings.AlwaysDownloadUsers,
                ConnectionTimeout = Settings.ConnectionTimeout,
                DefaultRetryMode = Settings.DefaultRetryMode,
                LogLevel = Settings.LogLevel,
                MessageCacheSize = Settings.MessageCacheSize
            });

            discordClient.Log += Log;
            discordClient.MessageReceived += DiscordClient_MessageReceived;
            discordClient.Ready += DiscordClient_Ready;

            await discordClient.LoginAsync(TokenType.Bot, Settings.DiscordToken);
            await discordClient.StartAsync();

            DependencyMap map = new DependencyMap();
            map.Add(discordClient);
            map.Add(new AudioService());
            map.Add(new OpenWeatherClient(Settings.OpenWeatherMapKey));

            await new CommandHandler().Install(map, new CommandServiceConfig()
            {
                CaseSensitiveCommands = Settings.CaseSensitiveCommands,
                DefaultRunMode = Settings.DefaultRunMode
            });

            if (Console.IsInputRedirected)
            {
                await Task.Delay(-1);
            }

            while (true)
            {
                string input = Console.ReadLine().ToLower();
                switch (input)
                {
                    case "quit":
                    case "exit":
                    case "stop":
                        await Quit();
                        break;
                    case "clear":
                    case "cls":
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine(input + ": command not found");
                        break;
                }
            }
        }

        private async static Task DiscordClient_Ready()
        {
            Owner = (await discordClient.GetApplicationInfoAsync()).Owner;
            await Log(new LogMessage(LogSeverity.Critical, nameof(Program), $"Succesfully connected as {discordClient.CurrentUser.ToString()}, with {Owner.ToString()} as owner"));

            if (!string.IsNullOrEmpty(Settings.Game))
            {
                await discordClient.SetGameAsync(Settings.Game);
            }
        }

        private async static Task DiscordClient_MessageReceived(SocketMessage arg)
        {
            await Log(new LogMessage(LogSeverity.Verbose, nameof(Program), arg.Author.Username + ": " + arg.Content));

            if (Settings.AntiSwear && !bannedWords.IsNullOrEmpty()
                && bannedWords.Any(Regex.Replace(arg.Content.ToLower(), "[^A-Za-z0-9]", "").Contains))
            {
                await arg.DeleteAsync();
                await arg.Channel.SendMessageAsync(arg.Author.Mention + ", please don't put such things in chat");
            }
        }

        public static Task Log(LogMessage msg)
        {
            if (msg.Severity > Settings.LogLevel)
            { return Task.CompletedTask; }

            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static async Task Quit()
        {
            await discordClient.LogoutAsync().ConfigureAwait(false);
            discordClient.Dispose();

            // TODO: Dispose OpenWeatherClient
            //OpenWeatherClient.Dispose();

            if (!BlacklistedUsers.IsNullOrEmpty())
            { File.WriteAllLines(Common.SettingsDir + Path.DirectorySeparatorChar + "blacklisted_users.txt", BlacklistedUsers.Select(x => x.ToString())); }

            if (!TrustedUsers.IsNullOrEmpty())
            { File.WriteAllLines(Common.SettingsDir + Path.DirectorySeparatorChar + "trusted_users.txt", TrustedUsers.Select(x => x.ToString())); }

            SettingsManager.SaveSettings(Settings);
            Environment.Exit(0);
        }
    }
}
