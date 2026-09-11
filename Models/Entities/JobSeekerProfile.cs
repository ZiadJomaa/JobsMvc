namespace JobsMvc.Models.Entities;

public class JobSeekerProfile
{
    // Shared PK with ApplicationUser
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? PhotoPath { get; set; }
    public int? ExperienceYears { get; set; }

    public int? CityId { get; set; }
    public virtual City? City { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<JobSeeker_Skill> Skills { get; set; } = new List<JobSeeker_Skill>();
    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}