
using MiniSupermarket.API.Models;
using MiniSupermarket.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.Winforms
{
    public partial class FormCustomerManagement : Form
    {
        private static readonly HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://localhost:7250/api/")
        };

        private DataGridView dgvCustomers = null!;
        private TextBox txtCustomerId = null!;
        private TextBox txtCustomerName = null!;
        private TextBox txtPhoneNumber = null!;
        private TextBox txtAddress = null!;
        private TextBox txtRewardPoints = null!;
        private TextBox txtMembershipRank = null!;
        private TextBox txtKeyword = null!;

        private Button btnLoad = null!;
        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnSearch = null!;
        private Button btnClear = null!;

        private Label lblCount = null!;

        private List<Customer> customerList = new List<Customer>();
        private Control? lblSubtitle;

        // Mau giao dien dong bo voi form Danh muc
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color primaryColor = Color.FromArgb(59, 130, 246);
        private readonly Color textColor = Color.FromArgb(31, 41, 55);
        private readonly Color mutedColor = Color.FromArgb(107, 114, 128);
        private readonly Color borderColor = Color.FromArgb(229, 231, 235);

        public FormCustomerManagement()
        {
            InitializeComponent();

            // Dung giao dien tu code, tranh trung lap control Designer
            Controls.Clear();

            BuildCustomerLayout();
        }

        private bool SetAuthorizationHeader()
        {
            client.DefaultRequestHeaders.Authorization = null;

            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                MessageBox.Show(
                    "Chưa có token đăng nhập. Vui lòng đăng nhập lại!",
                    "Lỗi xác thực",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return false;
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    SessionManager.JwtToken
                );

            return true;
        }




        private void BuildCustomerLayout()
        {
            Text = "Quản lý khách hàng - MiniSupermarket";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 760);
            MinimumSize = new Size(1000, 650);
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;

            // Layout chinh gom phan tieu de va noi dung
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20),
                BackColor = backgroundColor
            };

            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Controls.Add(root);

            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor
            };

            var title = new Label
            {
                Text = "Quản lý khách hàng",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            

            header.Controls.Add(title);
            header.Controls.Add(lblSubtitle);
            root.Controls.Add(header, 0, 0);

            // Chia noi dung thanh bang du lieu va form chi tiet
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = backgroundColor,
                Margin = new Padding(0)
            };

            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(content, 0, 1);

            var listCard = CreateCard();
            listCard.Margin = new Padding(0, 0, 12, 0);

            var detailCard = CreateCard();
            detailCard.Margin = new Padding(12, 0, 0, 0);

            content.Controls.Add(listCard, 0, 0);
            content.Controls.Add(detailCard, 1, 0);

            BuildCustomerList(listCard);
            BuildCustomerDetails(detailCard);

            Load += FormCustomerManagement_Load;
        }

        private Panel CreateCard()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void BuildCustomerList(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            card.Controls.Add(layout);

            var title = new Label
            {
                Text = "Danh sách khách hàng",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(title, 0, 0);

            // Khu vuc tim kiem va tai lai
            var searchPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 4, 0, 4)
            };

            searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
            searchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));

            txtKeyword = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Nhập tên hoặc số điện thoại...",
                Margin = new Padding(0, 4, 8, 4)
            };

            btnSearch = CreateButton("Tìm kiếm", primaryColor);
            btnSearch.Margin = new Padding(0, 3, 8, 3);
            btnSearch.Click += btnSearch_Click;

            btnLoad = CreateButton("Tải lại", Color.FromArgb(107, 114, 128));
            btnLoad.Margin = new Padding(0, 3, 0, 3);
            btnLoad.Click += btnLoad_Click;

            searchPanel.Controls.Add(txtKeyword, 0, 0);
            searchPanel.Controls.Add(btnSearch, 1, 0);
            searchPanel.Controls.Add(btnLoad, 2, 0);

            layout.Controls.Add(searchPanel, 0, 1);

            lblCount = new Label
            {
                Text = "Tổng số: 0 khách hàng",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(lblCount, 0, 2);

            dgvCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                ScrollBars = ScrollBars.Both,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 38 },
                ColumnHeadersHeight = 42,
                Font = new Font("Segoe UI", 9F),
                GridColor = borderColor
            };

            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(243, 246, 250);
            dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCustomers.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                Color.FromArgb(243, 246, 250);
            dgvCustomers.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(219, 234, 254);
            dgvCustomers.DefaultCellStyle.SelectionForeColor = textColor;
            dgvCustomers.DefaultCellStyle.Padding = new Padding(6, 2, 6, 2);
            dgvCustomers.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(249, 250, 251);


            // Hien thi day du thong tin khach hang
            dgvCustomers.Columns.Clear();

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerId",
                DataPropertyName = "CustomerId",
                HeaderText = "Mã KH",
                Width = 70
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CustomerName",
                DataPropertyName = "CustomerName",
                HeaderText = "Họ và tên",
                Width = 150
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PhoneNumber",
                DataPropertyName = "PhoneNumber",
                HeaderText = "Số điện thoại",
                Width = 125
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Address",
                DataPropertyName = "Address",
                HeaderText = "Địa chỉ",
                Width = 200
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RewardPoints",
                DataPropertyName = "RewardPoints",
                HeaderText = "Điểm thưởng",
                Width = 100
            });

            dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MembershipRank",
                DataPropertyName = "MembershipRank",
                HeaderText = "Hạng thành viên",
                Width = 130
            });

            dgvCustomers.CellClick += dgvCustomers_CellClick;


            layout.Controls.Add(dgvCustomers, 0, 3);
        }

        private void BuildCustomerDetails(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));

            card.Controls.Add(layout);

            var title = new Label
            {
                Text = "Thông tin khách hàng",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(title, 0, 0);

            // Cac truong nhap lieu tu dong co gian theo chieu rong
            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 12,
                Margin = new Padding(0, 6, 0, 0)
            };

            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 6; i++)
            {
                fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
                fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            }

            layout.Controls.Add(fields, 0, 1);

            AddField(fields, 0, "Mã khách hàng", out txtCustomerId, true);
            AddField(fields, 2, "Họ và tên", out txtCustomerName);
            AddField(fields, 4, "Số điện thoại", out txtPhoneNumber);
            AddField(fields, 6, "Địa chỉ", out txtAddress);
            AddField(fields, 8, "Điểm thưởng", out txtRewardPoints);
            AddField(fields, 10, "Hạng thành viên", out txtMembershipRank);

            var hint = new Label
            {
                Text = "Chọn một khách hàng trong bảng để xem hoặc chỉnh sửa.",
                Dock = DockStyle.Fill,
                ForeColor = mutedColor,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(hint, 0, 2);

            // Cac nut duoc sap xep deu nhau
            var buttons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 6, 0, 0)
            };

            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            btnAdd = CreateButton("Thêm mới", Color.FromArgb(22, 163, 74));
            btnUpdate = CreateButton("Cập nhật", primaryColor);
            btnDelete = CreateButton("Xóa khách hàng", Color.FromArgb(220, 38, 38));
            btnClear = CreateButton("Làm mới form", Color.FromArgb(107, 114, 128));

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();

            btnAdd.Margin = new Padding(0, 4, 5, 4);
            btnUpdate.Margin = new Padding(5, 4, 0, 4);
            btnDelete.Margin = new Padding(0, 4, 5, 4);
            btnClear.Margin = new Padding(5, 4, 0, 4);

            buttons.Controls.Add(btnAdd, 0, 0);
            buttons.Controls.Add(btnUpdate, 1, 0);
            buttons.Controls.Add(btnDelete, 0, 1);
            buttons.Controls.Add(btnClear, 1, 1);

            layout.Controls.Add(buttons, 0, 3);
        }

        private void AddField(
            TableLayoutPanel layout,
            int row,
            string labelText,
            out TextBox textBox,
            bool readOnly = false)
        {
            var label = new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 0, 2)
            };

            textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                ReadOnly = readOnly,
                BackColor = readOnly ? Color.FromArgb(243, 244, 246) : Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 3, 0, 7)
            };

            layout.Controls.Add(label, 0, row);
            layout.Controls.Add(textBox, 0, row + 1);
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Height = 36,
                UseVisualStyleBackColor = false
            }.Also(button =>
            {
                button.FlatAppearance.BorderSize = 0;
            });
        }

        private async void FormCustomerManagement_Load(object? sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await client.GetFromJsonAsync<List<Customer>>("customers");

                customerList = customers ?? new List<Customer>();
                dgvCustomers.DataSource = null;
                dgvCustomers.DataSource = customerList;

                lblCount.Text = $"Tổng số: {customerList.Count} khách hàng";
            }
            catch (Exception ex)
            {
                ShowError("Không thể tải danh sách khách hàng.", ex);
            }
        }

        private void dgvCustomers_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvCustomers.Rows.Count)
                return;

            var row = dgvCustomers.Rows[e.RowIndex];

            txtCustomerId.Text = row.Cells["CustomerId"].Value?.ToString() ?? "";
            txtCustomerName.Text = row.Cells["CustomerName"].Value?.ToString() ?? "";
            txtPhoneNumber.Text = row.Cells["PhoneNumber"].Value?.ToString() ?? "";

            // Lay dia chi va thong tin khac tu model dang duoc chon
            var customer = row.DataBoundItem as Customer;
            if (customer != null)
            {
                txtAddress.Text = customer.Address ?? "";
                txtRewardPoints.Text = customer.RewardPoints.ToString();
                txtMembershipRank.Text = customer.MembershipRank ?? "";
            }
        }

        private async void btnLoad_Click(object? sender, EventArgs e)
        {
            txtKeyword.Clear();
            await LoadCustomersAsync();
        }

        private async void btnSearch_Click(object? sender, EventArgs e)
        {
            string keyword = txtKeyword.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                await LoadCustomersAsync();
                return;
            }

            try
            {
                string encodedKeyword = Uri.EscapeDataString(keyword);

                var result = await client.GetFromJsonAsync<List<Customer>>(
                    $"customers/search?keyword={encodedKeyword}");

                var filtered = result ?? new List<Customer>();

                dgvCustomers.DataSource = null;
                dgvCustomers.DataSource = filtered;

                lblCount.Text = $"Kết quả: {filtered.Count} khách hàng";
            }
            catch (Exception ex)
            {
                ShowError("Không thể tìm kiếm khách hàng.", ex);
            }
        }

        private async void btnAdd_Click(object? sender, EventArgs e)
        {
            if (!SetAuthorizationHeader())
                return;

            var newCustomer = new Customer
            {
                CustomerName = txtCustomerName.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                RewardPoints = int.TryParse(txtRewardPoints.Text, out int points)
                    ? points : 0,
                MembershipRank = string.IsNullOrWhiteSpace(txtMembershipRank.Text)
                    ? "Chuẩn" : txtMembershipRank.Text.Trim()
            };

            try
            {
                var response = await client.PostAsJsonAsync("customers", newCustomer);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Thêm khách hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ClearForm();
                    await LoadCustomersAsync();
                }
                else
                {
                    await ShowResponseErrorAsync("Thêm khách hàng thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowError("Không thể thêm khách hàng.", ex);
            }
        }

        private async void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtCustomerId.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn khách hàng cần cập nhật trong bảng.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateCustomerInput())
                return;

            var updateCustomer = new Customer
            {
                CustomerId = id,
                CustomerName = txtCustomerName.Text.Trim(),
                PhoneNumber = txtPhoneNumber.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                RewardPoints = int.TryParse(txtRewardPoints.Text, out int points)
                    ? points : 0,
                MembershipRank = string.IsNullOrWhiteSpace(txtMembershipRank.Text)
                    ? "Chuẩn" : txtMembershipRank.Text.Trim()
            };

            try
            {
                var response = await client.PutAsJsonAsync(
                    $"customers/{id}", updateCustomer);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Cập nhật khách hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ClearForm();
                    await LoadCustomersAsync();
                }
                else
                {
                    await ShowResponseErrorAsync("Cập nhật khách hàng thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowError("Không thể cập nhật khách hàng.", ex);
            }
        }

        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtCustomerId.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn khách hàng cần xóa trong bảng.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa khách hàng này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                var response = await client.DeleteAsync($"customers/{id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Xóa khách hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ClearForm();
                    await LoadCustomersAsync();
                }
                else
                {
                    await ShowResponseErrorAsync("Xóa khách hàng thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowError("Không thể xóa khách hàng.", ex);
            }
        }

        private bool ValidateCustomerInput()
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập họ tên khách hàng.",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCustomerName.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtRewardPoints.Text) &&
                (!int.TryParse(txtRewardPoints.Text, out int points) || points < 0))
            {
                MessageBox.Show(
                    "Điểm thưởng phải là số nguyên không âm.",
                    "Dữ liệu không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtRewardPoints.Focus();
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            txtCustomerId.Clear();
            txtCustomerName.Clear();
            txtPhoneNumber.Clear();
            txtAddress.Clear();
            txtRewardPoints.Text = "0";
            txtMembershipRank.Text = "Chuẩn";

            dgvCustomers.ClearSelection();
            txtCustomerName.Focus();
        }

        private async Task ShowResponseErrorAsync(string message, HttpResponseMessage response)
        {
            string detail = await response.Content.ReadAsStringAsync();

            MessageBox.Show(
                $"{message}\nMã phản hồi: {(int)response.StatusCode}\n{detail}",
                "Lỗi API",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void ShowError(string message, Exception ex)
        {
            MessageBox.Show(
                $"{message}\nChi tiết: {ex.Message}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // Ho tro cau hinh Button ngay khi khoi tao
    internal static class ControlExtensions
    {
        public static T Also<T>(this T control, Action<T> action)
        {
            action(control);
            return control;
        }
    }
}
