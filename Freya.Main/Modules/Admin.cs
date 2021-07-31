using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;

using Freya.Helpers.Precondition;
using Freya.Helpers.Util;
using Freya.Infrastructure.Collections;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using static Config;

namespace Freya.Modules
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("enject", RunMode = RunMode.Async)]
        [RequireRole(RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task Enject(string filename)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{filename}.json");
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
            var FileContent = JsonConvert.DeserializeObject<List<FileContent>>(content);
            var AwaitMessage = await ReplyAsync("Подождите");
            var ToSearchEvent = await EventType.FindOne(x => x.Category == (Category)Enum.Parse(typeof(Category), FileContent.First().Category));
            ToSearchEvent.EventInfos = new();
            await ToSearchEvent.Save();
            for (int i = 0; i < FileContent.Count; i++)
            {
                var item = FileContent.ElementAt(i);
                var eventType = await EventType.GetOrCreate((Category)Enum.Parse(typeof(Category), item.Category));
                var type = eventType.EventInfos.FirstOrDefault(x => x.Name == item.Event) ?? new();
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
                eventType.EventInfos.Add(type);
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
            await AwaitMessage.DeleteAsync();
            await Context.Channel.SendSuccessMessage();
        }

        [Command("gencommands", RunMode = RunMode.Async)]
        [RequireRole(RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task GenerateSlashCommands()
        {
            try
            {
                var ReportCommand = new SlashCommandBuilder()
                {
                    Name = "report",
                    Description = "Написать отчёт об ивенте",
                    Options = new()
                    {
                        new()
                        {
                            Name = "eventid",
                            Type = ApplicationCommandOptionType.String,
                            Required = true,
                            Description = "Ид проведенного вами ивента"
                        },
                        new()
                        {
                            Name = "roundscount",
                            Type = ApplicationCommandOptionType.Integer,
                            Required = true,
                            Description = "Сколько кругов шёл ивент?"
                        },
                        new()
                        {
                            Name = "usercount",
                            Type = ApplicationCommandOptionType.Integer,
                            Required = true,
                            Description = "Сколько человек было на вашем ивенте?(между 1-99)"
                        },
                        new()
                        {
                            Name = "guildmembers",
                            Type = ApplicationCommandOptionType.Boolean,
                            Required = true,
                            Description = "Участвовали люди у которых есть своя гильдия?"
                        },
                    }
                };
                var PWarnCommand = new SlashCommandBuilder()
                {
                    Name = "pwarn",
                    Description = "Выдать выговор ивентеру",
                    Options = new()
                    {
                        new()
                        {
                            Name = "user",
                            Required = true,
                            Type = ApplicationCommandOptionType.User,
                            Description = "Ивентер"
                        },
                        new()
                        {
                            Name = "reason",
                            Required = true,
                            Type = ApplicationCommandOptionType.String,
                            Description = "Причина"
                        }
                    }
                };
                var UnWarnCommand = new SlashCommandBuilder()
                {
                    Name = "unwarn",
                    Description = "Снять выговор ивентеру",
                    Options = new()
                    {
                        new()
                        {
                            Name = "idwarn",
                            Required = true,
                            Type = ApplicationCommandOptionType.String,
                            Description = "Ид выговора"
                        },
                    }
                };
                var ProfileCommand = new SlashCommandBuilder()
                {
                    Name = "profile",
                    Description = "Профиль ивентера",
                    Options = new()
                    {
                        new()
                        {
                            Name = "user",
                            Type = ApplicationCommandOptionType.User,
                            Required = false,
                            Description = "Ивентер"
                        },
                    }
                };
                var RequestCommand = new SlashCommandBuilder()
                {
                    Name = "request",
                    Description = "Попросить кол-во монет для ивента",
                    Options = new()
                    {
                        new()
                        {
                            Name = "eventid",
                            Type = ApplicationCommandOptionType.String,
                            Required = true,
                            Description = "Ид проведенного вами ивента"
                        },
                        new()
                        {
                            Name = "coinsamount",
                            Type = ApplicationCommandOptionType.Integer,
                            Required = true,
                            Description = "Количество монет"
                        },
                    }
                };
                var StartEventCommand = new SlashCommandBuilder()
                {
                    Name = "startevent",
                    Description = "Запустить ивент",
                };

                await Context.Client.Rest.CreateGuildCommand(ReportCommand.Build(), StaticVars.MainGuild);
                await Context.Client.Rest.CreateGuildCommand(PWarnCommand.Build(), StaticVars.MainGuild);
                await Context.Client.Rest.CreateGuildCommand(UnWarnCommand.Build(), StaticVars.MainGuild);
                await Context.Client.Rest.CreateGuildCommand(ProfileCommand.Build(), StaticVars.MainGuild);
                await Context.Client.Rest.CreateGuildCommand(RequestCommand.Build(), StaticVars.MainGuild);
                await Context.Client.Rest.CreateGuildCommand(StartEventCommand.Build(), StaticVars.MainGuild);
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        [Command("delcmd", RunMode = RunMode.Async)]
        [RequireRole(RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteCmdCommand(string cmd)
        {
            try
            {
                await (await Context.Client.Rest.GetGuildApplicationCommands(StaticVars.MainGuild)).First(x => x.Name == cmd).DeleteAsync();
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        [Command("addrank", RunMode = RunMode.Async)]
        [RequireRole(RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task AddRankCommand(SocketGuildUser user, int rank)
        {
            try
            {
                var eventer = await Eventer.FindOne(x => x.UserID == user.Id);
                if (eventer is null) return;
                eventer.Rank = rank;
                await eventer.Save();
                await ReplyAsync(embed: new CustomEmbedBuilder()
                {
                    Author = new()
                    {
                        IconUrl = user.GetAvatarUrl(),
                        Name = $"| Успешно обновил ранг ивентера до: {rank}"
                    }
                }.Build());
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                Console.WriteLine(json);
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
