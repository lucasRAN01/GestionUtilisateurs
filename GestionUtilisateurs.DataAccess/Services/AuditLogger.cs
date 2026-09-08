using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Journalise les actions importantes dans la table AuditLogs.
/// </summary>
public interface IAuditLogger
{
    /// <summary>Enregistre une action dans le journal d'audit.</summary>
    Task LogAsync(AuditActionType action, string description, int? actorUserId = null, string? targetUsername = null, string? details = null);
}

public class AuditLogger : IAuditLogger
{
    private readonly IAppDbFactory _dbFactory;

    public AuditLogger(IAppDbFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task LogAsync(AuditActionType action, string description, int? actorUserId = null, string? targetUsername = null, string? details = null)
    {
        try
        {
            await using var context = _dbFactory.Create();
            context.AuditLogs.Add(new AuditLog
            {
                Date = DateTime.Now,
                UserId = actorUserId,
                ActionType = action,
                Description = description,
                TargetUsername = targetUsername,
                Details = details
            });

            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // L'enregistrement d'audit ne doit jamais bloquer l'action principale.
        }
    }
}
