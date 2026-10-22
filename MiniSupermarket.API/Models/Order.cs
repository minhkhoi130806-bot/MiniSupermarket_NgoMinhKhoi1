
namespace MiniSupermarket.API.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        // Ma don hang
        public string OrderCode { get; set; } = string.Empty;

        // Thong tin khach hang
        public int? CustomerId { get; set; }

        // Thoi diem tao don
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Tong so luong san pham trong don
        public int TotalItems { get; set; }

        // Ma thu ngan
        public int CashierId { get; set; }

        // Ten tai khoan thu ngan
        public string CashierUsername { get; set; } = string.Empty;

        // Tong tien hang truoc khi giam gia
        public decimal TotalAmount { get; set; }

        // So tien giam gia
        public decimal DiscountAmount { get; set; }

        // So tien khach phai thanh toan sau giam gia
        public decimal FinalAmount { get; set; }

        // So tien khach dua
        public decimal AmountReceived { get; set; }

        // So tien thoi lai cho khach
        public decimal ChangeAmount { get; set; }

        // Diem thuong nhan duoc
        public int RewardPoints { get; set; }

        // Phuong thuc thanh toan: Cash, Card...
        public string PaymentMethod { get; set; } = "Cash";

        // Trang thai don hang
        public string Status { get; set; } = "Completed";
    }
}
