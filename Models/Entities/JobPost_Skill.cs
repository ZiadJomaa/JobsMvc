namespace JobsMvc.Models.Entities;

public class JobPost_Skill
{
    public int JobPostId { get; set; }
    public virtual JobPost JobPost { get; set; } = null!;

    public int SkillId { get; set; }
    public virtual Skill Skill { get; set; } = null!;
}