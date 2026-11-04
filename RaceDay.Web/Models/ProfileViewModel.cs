using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public sealed class ProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    [StringLength(30)]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(120)]
    [Display(Name = "Emergency contact")]
    public string? EmergencyContact { get; set; }

    public string? ProfilePictureUrl { get; set; }
}
