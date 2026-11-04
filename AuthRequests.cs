using System.ComponentModel.DataAnnotations;

namespace RaceDayModel.Models;

// Only fields a new registrant is allowed to supply.
public class RegisterUserRequest
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    // Both roles can register in this coursework platform. No admin role is accepted.
    [Required, RegularExpression("^(Organiser|Participant)$",
        ErrorMessage = "Choose Organiser or Participant.")]
    public string Role { get; set; } = "Participant";

    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class SignInRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
