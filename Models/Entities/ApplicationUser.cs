using Microsoft.AspNetCore.Identity;
using JobsMvc.Models.Enums;

namespace JobsMvc.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public UserType UserType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? FullName { get; set; }

    public string? ProfileImagePath { get; set; }

    public virtual JobSeekerProfile? JobSeekerProfile { get; set; }
    public virtual CompanyProfile? CompanyProfile { get; set; }
}