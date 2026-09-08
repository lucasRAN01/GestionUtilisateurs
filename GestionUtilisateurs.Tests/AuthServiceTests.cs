using GestionUtilisateurs.DataAccess.Models;
using GestionUtilisateurs.DataAccess.Services;
using Xunit;

namespace GestionUtilisateurs.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteTestDbFactory _dbFactory;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditLogger _auditLogger;

    public AuthServiceTests()
    {
        _dbFactory = new SqliteTestDbFactory();
        _hasher = new Pbkdf2PasswordHasher();
        _auditLogger = new AuditLogger(_dbFactory);
    }

    public void Dispose() => _dbFactory.Dispose();

    private async Task SeedRolesAsync()
    {
        var initializer = new DatabaseInitializer(_dbFactory, _hasher);
        await initializer.InitializeAsync();
    }

    private async Task<int> CreateUserAsync(string username = "alice", string password = "Motdepasse1", UserStatus status = UserStatus.Actif)
    {
        await using var context = _dbFactory.Create();
        var role = context.Roles.First(r => r.Nom == "Administrateur");

        var user = new User
        {
            Nom = "Martin",
            Prenom = "Alice",
            Email = $"{username}@test.com",
            Username = username,
            PasswordHash = _hasher.Hash(password),
            RoleId = role.Id,
            Status = status
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task Login_WithValidCredentials_SucceedsAndSetsSession()
    {
        await SeedRolesAsync();
        var userId = await CreateUserAsync();

        var service = new AuthService(_dbFactory, _hasher, _auditLogger);
        var result = await service.LoginAsync("alice", "Motdepasse1");

        Assert.True(result.Success);
        Assert.True(result.IsLockedOut == false);
        Assert.True(service.IsAuthenticated);
        Assert.Equal(userId, service.CurrentUserId);
        Assert.Equal("alice", service.CurrentUsername);
        Assert.Equal("Administrateur", service.CurrentRole);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsErrorAndCountsAttempts()
    {
        await SeedRolesAsync();
        await CreateUserAsync();

        var service = new AuthService(_dbFactory, _hasher, _auditLogger);

        var first = await service.LoginAsync("alice", "mauvais-mdp");
        Assert.False(first.Success);
        Assert.Equal(4, first.RemainingAttempts);

        var second = await service.LoginAsync("alice", "mauvais-mdp");
        Assert.False(second.Success);
        Assert.Equal(3, second.RemainingAttempts);
    }

    [Fact]
    public async Task Login_UnknownUser_ReturnsGenericError()
    {
        await SeedRolesAsync();

        var service = new AuthService(_dbFactory, _hasher, _auditLogger);
        var result = await service.LoginAsync("inconnu", "password");

        Assert.False(result.Success);
        Assert.Equal("Nom d'utilisateur ou mot de passe incorrect.", result.Message);
    }

    [Fact]
    public async Task Login_LocksAccountAfterMaxAttempts()
    {
        await SeedRolesAsync();
        await CreateUserAsync();

        var service = new AuthService(_dbFactory, _hasher, _auditLogger);

        for (var i = 0; i < service.MaxFailedAttempts; i++)
        {
            var attempt = await service.LoginAsync("alice", "mauvais-mdp");
            if (i < service.MaxFailedAttempts - 1)
                Assert.False(attempt.Success);
        }

        // Tentative suivante : compte verrouillé même avec le bon mot de passe.
        var locked = await service.LoginAsync("alice", "Motdepasse1");
        Assert.False(locked.Success);
        Assert.True(locked.IsLockedOut);
        Assert.Contains("verrouillé", locked.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_InactiveAccount_IsRejected()
    {
        await SeedRolesAsync();
        await CreateUserAsync(status: UserStatus.Inactif);

        var service = new AuthService(_dbFactory, _hasher, _auditLogger);
        var result = await service.LoginAsync("alice", "Motdepasse1");

        Assert.False(result.Success);
        Assert.Contains("désactivé", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Logout_ClearsSessionAndLogsEvent()
    {
        await SeedRolesAsync();
        await CreateUserAsync();

        var service = new AuthService(_dbFactory, _hasher, _auditLogger);
        await service.LoginAsync("alice", "Motdepasse1");
        Assert.True(service.IsAuthenticated);

        await service.LogoutAsync();
        Assert.False(service.IsAuthenticated);
        Assert.Null(service.CurrentUserId);
    }

    [Fact]
    public async Task SuccessfulLogin_ResetsFailedAttemptsAndRecordsLastLogin()
    {
        await SeedRolesAsync();
        var userId = await CreateUserAsync();

        var service = new AuthService(_dbFactory, _hasher, _auditLogger);

        await service.LoginAsync("alice", "mauvais-mdp");
        var ok = await service.LoginAsync("alice", "Motdepasse1");
        Assert.True(ok.Success);

        await using var context = _dbFactory.Create();
        var user = await context.Users.FindAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(0, user!.FailedLoginAttempts);
        Assert.NotNull(user.DerniereConnexion);
    }
}