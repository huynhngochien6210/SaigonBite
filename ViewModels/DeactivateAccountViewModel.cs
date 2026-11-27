using System.ComponentModel.DataAnnotations;

namespace SaigonBite.ViewModels;

public class DeactivateAccountViewModel
{
    [Required(ErrorMessage =
        "Please enter your current password.")]
    [StringLength(128)]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }
        = string.Empty;

    [Range(typeof(bool), "true", "true",
        ErrorMessage =
        "Please confirm account deactivation.")]
    public bool ConfirmDeactivation { get; set; }
}