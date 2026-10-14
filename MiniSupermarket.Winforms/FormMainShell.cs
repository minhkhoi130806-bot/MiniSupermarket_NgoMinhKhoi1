using MiniSupermarket.Winforms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormMainShell : Form
    {
        // Các khu vực giao diện chính
        private Panel panelSidebar = null!;
        private Panel panelTopHeader = null!;
        private Panel panelMainContent = null!;
        private Panel panelUserFooter = null!;
        private FlowLayoutPanel panelMenu = null!;

        // Các nhãn hiển thị thông tin
        private Label lblTitle = null!;
        private Label lblUserInfo = null!;

        // Các nút chức năng
        private Button btnPOS = null!;
        private Button btnCategory = null!;
        private Button btnProduct = null!;
        private Button btnCustomer = null!;
        private Button btnReports = null!;
        private Button btnUserManage = null!;
        private Button btnLogout = null!;

        // Form con đang được hiển thị
        private Form? _activeForm = null;

        // Màu giao diện
        private readonly Color sidebarColor = Color.FromArgb(24, 30, 48);
        private readonly Color activeColor = Color.FromArgb(41, 100, 180);
        private readonly Color hoverColor = Color.FromArgb(35, 45, 68);

        public FormMainShell()
        {
            BuildShellLayout();
        }

        // ================= DỰNG GIAO DIỆN =================
        private void BuildShellLayout()
        {
            this.Size = new Size(1280, 720);
            this.MinimumSize = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Hệ thống Quản lý Bán lẻ & Tồn kho Siêu thị Mini";
            this.BackColor = Color.FromArgb(244, 245, 247);
            this.Font = new Font("Segoe UI", 10);
            this.DoubleBuffered = true;

            // Vùng nội dung chính
            panelMainContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(244, 245, 247),
                Padding = new Padding(12)
            };

            // Thanh tiêu đề phía trên
            panelTopHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };

            // Thanh menu bên trái
            panelSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 270,
                BackColor = sidebarColor
            };

            // Thứ tự Add: cái nào Add sau sẽ được Dock trước
            this.Controls.Add(panelMainContent);
            this.Controls.Add(panelTopHeader);
            this.Controls.Add(panelSidebar);

            BuildSidebar();
            BuildHeader();

            this.Load += FormMainShell_Load;
        }

        private void BuildSidebar()
        {
            Label lblLogo = new Label
            {
                Text = "🛒 MiniMart POS",
                Dock = DockStyle.Top,
                Height = 75,
                ForeColor = Color.White,
                BackColor = sidebarColor,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelSidebar.Controls.Add(lblLogo);

            // Khu vực chứa các nút menu
            panelMenu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10, 12, 10, 10),
                BackColor = sidebarColor
            };
            panelSidebar.Controls.Add(panelMenu);

            // Tạo các nút menu
            btnPOS = CreateSidebarButton("  🛒  Bán hàng (POS)");
            btnCategory = CreateSidebarButton("  📁  Quản lý Danh mục");
            btnProduct = CreateSidebarButton("  📦  Quản lý Sản phẩm");
            btnCustomer = CreateSidebarButton("  👥  Quản lý Khách hàng");
            btnReports = CreateSidebarButton("  📊  Báo cáo Doanh thu");
            btnUserManage = CreateSidebarButton("  🛡  Quản trị Tài khoản");

            panelMenu.Controls.Add(btnPOS);
            panelMenu.Controls.Add(btnCategory);
            panelMenu.Controls.Add(btnProduct);
            panelMenu.Controls.Add(btnCustomer);
            panelMenu.Controls.Add(btnReports);
            panelMenu.Controls.Add(btnUserManage);

            btnPOS.Click += btnPOS_Click;
            btnCategory.Click += btnCategory_Click;
            btnProduct.Click += btnProduct_Click;
            btnCustomer.Click += btnCustomer_Click;
            btnReports.Click += btnReports_Click;
            btnUserManage.Click += btnUserManage_Click;

            // Khu vực Đăng xuất ở đáy sidebar
            panelUserFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = sidebarColor,
                Padding = new Padding(10, 8, 10, 8)
            };

            btnLogout = CreateSidebarButton("  🚪  Đăng xuất");
            btnLogout.Dock = DockStyle.Fill;
            btnLogout.Click += btnLogout_Click;

            panelUserFooter.Controls.Add(btnLogout);
            panelSidebar.Controls.Add(panelUserFooter);

            // SỬA LỖI: đưa panelMenu lên trước để không bị logo che
            panelMenu.BringToFront();

            UpdateMenuButtonWidths();
            panelSidebar.Resize += (s, e) => UpdateMenuButtonWidths();
        }

        private Button CreateSidebarButton(string text)
        {
            Button btn = new Button
            {
                Text = text,
                Width = 208,
                Height = 45,
                Margin = new Padding(0, 0, 0, 7),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(220, 225, 235),
                BackColor = sidebarColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hoverColor;
            btn.FlatAppearance.MouseDownBackColor = activeColor;

            btn.MouseEnter += (s, e) =>
            {
                if (btn != GetActiveButton()) btn.BackColor = hoverColor;
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn != GetActiveButton()) btn.BackColor = sidebarColor;
            };

            return btn;
        }

        private void UpdateMenuButtonWidths()
        {
            if (panelMenu == null) return;

            int buttonWidth = Math.Max(150, panelMenu.ClientSize.Width - 25);

            foreach (Control control in panelMenu.Controls)
            {
                if (control is Button btn) btn.Width = buttonWidth;
            }
        }

        private void BuildHeader()
        {
            lblTitle = new Label
            {
                Text = "BÀN LÀM VIỆC HỆ THỐNG",
                Dock = DockStyle.Left,
                Width = 620,
                ForeColor = Color.FromArgb(24, 30, 48),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblUserInfo = new Label
            {
                Text = "Xin chào: ...",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(90, 100, 115),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true
            };

            panelTopHeader.Controls.Add(lblUserInfo);
            panelTopHeader.Controls.Add(lblTitle);
        }

        // ================= LOAD FORM =================
        private void FormMainShell_Load(object? sender, EventArgs e)
        {
            // Chưa đăng nhập thì không cho vào
            if (string.IsNullOrWhiteSpace(SessionManager.CurrentUsername) ||
                string.IsNullOrWhiteSpace(SessionManager.CurrentRole))
            {
                MessageBox.Show("Bạn chưa đăng nhập. Vui lòng đăng nhập lại!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            lblUserInfo.Text =
                $"Nhân viên: {SessionManager.CurrentUsername}   |   Vai trò: [{SessionManager.CurrentRole}]";

            // Nếu vai trò không hợp lệ thì dừng, không mở màn hình mặc định
            if (!ApplyRolePermissions(SessionManager.CurrentRole))
                return;

            OpenDefaultScreenByRole(SessionManager.CurrentRole);
        }

        // ================= NHÚNG FORM CON =================
        private void OpenChildForm(Form childForm, string screenTitle, Button senderButton)
        {
            if (childForm == null || senderButton == null) return;

            // Đóng form con trước đó
            if (_activeForm != null)
            {
                _activeForm.Close();
                _activeForm.Dispose();
                _activeForm = null;
            }

            panelMainContent.Controls.Clear();

            // Cấu hình form con để nhúng vào panel
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            _activeForm = childForm;

            panelMainContent.Controls.Add(childForm);
            panelMainContent.Tag = childForm;

            lblTitle.Text = screenTitle;
            HighlightActiveButton(senderButton);

            childForm.BringToFront();
            childForm.Show();
        }

        private Button? GetActiveButton()
        {
            foreach (Control control in panelMenu.Controls)
            {
                if (control is Button btn && btn.BackColor == activeColor)
                    return btn;
            }
            return null;
        }

        private void HighlightActiveButton(Button activeButton)
        {
            foreach (Control control in panelMenu.Controls)
            {
                if (control is Button btn)
                {
                    btn.BackColor = sidebarColor;
                    btn.ForeColor = Color.FromArgb(220, 225, 235);
                }
            }

            activeButton.BackColor = activeColor;
            activeButton.ForeColor = Color.White;
        }

        // ================= PHÂN QUYỀN =================
        // Trả về true nếu vai trò hợp lệ
        private bool ApplyRolePermissions(string role)
        {
            string currentRole = (role ?? string.Empty).Trim().ToUpperInvariant();

            switch (currentRole)
            {
                case "ADMIN":
                    btnPOS.Visible = true;
                    btnCategory.Visible = true;
                    btnProduct.Visible = true;
                    btnCustomer.Visible = true;
                    btnReports.Visible = true;
                    btnUserManage.Visible = true;
                    return true;

                case "CASHIER":
                    btnPOS.Visible = true;
                    btnCustomer.Visible = true;
                    btnCategory.Visible = false;
                    btnProduct.Visible = false;
                    btnReports.Visible = false;
                    btnUserManage.Visible = false;
                    return true;

                case "WAREHOUSE":
                    btnCategory.Visible = true;
                    btnProduct.Visible = true;
                    btnPOS.Visible = false;
                    btnCustomer.Visible = false;
                    btnReports.Visible = false;
                    btnUserManage.Visible = false;
                    return true;

                default:
                    MessageBox.Show("Tài khoản chưa được cấp vai trò hợp lệ!",
                        "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return false;
            }
        }

        private void OpenDefaultScreenByRole(string role)
        {
            string currentRole = (role ?? string.Empty).Trim().ToUpperInvariant();

            switch (currentRole)
            {
                case "ADMIN":
                    OpenChildForm(new FormCategoryManagement(),
                        "QUẢN LÝ DANH MỤC NHÓM HÀNG", btnCategory);
                    break;

                case "CASHIER":
                    OpenChildForm(new FormPOS(),
                        "HỆ THỐNG QUẦY BÁN HÀNG & THU NGÂN (POS)", btnPOS);
                    break;

                case "WAREHOUSE":
                    OpenChildForm(new FormProductManagement(),
                        "QUẢN LÝ THÔNG TIN SẢN PHẨM & KHO HÀNG", btnProduct);
                    break;
            }
        }

        // ================= SỰ KIỆN CLICK SIDEBAR =================
        private void btnCategory_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new FormCategoryManagement(),
                "QUẢN LÝ DANH MỤC NHÓM HÀNG", btnCategory);
        }

        private void btnProduct_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new FormProductManagement(),
                "QUẢN LÝ THÔNG TIN SẢN PHẨM & KHO HÀNG", btnProduct);
        }

        private void btnCustomer_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new FormCustomerManagement(),
                "QUẢN LÝ KHÁCH HÀNG & TÍCH ĐIỂM", btnCustomer);
        }

        private void btnPOS_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new FormPOS(),
                "HỆ THỐNG QUẦY BÁN HÀNG & THU NGÂN (POS)", btnPOS);
        }

        // Chỉ Admin được xem báo cáo (lớp bảo vệ thứ 2)
        private void btnReports_Click(object? sender, EventArgs e)
        {
            if (!string.Equals(SessionManager.CurrentRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không có quyền xem dữ liệu tài chính của siêu thị!",
                    "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            OpenChildForm(new FormQuickReport(),
                "BÁO CÁO DOANH THU & HIỆU SUẤT CA BÁN", btnReports);
        }

        // Chỉ Admin được quản trị tài khoản
        private void btnUserManage_Click(object? sender, EventArgs e)
        {
            if (!string.Equals(SessionManager.CurrentRole, "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Bạn không có quyền quản trị tài khoản!",
                    "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            OpenChildForm(new FormUserManagement(),
                "QUẢN TRỊ TÀI KHOẢN VÀ PHÂN QUYỀN HỆ THỐNG", btnUserManage);
        }

        // ================= ĐĂNG XUẤT =================
        private void btnLogout_Click(object? sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất phiên làm việc?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Xóa phiên đăng nhập
            SessionManager.JwtToken = string.Empty;
            SessionManager.CurrentUsername = string.Empty;
            SessionManager.CurrentRole = string.Empty;

            if (_activeForm != null)
            {
                _activeForm.Close();
                _activeForm.Dispose();
                _activeForm = null;
            }

            // Program.cs sẽ tự quay lại màn hình đăng nhập
            this.Close();
        }
    }
}