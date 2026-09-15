using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using JobsMvc.Models.Entities;

namespace JobsMvc.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<JobPost> JobPosts => Set<JobPost>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories => Set<ApplicationStatusHistory>();
    public DbSet<CompanyProfile_City> CompanyProfileCities => Set<CompanyProfile_City>();
    public DbSet<JobSeeker_Skill> JobSeekerSkills => Set<JobSeeker_Skill>();
    public DbSet<JobPost_Skill> JobPostSkills => Set<JobPost_Skill>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. One-to-One: ApplicationUser & CompanyProfile
        builder.Entity<CompanyProfile>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasOne(c => c.User)
                  .WithOne(u => u.CompanyProfile)
                  .HasForeignKey<CompanyProfile>(c => c.Id)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 2. One-to-One: ApplicationUser & JobSeekerProfile
        builder.Entity<JobSeekerProfile>(entity =>
        {
            entity.HasKey(j => j.Id);
            entity.HasOne(j => j.User)
                  .WithOne(u => u.JobSeekerProfile)
                  .HasForeignKey<JobSeekerProfile>(j => j.Id)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(j => j.City)
                  .WithMany(c => c.JobSeekers)
                  .HasForeignKey(j => j.CityId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // 3. Composite Keys for Many-to-Many
        builder.Entity<CompanyProfile_City>()
            .HasKey(cc => new { cc.CompanyProfileId, cc.CityId });

        builder.Entity<JobSeeker_Skill>()
            .HasKey(js => new { js.JobSeekerProfileId, js.SkillId });

        builder.Entity<JobPost_Skill>()
            .HasKey(jps => new { jps.JobPostId, jps.SkillId });

        // 4. JobPost Configuration
        builder.Entity<JobPost>(entity =>
        {
            entity.Property(j => j.MinSalary).HasColumnType("decimal(18,2)");
            entity.Property(j => j.MaxSalary).HasColumnType("decimal(18,2)");

            entity.HasOne(j => j.Company)
                  .WithMany(c => c.JobPosts)
                  .HasForeignKey(j => j.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(j => j.City)
                  .WithMany(c => c.JobPosts)
                  .HasForeignKey(j => j.CityId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.Category)
                  .WithMany(c => c.JobPosts)
                  .HasForeignKey(j => j.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // 5. JobApplication Configuration
        builder.Entity<JobApplication>(entity =>
        {
            entity.Property(a => a.AiTermScore).HasColumnType("decimal(5,2)");

            entity.HasOne(a => a.JobPost)
                  .WithMany(p => p.Applications)
                  .HasForeignKey(a => a.JobPostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.JobSeeker)
                  .WithMany(s => s.JobApplications)
                  .HasForeignKey(a => a.JobSeekerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.HasKey(h => h.Id);

            entity.HasOne(h => h.JobApplication)
                  .WithMany()
                  .HasForeignKey(h => h.JobApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}