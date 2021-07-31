using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Freya.Infrastructure.Mongo;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Freya.Infrastructure.Collections
{
    public class BaseCollection<T> : MongoContext
    {
        [BsonId]
        public ObjectId ID { get; set; }
        public static IMongoCollection<T> Collection => GetCollection();
        public static IMongoCollection<T> GetCollection()
        {
            return MongoDatabase.GetCollection<T>(typeof(T).Name + 's');

        }
        public BaseCollection()
        {

        }

        public static Task<List<T>> GetAll()
        {
            var list = Collection.AsQueryable().ToListAsync();
            return list;
        }
        public static async Task<T> FindOne(Expression<Func<T, bool>> filter)
        {
            var items = await Collection.FindAsync(filter, new() { Limit = 1 });
            return items.FirstOrDefault();
        }
        public static async Task<IEnumerable<T>> Find(Expression<Func<T, bool>> filter)
        {
            var items = await Collection.FindAsync(filter);
            return items.ToEnumerable();
        }
        public static async Task<UpdateResult> Update(Expression<Func<T, object>> filter, object filterValue, Expression<Func<T, object>> update, object updateValue)
        {
            var filterDefinition = Builders<T>.Filter.Eq(filter, filterValue);
            var updateDefinition = Builders<T>.Update.Set(update, updateValue);
            var result = await Collection.UpdateOneAsync(filterDefinition, updateDefinition);
            return result;
        }
        public static async Task<DeleteResult> DeleteOne(Expression<Func<T, bool>> filter)
        {
            return await Collection.DeleteOneAsync(filter);
        }
    }
}
