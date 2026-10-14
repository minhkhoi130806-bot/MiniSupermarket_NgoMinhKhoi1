using System;
using System.Windows.Forms;
using MiniSupermarket.WinForms;

namespace MiniSupermarket.Winforms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            while (true)
            {
                // 1. Hiện màn hình đăng nhập
                using (FormLogin login = new FormLogin())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                        break; // Đóng login mà chưa đăng nhập => thoát chương trình
                }

                // 2. Đăng nhập thành công => mở Shell
                Application.Run(new FormMainShell());

                // 3. Shell đóng: session rỗng (Đăng xuất) => quay lại login, ngược lại thoát
                if (!string.IsNullOrEmpty(SessionManager.CurrentUsername))
                    break;
            }
        }
    }
}