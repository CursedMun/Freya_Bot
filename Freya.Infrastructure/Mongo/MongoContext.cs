
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using System.Threading;
using System.Threading.Tasks;

namespace Freya.Infrastructure.Mongo
{
    public class MongoContext : IHostedService
    {
        private static IMongoDatabase mongoDatabase;
        private static IMongoClient client;
        public static IMongoDatabase MongoDatabase { get => mongoDatabase; set => mongoDatabase = value; }
        public static IMongoClient MongoClient { get => client; set => client = value; }

        public MongoContext(IMongoClient mongo)
        {
            MongoClient = mongo;
            MongoDatabase = mongo.GetDatabase("Ethereal");
        }
        public MongoContext()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
