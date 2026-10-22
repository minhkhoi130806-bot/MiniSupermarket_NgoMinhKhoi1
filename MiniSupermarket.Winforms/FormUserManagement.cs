using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormUserManagement : Form
    {
        // ======================================================
        // CAU HINH ENDPOINT (sua tai day neu API cua ban khac)
        //   GET    users               -> danh sach
        //   POST   users               -> them   {Username, Password, FullName, Role}
        //   PUT    users/{id}          -> sua    {FullName, Role, Password?}
        //   PUT    users/{id}/status   -> khoa / mo khoa {IsActive}
        // ======================================================
        private const string UsersEndpoint = "users";
        private static string UserEndpoint(int id) => $"users/{id}";
        private static string UserStatusEndpoint(int id) => $"users/{id}/status";

        private static readonly string[] Roles = { "Admin", "Cashier", "Warehouse" };

        // ----- Control -----
        private DataGridView dgvUsers = null!;
        private TextBox txtUserId = null!;
        private TextBox txtUsername = null!;
        private TextBox txtFullName = null!;
        private TextBox txtPassword = null!;
        private ComboBox cboRole = null!;
        private TextBox txtKeyword = null!;
        private Label lblPasswordCaption = null!;
        private Label lblStatus = null!;

        private Button btnAdd = null!;
        private Button btnUpdate = null!;
        private Button btnToggleLock = null!;
        private Button btnClear = null!;
        private Button btnReload = null!;
        private Button btnSearch = null!;

        // ----- Mau sac dong bo voi cac form khac -----
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color textColor = Color.FromArgb(24, 30, 48);
        private readonly Color primaryColor = Color.FromArgb(41, 100, 180);
        private readonly Color mutedColor = Color.FromArgb(100, 116, 139);
        private readonly Color successColor = Color.FromArgb(40, 167, 69);
        private readonly Color dangerColor = Color.FromArgb(220, 53, 69);
        private readonly Color warningColor = Color.FromArgb(245, 158, 11);

        // ----- Du lieu / trang thai -----
        private bool isAdmin;
        private bool _busy;
        private List<UserDto> _allUsers = new();
        private UserDto? _selected;

        // ----- Co gian theo kich thuoc form -----
        private TableLayoutPanel _root = null!;
        private TableLayoutPanel _searchBar = null!;
        private Label _lblListTitle = null!;
        private float _lastScale = -1f;
        private const float BaseHeaderFont = 9f;
        private const float BaseCellFont = 9.5f;
        private const int DesignWidth = 1000;
        private const int DesignHeight = 560;
        private readonly Dictionary<Control, float> _baseFonts = new();
        private readonly Dictionary<Control, Font> _ownFonts = new();
        private Font? _boldCell;

        public FormUserManagement()
        {
            BuildUserManagementLayout();
        }

        // ======================================================
        // LOAD
        // ======================================================
        private async void FormUserManagement_Load(object? sender, EventArgs e)
        {
            isAdmin = string.Equals(
                SessionManager.CurrentRole?.Trim(),
                "Admin",
                StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                MessageBox.Show(
                    "Tài khoản không có quyền quản trị.\n" +
                    $"Vai trò hiện tại: {SessionManager.CurrentRole}",
                    "Không có quyền truy cập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Close();
                return;
            }

            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                MessageBox.Show(
                    "Không tìm thấy token đăng nhập. Vui lòng đăng nhập lại.",
                    "Chưa xác thực",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Close();
                return;
            }

            await LoadUsersAsync();
        }

        // ======================================================
        // GIAO DIEN
        // ======================================================
        private void BuildUserManagementLayout()
        {
            Text = "Quản trị tài khoản";
            Size = new Size(LogicalToDeviceUnits(1200), LogicalToDeviceUnits(700));
            MinimumSize = new Size(LogicalToDeviceUnits(700), LogicalToDeviceUnits(420));
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10F);

            Load += FormUserManagement_Load;

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

            _root.Controls.Add(new Label
            {
                Text = "QUẢN TRỊ TÀI KHOẢN NHÂN VIÊN",
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = textColor,
                BackColor = Color.White,
                Margin = Padding.Empty
            }, 0, 0);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = backgroundColor,
                Margin = Padding.Empty,
                Padding = new Padding(12)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _root.Controls.Add(body, 0, 1);

            body.Controls.Add(BuildListCard(), 0, 0);
            body.Controls.Add(BuildDetailCard(), 1, 0);

            RefreshButtons();

            RecordBaseFonts(this);
            Resize += (s, e) => ApplyScale();
            Shown += (s, e) => ApplyScale();
            ApplyScale();
        }

        // ---------- Danh sach (ben trai) ----------
        private Panel BuildListCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 6, 0)
            };

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
                PlaceholderText = "Tìm theo tên đăng nhập hoặc họ tên...",
                BorderStyle = BorderStyle.FixedSingle
            };
            txtKeyword.TextChanged += (s, e) => ApplyFilter();
            txtKeyword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ApplyFilter();
                }
            };

            btnSearch = CreateActionButton("TÌM KIẾM", primaryColor);
            btnSearch.Margin = new Padding(8, 0, 0, 0);
            btnSearch.Click += (s, e) => ApplyFilter();

            btnReload = CreateActionButton("TẢI LẠI", mutedColor);
            btnReload.Margin = new Padding(8, 0, 0, 0);
            btnReload.Click += async (s, e) =>
            {
                txtKeyword.Clear();
                await LoadUsersAsync();
            };

            _searchBar.Controls.Add(txtKeyword, 0, 0);
            _searchBar.Controls.Add(btnSearch, 1, 0);
            _searchBar.Controls.Add(btnReload, 2, 0);

            _lblListTitle = new Label
            {
                Text = "DANH SÁCH TÀI KHOẢN",
                Dock = DockStyle.Top,
                Height = LogicalToDeviceUnits(32),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblStatus = new Label
            {
                Text = "Chưa tải dữ liệu",
                Dock = DockStyle.Bottom,
                Height = LogicalToDeviceUnits(28),
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            dgvUsers = CreateGrid();
            dgvUsers.CellClick += dgvUsers_CellClick;
            dgvUsers.CellFormatting += dgvUsers_CellFormatting;

            // Fill add TRUOC, Top/Bottom add SAU
            card.Controls.Add(dgvUsers);
            card.Controls.Add(lblStatus);
            card.Controls.Add(_searchBar);
            card.Controls.Add(_lblListTitle);

            return card;
        }

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
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", BaseHeaderFont, FontStyle.Bold);

            grid.DefaultCellStyle.Font = new Font("Segoe UI", BaseCellFont);
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

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

            grid.Columns.Add(Col("Id", "ID", 8, center));
            grid.Columns.Add(Col("Username", "Tên đăng nhập", 24));
            grid.Columns.Add(Col("FullName", "Họ và tên", 30));
            grid.Columns.Add(Col("Role", "Vai trò", 16, center));
            grid.Columns.Add(Col("IsActive", "Trạng thái", 18, center));

            return grid;
        }

        // To mau vai tro / trang thai, lam dam ten dang nhap
        private void dgvUsers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.CellStyle == null) return;

            string col = dgvUsers.Columns[e.ColumnIndex].Name;

            if (col == "Username" || col == "Role" || col == "IsActive")
            {
                _boldCell ??= new Font(dgvUsers.DefaultCellStyle.Font ?? Font, FontStyle.Bold);
                e.CellStyle.Font = _boldCell;
            }

            if (col == "Username")
            {
                e.CellStyle.ForeColor = textColor;
            }
            else if (col == "Role")
            {
                string role = e.Value?.ToString() ?? "";
                e.CellStyle.ForeColor =
                    role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(220, 38, 38) :
                    role.Equals("Warehouse", StringComparison.OrdinalIgnoreCase) ? Color.FromArgb(180, 83, 9) :
                                                                                     Color.FromArgb(37, 99, 235);
            }
            else if (col == "IsActive")
            {
                bool active = e.Value is bool b && b;
                e.Value = active ? "Hoạt động" : "Đã khóa";
                e.FormattingApplied = true;
                e.CellStyle.ForeColor = active ? Color.FromArgb(22, 163, 74) : Color.FromArgb(220, 38, 38);
            }
        }

        // ---------- Thong tin chi tiet (ben phai) ----------
        private Control BuildDetailCard()
        {
            var card = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.White,
                Padding = new Padding(14),
                Margin = new Padding(6, 0, 0, 0)
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 9));    // tieu de
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 15));   // ma + ten dang nhap
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 15));   // ho ten
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 15));   // mat khau
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 15));   // vai tro
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 31));   // cac nut

            var lblFormTitle = new Label
            {
                Text = "THÔNG TIN TÀI KHOẢN",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtUserId = CreateTextBox(readOnly: true);
            txtUsername = CreateTextBox("Tên đăng nhập...");
            txtFullName = CreateTextBox("Họ tên nhân viên...");
            txtPassword = CreateTextBox("Mật khẩu...");
            txtPassword.UseSystemPasswordChar = true;

            cboRole = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat,
                Margin = Padding.Empty
            };
            cboRole.Items.AddRange(Roles);
            cboRole.SelectedItem = "Cashier";

            card.Controls.Add(lblFormTitle, 0, 0);
            card.Controls.Add(CreatePairRow(
                CreateFieldCell("ID", txtUserId, out _), 26,
                CreateFieldCell("Tên đăng nhập *", txtUsername, out _), 74), 0, 1);
            card.Controls.Add(CreateFieldCell("Họ và tên *", txtFullName, out _), 0, 2);
            card.Controls.Add(CreateFieldCell("Mật khẩu *", txtPassword, out lblPasswordCaption), 0, 3);
            card.Controls.Add(CreateFieldCell("Phân quyền vai trò", cboRole, out _), 0, 4);
            card.Controls.Add(BuildButtonGrid(), 0, 5);

            return card;
        }

        private TableLayoutPanel CreateFieldCell(string label, Control input, out Label labelControl)
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
            cell.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
            cell.RowStyles.Add(new RowStyle(SizeType.Percent, 60));

            labelControl = new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(55, 65, 81),
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true,
                Margin = new Padding(0, 0, 0, 2)
            };

            input.Dock = DockStyle.Top;
            cell.Controls.Add(labelControl, 0, 0);
            cell.Controls.Add(input, 0, 1);
            return cell;
        }

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

        private TableLayoutPanel BuildButtonGrid()
        {
            var grid = new TableLayoutPanel
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

            btnAdd = CreateActionButton("THÊM MỚI", successColor);
            btnAdd.Margin = new Padding(0, 0, 4, 4);
            btnAdd.Click += btnAdd_Click;

            btnUpdate = CreateActionButton("CẬP NHẬT", Color.FromArgb(23, 162, 184));
            btnUpdate.Margin = new Padding(4, 0, 0, 4);
            btnUpdate.Click += btnUpdate_Click;

            btnToggleLock = CreateActionButton("KHÓA TÀI KHOẢN", dangerColor);
            btnToggleLock.Margin = new Padding(0, 4, 4, 0);
            btnToggleLock.Click += btnToggleLock_Click;

            btnClear = CreateActionButton("LÀM MỚI", mutedColor);
            btnClear.Margin = new Padding(4, 4, 0, 0);
            btnClear.Click += (s, e) => ClearForm();

            grid.Controls.Add(btnAdd, 0, 0);
            grid.Controls.Add(btnUpdate, 1, 0);
            grid.Controls.Add(btnToggleLock, 0, 1);
            grid.Controls.Add(btnClear, 1, 1);
            return grid;
        }

        private Button CreateActionButton(string text, Color color)
        {
            var button = new Button
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
        // CHE DO THEM / SUA + TRANG THAI NUT
        // ======================================================
        // Chua chon dong nao  -> che do THEM  (nhap ten dang nhap + mat khau)
        // Da chon 1 tai khoan -> che do SUA   (ten dang nhap chi doc, mat khau tuy chon)
        private void RefreshButtons()
        {
            bool editing = _selected != null;

            btnAdd.Enabled = !_busy && !editing;
            btnUpdate.Enabled = !_busy && editing;
            btnToggleLock.Enabled = !_busy && editing;
            btnReload.Enabled = !_busy;
            btnSearch.Enabled = !_busy;
            btnClear.Enabled = !_busy;

            txtUsername.ReadOnly = editing;
            txtUsername.BackColor = editing ? Color.FromArgb(240, 242, 245) : Color.White;

            lblPasswordCaption.Text = editing
                ? "Mật khẩu mới (bỏ trống nếu không đổi)"
                : "Mật khẩu *";
            txtPassword.PlaceholderText = editing ? "Không đổi" : "Mật khẩu...";

            if (editing && _selected != null)
            {
                btnToggleLock.Text = _selected.IsActive ? "KHÓA TÀI KHOẢN" : "MỞ KHÓA";
                btnToggleLock.BackColor = _selected.IsActive ? dangerColor : successColor;
            }
            else
            {
                btnToggleLock.Text = "KHÓA TÀI KHOẢN";
                btnToggleLock.BackColor = dangerColor;
            }

            // Nut vo hieu hoa thi doi mau xam cho de nhan biet
            foreach (var b in new[] { btnAdd, btnUpdate, btnToggleLock })
                b.ForeColor = b.Enabled ? Color.White : Color.FromArgb(229, 231, 235);
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            RefreshButtons();
        }

        private void ClearForm()
        {
            _selected = null;
            txtUserId.Clear();
            txtUsername.Clear();
            txtFullName.Clear();
            txtPassword.Clear();
            cboRole.SelectedItem = "Cashier";

            dgvUsers.ClearSelection();
            RefreshButtons();
            txtUsername.Focus();
        }

        private void SelectUser(UserDto user)
        {
            _selected = user;
            txtUserId.Text = user.Id.ToString();
            txtUsername.Text = user.Username;
            txtFullName.Text = user.FullName;
            txtPassword.Clear();

            int idx = Array.FindIndex(Roles, r => r.Equals(user.Role, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                cboRole.SelectedIndex = idx;
            }
            else
            {
                // Vai tro la khong co trong danh sach co san -> them tam de khong mat du lieu
                if (!cboRole.Items.Contains(user.Role)) cboRole.Items.Add(user.Role);
                cboRole.SelectedItem = user.Role;
            }

            RefreshButtons();
        }

        private void dgvUsers_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvUsers.Rows.Count) return;
            if (dgvUsers.Rows[e.RowIndex].DataBoundItem is UserDto user)
                SelectUser(user);
        }

        // ======================================================
        // TIM KIEM (loc ngay tren danh sach da tai)
        // ======================================================
        private void ApplyFilter()
        {
            string keyword = txtKeyword.Text.Trim();

            IEnumerable<UserDto> result = _allUsers;
            if (keyword.Length > 0)
            {
                result = result.Where(u =>
                    (u.Username ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (u.FullName ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var list = result.ToList();
            dgvUsers.DataSource = null;
            dgvUsers.DataSource = list;

            // Giu lai dong dang chon neu con trong ket qua
            dgvUsers.ClearSelection();
            if (_selected != null)
            {
                int idx = list.FindIndex(u => u.Id == _selected.Id);
                if (idx >= 0)
                {
                    dgvUsers.Rows[idx].Selected = true;
                }
            }

            lblStatus.Text = keyword.Length > 0
                ? $"Kết quả: {list.Count}/{_allUsers.Count} tài khoản"
                : $"Tổng số tài khoản: {_allUsers.Count}  |  Đang khóa: {_allUsers.Count(u => !u.IsActive)}";
        }

        // ======================================================
        // API
        // ======================================================
        private bool PrepareAuthorization()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                MessageBox.Show(
                    "Bạn chưa đăng nhập hoặc token đã hết hạn. Vui lòng đăng nhập lại.",
                    "Chưa xác thực",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            ApiClientService.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", SessionManager.JwtToken);
            return true;
        }

        private async Task LoadUsersAsync()
        {
            if (!isAdmin || !PrepareAuthorization()) return;

            SetBusy(true);
            lblStatus.Text = "Đang tải danh sách tài khoản...";

            try
            {
                var users = await ApiClientService.Client.GetFromJsonAsync<List<UserDto>>(UsersEndpoint);
                _allUsers = users ?? new List<UserDto>();

                // Cap nhat lai doi tuong dang chon theo du lieu moi
                if (_selected != null)
                {
                    var fresh = _allUsers.FirstOrDefault(u => u.Id == _selected.Id);
                    if (fresh == null) ClearForm();
                    else SelectUser(fresh);
                }

                ApplyFilter();
            }
            catch (HttpRequestException ex)
            {
                lblStatus.Text = "Không thể kết nối máy chủ.";
                MessageBox.Show(
                    "Không thể kết nối API quản lý tài khoản.\n" + ex.Message,
                    "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (System.Text.Json.JsonException ex)
            {
                lblStatus.Text = "Dữ liệu máy chủ không hợp lệ.";
                MessageBox.Show(
                    "Không đọc được dữ liệu tài khoản trả về từ Server.\n" + ex.Message,
                    "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Tải danh sách thất bại.";
                MessageBox.Show("Lỗi tải danh sách tài khoản:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ---------- Them ----------
        private async void btnAdd_Click(object? sender, EventArgs e)
        {
            if (!isAdmin) return;

            string username = txtUsername.Text.Trim();
            string fullName = txtFullName.Text.Trim();
            string password = txtPassword.Text;

            if (username.Length == 0 || fullName.Length == 0 || password.Length == 0)
            {
                Warn("Vui lòng nhập đầy đủ tên đăng nhập, họ tên và mật khẩu.", "Thiếu thông tin");
                return;
            }
            if (username.Contains(' '))
            {
                Warn("Tên đăng nhập không được chứa khoảng trắng.", "Dữ liệu không hợp lệ");
                txtUsername.Focus();
                return;
            }
            if (password.Length < 6)
            {
                Warn("Mật khẩu phải có ít nhất 6 ký tự.", "Dữ liệu không hợp lệ");
                txtPassword.Focus();
                return;
            }
            if (cboRole.SelectedItem == null)
            {
                Warn("Vui lòng chọn vai trò tài khoản.", "Thiếu thông tin");
                return;
            }
            if (!PrepareAuthorization()) return;

            var newUser = new
            {
                Username = username,
                Password = password,
                FullName = fullName,
                Role = cboRole.SelectedItem.ToString()
            };

            SetBusy(true);
            try
            {
                using var response = await ApiClientService.Client.PostAsJsonAsync(UsersEndpoint, newUser);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tạo tài khoản mới thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ClearForm();
                    await LoadUsersAsync();
                }
                else
                {
                    await ShowResponseErrorAsync("Tạo tài khoản thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowException("Lỗi khi tạo tài khoản:", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ---------- Sua ----------
        private async void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (!isAdmin || _selected == null)
            {
                Warn("Vui lòng chọn tài khoản cần cập nhật trong bảng.", "Thông báo");
                return;
            }

            string fullName = txtFullName.Text.Trim();
            string password = txtPassword.Text;
            string role = cboRole.SelectedItem?.ToString() ?? _selected.Role;

            if (fullName.Length == 0)
            {
                Warn("Họ tên không được để trống.", "Thiếu thông tin");
                txtFullName.Focus();
                return;
            }
            if (password.Length > 0 && password.Length < 6)
            {
                Warn("Mật khẩu mới phải có ít nhất 6 ký tự.", "Dữ liệu không hợp lệ");
                txtPassword.Focus();
                return;
            }

            // Khong cho tu ha quyen chinh minh de tranh mat quyen quan tri
            if (IsCurrentUser(_selected) &&
                !role.Equals(_selected.Role, StringComparison.OrdinalIgnoreCase))
            {
                Warn("Bạn không thể tự đổi vai trò của chính mình.", "Không được phép");
                cboRole.SelectedItem = _selected.Role;
                return;
            }

            if (!PrepareAuthorization()) return;

            int id = _selected.Id;
            var request = new
            {
                FullName = fullName,
                Role = role,
                Password = password.Length == 0 ? null : password
            };

            SetBusy(true);
            try
            {
                using var response = await ApiClientService.Client
                    .PutAsJsonAsync(UserEndpoint(id), request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật tài khoản thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtPassword.Clear();
                    await LoadUsersAsync();
                }
                else
                {
                    await ShowResponseErrorAsync("Cập nhật tài khoản thất bại.", response);
                }
            }
            catch (Exception ex)
            {
                ShowException("Lỗi khi cập nhật tài khoản:", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // ---------- Khoa / Mo khoa ----------
        private async void btnToggleLock_Click(object? sender, EventArgs e)
        {
            if (!isAdmin || _selected == null)
            {
                Warn("Vui lòng chọn tài khoản trong bảng.", "Thông báo");
                return;
            }

            bool lockIt = _selected.IsActive;   // dang hoat dong -> se khoa

            if (lockIt && IsCurrentUser(_selected))
            {
                Warn("Bạn không thể tự khóa tài khoản đang đăng nhập.", "Không được phép");
                return;
            }

            string action = lockIt ? "khóa" : "mở khóa";
            string extra = lockIt ? "\nNgười dùng này sẽ không thể đăng nhập cho đến khi được mở khóa." : "";

            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn {action} tài khoản \"{_selected.Username}\"?{extra}",
                $"Xác nhận {action}",
                MessageBoxButtons.YesNo,
                lockIt ? MessageBoxIcon.Warning : MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes || !PrepareAuthorization()) return;

            int id = _selected.Id;

            SetBusy(true);
            try
            {
                using var response = await ApiClientService.Client
                    .PutAsJsonAsync(UserStatusEndpoint(id), new { IsActive = !lockIt });

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Đã {action} tài khoản thành công!", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await LoadUsersAsync();
                }
                else
                {
                    await ShowResponseErrorAsync($"Không thể {action} tài khoản.", response);
                }
            }
            catch (Exception ex)
            {
                ShowException($"Lỗi khi {action} tài khoản:", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static bool IsCurrentUser(UserDto user) =>
            string.Equals(user.Username, SessionManager.CurrentUsername?.Trim(),
                StringComparison.OrdinalIgnoreCase);

        // ======================================================
        // THONG BAO LOI
        // ======================================================
        private static void Warn(string message, string title) =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private static async Task ShowResponseErrorAsync(string message, HttpResponseMessage response)
        {
            string detail = await response.Content.ReadAsStringAsync();

            string reason = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.",
                HttpStatusCode.Forbidden => "Server từ chối quyền thực hiện thao tác này.",
                HttpStatusCode.BadRequest => "Thông tin gửi lên không hợp lệ.",
                HttpStatusCode.NotFound => "Không tìm thấy tài khoản hoặc API tương ứng.",
                HttpStatusCode.Conflict => "Tên đăng nhập có thể đã tồn tại.",
                _ => "Server không thể xử lý yêu cầu."
            };

            MessageBox.Show(
                $"{message}\n{reason}\n" +
                $"Mã HTTP: {(int)response.StatusCode} ({response.StatusCode})\n" +
                (string.IsNullOrWhiteSpace(detail) ? "" : $"Chi tiết: {detail}"),
                "Lỗi API",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static void ShowException(string message, Exception ex)
        {
            string title = ex is HttpRequestException ? "Lỗi kết nối" : "Lỗi";
            MessageBox.Show($"{message}\n{ex.Message}", title,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (dgvUsers == null || _root == null) return;

            float fw = ClientSize.Width / (float)LogicalToDeviceUnits(DesignWidth);
            float fh = ClientSize.Height / (float)LogicalToDeviceUnits(DesignHeight);
            float f = Math.Max(0.7f, Math.Min(1.5f, Math.Min(fw, fh)));

            if (Math.Abs(f - _lastScale) < 0.02f) return;
            _lastScale = f;

            SuspendLayout();

            _root.RowStyles[0].Height = LogicalToDeviceUnits(50) * f;
            _searchBar.Height = (int)(LogicalToDeviceUnits(46) * f);
            _searchBar.ColumnStyles[1].Width = LogicalToDeviceUnits(110) * f;
            _searchBar.ColumnStyles[2].Width = LogicalToDeviceUnits(100) * f;
            _lblListTitle.Height = (int)(LogicalToDeviceUnits(32) * f);
            lblStatus.Height = (int)(LogicalToDeviceUnits(28) * f);

            ScaleFonts(this, f);

            dgvUsers.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", BaseHeaderFont * f, FontStyle.Bold);
            dgvUsers.DefaultCellStyle.Font = new Font("Segoe UI", BaseCellFont * f);
            _boldCell?.Dispose();
            _boldCell = null;

            int rowH = (int)(LogicalToDeviceUnits(40) * f);
            dgvUsers.RowTemplate.Height = rowH;
            dgvUsers.ColumnHeadersHeight = (int)(LogicalToDeviceUnits(42) * f);
            foreach (DataGridViewRow row in dgvUsers.Rows) row.Height = rowH;

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
    }

    // Du lieu hien thi tren bang tai khoan
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}