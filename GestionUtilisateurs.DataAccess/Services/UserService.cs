using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Service de gestion des utilisateurs : CRUD, activation, rôles et réinitialisation.
/// </summary>
public class UserService : IUserService
{
    private readonly IAppDbFactory _dbFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public UserService(IAppDbFactory dbFactory, IPasswordHasher passwordHasher, IAuditLogger auditLogger)
    {
        _dbFactory = dbFactory;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        await using var context = _dbFactory.Create();
        return await context.Users
            .Include(u => u.Role)
            .OrderBy(u => u.Prenom)
            .ThenBy(u => u.Nom)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var context = _dbFactory.Create();
        return await context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserOperationResult> CreateAsync(User user, string password, int actorUserId)
    {
        if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(password))
            return Fail("Le nom d'utilisateur et le mot de passe sont obligatoires.");

        if (await IsUsernameTakenAsync(user.Username))
            return Fail("Ce nom d'utilisateur est déjà utilisé.");

        if (await IsEmailTakenAsync(user.Email))
            return Fail("Cette adresse e-mail est déjà utilisée.");

        user.PasswordHash = _passwordHasher.Hash(password);
        user.DateCreation = DateTime.Now;
        user.Status = UserStatus.Actif;

        await using var context = _dbFactory.Create();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await _auditLogger.LogAsync(AuditActionType.Creation,
            $"Création de l'utilisateur {user.Username}", actorUserId, user.Username);

        return Success("Utilisateur créé avec succès.");
    }

    public async Task<UserOperationResult> UpdateAsync(User user, int actorUserId)
    {
        if (await IsUsernameTakenAsync(user.Username, user.Id))
            return Fail("Ce nom d'utilisateur est déjà utilisé.");

        if (await IsEmailTakenAsync(user.Email, user.Id))
            return Fail("Cette adresse e-mail est déjà utilisée.");

        var originalUsername = user.Username;

        await using var context = _dbFactory.Create();
        var existing = await context.Users.FindAsync(user.Id);
        if (existing == null)
            return Fail("Utilisateur introuvable.");

        existing.Nom = user.Nom;
        existing.Prenom = user.Prenom;
        existing.Email = user.Email;
        existing.Telephone = user.Telephone;
        existing.Username = user.Username;
        existing.Notes = user.Notes;

        await context.SaveChangesAsync();

        await _auditLogger.LogAsync(AuditActionType.Modification,
            $"Modification de l'utilisateur {originalUsername}", actorUserId, originalUsername);

        return Success("Utilisateur modifié avec succès.");
    }

    public async Task<UserOperationResult> DeleteAsync(int id, int actorUserId)
    {
        if (id == actorUserId)
            return Fail("Vous ne pouvez pas supprimer votre propre compte.");

        await using var context = _dbFactory.Create();
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return Fail("Utilisateur introuvable.");

        // Empêcher la suppression du dernier administrateur
        var adminRoleId = await GetRoleIdAsync(context, "Administrateur");
        if (user.RoleId == adminRoleId)
        {
            var activeAdmins = await context.Users
                .CountAsync(u => u.RoleId == adminRoleId && u.Status == UserStatus.Actif);
            if (activeAdmins <= 1)
                return Fail("Impossible de supprimer le dernier administrateur.");
        }

        var username = user.Username;
        context.Users.Remove(user);
        await context.SaveChangesAsync();

        await _auditLogger.LogAsync(AuditActionType.Suppression,
            $"Suppression de l'utilisateur {username}", actorUserId, username);

        return Success("Utilisateur supprimé avec succès.");
    }

    public async Task<UserOperationResult> ToggleStatusAsync(int id, int actorUserId)
    {
        await using var context = _dbFactory.Create();
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return Fail("Utilisateur introuvable.");

        if (id == actorUserId)
            return Fail("Vous ne pouvez pas désactiver votre propre compte.");

        // Empêcher la désactivation du dernier administrateur actif
        if (user.RoleId == (await GetRoleIdAsync(context, "Administrateur")) && user.Status == UserStatus.Actif)
        {
            var activeAdmins = await context.Users
                .CountAsync(u => u.RoleId == user.RoleId && u.Status == UserStatus.Actif);
            if (activeAdmins <= 1)
                return Fail("Impossible de désactiver le dernier administrateur actif.");
        }

        user.Status = user.Status == UserStatus.Actif ? UserStatus.Inactif : UserStatus.Actif;
        await context.SaveChangesAsync();

        await _auditLogger.LogAsync(AuditActionType.Activation,
            $"Compte {user.Username} {(user.Status == UserStatus.Actif ? "activé" : "désactivé")}",
            actorUserId, user.Username);

        return Success($"Compte {(user.Status == UserStatus.Actif ? "activé" : "désactivé")}.");
    }

    public async Task<UserOperationResult> ResetPasswordAsync(int id, string newPassword, int actorUserId)
    {
        await using var context = _dbFactory.Create();
        var user = await context.Users.FindAsync(id);
        if (user == null)
            return Fail("Utilisateur introuvable.");

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        await context.SaveChangesAsync();

        await _auditLogger.LogAsync(AuditActionType.ResetMotDePasse,
            $"Réinitialisation du mot de passe de {user.Username}", actorUserId, user.Username);

        return Success("Mot de passe réinitialisé avec succès.");
    }

    public async Task<UserOperationResult> AssignRoleAsync(int userId, int roleId, int actorUserId)
    {
        await using var context = _dbFactory.Create();
        var user = await context.Users.FindAsync(userId);
        if (user == null)
            return Fail("Utilisateur introuvable.");

        var role = await context.Roles.FindAsync(roleId);
        if (role == null)
            return Fail("Rôle introuvable.");

        // Empêcher la rétrogradation du dernier administrateur
        var adminRoleId = await GetRoleIdAsync(context, "Administrateur");
        if (user.RoleId == adminRoleId && roleId != adminRoleId)
        {
            var activeAdmins = await context.Users.CountAsync(u => u.RoleId == adminRoleId && u.Status == UserStatus.Actif);
            if (activeAdmins <= 1)
                return Fail("Impossible de retirer le rôle d'administrateur au dernier administrateur.");
        }

        user.RoleId = roleId;
        await context.SaveChangesAsync();

        await _auditLogger.LogAsync(AuditActionType.AttributionRole,
            $"Rôle '{role.Nom}' attribué à {user.Username}", actorUserId, user.Username);

        return Success($"Rôle '{role.Nom}' attribué.");
    }

    public Task<bool> IsUsernameTakenAsync(string username, int? excludeId = null)
    {
        return CheckUnique(x => x.Username == username, excludeId);
    }

    public Task<bool> IsEmailTakenAsync(string email, int? excludeId = null)
    {
        return CheckUnique(x => x.Email == email, excludeId);
    }

    private async Task<bool> CheckUnique(System.Linq.Expressions.Expression<Func<User, bool>> predicate, int? excludeId)
    {
        await using var context = _dbFactory.Create();
        var query = context.Users.Where(predicate);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    private async Task<int?> GetRoleIdAsync(Data.AppDbContext context, string roleName)
    {
        var role = await context.Roles.FirstOrDefaultAsync(r => r.Nom == roleName);
        return role?.Id;
    }

    private static UserOperationResult Success(string message) => new() { Success = true, Message = message };
    private static UserOperationResult Fail(string message) => new() { Success = false, Message = message };
}
