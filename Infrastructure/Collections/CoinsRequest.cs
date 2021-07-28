using System;
using System.Linq;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Freya.Infrastructure.Collections
{
    public class CoinsRequest : BaseCollection<CoinsRequest>
    {
        public CoinsRequest()
        {

        }
        public ulong UserID { get; set; }
        public ObjectId EventID { get; set; }
        public ulong MessageID { get; set; }
        public int Amount { get; set; }
        public bool Active { get; set; } = false;
        public bool Accepted { get; set; } = false;

        public DateTimeOffset CreateAT { get; set; } = DateTimeOffset.Now;
        public async Task Save()
        {
            var exists = Collection.Find(x => x.ID == ID);
            if (exists.Any())
                await Collection.ReplaceOneAsync(doc => doc.ID == ID, this, new ReplaceOptions { IsUpsert = true });
            else
                await Collection.InsertOneAsync(this);
        }
        //TODO maybe make it async/await so it awaits for it if it takes to much time to do it 
        public static async Task<CoinsRequest> GetOrCreate(string EventID)
        {
            var Result = Collection.Find((x) => x.EventID == new ObjectId(EventID));
            if (Result.Any())
                return Result.First();
            else
            {
                var NewClass = new CoinsRequest() { EventID = new ObjectId(EventID) };
                await NewClass.Save();
                return NewClass;
            }
        }
    }
}
