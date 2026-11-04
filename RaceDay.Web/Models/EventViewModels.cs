using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public sealed class EventFormViewModel
{
    [Required, StringLength(120)]
    public string EventName { get; set; } = string.Empty;
    [Required, StringLength(60)]
    public string EventType { get; set; } = "Road Race";
    [Required]
    public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;
    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.1", "500")]
    public decimal DistanceKm { get; set; } = 5;
    [Required, StringLength(120)]
    public string VenueName { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string Province { get; set; } = string.Empty;
    [Url, StringLength(500)]
    public string? BannerImageUrl { get; set; }
}

public sealed class EventListItemViewModel
{
    public string EventID { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string Description { get; set; } = string.Empty;
}

public sealed class EventDetailsEnvelope
{
    public EventDetailsDto? RaceEvent { get; set; }
    public VenueDetailsDto? Venue { get; set; }
}

public sealed class EventDetailsDto
{
    public string EventID { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string? BannerImageUrl { get; set; }
}

public sealed class VenueDetailsDto
{
    public string VenueName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
}
