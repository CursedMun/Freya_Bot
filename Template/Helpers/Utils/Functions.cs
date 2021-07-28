
using System;
using System.Threading.Tasks;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Freya.Helpers.Util;

public static class Functions
{
    public static IChannel TryGetChannel(this SocketGuild SocketGuild, ulong channel)
    {
        try
        {
            return SocketGuild.GetChannel(channel);
        }
        catch
        {
            return null;
        }
    }
    public static SocketGuild MainGuild { get; private set; }
    public static SocketGuild GetMainGuild(this DiscordSocketClient Client)
    {
        return MainGuild ??= Client.GetGuild(Config.StaticVars.MainGuild);
    }
    public static string Tag(this SocketUser User)
    {
        return $"{User.Username}#{User.Discriminator}";
    }
    public static async Task RespondError(this SocketMessageComponent component, string message = "Вы не можете это сделать")
    {
        await component.RespondAsync(embed: new CustomEmbedBuilder() { Author = new() { Name = $"| {message}", IconUrl = component.User.GetAvatarUrl() } }.Build(), ephemeral: true);
    }
    public static async Task<RestUserMessage> SendErrorMessage(this ISocketMessageChannel Channel, Exception exception = null, string ErrorMessage = "Произошла ошибка", int Timeout = 10000)
    {
        var ErrorEmbed = new CustomEmbedBuilder()
        {
            Title = "Ошибка",
            Color = Color.Red,
            Description = ErrorMessage + "\n" + exception?.Message,
            Timestamp = DateTimeOffset.Now
        }.Build();
        var msg = await Channel.SendMessageAsync(embed: ErrorEmbed);
        await msg.DeleteAsync(new() { Timeout = Timeout });
        return msg;
    }
    public static T[] Push<T>(this T[] source, T value)
    {
        var index = source.Length + 1;
        Array.Resize(ref source, index);
        if (index != -1)
        {
            source[index - 1] = value;
        }
        return source;
    }
    public static T[] PushArray<T>(this T[] source, T[] value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            source = source.Push(value[i]);
        }
        return source;
    }
}
