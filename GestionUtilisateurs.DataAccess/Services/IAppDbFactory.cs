using GestionUtilisateurs.DataAccess.Data;
using GestionUtilisateurs.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Fabrique du DbContext, résout le chemin de la base SQLite selon la plateforme.
/// Sépare la configuration du contexte pour rester testable.
/// </summary>
public interface IAppDbFactory
{
    /// <summary>Crée un nouveau DbContext.</summary>
    AppDbContext Create();
}
