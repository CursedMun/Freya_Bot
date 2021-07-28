using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;

using Freya.Helpers.Precondition;
using Freya.Infrastructure.Collections;

using Newtonsoft.Json;

using static Config;

namespace Freya.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("Enject", RunMode = RunMode.Async)]
        [RequireRole(RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task Enject(string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), $"\\{filename}.json");
            if (!File.Exists(file))
            {
                await Context.Channel.SendErrorMessage(ErrorMessage: "Неверный файл");
                return;
            }

            var content = File.ReadAllText(file);
            if (string.IsNullOrEmpty(content))
            {
                await Context.Channel.SendErrorMessage(ErrorMessage: "Файл пустой");
                return;
            }
            var Test = JsonConvert.DeserializeObject<List<FileContent>>(content);
            for (int i = 0; i < Test.Count; i++)
            {
                var item = Test.ElementAt(i);
                var eventType = await EventType.GetOrCreate((Category)Enum.Parse(typeof(Category), item.Category));
                var type = eventType.EventInfos.FirstOrDefault() ?? new();
                type.Name = item.Event;
                type.Embed = new()
                {
                    Color = item.Embed.Color,
                    Description = item.Embed.Description,
                    Title = item.Embed.Title,
                    Fields = item.Embed.Fields,
                    Image = item.Embed.Image,
                    PlainText = item.Embed.PlainText
                };
                switch (eventType.Category)
                {
                    case Category.маленький:
                    eventType.Text = "✧ маленький 5 баллов.";
                    break;
                    case Category.средний:
                    eventType.Text = "✧ средний 10 баллов.";
                    break;
                    case Category.большой:
                    eventType.Text = "✧ большой 15 баллов.";
                    break;
                    case Category.игровой:
                    eventType.Text = "✧ игровой 15 баллов.";
                    break;
                }
                await eventType.Save();
            }
        }
        public partial class FileContent
        {
            [JsonProperty("category")]
            public string Category { get; set; }

            [JsonProperty("event")]
            public string Event { get; set; }

            [JsonProperty("embed")]
            public Embed Embed { get; set; }
        }

        public partial class Embed
        {
            [JsonProperty("plainText")]
            public string PlainText { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("color")]
            public uint Color { get; set; }

            [JsonProperty("image")]
            public Uri Image { get; set; }

            [JsonProperty("fields")]
            public Field[] Fields { get; set; }
        }
    }
}
