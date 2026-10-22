
namespace MiniSupermarket.API.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        // Ma don hang
        public int OrderId { get; set; }

        // Ma san pham
        public int ProductId { get; set; }

        // So luong mua
        public int Quantity { get; set; }

        // Don gia tai thoi diem mua
        public decimal UnitPrice { get; set; }

        // Thanh tien cua dong san pham
        public decimal LineTotal { get; set; }
    }
}
