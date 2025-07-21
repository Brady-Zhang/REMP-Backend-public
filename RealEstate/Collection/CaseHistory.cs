using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealEstate.Enums;

namespace RealEstate.Collection
{
    public class CaseHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonElement("listCaseId")]
        public string ListingCaseId { get; set; }
        [BsonElement("changeDetail")]
        public string ChangeDetail { get; set; }
        [BsonElement("changedAction")]
        public ChangeAction Action { get; set; } 
        [BsonElement("changeDate")]
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
        [BsonElement("changedBy")]
        public string ChangedBy { get; set; } //UserId


    }
}
