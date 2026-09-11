using Microsoft.AspNetCore.Identity;
using JobsMvc.Models.Enums;

namespace JobsMvc.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public UserType UserType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
    public virtual JobSeekerProfile? JobSeekerProfile { get; set; }
    public virtual CompanyProfile? CompanyProfile { get; set; }
}