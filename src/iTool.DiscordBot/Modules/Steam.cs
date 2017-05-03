using Discord;
using Discord.Commands;
using iTool.DiscordBot.Steam;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace iTool.DiscordBot.Modules
{
    public class Steam : ModuleBase
    {
        Settings settings;
        SteamAPI client;

        public Steam(Settings settings, SteamAPI steamapi)
        {
            if (string.IsNullOrEmpty(settings.SteamKey))
            {
                throw new Exception("No SteamKey found.");
            }

            this.settings = settings;
            this.client = steamapi;
        }

        [Command("vanityurl")]
        [Alias("resolvevanityurl")]
        [Summary("Returns the steamID64 of the user")]
        public async Task ResolveVanityURL(string name = null)
        {
            if (name == null) { name = Context.User.Username; }
            
            await ReplyAsync((await client.ResolveVanityURL(name)).ToString());
        }

        [Command("steam")]
        [Alias("getplayersummaries", "playersummaries")]
        [Summary("Returns basic steam profile information")]
        public async Task PlayerSummaries(string name = null)
        {
            if (name == null) { name = Context.User.Username; }
            PlayerList<PlayerSummary> player = await client.GetPlayerSummaries(new [] {(await client.ResolveVanityURL(name))});

            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = $"Player summary fot {player.Players.First().PersonaName}",
                Color = new Color((uint)settings.Color),
                ThumbnailUrl = player.Players.First().AvatarMedium,
                Url = player.Players.First().ProfileURL
            }
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "SteamID";
                f.Value = player.Players.First().SteamID;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Persona name";
                f.Value = player.Players.First().PersonaName;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Persona state";
                f.Value = player.Players.First().PersonaState.ToString();
            }));
        }

        [Command("playerbans")]
        [Alias("getplayerbans")]
        [Summary("Returns Community, VAC, and Economy ban statuses for given players")]
        public async Task PlayerBans(string name = null)
        {
            if (name == null) { name = Context.User.Username; }

            PlayerList<PlayerBan> player = await client.GetPlayerBans(new [] {(await client.ResolveVanityURL(name))});

            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = $"Community, VAC, and Economy ban statuses",
                Color = new Color((uint)settings.Color),
            }
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "SteamID";
                f.Value = player.Players.First().SteamID;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "CommunityBanned";
                f.Value = player.Players.First().CommunityBanned;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "VACBanned";
                f.Value = player.Players.First().VACBanned;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Number of VAC bans";
                f.Value = player.Players.First().NumberOfVACBans;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Days since last ban";
                f.Value = player.Players.First().DaysSinceLastBan;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Number of game bans";
                f.Value = player.Players.First().NumberOfGameBans;
            })
            .AddField(f =>
            {
                f.IsInline = true;
                f.Name = "Economy ban";
                f.Value = player.Players.First().EconomyBan;
            }));
        }

        [Command("steamprofile")]
        [Summary("Returns the URL to the steam profile of the user")]
        public async Task SteamProfile(string name = null)
        {
            if (name == null) { name = Context.User.Username; }

            await ReplyAsync("https://steamcommunity.com/profiles/" + (await client.ResolveVanityURL(name)).ToString());
        }
    }
}
