using MongoDB.Driver;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Freya.Infrastructure.Collections
{
    public class CoinsRequest : BaseCollection<CoinsRequest>
    {
        public CoinsRequest()
        {

        }
        public ulong UserID { get; set; }
        public string EventID { get; set; }
        public ulong MessageID { get; set; }
        public int Amount { get; set; }
        public bool Active { get; set; }
        public bool Accepted { get; set; } = false;

        public DateTime CreateAT { get; set; } = DateTime.Now;
        public async Task Save()
        {
            var exists = Collection.Find(x => x.ID == ID);
            if (exists.Any())
                await Collection.ReplaceOneAsync(doc => doc.ID == ID, this, new ReplaceOptions { IsUpsert = true });
            else
                await Collection.InsertOneAsync(this);
        }
        public static async Task<CoinsRequest> GetOrCreate(string EventID)
        {
            var Result = Collection.Find((x) => x.EventID == EventID);
            if (Result.Any())
                return Result.First();
            else
            {
                var NewClass = new CoinsRequest() { EventID = EventID };
                await NewClass.Save();
                return NewClass;
            }
        }
    }
}
