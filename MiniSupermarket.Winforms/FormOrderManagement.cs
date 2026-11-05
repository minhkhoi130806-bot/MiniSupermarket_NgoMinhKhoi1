using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    // =========================================================================
    // MÀN HÌNH: QUẢN LÝ & TRA CỨU ĐƠN HÀNG (CHI TIẾT HÓA ĐƠN)
    // TÍCH HỢP TIÊU CHÍ 25: PHÂN TRANG DỮ LIỆU MÁY CHỦ BẰNG LINQ .Skip() & .Take()
    // - Bố cục cân đối 2 nửa: 55% Bảng danh sách - 45% Hóa đơn điện tử chi tiết
    // - Thẻ bộ lọc 4 nút thời gian chuẩn mực: [Tất cả] [Hôm nay] [7 ngày] [30 ngày]
    // - Chi tiết hóa đơn được bố trí dạng lưới TableLayoutPanel không bao giờ bị che chữ
    // - Cột mã hóa đơn và mã vạch hiển thị đầy đủ, không bị dấu ba chấm (...)
    // =========================================================================
    public partial class FormOrderManagement : Form
    {
        private static readonly CultureInfo Vi = new("vi-VN");
        private static readonly JsonSerializerOptions JsonOpt = new() { PropertyNameCaseInsensitive = true };

        // Bảng màu giao diện hiện đại
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color textColor = Color.FromArgb(24, 30, 48);
        private readonly Color primaryColor = Color.FromArgb(41, 100, 180);
        private readonly Color mutedColor = Color.FromArgb(100, 116, 139);
        private readonly Color successColor = Color.FromArgb(22, 163, 74);
        private readonly Color dangerColor = Color.FromArgb(220, 38, 38);
        private readonly Color cardBg = Color.White;
        private readonly Color borderGrid = Color.FromArgb(226, 232, 240);

        // =====================================================================
        // TIÊU CHÍ 25: CÁC BIẾN QUẢN LÝ PHÂN TRANG MÁY CHỦ (SERVER-SIDE PAGINATION)
        // =====================================================================
        private int _currentPage = 1;      // Trang hiện tại
        private int _pageSize = 8;         // Số lượng đơn hàng trên mỗi trang (8 đơn)
        private int _totalPages = 1;       // Tổng số trang tính từ Server
        private int _totalRecords = 0;     // Tổng số đơn hàng thỏa điều kiện tìm kiếm
        private bool _isAllTime = true;    // Mặc định nạp tất cả hoặc theo khoảng ngày

        // Các nút điều hướng phân trang
        private Button btnFirstPage = null!;
        private Button btnPrevPage = null!;
        private Label lblPageInfo = null!;
        private Button btnNextPage = null!;
        private Button btnLastPage = null!;

        // Các điều khiển bộ lọc thời gian và từ khóa
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private Button btnReset = null!;
        private Button btnAll = null!;
        private Button btnToday = null!;
        private Button btn7Days = null!;
        private Button btn30Days = null!;
        private Button btnExport = null!;

        // Bảng danh sách đơn hàng & Bảng chi tiết mặt hàng
        private DataGridView dgvOrders = null!;
        private DataGridView dgvDetails = null!;

        // Các nhãn hiển thị Thẻ Hóa Đơn Điện Tử ở nửa bên phải
        private Label lblReceiptCode = null!;
        private Label lblReceiptStatusBadge = null!;
        private Label lblReceiptDate = null!;
        private Label lblReceiptCashier = null!;
        private Label lblReceiptCustomer = null!;
        private Label lblReceiptPhone = null!;
        private Label lblReceiptPayment = null!;
        private Label lblReceiptPoints = null!;

        // Khung tổng kết thanh toán ở chân hóa đơn
        private Label lblSubTotal = null!;
        private Label lblDiscount = null!;
        private Label lblFinalAmount = null!;
        private Label lblCashReceived = null!;

        private Label lblStatus = null!;
        private List<OrderListDto> _ordersList = new();
        private bool _busy;

        public FormOrderManagement()
        {
            BuildLayout();
        }

        // =========================================================================
        // BƯỚC 1: XÂY DỰNG BỐ CỤC TỔNG THỂ (LAYOUT)
        // =========================================================================
        private void BuildLayout()
        {
            Text = "Quản lý & Tra cứu Đơn hàng";
            Size = new Size(LogicalToDeviceUnits(1280), LogicalToDeviceUnits(780));
            MinimumSize = new Size(LogicalToDeviceUnits(960), LogicalToDeviceUnits(580));
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12),
                BackColor = backgroundColor
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(84))); // Hàng 0: Card bộ lọc 2 tầng
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));                        // Hàng 1: Hai phân vùng chính (55% - 45%)
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(28))); // Hàng 2: Thanh trạng thái
            Controls.Add(root);

            root.Controls.Add(BuildFilterCard(), 0, 0);
            root.Controls.Add(BuildContentSplit(), 0, 1);

            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = mutedColor,
                Font = new Font("Segoe UI", 9.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Sẵn sàng tra cứu dữ liệu hóa đơn."
            };
            root.Controls.Add(lblStatus, 0, 2);

            Load += async (s, e) =>
            {
                _isAllTime = true;
                HighlightFilterButton(btnAll);
                dtpFrom.Value = DateTime.Today.AddDays(-29);
                dtpTo.Value = DateTime.Today;
                await LoadOrdersPagedAsync(1);
            };
        }

        // =========================================================================
        // BƯỚC 2: THANH BỘ LỌC 2 TẦNG GỌN GÀNG: [TẤT CẢ] [HÔM NAY] [7 NGÀY] [30 NGÀY]
        // =========================================================================
        private Control BuildFilterCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = cardBg,
                Padding = new Padding(12, 8, 12, 8),
                Margin = new Padding(0, 0, 0, 8)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 74));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            card.Controls.Add(grid);

            // --- TẦNG 1 (TRÁI): BỘ LỌC KHOẢNG NGÀY & 4 NÚT CHỌN NHANH ---
            var pnlDate = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = Padding.Empty
            };

            Label Cap(string t, int w = 55) => new Label
            {
                Text = t,
                AutoSize = false,
                Width = LogicalToDeviceUnits(w),
                Height = LogicalToDeviceUnits(30),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = textColor
            };

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Width = LogicalToDeviceUnits(105),
                Margin = new Padding(0, 2, 6, 0)
            };

            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Width = LogicalToDeviceUnits(105),
                Margin = new Padding(0, 2, 8, 0)
            };

            // 4 Nút bộ lọc thời gian theo đúng yêu cầu
            btnAll = MakeQuickFilterBtn("Tất cả", 65);
            btnToday = MakeQuickFilterBtn("Hôm nay", 72);
            btn7Days = MakeQuickFilterBtn("7 ngày", 65);
            btn30Days = MakeQuickFilterBtn("30 ngày", 70);

            // Bấm Tất cả -> tải toàn bộ không giới hạn ngày
            btnAll.Click += async (s, e) =>
            {
                _isAllTime = true;
                HighlightFilterButton(btnAll);
                await LoadOrdersPagedAsync(1);
            };

            // Bấm Hôm nay -> lấy ngày hôm nay
            btnToday.Click += async (s, e) =>
            {
                _isAllTime = false;
                dtpFrom.Value = DateTime.Today;
                dtpTo.Value = DateTime.Today;
                HighlightFilterButton(btnToday);
                await LoadOrdersPagedAsync(1);
            };

            // Bấm 7 ngày -> từ 6 ngày trước đến hôm nay
            btn7Days.Click += async (s, e) =>
            {
                _isAllTime = false;
                dtpFrom.Value = DateTime.Today.AddDays(-6);
                dtpTo.Value = DateTime.Today;
                HighlightFilterButton(btn7Days);
                await LoadOrdersPagedAsync(1);
            };

            // Bấm 30 ngày -> từ 29 ngày trước đến hôm nay
            btn30Days.Click += async (s, e) =>
            {
                _isAllTime = false;
                dtpFrom.Value = DateTime.Today.AddDays(-29);
                dtpTo.Value = DateTime.Today;
                HighlightFilterButton(btn30Days);
                await LoadOrdersPagedAsync(1);
            };

            // Khi người dùng chọn lại ngày thủ công thì chuyển sang chế độ lọc theo ngày
            dtpFrom.ValueChanged += (s, e) => { _isAllTime = false; HighlightFilterButton(null); };
            dtpTo.ValueChanged += (s, e) => { _isAllTime = false; HighlightFilterButton(null); };

            pnlDate.Controls.Add(Cap("Từ:"));
            pnlDate.Controls.Add(dtpFrom);
            pnlDate.Controls.Add(Cap("Đến:"));
            pnlDate.Controls.Add(dtpTo);
            pnlDate.Controls.Add(btnAll);
            pnlDate.Controls.Add(btnToday);
            pnlDate.Controls.Add(btn7Days);
            pnlDate.Controls.Add(btn30Days);
            grid.Controls.Add(pnlDate, 0, 0);

            // --- TẦNG 1 (PHẢI): NÚT XUẤT EXCEL / CSV ---
            var pnlExport = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = Padding.Empty
            };
            btnExport = MakeBtn("📥  XUẤT EXCEL / CSV", successColor, 160);
            btnExport.Click += (s, e) => ExportOrdersToExcel();
            pnlExport.Controls.Add(btnExport);
            grid.Controls.Add(pnlExport, 1, 0);

            // --- TẦNG 2 (TRÁI): Ô TÌM KIẾM TỪ KHÓA & NÚT LÀM MỚI ---
            var pnlSearch = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                Margin = Padding.Empty
            };

            pnlSearch.Controls.Add(Cap("Tìm kiếm:", 65));

            txtSearch = new TextBox
            {
                Width = LogicalToDeviceUnits(320),
                Height = LogicalToDeviceUnits(30),
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 2, 8, 0),
                PlaceholderText = "Nhập mã đơn, tên khách, SĐT, thu ngân..."
            };
            txtSearch.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await LoadOrdersPagedAsync(1);
                }
            };
            pnlSearch.Controls.Add(txtSearch);

            btnSearch = MakeBtn("🔍  TÌM KIẾM", primaryColor, 105);
            btnReset = MakeBtn("🔄  LÀM MỚI", mutedColor, 95);

            btnSearch.Click += async (s, e) => await LoadOrdersPagedAsync(1);
            btnReset.Click += async (s, e) =>
            {
                txtSearch.Clear();
                _isAllTime = true;
                HighlightFilterButton(btnAll);
                dtpFrom.Value = DateTime.Today.AddDays(-29);
                dtpTo.Value = DateTime.Today;
                await LoadOrdersPagedAsync(1);
            };

            pnlSearch.Controls.Add(btnSearch);
            pnlSearch.Controls.Add(btnReset);
            grid.Controls.Add(pnlSearch, 0, 1);

            return card;
        }

        private Button MakeQuickFilterBtn(string text, int width)
        {
            var b = new Button
            {
                Text = text,
                Width = LogicalToDeviceUnits(width),
                Height = LogicalToDeviceUnits(30),
                Margin = new Padding(0, 1, 5, 0),
                BackColor = Color.FromArgb(241, 245, 249),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderColor = borderGrid;
            return b;
        }

        private void HighlightFilterButton(Button? active)
        {
            var buttons = new[] { btnAll, btnToday, btn7Days, btn30Days };
            foreach (var b in buttons)
            {
                if (b == null) continue;
                if (b == active)
                {
                    b.BackColor = primaryColor;
                    b.ForeColor = Color.White;
                    b.FlatAppearance.BorderColor = primaryColor;
                }
                else
                {
                    b.BackColor = Color.FromArgb(241, 245, 249);
                    b.ForeColor = Color.FromArgb(51, 65, 85);
                    b.FlatAppearance.BorderColor = borderGrid;
                }
            }
        }

        private Button MakeBtn(string text, Color color, int width)
        {
            var b = new Button
            {
                Text = text,
                Width = LogicalToDeviceUnits(width),
                Height = LogicalToDeviceUnits(30),
                Margin = new Padding(0, 1, 6, 0),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // =========================================================================
        // BƯỚC 3: PHÂN VÙNG CHÍNH BẰNG TABLELAYOUTPANEL ĐẢM BẢO KHÔNG BỊ CHE KHUẤT
        // =========================================================================
        private Control BuildContentSplit()
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = backgroundColor,
                Margin = Padding.Empty
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ================= NỬA TRÁI: BẢNG DANH SÁCH ĐƠN HÀNG =================
            var leftCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = cardBg,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 6, 0)
            };
            leftCard.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(28))); // Tiêu đề
            leftCard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));                        // Bảng đơn hàng
            leftCard.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(42))); // Thanh phân trang

            var lblTitleLeft = new Label
            {
                Text = "DANH SÁCH ĐƠN HÀNG ĐÃ LẬP",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var pnlPagination = BuildPaginationPanel();

            dgvOrders = CreateGrid();
            // Điều chỉnh độ rộng tối thiểu để không bị hiện dấu "..."
            AddCol(dgvOrders, "Code", "Mã hóa đơn", 26, DataGridViewContentAlignment.MiddleLeft, 165);
            AddCol(dgvOrders, "Time", "Thời gian", 20, DataGridViewContentAlignment.MiddleCenter, 125);
            AddCol(dgvOrders, "Cashier", "Thu ngân", 14, DataGridViewContentAlignment.MiddleLeft, 85);
            AddCol(dgvOrders, "Customer", "Khách hàng", 18, DataGridViewContentAlignment.MiddleLeft, 110);
            AddCol(dgvOrders, "Final", "Thành tiền", 16, DataGridViewContentAlignment.MiddleRight, 95);
            AddCol(dgvOrders, "Method", "Thanh toán", 12, DataGridViewContentAlignment.MiddleCenter, 80);

            dgvOrders.SelectionChanged += async (s, e) => await OnOrderSelectionChanged();
            dgvOrders.CellClick += async (s, e) => await OnOrderSelectionChanged();

            leftCard.Controls.Add(lblTitleLeft, 0, 0);
            leftCard.Controls.Add(dgvOrders, 0, 1);
            leftCard.Controls.Add(pnlPagination, 0, 2);
            table.Controls.Add(leftCard, 0, 0);

            // ================= NỬA PHẢI: KHUNG CHI TIẾT HÓA ĐƠN =================
            // Sử dụng TableLayoutPanel 4 hàng để không bao giờ bị đè lấn
            var rightCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = cardBg,
                Padding = new Padding(12),
                Margin = new Padding(6, 0, 0, 0)
            };
            rightCard.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(28)));  // Hàng 0: Tiêu đề
            rightCard.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(128))); // Hàng 1: Thẻ thông tin Hóa đơn (128px)
            rightCard.RowStyles.Add(new RowStyle(SizeType.Percent, 100));                         // Hàng 2: Bảng chi tiết mặt hàng
            rightCard.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(72)));  // Hàng 3: Thẻ Footer tổng kết tiền

            var lblTitleRight = new Label
            {
                Text = "CHI TIẾT MẶT HÀNG TRONG HÓA ĐƠN",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var pnlReceiptHeader = BuildReceiptHeaderCard();

            dgvDetails = CreateGrid();
            // Tăng minWidth cột mã vạch lên 130px để hiển thị trọn vẹn 13 số không bị ba chấm (...)
            AddCol(dgvDetails, "Index", "#", 7, DataGridViewContentAlignment.MiddleCenter, 35);
            AddCol(dgvDetails, "Barcode", "Mã vạch", 25, DataGridViewContentAlignment.MiddleLeft, 130);
            AddCol(dgvDetails, "ProductName", "Tên sản phẩm", 38, DataGridViewContentAlignment.MiddleLeft, 150);
            AddCol(dgvDetails, "Quantity", "SL", 9, DataGridViewContentAlignment.MiddleCenter, 42);
            AddCol(dgvDetails, "UnitPrice", "Đơn giá", 17, DataGridViewContentAlignment.MiddleRight, 85);
            AddCol(dgvDetails, "LineTotal", "Thành tiền", 19, DataGridViewContentAlignment.MiddleRight, 95);

            var pnlReceiptFooter = BuildReceiptFooterCard();

            rightCard.Controls.Add(lblTitleRight, 0, 0);
            rightCard.Controls.Add(pnlReceiptHeader, 0, 1);
            rightCard.Controls.Add(dgvDetails, 0, 2);
            rightCard.Controls.Add(pnlReceiptFooter, 0, 3);
            table.Controls.Add(rightCard, 1, 0);

            return table;
        }

        // =========================================================================
        // TIÊU CHÍ 25: THANH ĐIỀU HƯỚNG PHÂN TRANG (PAGINATION BAR)
        // =========================================================================
        private Control BuildPaginationPanel()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(6, 4, 6, 4),
                Margin = new Padding(0, 4, 0, 0)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };

            Button MakePageBtn(string text, int width = 68)
            {
                var b = new Button
                {
                    Text = text,
                    Width = LogicalToDeviceUnits(width),
                    Height = LogicalToDeviceUnits(30),
                    Margin = new Padding(0, 2, 6, 0),
                    BackColor = Color.FromArgb(241, 245, 249),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                b.FlatAppearance.BorderColor = borderGrid;
                return b;
            }

            btnFirstPage = MakePageBtn("⏮ Đầu", 66);
            btnPrevPage = MakePageBtn("◀ Trước", 72);

            lblPageInfo = new Label
            {
                Text = "Trang 1 / 1  (0 đơn)",
                AutoSize = false,
                Width = LogicalToDeviceUnits(190),
                Height = LogicalToDeviceUnits(30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = primaryColor,
                Margin = new Padding(2, 2, 2, 0)
            };

            btnNextPage = MakePageBtn("Sau ▶", 72);
            btnLastPage = MakePageBtn("Cuối ⏭", 66);

            // Sự kiện bấm nút chuyển trang (Tiêu chí 25)
            btnFirstPage.Click += async (s, e) =>
            {
                if (_currentPage > 1) await LoadOrdersPagedAsync(1);
            };
            btnPrevPage.Click += async (s, e) =>
            {
                if (_currentPage > 1) await LoadOrdersPagedAsync(_currentPage - 1);
            };
            btnNextPage.Click += async (s, e) =>
            {
                if (_currentPage < _totalPages) await LoadOrdersPagedAsync(_currentPage + 1);
            };
            btnLastPage.Click += async (s, e) =>
            {
                if (_currentPage < _totalPages) await LoadOrdersPagedAsync(_totalPages);
            };

            flow.Controls.Add(btnFirstPage);
            flow.Controls.Add(btnPrevPage);
            flow.Controls.Add(lblPageInfo);
            flow.Controls.Add(btnNextPage);
            flow.Controls.Add(btnLastPage);

            pnl.Controls.Add(flow);
            return pnl;
        }

        // =========================================================================
        // BƯỚC 4: THẺ THÔNG TIN HÓA ĐƠN ĐƯỢC CHIA THÀNH 4 HÀNG RÕ RÀNG (KHÔNG BỊ CHE)
        // =========================================================================
        private Control BuildReceiptHeaderCard()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252),
                Padding = new Padding(12, 6, 12, 6),
                Margin = new Padding(0, 0, 0, 6)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(28))); // Hàng 0: Mã HĐ + Badge hoàn tất
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(26))); // Hàng 1: Ngày lập + Thu ngân
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(26))); // Hàng 2: Khách hàng + Thanh toán
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(26))); // Hàng 3: Số ĐT + Tích lũy

            lblReceiptCode = new Label
            {
                Text = "MÃ HÓA ĐƠN: ---",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = primaryColor,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            lblReceiptStatusBadge = new Label
            {
                Text = "[ ĐÃ HOÀN TẤT ]",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = successColor,
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true
            };

            Label MakeInfo(string initial) => new Label
            {
                Text = initial,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            lblReceiptDate = MakeInfo("🕒 Ngày lập: ---");
            lblReceiptCashier = MakeInfo("👤 Thu ngân: ---");
            lblReceiptCustomer = MakeInfo("👥 Khách hàng: ---");
            lblReceiptPayment = MakeInfo("💳 Thanh toán: ---");
            lblReceiptPhone = MakeInfo("📞 Số ĐT: ---");
            lblReceiptPoints = MakeInfo("🎁 Tích lũy: ---");

            // Đặt các ô vào đúng vị trí hàng/cột tuyệt đối
            grid.Controls.Add(lblReceiptCode, 0, 0);
            grid.Controls.Add(lblReceiptStatusBadge, 1, 0);

            grid.Controls.Add(lblReceiptDate, 0, 1);
            grid.Controls.Add(lblReceiptCashier, 1, 1);

            grid.Controls.Add(lblReceiptCustomer, 0, 2);
            grid.Controls.Add(lblReceiptPayment, 1, 2);

            grid.Controls.Add(lblReceiptPhone, 0, 3);
            grid.Controls.Add(lblReceiptPoints, 1, 3);

            pnl.Controls.Add(grid);
            return pnl;
        }

        // =========================================================================
        // BƯỚC 5: KHUNG TỔNG KẾT THANH TOÁN CHÂN HÓA ĐƠN (RECEIPT FOOTER)
        // =========================================================================
        private Control BuildReceiptFooterCard()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(238, 242, 255),
                Padding = new Padding(12, 6, 12, 6)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.FromArgb(238, 242, 255)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            lblSubTotal = new Label
            {
                Text = "Tạm tính tiền hàng: 0 đ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            lblCashReceived = new Label
            {
                Text = "Tiền khách đưa: 0 đ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true
            };

            lblDiscount = new Label
            {
                Text = "Giảm giá / Điểm: 0 đ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = dangerColor,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            lblFinalAmount = new Label
            {
                Text = "THANH TOÁN: 0 đ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                ForeColor = primaryColor,
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true
            };

            grid.Controls.Add(lblSubTotal, 0, 0);
            grid.Controls.Add(lblCashReceived, 1, 0);
            grid.Controls.Add(lblDiscount, 0, 1);
            grid.Controls.Add(lblFinalAmount, 1, 1);

            pnl.Controls.Add(grid);
            return pnl;
        }

        private DataGridView CreateGrid()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = cardBg,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = borderGrid,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9F)
            };

            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 37, 84);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight = LogicalToDeviceUnits(34);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.RowTemplate.Height = LogicalToDeviceUnits(32);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgv.DefaultCellStyle.SelectionForeColor = textColor;
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = textColor;

            return dgv;
        }

        private void AddCol(DataGridView dgv, string name, string header, float fillWeight,
            DataGridViewContentAlignment align, int minWidth)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = fillWeight,
                MinimumWidth = LogicalToDeviceUnits(minWidth),
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            col.DefaultCellStyle.Alignment = align;
            dgv.Columns.Add(col);
        }

        // =========================================================================
        // TIÊU CHÍ 25: NẠP DỮ LIỆU ĐƠN HÀNG PHÂN TRANG TỪ SERVER API
        // =========================================================================
        private async Task LoadOrdersPagedAsync(int targetPage)
        {
            if (_busy) return;
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                lblStatus.Text = "Chưa đăng nhập hệ thống.";
                return;
            }

            _busy = true;
            btnSearch.Enabled = false;
            lblStatus.Text = $"Đang tải danh sách đơn hàng (Trang {targetPage})...";
            UseWaitCursor = true;

            try
            {
                ApiClientService.Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);

                DateTime from = dtpFrom.Value.Date;
                DateTime to = dtpTo.Value.Date;

                // Nếu chọn "Tất cả" thì không truyền from & to để lấy toàn bộ lịch sử
                string url = _isAllTime
                    ? $"orders?page={targetPage}&pageSize={_pageSize}"
                    : $"orders?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&page={targetPage}&pageSize={_pageSize}";

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    url += $"&search={Uri.EscapeDataString(txtSearch.Text.Trim())}";
                }

                using var response = await ApiClientService.Client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    string err = await response.Content.ReadAsStringAsync();
                    lblStatus.Text = "Không tải được danh sách đơn hàng.";
                    MessageBox.Show($"Lỗi gọi API: {(int)response.StatusCode}\n{err}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var pagedResult = await response.Content.ReadFromJsonAsync<OrderPagedResponseDto>(JsonOpt);
                if (pagedResult == null) return;

                _currentPage = pagedResult.CurrentPage;
                _totalPages = Math.Max(1, pagedResult.TotalPages);
                _totalRecords = pagedResult.TotalRecords;
                _ordersList = pagedResult.Data;

                dgvOrders.Rows.Clear();
                dgvDetails.Rows.Clear();
                ResetReceiptDetails();

                decimal pageTotalRev = 0;
                foreach (var o in pagedResult.Data)
                {
                    pageTotalRev += o.FinalAmount;

                    string custName = o.CustomerName;
                    if (string.IsNullOrWhiteSpace(custName) || custName == "20" || int.TryParse(custName, out _))
                    {
                        custName = o.CustomerId.HasValue ? $"Khách #{o.CustomerId.Value}" : "Khách vãng lai";
                    }

                    int rowIdx = dgvOrders.Rows.Add(
                        o.OrderCode,
                        o.CreatedAt.ToString("dd/MM/yyyy HH:mm", Vi),
                        o.CashierUsername,
                        custName,
                        o.FinalAmount.ToString("N0", Vi) + " đ",
                        o.PaymentMethod == "Cash" ? "Tiền mặt" : "CK / Thẻ"
                    );
                    dgvOrders.Rows[rowIdx].Tag = o;
                }

                // TIÊU CHÍ 25: CẬP NHẬT TRẠNG THÁI THANH ĐIỀU HƯỚNG PHÂN TRANG
                lblPageInfo.Text = $"Trang {_currentPage} / {_totalPages}  ({_totalRecords} đơn)";
                btnFirstPage.Enabled = _currentPage > 1;
                btnPrevPage.Enabled = _currentPage > 1;
                btnNextPage.Enabled = _currentPage < _totalPages;
                btnLastPage.Enabled = _currentPage < _totalPages;

                lblStatus.Text = $"Hiển thị {pagedResult.Data.Count}/{_totalRecords} hóa đơn  |  Trang {_currentPage}/{_totalPages}  |  Doanh thu trang: {pageTotalRev:N0} đ";

                // TỰ ĐỘNG CHỌN ĐƠN HÀNG ĐẦU TIÊN VÀ TẢI CHI TIẾT NGAY LẬP TỨC
                if (pagedResult.Data.Count > 0 && dgvOrders.Rows.Count > 0)
                {
                    dgvOrders.ClearSelection();
                    dgvOrders.Rows[0].Selected = true;
                    await LoadOrderDetailAsync(pagedResult.Data[0].OrderId);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi kết nối khi tải danh sách đơn hàng.";
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi kết nối",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _busy = false;
                btnSearch.Enabled = true;
                UseWaitCursor = false;
            }
        }

        private async Task OnOrderSelectionChanged()
        {
            if (dgvOrders.CurrentRow != null && dgvOrders.CurrentRow.Tag is OrderListDto order)
            {
                await LoadOrderDetailAsync(order.OrderId);
            }
        }

        private void ResetReceiptDetails()
        {
            lblReceiptCode.Text = "MÃ HÓA ĐƠN: ---";
            lblReceiptStatusBadge.Text = "";
            lblReceiptDate.Text = "🕒 Ngày lập: ---";
            lblReceiptCashier.Text = "👤 Thu ngân: ---";
            lblReceiptCustomer.Text = "👥 Khách hàng: ---";
            lblReceiptPayment.Text = "💳 Thanh toán: ---";
            lblReceiptPhone.Text = "📞 Số ĐT: ---";
            lblReceiptPoints.Text = "🎁 Tích lũy: ---";

            lblSubTotal.Text = "Tạm tính tiền hàng: 0 đ";
            lblDiscount.Text = "Giảm giá / Điểm: 0 đ";
            lblFinalAmount.Text = "THANH TOÁN: 0 đ";
            lblCashReceived.Text = "Tiền khách đưa: 0 đ";
        }

        // =========================================================================
        // BƯỚC 6: TẢI CHI TIẾT MẶT HÀNG TRONG HÓA ĐƠN (GET api/orders/{id})
        // =========================================================================
        private async Task LoadOrderDetailAsync(int orderId)
        {
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken)) return;

            try
            {
                ApiClientService.Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);

                using var response = await ApiClientService.Client.GetAsync($"orders/{orderId}");
                if (!response.IsSuccessStatusCode) return;

                var detail = await response.Content.ReadFromJsonAsync<OrderDetailResponseDto>(JsonOpt);
                if (detail == null) return;

                string custName = detail.CustomerName;
                if (string.IsNullOrWhiteSpace(custName) || custName == "20" || int.TryParse(custName, out _))
                {
                    custName = detail.CustomerId.HasValue ? $"Khách #{detail.CustomerId.Value}" : "Khách vãng lai";
                }

                // Cập nhật thẻ Header Hóa đơn (không bị che khuất)
                lblReceiptCode.Text = $"MÃ HÓA ĐƠN: {detail.OrderCode}";
                lblReceiptStatusBadge.Text = "[ ĐÃ HOÀN TẤT ]";
                lblReceiptDate.Text = $"🕒 Ngày lập: {detail.CreatedAt:dd/MM/yyyy HH:mm:ss}";
                lblReceiptCashier.Text = $"👤 Thu ngân: {detail.CashierUsername}";
                lblReceiptCustomer.Text = $"👥 Khách hàng: {custName}";
                lblReceiptPayment.Text = $"💳 Thanh toán: {(detail.PaymentMethod == "Cash" ? "Tiền mặt" : "Chuyển khoản / Thẻ")}";
                lblReceiptPhone.Text = $"📞 Số ĐT: {(string.IsNullOrWhiteSpace(detail.CustomerPhone) ? "---" : detail.CustomerPhone)}";
                lblReceiptPoints.Text = $"🎁 Tích lũy: +{detail.RewardPoints} điểm";

                // Nạp bảng sản phẩm chi tiết
                dgvDetails.Rows.Clear();
                int stt = 1;
                foreach (var item in detail.Items)
                {
                    dgvDetails.Rows.Add(
                        stt++,
                        item.Barcode,
                        item.ProductName,
                        item.Quantity.ToString("N0", Vi),
                        item.UnitPrice.ToString("N0", Vi) + " đ",
                        item.LineTotal.ToString("N0", Vi) + " đ"
                    );
                }

                // Cập nhật chân Footer Hóa đơn
                lblSubTotal.Text = $"Tạm tính tiền hàng: {detail.TotalAmount:N0} đ";
                lblDiscount.Text = $"Giảm giá / Điểm: -{detail.DiscountAmount:N0} đ";
                lblFinalAmount.Text = $"THANH TOÁN: {detail.FinalAmount:N0} đ";
                lblCashReceived.Text = $"Khách đưa: {detail.AmountReceived:N0} đ (Thối: {detail.ChangeAmount:N0} đ)";
            }
            catch
            {
                // Bỏ qua lỗi tạm thời khi click nhanh
            }
        }

        // =========================================================================
        // TIÊU CHÍ 48: XUẤT TOÀN BỘ DANH SÁCH ĐƠN HÀNG RA TỆP CSV (exportAll=true)
        // =========================================================================
        private async void ExportOrdersToExcel()
        {
            if (_totalRecords == 0)
            {
                MessageBox.Show("Chưa có đơn hàng nào trong khoảng thời gian này để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "Tệp CSV UTF-8 (*.csv)|*.csv",
                FileName = $"DanhSachDonHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = "csv"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                DateTime from = dtpFrom.Value.Date;
                DateTime to = dtpTo.Value.Date;
                string url = _isAllTime
                    ? "orders?exportAll=true"
                    : $"orders?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}&exportAll=true";

                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    url += $"&search={Uri.EscapeDataString(txtSearch.Text.Trim())}";
                }

                ApiClientService.Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);

                using var response = await ApiClientService.Client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Không thể lấy dữ liệu để xuất file!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var pagedResult = await response.Content.ReadFromJsonAsync<OrderPagedResponseDto>(JsonOpt);
                var allOrders = pagedResult?.Data ?? new List<OrderListDto>();

                var sb = new StringBuilder();
                sb.AppendLine("HỆ THỐNG SIÊU THỊ MINI PURE FOOD - DANH SÁCH ĐƠN HÀNG");
                sb.AppendLine($"Thời gian xuất: {DateTime.Now:dd/MM/yyyy HH:mm:ss},Người xuất: {SessionManager.CurrentUsername}");
                sb.AppendLine($"Khoảng thời gian: {(_isAllTime ? "Toàn bộ thời gian" : $"Từ {dtpFrom.Value:dd/MM/yyyy} đến {dtpTo.Value:dd/MM/yyyy}")}");
                sb.AppendLine();
                sb.AppendLine("STT,Mã đơn hàng,Thời gian lập,Thu ngân,Khách hàng,Số điện thoại,Phương thức thanh toán,Tổng tiền hàng (VNĐ),Giảm giá (VNĐ),Thực thu (VNĐ),Tiền khách đưa (VNĐ),Tiền thối (VNĐ),Điểm tích lũy,Trạng thái");

                int idx = 1;
                decimal sumFinal = 0;
                foreach (var o in allOrders)
                {
                    sumFinal += o.FinalAmount;
                    string cName = o.CustomerName;
                    if (string.IsNullOrWhiteSpace(cName) || cName == "20" || int.TryParse(cName, out _))
                    {
                        cName = o.CustomerId.HasValue ? $"Khách #{o.CustomerId.Value}" : "Khách vãng lai";
                    }

                    sb.AppendLine(string.Join(",",
                        idx++,
                        "\"" + o.OrderCode + "\"",
                        "\"" + o.CreatedAt.ToString("dd/MM/yyyy HH:mm") + "\"",
                        "\"" + o.CashierUsername + "\"",
                        "\"" + cName + "\"",
                        "\"" + o.CustomerPhone + "\"",
                        "\"" + (o.PaymentMethod == "Cash" ? "Tiền mặt" : "Chuyển khoản") + "\"",
                        o.TotalAmount.ToString("0"),
                        o.DiscountAmount.ToString("0"),
                        o.FinalAmount.ToString("0"),
                        o.AmountReceived.ToString("0"),
                        o.ChangeAmount.ToString("0"),
                        o.RewardPoints,
                        "\"" + o.Status + "\""
                    ));
                }

                sb.AppendLine();
                sb.AppendLine($"TỔNG CỘNG SỐ ĐƠN HÀNG,{allOrders.Count} đơn");
                sb.AppendLine($"TỔNG DOANH THU THỰC THU,{sumFinal:0} VNĐ");

                File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));

                var res = MessageBox.Show(
                    $"Xuất danh sách đơn hàng thành công!\n\nĐường dẫn: {dlg.FileName}\n\nBạn có muốn mở tệp ngay không?",
                    "Thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (res == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất file: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // =========================================================================
    // TIÊU CHÍ 25: CÁC LỚP DTO PHỤC VỤ PHÂN TRANG VÀ CHI TIẾT ĐƠN HÀNG
    // =========================================================================
    public class OrderPagedResponseDto
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 8;
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; } = 1;
        public List<OrderListDto> Data { get; set; } = new();
    }

    public class OrderListDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int TotalItems { get; set; }
        public int CashierId { get; set; }
        public string CashierUsername { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal ChangeAmount { get; set; }
        public int RewardPoints { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Completed";
    }

    public class OrderDetailResponseDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CashierUsername { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public decimal ChangeAmount { get; set; }
        public int RewardPoints { get; set; }
        public string Status { get; set; } = "Completed";
        public List<OrderItemDetailDto> Items { get; set; } = new();
    }

    public class OrderItemDetailDto
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
