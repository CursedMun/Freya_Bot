using Discord.Commands;
using Discord.WebSocket;

using Freya.Helpers.Interaction;
using Freya.Helpers.Precondition;
using Freya.Infrastructure.Collections;

using Interactivity;
using Interactivity.Pagination;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Config;

namespace Freya.Modules
{
    public class Warns : ModuleBase<SocketCommandContext>
    {
        private Interaction Interaction { get; set; }
        public Warns(Interaction interaction)
        {
            Interaction = interaction;
        }
        [Command("warnlist", RunMode = RunMode.Async)]
        [Summary("Выговоры ивентера")]
        [RequireRole(RolesType.EventMod | RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public Task WarnListCommand(SocketUser user = null)
        {
            var User = user ?? Context.User;
            var eventer = Eventer.FindOne(x => x.UserID == User.Id).Result;
            if (eventer is default(Eventer))
                return Context.Channel.SendErrorMessage(ErrorMessage: "Не существует такого ивентера");

            var pages = GeneratePages(eventer);
            var paginator = new StaticPaginatorBuilder()
            {
                Users = new() { User },
                Pages = pages,
                Footer = PaginatorFooter.PageNumber | PaginatorFooter.Users,
                Deletion = DeletionOptions.AfterCapturedContext
            };
            return Interaction.Interactivity.SendPaginatorAsync(paginator.Build(), Context.Channel, TimeSpan.FromMinutes(2));
        }
        private static List<PageBuilder> GeneratePages(Eventer eventer)
        {
            var pages = new List<PageBuilder>();
            if (eventer.Warns.Count < 1)
            {
                PageBuilder pageBuilder = new() { Title = $"Выговоры || стр.0 из 0", Description = "Нет выговоров" };
                pages.Add(pageBuilder);
                return pages;
            }
            var pagesCount = Math.Ceiling((eventer.Warns.Count / 5m));
            var warns = Batch(eventer.Warns, 5).ToList();
            for (int i = 0; i < pagesCount; i++)
            {
                var item = warns.ElementAt(i);
                PageBuilder pageBuilder = new() { Title = $"Выговоры || стр.{i + 1} из {pagesCount}" };
                for (int x = 0; x < item.Count(); x++)
                {
                    var it = item.ElementAt(x);
                    pageBuilder.Description += @$"
                    -------------------
                    ид:{it.ID}
                    причина:{it.Reason}
                    когда:{it.CreateAt}
                    -------------------";
                }
                pages.Add(pageBuilder);
            }
            return pages;
        }
        public static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> items, int maxItems)
        {
            return items.Select((item, index) => new { item, index })
                        .GroupBy(x => x.index / maxItems)
                        .Select(g => g.Select(x => x.item));
        }
    }
}
