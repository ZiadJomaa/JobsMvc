using JobsMvc.Models.Entities;
using System.Collections.Generic;

namespace JobsMvc.ViewModels.Home
{
    public class HomeCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int JobCount { get; set; }
    }

    public class HomeViewModel
    {
        public List<HomeCategoryDto> Categories { get; set; } = new();
        public List<JobsMvc.Models.Entities.JobPost> NewestJobs { get; set; } = new(); public List<City> Cities { get; set; } = new();
    }
}