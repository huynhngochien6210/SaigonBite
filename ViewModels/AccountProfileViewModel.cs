using SaigonBite.Enums;

namespace SaigonBite.ViewModels;

public class AccountProfileViewModel
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? PassportNumber { get; set; }

    public string? Nationality { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
