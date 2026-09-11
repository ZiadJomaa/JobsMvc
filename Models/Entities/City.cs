namespace JobsMvc.Models.Entities;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<JobSeekerProfile> JobSeekers { get; set; } = new List<JobSeekerProfile>();
    public virtual ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();
    public virtual ICollection<CompanyProfile_City> CompanyCities { get; set; } = new List<CompanyProfile_City>();
}