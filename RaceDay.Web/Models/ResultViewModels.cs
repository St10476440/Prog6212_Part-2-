using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public sealed class EventEntryViewModel
{
    public EnrolmentDetailsDto Enrolment { get; set; } = new();
    public CategoryDetailsDto Category { get; set; } = new();
    public ParticipantDetailsDto Participant { get; set; } = new();
}

public sealed class ParticipantDetailsDto
{
    public string UserID { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class EventEntriesPageViewModel
{
    public string EventID { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public List<EventEntryViewModel> Entries { get; set; } = new();
}

public sealed class ResultFormViewModel
{
    [Required]
    public string EnrolmentID { get; set; } = string.Empty;
    [Required]
    public TimeOnly FinishTime { get; set; }
    [Required, StringLength(20)]
    public string Position { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string Status { get; set; } = "Finished";
}
