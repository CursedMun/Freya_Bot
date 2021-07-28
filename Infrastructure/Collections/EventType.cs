using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

using Newtonsoft.Json;

using static Freya.Infrastructure.Mongo.EventerExtension;

namespace Freya.Infrastructure.Collections
{
    public enum Category
    {
        маленький,
        средний,
        большой,
        игровой
    }
    public class EventType : BaseCollection<EventType>
    {
        [BsonRepresentation(BsonType.String)]
        public Category Category { get; set; }
        public string Text { get; set; }
        public List<EventInfo> EventInfos { get; set; } = new();
        public async Task Save()
        {
            var exists = Collection.Find(x => x.ID == ID);
            if (exists.Any())
                await Collection.ReplaceOneAsync(doc => doc.ID == ID, this, new ReplaceOptions { IsUpsert = true });
            else
                await Collection.InsertOneAsync(this);
        }
        //TODO maybe make it async/await so it awaits for it if it takes to much time to do it 
        public static async Task<EventType> GetOrCreate(Category category)
        {
            var Result = Collection.Find((x) => x.Category == category);

            if (Result.Any())
                return Result.First();
            else
            {
                var newEventer = new EventType() { Category = category };
                await newEventer.Save();
                return newEventer;
            }
        }
    }

    public class EventInfo : IMongoClass
    {
        public string Name { get; set; }
        public Embed Embed { get; set; }
    }
    public partial class Embed
    {
        public string PlainText { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public uint Color { get; set; }
        public Uri Image { get; set; }
        public Field[] Fields { get; set; }
    }
    public partial class Field
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool Inline { get; set; }
    }
}
