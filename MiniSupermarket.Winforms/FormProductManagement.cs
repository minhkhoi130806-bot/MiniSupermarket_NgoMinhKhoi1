

using System;

using System.Collections.Generic;

using System.Drawing;

using System.Linq;

using System.Net;

using System.Net.Http;

using System.Net.Http.Headers;

using System.Net.Http.Json;

using System.Text.Json;

using System.Threading.Tasks;

using System.Windows.Forms;



namespace MiniSupermarket.WinForms

{

    public partial class FormProductManagement : Form

    {

        private DataGridView dgvProducts = null!;

        private TextBox txtId = null!;

        private TextBox txtBarcode = null!;

        private TextBox txtProductName = null!;

        private TextBox txtKeyword = null!;



        private NumericUpDown nudPrice = null!;

        private NumericUpDown nudStock = null!;

        private ComboBox cboCategory = null!;



        private Button btnAdd = null!;

        private Button btnUpdate = null!;

        private Button btnDelete = null!;

        private Button btnClear = null!;

        private Button btnLoad = null!;

        private Button btnSearch = null!;



        private Label lblStatus = null!;



        // Tiêu chí 25: Các biến điều khiển phân trang

        private int _currentPage = 1;

        private int _pageSize = 8;

        private int _totalPages = 1;

        private int _totalRecords = 0;

        private Button btnFirstPage = null!;

        private Button btnPrevPage = null!;

        private Label lblPageInfo = null!;

        private Button btnNextPage = null!;

        private Button btnLastPage = null!;



        // Mau giao dien dong bo voi FormCategoryManagement

        private readonly Color backgroundColor =

            Color.FromArgb(244, 246, 250);

        private readonly Color textColor =

            Color.FromArgb(24, 30, 48);

        private readonly Color primaryColor =

            Color.FromArgb(41, 100, 180);

        private readonly Color mutedColor =

            Color.FromArgb(100, 116, 139);



        private readonly List<ProductDto> productList = new();



        public FormProductManagement()

        {

            BuildProductLayout();

        }



        // ======================================================

        // GIAO DIEN

        // ======================================================



        private void BuildProductLayout()

        {

            Text = "Quản lý sản phẩm";

            Size = new Size(1200, 720);

            MinimumSize = new Size(1000, 620);

            StartPosition = FormStartPosition.CenterScreen;

            BackColor = backgroundColor;

            Font = new Font("Segoe UI", 10F);



            Load += FormProductManagement_Load;



            // Layout chinh gom tieu de va noi dung

            TableLayoutPanel root = new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 2,

                BackColor = backgroundColor,

                Margin = Padding.Empty,

                Padding = Padding.Empty

            };



            root.ColumnStyles.Add(

                new ColumnStyle(SizeType.Percent, 100));

            root.RowStyles.Add(

                new RowStyle(SizeType.Absolute, 62));

            root.RowStyles.Add(

                new RowStyle(SizeType.Percent, 100));



            Controls.Add(root);



            Label lblTitle = new Label

            {

                Text = "QUẢN LÝ SẢN PHẨM",

                Dock = DockStyle.Fill,

                Padding = new Padding(20, 0, 10, 0),

                TextAlign = ContentAlignment.MiddleLeft,

                Font = new Font("Segoe UI", 15F, FontStyle.Bold),

                ForeColor = textColor,

                BackColor = Color.White,

                Margin = Padding.Empty

            };



            root.Controls.Add(lblTitle, 0, 0);



            // Danh sach ben trai, thong tin chi tiet ben phai

            TableLayoutPanel body = new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 2,

                RowCount = 1,

                BackColor = backgroundColor,

                Padding = new Padding(16),

                Margin = Padding.Empty

            };



            body.ColumnStyles.Add(

                new ColumnStyle(SizeType.Percent, 100));

            body.ColumnStyles.Add(

                new ColumnStyle(SizeType.Absolute, 360));

            body.RowStyles.Add(

                new RowStyle(SizeType.Percent, 100));



            root.Controls.Add(body, 0, 1);

            body.Controls.Add(BuildListCard(), 0, 0);

            body.Controls.Add(BuildDetailCard(), 1, 0);

        }



        // The danh sach san pham

        private Panel BuildListCard()

        {

            Panel card = new Panel

            {

                Dock = DockStyle.Fill,

                BackColor = Color.White,

                Padding = new Padding(16),

                Margin = new Padding(0, 0, 8, 0)

            };



            Label lblListTitle = new Label

            {

                Text = "DANH SÁCH SẢN PHẨM",

                Dock = DockStyle.Top,

                Height = 36,

                Font = new Font("Segoe UI", 11F, FontStyle.Bold),

                ForeColor = textColor,

                TextAlign = ContentAlignment.MiddleLeft

            };



            // Thanh tim kiem va tai lai

            TableLayoutPanel searchBar = new TableLayoutPanel

            {

                Dock = DockStyle.Top,

                Height = 48,

                ColumnCount = 4,

                RowCount = 1,

                Padding = new Padding(0, 5, 0, 5),

                Margin = Padding.Empty

            };



            searchBar.ColumnStyles.Add(

                new ColumnStyle(SizeType.Percent, 100));

            searchBar.ColumnStyles.Add(

                new ColumnStyle(SizeType.Absolute, 110));

            searchBar.ColumnStyles.Add(

                new ColumnStyle(SizeType.Absolute, 100));

            searchBar.ColumnStyles.Add(

                new ColumnStyle(SizeType.Absolute, 130));

            searchBar.RowStyles.Add(

                new RowStyle(SizeType.Percent, 100));



            txtKeyword = new TextBox

            {

                Dock = DockStyle.Fill,

                Font = new Font("Segoe UI", 10F),

                PlaceholderText = "Tìm tên hoặc mã vạch...",

                BorderStyle = BorderStyle.FixedSingle,

                Margin = Padding.Empty

            };



            txtKeyword.KeyDown += (s, e) =>

            {

                if (e.KeyCode == Keys.Enter)

                {

                    e.SuppressKeyPress = true;

                    btnSearch.PerformClick();

                }

            };



            btnSearch = CreateActionButton(

                "TÌM KIẾM", primaryColor);

            btnSearch.Margin = new Padding(8, 0, 0, 0);

            btnSearch.Click += btnSearch_Click;



            btnLoad = CreateActionButton(

                "TẢI LẠI", mutedColor);

            btnLoad.Margin = new Padding(8, 0, 0, 0);

            btnLoad.Click += btnLoad_Click;



            Button btnExportCsv = CreateActionButton(

                "XUẤT BÁO CÁO", Color.FromArgb(22, 163, 74));

            btnExportCsv.Margin = new Padding(8, 0, 0, 0);

            btnExportCsv.Click += (s, e) => ExportProductsReport();



            searchBar.Controls.Add(txtKeyword, 0, 0);

            searchBar.Controls.Add(btnSearch, 1, 0);

            searchBar.Controls.Add(btnLoad, 2, 0);

            searchBar.Controls.Add(btnExportCsv, 3, 0);







            dgvProducts = CreateGrid();

            dgvProducts.CellClick += dgvProducts_CellClick;



            // Thanh phân trang (Trang trước, Trang sau, Số trang)

            // Thanh Footer chuyên nghiệp: Thông tin trạng thái bên trái, Cụm phân trang bên phải

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

            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Status bên trái

            footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));   // Cụm nút bên phải

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

            btnFirstPage.Click += async (s, e) =>

            {

                if (_currentPage > 1)

                    await LoadProductsPagedAsync(1);

            };



            btnPrevPage = CreatePageNavButton("◀ Trước");

            btnPrevPage.Click += async (s, e) =>

            {

                if (_currentPage > 1)

                    await LoadProductsPagedAsync(_currentPage - 1);

            };



            lblPageInfo = new Label

            {

                Text = "Trang 1 / 1",

                AutoSize = false,

                Width = 110,

                Height = 32,

                TextAlign = ContentAlignment.MiddleCenter,

                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),

                ForeColor = primaryColor,

                BackColor = Color.FromArgb(239, 246, 255), // Badge xanh nhạt hiện đại

                Margin = new Padding(4, 0, 4, 0)

            };



            btnNextPage = CreatePageNavButton("Sau ▶");

            btnNextPage.Click += async (s, e) =>

            {

                if (_currentPage < _totalPages)

                    await LoadProductsPagedAsync(_currentPage + 1);

            };



            btnLastPage = CreatePageNavButton("Cuối ⏭");

            btnLastPage.Click += async (s, e) =>

            {

                if (_currentPage < _totalPages)

                    await LoadProductsPagedAsync(_totalPages);

            };



            paginationPanel.Controls.Add(btnFirstPage);

            paginationPanel.Controls.Add(btnPrevPage);

            paginationPanel.Controls.Add(lblPageInfo);

            paginationPanel.Controls.Add(btnNextPage);

            paginationPanel.Controls.Add(btnLastPage);



            footerPanel.Controls.Add(lblStatus, 0, 0);

            footerPanel.Controls.Add(paginationPanel, 1, 0);



            // Thứ tự Add vào Card

            card.Controls.Add(dgvProducts);

            card.Controls.Add(footerPanel);

            card.Controls.Add(searchBar);

            card.Controls.Add(lblListTitle);



            return card;

        }



        // Cau hinh bang san pham

        private DataGridView CreateGrid()

        {

            DataGridView grid = new DataGridView

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

                SelectionMode =

                    DataGridViewSelectionMode.FullRowSelect,

                AutoSizeColumnsMode =

                    DataGridViewAutoSizeColumnsMode.Fill,

                AutoSizeRowsMode =

                    DataGridViewAutoSizeRowsMode.AllCells,

                EnableHeadersVisualStyles = false,

                ColumnHeadersHeight = 42,

                ColumnHeadersHeightSizeMode =

                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,

                CellBorderStyle =

                    DataGridViewCellBorderStyle.SingleHorizontal,

                GridColor = Color.FromArgb(226, 232, 240)

            };



            grid.RowTemplate.MinimumHeight = 38;



            grid.ColumnHeadersDefaultCellStyle.BackColor =

                Color.FromArgb(31, 45, 65);

            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            grid.ColumnHeadersDefaultCellStyle.Font =

                new Font("Segoe UI", 9F, FontStyle.Bold);



            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);

            grid.DefaultCellStyle.ForeColor =

                Color.FromArgb(55, 65, 81);

            grid.DefaultCellStyle.Padding = new Padding(4, 3, 4, 3);

            grid.DefaultCellStyle.SelectionBackColor =

                Color.FromArgb(219, 234, 254);

            grid.DefaultCellStyle.SelectionForeColor =

                Color.FromArgb(30, 41, 59);



            grid.AlternatingRowsDefaultCellStyle.BackColor =

                Color.FromArgb(248, 250, 252);



            grid.Columns.Add(new DataGridViewTextBoxColumn

            {

                Name = "ProductId",

                DataPropertyName = "ProductId",

                HeaderText = "Mã",

                FillWeight = 10,

                MinimumWidth = 45

            });



            grid.Columns.Add(new DataGridViewTextBoxColumn

            {

                Name = "Barcode",

                DataPropertyName = "Barcode",

                HeaderText = "Mã vạch",

                FillWeight = 20,

                MinimumWidth = 90

            });



            grid.Columns.Add(new DataGridViewTextBoxColumn

            {

                Name = "ProductName",

                DataPropertyName = "ProductName",

                HeaderText = "Tên sản phẩm",

                FillWeight = 35,

                MinimumWidth = 140,

                DefaultCellStyle = new DataGridViewCellStyle

                {

                    WrapMode = DataGridViewTriState.True

                }

            });



// Hien thi ma danh muc

grid.Columns.Add(new DataGridViewTextBoxColumn

{

    Name = "CategoryId",

    DataPropertyName = "CategoryId",

    HeaderText = "Mã danh mục",

    FillWeight = 15,

    MinimumWidth = 85

});



            // Hien thi ten danh muc

            grid.Columns.Add(new DataGridViewTextBoxColumn

            {

                Name = "CategoryName",

                DataPropertyName = "CategoryName",

                HeaderText = "Danh mục",

                FillWeight = 25,

                MinimumWidth = 120

            });





            grid.Columns.Add(new DataGridViewTextBoxColumn

            {

                Name = "Price",

                DataPropertyName = "Price",

                HeaderText = "Giá bán (VNĐ)",

                FillWeight = 20,

                MinimumWidth = 100,

                DefaultCellStyle = new DataGridViewCellStyle

                {

                    Format = "N0",

                    Alignment =

                        DataGridViewContentAlignment.MiddleRight

                }

            });



            grid.Columns.Add(new DataGridViewTextBoxColumn

            {

                Name = "StockQuantity",

                DataPropertyName = "StockQuantity",

                HeaderText = "Tồn kho",

                FillWeight = 15,

                MinimumWidth = 70,

                DefaultCellStyle = new DataGridViewCellStyle

                {

                    Alignment =

                        DataGridViewContentAlignment.MiddleCenter

                }

            });

            // Bắt sự kiện CellFormatting để tô màu cảnh báo hàng sắp hết kho (tồn <= 10)

            grid.CellFormatting += dgvProducts_CellFormatting;









            return grid;







        }



        // Sự kiện CellFormatting: Tô màu đỏ nhạt (LightCoral) khi số lượng tồn kho <= 10

        private void dgvProducts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)

        {

            if (e.RowIndex < 0 || e.CellStyle == null) return;



            // Kiểm tra nếu đang format ô của cột "StockQuantity" (Tồn kho)

            if (dgvProducts.Columns[e.ColumnIndex].Name == "StockQuantity")

            {

                if (e.Value != null && int.TryParse(e.Value.ToString(), out int stock) && stock <= 10)

                {

                    e.CellStyle.BackColor = Color.LightCoral;

                    e.CellStyle.ForeColor = Color.White;

                    e.CellStyle.Font = new Font(dgvProducts.Font, FontStyle.Bold);

                }

            }

        }



        // Tiêu chí 48: Xuất toàn bộ danh sách sản phẩm & tồn kho ra Excel (.xlsx) hoặc CSV (.csv)
        private void ExportProductsReport()
        {
            if (productList == null || productList.Count == 0)
            {
                MessageBox.Show(
                    "Chưa có dữ liệu sản phẩm để xuất!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "Tệp Excel (*.xlsx)|*.xlsx|Tệp CSV (*.csv)|*.csv",
                FileName = $"BaoCao_SanPham_TonKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                string cashier = SessionManager.CurrentUsername ?? "admin";
                ExcelExportHelper.ExportProducts(dlg.FileName, productList, cashier);

                var answer = MessageBox.Show(
                    $"Xuất báo cáo sản phẩm & tồn kho thành công!\n\nĐường dẫn: {dlg.FileName}\n\nBạn có muốn mở file ngay không?",
                    "Xuất báo cáo thành công",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (answer == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // The thong tin chi tiet san pham

        private Control BuildDetailCard()

        {

            TableLayoutPanel card = new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 1,

                RowCount = 12,

                BackColor = Color.White,

                Padding = new Padding(20),

                Margin = new Padding(8, 0, 0, 0)

            };



            card.ColumnStyles.Add(

                new ColumnStyle(SizeType.Percent, 100));



            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));



            Label lblFormTitle = new Label

            {

                Text = "THÔNG TIN CHI TIẾT",

                AutoSize = true,

                Font = new Font("Segoe UI", 12F, FontStyle.Bold),

                ForeColor = textColor,

                Margin = new Padding(0, 0, 0, 8)

            };



            txtId = new TextBox

            {

                Visible = false

            };



            txtBarcode = CreateTextBox("Nhập mã vạch...");

            txtProductName = CreateTextBox("Nhập tên sản phẩm...");



            cboCategory = new ComboBox

            {

                Dock = DockStyle.Fill,

                Font = new Font("Segoe UI", 10F),

                DropDownStyle = ComboBoxStyle.DropDownList,

                Margin = Padding.Empty

            };



            nudPrice = new NumericUpDown

            {

                Dock = DockStyle.Fill,

                Font = new Font("Segoe UI", 10F),

                Maximum = 1000000000,

                Minimum = 0,

                Increment = 1000,

                ThousandsSeparator = true,

                TextAlign = HorizontalAlignment.Right,

                Margin = Padding.Empty

            };



            nudStock = new NumericUpDown

            {

                Dock = DockStyle.Fill,

                Font = new Font("Segoe UI", 10F),

                Maximum = 1000000,

                Minimum = 0,

                ThousandsSeparator = true,

                Margin = Padding.Empty

            };



            card.Controls.Add(lblFormTitle, 0, 0);

            card.Controls.Add(CreateFieldLabel("Mã vạch *"), 0, 1);

            card.Controls.Add(txtBarcode, 0, 2);

            card.Controls.Add(CreateFieldLabel("Tên sản phẩm *"), 0, 3);

            card.Controls.Add(txtProductName, 0, 4);

            card.Controls.Add(CreateFieldLabel("Nhóm hàng *"), 0, 5);

            card.Controls.Add(cboCategory, 0, 6);

            card.Controls.Add(CreateFieldLabel("Đơn giá bán (VNĐ)"), 0, 7);

            card.Controls.Add(nudPrice, 0, 8);

            card.Controls.Add(CreateFieldLabel("Số lượng tồn kho"), 0, 9);

            card.Controls.Add(nudStock, 0, 10);

            card.Controls.Add(BuildButtonGrid(), 0, 11);



            return card;

        }



        // Tao o nhap lieu

        private TextBox CreateTextBox(string placeholder)

        {

            return new TextBox

            {

                Dock = DockStyle.Fill,

                Font = new Font("Segoe UI", 10F),

                BorderStyle = BorderStyle.FixedSingle,

                PlaceholderText = placeholder,

                Margin = Padding.Empty

            };

        }



        // Tao nhan cho o nhap lieu

        private Label CreateFieldLabel(string text)

        {

            return new Label

            {

                Text = text,

                AutoSize = true,

                Font = new Font("Segoe UI", 9F, FontStyle.Bold),

                ForeColor = Color.FromArgb(55, 65, 81),

                Margin = new Padding(0, 10, 0, 4)

            };

        }



        // Luoi nut thao tac 2 cot

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



            grid.ColumnStyles.Add(

                new ColumnStyle(SizeType.Percent, 50));

            grid.ColumnStyles.Add(

                new ColumnStyle(SizeType.Percent, 50));

            grid.RowStyles.Add(

                new RowStyle(SizeType.Percent, 50));

            grid.RowStyles.Add(

                new RowStyle(SizeType.Percent, 50));



            btnAdd = CreateActionButton(

                "THÊM MỚI", Color.FromArgb(40, 167, 69));

            btnAdd.Margin = new Padding(0, 0, 4, 4);

            btnAdd.Click += btnAdd_Click;



            btnUpdate = CreateActionButton(

                "CẬP NHẬT", Color.FromArgb(23, 162, 184));

            btnUpdate.Margin = new Padding(4, 0, 0, 4);

            btnUpdate.Click += btnUpdate_Click;



            btnDelete = CreateActionButton(

                "XÓA SẢN PHẨM", Color.FromArgb(220, 53, 69));

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



        // Tao nut chuc nang

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

                UseVisualStyleBackColor = false,

                Margin = new Padding(0, 0, 0, 0)

            };



            button.FlatAppearance.BorderSize = 0;

            return button;

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



        // ======================================================

        // XU LY DU LIEU

        // ======================================================



        private void PrepareAuthorization()

        {

            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))

            {

                throw new InvalidOperationException(

                    "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            }



            ApiClientService.Client.DefaultRequestHeaders.Authorization =

                new AuthenticationHeaderValue(

                    "Bearer", SessionManager.JwtToken);

        }



        private async void FormProductManagement_Load(

            object? sender, EventArgs e)

        {

            await LoadInitialDataAsync();

        }



        // Tai nhom hang va san pham khi mo form

        private async Task LoadInitialDataAsync()

        {

            SetBusy(true);



            try

            {

                await LoadCategoriesToComboAsync();

                await LoadProductsPagedAsync(1); // Nạp trang đầu tiên

            }

            finally

            {

                SetBusy(false);

            }

        }



        // Doc thuoc tinh JSON khong phan biet hoa thuong

        private static bool TryGetJsonProperty(

            JsonElement item,

            string name,

            out JsonElement value)

        {

            foreach (JsonProperty property in item.EnumerateObject())

            {

                if (string.Equals(

                    property.Name,

                    name,

                    StringComparison.OrdinalIgnoreCase))

                {

                    value = property.Value;

                    return true;

                }

            }



            value = default;

            return false;

        }



        private static int ReadInt(JsonElement item, string name)

        {

            if (!TryGetJsonProperty(item, name, out JsonElement value))

                return 0;



            if (value.ValueKind == JsonValueKind.Number &&

                value.TryGetInt32(out int number))

                return number;



            if (value.ValueKind == JsonValueKind.String &&

                int.TryParse(value.GetString(), out number))

                return number;



            return 0;

        }



        private static string ReadString(JsonElement item, string name)

        {

            if (!TryGetJsonProperty(item, name, out JsonElement value))

                return "";



            return value.ValueKind == JsonValueKind.Null

                ? ""

                : value.ToString();

        }



        // Doc danh sach nhom hang tu JSON

        private static List<(int Id, string Name)> ParseCategories(

            string json)

        {

            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;



            // Chap nhan mang JSON truc tiep hoac object co data

            if (root.ValueKind == JsonValueKind.Object &&

                TryGetJsonProperty(root, "data", out JsonElement data))

            {

                root = data;

            }



            if (root.ValueKind != JsonValueKind.Array)

            {

                throw new InvalidOperationException(

                    "API categories không trả về danh sách JSON hợp lệ.");

            }



            var result = new List<(int Id, string Name)>();



            foreach (JsonElement item in root.EnumerateArray())

            {

                int id = ReadInt(item, "CategoryId");

                if (id == 0)

                    id = ReadInt(item, "Id");



                string name = ReadString(item, "CategoryName");

                if (string.IsNullOrWhiteSpace(name))

                    name = ReadString(item, "Name");



                result.Add((id, name));

            }



            return result;

        }



        // Tai nhom hang vao ComboBox

        private async Task LoadCategoriesToComboAsync()

        {

            PrepareAuthorization();



            using HttpResponseMessage response =

                await ApiClientService.Client.GetAsync("categories");



            if (!response.IsSuccessStatusCode)

                throw await CreateApiExceptionAsync(response);



            string json = await response.Content.ReadAsStringAsync();

            var categories = ParseCategories(json);



            cboCategory.DataSource = null;

            cboCategory.DisplayMember = "Name";

            cboCategory.ValueMember = "Id";

            cboCategory.DataSource = categories

                .Select(c => new { c.Id, c.Name })

                .ToList();



            if (cboCategory.Items.Count > 0)

                cboCategory.SelectedIndex = 0;

        }





        // Tai san pham tu API

        private async Task LoadProductsAsync()

        {

            PrepareAuthorization();

            lblStatus.Text = "Đang tải danh sách sản phẩm...";



            using HttpResponseMessage response =

                await ApiClientService.Client.GetAsync("products");



            if (!response.IsSuccessStatusCode)

                throw await CreateApiExceptionAsync(response);



            string json = await response.Content.ReadAsStringAsync();



            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;



            // Ho tro API tra ve mang truc tiep hoac object co data

            if (root.ValueKind == JsonValueKind.Object &&

                TryGetJsonProperty(root, "data", out JsonElement data))

            {

                root = data;

            }



            if (root.ValueKind != JsonValueKind.Array)

                throw new InvalidOperationException(

                    "API products không trả về danh sách hợp lệ.");



            productList.Clear();



            foreach (JsonElement item in root.EnumerateArray())

            {

                // Doc thong tin san pham

                ProductDto? product = JsonSerializer.Deserialize<ProductDto>(

                    item.GetRawText(),

                    new JsonSerializerOptions

                    {

                        PropertyNameCaseInsensitive = true

                    });



                if (product == null)

                    continue;



                // Lay ten danh muc tu JSON

                string categoryName = "";



                if (TryGetJsonProperty(item, "category", out JsonElement category) &&

                    category.ValueKind == JsonValueKind.Object)

                {

                    categoryName = ReadString(category, "CategoryName");



                    if (string.IsNullOrWhiteSpace(categoryName))

                        categoryName = ReadString(category, "Name");

                }



                // Gan ten danh muc vao DTO

                product.CategoryName = categoryName;



                productList.Add(product);

            }



            DisplayProducts(productList);

        }



        // Tiêu chí 25: Nạp sản phẩm có chia trang từ Server API

        private async Task LoadProductsPagedAsync(int page)

        {

            PrepareAuthorization();

            lblStatus.Text = $"Đang tải dữ liệu trang {page}...";



            try

            {

                using HttpResponseMessage response =

                    await ApiClientService.Client.GetAsync($"products/paged?page={page}&pageSize={_pageSize}");



                if (!response.IsSuccessStatusCode)

                    throw await CreateApiExceptionAsync(response);



                string json = await response.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(json);

                JsonElement root = document.RootElement;



                _currentPage = root.GetProperty("currentPage").GetInt32();

                _totalPages = root.GetProperty("totalPages").GetInt32();

                _totalRecords = root.GetProperty("totalRecords").GetInt32();

                JsonElement data = root.GetProperty("data");



                productList.Clear();

                foreach (JsonElement item in data.EnumerateArray())

                {

                    ProductDto? product = JsonSerializer.Deserialize<ProductDto>(

                        item.GetRawText(),

                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });



                    if (product != null)

                    {

                        // Bóc tách tên danh mục từ JSON (hỗ trợ cả lồng nhau lẫn ở ngoài)

                        string categoryName = "";

                        if (TryGetJsonProperty(item, "category", out JsonElement category) &&

                            category.ValueKind == JsonValueKind.Object)

                        {

                            categoryName = ReadString(category, "CategoryName");

                            if (string.IsNullOrWhiteSpace(categoryName))

                                categoryName = ReadString(category, "Name");

                        }



                        if (string.IsNullOrWhiteSpace(categoryName))

                            categoryName = ReadString(item, "CategoryName");



                        product.CategoryName = categoryName;



                        productList.Add(product);

                    }

                }



                DisplayProducts(productList);



                // Cập nhật giao diện phân trang

                lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages}";

                btnFirstPage.Enabled = _currentPage > 1;

                btnPrevPage.Enabled = _currentPage > 1;

                btnNextPage.Enabled = _currentPage < _totalPages;

                btnLastPage.Enabled = _currentPage < _totalPages;

                lblStatus.Text = $"📌 Hiển thị trang {_currentPage}/{_totalPages}  •  Tổng cộng: {_totalRecords} sản phẩm";

            }

            catch (Exception ex)

            {

                lblStatus.Text = "Lỗi khi tải trang dữ liệu: " + ex.Message;

            }

        }





        // Hien thi danh sach len bang

        private void DisplayProducts(IEnumerable<ProductDto> products)

        {

            dgvProducts.DataSource = null;

            dgvProducts.DataSource = products.ToList();

            dgvProducts.ClearSelection();



            lblStatus.Text = $"Tổng số sản phẩm: {dgvProducts.Rows.Count}";

        }



        // Tai lai danh sach

        private async void btnLoad_Click(object? sender, EventArgs e)

        {

            txtKeyword.Clear();

            SetBusy(true);



            try

            {

                await LoadProductsPagedAsync(1);

            }

            catch (Exception ex)

            {

                ShowApiError("Không thể tải sản phẩm:", ex);

            }

            finally

            {

                SetBusy(false);

            }

        }



        // Tim kiem tren danh sach da tai

        private void btnSearch_Click(object? sender, EventArgs e)

        {

            string keyword = txtKeyword.Text.Trim();



            if (string.IsNullOrWhiteSpace(keyword))

            {

                DisplayProducts(productList);

                return;

            }



            var result = productList.Where(p =>

                (p.ProductName ?? "").Contains(

                    keyword, StringComparison.OrdinalIgnoreCase) ||

                (p.Barcode ?? "").Contains(

                    keyword, StringComparison.OrdinalIgnoreCase))

                .ToList();



            DisplayProducts(result);



            if (result.Count == 0)

            {

                MessageBox.Show(

                    "Không tìm thấy sản phẩm phù hợp!",

                    "Thông báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);

            }

        }



        // Chon san pham tren bang

        private void dgvProducts_CellClick(

            object? sender, DataGridViewCellEventArgs e)

        {

            if (e.RowIndex < 0)

                return;



            if (dgvProducts.Rows[e.RowIndex].DataBoundItem

                is not ProductDto product)

                return;



            txtId.Text = product.ProductId.ToString();

            txtBarcode.Text = product.Barcode ?? "";

            txtProductName.Text = product.ProductName ?? "";



            nudPrice.Value = Math.Min(

                nudPrice.Maximum,

                Math.Max(nudPrice.Minimum, product.Price));



            nudStock.Value = Math.Min(

                nudStock.Maximum,

                Math.Max(nudStock.Minimum, product.StockQuantity));



            // Chon dung nhom hang neu DTO co CategoryId

            var categoryProperty = typeof(ProductDto)

                .GetProperty("CategoryId");



            if (categoryProperty != null)

            {

                object? value = categoryProperty.GetValue(product);

                if (value != null && cboCategory.Items.Count > 0)

                {

                    int categoryId = Convert.ToInt32(value);

                    cboCategory.SelectedValue = categoryId;

                }

            }

        }



        // Kiem tra thong tin truoc khi luu

        private bool ValidateInputs()

        {

            if (string.IsNullOrWhiteSpace(txtBarcode.Text) ||

                string.IsNullOrWhiteSpace(txtProductName.Text))

            {

                MessageBox.Show(

                    "Vui lòng nhập mã vạch và tên sản phẩm!",

                    "Cảnh báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Warning);

                return false;

            }



            if (cboCategory.SelectedValue == null)

            {

                MessageBox.Show(

                    "Vui lòng chọn nhóm hàng!",

                    "Cảnh báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Warning);

                return false;

            }



            return true;

        }



        private int GetSelectedCategoryId()

        {

            return Convert.ToInt32(cboCategory.SelectedValue);

        }



        // Them san pham

        private async void btnAdd_Click(object? sender, EventArgs e)

        {

            if (!ValidateInputs())

                return;



            var newProduct = new

            {

                Barcode = txtBarcode.Text.Trim(),

                ProductName = txtProductName.Text.Trim(),

                Price = nudPrice.Value,

                StockQuantity = (int)nudStock.Value,

                CategoryId = GetSelectedCategoryId()

            };



            SetBusy(true);



            try

            {

                PrepareAuthorization();



                using HttpResponseMessage response =

                    await ApiClientService.Client.PostAsJsonAsync(

                        "products", newProduct);



                if (!response.IsSuccessStatusCode)

                    throw await CreateApiExceptionAsync(response);



                MessageBox.Show(

                    "Thêm sản phẩm thành công!",

                    "Thông báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);



                await LoadProductsAsync();

                ClearInputs();

            }

            catch (Exception ex)

            {

                ShowApiError("Thêm sản phẩm thất bại:", ex);

            }

            finally

            {

                SetBusy(false);

            }

        }



        // Cap nhat san pham

        private async void btnUpdate_Click(object? sender, EventArgs e)

        {

            if (!int.TryParse(txtId.Text, out int id))

            {

                MessageBox.Show(

                    "Vui lòng chọn sản phẩm cần cập nhật!",

                    "Cảnh báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Warning);

                return;

            }



            if (!ValidateInputs())

                return;



            var updateProduct = new

            {

                ProductId = id,

                Barcode = txtBarcode.Text.Trim(),

                ProductName = txtProductName.Text.Trim(),

                Price = nudPrice.Value,

                StockQuantity = (int)nudStock.Value,

                CategoryId = GetSelectedCategoryId()

            };



            SetBusy(true);



            try

            {

                PrepareAuthorization();



                using HttpResponseMessage response =

                    await ApiClientService.Client.PutAsJsonAsync(

                        $"products/{id}", updateProduct);



                if (!response.IsSuccessStatusCode)

                    throw await CreateApiExceptionAsync(response);



                MessageBox.Show(

                    "Cập nhật sản phẩm thành công!",

                    "Thông báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);



                await LoadProductsAsync();

                ClearInputs();

            }

            catch (Exception ex)

            {

                ShowApiError("Cập nhật sản phẩm thất bại:", ex);

            }

            finally

            {

                SetBusy(false);

            }

        }



        // Xoa san pham

        private async void btnDelete_Click(object? sender, EventArgs e)

        {

            if (!int.TryParse(txtId.Text, out int id))

            {

                MessageBox.Show(

                    "Vui lòng chọn sản phẩm cần xóa!",

                    "Cảnh báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Warning);

                return;

            }



            DialogResult confirm = MessageBox.Show(

                $"Bạn có chắc muốn xóa sản phẩm \"{txtProductName.Text}\" (Mã {id})?",

                "Xác nhận xóa",

                MessageBoxButtons.YesNo,

                MessageBoxIcon.Question);



            if (confirm != DialogResult.Yes)

                return;



            SetBusy(true);



            try

            {

                PrepareAuthorization();



                using HttpResponseMessage response =

                    await ApiClientService.Client.DeleteAsync(

                        $"products/{id}");



                if (!response.IsSuccessStatusCode)

                    throw await CreateApiExceptionAsync(response);



                MessageBox.Show(

                    "Xóa sản phẩm thành công!",

                    "Thông báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information);



                await LoadProductsAsync();

                ClearInputs();

            }

            catch (Exception ex)

            {

                ShowApiError("Xóa sản phẩm thất bại:", ex);

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

            txtBarcode.Clear();

            txtProductName.Clear();

            nudPrice.Value = 0;

            nudStock.Value = 0;



            if (cboCategory.Items.Count > 0)

                cboCategory.SelectedIndex = 0;



            dgvProducts.ClearSelection();

            txtBarcode.Focus();

        }



        // Khoa nut trong luc dang goi API

        private void SetBusy(bool busy)

        {

            if (btnAdd == null)

                return;



            btnAdd.Enabled = !busy;

            btnUpdate.Enabled = !busy;

            btnDelete.Enabled = !busy;

            btnLoad.Enabled = !busy;

            btnSearch.Enabled = !busy;

        }



        // Tao loi co kem ma HTTP va noi dung server tra ve

        private static async Task<Exception> CreateApiExceptionAsync(

            HttpResponseMessage response)

        {

            string detail = await response.Content.ReadAsStringAsync();



            string message = response.StatusCode switch

            {

                HttpStatusCode.Unauthorized =>

                    "Phiên làm việc hết hạn hoặc chưa đăng nhập.",

                HttpStatusCode.Forbidden =>

                    "Tài khoản không có quyền thực hiện thao tác này.",

                _ =>

                    $"API trả về lỗi {(int)response.StatusCode} " +

                    $"({response.ReasonPhrase})."

            };



            if (!string.IsNullOrWhiteSpace(detail))

                message += "\nChi tiết: " + detail;



            return new InvalidOperationException(message);

        }



        // Hien thi loi API

        private static void ShowApiError(string message, Exception ex)

        {

            MessageBox.Show(

                message + "\n" + ex.Message,

                "Lỗi",

                MessageBoxButtons.OK,

                MessageBoxIcon.Error);

        }

    }

}

