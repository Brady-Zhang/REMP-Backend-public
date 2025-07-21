using System;

namespace RealEstate.DTOs.Order;

public class OrderResponseDto
{
  public int Id { get; set; }
  public int ListingCaseId { get; set; }
  public string AgentId { get; set; }
  public decimal Amount { get; set; }
  public string Status { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}
