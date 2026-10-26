using MiniSupermarket.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormPOS : Form
    {
        // ===================== DỮ LIỆU =====================
        private readonly List<CartItemDto> _cart = new();
        private readonly BindingSource _bs = new();
        private readonly List<HeldBill> _heldBills = new();

        private static readonly CultureInfo Vi = new("vi-VN");

        private decimal _discountValue = 0;
        private bool _discountIsPercent = false;
        private string _customerName = "Khách vãng lai";
        private bool _formattingCash = false;
        private ReceiptData? _lastReceipt;
        private int? _customerId;
        private int _pointsToRedeem = 0; // Tiêu chí 47: Số điểm khách muốn đổi trừ tiền
        private int _customerPoints;
        private const decimal VndPerPoint = 10000m;   // 10.000đ = 1 điểm

        private int CalcEarnedPoints() =>
            _customerId == null ? 0 : (int)Math.Floor(GetTotal() / VndPerPoint);

        // ===================== MÀU SẮC =====================
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color primaryColor = Color.FromArgb(59, 130, 246);
        private readonly Color darkColor = Color.FromArgb(30, 41, 59);
        private readonly Color textColor = Color.FromArgb(31, 41, 55);
        private readonly Color mutedColor = Color.FromArgb(107, 114, 128);
        private readonly Color borderColor = Color.FromArgb(229, 231, 235);
        private readonly Color successColor = Color.FromArgb(22, 163, 74);
        private readonly Color dangerColor = Color.FromArgb(220, 38, 38);
        private readonly Color warningColor = Color.FromArgb(245, 158, 11);

        // ===================== CONTROL =====================
        private DataGridView dgvCart = null!;
        private Label lblEmptyCart = null!;
        private TextBox txtBarcode = null!;
        private TextBox txtCustomerPhone = null!;
        private Label lblCustomerName = null!;
        private Label lblItemCount = null!;
        private Label lblSubtotal = null!;
        private Label lblDiscount = null!;
        private Label lblTotalAmount = null!;
        private TextBox txtCashReceived = null!;
        private Label lblChangeTitle = null!;
        private Label lblChange = null!;
        private Button btnCheckout = null!;
        private Button btnRecall = null!;
        private RadioButton rdoCash = null!;
        private RadioButton rdoTransfer = null!;
        private Label lblClock = null!;
        private Label lblStatus = null!;
        private TableLayoutPanel quickCashPanel = null!;
        private Panel _header = null!;
        private float _lastScale = -1f;
        private readonly Dictionary<Control, float> _baseFonts = new();
        private readonly Dictionary<Control, Font> _ownFonts = new();
        private readonly Dictionary<DataGridViewColumn, float> _baseColFonts = new();
        private float _baseGridHeaderFont = 9.5f;
        private readonly System.Windows.Forms.Timer _clockTimer = new() { Interval = 1000 };

        public FormPOS()
        {
            InitializeComponent();

            // Dùng giao diện từ code, tránh trùng control Designer
            Controls.Clear();

            BuildPOSLayout();
            SetupCartGrid();
            UpdateCartDisplay();

            _clockTimer.Tick += (s, e) =>
                lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");
            _clockTimer.Start();
            lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss");

            RecordBaseFonts(this);
            Resize += (s, e) => ApplyScale();
            Shown += (s, e) => { ApplyScale(); txtBarcode.Focus(); };
            ApplyScale();
            FormClosed += (s, e) => _clockTimer.Dispose();
        }

        // Đổi pixel thiết kế (96 DPI) sang pixel thật theo DPI màn hình
        private int S(int px) => LogicalToDeviceUnits(px);

        // ===================== PHÍM TẮT =====================
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F1: txtBarcode.Focus(); return true;
                case Keys.F2: txtCustomerPhone.Focus(); txtCustomerPhone.SelectAll(); return true;
                case Keys.F3:
                    if (rdoCash.Checked) { txtCashReceived.Focus(); txtCashReceived.SelectAll(); }
                    return true;
                case Keys.F4: HoldCurrentBill(); return true;
                case Keys.F5: RecallBill(); return true;
                case Keys.F6: ChangeQuantityOfSelected(); return true;
                case Keys.F7: ApplyDiscount(); return true;
                case Keys.F8: ClearCart(true); return true;
                case Keys.F9: btnCheckout.PerformClick(); return true;
                case Keys.F10: RemoveSelectedItem(); return true;
                case Keys.F11: RedeemPoints(); return true;
                case Keys.Delete when dgvCart.Focused: RemoveSelectedItem(); return true;
                case Keys.Add when !(ActiveControl is TextBox): ChangeQuantity(+1); return true;
                case Keys.Subtract when !(ActiveControl is TextBox): ChangeQuantity(-1); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ===================== XÂY DỰNG GIAO DIỆN =====================
        private void BuildPOSLayout()
        {
            Text = "Bán hàng tại quầy - MiniSupermarket";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(S(1280), S(780));
            MinimumSize = new Size(S(820), S(520));
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;

            // Vùng nội dung chính (Fill phải add đầu tiên)
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(S(12)),
                BackColor = backgroundColor
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            // Thanh tiêu đề trên cùng
            _header = BuildHeader();
            Controls.Add(_header);

            // Thanh trạng thái / hướng dẫn phím tắt dưới cùng
            lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = S(28),
                BackColor = darkColor,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(S(12), 0, 0, 0),
                Text = "F1 Mã vạch  |  F2 Khách hàng  |  F3 Tiền khách đưa  |  F4 Giữ đơn  |  F5 Gọi đơn  |  " +
                       "F6 Sửa SL  |  F7 Giảm giá  |  F8 Xóa giỏ  |  F9 Thanh toán  |  F10/Del Xóa dòng  |  +/- Tăng/giảm SL"
            };
            Controls.Add(lblStatus);

            var cartCard = CreatePOSCard();
            cartCard.Margin = new Padding(0, 0, S(8), 0);
            var paymentCard = CreatePOSCard();
            paymentCard.Margin = new Padding(S(8), 0, 0, 0);

            root.Controls.Add(cartCard, 0, 0);
            root.Controls.Add(paymentCard, 1, 0);

            BuildCartSection(cartCard);
            BuildPaymentSection(paymentCard);
        }

        private Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = S(56),
                BackColor = darkColor
            };

            // Fill add trước, Right add sau
            var lblStore = new Label
            {
                Dock = DockStyle.Fill,
                Text = "MINI SUPERMARKET  -  QUẦY THU NGÂN",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(S(16), 0, 0, 0)
            };

            lblClock = new Label
            {
                Dock = DockStyle.Right,
                Width = S(230),
                Padding = new Padding(0, 0, S(12), 0),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblCashier = new Label
            {
                Dock = DockStyle.Right,
                Width = S(260),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(147, 197, 253),
                TextAlign = ContentAlignment.MiddleRight,
                Text = "Thu ngân: " + (SessionManager.CurrentUsername ?? "")
            };

            header.Controls.Add(lblStore);
            header.Controls.Add(lblClock);
            header.Controls.Add(lblCashier);
            return header;
        }

        private Panel CreatePOSCard()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(S(14)),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        // ---------- KHỐI GIỎ HÀNG (BÊN TRÁI) ----------
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
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 9));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 81));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));
            card.Controls.Add(layout);

            // Ô nhập mã vạch (hỗ trợ cú pháp "SL*mã", ví dụ 5*8934567890123)
            txtBarcode = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 15F),
                PlaceholderText = "Quét mã vạch (F1)  —  gõ 5*mã để thêm nhiều sản phẩm",
                Margin = new Padding(0, S(4), 0, S(10))
            };
            txtBarcode.KeyDown += txtBarcode_KeyDown;
            layout.Controls.Add(txtBarcode, 0, 0);

            // Bảng giỏ hàng
            dgvCart = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Both,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = S(42) },
                ColumnHeadersHeight = S(42),
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Font = new Font("Segoe UI", 10.5F),
                GridColor = borderColor,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(243, 246, 250);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
            dgvCart.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvCart.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(243, 246, 250);
            dgvCart.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dgvCart.DefaultCellStyle.SelectionForeColor = textColor;
            dgvCart.DefaultCellStyle.Padding = new Padding(6, 2, 6, 2);
            dgvCart.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            dgvCart.DoubleClick += (s, e) => ChangeQuantityOfSelected();

            // Thông báo giỏ trống nằm đè lên grid
            lblEmptyCart = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Giỏ hàng đang trống\r\nQuét mã vạch để bắt đầu",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Italic),
                ForeColor = mutedColor,
                BackColor = Color.White
            };
            dgvCart.Controls.Add(lblEmptyCart);

            layout.Controls.Add(dgvCart, 0, 1);

            // Hàng nút thao tác giỏ hàng
            var actions = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 1,
                Margin = new Padding(0, S(8), 0, 0)
            };
            for (int i = 0; i < 7; i++)
                actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));

            actions.Controls.Add(CreateActionButton("＋  Tăng", primaryColor, (s, e) => ChangeQuantity(+1)), 0, 0);
            actions.Controls.Add(CreateActionButton("－  Giảm", primaryColor, (s, e) => ChangeQuantity(-1)), 1, 0);
            actions.Controls.Add(CreateActionButton("Sửa SL (F6)", primaryColor, (s, e) => ChangeQuantityOfSelected()), 2, 0);
            actions.Controls.Add(CreateActionButton("Xóa dòng (F10)", dangerColor, (s, e) => RemoveSelectedItem()), 3, 0);
            actions.Controls.Add(CreateActionButton("Xóa giỏ (F8)", dangerColor, (s, e) => ClearCart(true)), 4, 0);
            actions.Controls.Add(CreateActionButton("Giữ đơn (F4)", warningColor, (s, e) => HoldCurrentBill()), 5, 0);
            btnRecall = CreateActionButton("Gọi đơn (F5)", warningColor, (s, e) => RecallBill());
            actions.Controls.Add(btnRecall, 6, 0);

            layout.Controls.Add(actions, 0, 2);
        }

        private Button CreateActionButton(string text, Color color, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(S(3), 0, S(3), 0),
                UseVisualStyleBackColor = false,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        // ---------- KHỐI THANH TOÁN (BÊN PHẢI) ----------
        private void BuildPaymentSection(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 10,
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            // Tất cả hàng dùng tỉ lệ % => co giãn theo kích thước cửa sổ
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 6));    // 0 tiêu đề
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // 1 SĐT khách
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 5));    // 2 tên khách
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24));   // 3 tổng kết
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));   // 4 phương thức
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 13));   // 5 tiền khách đưa
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 8));    // 6 nút tiền nhanh
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // 7 tiền thừa
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 2));    // 8 khoảng đệm
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // 9 nút thanh toán
            card.Controls.Add(layout);

            layout.Controls.Add(new Label
            {
                Text = "Thông tin thanh toán",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            // --- Khách hàng ---
            var customerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0)
            };
            customerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            customerPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 64));
            customerPanel.Controls.Add(MakeLabel("Số điện thoại khách hàng (F2)",
                new Font("Segoe UI", 9F, FontStyle.Bold), textColor, ContentAlignment.MiddleLeft), 0, 0);

            txtCustomerPhone = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11F),
                PlaceholderText = "Nhập SĐT rồi nhấn Enter...",
                MaxLength = 15
            };
            txtCustomerPhone.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };
            txtCustomerPhone.KeyDown += txtCustomerPhone_KeyDown;
            customerPanel.Controls.Add(txtCustomerPhone, 0, 1);
            layout.Controls.Add(customerPanel, 0, 1);
            txtCustomerPhone.TextChanged += (s, e) =>
            {
                // Sửa SĐT sau khi đã tra cứu thì phải tra cứu lại
                if (_customerId != null) SetCustomer(null);
            };

            lblCustomerName = MakeLabel(_customerName, new Font("Segoe UI", 9.5F, FontStyle.Italic),
    mutedColor, ContentAlignment.MiddleLeft);
            lblCustomerName.Cursor = Cursors.Hand;              // Hiện bàn tay khi rê chuột vào
            lblCustomerName.Click += (s, e) => RedeemPoints();   // Bấm vào tên để mở hộp thoại đổi điểm
            layout.Controls.Add(lblCustomerName, 0, 2);

            // --- Tổng kết đơn hàng ---
            var summary = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = Color.FromArgb(249, 250, 251),
                Padding = new Padding(S(12), S(6), S(12), S(6)),
                Margin = new Padding(0, S(4), 0, S(4))
            };
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            summary.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            summary.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            summary.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            summary.RowStyles.Add(new RowStyle(SizeType.Percent, 46));

            var small = new Font("Segoe UI", 10F);
            var smallBold = new Font("Segoe UI", 10F, FontStyle.Bold);

            summary.Controls.Add(MakeLabel("Số mặt hàng", small, mutedColor, ContentAlignment.MiddleLeft), 0, 0);
            lblItemCount = MakeLabel("0", smallBold, textColor, ContentAlignment.MiddleRight);
            summary.Controls.Add(lblItemCount, 1, 0);

            summary.Controls.Add(MakeLabel("Tạm tính", small, mutedColor, ContentAlignment.MiddleLeft), 0, 1);
            lblSubtotal = MakeLabel("0 đ", smallBold, textColor, ContentAlignment.MiddleRight);
            summary.Controls.Add(lblSubtotal, 1, 1);

            var lblDiscTitle = MakeLabel("Giảm giá (F7)", small, primaryColor, ContentAlignment.MiddleLeft);
            lblDiscTitle.Cursor = Cursors.Hand;
            lblDiscTitle.Click += (s, e) => ApplyDiscount();
            summary.Controls.Add(lblDiscTitle, 0, 2);
            lblDiscount = MakeLabel("0 đ", smallBold, successColor, ContentAlignment.MiddleRight);
            lblDiscount.Cursor = Cursors.Hand;
            lblDiscount.Click += (s, e) => ApplyDiscount();
            summary.Controls.Add(lblDiscount, 1, 2);

            summary.Controls.Add(MakeLabel("TỔNG CỘNG", new Font("Segoe UI", 10.5F, FontStyle.Bold),
                textColor, ContentAlignment.MiddleLeft), 0, 3);
            lblTotalAmount = MakeLabel("0 đ", new Font("Segoe UI", 20F, FontStyle.Bold),
                dangerColor, ContentAlignment.MiddleRight);
            lblTotalAmount.AutoEllipsis = true;
            summary.Controls.Add(lblTotalAmount, 1, 3);
            layout.Controls.Add(summary, 0, 3);

            // --- Phương thức thanh toán (2 cột đều nhau) ---
            var methodPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0)
            };
            methodPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            methodPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            methodPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            methodPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));

            var lblMethod = MakeLabel("Phương thức thanh toán",
                new Font("Segoe UI", 9F, FontStyle.Bold), textColor, ContentAlignment.MiddleLeft);
            methodPanel.Controls.Add(lblMethod, 0, 0);
            methodPanel.SetColumnSpan(lblMethod, 2);

            rdoCash = new RadioButton
            {
                Text = "Tiền mặt",
                Checked = true,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            rdoTransfer = new RadioButton
            {
                Text = "Chuyển khoản/Thẻ",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            rdoCash.CheckedChanged += (s, e) => OnPaymentMethodChanged();
            methodPanel.Controls.Add(rdoCash, 0, 1);
            methodPanel.Controls.Add(rdoTransfer, 1, 1);
            layout.Controls.Add(methodPanel, 0, 4);

            // --- Tiền khách đưa ---
            var cashPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0)
            };
            cashPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 36));
            cashPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 64));
            cashPanel.Controls.Add(MakeLabel("Tiền khách đưa (F3)",
                new Font("Segoe UI", 9F, FontStyle.Bold), textColor, ContentAlignment.MiddleLeft), 0, 0);

            txtCashReceived = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                PlaceholderText = "0",
                TextAlign = HorizontalAlignment.Right
            };
            txtCashReceived.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            };
            txtCashReceived.TextChanged += txtCashReceived_TextChanged;
            txtCashReceived.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnCheckout.PerformClick();
                }
            };
            cashPanel.Controls.Add(txtCashReceived, 0, 1);
            layout.Controls.Add(cashPanel, 0, 5);

            // --- Nút tiền nhanh ---
            quickCashPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Margin = new Padding(0, S(4), 0, S(4))
            };
            for (int i = 0; i < 5; i++)
                quickCashPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            AddQuickCashButton("Đủ tiền", null, 0);
            AddQuickCashButton("50K", 50000, 1);
            AddQuickCashButton("100K", 100000, 2);
            AddQuickCashButton("200K", 200000, 3);
            AddQuickCashButton("500K", 500000, 4);
            layout.Controls.Add(quickCashPanel, 0, 6);

            // --- Tiền thừa ---
            var changePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.FromArgb(240, 253, 244),
                Padding = new Padding(S(12), S(4), S(12), S(4)),
                Margin = new Padding(0, S(4), 0, S(4))
            };
            changePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
            changePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 65));

            lblChangeTitle = MakeLabel("TIỀN THỪA TRẢ KHÁCH",
                new Font("Segoe UI", 8.5F, FontStyle.Bold), mutedColor, ContentAlignment.MiddleLeft);
            lblChange = MakeLabel("0 đ", new Font("Segoe UI", 16F, FontStyle.Bold),
                successColor, ContentAlignment.MiddleRight);
            lblChange.AutoEllipsis = true;
            changePanel.Controls.Add(lblChangeTitle, 0, 0);
            changePanel.Controls.Add(lblChange, 0, 1);
            layout.Controls.Add(changePanel, 0, 7);

            // --- Nút thanh toán (cao cố định, neo ở đáy) ---
            btnCheckout = new Button
            {
                Text = "THANH TOÁN (F9)",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                BackColor = successColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, S(4), 0, 0),
                UseVisualStyleBackColor = false,
                TabStop = false
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += btnCheckout_Click;
            layout.Controls.Add(btnCheckout, 0, 9);
        }

        private void AddQuickCashButton(string text, decimal? amount, int col)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = primaryColor,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(S(2), 0, S(2), 0),
                UseVisualStyleBackColor = false,
                TabStop = false,
                Tag = amount
            };
            btn.FlatAppearance.BorderColor = primaryColor;
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += (s, e) =>
            {
                if (!rdoCash.Checked) return;
                decimal value = amount ?? GetTotal();
                txtCashReceived.Text = value.ToString("N0", Vi);
                txtCashReceived.Focus();
            };
            quickCashPanel.Controls.Add(btn, col, 0);
        }

        private Label MakeLabel(string text, Font font, Color color, ContentAlignment align)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = font,
                ForeColor = color,
                TextAlign = align
            };
        }

        private void SetupCartGrid()
        {
            dgvCart.Columns.Clear();

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Stt",
                DataPropertyName = "Stt",
                HeaderText = "STT",
                FillWeight = 22,
                MinimumWidth = 40,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Barcode",
                DataPropertyName = "Barcode",
                HeaderText = "Mã vạch",
                FillWeight = 55,
                MinimumWidth = 90
            });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductName",
                DataPropertyName = "ProductName",
                HeaderText = "Tên sản phẩm",
                FillWeight = 140,
                MinimumWidth = 120
            });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UnitPrice",
                DataPropertyName = "UnitPrice",
                HeaderText = "Đơn giá",
                FillWeight = 65,
                MinimumWidth = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0",
                    FormatProvider = Vi,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                DataPropertyName = "Quantity",
                HeaderText = "SL",
                FillWeight = 30,
                MinimumWidth = 45,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
                }
            });
            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalPrice",
                DataPropertyName = "TotalPrice",
                HeaderText = "Thành tiền",
                FillWeight = 75,
                MinimumWidth = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0",
                    FormatProvider = Vi,
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
                }
            });

            _bs.DataSource = _cart;
            dgvCart.DataSource = _bs;
        }

        // ===================== HIỂN THỊ GIỎ HÀNG =====================
        private decimal GetSubtotal() => _cart.Sum(x => x.TotalPrice);

        private decimal GetDiscountAmount()
        {
            decimal sub = GetSubtotal();
            decimal d = _discountIsPercent
                ? Math.Round(sub * _discountValue / 100m)
                : _discountValue;
            // Tiêu chí 47: Cộng thêm tiền quy đổi từ điểm thưởng (1 điểm = 1.000 đ)
            d += (_pointsToRedeem * 1000m);
            return Math.Min(sub, d);
        }

        private decimal GetTotal() => GetSubtotal() - GetDiscountAmount();

        private string Money(decimal v) => v.ToString("N0", Vi) + " đ";

        private void UpdateCartDisplay(int? selectIndex = null)
        {
            for (int i = 0; i < _cart.Count; i++) _cart[i].Stt = i + 1;

            _bs.ResetBindings(false);
            lblEmptyCart.Visible = _cart.Count == 0;

            if (_cart.Count > 0)
            {
                int idx = selectIndex ?? _cart.Count - 1;
                idx = Math.Max(0, Math.Min(idx, _cart.Count - 1));
                dgvCart.ClearSelection();
                dgvCart.Rows[idx].Selected = true;
                dgvCart.CurrentCell = dgvCart.Rows[idx].Cells["ProductName"];
            }

            decimal sub = GetSubtotal();
            decimal disc = GetDiscountAmount();

            lblItemCount.Text = $"{_cart.Count} loại / {_cart.Sum(x => x.Quantity)} sp";
            lblSubtotal.Text = Money(sub);
            lblDiscount.Text = disc > 0
                ? "- " + Money(disc) + (_discountIsPercent ? $" ({_discountValue:0.##}%)" : "")
                : "0 đ";
            lblTotalAmount.Text = Money(sub - disc);

            if (btnRecall != null)
                btnRecall.Text = _heldBills.Count > 0 ? $"Gọi đơn (F5) [{_heldBills.Count}]" : "Gọi đơn (F5)";

            RefreshCustomerLabel();
            CalculateChange();
        }

        // ===================== THÊM SẢN PHẨM =====================
        private async void txtBarcode_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                e.SuppressKeyPress = true;

                string input = txtBarcode.Text.Trim();
                txtBarcode.Clear();

                // Cú pháp SL*mã
                int qty = 1;
                string barcode = input;
                int star = input.IndexOf('*');
                if (star > 0 && int.TryParse(input[..star], out int q) && q > 0)
                {
                    qty = q;
                    barcode = input[(star + 1)..].Trim();
                }

                if (barcode.Length == 0) return;
                await AddProductToCartByBarcodeAsync(barcode, qty);
            }
        }

        private async Task AddProductToCartByBarcodeAsync(string barcode, int quantity = 1)
        {
            try
            {
                var response = await ApiClientService.Client.GetAsync($"products/barcode/{barcode}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Warn($"Không tìm thấy sản phẩm có mã vạch: {barcode}");
                    return;
                }

                response.EnsureSuccessStatusCode();
                var product = await response.Content.ReadFromJsonAsync<ProductDto>();

                if (product == null)
                {
                    Warn($"Không tìm thấy sản phẩm có mã vạch: {barcode}");
                    return;
                }

                var existing = _cart.FirstOrDefault(c => c.ProductId == product.ProductId);
                int newQty = (existing?.Quantity ?? 0) + quantity;

                if (product.StockQuantity > 0 && newQty > product.StockQuantity)
                {
                    Warn($"Sản phẩm \"{product.ProductName}\" chỉ còn {product.StockQuantity} trong kho!");
                    return;
                }
                if (product.StockQuantity <= 0)
                {
                    Warn($"Sản phẩm \"{product.ProductName}\" đã hết hàng!");
                    return;
                }

                int index;
                if (existing != null)
                {
                    existing.Quantity = newQty;
                    index = _cart.IndexOf(existing);
                }
                else
                {
                    _cart.Add(new CartItemDto
                    {
                        ProductId = product.ProductId,
                        Barcode = product.Barcode,
                        ProductName = product.ProductName,
                        UnitPrice = product.Price,
                        StockQuantity = product.StockQuantity,
                        Quantity = quantity
                    });
                    index = _cart.Count - 1;
                }

                UpdateCartDisplay(index);
                SetStatus($"Đã thêm: {product.ProductName} x{quantity}");
            }
            catch (Exception ex)
            {
                System.Media.SystemSounds.Hand.Play();
                MessageBox.Show("Lỗi kết nối máy chủ: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                txtBarcode.Focus();
            }
        }

        // ===================== THAO TÁC GIỎ HÀNG =====================
        private CartItemDto? GetSelectedItem() => dgvCart.CurrentRow?.DataBoundItem as CartItemDto;

        private void ChangeQuantity(int delta)
        {
            var item = GetSelectedItem();
            if (item == null) return;

            int newQty = item.Quantity + delta;
            if (newQty < 1) return;
            if (item.StockQuantity > 0 && newQty > item.StockQuantity)
            {
                Warn($"Chỉ còn {item.StockQuantity} sản phẩm trong kho!");
                return;
            }

            item.Quantity = newQty;
            UpdateCartDisplay(_cart.IndexOf(item));
            txtBarcode.Focus();
        }

        private void ChangeQuantityOfSelected()
        {
            var item = GetSelectedItem();
            if (item == null) return;

            string? input = ShowInputDialog("Sửa số lượng", $"Số lượng cho \"{item.ProductName}\":",
                item.Quantity.ToString());
            if (input == null) return;

            if (!int.TryParse(input.Trim(), out int qty) || qty < 1)
            {
                Warn("Số lượng không hợp lệ!");
                return;
            }
            if (item.StockQuantity > 0 && qty > item.StockQuantity)
            {
                Warn($"Chỉ còn {item.StockQuantity} sản phẩm trong kho!");
                return;
            }

            item.Quantity = qty;
            UpdateCartDisplay(_cart.IndexOf(item));
            txtBarcode.Focus();
        }

        private void RemoveSelectedItem()
        {
            var item = GetSelectedItem();
            if (item == null) return;

            int idx = _cart.IndexOf(item);
            _cart.Remove(item);
            UpdateCartDisplay(idx);
            SetStatus($"Đã xóa: {item.ProductName}");
            txtBarcode.Focus();
        }

        private void ClearCart(bool confirm)
        {
            if (_cart.Count == 0) return;

            if (confirm && MessageBox.Show("Xóa toàn bộ giỏ hàng?", "Xác nhận",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _cart.Clear();
            ResetOrderState();
            UpdateCartDisplay();
            txtBarcode.Focus();
        }

        private void ResetOrderState()
        {
            _discountValue = 0;
            _pointsToRedeem = 0;
            _discountIsPercent = false;
            txtCashReceived.Clear();
            txtCustomerPhone.Clear();
            _customerId = null;
            _customerPoints = 0;
            _customerName = "Khách vãng lai";
            RefreshCustomerLabel();
            rdoCash.Checked = true;
        }

        // ===================== GIẢM GIÁ =====================
        private void ApplyDiscount()
        {
            if (_cart.Count == 0)
            {
                Warn("Giỏ hàng đang trống!");
                return;
            }

            string current = _discountValue == 0 ? "" :
                (_discountIsPercent ? _discountValue.ToString("0.##") + "%" : _discountValue.ToString("0"));

            string? input = ShowInputDialog("Giảm giá đơn hàng",
                "Nhập số tiền (vd: 20000) hoặc phần trăm (vd: 10%).\nĐể trống để bỏ giảm giá:", current);
            if (input == null) return;

            input = input.Trim().Replace(".", "").Replace(",", "");
            if (input.Length == 0)
            {
                _discountValue = 0;
                _discountIsPercent = false;
            }
            else if (input.EndsWith("%"))
            {
                if (!decimal.TryParse(input.TrimEnd('%'), out decimal pct) || pct < 0 || pct > 100)
                {
                    Warn("Phần trăm giảm giá phải từ 0 đến 100!");
                    return;
                }
                _discountValue = pct;
                _discountIsPercent = true;
            }
            else
            {
                if (!decimal.TryParse(input, out decimal amount) || amount < 0)
                {
                    Warn("Số tiền giảm giá không hợp lệ!");
                    return;
                }
                if (amount > GetSubtotal())
                {
                    Warn("Giảm giá không được lớn hơn tạm tính!");
                    return;
                }
                _discountValue = amount;
                _discountIsPercent = false;
            }

            UpdateCartDisplay(dgvCart.CurrentRow?.Index);
            txtBarcode.Focus();
        }

        // Tiêu chí 47: Thu ngân đổi điểm thưởng trừ tiền trực tiếp vào hóa đơn (Phím tắt F8)
        private void RedeemPoints()
        {
            if (_customerId == null)
            {
                Warn("Vui lòng tra cứu số điện thoại khách hàng trước khi đổi điểm!");
                return;
            }

            if (_customerPoints <= 0)
            {
                Warn($"Khách hàng {_customerName} hiện có 0 điểm, không thể đổi điểm!");
                return;
            }

            string prompt = $"Khách hàng [{_customerName}] hiện có {_customerPoints} điểm.\n(Quy đổi: 1 điểm = 1.000 đ trừ tiền hóa đơn)\nNhập số điểm muốn đổi (để trống hoặc 0 để hủy):";
            string current = _pointsToRedeem > 0 ? _pointsToRedeem.ToString() : "";

            string? input = ShowInputDialog("Đổi điểm thưởng trừ tiền (F8)", prompt, current);
            if (input == null) return;

            input = input.Trim();
            if (string.IsNullOrEmpty(input) || input == "0")
            {
                _pointsToRedeem = 0;
            }
            else if (int.TryParse(input, out int pts) && pts > 0)
            {
                if (pts > _customerPoints)
                {
                    Warn($"Khách hàng chỉ có {_customerPoints} điểm, không đủ để đổi {pts} điểm!");
                    return;
                }

                decimal money = pts * 1000m;
                if (money > GetSubtotal())
                {
                    Warn($"Số tiền quy đổi ({money:N0} đ) vượt quá tổng tiền hóa đơn!");
                    return;
                }

                _pointsToRedeem = pts;
            }
            else
            {
                Warn("Số điểm không hợp lệ!");
                return;
            }

            RefreshCustomerLabel();
            UpdateCartDisplay(dgvCart.CurrentRow?.Index);
            txtBarcode.Focus();
        }

        // ===================== GIỮ / GỌI ĐƠN =====================
        private void HoldCurrentBill()
        {
            if (_cart.Count == 0)
            {
                Warn("Giỏ hàng đang trống, không có gì để giữ!");
                return;
            }

            _heldBills.Add(new HeldBill
            {
                Items = _cart.Select(c => c.Clone()).ToList(),
                Phone = txtCustomerPhone.Text.Trim(),
                CustomerName = _customerName,
                DiscountValue = _discountValue,
                DiscountIsPercent = _discountIsPercent,
                CustomerId = _customerId,
                Points = _customerPoints,
                Time = DateTime.Now
                
            });

            _cart.Clear();
            ResetOrderState();
            UpdateCartDisplay();
            SetStatus($"Đã giữ đơn. Đang có {_heldBills.Count} đơn chờ.");
            txtBarcode.Focus();
        }

        private void RecallBill()
        {
            if (_heldBills.Count == 0)
            {
                Warn("Không có đơn nào đang giữ!");
                return;
            }

            if (_cart.Count > 0)
            {
                Warn("Hãy thanh toán hoặc giữ đơn hiện tại trước khi gọi đơn khác!");
                return;
            }

            int idx = 0;
            if (_heldBills.Count > 1)
            {
                int? chosen = ShowHeldBillsDialog();
                if (chosen == null) return;
                idx = chosen.Value;
            }

            var bill = _heldBills[idx];
            _heldBills.RemoveAt(idx);

            _cart.AddRange(bill.Items);
            _discountValue = bill.DiscountValue;
            _discountIsPercent = bill.DiscountIsPercent;
            txtCustomerPhone.Text = bill.Phone;      // đặt Text trước
            _customerId = bill.CustomerId;           // rồi mới gán khách
            _customerPoints = bill.Points;
            _customerName = bill.CustomerName;
            lblCustomerName.Text = _customerName;

            UpdateCartDisplay();
            SetStatus("Đã gọi lại đơn đang giữ.");
            txtBarcode.Focus();
        }

        private int? ShowHeldBillsDialog()
        {
            using var dlg = new Form
            {
                Text = "Chọn đơn đang giữ",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(480, 300),
                Font = Font
            };

            var list = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10.5F), ItemHeight = 24 };
            for (int i = 0; i < _heldBills.Count; i++)
            {
                var b = _heldBills[i];
                decimal sum = b.Items.Sum(x => x.TotalPrice);
                list.Items.Add($"#{i + 1}  {b.Time:HH:mm}  |  {b.Items.Count} mặt hàng  |  {sum.ToString("N0", Vi)} đ  |  {b.CustomerName}");
            }
            list.SelectedIndex = 0;

            var ok = new Button { Text = "Chọn", Dock = DockStyle.Bottom, Height = 40, DialogResult = DialogResult.OK };
            list.DoubleClick += (s, e) => { if (list.SelectedIndex >= 0) dlg.DialogResult = DialogResult.OK; };

            dlg.Controls.Add(list);
            dlg.Controls.Add(ok);
            dlg.AcceptButton = ok;

            return dlg.ShowDialog(this) == DialogResult.OK && list.SelectedIndex >= 0
                ? list.SelectedIndex
                : null;
        }

        // ===================== KHÁCH HÀNG =====================
        private async void txtCustomerPhone_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string phone = txtCustomerPhone.Text.Trim();
            if (phone.Length == 0)
            {
                SetCustomer(null);
                txtBarcode.Focus();
                return;
            }

            await LookupCustomerAsync(phone);
            txtBarcode.Focus();
        }

        // Giả định API: GET customers/phone/{phone} trả JSON có trường tên khách.
        // Nếu API của bạn khác, chỉ cần sửa URL bên dưới.
        private async Task LookupCustomerAsync(string phone)
        {
            try
            {
                var resp = await ApiClientService.Client.GetAsync($"customers/phone/{phone}");

                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    string? name = ShowInputDialog("Khách hàng mới",
                        $"SĐT {phone} chưa có trong hệ thống.\nNhập tên để đăng ký thành viên (để trống = bỏ qua):", "");
                    if (string.IsNullOrWhiteSpace(name)) { SetCustomer(null); return; }

                    var create = await ApiClientService.Client.PostAsJsonAsync("customers/quick-register",
    new { CustomerName = name.Trim(), PhoneNumber = phone });
                    if (!create.IsSuccessStatusCode)
                    {
                        string err = await create.Content.ReadAsStringAsync();
                        Warn($"Không thể đăng ký khách hàng mới!\nMã lỗi: {(int)create.StatusCode}\n{err}");
                        return;
                    }
                } 

                resp.EnsureSuccessStatusCode();
                SetCustomer(await resp.Content.ReadFromJsonAsync<CustomerDto>());
            }
            catch
            {
                SetCustomer(null);
                lblCustomerName.Text = "Không tra cứu được khách hàng";
                lblCustomerName.ForeColor = warningColor;
            }
        }

        private void SetCustomer(CustomerDto? c)
        {
            _customerId = c?.CustomerId;
            _customerPoints = c?.RewardPoints ?? 0;
            _customerName = c?.CustomerName ?? "Khách vãng lai";
            RefreshCustomerLabel();
        }

        private void RefreshCustomerLabel()
        {
            if (_customerId == null)
            {
                lblCustomerName.Text = _customerName;
                lblCustomerName.ForeColor = mutedColor;
                return;
            }
            int earn = CalcEarnedPoints();
            string redeemText = _pointsToRedeem > 0
                ? $"  [Đã đổi {_pointsToRedeem} điểm = -{_pointsToRedeem * 1000:N0} đ]"
                : "  👉 (Bấm vào đây hoặc ấn F11 để đổi điểm)";
            lblCustomerName.Text = $"{_customerName}  |  {_customerPoints} điểm" +
                                   redeemText +
                                   (earn > 0 ? $"  (+{earn} đơn này)" : "");
            lblCustomerName.ForeColor = successColor;
        }

        // ===================== TIỀN KHÁCH ĐƯA / TIỀN THỪA =====================
        private static decimal ParseMoney(string text)
        {
            string digits = new string((text ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length == 0 || digits.Length > 13) return 0;
            return decimal.Parse(digits, CultureInfo.InvariantCulture);
        }

        private void OnPaymentMethodChanged()
        {
            bool cash = rdoCash.Checked;
            txtCashReceived.Enabled = cash;
            quickCashPanel.Enabled = cash;
            if (!cash) txtCashReceived.Clear();
            CalculateChange();
        }

        private void txtCashReceived_TextChanged(object? sender, EventArgs e)
        {
            if (_formattingCash) return;
            _formattingCash = true;

            decimal v = ParseMoney(txtCashReceived.Text);
            txtCashReceived.Text = v == 0 ? "" : v.ToString("N0", Vi);
            txtCashReceived.SelectionStart = txtCashReceived.Text.Length;

            _formattingCash = false;
            CalculateChange();
        }

        private void CalculateChange()
        {
            if (lblChange == null) return;

            decimal total = GetTotal();

            if (!rdoCash.Checked)
            {
                lblChange.Text = "0 đ";
                lblChange.ForeColor = successColor;
                return;
            }

            decimal cash = ParseMoney(txtCashReceived.Text);
            if (cash == 0)
            {
                lblChange.Text = "0 đ";
                lblChange.ForeColor = successColor;
                return;
            }

            decimal change = cash - total;
            if (change >= 0)
            {
                lblChange.Text = Money(change);
                lblChange.ForeColor = successColor;
            }
            else
            {
                lblChange.Text = "Còn thiếu " + Money(-change);
                lblChange.ForeColor = dangerColor;
            }
        }

        // ===================== THANH TOÁN =====================
        private async void btnCheckout_Click(object? sender, EventArgs e)
        {

            if (_cart.Count == 0)
            {
                Warn("Giỏ hàng đang trống!");
                return;
            }
            string typedPhone = txtCustomerPhone.Text.Trim();
            if (_customerId == null && typedPhone.Length > 0)
                await LookupCustomerAsync(typedPhone);

            decimal subtotal = GetSubtotal();
            decimal discount = GetDiscountAmount();
            decimal total = subtotal - discount;
            bool isCash = rdoCash.Checked;
            decimal cashReceived = isCash ? ParseMoney(txtCashReceived.Text) : total;

            if (isCash && cashReceived < total)
            {
                Warn("Số tiền khách đưa chưa đủ hoặc không hợp lệ!");
                txtCashReceived.Focus();
                return;
            }

            decimal pureDiscount = _discountIsPercent
    ? Math.Round(subtotal * _discountValue / 100m)
    : _discountValue;

            var orderRequest = new
            {
                CashierUsername = SessionManager.CurrentUsername,
                CustomerId = _customerId,
                CustomerPhone = txtCustomerPhone.Text.Trim(),
                PaymentMethod = isCash ? "CASH" : "TRANSFER",
                DiscountAmount = pureDiscount,
                PointsToRedeem = _pointsToRedeem, // Gửi số điểm đổi lên API
                AmountReceived = isCash ? cashReceived : total,
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

                var response = await ApiClientService.Client.PostAsJsonAsync("orders/checkout", orderRequest);

                if (response.IsSuccessStatusCode)
                {
                    int redeemed = _pointsToRedeem;
                    int earned = 0;
                    int newTotal = _customerPoints;

                    // Đọc chính xác số điểm đã trừ và điểm mới từ API trả về
                    try
                    {
                        var resJson = await response.Content.ReadAsStringAsync();
                        using var doc = System.Text.Json.JsonDocument.Parse(resJson);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("redeemedPoints", out var rp)) redeemed = rp.GetInt32();
                        if (root.TryGetProperty("earnedPoints", out var ep)) earned = ep.GetInt32();
                        if (root.TryGetProperty("totalPoints", out var tp)) newTotal = tp.GetInt32();
                    }
                    catch
                    {
                        earned = CalcEarnedPoints();
                        newTotal = Math.Max(0, _customerPoints - redeemed + earned);
                    }

                    // Lưu lại dữ liệu hóa đơn trước khi xóa giỏ
                    _lastReceipt = new ReceiptData
                    {
                        Items = _cart.Select(c => c.Clone()).ToList(),
                        Subtotal = subtotal,
                        Discount = discount,
                        Total = total,
                        Cash = cashReceived,
                        Change = Math.Max(0, cashReceived - total),
                        Method = isCash ? "Tiền mặt" : "Chuyển khoản / Thẻ",
                        Phone = txtCustomerPhone.Text.Trim(),
                        CustomerName = _customerName,
                        Cashier = SessionManager.CurrentUsername ?? "",
                        OldPoints = _customerPoints,
                        RedeemedPoints = redeemed,
                        EarnedPoints = earned,
                        TotalPoints = _customerId != null ? newTotal : 0,
                        Time = DateTime.Now
                    };

                    // Thông báo chi tiết điểm trên màn hình
                    string pointInfo = "";
                    if (_customerId != null)
                    {
                        pointInfo = $"\n\n--- THÔNG TIN ĐIỂM THƯỞNG ---" +
                                    $"\n• Điểm trước thanh toán: {_customerPoints} điểm";
                        if (redeemed > 0)
                            pointInfo += $"\n• Điểm đổi trừ tiền: -{redeemed} điểm (-{redeemed * 1000:N0} đ)";
                        if (earned > 0)
                            pointInfo += $"\n• Điểm tích lũy đơn này: +{earned} điểm";
                        pointInfo += $"\n• Tổng điểm còn lại: {newTotal} điểm";
                    }

                    var answer = MessageBox.Show(
                        $"Thanh toán thành công!\nTổng tiền: {Money(total)}\nTiền thừa: {Money(_lastReceipt.Change)}" +
                        pointInfo +
                        "\n\nBạn có muốn xem và in hóa đơn không?",
                        "Thành công",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (answer == DialogResult.Yes)
                        PrintReceipt(_lastReceipt);

                    _cart.Clear();
                    _pointsToRedeem = 0; // Đặt lại điểm đổi về 0
                    ResetOrderState();
                    UpdateCartDisplay();
                    SetStatus("Thanh toán thành công. Sẵn sàng đơn mới.");
                    txtBarcode.Focus();
                }
                else
                {
                    string detail = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(
                        $"Thanh toán thất bại!\nMã phản hồi: {(int)response.StatusCode}\n{detail}",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối máy chủ để thanh toán.\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckout.Enabled = true;
            }
        }

        // ===================== IN HÓA ĐƠN =====================
        private void PrintReceipt(ReceiptData data)
        {
            try
            {
                var doc = new PrintDocument();
                doc.DefaultPageSettings.PaperSize = new PaperSize("Receipt80", 315, 900);
                doc.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
                doc.PrintPage += (s, e) => DrawReceipt(e.Graphics!, data);

                using var preview = new PrintPreviewDialog
                {
                    Document = doc,
                    Width = 500,
                    Height = 800,
                    StartPosition = FormStartPosition.CenterParent
                };
                preview.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể in hóa đơn: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawReceipt(Graphics g, ReceiptData d)
        {
            using var fTitle = new Font("Consolas", 12F, FontStyle.Bold);
            using var fNormal = new Font("Consolas", 8.5F);
            using var fBold = new Font("Consolas", 9F, FontStyle.Bold);
            using var brush = new SolidBrush(Color.Black);

            float width = 295;
            float x = 10, y = 10;
            var center = new StringFormat { Alignment = StringAlignment.Center };
            var right = new StringFormat { Alignment = StringAlignment.Far };

            void Center(string t, Font f)
            {
                g.DrawString(t, f, brush, new RectangleF(x, y, width, 24), center);
                y += f.GetHeight(g) + 2;
            }
            void Line(string l, string r, Font f)
            {
                g.DrawString(l, f, brush, new RectangleF(x, y, width, 20));
                g.DrawString(r, f, brush, new RectangleF(x, y, width, 20), right);
                y += f.GetHeight(g) + 2;
            }
            void Dash()
            {
                g.DrawString(new string('-', 42), fNormal, brush, x, y);
                y += fNormal.GetHeight(g);
            }

            Center("MINI SUPERMARKET", fTitle);
            Center("HÓA ĐƠN BÁN HÀNG", fBold);
            Center(d.Time.ToString("dd/MM/yyyy HH:mm:ss"), fNormal);
            Dash();
            Line("Thu ngân:", d.Cashier, fNormal);
            Line("Khách hàng:", string.IsNullOrEmpty(d.Phone) ? "Khách vãng lai" : d.CustomerName, fNormal); ;
            Dash();

            foreach (var it in d.Items)
            {
                string name = it.ProductName.Length > 38 ? it.ProductName[..38] : it.ProductName;
                g.DrawString(name, fNormal, brush, x, y);
                y += fNormal.GetHeight(g);
                Line($"  {it.Quantity} x {it.UnitPrice.ToString("N0", Vi)}", it.TotalPrice.ToString("N0", Vi), fNormal);
            }

            Dash();
            Line("Tạm tính:", d.Subtotal.ToString("N0", Vi), fNormal);
            if (d.Discount > 0) Line("Giảm giá:", "-" + d.Discount.ToString("N0", Vi), fNormal);
            Line("TỔNG CỘNG:", d.Total.ToString("N0", Vi) + " đ", fBold);
            Line("Thanh toán:", d.Method, fNormal);
            if (d.Method == "Tiền mặt")
            {
                Line("Tiền khách đưa:", d.Cash.ToString("N0", Vi), fNormal);
                Line("Tiền thừa:", d.Change.ToString("N0", Vi), fNormal);
            }
            if (d.TotalPoints > 0 || d.EarnedPoints > 0 || d.RedeemedPoints > 0)
            {
                Dash();
                if (d.OldPoints > 0)
                    Line("Điểm cũ:", d.OldPoints.ToString("N0", Vi), fNormal);
                if (d.RedeemedPoints > 0)
                    Line("Điểm đã đổi:", "-" + d.RedeemedPoints.ToString("N0", Vi) + $" (-{d.RedeemedPoints * 1000:N0} đ)", fNormal);
                if (d.EarnedPoints > 0)
                    Line("Điểm cộng đơn này:", "+" + d.EarnedPoints.ToString("N0", Vi), fNormal);
                Line("Tổng điểm tích lũy:", d.TotalPoints.ToString("N0", Vi), fBold);
            }
            Dash();
            y += 6;
            Center("Cảm ơn quý khách - Hẹn gặp lại!", fNormal);
        }

        // ===================== CO GIÃN THEO KÍCH THƯỚC CỬA SỔ =====================
        private void RecordBaseFonts(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                _baseFonts[c] = c.Font.Size;
                RecordBaseFonts(c);
            }

            if (parent == this)
            {
                _baseGridHeaderFont = dgvCart.ColumnHeadersDefaultCellStyle.Font?.Size ?? 9.5f;
                foreach (DataGridViewColumn col in dgvCart.Columns)
                    if (col.DefaultCellStyle.Font != null)
                        _baseColFonts[col] = col.DefaultCellStyle.Font.Size;
            }
        }

        private void ApplyScale()
        {
            if (WindowState == FormWindowState.Minimized) return;
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            if (_header == null || dgvCart == null) return;

            // Kích thước thiết kế gốc: 1280 x 780 (đã nhân DPI)
            float fw = ClientSize.Width / (float)S(1280);
            float fh = ClientSize.Height / (float)S(780);
            float f = Math.Max(0.7f, Math.Min(1.8f, Math.Min(fw, fh)));

            if (Math.Abs(f - _lastScale) < 0.02f) return;
            _lastScale = f;

            SuspendLayout();

            _header.Height = (int)(S(56) * f);
            lblStatus.Height = (int)(S(28) * f);

            ScaleFonts(this, f);

            // Bảng giỏ hàng
            dgvCart.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", _baseGridHeaderFont * f, FontStyle.Bold);
            foreach (var kv in _baseColFonts)
            {
                var style = kv.Key.DefaultCellStyle.Font?.Style ?? FontStyle.Regular;
                kv.Key.DefaultCellStyle.Font = new Font("Segoe UI", kv.Value * f, style);
            }
            int rowH = (int)(S(42) * f);
            dgvCart.RowTemplate.Height = rowH;
            dgvCart.ColumnHeadersHeight = rowH;
            foreach (DataGridViewRow row in dgvCart.Rows) row.Height = rowH;

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

        // ===================== TIỆN ÍCH =====================
        private void Warn(string message)
        {
            System.Media.SystemSounds.Beep.Play();
            MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtBarcode?.Focus();
        }

        private void SetStatus(string message)
        {
            lblStatus.Text = "  " + message + "      |      F1 Mã vạch  F2 Khách  F3 Tiền  F4 Giữ  F5 Gọi  F6 Sửa SL  F7 Giảm giá  F8 Xóa giỏ  F9 Thanh toán";
        }

        private string? ShowInputDialog(string title, string prompt, string defaultValue)
        {
            using var dlg = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(420, 160),
                Font = Font
            };

            var lbl = new Label { Text = prompt, Left = 16, Top = 14, Width = 388, Height = 50 };
            var txt = new TextBox { Left = 16, Top = 70, Width = 388, Font = new Font("Segoe UI", 12F), Text = defaultValue };
            var ok = new Button { Text = "OK", Left = 228, Top = 112, Width = 80, Height = 32, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Hủy", Left = 324, Top = 112, Width = 80, Height = 32, DialogResult = DialogResult.Cancel };

            dlg.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
            dlg.AcceptButton = ok;
            dlg.CancelButton = cancel;
            dlg.Shown += (s, e) => { txt.Focus(); txt.SelectAll(); };

            return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
        }
    }

    // ===================== DTO / MODEL =====================
    public class CartItemDto
    {
        public int Stt { get; set; }
        public int ProductId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;

        public CartItemDto Clone() => new()
        {
            Stt = Stt,
            ProductId = ProductId,
            Barcode = Barcode,
            ProductName = ProductName,
            UnitPrice = UnitPrice,
            Quantity = Quantity,
            StockQuantity = StockQuantity
        };
    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Thông tin danh mục sản phẩm
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int RewardPoints { get; set; }
        public string MembershipRank { get; set; } = string.Empty;
    }

    public class CheckoutResultDto
    {
        public int EarnedPoints { get; set; }
        public int TotalPoints { get; set; }
    }

    public class HeldBill
    {
        public List<CartItemDto> Items { get; set; } = new();
        public string Phone { get; set; } = string.Empty;
        public string CustomerName { get; set; } = "Khách vãng lai";
        public decimal DiscountValue { get; set; }
        public bool DiscountIsPercent { get; set; }
        public DateTime Time { get; set; }
        public int? CustomerId { get; set; }
        public int Points { get; set; }
    }

    public class ReceiptData
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal Cash { get; set; }
        public decimal Change { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Cashier { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public int OldPoints { get; set; }       // Điểm cũ trước khi thanh toán
        public int RedeemedPoints { get; set; }  // Điểm đã đổi trừ tiền
        public int EarnedPoints { get; set; }    // Điểm cộng từ đơn hàng
        public int TotalPoints { get; set; }     // Tổng điểm còn lại
    }
}