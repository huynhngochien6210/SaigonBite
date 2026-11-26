
using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class EditAccountViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(254)]
    public string? Email { get; set; }

    [StringLength(20)]
    [RegularExpression(
        @"^\+?[0-9]{7,15}$",
        ErrorMessage = "Invalid phone number.")]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(100)]
    public string? Nationality { get; set; }
}
