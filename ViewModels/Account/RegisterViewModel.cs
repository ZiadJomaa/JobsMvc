using System.ComponentModel.DataAnnotations;
using JobsMvc.Models.Enums;

namespace JobsMvc.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "full name or company name is required")]
    [Display(Name = "Full Name / Company Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "email address is required")]
    [EmailAddress(ErrorMessage = "invalid email address")]

    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "password must be at least 6 characters long")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "the passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "please select an account type")]
    public UserType UserType { get; set; } = UserType.JobSeeker;
}