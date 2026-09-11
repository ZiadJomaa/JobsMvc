namespace JobsMvc.Models.Entities;

public class Resume
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string JobSeekerId { get; set; } = string.Empty;
    public virtual JobSeekerProfile JobSeeker { get; set; } = null!;
}