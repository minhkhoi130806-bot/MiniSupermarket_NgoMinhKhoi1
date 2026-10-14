using System;
using System.Collections.Generic;
using System.Drawing;
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
        private DataGridView dgvUsers = null!;
        private TextBox txtUsername = null!;
        private TextBox txtFullName = null!;
        private TextBox txtPassword = null!;
        private ComboBox cboRole = null!;
        private Button btnAddUser = null!;
        private Button btnReload = null!;
        private Label lblStatus = null!;
        private SplitContainer split = null!;

        private readonly Color primaryColor = Color.FromArgb(41, 100, 180);
        private readonly Color backgroundColor = Color.FromArgb(244, 246, 250);
        private readonly Color textColor = Color.FromArgb(24, 30, 48);

        // Kich thuoc toi thieu cua 2 panel (gan trong Load, KHONG gan trong constructor)
        private const int Panel1Min = 250;
        private const int Panel2Min = 250;

        private bool isAdmin;

        public FormUserManagement()
        {
            BuildUserManagementLayout();
        }

        // ======================================================
        // LOAD: form da co kich thuoc that -> an toan de dat split
        // ======================================================
        private async void FormUserManagement_Load(object? sender, EventArgs e)
        {
            ConfigureSplitContainer();

            isAdmin = string.Equals(
                SessionManager.CurrentRole?.Trim(),
                "Admin",
                StringComparison.OrdinalIgnoreCase
            );

            if (!isAdmin)
            {
                MessageBox.Show(
                    $"Tài khoản không có quyền quản trị.\n" +
                    $"Vai trò hiện tại: {SessionManager.CurrentRole}",
                    "Không có quyền truy cập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                Close();
                return;
            }

            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                MessageBox.Show(
                    "Không tìm thấy token đăng nhập. Vui lòng đăng nhập lại.",
                    "Chưa xác thực",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                Close();
                return;
            }

            await LoadUsersAsync();
        }

        // Gan MinSize va SplitterDistance theo dung thu tu, co kiem tra gioi han
        private void ConfigureSplitContainer()
        {
            int width = split.Width;

            // Khong du cho -> khong rang buoc MinSize de tranh exception
            if (width < Panel1Min + Panel2Min + split.SplitterWidth)
                return;

            // Dat SplitterDistance truoc (trong khoang hop le), roi moi gan MinSize
            int desired = width - Panel2Min - split.SplitterWidth - 60;
            desired = Math.Clamp(
                desired,
                0,
                width - split.SplitterWidth
            );

            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;
            split.SplitterDistance = Math.Max(desired, Panel1Min);

            split.Panel1MinSize = Panel1Min;
            split.Panel2MinSize = Panel2Min;
        }

        // ======================================================
        // XAY DUNG GIAO DIEN
        // ======================================================
        private void BuildUserManagementLayout()
        {
            Text = "Quản trị tài khoản";
            Size = new Size(1100, 680);
            MinimumSize = new Size(850, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = backgroundColor;
            Font = new Font("Segoe UI", 10);
            AutoScaleMode = AutoScaleMode.Font;

            Load += FormUserManagement_Load;

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

            // Tieu de
            Label lblTitle = new Label
            {
                Text = "QUẢN TRỊ TÀI KHOẢN NHÂN VIÊN",
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = textColor,
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            root.Controls.Add(lblTitle, 0, 0);

            // SplitContainer: KHONG gan MinSize / SplitterDistance o day
            split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel2,
                SplitterWidth = 6,
                BackColor = backgroundColor,
                Margin = Padding.Empty
            };

            root.Controls.Add(split, 0, 1);

            BuildListPanel();
            BuildCreatePanel();
        }

        // Khu vuc danh sach tai khoan (ben trai)
        private void BuildListPanel()
        {
            Panel panelList = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            split.Panel1.Controls.Add(panelList);

            Label lblListTitle = new Label
            {
                Text = "Danh sách tài khoản",
                Dock = DockStyle.Top,
                Height = 42,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = textColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnReload = new Button
            {
                Text = "Tải lại danh sách",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = primaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            btnReload.FlatAppearance.BorderSize = 0;
            btnReload.Click += async (s, e) => await LoadUsersAsync();

            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EnableHeadersVisualStyles = false
            };

            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(235, 240, 248);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = textColor;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);

            dgvUsers.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvUsers.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(218, 232, 250);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 253);

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                HeaderText = "ID",
                FillWeight = 35,
                MinimumWidth = 45
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Username",
                DataPropertyName = "Username",
                HeaderText = "Tên đăng nhập",
                FillWeight = 100,
                MinimumWidth = 100
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FullName",
                DataPropertyName = "FullName",
                HeaderText = "Họ và tên",
                FillWeight = 120,
                MinimumWidth = 100
            });

            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Role",
                DataPropertyName = "Role",
                HeaderText = "Vai trò",
                FillWeight = 75,
                MinimumWidth = 70
            });

            dgvUsers.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsActive",
                DataPropertyName = "IsActive",
                HeaderText = "Hoạt động",
                FillWeight = 65,
                MinimumWidth = 75
            });

            lblStatus = new Label
            {
                Text = "Chưa tải dữ liệu",
                Dock = DockStyle.Bottom,
                Height = 32,
                ForeColor = Color.DimGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // THU TU ADD QUAN TRONG khi dung Dock:
            // control Fill phai duoc add TRUOC, cac control Top/Bottom add SAU
            // (control add sau se duoc dock truoc).
            panelList.Controls.Add(dgvUsers);      // Fill
            panelList.Controls.Add(lblStatus);     // Bottom
            panelList.Controls.Add(btnReload);     // Top (nam duoi tieu de)
            panelList.Controls.Add(lblListTitle);  // Top (tren cung)
        }

        // Khu vuc tao tai khoan (ben phai)
        private void BuildCreatePanel()
        {
            Panel panelRight = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(18),
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            split.Panel2.Controls.Add(panelRight);

            Label lblFormTitle = new Label
            {
                Text = "CẤP TÀI KHOẢN MỚI",
                Location = new Point(18, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = textColor
            };

            panelRight.Controls.Add(lblFormTitle);

            int y = 65;

            AddField(panelRight, "Tên đăng nhập", ref txtUsername, ref y);
            AddField(panelRight, "Họ và tên nhân viên", ref txtFullName, ref y);
            AddField(panelRight, "Mật khẩu ban đầu", ref txtPassword, ref y);

            txtPassword.UseSystemPasswordChar = true;

            Label lblRole = new Label
            {
                Text = "Phân quyền vai trò",
                Location = new Point(18, y),
                AutoSize = true
            };

            panelRight.Controls.Add(lblRole);

            cboRole = new ComboBox
            {
                Location = new Point(18, y + 25),
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };

            cboRole.Items.AddRange(new object[]
            {
                "Admin",
                "Cashier",
                "Warehouse"
            });

            cboRole.SelectedItem = "Cashier";
            panelRight.Controls.Add(cboRole);

            y += 85;

            btnAddUser = new Button
            {
                Text = "TẠO TÀI KHOẢN",
                Location = new Point(18, y),
                Size = new Size(260, 44),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnAddUser.FlatAppearance.BorderSize = 0;
            btnAddUser.Click += btnAddUser_Click;
            panelRight.Controls.Add(btnAddUser);

            Label lblNote = new Label
            {
                Text =
                    "Lưu ý:\n" +
                    "• Vai trò quyết định quyền sử dụng chức năng.\n" +
                    "• Hãy kiểm tra thông tin trước khi tạo tài khoản.",
                Location = new Point(18, y + 58),
                Size = new Size(280, 90),
                ForeColor = Color.DimGray,
                AutoSize = false
            };

            panelRight.Controls.Add(lblNote);
        }

        // Tao nhan va o nhap lieu dung chung
        private void AddField(
            Panel parent,
            string labelText,
            ref TextBox textBox,
            ref int y)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(18, y),
                AutoSize = true
            };

            parent.Controls.Add(label);

            textBox = new TextBox
            {
                Location = new Point(18, y + 25),
                Width = 260,
                Font = new Font("Segoe UI", 10)
            };

            parent.Controls.Add(textBox);

            y += 75;
        }

        // ======================================================
        // XU LY API
        // ======================================================

        // Gan token cho request API
        private bool PrepareAuthorization()
        {
            if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
            {
                MessageBox.Show(
                    "Bạn chưa đăng nhập hoặc token đã hết hạn. Vui lòng đăng nhập lại.",
                    "Chưa xác thực",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return false;
            }

            ApiClientService.Client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    SessionManager.JwtToken
                );

            return true;
        }

        // Tai danh sach tai khoan
        private async Task LoadUsersAsync()
        {
            if (!isAdmin || !PrepareAuthorization())
                return;

            btnReload.Enabled = false;
            lblStatus.Text = "Đang tải danh sách tài khoản...";

            try
            {
                var users = await ApiClientService.Client
                    .GetFromJsonAsync<List<UserDto>>("users");

                dgvUsers.DataSource = null;
                dgvUsers.DataSource = users ?? new List<UserDto>();

                lblStatus.Text = $"Tổng số tài khoản: {dgvUsers.Rows.Count}";
            }
            catch (HttpRequestException ex)
            {
                lblStatus.Text = "Không thể kết nối máy chủ.";

                MessageBox.Show(
                    "Không thể kết nối API quản lý tài khoản.\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (System.Text.Json.JsonException ex)
            {
                lblStatus.Text = "Dữ liệu máy chủ không hợp lệ.";

                MessageBox.Show(
                    "Không đọc được dữ liệu tài khoản trả về từ Server.\n" +
                    ex.Message,
                    "Lỗi dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Tải danh sách thất bại.";

                MessageBox.Show(
                    "Lỗi tải danh sách tài khoản:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnReload.Enabled = true;
            }
        }

        // Tao tai khoan moi
        private async void btnAddUser_Click(object? sender, EventArgs e)
        {
            if (!isAdmin)
            {
                MessageBox.Show(
                    "Bạn không có quyền tạo tài khoản.",
                    "Từ chối truy cập",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!PrepareAuthorization())
                return;

            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ tên đăng nhập, họ tên và mật khẩu.",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (cboRole.SelectedItem == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn vai trò tài khoản.",
                    "Thiếu thông tin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            var newUser = new
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text,
                FullName = txtFullName.Text.Trim(),
                Role = cboRole.SelectedItem.ToString()
            };

            btnAddUser.Enabled = false;

            try
            {
                using var response = await ApiClientService.Client
                    .PostAsJsonAsync("users", newUser);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Tạo tài khoản mới thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    txtUsername.Clear();
                    txtFullName.Clear();
                    txtPassword.Clear();
                    cboRole.SelectedItem = "Cashier";

                    await LoadUsersAsync();
                }
                else
                {
                    string detail = await response.Content.ReadAsStringAsync();

                    string message = response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized =>
                            "Bạn chưa đăng nhập hoặc phiên làm việc đã hết hạn.",

                        HttpStatusCode.Forbidden =>
                            "Server từ chối quyền tạo tài khoản của bạn.",

                        HttpStatusCode.BadRequest =>
                            "Thông tin tài khoản không hợp lệ.",

                        HttpStatusCode.Conflict =>
                            "Tên tài khoản có thể đã tồn tại.",

                        _ =>
                            "Server không thể tạo tài khoản."
                    };

                    MessageBox.Show(
                        $"{message}\n" +
                        $"Mã HTTP: {(int)response.StatusCode} ({response.StatusCode})\n" +
                        $"Chi tiết: {detail}",
                        "Lỗi API",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "Không thể kết nối Server:\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi tạo tài khoản:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnAddUser.Enabled = true;
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