using IsArama.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Job> Jobs { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Source> Sources { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Job
        modelBuilder.Entity<Job>(e =>
        {
            e.HasIndex(j => j.Hash).IsUnique();
            e.HasIndex(j => j.PublishedAt);                          // ORDER BY PublishedAt DESC
            e.HasIndex(j => j.City);                                 // WHERE City = ?
            e.HasIndex(j => j.JobType);                              // WHERE JobType = ?
            e.HasIndex(j => j.SourceId);                             // WHERE SourceId = ?
            e.HasIndex(j => j.Title);                                // WHERE Title LIKE / StartsWith
            e.HasIndex(j => new { j.SourceId, j.PublishedAt });      // kaynak + tarih sıralaması
            e.HasIndex(j => new { j.City, j.PublishedAt });          // şehir + tarih sıralaması
            e.Property(j => j.Title).HasMaxLength(300);
            e.Property(j => j.City).HasMaxLength(100);
            e.Property(j => j.JobType).HasMaxLength(50);
            e.Property(j => j.OriginalUrl).HasMaxLength(1000);
            e.Property(j => j.Hash).HasMaxLength(64);

            e.HasOne(j => j.Company)
             .WithMany(c => c.Jobs)
             .HasForeignKey(j => j.CompanyId);

            e.HasOne(j => j.Source)
             .WithMany(s => s.Jobs)
             .HasForeignKey(j => j.SourceId);
        });

        // Company
        modelBuilder.Entity<Company>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(200);
            e.HasIndex(c => c.Name);
        });

        // Source
        modelBuilder.Entity<Source>(e =>
        {
            e.Property(s => s.Name).HasMaxLength(100);
            e.Property(s => s.BaseUrl).HasMaxLength(300);
        });

        // Feedback
        modelBuilder.Entity<Feedback>(e =>
        {
            e.Property(f => f.Subject).HasMaxLength(100);
            e.Property(f => f.Name).HasMaxLength(100);
            e.Property(f => f.Email).HasMaxLength(200);
            e.Property(f => f.Platform).HasMaxLength(20);
            e.HasIndex(f => f.CreatedAt);
            e.HasIndex(f => f.IsRead);
        });

        // Seed Data
        modelBuilder.Entity<Source>().HasData(
            new Source { Id = 1,  Name = "Kariyer.net",   BaseUrl = "https://www.kariyer.net",    IsActive = true },
            new Source { Id = 2,  Name = "Eleman.net",    BaseUrl = "https://www.eleman.net",     IsActive = true },
            new Source { Id = 3,  Name = "Yenibiris.com", BaseUrl = "https://www.yenibiris.com",  IsActive = true },
            new Source { Id = 4,  Name = "Secretcv.com",  BaseUrl = "https://www.secretcv.com",   IsActive = true },
            new Source { Id = 7,  Name = "Memurlar.net",  BaseUrl = "https://ilan.memurlar.net",  IsActive = true },
            new Source { Id = 11, Name = "İşbul.net",     BaseUrl = "https://www.isbul.net",      IsActive = true },
            new Source { Id = 12, Name = "LinkedIn TR",   BaseUrl = "https://www.linkedin.com",   IsActive = true }
        );
    }
}
