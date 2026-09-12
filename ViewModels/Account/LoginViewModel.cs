using System.ComponentModel.DataAnnotations;

namespace JobsMvc.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "email address is required")]
    [EmailAddress(ErrorMessage = "invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}