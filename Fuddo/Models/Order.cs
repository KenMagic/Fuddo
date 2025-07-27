using System;
using System.Collections.Generic;

namespace Fuddo.Models;

public partial class Order
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public int UserId { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
