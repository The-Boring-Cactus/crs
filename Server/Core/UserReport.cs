namespace Server.Core;

public class UserReport
{
    public string ReportId { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConnectionKey { get; set; }
    public string SqlQuery { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public TimeSpan CacheAge { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan RefreshFrequency { get; set; } = TimeSpan.FromHours(1);
    public bool IsPublic { get; set; } = false;
    public List<string> SharedWithUsers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecuted { get; set; }
    public bool IsActive { get; set; } = true;
}