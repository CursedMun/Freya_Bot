using System;

using MongoDB.Bson;

namespace Freya.Infrastructure.Mongo
{
    public class EventerExtension
    {
        public abstract class IMongoClass
        {
            public string ID { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10);
        }

        public class IWarns : IMongoClass
        {
            public string Reason { get; set; }
            public DateTime CreateAt { get; set; } = DateTime.Now;
        }
        public class IVacation : IMongoClass
        {
            public double Days => (EndDate - StartDate).TotalDays;
            public string Reason { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool Active
            {
                get
                {

                    return EndDate != DateTime.MinValue && (EndDate - DateTime.Now).TotalSeconds >= 0;
                }
            }
            public DateTime CreateAT { get; set; } = DateTime.Now;
        }
        public class IEvent : IMongoClass
        {
            public ObjectId EventCategoryID { get; set; }
            public string EventTypeID { get; set; }
            public IEventChannels Channels { get; set; }
            public ITime Time { get; set; }
            public IEventReport EventReport { get; set; }
            public bool Finished { get; set; } = false;
            public bool Reported { get; set; } = false;

            public class IEventReport
            {
                public int RoundsCount { get; set; }
                public int UsersCount { get; set; }
                public bool GuildMembersPresent { get; set; }

            }
            public class IEventChannels
            {

                public ulong VoiceChannelID { get; set; }
                public ulong TextChannelID { get; set; }
                public ulong SettingsChannelID { get; set; }
            }
            public class ITime
            {
                public DateTime StartTime { get; set; }
                public DateTime EndTime { get; set; }
                public DateTime CreatedAT { get; set; }
                public double CountDiff => EndTime.Subtract(StartTime).TotalSeconds;
            }
        }
    }
}
