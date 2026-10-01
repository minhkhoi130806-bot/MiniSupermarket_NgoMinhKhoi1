
namespace MiniSupermarket.WinForms
{
    partial class FormCategoryManagement
    {
        private System.ComponentModel.IContainer components = null;

        private GroupBox grpSearch;
        private Label lblKeyword;
        private TextBox txtKeyword;
        private Button btnSearch;
        private Button btnLoad;

        private GroupBox grpCategories;
        private DataGridView dgvCategories;

        private GroupBox grpInfo;
        private Label lblId;
        private Label lblCategoryName;
        private Label lblDescription;
        private TextBox txtId;
        private TextBox txtCategoryName;
        private TextBox txtDescription;

        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            grpSearch = new GroupBox();
            lblKeyword = new Label();
            txtKeyword = new TextBox();
            btnSearch = new Button();
            btnLoad = new Button();
            grpCategories = new GroupBox();
            dgvCategories = new DataGridView();
            grpInfo = new GroupBox();
            lblId = new Label();
            lblCategoryName = new Label();
            lblDescription = new Label();
            txtId = new TextBox();
            txtCategoryName = new TextBox();
            txtDescription = new TextBox();
            btnAdd = new Button();
            btnUpdate = new Button();
            btnDelete = new Button();
            grpSearch.SuspendLayout();
            grpCategories.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCategories).BeginInit();
            grpInfo.SuspendLayout();
            SuspendLayout();
            // 
            // grpSearch
            // 
            grpSearch.Controls.Add(lblKeyword);
            grpSearch.Controls.Add(txtKeyword);
            grpSearch.Controls.Add(btnSearch);
            grpSearch.Controls.Add(btnLoad);
            grpSearch.Location = new Point(17, 20);
            grpSearch.Margin = new Padding(4, 5, 4, 5);
            grpSearch.Name = "grpSearch";
            grpSearch.Padding = new Padding(4, 5, 4, 5);
            grpSearch.Size = new Size(764, 125);
            grpSearch.TabIndex = 0;
            grpSearch.TabStop = false;
            grpSearch.Text = "Tìm kiếm";
            // 
            // lblKeyword
            // 
            lblKeyword.AutoSize = true;
            lblKeyword.Location = new Point(21, 53);
            lblKeyword.Margin = new Padding(4, 0, 4, 0);
            lblKeyword.Name = "lblKeyword";
            lblKeyword.Size = new Size(0, 25);
            lblKeyword.TabIndex = 0;
            // 
            // txtKeyword
            // 
            txtKeyword.Location = new Point(21, 43);
            txtKeyword.Margin = new Padding(4, 5, 4, 5);
            txtKeyword.Name = "txtKeyword";
            txtKeyword.PlaceholderText = "Nhập từ khóa...";
            txtKeyword.Size = new Size(448, 31);
            txtKeyword.TabIndex = 1;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(493, 40);
            btnSearch.Margin = new Padding(4, 5, 4, 5);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(121, 47);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Tìm kiếm";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(629, 40);
            btnLoad.Margin = new Padding(4, 5, 4, 5);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(114, 47);
            btnLoad.TabIndex = 3;
            btnLoad.Text = "Tải lại";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // grpCategories
            // 
            grpCategories.Controls.Add(dgvCategories);
            grpCategories.Location = new Point(17, 163);
            grpCategories.Margin = new Padding(4, 5, 4, 5);
            grpCategories.Name = "grpCategories";
            grpCategories.Padding = new Padding(4, 5, 4, 5);
            grpCategories.Size = new Size(764, 592);
            grpCategories.TabIndex = 1;
            grpCategories.TabStop = false;
            grpCategories.Text = "Danh sách Nhóm hàng";
            // 
            // dgvCategories
            // 
            dgvCategories.AllowUserToAddRows = false;
            dgvCategories.AllowUserToDeleteRows = false;
            dgvCategories.BackgroundColor = SystemColors.Window;
            dgvCategories.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCategories.Location = new Point(14, 42);
            dgvCategories.Margin = new Padding(4, 5, 4, 5);
            dgvCategories.MultiSelect = false;
            dgvCategories.Name = "dgvCategories";
            dgvCategories.ReadOnly = true;
            dgvCategories.RowHeadersWidth = 35;
            dgvCategories.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCategories.Size = new Size(736, 533);
            dgvCategories.TabIndex = 0;
            dgvCategories.CellClick += dgvCategories_CellClick;
            // 
            // grpInfo
            // 
            grpInfo.Controls.Add(lblId);
            grpInfo.Controls.Add(lblCategoryName);
            grpInfo.Controls.Add(lblDescription);
            grpInfo.Controls.Add(txtId);
            grpInfo.Controls.Add(txtCategoryName);
            grpInfo.Controls.Add(txtDescription);
            grpInfo.Location = new Point(796, 163);
            grpInfo.Margin = new Padding(4, 5, 4, 5);
            grpInfo.Name = "grpInfo";
            grpInfo.Padding = new Padding(4, 5, 4, 5);
            grpInfo.Size = new Size(386, 383);
            grpInfo.TabIndex = 2;
            grpInfo.TabStop = false;
            grpInfo.Text = "Thông tin Nhóm hàng";
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new Point(17, 45);
            lblId.Margin = new Padding(4, 0, 4, 0);
            lblId.Name = "lblId";
            lblId.Size = new Size(60, 25);
            lblId.TabIndex = 0;
            lblId.Text = "Mã ID";
            // 
            // lblCategoryName
            // 
            lblCategoryName.AutoSize = true;
            lblCategoryName.Location = new Point(17, 137);
            lblCategoryName.Margin = new Padding(4, 0, 4, 0);
            lblCategoryName.Name = "lblCategoryName";
            lblCategoryName.Size = new Size(276, 25);
            lblCategoryName.TabIndex = 2;
            lblCategoryName.Text = "Tên Nhóm hàng (Ví dụ: Bánh kẹo)";
            // 
            // lblDescription
            // 
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(17, 228);
            lblDescription.Margin = new Padding(4, 0, 4, 0);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(190, 25);
            lblDescription.TabIndex = 4;
            lblDescription.Text = "Mô tả (Mô tả chi tiết...)";
            // 
            // txtId
            // 
            txtId.Location = new Point(17, 78);
            txtId.Margin = new Padding(4, 5, 4, 5);
            txtId.Name = "txtId";
            txtId.ReadOnly = true;
            txtId.Size = new Size(350, 31);
            txtId.TabIndex = 1;
            // 
            // txtCategoryName
            // 
            txtCategoryName.Location = new Point(17, 170);
            txtCategoryName.Margin = new Padding(4, 5, 4, 5);
            txtCategoryName.Name = "txtCategoryName";
            txtCategoryName.Size = new Size(350, 31);
            txtCategoryName.TabIndex = 3;
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(17, 262);
            txtDescription.Margin = new Padding(4, 5, 4, 5);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.Size = new Size(350, 97);
            txtDescription.TabIndex = 5;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(814, 575);
            btnAdd.Margin = new Padding(4, 5, 4, 5);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(114, 50);
            btnAdd.TabIndex = 3;
            btnAdd.Text = "Thêm mới";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(940, 575);
            btnUpdate.Margin = new Padding(4, 5, 4, 5);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(114, 50);
            btnUpdate.TabIndex = 4;
            btnUpdate.Text = "Cập nhật";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(1066, 575);
            btnDelete.Margin = new Padding(4, 5, 4, 5);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(114, 50);
            btnDelete.TabIndex = 5;
            btnDelete.Text = "Xóa";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // FormCategoryManagement
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1199, 800);
            Controls.Add(grpSearch);
            Controls.Add(grpCategories);
            Controls.Add(grpInfo);
            Controls.Add(btnAdd);
            Controls.Add(btnUpdate);
            Controls.Add(btnDelete);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "FormCategoryManagement";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản lý Danh mục Nhóm hàng - FormCategoryManagement";
            Load += FormCategoryManagement_Load;
            grpSearch.ResumeLayout(false);
            grpSearch.PerformLayout();
            grpCategories.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCategories).EndInit();
            grpInfo.ResumeLayout(false);
            grpInfo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}