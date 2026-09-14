using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JobsMvc.ViewModels;

public class AdminProfileViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? CurrentImagePath { get; set; }

    [Display(Name = "Profile Picture")]
    public IFormFile? ProfileImage { get; set; }
}