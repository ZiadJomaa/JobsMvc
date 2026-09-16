using JobsMvc.Models.Enums;

namespace JobsMvc.ViewModel
{
  public class CustomRegisterViewModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!; 
    public UserType SelectedUserType { get; set; }
}

    }

