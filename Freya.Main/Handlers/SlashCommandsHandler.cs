using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Freya.Helpers.Util;
using Freya.Infrastructure.Collections;
using Freya.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Config;
using static Freya.Infrastructure.Mongo.EventerExtension;
namespace Freya.Handler
{
    public class SlashCommandsHandler : DiscordHandler
    {
        private static DiscordSocketClient Client;

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
            await command.DeferAsync();
            switch (command.Data.Name)
            {
                case "profile":
                    await HandleProfileCommand(command);
                    break;
                case "report":
                    await HandleReportCommand(command, eventer);
                    break;
                case "pwarn":
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
                case "startevent":
                    await HandleStartEventCommand(command, eventer);
                    break;
                case "transferevent":
                    await HandleTransferEventCommand(command, eventer);
                    break;
            }
        }

        private static async Task HandleTransferEventCommand(SocketSlashCommand command, Eventer eventer)
        {
            var options = command.Data.Options.ToArray();
            var Target = (IGuildUser)options[0].Value;
#pragma warning disable CS0183 // 'is' expression's given expression is always of the provided type
            var EventGoing = eventer.Events.First(x => !x.Finished && x.Messages is not null && x.Messages.ManageMessageID is ulong);
#pragma warning restore CS0183 // 'is' expression's given expression is always of the provided type

            if (EventGoing is null)
            {
                await SendError(command, "У вас нет активных ивентов");
                return;
            }

            if (Target is null || !Target.RoleIds.Contains(StaticVars.PhoenixRole))
            {
                await SendError(command, "Данный участник не является ивентером");
                return;
            }

            var TargetEventer = await Eventer.GetOrCreate(Target.Id);
            TargetEventer.Events.Add(EventGoing);
            eventer.Events.Remove(EventGoing);

            var SettingsChannel = Client.GetMainGuild().GetTextChannel(EventGoing.Channels.SettingsChannelID);
            var SettingsMessage = await GetMessage(SettingsChannel, EventGoing.Messages.ManageMessageID);

            var eventID = EventGoing.ID;
            var ManageEvent = new ComponentBuilder()
            .WithButton(new ButtonBuilder()
            {
                CustomId = $"SendNews-{eventID}",
                Style = ButtonStyle.Primary,
                Label = "Отправить Анонс?"
            })
            .WithButton(new ButtonBuilder()
            {
                CustomId = $"CloseChat-{eventID}",
                Style = ButtonStyle.Secondary,
                Label = "Закрыть чат?",
            })
            .WithButton(new ButtonBuilder()
            {
                CustomId = $"CloseVoice-{eventID}",
                Style = ButtonStyle.Secondary,
                Label = "Закрыть войс?"
            })
            .WithButton(new ButtonBuilder()
            {
                CustomId = $"EndEvent-{eventID}",
                Style = ButtonStyle.Danger,
                Label = "Закончить ивент"
            });
            var category = await EventType.FindOne(x => x.ID == EventGoing.EventCategoryID);
            var EventName = category.EventInfos.Find(x => x.ID == EventGoing.EventTypeID);
            CustomEmbedBuilder EventEmbed = new()
            {
                Title = $"Информация о столе",
                ThumbnailUrl = Target.GetAvatarUrl(),
                ImageUrl = "https://cdn.discordapp.com/attachments/847851000815681536/868820301939638292/rY0VuT6.gif",
                Description = $"<:circle:855370914841231371>**Категория: {category.Category}**\n<:circle:855370914841231371>**Ивент: {EventName.Name}** \n<:circle:855370993433444383>**Время проведения: {EventGoing.Time.StartTime:M/d/yyyy HH:mm} **\n<a:circle:847956594725486624>**Канал управления ─ <#{EventGoing.Channels.SettingsChannelID}>**\n<a:circle:847956594725486624>**Голосовой канал ─ <#{EventGoing.Channels.VoiceChannelID}>**\n<a:circle:847956594725486624>**Ивентер ─ <@{Target.Id}>**",
            };

            await Task.WhenAll(
                eventer.Save(),
                TargetEventer.Save(),
                SettingsChannel.RemovePermissionOverwriteAsync(command.User),
                SettingsChannel.AddPermissionOverwriteAsync(Target, new(viewChannel: PermValue.Allow)),
                SettingsChannel.SendMessageAsync(embed: EventEmbed.Build(), component: ManageEvent.Build()),
                SettingsMessage.DeleteAsync(),
                command.Respond($"Вы успешно передали ивент {Target}"));
        }

        private static async Task<IMessage> GetMessage(SocketTextChannel SettingsChannel, ulong MessageID)
        {
            var message = SettingsChannel.CachedMessages.FirstOrDefault(x => x.Id == MessageID);
            return message ?? (await SettingsChannel.GetMessageAsync(MessageID));
        }
        private static async Task HandleStartEventCommand(SocketSlashCommand command, Eventer eventer)
        {
            IEvent NewEvent = new()
            {
                Channels = new(),
                Messages = new(),
                EventReport = new(),
                Time = new()
                {
                    CreatedAT = DateTime.Now,
                }
            };
            eventer.Events.Add(NewEvent);
            try
            {
                var ListCategorys = new List<SelectMenuOptionBuilder>();
                var EventsCategory = await EventType.GetAll();
                if (EventsCategory is null || EventsCategory.Count == 0)
                {
                    await command.Respond("Нет доступных категории");
                    return;
                }
                for (int i = 0; i < EventsCategory.Count; i++)
                {
                    var cat = EventsCategory.ElementAt(i);
                    ListCategorys.Add(new SelectMenuOptionBuilder() { Label = cat.Category.ToString(), Description = cat.Text, Value = cat.ID.ToString() });
                }
                var TypeCategory = new ComponentBuilder()
                                       .WithSelectMenu(new SelectMenuBuilder()
                                       {
                                           CustomId = $"ChooseCategory-{NewEvent.ID}",
                                           Placeholder = "Категории",
                                           Options = ListCategorys,
                                       })
                                       .WithButton(new ButtonBuilder()
                                       {
                                           CustomId = $"CancelEventCreation-{NewEvent.ID}",
                                           Style = ButtonStyle.Danger,
                                           Label = "Отменить"
                                       });
                await command.FollowupAsync(embed: new CustomEmbedBuilder()
                {
                    Title = "Запуск Ивента",
                    Description = $"*Выберите категорию ивента.*",
                    ImageUrl = "https://cdn.discordapp.com/attachments/847851000815681536/868820301939638292/rY0VuT6.gif"
                }.Build(), component: TypeCategory.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //await command.Respond(ex.ToString());
            }
            finally
            {
                await eventer.Save();
            }

        }
        /// <summary>
        /// Request money, for an event
        /// </summary>
        /// <param name="command"></param>
        /// <param name="eventer"></param>
        /// <returns></returns>
        private static async Task HandleRequestCommand(SocketSlashCommand command, Eventer eventer)
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
                    Description = $@"| Запрос успешно отправлен"
                }.Build(), ephemeral: false);
                var msg = await (Client.GetMainGuild().GetChannel(StaticVars.RequestsChannel) as SocketTextChannel)
                    .SendMessageAsync(
                    embed: new CustomEmbedBuilder()
                    {
                        Title = "Поступил запрос",
                        Description = $@"> **От: <@{eventer.UserID}>**
                                         > **Количество: {Request.Amount}** <:Egold:802323778134081556> "
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
                RequestsHandler.Requests.Add(Request.ID, Request);
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
            var User = options.Length > 0 ? (SocketGuildUser)options[0].Value : command.User;
            var eventer = await Eventer.GetOrCreate(User.Id);
            if (eventer.RoleGivenAt == DateTime.MinValue)
            {
                var audits = await Client.GetMainGuild().GetAuditLogsAsync(100, actionType: ActionType.MemberRoleUpdated).FlattenAsync();
                var RoleInfo = audits.FirstOrDefault(x => x.Action == ActionType.MemberRoleUpdated && (x.Data as MemberRoleAuditLogData).Target.Id == User.Id && (x.Data as MemberRoleAuditLogData).Roles.Any(x => x.RoleId == StaticVars.PhoenixRole));
                if (RoleInfo != null)
                {
                    eventer.RoleGivenAt = new DateTime(RoleInfo.CreatedAt.Ticks);
                    await eventer.Save();
                }
            }
            var VoiceActivity = eventer.Events.Sum(x => x.Time.CountDiff);
            var WeekEvents = eventer.Events.Select(x => DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek) == x.Time.CreatedAT.AddDays(-(int)DateTime.Now.DayOfWeek)).Count();
            var TotalEvents = eventer.Events.Count;
            var EventerProfile = new CustomEmbedBuilder()
            {
                Title = $"Статистика ивентера ─ {User.Tag()}",
                ThumbnailUrl = User.GetAvatarUrl(size: 1024),
                Description = @$"```Кол-во дней в стаффе - {Math.Round((DateTime.Now - (eventer.RoleGivenAt == DateTime.MinValue ? eventer.CreateAt : eventer.RoleGivenAt)).TotalDays, 0, MidpointRounding.ToZero)}```",
                Fields = new()
                {
                    new()
                    {
                        Name = "<:rules:841955263585321001>Ранг:",
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
                        Value = $"```{eventer.Events.Select(x => DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek) == x.Time.CreatedAT.AddDays(-(int)DateTime.Now.DayOfWeek)).Count()}```",
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

            await command.FollowupAsync(embed: EventerProfile.Build(), ephemeral: false);
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
            if (eventer is null)
            {
                await SendError(command, "Не существует такого ид");
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
                Timestamp = System.DateTime.Now,
            };
            await command.FollowupAsync(embed: success.Build(), ephemeral: false);
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
                    Timestamp = DateTime.Now,
                };
                await command.FollowupAsync(embed: error.Build(), ephemeral: false);
            }

            eventer.Warns.Add(new()
            {
                Reason = Reason,
            });
            try
            {

            }
            catch (Exception)
            {
                var success = new CustomEmbedBuilder()
                {
                    Description = "Вы успешно выдали выговор",
                    Color = Color.Green,
                    Timestamp = DateTime.Now,
                };
                await (Client.GetMainGuild().GetChannel(StaticVars.WarnReportsChannel) as SocketTextChannel).SendMessageAsync(embed: new CustomEmbedBuilder()
                {
                    Title = "Выдан выговор",
                    Description = $"**Ивентёр: <@{eventer.ID}>**\n**Администартор: <@{command.User.Id}>**",
                    ThumbnailUrl = "https://cdn.discordapp.com/attachments/847850999397744663/848454662759776287/ban.png",
                    Fields = new()
                    {
                        new()
                        {
                            Name = "> Причина",
                            Value = $"```{Reason}```",
                            IsInline = true
                        },
                        new()
                        {
                            Name = "> Активных Выговоров",
                            Value = $"```{eventer.Warns.Count}```",
                            IsInline = true
                        }
                    }
                }.Build());
                await command.FollowupAsync(embed: success.Build(), ephemeral: false);
            }
            finally
            {
                await eventer.Save();

            }


        }
        private static Task<RestFollowupMessage> SendError(SocketSlashCommand command, string message = null)
        {
            var error = new CustomEmbedBuilder()
            {
                Description = message ?? "Неверное использование команды",
                Color = Color.Red,
                Timestamp = DateTime.Now,
            };
            return command.FollowupAsync(embed: error.Build(), ephemeral: false);
        }
        /// <summary>
        /// Report for an event
        /// </summary>
        /// <param name="command"></param>
        /// <param name="eventer"></param>
        /// <returns></returns>
        private static async Task HandleReportCommand(SocketSlashCommand command, Eventer eventer)
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
                Timestamp = DateTime.Now,
            };
            try
            {
                var category = await EventType.FindOne(x => x.ID == EndedEvent.EventCategoryID);
                var EventName = category.EventInfos.Find(x => x.ID == EndedEvent.EventTypeID);
                var Request = await CoinsRequest.FindOne(x => x.EventID == EndedEvent.ID);
                await (Client.GetMainGuild().GetChannel(StaticVars.EventReportsChannel) as ITextChannel).SendMessageAsync(embed: new CustomEmbedBuilder()
                {
                    Title = "ОТЧЕТНОСТЬ О ИВЕНТЕ!",
                    Description = $@"<:phoenix:848077530467532821>**Ведущий:** <@{eventer.UserID}>
                                    <:time:841952512060030977>**Время:** `[от {EndedEvent.Time.StartTime:t} до {EndedEvent.Time.EndTime:t}]`
                                    <:sound:841952512336068609>**Участвовало людей:** `[{EndedEvent.EventReport.UsersCount}]`
                                    <a:circle:847956594725486624>**Категория:** `[{category.Category}]`
                                    <a:circle:847956594725486624>**Ивент:** `[{EventName.Name}]`
                                    <a:circle:847956594725486624>**Количество кругов:** `[{EndedEvent.EventReport.RoundsCount}]`
                                    <a:circle:847956594725486624>**Вознаграждение:** `[{Request?.Amount.ToString() ?? "0"}]`<:Egold:802323778134081556>
                                    <a:circle:847956594725486624>**Гильдии | Пары:** `[{EndedEvent.EventReport.GuildMembersPresent}]`",
                    Timestamp = DateTime.Now,
                }.Build());
                await command.FollowupAsync(embed: SuccessEmbed.Build(), ephemeral: false);
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
