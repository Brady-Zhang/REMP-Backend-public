using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace RealEstate.Collection
{
    public class UserRegisterHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserName")]
        public string UserName { get; set; }

        [BsonElement("EventDate")]
        public DateTime EventDate { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; }
        [BsonElement("ErrorMessage")]
        public string ErrorMessage { get; set; }

    }
}
