using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MongoDB.Driver;

using static Freya.Infrastructure.Mongo.EventerExtension;

namespace Freya.Infrastructure.Collections
{
    public class Eventer : BaseCollection<Eventer>
    {
        public Eventer()
        {

        }
        public Eventer(ulong UserID)
        {
            this.UserID = UserID;
        }

        public async Task Save()
        {
            var exists = Collection.Find(x => x.ID == ID);
            if (exists.Any())
                await Collection.ReplaceOneAsync(doc => doc.ID == ID, this, new ReplaceOptions { IsUpsert = true });
            else
                await Collection.InsertOneAsync(this);
        }
        //TODO maybe make it async/await so it awaits for it if it takes to much time to do it 
        public static async Task<Eventer> GetOrCreate(ulong UserID)
        {
            var Result = Collection.Find((x) => x.UserID == UserID);

            if (Result.Any())
                return Result.First();
            else
            {
                var newEventer = new Eventer(UserID);
                await newEventer.Save();
                return newEventer;
            }
        }

        public async Task<DeleteResult> Delete()
        {
            return await Collection.DeleteOneAsync(x => x.ID == ID);
        }
        #region Constructor
        public ulong UserID { get; set; }
        public DateTimeOffset CreateAt { get; set; } = DateTimeOffset.Now;
        public int Rank { get; set; } = 1;
        public List<IWarns> Warns { get; set; } = new();
        public List<IEvent> Events { get; set; } = new();
        public List<IVacation> Vacations { get; set; } = new();

        #endregion

    }
}
