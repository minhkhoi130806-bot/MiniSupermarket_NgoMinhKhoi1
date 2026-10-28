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
    // ======================================================
    // FORM BAO CAO DOANH THU
    //   GET reports/revenue?from=yyyy-MM-dd&to=yyyy-MM-dd
    // ======================================================
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

        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;
        private Button btnView = null!;
        private Button btnExport = null!;
        private Button btnToday = null!;
        private Button btn7 = null!;
        private Button btn30 = null!;
        private Button btnMonth = null!;

        private Label lblRevenue = null!;
        private Label lblOrders = null!;
        private Label lblAvg = null!;
        private Label lblDiscount = null!;
        private Label lblPayment = null!;
        private Label lblStatus = null!;

        private RevenueBarChart chart = null!;
        private DataGridView dgvTop = null!;
        private DataGridView dgvOrders = null!;

        private ReportDto? _data;
        private bool _busy;
        private bool _refreshing;
        private bool _followToday = true;
        private readonly System.Windows.Forms.Timer _autoTimer = new() { Interval = 10000 };

        public FormQuickReport()
        {
            BuildLayout();
        }

        // ======================================================
        // GIAO DIEN
        // ======================================================
        private void BuildLayout()
        {
            Text = "Báo cáo doanh thu";
            Size = new Size(LogicalToDeviceUnits(1200), LogicalToDeviceUnits(760));
            MinimumSize = new Size(LogicalToDeviceUnits(800), LogicalToDeviceUnits(520));
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(58)));  // bo loc
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(96)));  // the so lieu
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));                         // bieu do + top SP
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));                         // danh sach don
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, LogicalToDeviceUnits(26)));  // trang thai
            Controls.Add(root);

            root.Controls.Add(BuildFilterBar(), 0, 0);
            root.Controls.Add(BuildKpiRow(), 0, 1);
            root.Controls.Add(BuildMiddleRow(), 0, 2);
            root.Controls.Add(BuildOrdersCard(), 0, 3);

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

                // Tự động cập nhật số liệu hôm nay từ database
                _autoTimer.Tick += async (s2, e2) => await RefreshSilentAsync();
                _autoTimer.Start();
            };

            FormClosed += (s, e) =>
            {
                _autoTimer.Stop();
                _autoTimer.Dispose();
            };
        }

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
                Width = LogicalToDeviceUnits(120),
                Margin = new Padding(0, 3, 14, 0)
            };
            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Width = LogicalToDeviceUnits(120),
                Margin = new Padding(0, 3, 14, 0)
            };

            btnToday = MakeButton("Hôm nay", mutedColor, 90);
            btn7 = MakeButton("7 ngày", mutedColor, 84);
            btn30 = MakeButton("30 ngày", mutedColor, 90);
            btnMonth = MakeButton("Tháng này", mutedColor, 100);
            btnView = MakeButton("XEM BÁO CÁO", primaryColor, 130);
            btnExport = MakeButton("XUẤT BÁO CÁO", successColor, 130);

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

            row.Controls.Add(MakeKpi("TỔNG DOANH THU", successColor, 18F, out lblRevenue), 0, 0);
            row.Controls.Add(MakeKpi("SỐ ĐƠN HÀNG", primaryColor, 18F, out lblOrders), 1, 0);
            row.Controls.Add(MakeKpi("GIÁ TRỊ TB / ĐƠN", warningColor, 18F, out lblAvg), 2, 0);
            row.Controls.Add(MakeKpi("TỔNG GIẢM GIÁ", dangerColor, 18F, out lblDiscount), 3, 0);
            row.Controls.Add(MakeKpi("TIỀN MẶT / CK-THẺ", mutedColor, 12F, out lblPayment), 4, 0);
            return row;
        }

        private Panel MakeKpi(string title, Color accent, float valueSize, out Label valueLabel)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(14, 8, 8, 8)
            };

            valueLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "0 đ",
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
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = mutedColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var strip = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accent };

            card.Controls.Add(valueLabel);   // Fill truoc
            card.Controls.Add(lblTitle);
            card.Controls.Add(strip);
            return card;
        }

        private Control BuildMiddleRow()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = backgroundColor
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            chart = new RevenueBarChart { Dock = DockStyle.Fill, BarColor = primaryColor };
            var chartCard = MakeCard("DOANH THU THEO NGÀY", chart);
            chartCard.Margin = new Padding(0, 0, 4, 0);

            dgvTop = CreateGrid();
            AddCol(dgvTop, "Rank", "#", 8, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvTop, "Name", "Sản phẩm", 52, DataGridViewContentAlignment.MiddleLeft);
            AddCol(dgvTop, "Qty", "SL bán", 15, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvTop, "Revenue", "Doanh thu", 25, DataGridViewContentAlignment.MiddleRight);
            var topCard = MakeCard("TOP SẢN PHẨM BÁN CHẠY", dgvTop);
            topCard.Margin = new Padding(4, 0, 0, 0);

            row.Controls.Add(chartCard, 0, 0);
            row.Controls.Add(topCard, 1, 0);
            return row;
        }

        private Control BuildOrdersCard()
        {
            dgvOrders = CreateGrid();
            AddCol(dgvOrders, "Code", "Mã đơn", 12, DataGridViewContentAlignment.MiddleLeft);
            AddCol(dgvOrders, "Time", "Thời gian", 16, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvOrders, "Cashier", "Thu ngân", 13, DataGridViewContentAlignment.MiddleLeft);
            AddCol(dgvOrders, "Customer", "Khách hàng", 17, DataGridViewContentAlignment.MiddleLeft);
            AddCol(dgvOrders, "Method", "Thanh toán", 14, DataGridViewContentAlignment.MiddleCenter);
            AddCol(dgvOrders, "Discount", "Giảm giá", 16, DataGridViewContentAlignment.MiddleRight);
            AddCol(dgvOrders, "Final", "Thành tiền", 14, DataGridViewContentAlignment.MiddleRight);

            var card = MakeCard("DANH SÁCH ĐƠN HÀNG", dgvOrders);
            card.Margin = new Padding(0, 0, 0, 4);
            return card;
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
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(content);   // Fill truoc
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
                ColumnHeadersHeight = LogicalToDeviceUnits(36),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI", 9.5F)
            };
            g.RowTemplate.Height = LogicalToDeviceUnits(34);
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

        private static void AddCol(DataGridView g, string name, string header, float weight,
            DataGridViewContentAlignment align)
        {
            g.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = weight,
                MinimumWidth = 40,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = align }
            });
        }

        // ======================================================
        // TAI DU LIEU
        // ======================================================
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

            // Khoảng ngày đang xem kết thúc ở hôm nay -> tự động theo dõi dữ liệu mới
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
            lblStatus.Text = "Đang tải báo cáo...";

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

        // ======================================================
        // HIEN THI
        // ======================================================
        // Tải lại âm thầm (không hộp thoại, không đổi con trỏ) để số liệu luôn mới
        private async Task RefreshSilentAsync()
        {
            if (_busy || _refreshing || IsDisposed || !IsHandleCreated) return;
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken)) return;

            // Sang ngày mới: kéo "Đến ngày" theo hôm nay
            if (_followToday && dtpTo.Value.Date < DateTime.Today)
                dtpTo.Value = DateTime.Today;

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;
            if (from > to || to < DateTime.Today) return;   // đang xem khoảng cũ, không cần làm mới

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
                lblStatus.Text += $"  |  Cập nhật lúc {DateTime.Now:HH:mm:ss}";
            }
            catch
            {
                // Lỗi mạng tạm thời: bỏ qua, lần sau thử lại
            }
            finally
            {
                _refreshing = false;
            }
        }

        private static string Money(decimal v) => v.ToString("N0", Vi) + " đ";

        private static string MethodText(string? m) =>
            string.Equals(m, "CASH", StringComparison.OrdinalIgnoreCase) ? "Tiền mặt" : "Chuyển khoản/Thẻ";

        private void Render(ReportDto d, DateTime from, DateTime to)
        {
            var s = d.Summary ?? new ReportSummaryDto();

            lblRevenue.Text = Money(s.TotalRevenue);
            lblOrders.Text = s.TotalOrders.ToString("N0", Vi) + $"  ({s.TotalItems:N0} sp)";
            lblAvg.Text = Money(s.AverageOrderValue);
            if (s.TotalDiscount > 0)
            {
                lblDiscount.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
                lblDiscount.Text = Money(s.TotalDiscount);
            }
            else
            {
                lblDiscount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                lblDiscount.Text = "Chưa có giảm giá";
            }
            lblPayment.Text = RevenueBarChart.Short(s.CashRevenue) + "  /  " + RevenueBarChart.Short(s.TransferRevenue);

            // Bieu do: dien du cac ngay khong co doanh thu
            var days = new List<DailyRevenueDto>();
            for (var day = from; day <= to; day = day.AddDays(1))
            {
                var f = d.Daily.FirstOrDefault(x => x.Date.Date == day);
                days.Add(new DailyRevenueDto { Date = day, Revenue = f?.Revenue ?? 0, Orders = f?.Orders ?? 0 });
            }
            chart.Data = days;
            chart.Invalidate();

            // Top san pham
            dgvTop.Rows.Clear();
            int rank = 1;
            foreach (var p in d.TopProducts)
                dgvTop.Rows.Add(rank++, p.ProductName, p.Quantity.ToString("N0", Vi), p.Revenue.ToString("N0", Vi));

            // Don hang
            dgvOrders.Rows.Clear();
            foreach (var o in d.Orders)
            {
                dgvOrders.Rows.Add(
                    o.OrderCode,
                    o.CreatedAt.ToString("dd/MM/yyyy HH:mm", Vi),
                    o.CashierUsername,
                    string.IsNullOrWhiteSpace(o.CustomerName) ? "Khách vãng lai" : o.CustomerName,
                    MethodText(o.PaymentMethod),
                    o.DiscountAmount > 0 ? o.DiscountAmount.ToString("N0", Vi) : "Chưa có giảm giá",
                    o.FinalAmount.ToString("N0", Vi));
            }

            lblStatus.Text =
                $"Từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}  |  {s.TotalOrders} đơn hàng  |  Doanh thu {Money(s.TotalRevenue)}";
        }

        // ======================================================
        // TIÊU CHÍ 48: XUẤT BÁO CÁO EXCEL (.XLSX) HOẶC CSV (.CSV)
        // ======================================================
        private void ExportReport()
        {
            if (_data == null || _data.Orders.Count == 0)
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

    // ======================================================
    // BIEU DO COT TU VE (khong can thu vien ngoai)
    // ======================================================
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

    // ======================================================
    // DTO NHAN TU API
    // ======================================================
    public class ReportDto
    {
        public ReportSummaryDto Summary { get; set; } = new();
        public List<DailyRevenueDto> Daily { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = new();
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