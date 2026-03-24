using IsArama.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Job> Jobs { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Source> Sources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Job
        modelBuilder.Entity<Job>(e =>
        {
            e.HasIndex(j => j.Hash).IsUnique();
            e.Property(j => j.Title).HasMaxLength(300);
            e.Property(j => j.City).HasMaxLength(100);
            e.Property(j => j.JobType).HasMaxLength(50);
            e.Property(j => j.OriginalUrl).HasMaxLength(1000);
            e.Property(j => j.Hash).HasMaxLength(64);

            e.HasOne(j => j.Company)
             .WithMany(c => c.Jobs)
             .HasForeignKey(j => j.CompanyId);

            e.HasOne(j => j.Category)
             .WithMany(c => c.Jobs)
             .HasForeignKey(j => j.CategoryId);

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

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
        });

        // Source
        modelBuilder.Entity<Source>(e =>
        {
            e.Property(s => s.Name).HasMaxLength(100);
            e.Property(s => s.BaseUrl).HasMaxLength(300);
        });

        // Seed Data
        modelBuilder.Entity<Category>().HasData(
    // Teknoloji
    new Category { Id = 1, Name = "Yazılım Geliştirme" },
    new Category { Id = 2, Name = "Veri Bilimi & Yapay Zeka" },
    new Category { Id = 3, Name = "Siber Güvenlik" },
    new Category { Id = 4, Name = "Sistem & Ağ Yönetimi" },
    new Category { Id = 5, Name = "UI/UX & Tasarım" },
    new Category { Id = 6, Name = "DevOps & Cloud" },
    new Category { Id = 7, Name = "Oyun Geliştirme" },

    // Finans & Muhasebe
    new Category { Id = 8, Name = "Muhasebe" },
    new Category { Id = 9, Name = "Finans & Bankacılık" },
    new Category { Id = 10, Name = "Denetim & Vergi" },
    new Category { Id = 11, Name = "Sigortacılık" },

    // Satış & Pazarlama
    new Category { Id = 12, Name = "Satış & Satış Temsilcisi" },
    new Category { Id = 13, Name = "Dijital Pazarlama" },
    new Category { Id = 14, Name = "Marka & İletişim" },
    new Category { Id = 15, Name = "E-ticaret" },
    new Category { Id = 16, Name = "Sosyal Medya Yönetimi" },

    // Mühendislik
    new Category { Id = 17, Name = "Makine Mühendisliği" },
    new Category { Id = 18, Name = "Elektrik & Elektronik Mühendisliği" },
    new Category { Id = 19, Name = "İnşaat & İnşaat Mühendisliği" },
    new Category { Id = 20, Name = "Endüstri Mühendisliği" },
    new Category { Id = 21, Name = "Kimya Mühendisliği" },
    new Category { Id = 22, Name = "Biyomedikal Mühendisliği" },

    // İnsan Kaynakları & Yönetim
    new Category { Id = 23, Name = "İnsan Kaynakları" },
    new Category { Id = 24, Name = "İdari İşler & Sekreterlik" },
    new Category { Id = 25, Name = "Genel Müdürlük & Üst Yönetim" },

    // Sağlık
    new Category { Id = 26, Name = "Doktor & Uzman Hekim" },
    new Category { Id = 27, Name = "Hemşire & Sağlık Personeli" },
    new Category { Id = 28, Name = "Eczacılık" },
    new Category { Id = 29, Name = "Diş Hekimliği" },
    new Category { Id = 30, Name = "Psikoloji & Danışmanlık" },

    // Eğitim
    new Category { Id = 31, Name = "Öğretmenlik" },
    new Category { Id = 32, Name = "Akademik & Araştırma" },
    new Category { Id = 33, Name = "Özel Ders & Eğitmenlik" },
    new Category { Id = 34, Name = "Çocuk Gelişimi & Bakım" },

    // Lojistik & Üretim
    new Category { Id = 35, Name = "Lojistik & Tedarik Zinciri" },
    new Category { Id = 36, Name = "Depo & Ambar" },
    new Category { Id = 37, Name = "Şoför & Kurye" },
    new Category { Id = 38, Name = "Üretim & İmalat" },
    new Category { Id = 39, Name = "Kalite Kontrol" },

    // Müşteri Hizmetleri & Çağrı Merkezi
    new Category { Id = 40, Name = "Müşteri Hizmetleri" },
    new Category { Id = 41, Name = "Çağrı Merkezi" },
    new Category { Id = 42, Name = "Teknik Destek" },

    // Hukuk & Kamu
    new Category { Id = 43, Name = "Hukuk & Avukatlık" },
    new Category { Id = 44, Name = "Kamu & Devlet" },
    new Category { Id = 45, Name = "Askerlik & Güvenlik" },

    // Medya & Yaratıcı
    new Category { Id = 46, Name = "Gazetecilik & İçerik" },
    new Category { Id = 47, Name = "Grafik Tasarım" },
    new Category { Id = 48, Name = "Fotoğrafçılık & Video" },
    new Category { Id = 49, Name = "Mimarlık & İç Mimarlık" },

    // Turizm & Yiyecek
    new Category { Id = 50, Name = "Turizm & Otelcilik" },
    new Category { Id = 51, Name = "Aşçılık & Mutfak" },
    new Category { Id = 52, Name = "Garsonluk & Servis" },

    // Perakende & Moda
    new Category { Id = 53, Name = "Perakende & Mağazacılık" },
    new Category { Id = 54, Name = "Moda & Tekstil" },

    // Diğer
    new Category { Id = 55, Name = "Part-time & Yarı Zamanlı" },
    new Category { Id = 56, Name = "Staj & Yeni Mezun" },
    new Category { Id = 57, Name = "Uzaktan Çalışma" },
    new Category { Id = 58, Name = "Freelance" },
    new Category { Id = 59, Name = "Diğer" }
);


        modelBuilder.Entity<Source>().HasData(
            new Source { Id = 1, Name = "Kariyer.net", BaseUrl = "https://www.kariyer.net", IsActive = true },
            new Source { Id = 2, Name = "Eleman.net", BaseUrl = "https://www.eleman.net", IsActive = true },
            new Source { Id = 3, Name = "Yenibiris.com", BaseUrl = "https://www.yenibiris.com", IsActive = true },
            new Source { Id = 4, Name = "Secretcv.com", BaseUrl = "https://www.secretcv.com", IsActive = true },
            new Source { Id = 5, Name = "İŞKUR", BaseUrl = "https://www.iskur.gov.tr", IsActive = true },
            new Source { Id = 6, Name = "Sahibinden.com", BaseUrl = "https://www.sahibinden.com", IsActive = true },
            new Source { Id = 7, Name = "Memurlar.net", BaseUrl = "https://ilan.memurlar.net", IsActive = true },
            new Source { Id = 8, Name = "Indeed TR", BaseUrl = "https://tr.indeed.com", IsActive = true },
            new Source { Id = 9, Name = "Adzuna TR", BaseUrl = "https://www.adzuna.com.tr", IsActive = true },
            new Source { Id = 10, Name = "Jooble TR", BaseUrl = "https://jooble.org/tr", IsActive = true }
        );
    }
}
