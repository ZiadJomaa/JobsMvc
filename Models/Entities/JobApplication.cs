using JobsMvc.Models.Enums;

namespace JobsMvc.Models.Entities;

public class JobApplication
{
    public int Id { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public string? HrPrivateNote { get; set; }
    public decimal? AiTermScore { get; set; }

    public string JobSeekerId { get; set; } = string.Empty;
    public virtual JobSeekerProfile JobSeeker { get; set; } = null!;

    public int JobPostId { get; set; }
    public virtual JobPost JobPost { get; set; } = null!;
}