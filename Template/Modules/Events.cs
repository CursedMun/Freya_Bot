using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Freya.Helpers.Precondition;
using Freya.Helpers.Util;
using Freya.Infrastructure.Collections;

using static Config;
using static Freya.Infrastructure.Mongo.EventerExtension;

namespace Freya.Modules
{
    [Group("event")]
    public class Events : ModuleBase<SocketCommandContext>
    {
        public Events()
        {
        }
        [Command("start", RunMode = RunMode.Async)]
        [Summary("Запустить Ивент")]
        [RequireRole(RolesType.EventMod | RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task StartEvent()
        {
            var userID = Context.Message.Author.Id;
            var eventer = await Eventer.GetOrCreate(userID);
            IEvent NewEvent = new()
            {
                Channels = new(),
                EventReport = new(),
                Time = new()
                {
                    CreatedAT = DateTimeOffset.Now,
                }
            };
            eventer.Events.Add(NewEvent);
            try
            {
                var ListCategorys = new List<SelectMenuOptionBuilder>();
                var EventsCategory = await EventType.GetAll();
                if (EventsCategory is null || EventsCategory.Count == 0)
                {
                    await Context.Channel.SendErrorMessage(ErrorMessage: "Нет доступных категории");
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
                await Context.Channel.SendMessageAsync(embed: new CustomEmbedBuilder()
                {
                    Title = "Запуск Ивента",
                    Description = $"*Выберите категорию ивента.*",
                    ImageUrl = "https://cdn.discordapp.com/attachments/847851000815681536/868820301939638292/rY0VuT6.gif"
                }.Build(), component: TypeCategory.Build());
            }
            catch (Exception ex)
            {
                await Context.Channel.SendErrorMessage(ex);
            }
            finally
            {
                await eventer.Save();
            }

        }
        [Command("test", RunMode = RunMode.Async)]
        [RequireRole(RolesType.EventMod | RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public Task Test()
        {
            return Task.CompletedTask;
        }
    }
}
