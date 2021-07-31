using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Freya.Helpers.Injectables;
using Freya.Helpers.Interaction;
using Freya.Helpers.Services;
using Freya.Infrastructure.Mongo;
using Freya.Services;

using Interactivity;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

using System;
using System.IO;
using System.Threading.Tasks;

namespace Freya
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .AddEnvironmentVariables()
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug); // Defines what kind of information should be logged (e.g. Debug, Information, Warning, Critical) adjust this to your liking
                })
                //Client
                .ConfigureServices((context, collection) =>
                {
                    collection.AddOptions<DiscordSocketConfig>();

                    collection.Configure<DiscordHostConfiguration>(x =>
                    {
                        x.SocketConfig = new DiscordSocketConfig
                        {

                            LogLevel = LogSeverity.Verbose, // Defines what kind of information should be logged from the API (e.g. Verbose, Info, Warning, Critical) adjust this to your liking
                            AlwaysDownloadUsers = false,
                            AlwaysAcknowledgeInteractions = false,
                            MessageCacheSize = 200,
                        };
                        x.Token = context.Configuration["token"];
                    });

                    collection.AddSingleton(typeof(LogAdapter<>));
                    collection.AddHostedService<DiscordHostedService<DiscordSocketClient>>();
                    collection.AddSingleton<DiscordSocketClient, InjectableDiscordSocketClient>();
                })
                //Commands
                .ConfigureServices((context, collection) =>
                {
                    collection.Configure<CommandServiceConfig>(x =>
                    {
                        x.CaseSensitiveCommands = false;
                        x.LogLevel = LogSeverity.Verbose;
                        x.DefaultRunMode = RunMode.Sync;
                    });

                    collection.AddSingleton(x => new CommandService(x.GetRequiredService<IOptions<CommandServiceConfig>>().Value));
                    collection.AddHostedService<CommandServiceRegistrationHost>();
                    collection.AddHostedService<CommandHandler>();
                })
                //Mongo
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<HandlerService>()
                    .AddSingleton<IMongoClient, MongoClient>(s =>
                     {
                         var uri = context.Configuration["mongoUri"];
                         return new MongoClient(uri);
                     })
                    .AddHostedService<MongoContext>();
                })
                //Interactivity
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddSingleton<InteractivityService>()
                    .AddSingleton(new InteractivityConfig { DefaultTimeout = TimeSpan.FromSeconds(20) })
                    .AddSingleton<Interaction>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
