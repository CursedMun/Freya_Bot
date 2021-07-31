using Discord;
using Discord.WebSocket;

using Freya.Helpers.Util;
using Freya.Infrastructure.Collections;
using Freya.Services;

using MongoDB.Bson;
using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Config;
using static Freya.Infrastructure.Mongo.EventerExtension;

namespace Freya.Helpers.Handler
{
    class EventHandler : DiscordHandler
    {
        private DiscordSocketClient Client;

        public override void Initialize(DiscordSocketClient client)
        {
            this.Client = client;

            Client.InteractionCreated += Client_InteractionCreated;
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            if (arg is not SocketMessageComponent component)
                return;
            var eventer = await Eventer.FindOne(x => x.UserID == component.User.Id);
            if (eventer is null)
            {
                return;
            }
            await component.AcknowledgeAsync();
            switch (component.Data.CustomId)
            {
                case string a when a.Contains("Start"):
                    {
                        var eventID = component.Data.CustomId.Split("-")[1];
                        await StartEvent(component, eventID, eventer);
                        break;
                    }
                case string a when a.Contains("SendNews"):
                    {
                        var eventID = component.Data.CustomId.Split("-")[1];
                        await SendNews(component, eventID, eventer);
                        break;
                    }
                case string a when a.Contains("ChooseCategory"):
                    {
                        if (component.Data.Values is null || component.Data.Values.Count < 0) break;
                        var items = component.Data.CustomId.Split("-");
                        var catID = new ObjectId(component.Data.Values.First());
                        await component.Message.DeleteAsync();
                        await GenerateTypeOfEvent(component, catID, items, eventer);
                        break;
                    }
                case string a when a.Contains("ChooseEvent"):
                    {
                        if (component.Data.Values is null || component.Data.Values.Count < 0) break;
                        var items = component.Data.CustomId.Split("-");
                        var typeID = component.Data.Values.First().ToString();
                        await component.Message.DeleteAsync();
                        await GenerateLastMessage(component, typeID, items, eventer);
                        break;
                    }
                case string a when a.Contains("CancelEventCreation"):
                    await CancelEvent(component, eventer);
                    break;
                case string a when a.Contains("EndEvent"):
                    await EndEvent(component, eventer);
                    break;
                default:
                    break;
            }
        }
        private async Task StartEvent(SocketMessageComponent component, string eventID, Eventer eventer)
        {
            var EventGoing = eventer.Events.FirstOrDefault(x => x.ID == eventID);
            if (EventGoing is null) return;
            var category = await EventType.FindOne(x => x.ID == EventGoing.EventCategoryID);
            if (category is null) return;
            var EventName = category.EventInfos.Find(x => x.ID == EventGoing.EventTypeID);
            EventGoing.Time.StartTime = DateTime.Now;
            try
            {
                var TextEvent = await Client.GetMainGuild().CreateTextChannelAsync($"🎯・{EventName.Name}", x =>
                {
                    x.CategoryId = StaticVars.EventChannelsCategory;
                    x.PermissionOverwrites = new List<Overwrite>()
                    {
                        new(StaticVars.EventBan,PermissionTarget.Role,new(addReactions: PermValue.Deny,sendMessages:PermValue.Deny)),
                        new(StaticVars.ChatMute,PermissionTarget.Role,new(addReactions: PermValue.Deny,sendMessages:PermValue.Deny)),
                        new(StaticVars.PhoenixRole,PermissionTarget.Role,new(PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow)),
                        new(Client.GetMainGuild().EveryoneRole.Id,PermissionTarget.Role,new(mentionEveryone:PermValue.Deny)),
                    };
                });
                var TextManageEvent = await Client.GetMainGuild().CreateTextChannelAsync($"📢・управление", x =>
                {
                    x.CategoryId = StaticVars.EventChannelsCategory;
                    x.PermissionOverwrites = new List<Overwrite>()
                    {
                        new(component.User.Id,PermissionTarget.User, new(viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow)),
                        new(Client.GetMainGuild().EveryoneRole.Id,PermissionTarget.Role, new(viewChannel: PermValue.Deny, readMessageHistory: PermValue.Deny)),
                    };
                });
                var ChannelEvent = await Client.GetMainGuild().CreateVoiceChannelAsync($"🔊・{EventName.Name}", x =>
                {
                    x.CategoryId = StaticVars.EventChannelsCategory;
                    x.PermissionOverwrites = new List<Overwrite>()
                    {
                        new(StaticVars.EventBan,PermissionTarget.Role,new(connect: PermValue.Deny)),
                        new(StaticVars.PhoenixRole,PermissionTarget.Role,new(PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow,PermValue.Allow)),
                        new(StaticVars.Mute,PermissionTarget.Role,new(connect:PermValue.Deny)),
                    };
                });
                EventGoing.Channels.VoiceChannelID = ChannelEvent.Id;
                EventGoing.Channels.TextChannelID = TextEvent.Id;
                EventGoing.Channels.SettingsChannelID = TextManageEvent.Id;

                var FinishEvent = new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                {
                    CustomId = $"SendNews-{eventID}",
                    Style = ButtonStyle.Success,
                    Label = "Отправить Анонс?"
                })
                .WithButton(new ButtonBuilder()
                {
                    CustomId = $"EndEvent-{eventID}",
                    Style = ButtonStyle.Danger,
                    Label = "Закончить ивент"
                });

                CustomEmbedBuilder EventEmbed = new()
                {
                    Title = $"Информация о столе",
                    ThumbnailUrl = component.User.GetAvatarUrl(),
                    ImageUrl = "https://cdn.discordapp.com/attachments/847851000815681536/868820301939638292/rY0VuT6.gif",
                    Description = $"<:circle:855370914841231371>**Категория: {category.Category}**\n<:circle:855370914841231371>**Ивент: {EventName.Name}** \n<:circle:855370993433444383>**Время проведения: {EventGoing.Time.StartTime.ToString("M/d/yyyy HH:mm")} **\n<a:circle:847956594725486624>**Канал управления ─ <#{TextManageEvent.Id}>**\n<a:circle:847956594725486624>**Голосовой канал ─ <#{ChannelEvent.Id}>**\n<a:circle:847956594725486624>**Ивентер ─ <@{eventer.UserID}>**",
                };
                await component.Message.DeleteAsync();
                await TextManageEvent.SendMessageAsync(embed: EventEmbed.Build(), component: FinishEvent.Build());
                CustomEmbedBuilder SuccessEmbed = new()
                {
                    Description = $@"| Ивент успешно запущен
                                     | Канал управления: <@{TextManageEvent.Id}>"
                };
                await component.FollowupAsync(embed: SuccessEmbed.Build(), ephemeral: true);
                return;
            }
            catch (Exception ex)
            {
                await component.Message.Channel.SendErrorMessage(ex);
            }
            finally
            {
                await eventer.Save();
            }



        }
        private async Task SendNews(SocketMessageComponent component, string eventID, Eventer eventer)
        {
            SocketTextChannel NewsChannel = (SocketTextChannel)await Client.GetChannelAsync(StaticVars.NewsChannel);
            if (NewsChannel is null)
            {
                await component.Respond("Не нашёл канал");
                return;
            }
            var GoingEvent = eventer.Events.FirstOrDefault(x => x.ID == eventID);
            if (GoingEvent is null)
            {
                await component.Respond();
                return;
            }
            var eventType = await EventType.FindOne(x => x.ID == GoingEvent.EventCategoryID);
            var embed = eventType.EventInfos.Find(x => x.ID == GoingEvent.EventTypeID).Embed;
            List<EmbedFieldBuilder> Fields = new();
            for (int i = 0; i < embed.Fields.Length; i++)
            {
                var field = embed.Fields.ElementAt(i);
                Fields.Add(new()
                {
                    Name = field.Name,
                    IsInline = field.Inline,
                    Value = field.Value
                });
            }
            try
            {
                var embedToSend = new EmbedBuilder()
                {
                    Title = embed.Title,
                    Description = await GenerateDescription(embed, GoingEvent, eventer),
                    Color = new Color(embed.Color),
                    Fields = Fields,
                    ImageUrl = embed.Image.ToString(),
                };
                await NewsChannel.SendMessageAsync(embed.PlainText, embed: embedToSend.Build());
                await component.Message.ModifyAsync(x =>
                {
                    x.Components = new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                {
                    CustomId = $"EndEvent-{eventID}",
                    Style = ButtonStyle.Danger,
                    Label = "Закончить ивент"
                }).Build();
                });
                await component.Respond("Успех");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private async Task<string> GenerateDescription(Infrastructure.Collections.Embed embed, IEvent GoingEvent, Eventer eventer)
        {
            var str = embed.Description
                .Replace("DateNow", DateTime.Now.ToString("HH:mm"))
                .Replace("Channel", (await Client.GetMainGuild().GetVoiceChannel(GoingEvent.Channels.VoiceChannelID).CreateInviteAsync()).Url)
                .Replace("Eventer", $"<@{eventer.UserID}>");
            return str;
        }

        private static async Task GenerateTypeOfEvent(SocketMessageComponent component, ObjectId catID, string[] info, Eventer eventer)
        {
            var eventID = info[1];
            var cat = await EventType.FindOne(x => x.ID == catID);
            if (cat is null) return;
            var Event = eventer.Events.FirstOrDefault(x => x.ID == eventID);
            if (Event is null)
            {
                await component.Respond("Неверный ивент");
                return;
            }
            Event.EventCategoryID = catID;
            try
            {
                var ListOptions = new List<SelectMenuOptionBuilder>();
                for (int i = 0; i < cat.EventInfos.Count; i++)
                {
                    var item = cat.EventInfos.ElementAt(i);
                    ListOptions.Add(new SelectMenuOptionBuilder() { Label = item.Name, Value = item.ID });
                }

                var TypeEvents = new ComponentBuilder()
                    .WithSelectMenu(new SelectMenuBuilder()
                    {
                        CustomId = $"ChooseEvent-{eventID}",
                        Placeholder = "Ивенты",
                        Options = ListOptions
                    })
                    .WithButton(new ButtonBuilder()
                    {
                        CustomId = $"CancelEventCreation-{eventID}",
                        Style = ButtonStyle.Danger,
                        Label = "Отменить"
                    });
                await component.Message.Channel.SendMessageAsync(embed: new CustomEmbedBuilder()
                {
                    Title = "Запуск Ивента",
                    Description = "*Выберите ивент.*",
                    ImageUrl = "https://cdn.discordapp.com/attachments/847851000815681536/868820301939638292/rY0VuT6.gif"
                }.Build(), component: TypeEvents.Build());
            }
            catch (Exception ex)
            {
                await component.Message.Channel.SendErrorMessage(ex);
            }
            finally
            {
                await eventer.Save();
            }

        }
        private static async Task GenerateLastMessage(SocketMessageComponent component, string typeID, string[] info, Eventer eventer)
        {
            var eventID = info[1];
            var EventGoing = eventer.Events.FirstOrDefault(x => x.ID == eventID);
            if (EventGoing is null) return;
            var cat = await EventType.FindOne(x => x.ID == EventGoing.EventCategoryID);
            if (cat is null) return;

            var EventName = cat.EventInfos.Find(x => x.ID == typeID);
            CustomEmbedBuilder embed = new()
            {
                Title = $"Информация",
                Description = $"> <a:circle:847956594725486624>**Категория: {cat.Category}**\n> <a:circle:847956594725486624>**Название ивента: {EventName.Name}**",
                ThumbnailUrl = "https://cdn.discordapp.com/attachments/625608366537965599/826519698732351488/bbbbbbbbbbbbbbbbb.png",
                ImageUrl = "https://cdn.discordapp.com/attachments/847851000815681536/868820301939638292/rY0VuT6.gif"
            };
            try
            {
                var ExecEvent = new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                {
                    CustomId = $"Start-{eventID}",
                    Style = ButtonStyle.Success,
                    Label = "Запустить?"
                })
                .WithButton(new ButtonBuilder()
                {
                    CustomId = $"CancelEventCreation-{eventID}",
                    Style = ButtonStyle.Danger,
                    Label = "Отменить"
                });

                EventGoing.EventTypeID = typeID;
                await component.Message.Channel.SendMessageAsync(embed: embed.Build(), component: ExecEvent.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                await eventer.Save();
            }
        }
        private async Task EndEvent(SocketMessageComponent component, Eventer eventer)
        {
            var eventID = component.Data.CustomId.Split("-")[1];
            var EventGoing = eventer.Events.FirstOrDefault(x => x.ID == eventID);
            if (EventGoing is null) return;
            CustomEmbedBuilder EndEmbed = new()
            {
                Title = $"Отчётность!",
                Description = $"> **__Напишите отчёт след. образом:/report eventid:{eventID}__**\n\n\n> **__Так-же вы можете попросить денег /request eventid:{eventID}__**",
                Footer = new()
                {
                    Text = "В противном случае результат не будет записан в вашу статистику"
                },
                ImageUrl = "https://cdn.discordapp.com/attachments/847851000815681536/868820301939638292/rY0VuT6.gif",
                ThumbnailUrl = component.User.GetAvatarUrl()
            };
            try
            {
                EventGoing.Time.EndTime = DateTime.Now;
                EventGoing.Finished = true;
                await Client.GetMainGuild().GetChannel(EventGoing.Channels.TextChannelID).DeleteAsync();
                await Client.GetMainGuild().GetChannel(EventGoing.Channels.VoiceChannelID).DeleteAsync();
                await Client.GetMainGuild().GetChannel(EventGoing.Channels.SettingsChannelID).DeleteAsync();
                await component.User.SendMessageAsync(embed: EndEmbed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                await eventer.Save();

            }

        }
        private static async Task CancelEvent(SocketMessageComponent component, Eventer eventer)
        {
            var eventID = component.Data.CustomId.Split("-")[1];
            await component.Message.DeleteAsync(new() { AuditLogReason = "Canceled by the user" });
            await DeleteEvent(eventID, eventer);
        }
        private static async Task DeleteEvent(string eventID, Eventer eventer)
        {
            var Event = eventer.Events.FirstOrDefault(x => x.ID == eventID);
            if (Event is not null)
            {
                eventer.Events.Remove(Event);
                await eventer.Save();
            }
        }
    }
}
