// using System;
// using RealEstate.Enums;

// namespace RealEstate.Domain;

// public class Order
// {
//    public int Id { get; set; }
//   public int ListingCaseId { get; set; }
//   public string AgentId { get; set; }
//   public decimal Amount { get; set; }
//   public OrderStatus Status { get; set; }
//   public DateTime CreatedAt { get; set; }
//   public DateTime? UpdatedAt { get; set; }
        
//   public virtual ListingCase ListingCase { get; set; }
//   public virtual Agent Agent { get; set; }
// }


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RealEstate.Enums;

namespace RealEstate.Domain
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string AgentId { get; set; }
        public int ListingCaseId { get; set; }
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ListingCase ListingCase { get; set; }
        public virtual Agent Agent { get; set; }
    }
}