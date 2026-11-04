using System.ComponentModel.DataAnnotations;

namespace RaceDayModel.Models;

public sealed class CreateEventRequest
{
    [Required, StringLength(120)]
    public string EventName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateOnly EventDate { get; set; }

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.1", "500")]
    public decimal DistanceKm { get; set; }

    [Required, StringLength(120)]
    public string VenueName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Province { get; set; } = string.Empty;

    [Url, StringLength(500)]
    public string? BannerImageUrl { get; set; }
}

public sealed class CreateCategoryRequest
{
    [Required, StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Distance { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = "South Africa";

    [Range(1, 120)]
    public int? MinimumAge { get; set; }

    [Range(1, 120)]
    public int? MaximumAge { get; set; }

    [Range(typeof(decimal), "0", "100000")]
    public decimal EntryFee { get; set; }
}
