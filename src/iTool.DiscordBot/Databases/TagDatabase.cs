using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iTool.DiscordBot
{
    public class TagDatabase : DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!Directory.Exists(Common.DataDir)) Directory.CreateDirectory(Common.DataDir);

            optionsBuilder.UseSqlite($"Filename={Path.Combine(Common.DataDir, "tags.sqlite.db")}");
        }
        
        public Task<Tag> GetTagAsync(ulong guildID, string name)
            => Tags.FirstOrDefaultAsync(x => x.GuildID == guildID && x.Name == name.ToLower());

        public IEnumerable<Tag> GetTags(ulong guildID)
            => Tags.Where(x => x.GuildID == guildID).AsEnumerable();

        public async Task CreateTagAsync(SocketCommandContext context, string name, string content, string attachment = null)
        {
            if (await Tags.AnyAsync(x => x.GuildID == context.Guild.Id && x.Name == name.ToLower()))
                throw new ArgumentException($"The tag `{name}` already exists.");

            await Tags.AddAsync(new Tag()
            {
                Name = name.ToLower(),
                GuildID = context.Guild.Id,
                AuthorID = context.User.Id,
                Text = content,
                Attachment = attachment
            });
            await SaveChangesAsync();
        }

        public async Task DeleteTagAsync(SocketCommandContext context, string name)
        {
            Tag tag = await Tags.FirstOrDefaultAsync(x => x.GuildID == context.Guild.Id && x.Name == name);

            if (tag == null)
                throw new ArgumentException($"The tag `{name}` does not exist.");

            var user = context.User as SocketGuildUser;
            if (tag.AuthorID != user.Id && !user.GuildPermissions.ManageMessages)
                throw new UnauthorizedAccessException($"You are not the owner of the tag `{name}`.");

            Tags.Remove(tag);
            await SaveChangesAsync();
        }
    }
}