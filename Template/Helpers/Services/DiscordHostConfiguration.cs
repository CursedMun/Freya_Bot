using System;

using Discord;
using Discord.WebSocket;

namespace Freya.Helpers.Services
{
    public class DiscordHostConfiguration
    {

        /// <summary>
        /// The bots token.
        /// </summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// Sets a custom output format for logs coming from Discord.NET's integrated logger.
        /// </summary>
        /// <remarks>
        /// The default simply concatenates the message source with the log message.
        /// </remarks>
        public Func<LogMessage, Exception, string> LogFormat { get; set; } = (message, exception) => $"{message.Source}: {message.Message}";

        /// <inheritdoc cref="DiscordSocketConfig"/>
        public DiscordSocketConfig SocketConfig { get; set; } = new DiscordSocketConfig();
    }
}
