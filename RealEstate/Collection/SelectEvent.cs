using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RealEstate.Collection
{
    public class SelectEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("listingCaseId")]
        public string ListingCaseId { get; set; }

        [BsonElement("selectMediaId")]
        public List<int> SelectMediaIds { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedBy")]
        public string UpdatedBy { get; set; } // UserId
    }
}
