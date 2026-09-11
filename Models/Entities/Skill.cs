namespace JobsMvc.Models.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<JobSeeker_Skill> JobSeekerSkills { get; set; } = new List<JobSeeker_Skill>();
    public virtual ICollection<JobPost_Skill> JobPostSkills { get; set; } = new List<JobPost_Skill>();
}