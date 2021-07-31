using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime RoleGivenAt { get; set; }
        public int Rank { get; set; } = 1;
        public List<IWarns> Warns { get; set; } = new();
        public List<IEvent> Events { get; set; } = new();
        public List<IVacation> Vacations { get; set; } = new();

        #endregion

    }
}
