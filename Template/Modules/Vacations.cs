using System;
using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Freya.Helpers.Interaction;
using Freya.Helpers.Precondition;
using Freya.Helpers.Util;
using Freya.Infrastructure.Collections;

using static Config;

namespace Freya.Modules
{
    public class Vacations : ModuleBase<SocketCommandContext>
    {
        private readonly Interaction Interaction;
        public Vacations(Interaction interaction)
        {
            Interaction = interaction;
        }
        [Command("отпуск", RunMode = RunMode.Async)]
        [Summary("Попросить отпуск")]
        [RequireRole(RolesType.EventMod | RolesType.MaxPerms, ErrorMessage = "Ошибка")]
        [RequireContext(ContextType.Guild)]
        public async Task VacationCommand()
        {
            var User = Context.User;
            var eventer = await Eventer.GetOrCreate(User.Id);
            var vacation = eventer.Vacations.FirstOrDefault(x => x.Active) ?? new();
            if (vacation.Active)
            {
                await Context.Channel.SendErrorMessage(ErrorMessage: "Вы уже в отпуске");
                return;
            }
            try
            {
                var Answers = await Interaction.Ask(Context.Message,
                    new string[]
                    { $"Укажите дату начало отпуска ({DateTimeOffset.Now:d})",
                      $"Укажите дату конеца отпуска ({DateTimeOffset.Now:d})",
                      "Краткое описание причины отпуска"
                    });
                string Reason = Answers[2];
                if (!DateTimeOffset.TryParse(Answers[0], out DateTimeOffset StartDate) || StartDate < DateTimeOffset.Now.Date)
                {
                    await Context.Channel.SendErrorMessage(ErrorMessage: "Некорректная дата");
                    return;
                }
                if (!DateTimeOffset.TryParse(Answers[1], out DateTimeOffset EndDate) || EndDate <= StartDate)
                {
                    await Context.Channel.SendErrorMessage(ErrorMessage: "Некорректная дата");
                    return;
                }
                if (string.IsNullOrEmpty(Reason))
                {
                    await Context.Channel.SendErrorMessage(ErrorMessage: "Некорректное описание");
                    return;
                }
                vacation.StartDate = StartDate;
                vacation.EndDate = EndDate;
                vacation.Reason = Reason;

                var embed = new CustomEmbedBuilder()
                {
                    Title = "Просьба Отпуска",
                    Description = @$"
                1.{User.Tag()}
                2.От:{StartDate:d} до:{EndDate:d}
                3.Причина: {Reason}"
                };
                var VacationChannel = Context.Guild.GetChannel(StaticVars.VacationChannel) as SocketTextChannel;
                await VacationChannel.SendMessageAsync(embed: embed.Build());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                eventer.Vacations.Add(vacation);
                await eventer.Save();
            }
        }
    }
}
