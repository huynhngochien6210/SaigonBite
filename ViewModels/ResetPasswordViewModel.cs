
using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(128, MinimumLength = 8,
        ErrorMessage = "Password must contain 8 to 128 characters.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9\s]).{8,128}$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
        = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [StringLength(128)]
    [Compare(nameof(NewPassword),
        ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; }
        = string.Empty;
}
