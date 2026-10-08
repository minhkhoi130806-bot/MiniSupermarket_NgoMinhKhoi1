using System;
using System.Windows.Forms;

namespace MiniSupermarket.Winforms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // Khởi chạy trực tiếp Form quản lý khách hàng để nghiệm thu bài tập
            Application.Run(new FormCustomerManagement());
        }
    }
}