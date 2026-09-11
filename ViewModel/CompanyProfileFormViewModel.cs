using JobsMvc.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace JobsMvc.ViewModel
{
    public class CompanyProfileFormViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required, StringLength(150)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Industry { get; set; }

        [Display(Name = "Logo Path / URL")]
        public string? LogoPath { get; set; }

        public bool IsApproved { get; set; }

        [Display(Name = "Operating Cities")]
        public List<int> SelectedCityIds { get; set; } = new();

        public List<City> AllCities { get; set; } = new();
    }
}

