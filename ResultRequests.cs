using System.ComponentModel.DataAnnotations;

namespace RaceDayModel.Models;

public sealed class RecordResultRequest
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
