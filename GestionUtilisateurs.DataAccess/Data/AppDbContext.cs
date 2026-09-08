using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Data;

/// <summary>
/// Contexte Entity Framework Core de l'application.
/// Utilise SQLite comme base de données locale.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Table des utilisateurs.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Table des rôles.</summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>Table des permissions.</summary>
    public DbSet<Permission> Permissions => Set<Permission>();

    /// <summary>Table du journal d'audit.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==== Configuration de la table User ====
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Nom).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Prenom).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Telephone).HasMaxLength(30);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Notes).HasMaxLength(2000);

            // Relation User -> Role
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ==== Configuration de la table Role ====
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Nom).IsUnique();

            entity.Property(r => r.Nom).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Description).HasMaxLength(500);
        });

        // ==== Configuration de la table Permission ====
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Code).IsUnique();

            entity.Property(p => p.Code).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Libelle).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(500);
        });

        // ==== Relation N-N Role <-> Permission ====
        modelBuilder.Entity<Role>()
            .HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity(j => j.ToTable("RolePermissions"));

        // ==== Configuration de la table AuditLog ====
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Description).IsRequired().HasMaxLength(500);
            entity.Property(a => a.TargetUsername).HasMaxLength(100);
            entity.Property(a => a.Details).HasMaxLength(2000);

            entity.HasIndex(a => a.Date);
            entity.HasIndex(a => a.ActionType);

            // Relation AuditLog -> User
            entity.HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
