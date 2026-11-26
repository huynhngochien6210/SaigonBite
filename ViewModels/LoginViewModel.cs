
using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email or phone number is required.")]
    [StringLength(254)]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
