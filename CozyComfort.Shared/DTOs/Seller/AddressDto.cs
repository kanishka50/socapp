using System.ComponentModel.DataAnnotations;

namespace CozyComfort.Shared.DTOs.Seller
{
    public class AddressDto
    {
        [Required(ErrorMessage = "Street address is required")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "ZIP code is required")]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Invalid ZIP code")]
        public string ZipCode { get; set; } = string.Empty;
    }
}