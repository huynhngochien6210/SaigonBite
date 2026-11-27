using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(128)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
