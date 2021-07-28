
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

namespace Freya.Infrastructure.Mongo
{
    public class MongoContext : IHostedService
    {
        private static IMongoDatabase mongoDatabase;

        public static IMongoDatabase MongoDatabase { get => mongoDatabase; set => mongoDatabase = value; }

        public MongoContext(IMongoClient mongo)
        {
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
