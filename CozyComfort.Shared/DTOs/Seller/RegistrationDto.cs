namespace CozyComfort.Shared.DTOs.Seller
{
    public class RegisterCustomerDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;  // Changed from PhoneNumber to Phone
        public string Address { get; set; } = string.Empty;
    }
}