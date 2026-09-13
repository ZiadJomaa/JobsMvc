using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JobsMvc.ViewModels;

public class CompanyProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Company Name is required")]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Display(Name = "Company Description")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Please enter a valid website URL")]
    [Display(Name = "Website URL")]
    public string? Website { get; set; }

    [Display(Name = "Company Logo")]
    public string? ExistingLogoPath { get; set; }
    public IFormFile? LogoFile { get; set; }

    public bool IsApproved { get; set; }
}