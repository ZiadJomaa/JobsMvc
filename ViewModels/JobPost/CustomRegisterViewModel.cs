using JobsMvc.Models.Enums;

namespace JobsMvc.ViewModel
{
    public class CustomRegisterViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType SelectedUserType { get; set; }
    }
}
