using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JobsMvc.ViewModels;

public class JobSeekerProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "full name is required")]
    public string FullName { get; set; } = string.Empty;

    public string? Title { get; set; }

    [Range(0, 50, ErrorMessage = "experience years must be a logical value")]
    public int? ExperienceYears { get; set; }

    public int? CityId { get; set; }

    public string? ExistingPhotoPath { get; set; }
    public IFormFile? PhotoFile { get; set; }

    //(IDs)
    public List<int> SelectedSkillIds { get; set; } = new();

 
    public IEnumerable<SelectListItem> CitiesList { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> SkillsList { get; set; } = Enumerable.Empty<SelectListItem>();
}