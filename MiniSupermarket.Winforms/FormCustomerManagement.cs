using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniSupermarket.API.Models; // Đảm bảo namespace chứa model Customer của bạn

namespace MiniSupermarket.Winforms
{
    public partial class FormCustomerManagement : Form
    {
        // ⚠️ LƯU Ý: Thay số cổng (7123) bằng cổng thực tế của API trên Swagger máy bạn
        private static readonly HttpClient client = new HttpClient() { BaseAddress = new Uri("https://localhost:7250/api/") };

        private DataGridView dgvCustomers;
        private TextBox txtCustomerId, txtCustomerName, txtPhoneNumber, txtAddress, txtRewardPoints, txtMembershipRank, txtKeyword;
        private Button btnLoad, btnAdd, btnUpdate, btnDelete, btnSearch;
        private Label lblId, lblName, lblPhone, lblAddress, lblPoints, lblRank, lblSearch;

        public FormCustomerManagement()
        {
            InitializeComponent(); // Gọi hàm khởi tạo chuẩn của Designer, không lo lỗi trắng trang!
            SetupFormLayout();     // Tự vẽ các control giao diện quản lý khách hàng
        }

        private void SetupFormLayout()
        {
            this.Text = "Quản lý Khách hàng - MiniSupermarket";
            this.Width = 1150; // Mở rộng tổng thể form để không bị chật
            this.Height = 650;
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- 1. DataGridView (Nới rộng không gian hiển thị bảng dữ liệu) ---
            dgvCustomers = new DataGridView()
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(720, 560), // Cho bảng rộng hơn hẳn
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 30 } // Tăng chiều cao dòng cho dễ đọc
            };
            dgvCustomers.CellClick += dgvCustomers_CellClick;
            this.Controls.Add(dgvCustomers);

            // --- 2. Các ô nhập liệu bên phải (Rộng rãi, không bị che) ---
            int startX = 760; // Dời lùi sang phải để tạo khoảng cách thoáng
            int labelWidth = 100;
            int inputWidth = 220; // Tăng chiều rộng ô nhập liệu
            int startY = 20;
            int spacing = 45;

            lblId = new Label() { Text = "Mã KH:", Location = new System.Drawing.Point(startX, startY), Width = labelWidth };
            txtCustomerId = new TextBox() { Location = new System.Drawing.Point(startX + labelWidth, startY), Width = inputWidth, ReadOnly = true };

            lblName = new Label() { Text = "Tên KH:", Location = new System.Drawing.Point(startX, startY + spacing), Width = labelWidth };
            txtCustomerName = new TextBox() { Location = new System.Drawing.Point(startX + labelWidth, startY + spacing), Width = inputWidth };

            lblPhone = new Label() { Text = "Số điện thoại:", Location = new System.Drawing.Point(startX, startY + spacing * 2), Width = labelWidth };
            txtPhoneNumber = new TextBox() { Location = new System.Drawing.Point(startX + labelWidth, startY + spacing * 2), Width = inputWidth };

            lblAddress = new Label() { Text = "Địa chỉ:", Location = new System.Drawing.Point(startX, startY + spacing * 3), Width = labelWidth };
            txtAddress = new TextBox() { Location = new System.Drawing.Point(startX + labelWidth, startY + spacing * 3), Width = inputWidth };

            lblPoints = new Label() { Text = "Điểm thưởng:", Location = new System.Drawing.Point(startX, startY + spacing * 4), Width = labelWidth };
            txtRewardPoints = new TextBox() { Location = new System.Drawing.Point(startX + labelWidth, startY + spacing * 4), Width = inputWidth };

            lblRank = new Label() { Text = "Hạng thẻ:", Location = new System.Drawing.Point(startX, startY + spacing * 5), Width = labelWidth };
            txtMembershipRank = new TextBox() { Location = new System.Drawing.Point(startX + labelWidth, startY + spacing * 5), Width = inputWidth };

            this.Controls.AddRange(new Control[] { lblId, txtCustomerId, lblName, txtCustomerName, lblPhone, txtPhoneNumber, lblAddress, txtAddress, lblPoints, txtRewardPoints, lblRank, txtMembershipRank });

            // --- 3. Các nút chức năng (Sắp xếp cân đối, dễ thao tác) ---
            int btnY = startY + spacing * 6 + 10;
            int btnWidth = 155;
            int btnHeight = 40;

            btnAdd = new Button() { Text = "➕ Thêm mới", Location = new System.Drawing.Point(startX, btnY), Width = btnWidth, Height = btnHeight, BackColor = System.Drawing.Color.LightGreen, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };
            btnAdd.Click += btnAdd_Click;

            btnUpdate = new Button() { Text = "✏️ Cập nhật", Location = new System.Drawing.Point(startX + btnWidth + 10, btnY), Width = btnWidth, Height = btnHeight, BackColor = System.Drawing.Color.LightYellow, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };
            btnUpdate.Click += btnUpdate_Click;

            btnDelete = new Button() { Text = "🗑️ Xóa", Location = new System.Drawing.Point(startX, btnY + 50), Width = btnWidth, Height = btnHeight, BackColor = System.Drawing.Color.LightCoral, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };
            btnDelete.Click += btnDelete_Click;

            btnLoad = new Button() { Text = "🔄 Tải lại", Location = new System.Drawing.Point(startX + btnWidth + 10, btnY + 50), Width = btnWidth, Height = btnHeight, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };
            btnLoad.Click += btnLoad_Click;

            this.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnLoad });

            // --- 4. Khu vực Tìm kiếm ---
            int searchY = btnY + 110;
            lblSearch = new Label() { Text = "🔍 Tìm kiếm theo Tên / SĐT:", Location = new System.Drawing.Point(startX, searchY), Width = 320, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };

            txtKeyword = new TextBox() { Location = new System.Drawing.Point(startX, searchY + 25), Width = 215, Height = 30 };

            btnSearch = new Button() { Text = "Tìm kiếm", Location = new System.Drawing.Point(startX + 225, searchY + 23), Width = 95, Height = 30, BackColor = System.Drawing.Color.LightSkyBlue, Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold) };
            btnSearch.Click += btnSearch_Click;

            this.Controls.AddRange(new Control[] { lblSearch, txtKeyword, btnSearch });

            this.Load += FormCustomerManagement_Load;
        }

        private async void FormCustomerManagement_Load(object sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await client.GetFromJsonAsync<List<Customer>>("customers");
                dgvCustomers.DataSource = customers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối API: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];
                txtCustomerId.Text = row.Cells["CustomerId"].Value?.ToString();
                txtCustomerName.Text = row.Cells["CustomerName"].Value?.ToString();
                txtPhoneNumber.Text = row.Cells["PhoneNumber"].Value?.ToString();
                txtAddress.Text = row.Cells["Address"].Value?.ToString();
                txtRewardPoints.Text = row.Cells["RewardPoints"].Value?.ToString();
                txtMembershipRank.Text = row.Cells["MembershipRank"].Value?.ToString();
            }
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var newCust = new Customer
            {
                CustomerName = txtCustomerName.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                RewardPoints = int.TryParse(txtRewardPoints.Text, out int pts) ? pts : 0,
                MembershipRank = string.IsNullOrEmpty(txtMembershipRank.Text) ? "Chuẩn" : txtMembershipRank.Text.Trim()
            };

            var response = await client.PostAsJsonAsync("customers", newCust);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomersAsync();
            }
            else
            {
                MessageBox.Show("Thêm thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerId.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần sửa trên bảng!");
                return;
            }

            int id = int.Parse(txtCustomerId.Text);
            var updateCust = new Customer
            {
                CustomerId = id,
                CustomerName = txtCustomerName.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                RewardPoints = int.TryParse(txtRewardPoints.Text, out int pts) ? pts : 0,
                MembershipRank = txtMembershipRank.Text.Trim()
            };

            var response = await client.PutAsJsonAsync($"customers/{id}", updateCust);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCustomersAsync();
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCustomerId.Text))
            {
                MessageBox.Show("Vui lòng chọn khách hàng cần xóa!");
                return;
            }

            int id = int.Parse(txtCustomerId.Text);
            var confirm = MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                var response = await client.DeleteAsync($"customers/{id}");
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadCustomersAsync();
                }
                else
                {
                    MessageBox.Show("Xóa thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtKeyword.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                await LoadCustomersAsync();
                return;
            }

            try
            {
                var result = await client.GetFromJsonAsync<List<Customer>>($"customers/search?keyword={keyword}");
                dgvCustomers.DataSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không tìm thấy kết quả hoặc lỗi API: " + ex.Message);
            }
        }
    }
}