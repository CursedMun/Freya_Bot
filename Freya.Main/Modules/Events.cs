using Discord.Commands;

using Freya.Helpers.Precondition;

using MongoDB.Bson;

using MongoDB.Driver;

using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static Config;

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
        [RequireRole(RolesType.EventMod | RolesType.MaxPerms | RolesType.Ogma, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task StartEvent()
        {
            await Context.Channel.SendMessageAsync("blabla");

        }
        [Command("test", RunMode = RunMode.Async)]
        [RequireRole(RolesType.Ogma, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task Test(int amount)
        {

            //var maindb = MongoContext.MongoClient.GetDatabase("ethereal_main");
            var Filter = Builders<BsonDocument>.Filter.Eq("userID", Context.User.Id.ToString());
            var Update = Builders<BsonDocument>.Update.Inc("gold", amount);
            try
            {
                var uri = "mongodb://localhost:27017";
                var client = new MongoClient(uri);
                var user = await client.GetDatabase("ethereal_main").GetCollection<BsonDocument>("users").UpdateOneAsync(Filter, Update);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw;
            }
            //user.UpdateOneAsync(Filter, Update);
        }
    }
}
