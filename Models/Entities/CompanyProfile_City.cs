namespace JobsMvc.Models.Entities;

public class CompanyProfile_City
{
    public string CompanyProfileId { get; set; } = string.Empty;
    public virtual CompanyProfile CompanyProfile { get; set; } = null!;

    public int CityId { get; set; }
    public virtual City City { get; set; } = null!;
}