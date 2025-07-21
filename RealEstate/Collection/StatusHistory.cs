using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RealEstate.Collection
{
    public class StatusHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("listingCaseId")]
        public string ListingCaseId { get; set; }

        [BsonElement("oldStatus")]
        public string OldStatus { get; set; }

        [BsonElement("newStatus")]
        public string NewStatus { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedBy")]
        public string UpdatedBy { get; set; } // UserId
    }
}