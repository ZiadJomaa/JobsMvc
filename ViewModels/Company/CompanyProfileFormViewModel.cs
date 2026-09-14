using System.ComponentModel.DataAnnotations;
using JobsMvc.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace JobsMvc.ViewModels;

public class CompanyProfileFormViewModel
{
    public string Id { get; set; } = string.Empty;

[Required(ErrorMessage = "Company Name is required")]
    [StringLength(100, MinimumLength = 2,
    ErrorMessage = "Company Name must be between 2 and 100 characters")]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Industry cannot exceed 100 characters")]
    [Display(Name = "Industry")]
    public string? Industry { get; set; }

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [Display(Name = "Company Description")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Please enter a valid website URL")]
    [StringLength(500, ErrorMessage = "Website URL cannot exceed 500 characters")]
    [Display(Name = "Website URL")]
    public string? Website { get; set; }

    public string? LogoPath { get; set; }

    [Display(Name = "Company Logo")]
    public IFormFile? LogoFile { get; set; }

    public bool IsApproved { get; set; }

    public List<City> AllCities { get; set; } = new();

    public List<int> SelectedCityIds { get; set; } = new();

}
