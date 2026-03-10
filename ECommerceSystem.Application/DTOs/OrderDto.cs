using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceSystem.Domain.Entities;

namespace ECommerceSystem.Application.DTOs
{
    public class OrderDto
    {
       
       
            public int Id { get; set; }
            public string UserId { get; set; } = null!;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public List<OrderItem> Items { get; set; } = new();
        
}
}
