
using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class RegisterLocalViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(
        @"^0[35789][0-9]{8}$",
        ErrorMessage = "Invalid Vietnamese phone number.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128, MinimumLength = 8,
        ErrorMessage = "Password must contain 8 to 128 characters.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9\s]).{8,128}$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password),
        ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true",
        ErrorMessage = "You must accept the terms and conditions.")]
    public bool AcceptTerms { get; set; }
}
