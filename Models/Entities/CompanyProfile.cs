namespace JobsMvc.Models.Entities;

public class CompanyProfile
{
    // Shared PK with ApplicationUser
    public string Id { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? LogoPath { get; set; }
    public bool IsApproved { get; set; } = false;

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
    public virtual ICollection<CompanyProfile_City> OperatingCities { get; set; } = new List<CompanyProfile_City>();
}