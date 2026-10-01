namespace MiniSupermarket.WinForms
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblUser;
        private Label lblPass;
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblUser = new Label();
            lblPass = new Label();
            txtUser = new TextBox();
            txtPass = new TextBox();
            btnLogin = new Button();

            SuspendLayout();

            // ==============================
            // FormLogin
            // ==============================

            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;

            ClientSize = new Size(450, 300);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            StartPosition = FormStartPosition.CenterScreen;

            Text = "Đăng nhập hệ thống";

            // ==============================
            // lblTitle
            // ==============================

            lblTitle.AutoSize = true;

            lblTitle.Font = new Font(
                "Segoe UI",
                16F,
                FontStyle.Bold
            );

            lblTitle.Location = new Point(125, 30);

            lblTitle.Name = "lblTitle";

            lblTitle.Size = new Size(205, 30);

            lblTitle.Text = "ĐĂNG NHẬP HỆ THỐNG";

            // ==============================
            // lblUser
            // ==============================

            lblUser.AutoSize = true;

            lblUser.Location = new Point(55, 90);

            lblUser.Name = "lblUser";

            lblUser.Size = new Size(65, 15);

            lblUser.Text = "Tài khoản:";

            // ==============================
            // txtUser
            // ==============================

            txtUser.Location = new Point(145, 87);

            txtUser.Name = "txtUser";

            txtUser.Size = new Size(230, 23);

            // ==============================
            // lblPass
            // ==============================

            lblPass.AutoSize = true;

            lblPass.Location = new Point(55, 135);

            lblPass.Name = "lblPass";

            lblPass.Size = new Size(61, 15);

            lblPass.Text = "Mật khẩu:";

            // ==============================
            // txtPass
            // ==============================

            txtPass.Location = new Point(145, 132);

            txtPass.Name = "txtPass";

            txtPass.Size = new Size(230, 23);

            // Ẩn ký tự mật khẩu
            txtPass.UseSystemPasswordChar = true;

            // ==============================
            // btnLogin
            // ==============================

            btnLogin.Location = new Point(145, 185);

            btnLogin.Name = "btnLogin";

            btnLogin.Size = new Size(230, 40);

            btnLogin.Text = "Đăng nhập hệ thống";

            btnLogin.UseVisualStyleBackColor = true;

            // Gắn sự kiện Click cho nút Login
            btnLogin.Click += btnLogin_Click;

            // ==============================
            // Thêm các Control vào Form
            // ==============================

            Controls.Add(lblTitle);
            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(btnLogin);

            ResumeLayout(false);
            PerformLayout();
        }
    }
}