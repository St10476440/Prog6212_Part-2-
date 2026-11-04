using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RaceDayData.Data;
using RaceDayModel.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RaceDayApp.Controllers;

[ApiController]
public class RaceDayController : ControllerBase
{
    private readonly RaceDayDataContext _context;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public RaceDayController(RaceDayDataContext context, IConfiguration configuration)
    {
        _context = context;
        _jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Configure Jwt:Key.");
        _jwtIssuer = configuration["Jwt:Issuer"] ?? "RaceDay.Api";
        _jwtAudience = configuration["Jwt:Audience"] ?? "RaceDay.Client";
    }

    // ==========================================
    // 1. AUTHENTICATION ENDPOINTS
    // ==========================================

    [HttpPost("api/auth/register")]
    public IActionResult Register([FromBody] RegisterUserRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (_context.Users.Any(u => u.Email.ToLower() == email))
            return Conflict("Email already registered.");

        // Construct the entity here so callers cannot choose a user ID.
        var user = new Users
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = request.Role,
            PhoneNumber = request.PhoneNumber.Trim()
        };
        user.Password = new PasswordHasher<Users>().HashPassword(user, request.Password);
        _context.Users.Add(user);
        _context.SaveChanges();

        return StatusCode(201, new
        {
            message = "User registered successfully",
            token = CreateJwtToken(user),
            userId = user.UserID,
            role = user.Role,
            user = PublicProfile(user)
        });
    }

    [HttpPost("api/auth/login")]
    public IActionResult Login([FromBody] SignInRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        var hasher = new PasswordHasher<Users>();
        PasswordVerificationResult verification;
        try
        {
            verification = hasher.VerifyHashedPassword(user, user.Password, request.Password);
        }
        catch (FormatException)
        {
            // Legacy plaintext values are not accepted as password hashes.
            verification = PasswordVerificationResult.Failed;
        }
        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid email or password.");

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.Password = hasher.HashPassword(user, request.Password);
            _context.SaveChanges();
        }
        return Ok(new
        {
            token = CreateJwtToken(user),
            userId = user.UserID,
            role = user.Role,
            user = PublicProfile(user)
        });
    }

    private static object PublicProfile(Users user) => new
    {
        user.UserID, user.FullName, user.Email, user.Role, user.PhoneNumber,
        user.DateOfBirth, user.EmergencyContact, user.ProfilePictureUrl
    };

    // ==========================================
    // 2. USER PROFILE ENDPOINTS
    // ==========================================

    [Authorize]
    [HttpGet("api/users/profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = _context.Users.Find(userId);
        if (user == null)
            return NotFound("User profile not found.");

        return Ok(PublicProfile(user));
    }

    [Authorize]
    [HttpPut("api/users/profile")]
    public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = _context.Users.Find(userId);
        if (user == null)
            return NotFound("User profile not found.");

        user.PhoneNumber = request.Phone;
        _context.SaveChanges();

        return Ok(PublicProfile(user));
    }

    // ==========================================
    // 3. EVENTS ENDPOINTS
    // ==========================================

    [HttpGet("api/events")]
    public IActionResult GetEvents([FromQuery] string? eventType, [FromQuery] DateOnly? date)
    {
        var query = _context.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => e.EventType == eventType);

        if (date.HasValue)
            query = query.Where(e => e.EventDate == date.Value);

        return Ok(query.ToList());
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events")]
    public IActionResult CreateEvent([FromBody] Events newEvent)
    {
        _context.Events.Add(newEvent);
        _context.SaveChanges();

        return StatusCode(201, newEvent);
    }

    [HttpGet("api/events/{id}")]
    public IActionResult GetEventById(string id)
    {
        var raceEvent = _context.Events.Find(id);
        if (raceEvent == null)
            return NotFound("Event not found.");

        var categories = _context.Categories.Where(c => c.EventID == id).ToList();

        return Ok(new { raceEvent, categories });
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{id}/categories")]
    public IActionResult AddCategory(string id, [FromBody] Categories newCategory)
    {
        newCategory.EventID = id;
        _context.Categories.Add(newCategory);
        _context.SaveChanges();

        return StatusCode(StatusCodes.Status201Created, newCategory);
    }

    [HttpGet("api/events/{id}/categories")]
    public IActionResult GetCategoriesForEvent(string id)
    {
        var categories = _context.Categories.Where(c => c.EventID == id).ToList();
        return Ok(categories);
    }

    // ==========================================
    // 4. ENROLMENT ENDPOINTS
    // ==========================================

    [Authorize(Roles = "Participant")]
    [HttpPost("api/enrolments")]
    public IActionResult Enrol([FromBody] EnrolmentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        bool alreadyEnrolled = _context.Enrolments.Any(e => e.ParticipantID == userId);
        if (alreadyEnrolled)
            return Conflict("Already entered in this event.");

        var enrolment = new Enrolments
        {
            ParticipantID = userId,
            CategoryID = request.CategoryID,
        };

        _context.Enrolments.Add(enrolment);
        _context.SaveChanges();

        return StatusCode(StatusCodes.Status201Created, enrolment);
    }

    [Authorize(Roles = "Participant")]
    [HttpGet("api/enrolments/my-entries")]
    public IActionResult GetMyEntries()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var myEnrolments = _context.Enrolments.Where(e => e.ParticipantID == userId).ToList();

        return Ok(myEnrolments);
    }

    // ==========================================
    // 5. RESULTS ENDPOINTS
    // ==========================================

    [Authorize(Roles = "Organiser")]
    [HttpPost("api/results")]
    public IActionResult RecordResult([FromBody] RaceDayModel.Models.Results result)
    {
        if (result == null)
            return BadRequest("Result payload cannot be null.");

        _context.Results.Add(result);
        _context.SaveChanges();

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("api/results/event/{eventId}")]
    public IActionResult GetLeaderboard(string enrolmentId)
    {
        var enrolmentIds = _context.Enrolments
            .Where(e => e.EnrolmentID == enrolmentId)
            .Select(e => e.EnrolmentID)
            .ToList();

        var leaderboard = _context.Results
            .Where(r => enrolmentIds.Contains(r.EnrolmentID))
            .OrderBy(r => r.Position)
            .ToList();

        return Ok(leaderboard);
    }

    [Authorize(Roles = "Participant")]
    [HttpGet("api/results/my-history")]
    public IActionResult GetMyHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var myEnrolmentIds = _context.Enrolments
            .Where(e => e.ParticipantID == userId)
            .Select(e => e.EnrolmentID)
            .ToList();

        var history = _context.Results
            .Where(r => myEnrolmentIds.Contains(r.EnrolmentID))
            .ToList();

        return Ok(history);
    }

    // ==========================================
    // TOKEN GENERATOR HELPER
    // ==========================================

    private string CreateJwtToken(Users user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Email, user.Email)
            },
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
