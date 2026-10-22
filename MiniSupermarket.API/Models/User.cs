
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MiniSupermarket.API.Models
{
    // Model tai khoan luu trong SQL Server
    [Index(nameof(Username), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
