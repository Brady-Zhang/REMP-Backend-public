using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealEstate.Enums;

namespace RealEstate.Collection;

public class OrderHistory
{ 
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public ObjectId Id {get; set;}

  [BsonElement("OrderId")]
  public string OrderId {get; set;}

  [BsonElement("oldStatus")]
  public string OldStatus { get; set; }

  [BsonElement("newStatus")]
  public string NewStatus { get; set; }

  [BsonElement("updatedAt")]
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  [BsonElement("updatedBy")]
  public string UpdatedBy { get; set; } // UserId
  
 
}
