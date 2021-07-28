using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Net;
using Discord.WebSocket;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Freya.Services
{
    public class HandlerService : IHostedService
    {
        private readonly DiscordSocketClient client;
        private static readonly Dictionary<DiscordHandler, object> Handlers = new();

        /// <summary>
        /// Gets a handler with the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the handler to get.</typeparam>
        /// <returns>The handler with the type of <typeparamref name="T"/>. If no handler is found then <see langword="null"/>.</returns>
        public static T GetHandlerInstance<T>()
            where T : DiscordHandler => Handlers.FirstOrDefault(x => x.Key.GetType() == typeof(T)).Value as T;
        private readonly ILogger<HandlerService> _logger;
        public HandlerService(DiscordSocketClient client, ILogger<HandlerService> logger)
        {
            this.client = client;
            _logger = logger;
        }

        private async Task Client_Ready()
        {
            await GenerateSlashCommands();

            foreach (var item in Handlers)
            {
                try
                {
                    await item.Key.InitializeAsync(client);
                    item.Key.Initialize(client);
                }
                catch (Exception x)
                {
                    Console.Error.WriteLine($"Exception occured while initializing {item.Key.GetType().Name}: ", x);
                }
            }

        }



        private async Task GenerateSlashCommands()
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
                var WarnCommand = new SlashCommandBuilder()
                {
                    Name = "warn",
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
                await client.Rest.CreateGuildCommand(ReportCommand.Build(), Config.StaticVars.MainGuild);
                await client.Rest.CreateGuildCommand(WarnCommand.Build(), Config.StaticVars.MainGuild);
                await client.Rest.CreateGuildCommand(UnWarnCommand.Build(), Config.StaticVars.MainGuild);
                await client.Rest.CreateGuildCommand(ProfileCommand.Build(), Config.StaticVars.MainGuild);
                await client.Rest.CreateGuildCommand(RequestCommand.Build(), Config.StaticVars.MainGuild);
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            this.client.Ready += Client_Ready;
            List<Type> typs = new();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes())
                    if (type.IsAssignableTo(typeof(DiscordHandler)) && type != typeof(DiscordHandler))
                        typs.Add(type);

            foreach (var handler in typs)
            {
                var inst = Activator.CreateInstance(handler);
                Handlers.Add(inst as DiscordHandler, inst);
            }

            _logger.LogInformation($"Handler service <Green>Initialized</Green>! {Handlers.Count} handlers created!");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    ///     Marks the current class as a handler.
    /// </summary>
    public abstract class DiscordHandler
    {
        /// <summary>
        ///     Intitialized this handler asynchronously.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <returns>A task representing the asynchronous operation of initializing this handler.</returns>
        public virtual Task InitializeAsync(DiscordSocketClient client)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Intitialized this handler.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        public virtual void Initialize(DiscordSocketClient client)
        {
        }
    }
}
