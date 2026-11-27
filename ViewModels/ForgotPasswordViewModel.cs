
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SaigonBite.ViewModels;

public class ForgotPasswordViewModel : IValidatableObject
{
    [Required]
    public string RecoveryMethod { get; set; } = "local";

    [StringLength(10)]
    public string? Phone { get; set; }

    [StringLength(254)]
    public string? Email { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (RecoveryMethod == "local")
        {
            if (string.IsNullOrWhiteSpace(Phone) ||
                !Regex.IsMatch(
                    Phone.Trim(),
                    @"\A0[35789][0-9]{8}\z"))
            {
                yield return new ValidationResult(
                    "Invalid Vietnamese phone number.",
                    new[] { nameof(Phone) });
            }
        }
        else if (RecoveryMethod == "foreign")
        {
            if (string.IsNullOrWhiteSpace(Email) ||
                !new EmailAddressAttribute()
                    .IsValid(Email.Trim()))
            {
                yield return new ValidationResult(
                    "Invalid email address.",
                    new[] { nameof(Email) });
            }
        }
        else
        {
            yield return new ValidationResult(
                "Invalid recovery method.",
                new[] { nameof(RecoveryMethod) });
        }
    }
}
