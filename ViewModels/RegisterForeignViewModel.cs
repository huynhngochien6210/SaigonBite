
using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class RegisterForeignViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passport number is required.")]
    [StringLength(9, MinimumLength = 6)]
    [RegularExpression(
        @"^[A-Za-z0-9]{6,9}$",
        ErrorMessage = "Passport must contain 6 to 9 letters or digits.")]
    public string PassportNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nationality is required.")]
    [StringLength(100)]
    public string Nationality { get; set; } = string.Empty;

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
