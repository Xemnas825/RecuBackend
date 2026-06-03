using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Models;

namespace RecuBackend.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<RollLog> RollLogs => Set<RollLog>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(60);
            e.Property(x => x.DisplayName).HasMaxLength(120);
            e.Property(x => x.PasswordHash).HasMaxLength(200);
            e.Property(x => x.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<Campaign>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120);
            e.Property(x => x.Setting).HasMaxLength(120);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => x.OwnerUserId);
            e.HasIndex(x => x.IsPublic);
        });

        modelBuilder.Entity<Character>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120);
            e.Property(x => x.Race).HasMaxLength(60);
            e.Property(x => x.CharacterClass).HasMaxLength(60);
            e.Property(x => x.Alignment).HasMaxLength(40);
            e.Property(x => x.Background).HasMaxLength(120);
            e.Property(x => x.Languages).HasMaxLength(300);
            e.Property(x => x.SkillProficienciesJson).HasMaxLength(2000);
            e.HasIndex(x => x.OwnerUserId);
            e.HasOne(x => x.Campaign)
                .WithMany(c => c.Characters)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RollLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Label).HasMaxLength(120);
            e.Property(x => x.DiceExpression).HasMaxLength(40);
            e.Property(x => x.RawResults).HasMaxLength(200);
            e.HasIndex(x => x.OwnerUserId);
            e.HasOne(x => x.Character)
                .WithMany(c => c.Rolls)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FileAttachment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FileName).HasMaxLength(255);
            e.Property(x => x.ContentType).HasMaxLength(120);
            e.Property(x => x.StoragePath).HasMaxLength(500);
            e.HasIndex(x => x.OwnerUserId);
            e.HasOne(x => x.Character)
                .WithMany(c => c.Attachments)
                .HasForeignKey(x => x.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
