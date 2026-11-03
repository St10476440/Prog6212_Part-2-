namespace RaceDayModel.Models
{
    public class Venue
    {
        public string VenueID { get; set; } = Guid.NewGuid().ToString();
        public string VenueName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
    }

    public class Users
    {
        public string UserID { get; set; } = Guid.NewGuid().ToString();
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? EmergencyContact { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class Events
    {
        public string EventID { get; set; } = Guid.NewGuid().ToString();
        public string OrganiserID { get; set; } = string.Empty;
        public string VenueID { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateOnly EventDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string? BannerImageUrl { get; set; }
    }

    public class Categories
    {
        public string CategoryID { get; set; } = Guid.NewGuid().ToString();
        public string EventID { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int? MinimumAge { get; set; }
        public int? MaximumAge { get; set; }
        public decimal EntryFee { get; set; }
    }

    public class Enrolments
    {
        public string EnrolmentID { get; set; } = Guid.NewGuid().ToString();
        public string ParticipantID { get; set; } = string.Empty;
        public string CategoryID { get; set; } = string.Empty;
        public DateOnly RegistrationDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public string Status { get; set; } = "Pending";
    }

    public class Results
    {
        public string ResultsID { get; set; } = Guid.NewGuid().ToString();
        public string EnrolmentID { get; set; } = string.Empty;
        public TimeOnly FinishTime { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class Login
    { 
        public string Email { set; get; } = string.Empty;
        public string Password { set; get; } = string.Empty;
    }

    public class InsertEvent
    {
        public string EventID { get; set; } = string.Empty;
        public string OrganiserID { get; set; } = string.Empty;
        public string VenueID { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateOnly EventDate { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string? EmergencyContact { get; set; }
    }

    public class EnrolmentRequest
    {
        public string EventID { get; set; } = string.Empty;
        public string CategoryID { get; set; } = string.Empty;
    }
}
