using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Interface du service de gestion des rôles.
/// </summary>
public interface IRoleService
{
    /// <summary>Récupère tous les rôles.</summary>
    Task<IReadOnlyList<Role>> GetAllAsync();

    /// <summary>Récupère un rôle par son identifiant.</summary>
    Task<Role?> GetByIdAsync(int id);
}

public class RoleService : IRoleService
{
    private readonly IAppDbFactory _dbFactory;

    public RoleService(IAppDbFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync()
    {
        await using var context = _dbFactory.Create();
        return await context.Roles
            .Include(r => r.Permissions)
            .OrderBy(r => r.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        await using var context = _dbFactory.Create();
        return await context.Roles
            .Include(r => r.Permissions)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}