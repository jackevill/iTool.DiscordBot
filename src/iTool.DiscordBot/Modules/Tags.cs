using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iTool.DiscordBot.Modules
{
    public class Tags : ModuleBase<SocketCommandContext>, IDisposable
    {
        Settings settings;
        TagDatabase db;

        public Tags(Settings settings)
        {
            this.settings = settings;
            db = new TagDatabase();
            db.Database.EnsureCreated();
        }

        [Command("tag create")]
        [Alias("createtag")]
        [Summary("Creates a new tag")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task CreateTag(string name, [Remainder]string text)
        {
            await db.CreateTagAsync(Context, name, text, Context.Message.Attachments.FirstOrDefault()?.Url);

            await Tag(name);
        }

        [Command("tag")]
        [Summary("Searches for a tag")]
        [RequireContext(ContextType.Guild)]
        public async Task Tag(string name)
        {
            Tag tag = await db.GetTagAsync(Context.Guild.Id, name);

            if (tag == null) return;

            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = tag.Name,
                Color = settings.GetColor(),
                Description = tag.Text,
                ImageUrl = tag.Attachment
            });
        }

        [Command("tag delete")]
        [Alias("tag remove", "deletetag", "removetag")]
        [Summary("Deletes a tag")]
        [RequireContext(ContextType.Guild)]
        public async Task TagDelete(string name)
        {
            await db.DeleteTagAsync(Context, name);

            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = $"Delete tag {name}",
                Color = settings.GetColor(),
                Description = $"Successfully deleted tag {name}",
            });
        }

        [Command("tags")]
        [Alias("tag list", "tags list", "listtags")]
        [Summary("Lists all tags")]
        [RequireContext(ContextType.Guild)]
        public async Task TagList()
        {
            await ReplyAsync("", embed: new EmbedBuilder()
            {
                Title = $"Tags",
                Color = settings.GetColor(),
                Description = string.Join(" ,", db.GetTags(Context.Guild.Id)
                                                .Select(x => x.Name)),
            });
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}