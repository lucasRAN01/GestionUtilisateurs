using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Initialise la base de données au démarrage :
/// - Applique les migrations (crée les tables si nécessaire).
/// - Insère les rôles et permissions par défaut.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>Initialise la base de données.</summary>
    Task InitializeAsync();

    /// <summary>Vérifie si un compte administrateur existe déjà.</summary>
    Task<bool> HasAdminAccountAsync();
}

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IAppDbFactory _dbFactory;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseInitializer(IAppDbFactory dbFactory, IPasswordHasher passwordHasher)
    {
        _dbFactory = dbFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync()
    {
        await using var context = _dbFactory.Create();
        await context.Database.MigrateAsync();

        await SeedRolesAndPermissionsAsync(context);
    }

    public async Task<bool> HasAdminAccountAsync()
    {
        await using var context = _dbFactory.Create();
        return await context.Users
            .AnyAsync(u => u.Role != null && u.Role.Nom == "Administrateur");
    }

    private static async Task SeedRolesAndPermissionsAsync(Data.AppDbContext context)
    {
        if (!await context.Roles.AnyAsync())
        {
            var (admin, manager, user) = BuildRoles();

            context.Roles.AddRange(admin, manager, user);
            await context.SaveChangesAsync();
        }
    }

    private static (Role, Role, Role) BuildRoles()
    {
        var createUser = new Permission { Code = "users.create", Libelle = "Créer un utilisateur", Description = "Permet de créer un nouvel utilisateur." };
        var editUser = new Permission { Code = "users.edit", Libelle = "Modifier un utilisateur", Description = "Permet de modifier un utilisateur existant." };
        var deleteUser = new Permission { Code = "users.delete", Libelle = "Supprimer un utilisateur", Description = "Permet de supprimer un utilisateur." };
        var viewUsers = new Permission { Code = "users.view", Libelle = "Voir les utilisateurs", Description = "Permet de consulter la liste des utilisateurs." };
        var manageRoles = new Permission { Code = "roles.manage", Libelle = "Gérer les rôles", Description = "Permet d'attribuer des rôles aux utilisateurs." };
        var viewAudit = new Permission { Code = "audit.view", Libelle = "Voir l'historique", Description = "Permet de consulter le journal d'audit." };

        var admin = new Role
        {
            Nom = "Administrateur",
            Description = "Accès complet à toutes les fonctionnalités.",
            Permissions = { createUser, editUser, deleteUser, viewUsers, manageRoles, viewAudit }
        };

        var manager = new Role
        {
            Nom = "Gestionnaire",
            Description = "Gère les utilisateurs courants, accès limité à la configuration.",
            Permissions = { createUser, editUser, viewUsers }
        };

        var user = new Role
        {
            Nom = "Utilisateur",
            Description = "Accès en lecture seule.",
            Permissions = { viewUsers }
        };

        return (admin, manager, user);
    }
}
