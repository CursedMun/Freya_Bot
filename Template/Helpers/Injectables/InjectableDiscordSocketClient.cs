
using Discord.WebSocket;

using Freya.Helpers.Services;

using Microsoft.Extensions.Options;
namespace Freya.Helpers.Injectables
{
    internal class InjectableDiscordSocketClient : DiscordSocketClient
    {
        public InjectableDiscordSocketClient(IOptions<DiscordHostConfiguration> config) : base(config.Value.SocketConfig)
        {
            this.RegisterSocketClientReady();
        }
    }

}
