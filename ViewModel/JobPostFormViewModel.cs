using JobsMvc.Models.Entities;
using JobsMvc.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace JobsMvc.ViewModel
{
    public class JobPostFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150)]
        [Display(Name = "Job Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Job Type")]
        public JobType JobType { get; set; }

        [Required]
        [Display(Name = "Experience Level")]
        public ExperienceLevel ExperienceLevel { get; set; }

        [Display(Name = "Min Salary")]
        public decimal? MinSalary { get; set; }

        [Display(Name = "Max Salary")]
        public decimal? MaxSalary { get; set; }

        [Display(Name = "Closing Date")]
        [DataType(DataType.Date)]
        public DateTime? ClosingDate { get; set; }

        [Required(ErrorMessage = "Please select a city")]
        [Display(Name = "City")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Required Skills")]
        public List<int> SelectedSkillIds { get; set; } = new();

        public SelectList? Cities { get; set; }
        public SelectList? Categories { get; set; }

        public List<Skill> AllSkills { get; set; } = new();
    }
}

