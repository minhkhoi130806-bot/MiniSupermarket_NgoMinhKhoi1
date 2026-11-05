using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    // =========================================================================
    // MÀN HÌNH BÁO CÁO DOANH THU & HIỆU SUẤT KINH DOANH
    // - Đã tách Danh sách đơn hàng sang màn hình Quản lý Đơn hàng riêng biệt.
    // - Bổ sung Biểu đồ tròn (Pie Chart) cơ cấu doanh thu theo nhóm hàng (Yêu cầu của Thầy).
    // - Bổ sung Thẻ chỉ số tổng quan (KPI Summary Cards) theo Tiêu chí 44:
    //   + Doanh thu hôm nay (VNĐ)
    //   + Tổng số hóa đơn đã xuất
    //   + Số lượng khách hàng mới đăng ký
    //   + Số mặt hàng sắp hết hàng (tồn kho <= 10)
    // - Biểu đồ cột Doanh thu theo ngày (Tiêu chí 45)
    // - Thống kê Top sản phẩm bán chạy nhất (Tiêu chí 46)
    // =========================================================================
    public partial class FormQuickReport : Form
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color textColor = Color.FromArgb(24, 30, 48);
        private readonly Color primaryColor = Color.FromArgb(41, 100, 180);
        private readonly Color mutedColor = Color.FromArgb(100, 116, 139);
        private readonly Color successColor = Color.FromArgb(22, 163, 74);
        private readonly Color dangerColor = Color.FromArgb(220, 38, 38);
        private readonly Color warningColor = Color.FromArgb(245, 158, 11);
        private readonly Color purpleColor = Color.FromArgb(124, 58, 237);

        // Các điều khiển bộ lọc ngày
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private Button btnView = null!;
        private Button btnExport = null!;
        private Button btnToday = null!;
        private Button btn7 = null!;
        private Button btn30 = null!;
        private Button btnMonth = null!;

        // =====================================================================
        // TIÊU CHÍ 44: CÁC THẺ CHỈ SỐ TỔNG QUAN (KPI SUMMARY CARDS)
        // =====================================================================
        private Label lblRevenue = null!;     // Doanh thu kỳ chọn / Hôm nay
        private Label lblOrders = null!;      // Tổng số hóa đơn đã xuất
        private Label lblCustomers = null!;   // TIÊU CHÍ 44: Số lượng khách hàng mới đăng ký
        private Label lblLowStock = null!;    // TIÊU CHÍ 44: Số mặt hàng sắp hết hàng (tồn kho <= 10)
        private Label lblPayment = null!;     // Tỷ trọng Tiền mặt / Chuyển khoản
        private Label lblStatus = null!;

        // =====================================================================
        // BIỂU ĐỒ & BẢNG SỐ LIỆU
        // =====================================================================
        private RevenueBarChart chart = null!;        // Tiêu chí 45: Biểu đồ cột doanh thu theo ngày
        private RevenuePieChart pieChart = null!;     // Yêu cầu của Thầy: Biểu đồ tròn cơ cấu nhóm hàng
        private DataGridView dgvTop = null!;          // Tiêu chí 46: Top sản phẩm bán chạy
        private DataGridView dgvCategory = null!;     // Bảng đối chiếu tỷ trọng doanh thu theo nhóm hàng

        private ReportDto? _data;
        private bool _busy;
        private bool _refreshing;
        private bool _followToday = true;
        private readonly System.Windows.Forms.Timer _autoTimer = new() { Interval = 10000 };

        public FormQuickReport()
        {
            BuildLayout();
        }

        // =====================================================================
        // BƯỚC 1: XÂY DỰNG BỐ CỤC GIAO DIỆN
        // =====================================================================
        private void BuildLayout()
        {
            Text = "Báo cáo doanh thu & Hiệu suất kinh doanh";
            Size = new Size(LogicalToDeviceUnits(1280), LogicalToDeviceUnits(780));
            MinimumSize = new Size(LogicalToDeviceUnits(900), LogicalToDeviceUnits(550));
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12),
                BackColor = backgroundColor
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(58))); // Hàng 0: Bộ lọc ngày
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(96))); // Hàng 1: Thẻ KPI Tiêu chí 44
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 54));                        // Hàng 2: Biểu đồ Cột + Biểu đồ Tròn
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 46));                        // Hàng 3: Top SP + Bảng cơ cấu nhóm hàng
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(26))); // Hàng 4: Thanh trạng thái
            Controls.Add(root);

            root.Controls.Add(BuildFilterBar(), 0, 0);
            root.Controls.Add(BuildKpiRow(), 0, 1);
            root.Controls.Add(BuildChartsRow(), 0, 2);
            root.Controls.Add(BuildTablesRow(), 0, 3);

            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Chưa tải dữ liệu"
            };
            root.Controls.Add(lblStatus, 0, 4);

            Load += async (s, e) =>
            {
                SetRange(DateTime.Today.AddDays(-29), DateTime.Today);
                await LoadReportAsync();

                // Tự động cập nhật số liệu hôm nay từ database theo chu kỳ 10 giây
                _autoTimer.Tick += async (s2, e2) => await RefreshSilentAsync();
                _autoTimer.Start();
            };

            FormClosed += (s, e) =>
            {
                _autoTimer.Stop();
                _autoTimer.Dispose();
            };
        }

        // =====================================================================
        // BƯỚC 2: BỘ LỌC NGÀY VÀ CÁC NÚT THAO TÁC NHANH
        // =====================================================================
        private Control BuildFilterBar()
        {
            var bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12, 10, 12, 6),
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };

            Label Cap(string t) => new Label
            {
                Text = t,
                AutoSize = false,
                Width = LogicalToDeviceUnits(66),
                Height = LogicalToDeviceUnits(30),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = textColor,
                Margin = new Padding(0, 0, 4, 0)
            };

            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Width = LogicalToDeviceUnits(115),
                Margin = new Padding(0, 3, 12, 0)
            };
            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Width = LogicalToDeviceUnits(115),
                Margin = new Padding(0, 3, 12, 0)
            };

            btnToday = MakeButton("Hôm nay", mutedColor, 85);
            btn7 = MakeButton("7 ngày", mutedColor, 80);
            btn30 = MakeButton("30 ngày", mutedColor, 85);
            btnMonth = MakeButton("Tháng này", mutedColor, 95);
            btnView = MakeButton("XEM BÁO CÁO", primaryColor, 125);
            btnExport = MakeButton("XUẤT EXCEL", successColor, 120);

            btnToday.Click += async (s, e) => await QuickRange(DateTime.Today, DateTime.Today);
            btn7.Click += async (s, e) => await QuickRange(DateTime.Today.AddDays(-6), DateTime.Today);
            btn30.Click += async (s, e) => await QuickRange(DateTime.Today.AddDays(-29), DateTime.Today);
            btnMonth.Click += async (s, e) =>
                await QuickRange(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Today);
            btnView.Click += async (s, e) => await LoadReportAsync();
            btnExport.Click += (s, e) => ExportReport();

            bar.Controls.Add(Cap("Từ ngày"));
            bar.Controls.Add(dtpFrom);
            bar.Controls.Add(Cap("Đến ngày"));
            bar.Controls.Add(dtpTo);
            bar.Controls.Add(btnToday);
            bar.Controls.Add(btn7);
            bar.Controls.Add(btn30);
            bar.Controls.Add(btnMonth);
            bar.Controls.Add(btnView);
            bar.Controls.Add(btnExport);
            return bar;
        }

        private Button MakeButton(string text, Color color, int width)
        {
            var b = new Button
            {
                Text = text,
                Width = LogicalToDeviceUnits(width),
                Height = LogicalToDeviceUnits(32),
                Margin = new Padding(0, 1, 8, 0),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // =====================================================================
        // BƯỚC 3: DÃY THẺ CHỈ SỐ TỔNG QUAN (DASHBOARD KPI SUMMARY CARDS - TIÊU CHÍ 44)
        // 1. Doanh thu hôm nay / Kỳ chọn (VNĐ)
        // 2. Tổng số hóa đơn đã xuất
        // 3. Số lượng khách hàng mới đăng ký (Tiêu chí 44)
        // 4. Số mặt hàng sắp hết hàng (tồn kho <= 10) (Tiêu chí 44)
        // 5. Cơ cấu Tiền mặt / Chuyển khoản
        // =====================================================================
        private Control BuildKpiRow()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = backgroundColor
            };
            for (int i = 0; i < 5; i++)
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Thẻ 1: Doanh thu
            row.Controls.Add(MakeKpi("TỔNG DOANH THU", successColor, 17F, out lblRevenue), 0, 0);

            // Thẻ 2: Hóa đơn
            row.Controls.Add(MakeKpi("SỐ HÓA ĐƠN XUẤT", primaryColor, 17F, out lblOrders), 1, 0);

            // Thẻ 3: TIÊU CHÍ 44 - Số lượng khách hàng mới đăng ký
            row.Controls.Add(MakeKpi("KHÁCH HÀNG THÀNH VIÊN", purpleColor, 17F, out lblCustomers), 2, 0);

            // Thẻ 4: TIÊU CHÍ 44 - Mặt hàng sắp hết hàng (tồn kho <= 10)
            row.Controls.Add(MakeKpi("HÀNG SẮP HẾT ", dangerColor, 17F, out lblLowStock), 3, 0);

            // Thẻ 5: Tỷ trọng thanh toán tiền mặt vs chuyển khoản
            row.Controls.Add(MakeKpi("TIỀN MẶT / CK - THẺ", warningColor, 12F, out lblPayment), 4, 0);

            return row;
        }

        private Panel MakeKpi(string title, Color accent, float valueSize, out Label valueLabel)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(12, 8, 8, 8)
            };

            valueLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "0",
                Font = new Font("Segoe UI", valueSize, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            var lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(24),
                Text = title,
                Font = new Font("Segoe UI", 8.2F, FontStyle.Bold),
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var strip = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };

            card.Controls.Add(valueLabel);
            card.Controls.Add(lblTitle);
            card.Controls.Add(strip);
            return card;
        }

        // =====================================================================
        // BƯỚC 4: HÀNG 2 - BIỂU ĐỒ CỘT (TIÊU CHÍ 45) VÀ BIỂU ĐỒ TRÒN (YÊU CẦU CỦA THẦY)
        // =====================================================================
        private Control BuildChartsRow()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = backgroundColor
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55)); // 55% cho Biểu đồ cột
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45)); // 45% cho Biểu đồ tròn
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // 1. Biểu đồ cột theo ngày (Tiêu chí 45)
            chart = new RevenueBarChart { Dock = DockStyle.Fill, BarColor = primaryColor };
            var chartCard = MakeCard("BIỂU ĐỒ CỘT: DOANH THU THEO NGÀY ", chart);
            chartCard.Margin = new Padding(0, 0, 4, 0);

            // 2. Biểu đồ tròn theo nhóm hàng (Yêu cầu của Thầy)
            pieChart = new RevenuePieChart { Dock = DockStyle.Fill };
            var pieCard = MakeCard("BIỂU ĐỒ TRÒN: CƠ CẤU DOANH THU THEO NHÓM HÀNG", pieChart);
            pieCard.Margin = new Padding(4, 0, 0, 0);

            row.Controls.Add(chartCard, 0, 0);
            row.Controls.Add(pieCard, 1, 0);
            return row;
        }

        // =====================================================================
        // BƯỚC 5: HÀNG 3 - TOP SẢN PHẨM (TIÊU CHÍ 46) & BẢNG CƠ CẤU DOANH THU
        // (Thay thế cho danh sách đơn hàng đã bỏ ra ngoài)
        // =====================================================================
        private Control BuildTablesRow()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 4),
                BackColor = backgroundColor
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Bảng 1: Top 10 sản phẩm bán chạy nhất (Tiêu chí 46)
            dgvTop = CreateGrid();
            AddCol(dgvTop, "Rank", "#", 8, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvTop, "Name", "Tên sản phẩm", 52, DataGridViewContentAlignment.MiddleLeft);
            AddCol(dgvTop, "Qty", "SL đã bán", 16, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvTop, "Revenue", "Doanh thu", 24, DataGridViewContentAlignment.MiddleRight);
            var topCard = MakeCard("TOP SẢN PHẨM BÁN CHẠY NHẤT ", dgvTop);
            topCard.Margin = new Padding(0, 0, 4, 0);

            // Bảng 2: Bảng chi tiết doanh thu theo Danh mục hàng hóa (đối chiếu biểu đồ tròn)
            dgvCategory = CreateGrid();
            AddCol(dgvCategory, "CatRank", "#", 8, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvCategory, "CatName", "Nhóm danh mục hàng", 48, DataGridViewContentAlignment.MiddleLeft);
            AddCol(dgvCategory, "CatQty", "Tổng SL", 16, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvCategory, "CatRev", "Doanh thu", 28, DataGridViewContentAlignment.MiddleRight);
            var catCard = MakeCard("CHI TIẾT DOANH THU THEO NHÓM DANH MỤC HÀNG", dgvCategory);
            catCard.Margin = new Padding(4, 0, 0, 0);

            row.Controls.Add(topCard, 0, 0);
            row.Controls.Add(catCard, 1, 0);
            return row;
        }

        private Panel MakeCard(string title, Control content)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            content.Dock = DockStyle.Fill;
            var lbl = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(28),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(content);
            card.Controls.Add(lbl);
            return card;
        }

        private DataGridView CreateGrid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = LogicalToDeviceUnits(34),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI", 9.5F)
            };
            g.RowTemplate.Height = LogicalToDeviceUnits(32);
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 45, 65);
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(31, 45, 65);
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            return g;
        }

        private static void AddCol(DataGridView g, string name, string header, float weight, DataGridViewContentAlignment align)
        {
            g.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = 35,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = align }
            });
        }

        // =====================================================================
        // BƯỚC 6: TẢI DỮ LIỆU TỪ WEB API (GET reports/revenue)
        // =====================================================================
        private void SetRange(DateTime from, DateTime to)
        {
            dtpFrom.Value = from;
            dtpTo.Value = to;
        }

        private async Task QuickRange(DateTime from, DateTime to)
        {
            SetRange(from, to);
            await LoadReportAsync();
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            foreach (var b in new[] { btnView, btnExport, btnToday, btn7, btn30, btnMonth })
                b.Enabled = !busy;
            UseWaitCursor = busy;
        }

        private async Task LoadReportAsync()
        {
            if (_busy) return;

            _followToday = dtpTo.Value.Date >= DateTime.Today;

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;
            if (from > to)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                MessageBox.Show("Bạn chưa đăng nhập hoặc token đã hết hạn.", "Chưa xác thực",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ApiClientService.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);

            SetBusy(true);
            lblStatus.Text = "Đang tải báo cáo doanh thu & biểu đồ...";

            try
            {
                string url = "reports/revenue?from=" + from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                             "&to=" + to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                using var response = await ApiClientService.Client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    string detail = await response.Content.ReadAsStringAsync();
                    lblStatus.Text = "Tải báo cáo thất bại.";
                    MessageBox.Show(
                        $"Không tải được báo cáo.\nMã HTTP: {(int)response.StatusCode} ({response.StatusCode})\n{detail}",
                        "Lỗi API", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _data = await response.Content.ReadFromJsonAsync<ReportDto>() ?? new ReportDto();
                Render(_data, from, to);
            }
            catch (HttpRequestException ex)
            {
                lblStatus.Text = "Không thể kết nối máy chủ.";
                MessageBox.Show("Không thể kết nối API báo cáo.\n" + ex.Message, "Lỗi kết nối",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Tải báo cáo thất bại.";
                MessageBox.Show("Lỗi khi tải báo cáo:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Tải lại ngầm để cập nhật số liệu theo thời gian thực
        private async Task RefreshSilentAsync()
        {
            if (_busy || _refreshing || IsDisposed || !IsHandleCreated) return;
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken)) return;

            if (_followToday && dtpTo.Value.Date < DateTime.Today)
                dtpTo.Value = DateTime.Today;

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;
            if (from > to || to < DateTime.Today) return;

            _refreshing = true;
            try
            {
                ApiClientService.Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);

                string url = "reports/revenue?from=" + from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                             "&to=" + to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                using var response = await ApiClientService.Client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return;

                var data = await response.Content.ReadFromJsonAsync<ReportDto>();
                if (data == null || IsDisposed) return;

                _data = data;
                Render(data, from, to);
                lblStatus.Text += $"  |  Tự động cập nhật: {DateTime.Now:HH:mm:ss}";
            }
            catch
            {
                // Bỏ qua lỗi tạm thời
            }
            finally
            {
                _refreshing = false;
            }
        }

        private static string Money(decimal v) => v.ToString("N0", Vi) + " đ";

        // =====================================================================
        // BƯỚC 7: HIỂN THỊ DỮ LIỆU LÊN GIAO DIỆN & VẼ BIỂU ĐỒ
        // =====================================================================
        private void Render(ReportDto d, DateTime from, DateTime to)
        {
            var s = d.Summary ?? new ReportSummaryDto();

            // 1. Thẻ KPI
            lblRevenue.Text = Money(s.TotalRevenue);
            lblOrders.Text = s.TotalOrders.ToString("N0", Vi) + $" đơn ({s.TotalItems:N0} sp)";

            // TIÊU CHÍ 44: Hiển thị số lượng khách hàng mới đăng ký & Tồn kho thấp
            lblCustomers.Text = s.TotalCustomers.ToString("N0", Vi) + " thành viên";
            lblLowStock.Text = s.LowStockCount.ToString("N0", Vi) + " mặt hàng";

            lblPayment.Text = RevenueBarChart.Short(s.CashRevenue) + "  /  " + RevenueBarChart.Short(s.TransferRevenue);

            // 2. Biểu đồ cột doanh thu theo ngày (Tiêu chí 45)
            var days = new List<DailyRevenueDto>();
            for (var day = from; day <= to; day = day.AddDays(1))
            {
                var f = d.Daily.FirstOrDefault(x => x.Date.Date == day);
                days.Add(new DailyRevenueDto { Date = day, Revenue = f?.Revenue ?? 0, Orders = f?.Orders ?? 0 });
            }
            chart.Data = days;
            chart.Invalidate();

            // 3. Biểu đồ tròn cơ cấu nhóm hàng (Yêu cầu của Thầy)
            pieChart.Data = d.CategoryShares;
            pieChart.Invalidate();

            // 4. Bảng Top 10 sản phẩm bán chạy (Tiêu chí 46)
            dgvTop.Rows.Clear();
            int rank = 1;
            foreach (var p in d.TopProducts)
                dgvTop.Rows.Add(rank++, p.ProductName, p.Quantity.ToString("N0", Vi), p.Revenue.ToString("N0", Vi) + " đ");

            // 5. Bảng chi tiết doanh thu theo Nhóm hàng
            dgvCategory.Rows.Clear();
            int catRank = 1;
            foreach (var c in d.CategoryShares)
            {
                dgvCategory.Rows.Add(
                    catRank++,
                    c.CategoryName,
                    c.Quantity.ToString("N0", Vi),
                    c.Revenue.ToString("N0", Vi) + " đ"
                );
            }

            lblStatus.Text =
                $"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}  |  {s.TotalOrders} đơn hàng  |  Tổng doanh thu: {Money(s.TotalRevenue)}";
        }

        // =====================================================================
        // TIÊU CHÍ 48: XUẤT BÁO CÁO EXCEL (.XLSX) HOẶC CSV (.CSV)
        // =====================================================================
        private void ExportReport()
        {
            if (_data == null)
            {
                MessageBox.Show(
                    "Chưa có dữ liệu để xuất. Hãy bấm XEM BÁO CÁO trước.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Filter = "Tệp Excel (*.xlsx)|*.xlsx|Tệp CSV (*.csv)|*.csv",
                FileName = $"BaoCaoDoanhThu_{dtpFrom.Value:yyyyMMdd}_{dtpTo.Value:yyyyMMdd}.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                string cashier = SessionManager.CurrentUsername ?? "admin";
                ExcelExportHelper.ExportRevenue(dlg.FileName, _data, dtpFrom.Value.Date, dtpTo.Value.Date, cashier);

                var answer = MessageBox.Show(
                    $"Xuất báo cáo doanh thu thành công!\n\nĐường dẫn: {dlg.FileName}\n\nBạn có muốn mở file ngay không?",
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
                MessageBox.Show("Không thể xuất file:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // =========================================================================
    // BIỂU ĐỒ CỘT DOANH THU THEO NGÀY (TIÊU CHÍ 45 - TỰ VẼ BẰNG C# GDI+)
    // =========================================================================
    public class RevenueBarChart : Panel
    {
        public List<DailyRevenueDto> Data { get; set; } = new();
        public Color BarColor { get; set; } = Color.FromArgb(41, 100, 180);

        public RevenueBarChart()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }

        public static string Short(decimal v)
        {
            if (v >= 1_000_000_000m) return (v / 1_000_000_000m).ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) + " tỷ";
            if (v >= 1_000_000m) return (v / 1_000_000m).ToString("0.##", CultureInfo.GetCultureInfo("vi-VN")) + " tr";
            if (v >= 1_000m) return (v / 1_000m).ToString("0.#", CultureInfo.GetCultureInfo("vi-VN")) + "k";
            return v.ToString("0");
        }

        private static decimal NiceCeil(decimal max)
        {
            if (max <= 0) return 1;
            double m = Math.Pow(10, Math.Floor(Math.Log10((double)max)));
            double n = (double)max / m;
            double nice = n <= 1 ? 1 : n <= 2 ? 2 : n <= 5 ? 5 : 10;
            return (decimal)(nice * m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var small = new Font("Segoe UI", 8.5F);
            using var muted = new SolidBrush(Color.FromArgb(100, 116, 139));
            using var gridPen = new Pen(Color.FromArgb(226, 232, 240));
            using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var far = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };

            if (Data.Count == 0 || Data.All(x => x.Revenue <= 0))
            {
                g.DrawString("Không có doanh thu trong khoảng thời gian này", small, muted,
                    new RectangleF(0, 0, Width, Height), center);
                return;
            }

            int left = 56, right = 10, top = 14, bottom = 26;
            var plot = new Rectangle(left, top, Width - left - right, Height - top - bottom);
            if (plot.Width < 30 || plot.Height < 30) return;

            decimal niceMax = NiceCeil(Data.Max(x => x.Revenue));

            for (int i = 0; i <= 4; i++)
            {
                float y = plot.Bottom - plot.Height * i / 4f;
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                g.DrawString(Short(niceMax * i / 4m), small, muted,
                    new RectangleF(0, y - 10, left - 6, 20), far);
            }

            float slot = plot.Width / (float)Data.Count;
            float barW = Math.Max(2f, slot * 0.65f);
            int labelStep = Math.Max(1, (int)Math.Ceiling(46f / slot));
            using var barBrush = new SolidBrush(BarColor);

            for (int i = 0; i < Data.Count; i++)
            {
                var d = Data[i];
                float x = plot.Left + slot * i + (slot - barW) / 2f;
                float h = (float)(d.Revenue / niceMax) * plot.Height;

                if (h > 0)
                {
                    var rect = new RectangleF(x, plot.Bottom - h, barW, h);
                    g.FillRectangle(barBrush, rect);

                    if (slot >= 52)
                        g.DrawString(Short(d.Revenue), small, muted,
                            new RectangleF(x - 14, plot.Bottom - h - 18, barW + 28, 16), center);
                }

                if (i % labelStep == 0)
                    g.DrawString(d.Date.ToString("dd/MM"), small, muted,
                        new RectangleF(x - 14, plot.Bottom + 4, barW + 28, 18), center);
            }
        }
    }

    // =========================================================================
    // BIỂU ĐỒ TRÒN CƠ CẤU DOANH THU (PIE CHART - YÊU CẦU CỦA THẦY)
    // Tự vẽ 100% bằng C# GDI+ với bảng màu phân biệt, chú giải danh mục và phần trăm
    // =========================================================================
    // BIỂU ĐỒ TRÒN CƠ CẤU DOANH THU THEO NHÓM HÀNG (YÊU CẦU CỦA THẦY - VẼ GDI+)
    // Thiết kế Donut Chart hiện đại, tự động căn chỉnh chữ, chống đè chữ tuyệt đối
    // =========================================================================
    public class RevenuePieChart : Panel
    {
        public List<CategoryShareDto> Data { get; set; } = new();

        // Bảng màu sắc tương phản, hài hòa cho từng lát cắt danh mục
        private static readonly Color[] Palette = new[]
        {
            Color.FromArgb(16, 185, 129),   // Xanh lá hữu cơ
            Color.FromArgb(59, 130, 246),   // Xanh dương tươi
            Color.FromArgb(245, 158, 11),   // Vàng cam ấm
            Color.FromArgb(139, 92, 246),   // Tím thạch anh
            Color.FromArgb(239, 68, 68),    // Đỏ san hô
            Color.FromArgb(6, 182, 212),    // Xanh lơ cyan
            Color.FromArgb(236, 72, 153),   // Hồng sen
            Color.FromArgb(99, 102, 241),   // Xanh chàm indigo
            Color.FromArgb(20, 184, 166),   // Xanh ngọc lục bảo
            Color.FromArgb(100, 116, 139)   // Xám ghi
        };

        public RevenuePieChart()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using var fontNormal = new Font("Segoe UI", 8.8F);
            using var fontBold = new Font("Segoe UI", 8.8F, FontStyle.Bold);
            using var fontCenterLabel = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            using var fontCenterVal = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            using var textBrush = new SolidBrush(Color.FromArgb(30, 41, 59));
            using var mutedBrush = new SolidBrush(Color.FromArgb(100, 116, 139));
            using var centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var leftFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            using var rightFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };

            var validData = Data.Where(x => x.Revenue > 0).OrderByDescending(x => x.Revenue).ToList();
            decimal totalRevenue = validData.Sum(x => x.Revenue);

            if (validData.Count == 0 || totalRevenue <= 0)
            {
                g.DrawString("Chưa có số liệu doanh thu theo nhóm hàng", fontNormal, mutedBrush,
                    new RectangleF(0, 0, Width, Height), centerFormat);
                return;
            }

            // Gom nhóm danh mục: Hiển thị tối đa Top 5 nhóm hàng, phần còn lại gom vào "Nhóm khác"
            // Cách làm này giúp biểu đồ thoáng đãng, chuyên nghiệp chuẩn Dashboard BI
            var displayList = new List<(string Name, decimal Revenue, Color Color, double Percent)>();
            int topLimit = Math.Min(validData.Count, 5);
            for (int i = 0; i < topLimit; i++)
            {
                var d = validData[i];
                double pct = (double)(d.Revenue / totalRevenue * 100m);
                displayList.Add((d.CategoryName, d.Revenue, Palette[i % Palette.Length], pct));
            }
            if (validData.Count > 5)
            {
                decimal otherRev = validData.Skip(5).Sum(x => x.Revenue);
                double otherPct = (double)(otherRev / totalRevenue * 100m);
                displayList.Add(($"Nhóm khác ({validData.Count - 5})", otherRev, Palette[5 % Palette.Length], otherPct));
            }

            // 1. Phân vùng: Bên trái vẽ Biểu đồ tròn Donut (40%), Bên phải vẽ Chú giải chi tiết (60%)
            int splitX = (int)(Width * 0.40f);
            int padding = 10;
            int pieDiameter = Math.Min(splitX - padding * 2, Height - padding * 2);

            if (pieDiameter >= 36)
            {
                int pieX = padding + (splitX - padding * 2 - pieDiameter) / 2;
                int pieY = (Height - pieDiameter) / 2;
                var pieRect = new Rectangle(pieX, pieY, pieDiameter, pieDiameter);

                // Vẽ các lát cắt tròn
                float startAngle = -90f;
                using var borderPen = new Pen(Color.White, 2f);

                foreach (var item in displayList)
                {
                    float sweepAngle = (float)(item.Percent / 100.0 * 360.0);
                    if (sweepAngle <= 0.1f) continue;

                    using var sliceBrush = new SolidBrush(item.Color);
                    g.FillPie(sliceBrush, pieRect, startAngle, sweepAngle);
                    g.DrawPie(borderPen, pieRect, startAngle, sweepAngle);
                    startAngle += sweepAngle;
                }

                // Vẽ tâm tròn Donut màu trắng
                int innerDiameter = (int)(pieDiameter * 0.52f);
                int innerX = pieX + (pieDiameter - innerDiameter) / 2;
                int innerY = pieY + (pieDiameter - innerDiameter) / 2;
                g.FillEllipse(Brushes.White, innerX, innerY, innerDiameter, innerDiameter);

                // Chữ hiển thị ngắn gọn ở tâm Donut
                float midY = innerY + (innerDiameter / 2f);
                g.DrawString("CƠ CẤU", fontCenterLabel, mutedBrush,
                    new RectangleF(innerX, midY - 14f, innerDiameter, 13f), centerFormat);
                g.DrawString("100%", fontCenterVal, textBrush,
                    new RectangleF(innerX, midY, innerDiameter, 15f), centerFormat);
            }

            // 2. Vẽ Bảng Chú Giải (Legend) thông minh ở nửa bên phải - CHỐNG ĐÈ CHỮ HOÀN TOÀN
            int legendLeft = splitX + 8;
            int legendRight = Width - 8;
            int legendAvailableWidth = legendRight - legendLeft;

            if (legendAvailableWidth < 80) return;

            int rowCount = displayList.Count;
            int rowHeight = Math.Min(28, Math.Max(20, (Height - 16) / Math.Max(1, rowCount)));
            int startY = Math.Max(6, (Height - rowCount * rowHeight) / 2);

            for (int i = 0; i < rowCount; i++)
            {
                var item = displayList[i];
                int y = startY + i * rowHeight;

                // A. Ô vuông màu đại diện (kích thước 10x10)
                int boxSize = 10;
                int boxY = y + (rowHeight - boxSize) / 2;
                using var boxBrush = new SolidBrush(item.Color);
                g.FillRectangle(boxBrush, legendLeft, boxY, boxSize, boxSize);

                // B. Cột Tỷ lệ % & Doanh thu (Canh lề PHẢI sát mép phải, không bị che)
                string shareText = $"{item.Percent:0.0}% ({RevenueBarChart.Short(item.Revenue)})";
                var shareSize = g.MeasureString(shareText, fontBold);
                float shareWidth = (float)Math.Ceiling(shareSize.Width) + 2f;
                float shareX = legendRight - shareWidth;

                var shareRect = new RectangleF(shareX, y, shareWidth, rowHeight);
                g.DrawString(shareText, fontBold, mutedBrush, shareRect, rightFormat);

                // C. Cột Tên nhóm hàng (Nằm giữa ô màu và cột tỷ lệ, tự động gắn dấu ... nếu quá dài)
                float catX = legendLeft + boxSize + 6f;
                float catWidth = Math.Max(20f, shareX - catX - 6f); // Đảm bảo luôn cách số % ít nhất 6px

                var catRect = new RectangleF(catX, y, catWidth, rowHeight);
                g.DrawString(item.Name, fontNormal, textBrush, catRect, leftFormat);
            }
        }
    }

    // =========================================================================
    // DTO NHẬN DỮ LIỆU TỪ WEB API
    // =========================================================================
    public class ReportDto
    {
        public ReportSummaryDto Summary { get; set; } = new();
        public List<DailyRevenueDto> Daily { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
        public List<CategoryShareDto> CategoryShares { get; set; } = new(); // Dành cho Biểu đồ tròn
        public List<ReportOrderDto> Orders { get; set; } = new();
    }

    public class ReportSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalItems { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal CashRevenue { get; set; }
        public decimal TransferRevenue { get; set; }

        // TIÊU CHÍ 44: Thêm trường khách hàng và hàng sắp hết kho
        public int TotalCustomers { get; set; }
        public int LowStockCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TodayOrders { get; set; }
    }

    public class CategoryShareDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Quantity { get; set; }
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class TopProductDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ReportOrderDto
    {
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CashierUsername { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
