using GestionUtilisateurs.DataAccess.Models;
using GestionUtilisateurs.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionUtilisateurs.Tests;

public class UserServiceTests : IDisposable
{
    private readonly SqliteTestDbFactory _dbFactory;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditLogger _auditLogger;
    private readonly IUserService _service;

    private const int ActorId = 0;

    public UserServiceTests()
    {
        _dbFactory = new SqliteTestDbFactory();
        _hasher = new Pbkdf2PasswordHasher();
        _auditLogger = new AuditLogger(_dbFactory);
        _service = new UserService(_dbFactory, _hasher, _auditLogger);
    }

    public void Dispose() => _dbFactory.Dispose();

    private async Task SeedRolesAsync()
    {
        var initializer = new DatabaseInitializer(_dbFactory, _hasher);
        await initializer.InitializeAsync();
    }

    private async Task<int> CreateTestUserAsync(string username = "bob", string email = "bob@test.com", int actorUserId = 0)
    {
        await SeedRolesAsync();

        await using var context = _dbFactory.Create();
        var role = context.Roles.First(r => r.Nom == "Utilisateur");

        var user = new User
        {
            Nom = "Dupont",
            Prenom = "Bob",
            Email = email,
            Username = username,
            RoleId = role.Id,
            Status = UserStatus.Actif
        };

        var result = await _service.CreateAsync(user, "Motdepasse1", actorUserId);
        Assert.True(result.Success);

        await using var ctx2 = _dbFactory.Create();
        return (await ctx2.Users.FirstAsync(u => u.Username == username)).Id;
    }

    [Fact]
    public async Task Create_ValidUser_SucceedsAndHashesPassword()
    {
        await SeedRolesAsync();
        await using var context = _dbFactory.Create();

        var role = context.Roles.First(r => r.Nom == "Utilisateur");
        var user = new User
        {
            Nom = "Bernard",
            Prenom = "Julien",
            Email = "julien@test.com",
            Username = "julien",
            RoleId = role.Id,
            Status = UserStatus.Actif
        };

        var result = await _service.CreateAsync(user, "S3cret!", ActorId);

        Assert.True(result.Success);

        var saved = await context.Users.FirstAsync(u => u.Username == "julien");
        Assert.NotEqual("S3cret!", saved.PasswordHash);
        Assert.True(_hasher.Verify("S3cret!", saved.PasswordHash));
        Assert.NotEqual(default, saved.DateCreation);
    }

    [Fact]
    public async Task Create_DuplicateUsername_IsRejected()
    {
        await CreateTestUserAsync();

        await using var context = _dbFactory.Create();
        var role = context.Roles.First(r => r.Nom == "Utilisateur");

        var duplicate = new User
        {
            Nom = "Autre",
            Prenom = "Autre",
            Email = "autre@test.com",
            Username = "bob",
            RoleId = role.Id
        };

        var result = await _service.CreateAsync(duplicate, "Motdepasse1", ActorId);
        Assert.False(result.Success);
        Assert.Contains("déjà utilisé", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_DuplicateEmail_IsRejected()
    {
        await CreateTestUserAsync();

        await using var context = _dbFactory.Create();
        var role = context.Roles.First(r => r.Nom == "Utilisateur");

        var duplicate = new User
        {
            Nom = "Autre",
            Prenom = "Autre",
            Email = "bob@test.com",
            Username = "unique",
            RoleId = role.Id
        };

        var result = await _service.CreateAsync(duplicate, "Motdepasse1", ActorId);
        Assert.False(result.Success);
        Assert.Contains("e-mail", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAll_ReturnsUsersWithRoles()
    {
        await CreateTestUserAsync("user1", "user1@test.com");
        await CreateTestUserAsync("user2", "user2@test.com");

        var users = await _service.GetAllAsync();

        Assert.Equal(2, users.Count);
        Assert.All(users, u => Assert.Equal("Utilisateur", u.Role?.Nom));
    }

    [Fact]
    public async Task Update_ModifiesFields()
    {
        var id = await CreateTestUserAsync();

        var loaded = await _service.GetByIdAsync(id);
        Assert.NotNull(loaded);

        loaded!.Prenom = "Robert";
        loaded.Telephone = "0123456789";
        loaded.Notes = "Note de test";

        var result = await _service.UpdateAsync(loaded, ActorId);
        Assert.True(result.Success);

        var updated = await _service.GetByIdAsync(id);
        Assert.Equal("Robert", updated!.Prenom);
        Assert.Equal("0123456789", updated.Telephone);
        Assert.Equal("Note de test", updated.Notes);
    }

    [Fact]
    public async Task Update_DuplicateUsername_IsRejected()
    {
        await CreateTestUserAsync("alice", "alice@test.com");
        var id = await CreateTestUserAsync("bob", "bob@test.com");

        var loaded = await _service.GetByIdAsync(id);
        loaded!.Username = "alice";

        var result = await _service.UpdateAsync(loaded, ActorId);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Delete_RemovesUser()
    {
        var id = await CreateTestUserAsync();

        var result = await _service.DeleteAsync(id, ActorId);
        Assert.True(result.Success);

        var deleted = await _service.GetByIdAsync(id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Delete_Self_IsRejected()
    {
        var id = await CreateTestUserAsync();

        var result = await _service.DeleteAsync(id, id);
        Assert.False(result.Success);
        Assert.Contains("propre compte", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ToggleStatus_FlipsStatus()
    {
        var id = await CreateTestUserAsync();

        var result = await _service.ToggleStatusAsync(id, ActorId);
        Assert.True(result.Success);

        var user = await _service.GetByIdAsync(id);
        Assert.Equal(UserStatus.Inactif, user!.Status);

        await _service.ToggleStatusAsync(id, ActorId);
        user = await _service.GetByIdAsync(id);
        Assert.Equal(UserStatus.Actif, user!.Status);
    }

    [Fact]
    public async Task ToggleStatus_Self_IsRejected()
    {
        var id = await CreateTestUserAsync();

        var result = await _service.ToggleStatusAsync(id, id);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task ResetPassword_AllowsLoginWithNewPassword()
    {
        var id = await CreateTestUserAsync();

        var result = await _service.ResetPasswordAsync(id, "NouveauMdp1", ActorId);
        Assert.True(result.Success);

        var auth = new AuthService(_dbFactory, _hasher, _auditLogger);
        var login = await auth.LoginAsync("bob", "NouveauMdp1");

        Assert.True(login.Success);
    }

    [Fact]
    public async Task AssignRole_ChangesUserRole()
    {
        var id = await CreateTestUserAsync();

        await using var context = _dbFactory.Create();
        var adminRole = context.Roles.First(r => r.Nom == "Administrateur");

        var result = await _service.AssignRoleAsync(id, adminRole.Id, ActorId);
        Assert.True(result.Success);

        var user = await _service.GetByIdAsync(id);
        Assert.Equal("Administrateur", user!.Role!.Nom);
    }

    [Fact]
    public async Task AuditLogs_AreWrittenForActions()
    {
        var id = await CreateTestUserAsync();
        // Créer un deuxième utilisateur pour servir d'acteur (self-toggle est bloqué)
        var adminId = await CreateTestUserAsync("admin", "admin@test.com");

        await _service.UpdateAsync((await _service.GetByIdAsync(id))!, adminId);
        await _service.ToggleStatusAsync(id, adminId);
        await _service.ResetPasswordAsync(id, "AutreMdp1", adminId);

        var auditService = new AuditService(_dbFactory);
        var logs = await auditService.GetAllAsync();

        Assert.Contains(logs, l => l.ActionType == AuditActionType.Modification);
        Assert.Contains(logs, l => l.ActionType == AuditActionType.Activation);
        Assert.Contains(logs, l => l.ActionType == AuditActionType.ResetMotDePasse);
    }

    [Fact]
    public async Task AuditService_Search_FindsByTarget()
    {
        var id = await CreateTestUserAsync();
        var adminId = await CreateTestUserAsync("admin", "admin@test.com");

        await _service.UpdateAsync((await _service.GetByIdAsync(id))!, adminId);

        var auditService = new AuditService(_dbFactory);
        var logs = await auditService.SearchAsync("bob");

        Assert.NotEmpty(logs);
    }
}