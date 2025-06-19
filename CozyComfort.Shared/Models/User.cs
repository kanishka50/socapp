using CozyComfort.Shared.Enums;

namespace CozyComfort.Shared.Models
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}