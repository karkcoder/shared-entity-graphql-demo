using Microsoft.EntityFrameworkCore;
using SharedEntityGraphQL.Models;

namespace SharedEntityGraphQL.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<State> States { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure State entity
        modelBuilder.Entity<State>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<State>()
            .Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<State>()
            .Property(s => s.Abbreviation)
            .IsRequired()
            .HasMaxLength(2);

        modelBuilder.Entity<State>()
            .HasIndex(s => s.Name)
            .IsUnique();

        modelBuilder.Entity<State>()
            .HasIndex(s => s.Abbreviation)
            .IsUnique();
    }
}
