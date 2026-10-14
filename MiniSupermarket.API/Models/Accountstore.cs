using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniSupermarket.API.Services
{
    // Tai khoan luu trong bo nho (mat khi tat server)
    public class AccountEntry
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    // Kho tai khoan dung chung cho AuthController va UsersController
    public static class AccountStore
    {
        public static readonly string[] ValidRoles =
            { "Admin", "Cashier", "Warehouse" };

        private static readonly object _lock = new object();
        private static int _nextId = 1;
        private static readonly List<AccountEntry> _accounts = new List<AccountEntry>();

        static AccountStore()
        {
            // Tai khoan mau (giu nguyen nhu danh sach cu)
            Seed("admin01", "Quản trị viên 01", "Admin");
            Seed("admin02", "Quản trị viên 02", "Admin");

            for (int i = 1; i <= 6; i++)
                Seed($"cashier{i:00}", $"Thu ngân {i:00}", "Cashier");

            for (int i = 1; i <= 5; i++)
                Seed($"ware{i:00}", $"Thủ kho {i:00}", "Warehouse");

            Seed("admin_backup", "Quản trị dự phòng", "Admin");
            Seed("supervisor", "Giám sát", "Admin");
        }

        private static void Seed(string username, string fullName, string role)
        {
            _accounts.Add(new AccountEntry
            {
                Id = _nextId++,
                Username = username,
                Password = "123456",
                FullName = fullName,
                Role = role,
                IsActive = true
            });
        }

        // Lay ban sao danh sach tai khoan
        public static List<AccountEntry> GetAll()
        {
            lock (_lock)
            {
                return _accounts
                    .Select(a => new AccountEntry
                    {
                        Id = a.Id,
                        Username = a.Username,
                        Password = string.Empty, // khong tra mat khau ra ngoai
                        FullName = a.FullName,
                        Role = a.Role,
                        IsActive = a.IsActive
                    })
                    .ToList();
            }
        }

        // Tim tai khoan theo ten dang nhap (khong phan biet hoa thuong)
        public static AccountEntry? Find(string username)
        {
            lock (_lock)
            {
                return _accounts.FirstOrDefault(a =>
                    string.Equals(a.Username, username,
                        StringComparison.OrdinalIgnoreCase));
            }
        }

        // Them tai khoan moi, tra ve false neu da ton tai
        public static AccountEntry? TryAdd(
            string username,
            string password,
            string fullName,
            string role)
        {
            lock (_lock)
            {
                bool exists = _accounts.Any(a =>
                    string.Equals(a.Username, username,
                        StringComparison.OrdinalIgnoreCase));

                if (exists)
                    return null;

                var account = new AccountEntry
                {
                    Id = _nextId++,
                    Username = username,
                    Password = password,
                    FullName = fullName,
                    Role = role,
                    IsActive = true
                };

                _accounts.Add(account);
                return account;
            }
        }
    }
}