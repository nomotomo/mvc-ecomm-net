namespace Ordering.Core.Entities;

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2, 
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5   
}