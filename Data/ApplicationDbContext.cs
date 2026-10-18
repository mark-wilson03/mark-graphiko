using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarkGraphiko.Models;

namespace MarkGraphiko.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<DesignPost> DesignPosts { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<FavoriteDesign> FavoriteDesigns { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Composite primary key for favorites
        builder.Entity<FavoriteDesign>()
            .HasKey(f => new { f.UserId, f.DesignPostId });

        builder.Entity<FavoriteDesign>()
            .HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FavoriteDesign>()
            .HasOne(f => f.DesignPost)
            .WithMany(d => d.FavoritedBy)
            .HasForeignKey(f => f.DesignPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // DesignPost relationship
        builder.Entity<DesignPost>()
            .HasOne(d => d.Category)
            .WithMany(c => c.DesignPosts)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message relationship
        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Performance indexes
        builder.Entity<DesignPost>()
            .HasIndex(d => d.CategoryId);

        builder.Entity<DesignPost>()
            .HasIndex(d => d.IsFeatured);

        builder.Entity<DesignPost>()
            .HasIndex(d => d.CreatedAt);

        builder.Entity<Message>()
            .HasIndex(m => m.SentAt);

        builder.Entity<Message>()
            .HasIndex(m => m.IsRead);
    }
}
