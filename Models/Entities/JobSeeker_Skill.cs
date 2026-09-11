namespace JobsMvc.Models.Entities;

public class JobSeeker_Skill
{
    public string JobSeekerProfileId { get; set; } = string.Empty;
    public virtual JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    public int SkillId { get; set; }
    public virtual Skill Skill { get; set; } = null!;
}