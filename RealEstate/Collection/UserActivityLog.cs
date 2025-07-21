using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RealEstate.Collection
{
    public class UserActivityLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonElement("userId")]
        public string UserId { get; set; }
        [BsonElement("activityDetail")]
        public string ActivityDetail { get; set; }
        [BsonElement("activityDate")]
        public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    }

}
