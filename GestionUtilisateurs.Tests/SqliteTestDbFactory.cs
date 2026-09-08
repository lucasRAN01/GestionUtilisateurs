using GestionUtilisateurs.DataAccess.Data;
using GestionUtilisateurs.DataAccess.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GestionUtilisateurs.Tests;

/// <summary>
/// Fabrique de contexte pour les tests, basée sur une base SQLite en mémoire.
/// La connexion est partagée pour toute la durée des tests.
/// </summary>
public class SqliteTestDbFactory : IAppDbFactory
{
    private readonly SqliteConnection _connection;

    public SqliteTestDbFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}