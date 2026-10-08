using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniSupermarket.Winforms
{
    public partial class FormMain : Form
    {
        private Button btnOpenCustomers;
        private Button btnOpenProducts;
        private Button btnOpenCategories;
        private Label lblTitle;

        public FormMain()
        {
            InitializeComponent(); // Thêm dòng này để gọi file Designer chạy trước
            SetupMainLayout();     // Sau đó chạy tiếp code dựng giao diện của bạn
        }

        private void SetupMainLayout()
        {
            this.Text = "Hệ thống Quản lý Siêu thị - PureFoodStore";
            this.Width = 500;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tiêu đề
            lblTitle = new Label()
            {
                Text = "PHẦN MỀM QUẢN LÝ SIÊU THỊ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 40),
                Width = 400,
                Height = 40
            };
            this.Controls.Add(lblTitle);

            // Nút mở Quản lý Khách hàng
            btnOpenCustomers = new Button()
            {
                Text = "👥 Quản lý Khách hàng",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(100, 110),
                Width = 280,
                Height = 50,
                BackColor = Color.LightGreen
            };
            btnOpenCustomers.Click += (s, e) => {
                var frm = new FormCustomerManagement();
                frm.ShowDialog(); // Mở form khách hàng dạng cửa sổ độc lập
            };
            this.Controls.Add(btnOpenCustomers);

            // Nút mở Quản lý Sản phẩm (Sẽ làm tiếp theo)
            btnOpenProducts = new Button()
            {
                Text = "📦 Quản lý Sản phẩm",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(100, 180),
                Width = 280,
                Height = 50,
                BackColor = Color.LightYellow
            };
            btnOpenProducts.Click += (s, e) => {
                MessageBox.Show("Phân hệ Quản lý Sản phẩm sẽ được tích hợp tiếp theo!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            this.Controls.Add(btnOpenProducts);

            // Nút Thoát
            Button btnExit = new Button()
            {
                Text = "❌ Thoát ứng dụng",
                Font = new Font("Arial", 10, FontStyle.Regular),
                Location = new Point(150, 270),
                Width = 180,
                Height = 40,
                BackColor = Color.LightCoral
            };
            btnExit.Click += (s, e) => { Application.Exit(); };
            this.Controls.Add(btnExit);
        }
    }
}