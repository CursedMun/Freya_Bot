
using Discord;
using Discord.Rest;
using Discord.WebSocket;

using Freya.Helpers.Util;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    public static ComponentBuilder ToComponentBuilder(this IReadOnlyCollection<IMessageComponent> components)
    {
        if (components.Count == 0) return null;

        var builder = new ComponentBuilder
        {
            ActionRows = new()
            {
                new ActionRowBuilder()
                {
                    Components = components.ToList()
                }
            }
        };
        return builder;
    }
    public static SocketGuild GetMainGuild(this DiscordSocketClient Client)
    {
        return Client.GetGuild(Config.StaticVars.MainGuild);
    }
    public static SocketGuild GetAdminGuild(this DiscordSocketClient Client)
    {
        return Client.GetGuild(Config.StaticVars.AdminGuild);
    }
    public static string Tag(this SocketUser User)
    {
        return $"{User.Username}#{User.Discriminator}";
    }
    public static async Task Respond(this SocketMessageComponent component, string message = "Вы не можете это сделать")
    {
        await component.FollowupAsync(embed:
            new CustomEmbedBuilder()
            {
                Author = new()
                {
                    Name = $"| {message}",
                    IconUrl = component.User.GetAvatarUrl()
                }
            }.Build(), ephemeral: true);
    }
    public static async Task Respond(this SocketSlashCommand component, string message = "Вы не можете это сделать")
    {
        await component.FollowupAsync(embed:
            new CustomEmbedBuilder()
            {
                Author = new()
                {
                    Name = $"| {message}",
                    IconUrl = component.User.GetAvatarUrl()
                }
            }.Build(), ephemeral: true);
    }
    public static async Task<RestUserMessage> SendErrorMessage(this ISocketMessageChannel Channel, Exception exception = null, string ErrorMessage = "Произошла ошибка", int Timeout = 10000)
    {
        var ErrorEmbed = new CustomEmbedBuilder()
        {
            Color = Color.Red,
            Author = new() { Name = $"| {ErrorMessage + "\n" + exception?.Message}" }
        }.Build();
        var msg = await Channel.SendMessageAsync(embed: ErrorEmbed);
        await Task.Delay(Timeout);
        await msg.DeleteAsync();
        return msg;
    }
    public static async Task<RestUserMessage> SendSuccessMessage(this ISocketMessageChannel Channel, string SuccesMessage = "Успешно выполнил", int Timeout = 10000)
    {
        var SuccessEmbed = new CustomEmbedBuilder()
        {
            Color = Color.Green,
            Author = new() { Name = $"| {SuccesMessage}" }
        }.Build();
        var msg = await Channel.SendMessageAsync(embed: SuccessEmbed);
        await Task.Delay(Timeout);
        await msg.DeleteAsync();
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
