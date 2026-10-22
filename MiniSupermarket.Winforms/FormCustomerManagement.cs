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

        private Label lblStatus = null!;

        private List<Customer> customerList = new List<Customer>();

        // Bang mau dong bo voi form Nhom hang
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color textColor = Color.FromArgb(24, 30, 48);
        private readonly Color primaryColor = Color.FromArgb(41, 100, 180);
        private readonly Color mutedColor = Color.FromArgb(100, 116, 139);

        public FormCustomerManagement()
        {
            InitializeComponent();

            // Dung giao dien tu code, tranh trung lap control Designer
            Controls.Clear();

            BuildCustomerLayout();
        }

        // ======================================================
        // GIAO DIEN
        // ======================================================
        private TableLayoutPanel _root = null!;
        private TableLayoutPanel _searchBar = null!;
        private Label _lblListTitle = null!;
        private float _lastScale = -1f;
        private float _baseGridHeaderFont = 9f;
        private float _baseGridCellFont = 9.5f;
        private readonly Dictionary<Control, float> _baseFonts = new();
        private readonly Dictionary<Control, Font> _ownFonts = new();

        // Kich thuoc thiet ke goc (pixel logic) de tinh he so co gian
        private const int DesignWidth = 1000;
        private const int DesignHeight = 560;

        private void BuildCustomerLayout()
        {
            Text = "Quản lý khách hàng - MiniSupermarket";
            Size = new Size(LogicalToDeviceUnits(1200), LogicalToDeviceUnits(700));
            MinimumSize = new Size(LogicalToDeviceUnits(700), LogicalToDeviceUnits(420));
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10F);

            Load += FormCustomerManagement_Load;

            // Layout chinh: tieu de + noi dung
            _root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = backgroundColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(50)));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Controls.Add(_root);

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ KHÁCH HÀNG",
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = textColor,
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            _root.Controls.Add(lblTitle, 0, 0);

            // Noi dung: danh sach + thong tin chi tiet, chia theo ti le
            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = backgroundColor,
                Margin = Padding.Empty,
                Padding = new Padding(12)
            };

            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 73));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _root.Controls.Add(body, 0, 1);

            body.Controls.Add(BuildListCard(), 0, 0);
            body.Controls.Add(BuildDetailCard(), 1, 0);

            // Ghi nho co chu goc roi co gian theo kich thuoc form
            RecordBaseFonts(this);
            Resize += (s, e) => ApplyScale();
            Shown += (s, e) => ApplyScale();
            ApplyScale();
        }

        // The danh sach khach hang (ben trai)
        private Panel BuildListCard()
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 6, 0)
            };

            // Thanh tim kiem
            _searchBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(46),
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 4, 0, 4),
                Margin = Padding.Empty
            };

            _searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LogicalToDeviceUnits(110)));
            _searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LogicalToDeviceUnits(100)));
            _searchBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            txtKeyword = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Tìm theo tên hoặc SĐT...",
                BorderStyle = BorderStyle.FixedSingle
            };

            // Nhan Enter de tim kiem
            txtKeyword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSearch.PerformClick();
                }
            };

            btnSearch = CreateActionButton("TÌM KIẾM", primaryColor);
            btnSearch.Margin = new Padding(8, 0, 0, 0);
            btnSearch.Click += btnSearch_Click;

            btnLoad = CreateActionButton("TẢI LẠI", mutedColor);
            btnLoad.Margin = new Padding(8, 0, 0, 0);
            btnLoad.Click += btnLoad_Click;

            _searchBar.Controls.Add(txtKeyword, 0, 0);
            _searchBar.Controls.Add(btnSearch, 1, 0);
            _searchBar.Controls.Add(btnLoad, 2, 0);

            // Tieu de danh sach
            _lblListTitle = new Label
            {
                Text = "DANH SÁCH KHÁCH HÀNG",
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(32),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Dong trang thai duoi bang
            lblStatus = new Label
            {
                Text = "Chưa tải dữ liệu",
                Dock = DockStyle.Bottom,
                Height = LogicalToDeviceUnits(28),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            dgvCustomers = CreateGrid();
            dgvCustomers.CellClick += dgvCustomers_CellClick;
            dgvCustomers.CellFormatting += dgvCustomers_CellFormatting;

            // Fill add TRUOC, Top/Bottom add SAU
            card.Controls.Add(dgvCustomers);  // Fill
            card.Controls.Add(lblStatus);     // Bottom
            card.Controls.Add(_searchBar);    // Top (duoi tieu de)
            card.Controls.Add(_lblListTitle); // Top (tren cung)

            return card;
        }

        // Bang danh sach: cot tu chia theo chieu rong, dong co chieu cao co dinh
        private DataGridView CreateGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                AllowUserToOrderColumns = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                MultiSelect = false,
                ShowCellToolTips = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                ScrollBars = ScrollBars.Vertical,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = LogicalToDeviceUnits(42),
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(226, 232, 240)
            };

            grid.RowTemplate.Height = LogicalToDeviceUnits(40);

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 45, 65);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(31, 45, 65);
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", _baseGridHeaderFont, FontStyle.Bold);

            grid.DefaultCellStyle.Font = new Font("Segoe UI", _baseGridCellFont);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False; // cat bang "..." thay vi xuong dong
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252);

            var center = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewTextBoxColumn Col(string name, string header, float weight,
                DataGridViewCellStyle? style = null) => new()
                {
                    Name = name,
                    DataPropertyName = name,
                    HeaderText = header,
                    FillWeight = weight,
                    MinimumWidth = 30,
                    DefaultCellStyle = style ?? new DataGridViewCellStyle(),
                    SortMode = DataGridViewColumnSortMode.NotSortable
                };

            grid.Columns.Add(Col("CustomerId", "Mã", 7, center));
            grid.Columns.Add(Col("CustomerName", "Họ và tên", 22));
            grid.Columns.Add(Col("PhoneNumber", "Số điện thoại", 16));
            grid.Columns.Add(Col("Address", "Địa chỉ", 33));
            grid.Columns.Add(Col("RewardPoints", "Điểm", 9,
                new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Format = "N0"
                }));
            grid.Columns.Add(Col("MembershipRank", "Hạng", 13, center));

            return grid;
        }

        private Font? _boldCell;

        // To mau hang thanh vien va lam dam ten / diem cho de nhin
        private void dgvCustomers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle == null) return;

            string col = dgvCustomers.Columns[e.ColumnIndex].Name;
            if (col != "MembershipRank" && col != "CustomerName" && col != "RewardPoints") return;

            _boldCell ??= new Font(dgvCustomers.DefaultCellStyle.Font ?? Font, FontStyle.Bold);
            e.CellStyle.Font = _boldCell;

            if (col == "CustomerName")
            {
                e.CellStyle.ForeColor = textColor;
            }
            else if (col == "RewardPoints")
            {
                e.CellStyle.ForeColor = Color.FromArgb(217, 119, 6);
            }
            else
            {
                string rank = (e.Value?.ToString() ?? "").ToLowerInvariant();
                e.CellStyle.ForeColor =
                    rank.Contains("kim") ? Color.FromArgb(37, 99, 235) :
                    rank.Contains("vàng") ? Color.FromArgb(180, 83, 9) :
                    rank.Contains("bạc") ? Color.FromArgb(100, 116, 139) :
                                           Color.FromArgb(22, 163, 74);
            }
        }

        // The thong tin chi tiet (ben phai) - moi hang chia theo ti le %
        private Control BuildDetailCard()
        {
            TableLayoutPanel card = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(6, 0, 0, 0)
            };

            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 9));    // tieu de
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // ma + ho ten
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // so dien thoai
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 28));   // dia chi
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 16));   // diem + hang
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 20));   // cac nut

            Label lblFormTitle = new Label
            {
                Text = "THÔNG TIN",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtCustomerId = CreateTextBox(readOnly: true);
            txtCustomerName = CreateTextBox("Nhập họ tên...");
            txtPhoneNumber = CreateTextBox("Nhập số điện thoại...");
            txtPhoneNumber.MaxLength = 15;
            txtAddress = CreateTextBox("Nhập địa chỉ (không bắt buộc)...");
            txtAddress.Multiline = true;
            txtAddress.ScrollBars = ScrollBars.Vertical;
            txtRewardPoints = CreateTextBox("0");
            txtMembershipRank = CreateTextBox("Chuẩn");

            txtRewardPoints.Text = "0";
            txtMembershipRank.Text = "Chuẩn";

            card.Controls.Add(lblFormTitle, 0, 0);
            card.Controls.Add(CreatePairRow(
                CreateFieldCell("Mã KH", txtCustomerId), 30,
                CreateFieldCell("Họ và tên *", txtCustomerName), 70), 0, 1);
            card.Controls.Add(CreateFieldCell("Số điện thoại", txtPhoneNumber), 0, 2);
            card.Controls.Add(CreateFieldCell("Địa chỉ", txtAddress, fillInput: true, labelPercent: 22), 0, 3);
            card.Controls.Add(CreatePairRow(
                CreateFieldCell("Điểm thưởng", txtRewardPoints), 45,
                CreateFieldCell("Hạng thành viên", txtMembershipRank), 55), 0, 4);
            card.Controls.Add(BuildButtonGrid(), 0, 5);

            return card;
        }

        // Mot o gom nhan + o nhap
        private TableLayoutPanel CreateFieldCell(
            string label, TextBox input, bool fillInput = false, int labelPercent = 40)
        {
            var cell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            cell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            cell.RowStyles.Add(new RowStyle(SizeType.Percent, labelPercent));
            cell.RowStyles.Add(new RowStyle(SizeType.Percent, 100 - labelPercent));

            cell.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 0, 2)
            }, 0, 0);

            input.Dock = fillInput ? DockStyle.Fill : DockStyle.Top;
            cell.Controls.Add(input, 0, 1);

            return cell;
        }

        // Hai o nam canh nhau tren cung mot hang
        private TableLayoutPanel CreatePairRow(Control left, float leftPercent, Control right, float rightPercent)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, leftPercent));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, rightPercent));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            left.Margin = new Padding(0, 0, 6, 0);
            right.Margin = new Padding(6, 0, 0, 0);

            row.Controls.Add(left, 0, 0);
            row.Controls.Add(right, 1, 0);
            return row;
        }

        private TextBox CreateTextBox(string placeholder = "", bool readOnly = false)
        {
            return new TextBox
            {
                ReadOnly = readOnly,
                BackColor = readOnly ? Color.FromArgb(240, 242, 245) : Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = placeholder,
                Margin = Padding.Empty
            };
        }

        // Luoi 2x2 gom 4 nut chuc nang, chia deu
        private TableLayoutPanel BuildButtonGrid()
        {
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 8, 0, 0),
                Padding = Padding.Empty
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            btnAdd = CreateActionButton("THÊM MỚI", Color.FromArgb(40, 167, 69));
            btnAdd.Margin = new Padding(0, 0, 4, 4);
            btnAdd.Click += btnAdd_Click;

            btnUpdate = CreateActionButton("CẬP NHẬT", Color.FromArgb(23, 162, 184));
            btnUpdate.Margin = new Padding(4, 0, 0, 4);
            btnUpdate.Click += btnUpdate_Click;

            btnDelete = CreateActionButton("XÓA", Color.FromArgb(220, 53, 69));
            btnDelete.Margin = new Padding(0, 4, 4, 0);
            btnDelete.Click += btnDelete_Click;

            btnClear = CreateActionButton("LÀM MỚI", mutedColor);
            btnClear.Margin = new Padding(4, 4, 0, 0);
            btnClear.Click += (s, e) => ClearForm();

            grid.Controls.Add(btnAdd, 0, 0);
            grid.Controls.Add(btnUpdate, 1, 0);
            grid.Controls.Add(btnDelete, 0, 1);
            grid.Controls.Add(btnClear, 1, 1);

            return grid;
        }

        // Nut chuc nang (dien day o chua no)
        private Button CreateActionButton(string text, Color color)
        {
            Button button = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        // ======================================================
        // CO GIAN THEO KICH THUOC FORM
        // ======================================================
        private void RecordBaseFonts(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                _baseFonts[c] = c.Font.Size;
                RecordBaseFonts(c);
            }
        }

        private void ApplyScale()
        {
            if (WindowState == FormWindowState.Minimized) return;
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            if (dgvCustomers == null || _root == null) return;

            float fw = ClientSize.Width / (float)LogicalToDeviceUnits(DesignWidth);
            float fh = ClientSize.Height / (float)LogicalToDeviceUnits(DesignHeight);
            float f = Math.Max(0.7f, Math.Min(1.5f, Math.Min(fw, fh)));

            if (Math.Abs(f - _lastScale) < 0.02f) return;
            _lastScale = f;

            SuspendLayout();

            // Cac kich thuoc pixel co dinh cung co gian theo he so f
            _root.RowStyles[0].Height = LogicalToDeviceUnits(50) * f;
            _searchBar.Height = (int)(LogicalToDeviceUnits(46) * f);
            _searchBar.ColumnStyles[1].Width = LogicalToDeviceUnits(110) * f;
            _searchBar.ColumnStyles[2].Width = LogicalToDeviceUnits(100) * f;
            _lblListTitle.Height = (int)(LogicalToDeviceUnits(32) * f);
            lblStatus.Height = (int)(LogicalToDeviceUnits(28) * f);

            ScaleFonts(this, f);

            // Bang
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", _baseGridHeaderFont * f, FontStyle.Bold);
            dgvCustomers.DefaultCellStyle.Font =
                new Font("Segoe UI", _baseGridCellFont * f);

            int rowH = (int)(LogicalToDeviceUnits(40) * f);
            _boldCell?.Dispose();
            _boldCell = null;
            dgvCustomers.RowTemplate.Height = rowH;
            dgvCustomers.ColumnHeadersHeight = (int)(LogicalToDeviceUnits(42) * f);
            foreach (DataGridViewRow row in dgvCustomers.Rows) row.Height = rowH;

            ResumeLayout(true);
        }

        private void ScaleFonts(Control parent, float f)
        {
            foreach (Control c in parent.Controls)
            {
                if (_baseFonts.TryGetValue(c, out float baseSize))
                {
                    var nf = new Font(c.Font.FontFamily, baseSize * f, c.Font.Style);
                    c.Font = nf;
                    if (_ownFonts.TryGetValue(c, out var old)) old.Dispose();
                    _ownFonts[c] = nf;
                }
                ScaleFonts(c, f);
            }
        }

        // ======================================================
        // XU LY DU LIEU
        // ======================================================

        // Gan JWT cho moi request (dung chung HttpClient cua ung dung)
        private bool PrepareAuthorization()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                MessageBox.Show(
                    "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.",
                    "Chưa xác thực",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            ApiClientService.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);

            return true;
        }

        // Khoa/mo cac nut trong luc goi API
        private void SetBusy(bool busy)
        {
            btnAdd.Enabled = !busy;
            btnUpdate.Enabled = !busy;
            btnDelete.Enabled = !busy;
            btnLoad.Enabled = !busy;
            btnSearch.Enabled = !busy;
        }

        private void DisplayCustomers(List<Customer> customers, string statusPrefix)
        {
            dgvCustomers.DataSource = null;
            dgvCustomers.DataSource = customers;
            dgvCustomers.ClearSelection();

            lblStatus.Text = $"{statusPrefix}: {customers.Count} khách hàng";
        }

        private async void FormCustomerManagement_Load(object? sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            if (!PrepareAuthorization())
                return;

            lblStatus.Text = "Đang tải danh sách khách hàng...";

            try
            {
                var customers = await ApiClientService.Client
                    .GetFromJsonAsync<List<Customer>>("customers");

                customerList = customers ?? new List<Customer>();
                DisplayCustomers(customerList, "Tổng số");
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Tải danh sách thất bại.";
                ShowError("Không thể tải danh sách khách hàng.", ex);
            }
        }

        private void dgvCustomers_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvCustomers.Rows.Count)
                return;

            if (dgvCustomers.Rows[e.RowIndex].DataBoundItem is not Customer customer)
                return;

            txtCustomerId.Text = customer.CustomerId.ToString();
            txtCustomerName.Text = customer.CustomerName ?? "";
            txtPhoneNumber.Text = customer.PhoneNumber ?? "";
            txtAddress.Text = customer.Address ?? "";
            txtRewardPoints.Text = customer.RewardPoints.ToString();
            txtMembershipRank.Text = customer.MembershipRank ?? "";
        }

        private async void btnLoad_Click(object? sender, EventArgs e)
        {
            SetBusy(true);

            try
            {
                txtKeyword.Clear();
                await LoadCustomersAsync();
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnSearch_Click(object? sender, EventArgs e)
        {
            string keyword = txtKeyword.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                await LoadCustomersAsync();
                return;
            }

            if (!PrepareAuthorization())
                return;

            SetBusy(true);

            try
            {
                string encodedKeyword = Uri.EscapeDataString(keyword);

                var result = await ApiClientService.Client
                    .GetFromJsonAsync<List<Customer>>(
                        $"customers/search?keyword={encodedKeyword}");

                DisplayCustomers(result ?? new List<Customer>(), "Kết quả");
            }
            catch (Exception ex)
            {
                ShowError("Không thể tìm kiếm khách hàng.", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private Customer ReadCustomerFromForm(int id = 0)
        {
            return new Customer
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
        }

        private async void btnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateCustomerInput() || !PrepareAuthorization())
                return;

            var newCustomer = ReadCustomerFromForm();

            SetBusy(true);

            try
            {
                using var response = await ApiClientService.Client
                    .PostAsJsonAsync("customers", newCustomer);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Thêm khách hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadCustomersAsync();
                    ClearForm();
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
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtCustomerId.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn khách hàng cần cập nhật trong bảng.",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateCustomerInput() || !PrepareAuthorization())
                return;

            var updateCustomer = ReadCustomerFromForm(id);

            SetBusy(true);

            try
            {
                using var response = await ApiClientService.Client
                    .PutAsJsonAsync($"customers/{id}", updateCustomer);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Cập nhật khách hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadCustomersAsync();
                    ClearForm();
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
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtCustomerId.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn khách hàng cần xóa trong bảng.",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa khách hàng \"{txtCustomerName.Text}\" (Mã {id})?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes || !PrepareAuthorization())
                return;

            SetBusy(true);

            try
            {
                using var response = await ApiClientService.Client
                    .DeleteAsync($"customers/{id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Xóa khách hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadCustomersAsync();
                    ClearForm();
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
            finally
            {
                SetBusy(false);
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

        private static async Task ShowResponseErrorAsync(
            string message,
            HttpResponseMessage response)
        {
            string detail = await response.Content.ReadAsStringAsync();

            MessageBox.Show(
                $"{message}\n" +
                $"Mã HTTP: {(int)response.StatusCode} ({response.StatusCode})\n" +
                detail,
                "Lỗi API",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void ShowError(string message, Exception ex)
        {
            string detail =
                ex is HttpRequestException { StatusCode: { } code }
                    ? $"Server trả về lỗi {(int)code} ({code})."
                    : ex.Message;

            MessageBox.Show(
                $"{message}\n{detail}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}