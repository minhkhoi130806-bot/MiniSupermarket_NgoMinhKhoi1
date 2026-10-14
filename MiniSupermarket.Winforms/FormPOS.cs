
using MiniSupermarket.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormPOS : Form
    {
        private readonly List<CartItemDto> _cart = new();

        // Mau giao dien dong bo voi form Danh muc
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color primaryColor = Color.FromArgb(59, 130, 246);
        private readonly Color textColor = Color.FromArgb(31, 41, 55);
        private readonly Color mutedColor = Color.FromArgb(107, 114, 128);
        private readonly Color borderColor = Color.FromArgb(229, 231, 235);

        private DataGridView dgvCart = null!;
        private TextBox txtBarcode = null!;
        private TextBox txtCustomerPhone = null!;
        private Label lblCustomerName = null!;
        private Label lblTotalAmount = null!;
        private TextBox txtCashReceived = null!;
        private Label lblChange = null!;
        private Button btnCheckout = null!;

        public FormPOS()
        {
            InitializeComponent();

            // Dung giao dien tu code, tranh trung lap control Designer
            Controls.Clear();

            BuildPOSLayout();
            SetupCartGrid();
            UpdateCartDisplay();

            // Cho phep bam F9 de thanh toan
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F9)
                {
                    e.Handled = true;
                    btnCheckout.PerformClick();
                }
            };
        }

        private void BuildPOSLayout()
        {
            Text = "Bán hàng tại quầy - MiniSupermarket";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 760);
            MinimumSize = new Size(1000, 650);
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;

            // Layout chinh gom tieu de va noi dung
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

            // Tieu de
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = backgroundColor
            };

            var lblMainTitle = new Label
            {
                Text = "Bán hàng tại quầy",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Location = new Point(0, 2)
            };

            

            header.Controls.Add(lblMainTitle);
   
            root.Controls.Add(header, 0, 0);

            // Chia noi dung thanh gio hang va thanh toan
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = backgroundColor,
                Margin = new Padding(0)
            };

            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(content, 0, 1);

            var cartCard = CreatePOSCard();
            cartCard.Margin = new Padding(0, 0, 10, 0);

            var paymentCard = CreatePOSCard();
            paymentCard.Margin = new Padding(10, 0, 0, 0);

            content.Controls.Add(cartCard, 0, 0);
            content.Controls.Add(paymentCard, 1, 0);

            BuildCartSection(cartCard);
            BuildPaymentSection(paymentCard);
        }

        private Panel CreatePOSCard()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void BuildCartSection(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            card.Controls.Add(layout);

            var title = new Label
            {
                Text = "Giỏ hàng sản phẩm",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(title, 0, 0);

            // O nhap ma vach
            txtBarcode = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Quét hoặc nhập mã vạch sản phẩm...",
                Margin = new Padding(0, 5, 0, 8)
            };

            txtBarcode.KeyDown += txtBarcode_KeyDown;
            layout.Controls.Add(txtBarcode, 0, 1);

            // Bang gio hang
            dgvCart = new DataGridView
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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Both,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 38 },
                ColumnHeadersHeight = 42,
                Font = new Font("Segoe UI", 9F),
                GridColor = borderColor
            };

            dgvCart.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(243, 246, 250);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
            dgvCart.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCart.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                Color.FromArgb(243, 246, 250);

            dgvCart.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(219, 234, 254);
            dgvCart.DefaultCellStyle.SelectionForeColor = textColor;
            dgvCart.DefaultCellStyle.Padding = new Padding(6, 2, 6, 2);
            dgvCart.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(249, 250, 251);

            layout.Controls.Add(dgvCart, 0, 2);
        }

        private void BuildPaymentSection(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 7,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));

            card.Controls.Add(layout);

            var title = new Label
            {
                Text = "Thông tin thanh toán",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(title, 0, 0);

            // Thong tin khach hang
            var customerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 4, 0, 4)
            };

            customerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            customerPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            customerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblPhoneTitle = new Label
            {
                Text = "Số điện thoại khách hàng",
                Dock = DockStyle.Fill,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtCustomerPhone = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Nhập số điện thoại...",
                Margin = new Padding(0, 3, 0, 3)
            };

            customerPanel.Controls.Add(lblPhoneTitle, 0, 0);
            customerPanel.Controls.Add(txtCustomerPhone, 0, 1);
            layout.Controls.Add(customerPanel, 0, 1);

            lblCustomerName = new Label
            {
                Text = "Khách vãng lai",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 6)
            };

            layout.Controls.Add(lblCustomerName, 0, 2);

            // Tong tien
            var totalPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 4, 0, 4)
            };

            totalPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            totalPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            totalPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblTotalTitle = new Label
            {
                Text = "Tổng tiền thanh toán",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblTotalAmount = new Label
            {
                Text = "0 đ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 38, 38),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            totalPanel.Controls.Add(lblTotalTitle, 0, 0);
            totalPanel.Controls.Add(lblTotalAmount, 0, 1);
            layout.Controls.Add(totalPanel, 0, 3);

            // Tien khach dua
            var cashPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0, 4, 0, 4)
            };

            cashPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cashPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            cashPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblCashTitle = new Label
            {
                Text = "Tiền khách đưa",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtCashReceived = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "Nhập số tiền khách đưa...",
                Margin = new Padding(0, 3, 0, 3)
            };

            txtCashReceived.TextChanged += txtCashReceived_TextChanged;

            cashPanel.Controls.Add(lblCashTitle, 0, 0);
            cashPanel.Controls.Add(txtCashReceived, 0, 1);
            layout.Controls.Add(cashPanel, 0, 4);

            // Tien thua
            var changePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(10),
                Margin = new Padding(0, 6, 0, 6)
            };

            var lblChangeTitle = new Label
            {
                Text = "TIỀN THỪA",
                Dock = DockStyle.Top,
                Height = 22,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = mutedColor
            };

            lblChange = new Label
            {
                Text = "0 đ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 163, 74),
                TextAlign = ContentAlignment.MiddleLeft
            };

            changePanel.Controls.Add(lblChange);
            changePanel.Controls.Add(lblChangeTitle);
            layout.Controls.Add(changePanel, 0, 5);

            // Nut thanh toan
            btnCheckout = new Button
            {
                Text = "THANH TOÁN (F9)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 5, 0, 0),
                UseVisualStyleBackColor = false
            };

            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += btnCheckout_Click;
            layout.Controls.Add(btnCheckout, 0, 6);
        }

        private void SetupCartGrid()
        {
            dgvCart.DataSource = null;
            dgvCart.AutoGenerateColumns = false;
            dgvCart.Columns.Clear();

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductId",
                DataPropertyName = "ProductId",
                HeaderText = "Mã SP",
                FillWeight = 45,
                MinimumWidth = 50
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductName",
                DataPropertyName = "ProductName",
                HeaderText = "Tên sản phẩm",
                FillWeight = 130,
                MinimumWidth = 100
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UnitPrice",
                DataPropertyName = "UnitPrice",
                HeaderText = "Đơn giá",
                FillWeight = 75,
                MinimumWidth = 70,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                DataPropertyName = "Quantity",
                HeaderText = "SL",
                FillWeight = 35,
                MinimumWidth = 40,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalPrice",
                DataPropertyName = "TotalPrice",
                HeaderText = "Thành tiền",
                FillWeight = 85,
                MinimumWidth = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
        }

        // Hien thong bao khi gio hang trong, hoac hien danh sach khi co san pham
        private void UpdateCartDisplay()
        {
            dgvCart.DataSource = null;

            if (_cart.Count == 0)
            {
                // Tao cot rieng de hien thong bao trong bang
                dgvCart.Columns.Clear();

                var messageColumn = new DataGridViewTextBoxColumn
                {
                    Name = "EmptyMessage",
                    HeaderText = "",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                };

                dgvCart.Columns.Add(messageColumn);
                dgvCart.Rows.Clear();
                dgvCart.Rows.Add("Giỏ hàng đang trống");

                dgvCart.ColumnHeadersVisible = false;
                dgvCart.RowHeadersVisible = false;
                dgvCart.Rows[0].Height = 60;
                dgvCart.Rows[0].ReadOnly = true;
                dgvCart.Rows[0].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleCenter;
                dgvCart.Rows[0].DefaultCellStyle.ForeColor = mutedColor;
                dgvCart.Rows[0].DefaultCellStyle.SelectionForeColor = mutedColor;
                dgvCart.Rows[0].DefaultCellStyle.SelectionBackColor = Color.White;
                dgvCart.Rows[0].DefaultCellStyle.BackColor = Color.White;
                dgvCart.Rows[0].DefaultCellStyle.Font =
                    new Font("Segoe UI", 11F, FontStyle.Italic);
            }
            else
            {
                // Khoi phuc cot san pham va hien thi gio hang
                SetupCartGrid();
                dgvCart.ColumnHeadersVisible = true;
                dgvCart.DataSource = _cart;
            }

            decimal total = _cart.Sum(x => x.TotalPrice);
            lblTotalAmount.Text = $"{total:N0} đ";

            CalculateChange();
        }

        private async void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter &&
                !string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                e.SuppressKeyPress = true;

                string barcode = txtBarcode.Text.Trim();
                txtBarcode.Clear();

                await AddProductToCartByBarcodeAsync(barcode);
            }
        }

        private async Task AddProductToCartByBarcodeAsync(string barcode)
        {
            try
            {
                var product = await ApiClientService.Client
                    .GetFromJsonAsync<ProductDto>($"products/barcode/{barcode}");

                if (product == null)
                {
                    MessageBox.Show(
                        "Không tìm thấy sản phẩm có mã vạch này!",
                        "Cảnh báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var existingItem = _cart.FirstOrDefault(
                    c => c.ProductId == product.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    _cart.Add(new CartItemDto
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        UnitPrice = product.Price,
                        Quantity = 1
                    });
                }

                UpdateCartDisplay();
                txtBarcode.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi kết nối máy chủ: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void txtCashReceived_TextChanged(object sender, EventArgs e)
        {
            CalculateChange();
        }

        private void CalculateChange()
        {
            decimal total = _cart.Sum(x => x.TotalPrice);

            if (decimal.TryParse(txtCashReceived.Text.Trim(), out decimal cashReceived))
            {
                decimal change = cashReceived - total;

                if (change >= 0)
                {
                    lblChange.Text = $"{change:N0} đ";
                    lblChange.ForeColor = Color.FromArgb(22, 163, 74);
                }
                else
                {
                    lblChange.Text = "Chưa đủ tiền!";
                    lblChange.ForeColor = Color.FromArgb(220, 38, 38);
                }
            }
            else
            {
                lblChange.Text = "0 đ";
                lblChange.ForeColor = Color.FromArgb(22, 163, 74);
            }
        }

        private async void btnCheckout_Click(object sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show(
                    "Giỏ hàng đang trống!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            decimal total = _cart.Sum(x => x.TotalPrice);

            if (!decimal.TryParse(txtCashReceived.Text.Trim(), out decimal cashReceived)
                || cashReceived < total)
            {
                MessageBox.Show(
                    "Số tiền khách đưa chưa đủ hoặc không hợp lệ!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCashReceived.Focus();
                return;
            }

            var orderRequest = new
            {
                CashierUsername = SessionManager.CurrentUsername,
                CustomerPhone = txtCustomerPhone.Text.Trim(),
                Items = _cart.Select(i => new
                {
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice
                }).ToList()
            };

            try
            {
                btnCheckout.Enabled = false;

                var response = await ApiClientService.Client.PostAsJsonAsync(
                    "orders/checkout", orderRequest);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Thanh toán thành công và đã in hóa đơn!",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _cart.Clear();
                    UpdateCartDisplay();

                    txtCashReceived.Clear();
                    txtCustomerPhone.Clear();
                    lblCustomerName.Text = "Khách vãng lai";
                    txtBarcode.Focus();
                }
                else
                {
                    string detail = await response.Content.ReadAsStringAsync();

                    MessageBox.Show(
                        $"Thanh toán thất bại!\nMã phản hồi: {(int)response.StatusCode}\n{detail}",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể kết nối máy chủ để thanh toán.\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckout.Enabled = true;
            }
        }
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
