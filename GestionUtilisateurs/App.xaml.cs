using GestionUtilisateurs.Views;

namespace GestionUtilisateurs;

/// <summary>
/// Racine de l'application.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
