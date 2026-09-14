using JobsMvc.Models.Enums;

namespace JobsMvc.Models.Entities;

public class JobPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JobType JobType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public DateTime? ClosingDate { get; set; }
    public bool IsApproved { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CityId { get; set; }
    public virtual City City { get; set; } = null!;

    public string CompanyId { get; set; } = string.Empty;
    public virtual CompanyProfile Company { get; set; } = null!;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<JobPost_Skill> RequiredSkills { get; set; } = new List<JobPost_Skill>();
    public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}