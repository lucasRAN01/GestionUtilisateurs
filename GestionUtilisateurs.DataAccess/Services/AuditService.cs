using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Interface du service de journal d'audit pour l'affichage de l'historique.
/// </summary>
public interface IAuditService
{
    /// <summary>Récupère l'historique complet des actions.</summary>
    Task<IReadOnlyList<AuditLog>> GetAllAsync();

    /// <summary>Récupère l'historique des actions d'un utilisateur.</summary>
    Task<IReadOnlyList<AuditLog>> GetForUserAsync(int userId);

    /// <summary>Recherche dans l'historique par texte.</summary>
    Task<IReadOnlyList<AuditLog>> SearchAsync(string? term);
}

public class AuditService : IAuditService
{
    private readonly IAppDbFactory _dbFactory;

    public AuditService(IAppDbFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync()
    {
        await using var context = _dbFactory.Create();
        return await context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Date)
            .Take(500)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> GetForUserAsync(int userId)
    {
        await using var context = _dbFactory.Create();
        return await context.AuditLogs
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Date)
            .Take(500)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AuditLog>> SearchAsync(string? term)
    {
        await using var context = _dbFactory.Create();
        var query = context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var likeTerm = $"%{term.Trim()}%";
            query = query.Where(a =>
                a.Description.Contains(term) ||
                a.TargetUsername != null && a.TargetUsername.Contains(term) ||
                a.User != null && EF.Functions.Like(a.User.Username, likeTerm));
        }

        return await query
            .OrderByDescending(a => a.Date)
            .Take(500)
            .ToListAsync();
    }
}