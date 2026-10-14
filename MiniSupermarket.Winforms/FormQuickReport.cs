using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormQuickReport : Form
    {
        private DateTimePicker dtpDate = null!;
        private Button btnRunReport = null!;
        private Label lblTotalOrders = null!;
        private Label lblTotalRevenue = null!;
        private Label lblBestSeller = null!;

        public FormQuickReport()
        {
            BuildReportLayout();
        }

        private void BuildReportLayout()
        {
            this.Size = new Size(1000, 600);
            this.BackColor = Color.FromArgb(244, 245, 247);
            this.Font = new Font("Segoe UI", 10);

            // Chọn ngày + nút xem báo cáo
            Label lblDate = new Label
            {
                Text = "Chọn ngày:",
                Location = new Point(20, 25),
                AutoSize = true
            };

            dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(110, 20),
                Width = 150
            };

            btnRunReport = new Button
            {
                Text = "Xem báo cáo",
                Location = new Point(275, 17),
                Size = new Size(140, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(41, 100, 180),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnRunReport.FlatAppearance.BorderSize = 0;
            btnRunReport.Click += btnRunReport_Click;

            this.Controls.Add(lblDate);
            this.Controls.Add(dtpDate);
            this.Controls.Add(btnRunReport);

            // 3 thẻ tóm tắt
            lblTotalOrders = CreateCard("Tổng số hóa đơn", 20, Color.FromArgb(52, 152, 219));
            lblTotalRevenue = CreateCard("Tổng doanh thu", 310, Color.FromArgb(39, 174, 96));
            lblBestSeller = CreateCard("Mặt hàng bán chạy nhất", 600, Color.FromArgb(230, 126, 34));
        }

        // Tạo một thẻ màu, trả về Label chứa giá trị
        private Label CreateCard(string title, int x, Color color)
        {
            Panel card = new Panel
            {
                Location = new Point(x, 80),
                Size = new Size(270, 130),
                BackColor = color
            };

            Label lblCaption = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 38,
                Padding = new Padding(12, 10, 0, 0)
            };

            Label lblValue = new Label
            {
                Text = "---",
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(lblValue);
            card.Controls.Add(lblCaption);
            lblValue.BringToFront();
            this.Controls.Add(card);

            return lblValue;
        }

        private void btnRunReport_Click(object? sender, EventArgs e)
        {
            // Lớp bảo vệ thứ 2: chỉ Admin được xem
            if (!string.Equals(SessionManager.CurrentRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không có quyền xem doanh thu ca!",
                    "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // Dữ liệu mẫu, sẽ nối API thật ở bước sau
            lblTotalOrders.Text = "25";
            lblTotalRevenue.Text = "3,450,000 đ";
            lblBestSeller.Text = "Nước suối";
        }
    }
}