using JobsMvc.Models.Enums;

namespace JobsMvc.Models.Entities;

public class ApplicationStatusHistory
{
    public int Id { get; set; }

    public int JobApplicationId { get; set; }
    public virtual JobApplication JobApplication { get; set; } = null!;

    public ApplicationStatus Status { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}