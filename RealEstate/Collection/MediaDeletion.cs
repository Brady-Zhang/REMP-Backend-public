using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RealEstate.Collection
{
    public class MediaDeletion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("mediaAssetId")]
        public string MediaAssetId { get; set; }

        [BsonElement("detail")]
        public string Detail { get; set; }

        [BsonElement("deleteAt")]
        public DateTime DeleteAt { get; set; } = DateTime.UtcNow;

        [BsonElement("deleteBy")]
        public string DeleteBy { get; set; } // UserId
    }
}
