using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MiniSupermarket.WinForms
{
    public partial class FormCategoryManagement : Form
    {
        private DataGridView dgvCategories = null!;
        private TextBox txtId = null!;
        private TextBox txtCategoryName = null!;
        private TextBox txtDescription = null!;
        private TextBox txtKeyword = null!;

        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnDelete = null!;
        private Button btnClear = null!;
        private Button btnLoad = null!;
        private Button btnSearch = null!;

        private Label lblStatus = null!;

        // Phân trang danh mục
        private int _currentPage = 1;
        private int _pageSize = 8;
        private int _totalPages = 1;
        private int _totalRecords = 0;
        private Button btnFirstPage = null!;
        private Button btnPrevPage = null!;
        private Label lblPageInfo = null!;
        private Button btnNextPage = null!;
        private Button btnLastPage = null!;

        // Bang mau dung chung cho form
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color textColor = Color.FromArgb(24, 30, 48);
        private readonly Color primaryColor = Color.FromArgb(41, 100, 180);
        private readonly Color mutedColor = Color.FromArgb(100, 116, 139);

        // Luu danh sach nhom hang, khong can CategoryDto
        private readonly List<(int Id, string Name, string Description)>
            categoryList = new();

        public FormCategoryManagement()
        {
            InitializeComponent();
            BuildCategoryLayout();
        }

        // ======================================================
        // GIAO DIEN
        // ======================================================
        private void BuildCategoryLayout()
        {
            Text = "Quản lý nhóm hàng";
            Size = new Size(1100, 650);
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10F);

            Load += FormCategoryManagement_Load;

            // Layout chinh: tieu de + noi dung
            TableLayoutPanel root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = backgroundColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Controls.Add(root);

            Label lblTitle = new Label
            {
                Text = "QUẢN LÝ NHÓM HÀNG",
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = textColor,
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            root.Controls.Add(lblTitle, 0, 0);

            // Noi dung: danh sach (co gian) + thong tin chi tiet (co dinh 380px)
            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = backgroundColor,
                Margin = Padding.Empty,
                Padding = new Padding(16)
            };

            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            root.Controls.Add(body, 0, 1);

            body.Controls.Add(BuildListCard(), 0, 0);
            body.Controls.Add(BuildDetailCard(), 1, 0);
        }

        // The danh sach nhom hang (ben trai)
        private Panel BuildListCard()
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 8, 0)
            };

            // Thanh tim kiem
            TableLayoutPanel searchBar = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 52,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 6, 0, 6),
                Margin = Padding.Empty
            };

            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            searchBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            searchBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            txtKeyword = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Nhập tên nhóm hàng cần tìm...",
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

            searchBar.Controls.Add(txtKeyword, 0, 0);
            searchBar.Controls.Add(btnSearch, 1, 0);
            searchBar.Controls.Add(btnLoad, 2, 0);

            // Tieu de danh sach
            Label lblListTitle = new Label
            {
                Text = "DANH SÁCH NHÓM HÀNG",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Footer phân trang chuyên nghiệp
            TableLayoutPanel footerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(12, 6, 12, 6),
                Margin = Padding.Empty
            };
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            lblStatus = new Label
            {
                Text = "Chưa tải dữ liệu",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty
            };

            FlowLayoutPanel paginationPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            btnFirstPage = CreatePageNavButton("⏮ Đầu");
            btnFirstPage.Click += async (s, e) => { if (_currentPage > 1) await LoadCategoriesPagedAsync(1); };

            btnPrevPage = CreatePageNavButton("◀ Trước");
            btnPrevPage.Click += async (s, e) => { if (_currentPage > 1) await LoadCategoriesPagedAsync(_currentPage - 1); };

            lblPageInfo = new Label
            {
                Text = "Trang 1 / 1",
                AutoSize = false,
                Width = 110,
                Height = 32,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = primaryColor,
                BackColor = Color.FromArgb(239, 246, 255),
                Margin = new Padding(4, 0, 4, 0)
            };

            btnNextPage = CreatePageNavButton("Sau ▶");
            btnNextPage.Click += async (s, e) => { if (_currentPage < _totalPages) await LoadCategoriesPagedAsync(_currentPage + 1); };

            btnLastPage = CreatePageNavButton("Cuối ⏭");
            btnLastPage.Click += async (s, e) => { if (_currentPage < _totalPages) await LoadCategoriesPagedAsync(_totalPages); };

            paginationPanel.Controls.Add(btnFirstPage);
            paginationPanel.Controls.Add(btnPrevPage);
            paginationPanel.Controls.Add(lblPageInfo);
            paginationPanel.Controls.Add(btnNextPage);
            paginationPanel.Controls.Add(btnLastPage);

            footerPanel.Controls.Add(lblStatus, 0, 0);
            footerPanel.Controls.Add(paginationPanel, 1, 0);

            dgvCategories = CreateGrid();
            dgvCategories.CellClick += dgvCategories_CellClick;

            card.Controls.Add(dgvCategories); // Fill
            card.Controls.Add(footerPanel);   // Bottom
            card.Controls.Add(searchBar);     // Top (duoi tieu de)
            card.Controls.Add(lblListTitle);  // Top (tren cung)

            return card;

            dgvCategories = CreateGrid();
            dgvCategories.CellClick += dgvCategories_CellClick;

            // THU TU ADD QUAN TRONG khi dung Dock:
            // control Fill add TRUOC, control Top/Bottom add SAU
            // (control add sau se duoc dock truoc)
            card.Controls.Add(dgvCategories); // Fill
            card.Controls.Add(lblStatus);     // Bottom
            card.Controls.Add(searchBar);     // Top (nam duoi tieu de)
            card.Controls.Add(lblListTitle);  // Top (tren cung)

            return card;
        }

        // Bang danh sach nhom hang
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
                RowHeadersVisible = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(226, 232, 240)
            };

            grid.RowTemplate.MinimumHeight = 38;

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 45, 65);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252);

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CategoryId",
                HeaderText = "Mã",
                FillWeight = 15,
                MinimumWidth = 50,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                }
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CategoryName",
                HeaderText = "Tên nhóm hàng",
                FillWeight = 40,
                MinimumWidth = 120
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Mô tả",
                FillWeight = 45,
                MinimumWidth = 120,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    WrapMode = DataGridViewTriState.True
                }
            });

            return grid;
        }

        // The thong tin chi tiet (ben phai)
        private Control BuildDetailCard()
        {
            TableLayoutPanel card = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(8, 0, 0, 0)
            };

            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // tieu de
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // nhan ma
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // o ma
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // nhan ten
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // o ten
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));       // nhan mo ta
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // o mo ta (gian het)
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));  // cac nut

            Label lblFormTitle = new Label
            {
                Text = "THÔNG TIN CHI TIẾT",
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = textColor,
                Margin = new Padding(0, 0, 0, 4)
            };

            txtId = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(240, 242, 245),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                Margin = Padding.Empty
            };

            txtCategoryName = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Nhập tên nhóm hàng...",
                Margin = Padding.Empty
            };

            txtDescription = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Nhập mô tả (không bắt buộc)...",
                Margin = Padding.Empty
            };

            card.Controls.Add(lblFormTitle, 0, 0);
            card.Controls.Add(CreateFieldLabel("Mã nhóm hàng"), 0, 1);
            card.Controls.Add(txtId, 0, 2);
            card.Controls.Add(CreateFieldLabel("Tên nhóm hàng *"), 0, 3);
            card.Controls.Add(txtCategoryName, 0, 4);
            card.Controls.Add(CreateFieldLabel("Mô tả"), 0, 5);
            card.Controls.Add(txtDescription, 0, 6);
            card.Controls.Add(BuildButtonGrid(), 0, 7);

            return card;
        }

        // Luoi 2x2 gom 4 nut chuc nang, chia deu
        private TableLayoutPanel BuildButtonGrid()
        {
            TableLayoutPanel grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 12, 0, 0),
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
            btnClear.Click += (s, e) => ClearInputs();

            grid.Controls.Add(btnAdd, 0, 0);
            grid.Controls.Add(btnUpdate, 1, 0);
            grid.Controls.Add(btnDelete, 0, 1);
            grid.Controls.Add(btnClear, 1, 1);

            return grid;
        }

        // Nhan cho truong nhap lieu
        private Label CreateFieldLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                Margin = new Padding(0, 14, 0, 4)
            };
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
        // XU LY DU LIEU
        // ======================================================

        // Gan JWT cho request (dung chung HttpClient cua ung dung)
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

        // Tim thuoc tinh JSON khong phan biet hoa thuong
        private static bool TryGetJsonProperty(
            JsonElement item,
            string propertyName,
            out JsonElement value)
        {
            foreach (JsonProperty property in item.EnumerateObject())
            {
                if (string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        // Doc so nguyen tu JSON
        private static int ReadInt(JsonElement item, string propertyName)
        {
            if (!TryGetJsonProperty(item, propertyName, out JsonElement value))
                return 0;

            if (value.ValueKind == JsonValueKind.Number &&
                value.TryGetInt32(out int number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String &&
                int.TryParse(value.GetString(), out number))
            {
                return number;
            }

            return 0;
        }

        // Doc chuoi tu JSON
        private static string ReadString(JsonElement item, string propertyName)
        {
            if (!TryGetJsonProperty(item, propertyName, out JsonElement value))
                return "";

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? "",
                JsonValueKind.Null => "",
                _ => value.ToString()
            };
        }

        // Chuyen JSON thanh danh sach nhom hang, khong dung DTO
        private static List<(int Id, string Name, string Description)>
            ParseCategories(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    "API categories không trả về mảng JSON. Hãy kiểm tra dữ liệu API.");
            }

            var result = new List<(int Id, string Name, string Description)>();

            foreach (JsonElement item in root.EnumerateArray())
            {
                // Chap nhan ca "CategoryId" va "Id"
                int id = ReadInt(item, "CategoryId");
                if (id == 0)
                    id = ReadInt(item, "Id");

                result.Add((
                    id,
                    ReadString(item, "CategoryName"),
                    ReadString(item, "Description")
                ));
            }

            return result;
        }

        // Hien thi danh sach len bang
        private void DisplayCategories(
            IEnumerable<(int Id, string Name, string Description)> categories)
        {
            dgvCategories.Rows.Clear();

            int count = 0;

            foreach (var category in categories)
            {
                dgvCategories.Rows.Add(
                    category.Id,
                    category.Name,
                    category.Description);

                count++;
            }

            dgvCategories.ClearSelection();
            lblStatus.Text = $"Tổng số nhóm hàng: {count}";
        }

        // Khi mo form, tai danh sach nhom hang
        private async void FormCategoryManagement_Load(object? sender, EventArgs e)
        {
            await LoadCategoriesPagedAsync(1);
        }

        // Tai danh sach nhom hang tu API
        private async Task LoadDataAsync()
        {
            if (!PrepareAuthorization())
                return;

            lblStatus.Text = "Đang tải danh sách nhóm hàng...";

            try
            {
                var response = await ApiClientService.Client.GetAsync("categories");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                categoryList.Clear();
                categoryList.AddRange(ParseCategories(json));

                DisplayCategories(categoryList);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Tải danh sách thất bại.";
                ShowApiError("Lỗi tải danh sách nhóm hàng:", ex);
            }
        }

        // Tai lai danh sach
        private async void btnLoad_Click(object? sender, EventArgs e)
        {
            SetBusy(true);

            try
            {
                txtKeyword.Clear();
                await LoadCategoriesPagedAsync(1);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Chon nhom hang tren bang
        private void dgvCategories_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvCategories.Rows[e.RowIndex];

            txtId.Text = row.Cells["CategoryId"].Value?.ToString() ?? "";
            txtCategoryName.Text = row.Cells["CategoryName"].Value?.ToString() ?? "";
            txtDescription.Text = row.Cells["Description"].Value?.ToString() ?? "";
        }

        // Them nhom hang
        private async void btnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập tên nhóm hàng!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCategoryName.Focus();
                return;
            }

            if (!PrepareAuthorization())
                return;

            var newCategory = new
            {
                CategoryName = txtCategoryName.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            SetBusy(true);

            try
            {
                using var response = await ApiClientService.Client
                    .PostAsJsonAsync("categories", newCategory);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Thêm nhóm hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadDataAsync();
                    ClearInputs();
                }
                else
                {
                    await ShowFailureAsync("Thêm nhóm hàng thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowApiError("Lỗi thêm nhóm hàng:", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Cap nhat nhom hang
        private async void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn nhóm hàng cần cập nhật!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show(
                    "Tên nhóm hàng không được để trống!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCategoryName.Focus();
                return;
            }

            if (!PrepareAuthorization())
                return;

            var updateCategory = new
            {
                CategoryId = id,
                CategoryName = txtCategoryName.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            SetBusy(true);

            try
            {
                using var response = await ApiClientService.Client
                    .PutAsJsonAsync($"categories/{id}", updateCategory);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Cập nhật nhóm hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadDataAsync();
                    ClearInputs();
                }
                else
                {
                    await ShowFailureAsync("Cập nhật thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowApiError("Lỗi cập nhật nhóm hàng:", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Xoa nhom hang
        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtId.Text, out int id))
            {
                MessageBox.Show(
                    "Vui lòng chọn nhóm hàng cần xóa!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa nhóm hàng \"{txtCategoryName.Text}\" (Mã {id})?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            if (!PrepareAuthorization())
                return;

            SetBusy(true);

            try
            {
                using var response = await ApiClientService.Client
                    .DeleteAsync($"categories/{id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Xóa nhóm hàng thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadDataAsync();
                    ClearInputs();
                }
                else
                {
                    await ShowFailureAsync("Xóa thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowApiError("Lỗi xóa nhóm hàng:", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Tim kiem nhom hang theo ten
        private async void btnSearch_Click(object? sender, EventArgs e)
        {
            string keyword = txtKeyword.Text.Trim();

            SetBusy(true);

            try
            {
                // Chua co du lieu thi tai tu API truoc
                if (categoryList.Count == 0)
                    await LoadDataAsync();

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    DisplayCategories(categoryList);
                    return;
                }

                var result = categoryList
                    .Where(c => c.Name.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                DisplayCategories(result);

                if (result.Count == 0)
                {
                    MessageBox.Show(
                        "Không tìm thấy nhóm hàng phù hợp!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ShowApiError("Không thể tìm kiếm nhóm hàng:", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Xoa trang cac o nhap
        private void ClearInputs()
        {
            txtId.Clear();
            txtCategoryName.Clear();
            txtDescription.Clear();

            dgvCategories.ClearSelection();
            txtCategoryName.Focus();
        }

        // Hien thi loi tu response cua API
        private static async Task ShowFailureAsync(
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

        // Hien thi loi ngoai le
        private static void ShowApiError(string message, Exception ex)
        {
            string detail =
                ex is HttpRequestException { StatusCode: { } code }
                    ? $"Server trả về lỗi {(int)code} ({code})."
                    : ex.Message;

            MessageBox.Show(
                message + "\n" + detail,
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private Button CreatePageNavButton(string text)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 80,
                Height = 32,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(3, 0, 3, 0)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(226, 232, 240);
            return btn;
        }

        private async Task LoadCategoriesPagedAsync(int page)
        {
            if (!PrepareAuthorization()) return;
            lblStatus.Text = $"Đang tải danh mục trang {page}...";

            try
            {
                var response = await ApiClientService.Client.GetAsync($"categories/paged?page={page}&pageSize={_pageSize}");
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;

                _currentPage = root.GetProperty("currentPage").GetInt32();
                _totalPages = root.GetProperty("totalPages").GetInt32();
                _totalRecords = root.GetProperty("totalRecords").GetInt32();
                JsonElement data = root.GetProperty("data");

                categoryList.Clear();
                foreach (JsonElement item in data.EnumerateArray())
                {
                    int id = ReadInt(item, "CategoryId");
                    if (id == 0) id = ReadInt(item, "Id");
                    string name = ReadString(item, "CategoryName");
                    string desc = ReadString(item, "Description");
                    categoryList.Add((id, name, desc));
                }

                DisplayCategories(categoryList);

                lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages}";
                btnFirstPage.Enabled = _currentPage > 1;
                btnPrevPage.Enabled = _currentPage > 1;
                btnNextPage.Enabled = _currentPage < _totalPages;
                btnLastPage.Enabled = _currentPage < _totalPages;
                lblStatus.Text = $"📌 Hiển thị trang {_currentPage}/{_totalPages}  •  Tổng cộng: {_totalRecords} nhóm hàng";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi khi tải danh mục: " + ex.Message;
            }
        }
    }
}