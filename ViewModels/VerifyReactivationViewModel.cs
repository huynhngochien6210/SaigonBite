using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class VerifyReactivationViewModel
{
    [Required(
        ErrorMessage = "Please enter the verification code.")]

    [RegularExpression(
        @"^[0-9]{6}$",
        ErrorMessage = "Verification code must contain 6 digits.")]

    public string Code { get; set; } = string.Empty;
}