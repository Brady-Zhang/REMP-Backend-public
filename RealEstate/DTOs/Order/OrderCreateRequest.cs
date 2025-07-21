using System;

namespace RealEstate.DTOs.Order;

public class OrderCreateRequest
{
  public int ListingCaseId { get; set; }
  public string AgentId { get; set; }
  public decimal Amount { get; set; }
}
