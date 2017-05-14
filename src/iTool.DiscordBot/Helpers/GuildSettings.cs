using System.ComponentModel.DataAnnotations;

namespace iTool.DiscordBot
{
    public class GuildSettings : DbEntity
    {
        [Required]
        public ulong GuildID { get; set; }
        public string Prefix { get; set; }
        public ulong? WelcomeChannel { get; set; }
        public bool WelcomeMembers { get; set; }
        public string WelcomeMessage { get; set; } = "Welcome {mention} to {server} server.";
    }
}
