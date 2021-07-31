using Discord.Commands;

using Freya.Helpers.Precondition;
using Freya.Infrastructure.Mongo;

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
        [RequireRole(RolesType.EventMod | RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task StartEvent()
        {
            await Context.Channel.SendMessageAsync("blabla");

        }
        [Command("test", RunMode = RunMode.Async)]
        [RequireRole(RolesType.EventMod | RolesType.MaxPerms, ErrorMessage = "test")]
        [RequireContext(ContextType.Guild)]
        public async Task Test()
        {

            var maindb = MongoContext.MongoClient.GetDatabase("ethereal_main");
            var Filter = Builders<BsonDocument>.Filter.Empty;
            var Update = Builders<BsonDocument>.Update.Inc("gold", 100);
            try
            {
                var uri = "mongodb://localhost:27017";
                var client = new MongoClient(uri);
                var user = client.GetDatabase("ethereal_main").GetCollection<BsonDocument>("users").Find(Filter);
                var ss = user.FirstOrDefault();
                var users = await user.ToListAsync();
                foreach (var item in users)
                {
                    System.Console.WriteLine(item);
                }

            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw;
            }
            //user.UpdateOneAsync(Filter, Update);

        }
        public class test
        {
            public string userID { get; set; }
            public int gold { get; set; }
        }
    }
}
