using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Service d'authentification locale avec verrouillage après échecs répétés.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAppDbFactory _dbFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogger _auditLogger;

    public int MaxFailedAttempts { get; } = 5;
    public int LockoutSeconds { get; } = 300;

    private User? _currentUser;

    public int? CurrentUserId => _currentUser?.Id;
    public string? CurrentUsername => _currentUser?.Username;
    public string? CurrentRole => _currentUser?.Role?.Nom;
    public bool IsAuthenticated => _currentUser != null;

    public AuthService(IAppDbFactory dbFactory, IPasswordHasher passwordHasher, IAuditLogger auditLogger)
    {
        _dbFactory = dbFactory;
        _passwordHasher = passwordHasher;
        _auditLogger = auditLogger;
    }

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        var result = new LoginResult();

        await using var context = _dbFactory.Create();

        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            // Temps de réponse volontairement équivalent pour éviter l'énumération des comptes.
            _passwordHasher.Verify(password, new Pbkdf2PasswordHasher().Hash("placeholder"));
            result.Message = "Nom d'utilisateur ou mot de passe incorrect.";
            return result;
        }

        // Vérifier le verrouillage
        if (user.IsLocked)
        {
            var remaining = Math.Max(0, (int)Math.Ceiling((user.LockoutEnd!.Value - DateTime.Now).TotalSeconds));
            result.IsLockedOut = true;
            result.Message = $"Compte verrouillé. Réessayez dans {remaining} secondes.";
            return result;
        }

        // Vérifier le statut du compte
        if (user.Status == UserStatus.Inactif)
        {
            result.Message = "Ce compte est désactivé. Contactez l'administrateur.";
            return result;
        }

        // Vérifier le mot de passe
        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            var remaining = MaxFailedAttempts - user.FailedLoginAttempts;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEnd = DateTime.Now.AddSeconds(LockoutSeconds);
                user.FailedLoginAttempts = 0;
                result.IsLockedOut = true;
                result.Message = $"Compte verrouillé pendant {LockoutSeconds / 60} minutes après {MaxFailedAttempts} échecs.";

                await _auditLogger.LogAsync(AuditActionType.EchecConnexion,
                    "Compte verrouillé après échecs répétés", user.Id, user.Username, $"MaxAttempts={MaxFailedAttempts}");
            }
            else
            {
                result.RemainingAttempts = remaining;
                result.Message = $"Nom d'utilisateur ou mot de passe incorrect. ({remaining} tentative(s) restante(s))";

                await _auditLogger.LogAsync(AuditActionType.EchecConnexion,
                    "Échec de connexion", user.Id, user.Username);
            }

            await context.SaveChangesAsync();
            return result;
        }

        // Connexion réussie
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.DerniereConnexion = DateTime.Now;
        await context.SaveChangesAsync();

        _currentUser = user;
        result.Success = true;
        result.User = user;

        await _auditLogger.LogAsync(AuditActionType.Connexion,
            $"Connexion réussie de {user.Username}", user.Id, user.Username);

        return result;
    }

    public async Task LogoutAsync()
    {
        if (_currentUser != null)
        {
            await _auditLogger.LogAsync(AuditActionType.Deconnexion,
                $"Déconnexion de {_currentUser.Username}", _currentUser.Id, _currentUser.Username);
        }

        _currentUser = null;
        await Task.CompletedTask;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser == null)
            return null;

        await using var context = _dbFactory.Create();
        return await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.Id);
    }
}
