namespace CozyComfort.Shared.DTOs.Seller
{
    public class CustomerOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty; // ADD THIS
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal SubTotal { get; set; } // ADD THIS
        public decimal Tax { get; set; } // ADD THIS
        public decimal ShippingCost { get; set; } // ADD THIS
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty; // ADD THIS
        public string PaymentMethod { get; set; } = string.Empty; // ADD THIS
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; } // ADD THIS
        public int ItemCount { get; set; } // ADD THIS
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class CreateCustomerOrderDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

   
}