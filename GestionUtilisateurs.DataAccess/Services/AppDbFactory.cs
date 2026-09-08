using GestionUtilisateurs.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Fabrique du DbContext pour l'application MAUI.
/// Utilise un chemin de base de données spécifique à la plateforme.
/// </summary>
public class AppDbFactory : IAppDbFactory
{
    private readonly string _databasePath;

    public AppDbFactory(string databasePath)
    {
        _databasePath = databasePath;
    }

    public AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        return new AppDbContext(options);
    }
}
