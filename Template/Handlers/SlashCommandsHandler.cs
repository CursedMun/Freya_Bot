using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Freya.Helpers.Util;
using Freya.Infrastructure.Collections;
using Freya.Services;

using static Config;

namespace Freya.Handler
{
    public class SlashCommandsHandler : DiscordHandler
    {
        private DiscordSocketClient Client;

        private ulong[] _roles;
        public override void Initialize(DiscordSocketClient client)
        {
            Client = client;
            _roles = TypeRoles.GetValueOrDefault((int)RolesType.EventMod).PushArray(TypeRoles.GetValueOrDefault((int)RolesType.MaxPerms));
            Client.InteractionCreated += Client_InteractionCreated;
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            if (arg is not SocketSlashCommand command) return;
            if (!(command.User as SocketGuildUser).Roles.Any(r => _roles.Any(s => r.Id == s))) return;
            var eventer = await Eventer.GetOrCreate(command.User.Id);
            await command.AcknowledgeAsync();
            switch (command.Data.Name)
            {
                case "profile":
                await HandleProfileCommand(command);
                break;
                case "report":
                await HandleReportCommand(command, eventer);
                break;
                case "warn":
                if (!(command.User as SocketGuildUser).Roles.Any(r => TypeRoles.GetValueOrDefault((int)RolesType.MaxPerms).Any(s => r.Id == s))) return;
                await HandleWarnCommand(command);
                break;
                case "unwarn":
                if (!(command.User as SocketGuildUser).Roles.Any(r => TypeRoles.GetValueOrDefault((int)RolesType.MaxPerms).Any(s => r.Id == s))) return;
                await HandleUnWarnCommand(command);
                break;
                case "request":
                await HandleRequestCommand(command, eventer);
                break;
            }
        }
        /// <summary>
        /// Request money, for an event
        /// </summary>
        /// <param name="command"></param>
        /// <param name="eventer"></param>
        /// <returns></returns>
        private async Task HandleRequestCommand(SocketSlashCommand command, Eventer eventer)
        {
            var options = command.Data.Options.ToArray();
            var EventID = (string)options[0];
            var CoinsAmount = (int)options[1];
            var GoingEvent = eventer.Events.Find(x => x.ID == EventID);
            if (GoingEvent is null)
            {
                await SendError(command, "Данного ивента не существует");
                return;
            }
            var Request = await CoinsRequest.GetOrCreate(EventID);
            if (Request.Active || Request.Accepted)
            {
                await SendError(command, "Данный запрос уже приняли или он уже на рассмотре");
                return;
            }
            Request.Active = true;
            Request.Amount = CoinsAmount;
            Request.UserID = eventer.UserID;
            try
            {
                var ConfirmRequest = new ComponentBuilder()
                    .WithButton(new ButtonBuilder()
                    {
                        CustomId = $"ConfirmRequest-{Request.ID}",
                        Style = ButtonStyle.Success,
                        Label = "Принять"
                    })
                    .WithButton(new ButtonBuilder()
                    {
                        CustomId = $"CancelRequest-{Request.ID}",
                        Style = ButtonStyle.Danger,
                        Label = "Отклонить"
                    });
                await command.FollowupAsync(embed: new CustomEmbedBuilder()
                {
                    Description = $@"| Ивент успешно запущен"
                }.Build(), ephemeral: true);
                var msg = await (Client.GetMainGuild().GetChannel(StaticVars.RequestsChannel) as SocketTextChannel)
                    .SendMessageAsync(
                    embed: new CustomEmbedBuilder()
                    {
                        Title = "Поступил запрос",
                        Description = $@"От: <@{eventer.UserID}>
                                   количество: {Request.Amount}"
                    }.Build(), component: ConfirmRequest.Build());
                Request.MessageID = msg.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await Request.Save();
            }
        }
        /// <summary>
        /// Eventer Profile no need to pass the eventer because it can be a someone profile
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task HandleProfileCommand(SocketSlashCommand command)
        {
            var options = command.Data.Options?.ToArray();
            var User = options != null ? (SocketGuildUser)options[0].Value : command.User;
            var eventer = await Eventer.GetOrCreate(User.Id);
            var VoiceActivity = eventer.Events.Sum(x => x.Time.CountDiff);
            var WeekEvents = eventer.Events.Select(x => DateTimeOffset.Now.AddDays(-(int)DateTimeOffset.Now.DayOfWeek) == x.Time.CreatedAT.AddDays(-(int)DateTimeOffset.Now.DayOfWeek)).Count();
            var TotalEvents = eventer.Events.Count;
            var EventerProfile = new CustomEmbedBuilder()
            {
                Title = $"Статистика ивентера -- {User.Tag()}",
                ThumbnailUrl = User.GetAvatarUrl(size: 1024),
                Description = @$"```Кол-во дней в стаффе - {Math.Round((DateTimeOffset.Now - eventer.CreateAt).TotalDays, 0, MidpointRounding.ToZero)}```",
                Fields = new()
                {
                    new()
                    {
                        Name = "Ранг:",
                        Value = $"```{eventer.Rank}```",
                        IsInline = true,
                    },
                    new()
                    {
                        Name = "<:warn:848454640710320138>Выговоры:",
                        Value = $"```{eventer.Warns.Count}```",
                        IsInline = true,
                    },
                    new()
                    {
                        Name = "<:timelock:841952512060030977>Голосовой:",
                        Value = $"```{FormatDate(eventer.Events.Sum(x => x.Time.CountDiff))}```",
                        IsInline = true,
                    },
                    new()
                    {
                        Name = "<a:circle:847956594725486624>За неделю:",
                        Value = $"```{eventer.Events.Select(x => DateTimeOffset.Now.AddDays(-(int)DateTimeOffset.Now.DayOfWeek) == x.Time.CreatedAT.AddDays(-(int)DateTimeOffset.Now.DayOfWeek)).Count()}```",
                        IsInline = true,
                    },
                    new()
                    {
                        Name = "<a:circle:847956594725486624>Общее кол-во:",
                        Value = $"```{eventer.Events.Count}```",
                        IsInline = true,
                    },
                }
            };

            await command.FollowupAsync(embed: EventerProfile.Build(), ephemeral: true);
        }
        /// <summary>
        /// Format seconds to get a date
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private static string FormatDate(double seconds)
        {
            var FormatedTime = new TimeSpan(0, 0, Convert.ToInt32(seconds));
            return FormatedTime.ToString();
        }
        /// <summary>
        /// Unwarn an eventer
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static async Task HandleUnWarnCommand(SocketSlashCommand command)
        {
            var options = command.Data.Options.ToArray();
            var WarnID = (string)options[0].Value;
            var eventer = await Eventer.FindOne(x => x.Warns.Any(x => x.ID.ToString() == WarnID));
            if (eventer is not Eventer)
            {
                await SendError(command, "Не существует такого иди");
                return;
            }

            var deleted = eventer.Warns.Remove(eventer.Warns.Find(x => x.ID.ToString() == WarnID));
            if (!deleted)
            {
                await SendError(command, "Не существует такого иди");
                return;
            }
            await eventer.Save();
            var success = new CustomEmbedBuilder()
            {
                Description = "Вы успешно сняли выговор",
                Color = Color.Green,
                Timestamp = System.DateTimeOffset.Now,
            };
            await command.FollowupAsync(embed: success.Build(), ephemeral: true);
        }
        /// <summary>
        /// Warn an eventer
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>

        private static async Task HandleWarnCommand(SocketSlashCommand command)
        {
            var options = command.Data.Options.ToArray();
            var User = (SocketGuildUser)options[0].Value;
            var Reason = (string)options[1].Value;
            var eventer = await Eventer.FindOne(x => x.UserID == User.Id);
            if (eventer is default(Eventer))
            {
                var error = new CustomEmbedBuilder()
                {
                    Description = "Неверное использование команды",
                    Color = Color.Red,
                    Timestamp = DateTimeOffset.Now,
                };
                await command.FollowupAsync(embed: error.Build(), ephemeral: true);
            }

            eventer.Warns.Add(new()
            {
                Reason = Reason,
            });
            await eventer.Save();
            var success = new CustomEmbedBuilder()
            {
                Description = "Вы успешно выдали выговор",
                Color = Color.Green,
                Timestamp = System.DateTimeOffset.Now,
            };
            await command.FollowupAsync(embed: success.Build(), ephemeral: true);
        }
        private static Task<RestFollowupMessage> SendError(SocketSlashCommand command, string message = null)
        {
            var error = new CustomEmbedBuilder()
            {
                Description = message ?? "Неверное использование команды",
                Color = Color.Red,
                Timestamp = DateTimeOffset.Now,
            };
            return command.FollowupAsync(embed: error.Build(), ephemeral: true);
        }
        /// <summary>
        /// Report for an event
        /// </summary>
        /// <param name="command"></param>
        /// <param name="eventer"></param>
        /// <returns></returns>
        private async Task HandleReportCommand(SocketSlashCommand command, Eventer eventer)
        {
            var options = command.Data.Options.ToArray();
            var EventID = options[0].Value.ToString();
            var EndedEvent = eventer.Events.FirstOrDefault(x => x.ID == EventID);
            if (EndedEvent is null)
            {
                await SendError(command);
                return;
            }
            EndedEvent.EventReport.RoundsCount = (int)options[1].Value;
            EndedEvent.EventReport.UsersCount = (int)options[2].Value;
            if (EndedEvent.EventReport.UsersCount > 99)
            {
                await SendError(command);
                return;
            }

            EndedEvent.EventReport.GuildMembersPresent = (bool)options[3].Value;
            if (EndedEvent.Reported)
            {
                await SendError(command);
                return;
            }
            var SuccessEmbed = new CustomEmbedBuilder()
            {
                Description = $"Данные записаны в вашу статистику и отправлены в канал <#{StaticVars.EventReportsChannel}>",
                Color = Color.Green,
                Timestamp = DateTimeOffset.Now,
            };
            try
            {
                var category = await EventType.FindOne(x => x.ID == EndedEvent.EventCategoryID);
                var EventName = category.EventInfos.Find(x => x.ID == EndedEvent.EventTypeID);
                await (Client.GetMainGuild().GetChannel(StaticVars.EventReportsChannel) as ITextChannel).SendMessageAsync(embed: new CustomEmbedBuilder()
                {
                    Title = "ОТЧЕТНОСТЬ О ИВЕНТЕ!",
                    Description = $@"
                                    Ведущий: <@{eventer.UserID}>
                                    Время: от {EndedEvent.Time.StartTime} до {EndedEvent.Time.EndTime}
                                    Участвовало людей: {EndedEvent.EventReport.UsersCount}
                                    Категория: {category.Category}
                                    Ивент: {EventName.Name}
                                    Вознаграждение: {null}
                                    Гильдия | Пар: {null}",
                    Color = Color.Green,
                    Timestamp = DateTimeOffset.Now,
                }.Build());


                await command.FollowupAsync(embed: SuccessEmbed.Build(), ephemeral: true);
            }
            catch
            {
                await command.Channel.SendMessageAsync(embed: SuccessEmbed.Build());
            }
            finally
            {
                await eventer.Save();

            }
        }
    }
}
