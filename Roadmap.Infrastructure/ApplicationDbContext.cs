using Microsoft.EntityFrameworkCore;
using Roadmap.Domain.Entities;

namespace Roadmap.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<ExpiredToken> ExpiredTokens { get; set; }
    
    public DbSet<Domain.Entities.Roadmap> Roadmaps { get; set; }
    
    public DbSet<PrivateAccess> PrivateAccesses { get; set; }
    
    public DbSet<Progress> Progresses { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Domain.Entities.Roadmap>()
            .HasOne(r => r.User)
            .WithMany(u => u.CreatedRoadmaps)
            .HasForeignKey(r => r.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Domain.Entities.Roadmap>()
            .Property(r => r.Content)
            .HasColumnType("jsonb");

        modelBuilder.Entity<PrivateAccess>()
            .HasOne(pa => pa.Roadmap)
            .WithMany(r => r.PrivateAccesses)
            .HasForeignKey(pa => pa.RoadmapId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PrivateAccess>()
            .HasOne(pa => pa.User)
            .WithMany(r => r.PrivateAccesses)
            .HasForeignKey(pa => pa.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Progress>()
            .HasOne(pa => pa.Roadmap)
            .WithMany(r => r.Progresses)
            .HasForeignKey(pa => pa.RoadmapId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Progress>()
            .HasOne(pa => pa.User)
            .WithMany(r => r.Progresses)
            .HasForeignKey(pa => pa.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.TokenString)
            .IsUnique();

        modelBuilder.Entity<ExpiredToken>()
            .HasIndex(t => t.TokenString)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}