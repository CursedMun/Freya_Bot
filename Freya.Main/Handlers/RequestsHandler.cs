﻿using Discord.WebSocket;

using Freya.Infrastructure.Collections;
using Freya.Services;

using MongoDB.Bson;
using MongoDB.Driver;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Config;

namespace Freya.Handler
{
    public class RequestsHandler : DiscordHandler
    {
        private DiscordSocketClient Client;
        private readonly ulong[] RolesAdmin = TypeRoles.GetValueOrDefault((int)RolesType.MaxPerms).PushArray(TypeRoles.GetValueOrDefault((int)RolesType.Ogma));
        public static Dictionary<ObjectId, CoinsRequest> Requests { get; set; } = new();
        public async override void Initialize(DiscordSocketClient client)
        {
            Client = client;
            await ManageRequest();
            Client.InteractionCreated += Client_InteractionCreated;
        }
        private async Task ManageRequest()
        {
            try
            {
                var reqs = (await CoinsRequest.Find(x => x.Active == true)).ToList();
                var RequestsChannel = Client.GetMainGuild().GetChannel(StaticVars.RequestsChannel) as SocketTextChannel;
                for (int i = 0; i < reqs.Count; i++)
                {
                    var req = reqs.ElementAt(i);
                    await RequestsChannel.GetMessageAsync(req.MessageID);
                    Requests.Add(req.ID, req);
                }
            }
            catch (Exception ex)
            {
                var json = JsonConvert.SerializeObject(ex.Message, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        private async Task Client_InteractionCreated(SocketInteraction arg)
        {
            if (arg is not SocketMessageComponent component) return;
            await ManageRequest();
            switch (component.Data.CustomId)
            {
                case string a when a.Contains("ConfirmRequest"):
                    {
                        var requestID = component.Data.CustomId.Split("-")[1];
                        await AcceptRequestHandler(component, requestID);
                        break;
                    }
                case string a when a.Contains("CancelRequest"):
                    {
                        var requestID = component.Data.CustomId.Split("-")[1];
                        await RejectRequestHandler(component, requestID);
                        break;
                    }
                default:
                    break;
            }
        }

        private async Task RejectRequestHandler(SocketMessageComponent Component, string requestID)
        {
            if (Requests.TryGetValue(new ObjectId(requestID), out var Request))
            {
                var user = Component.User as SocketGuildUser;
                if (!user.Roles.Any(r => RolesAdmin.Any(s => r.Id == s))) return;
                Request.Accepted = false;
                Request.Active = false;
                await Component.Message.ModifyAsync(x => { x.Content = $"Отклонил <@{Component.User.Id}>"; x.Components = null; });
                await Request.Save();
                await Component.Respond("Не выдал");
            }
        }

        private async Task AcceptRequestHandler(SocketMessageComponent Component, string requestID)
        {
            if (Requests.TryGetValue(new ObjectId(requestID), out var Request))
            {
                var user = Component.User as SocketGuildUser;
                if (!user.Roles.Any(r => RolesAdmin.Any(s => r.Id == s))) return;
                Request.Accepted = true;
                Request.Active = false;
                //TODO FIX IT SO IT CAN AWARD USER's
                var Filter = Builders<BsonDocument>.Filter.Eq("userID", Request.UserID.ToString());
                var Update = Builders<BsonDocument>.Update.Inc("gold", Request.Amount);
                try
                {
                    var uri = "mongodb://localhost:27017";
                    var client = new MongoClient(uri);
                    await client.GetDatabase("ethereal_main").GetCollection<BsonDocument>("users").UpdateOneAsync(Filter, Update);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                await Component.Message.ModifyAsync(x =>
                {
                    x.Components = null;
                    x.Content = $"Принял <@{Component.User.Id}>";
                });
                await Request.Save();
                await Component.Respond("Выдал");
            }
        }
        #region ReactionAdded
        //private async Task OnReactionAdd(Discord.Cacheable<Discord.IUserMessage, ulong> User, Discord.Cacheable<Discord.IMessageChannel, ulong> Message, SocketReaction Reaction)
        //{
        //    if (HandlerService.Requests.TryGetValue(Message.Id, out var Request))
        //    {
        //        var user = await User.DownloadAsync();
        //        var member = Client.GetMainGuild().GetUser(user.Id);
        //        if (!member.Roles.Any(r => RolesAdmin.Any(s => r.Id == s))) return;
        //        switch (Reaction.Emote.Name)
        //        {
        //            case StaticVars.Emoji.AcceptEmoji:
        //            {
        //                await AcceptRequestHandler(Request);
        //                break;
        //            }
        //            case StaticVars.Emoji.DeclineEmoji:
        //            {
        //                RequestHandler();
        //                break;
        //            }
        //        }
        //    }
        //}
        #endregion

    }
}
