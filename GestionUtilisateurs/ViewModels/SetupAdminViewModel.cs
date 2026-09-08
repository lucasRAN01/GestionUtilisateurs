using System.Text.RegularExpressions;
using GestionUtilisateurs.Services;
using GestionUtilisateurs.DataAccess.Services;
using GestionUtilisateurs.DataAccess.Models;

namespace GestionUtilisateurs.ViewModels;

public partial class SetupAdminViewModel : BaseViewModel
{
    private readonly IDatabaseInitializer _dbInitializer;
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private readonly IRoleService _roleService;

    private string _nom = string.Empty;
    private string _prenom = string.Empty;
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;

    public SetupAdminViewModel(
        IDatabaseInitializer dbInitializer,
        IUserService userService,
        INavigationService navigationService,
        IAuthService authService,
        IRoleService roleService)
    {
        _dbInitializer = dbInitializer;
        _userService = userService;
        _navigationService = navigationService;
        _authService = authService;
        _roleService = roleService;

        Title = "Configuration de l'administrateur";

        CreateAdminCommand = new AsyncRelayCommand(ExecuteCreateAdminAsync);
    }

    public string Nom
    {
        get => _nom;
        set => SetProperty(ref _nom, value);
    }

    public string Prenom
    {
        get => _prenom;
        set => SetProperty(ref _prenom, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public AsyncRelayCommand CreateAdminCommand { get; }

    public async Task OnAppearingAsync()
    {
        ClearMessages();

        await _dbInitializer.InitializeAsync();

        bool hasAdmin = await _dbInitializer.HasAdminAccountAsync();
        if (hasAdmin)
        {
            await _navigationService.GoToLoginAsync();
        }
    }

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private bool Validate()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(Nom))
        {
            ShowError("Le nom est requis.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Prenom))
        {
            ShowError("Le prénom est requis.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("L'email est requis.");
            return false;
        }

        if (!EmailRegex.IsMatch(Email))
        {
            ShowError("Le format de l'email est invalide.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("Le nom d'utilisateur est requis.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Le mot de passe est requis.");
            return false;
        }

        if (Password.Length < 6)
        {
            ShowError("Le mot de passe doit contenir au moins 6 caractères.");
            return false;
        }

        if (Password != ConfirmPassword)
        {
            ShowError("Les mots de passe ne correspondent pas.");
            return false;
        }

        return true;
    }

    private async Task ExecuteCreateAdminAsync()
    {
        if (!Validate())
            return;

        IsBusy = true;

        try
        {
            var roles = await _roleService.GetAllAsync();
            var adminRole = roles.FirstOrDefault(r => r.Nom == "Administrateur");

            if (adminRole is null)
            {
                ShowError("Le rôle administrateur est introuvable.");
                return;
            }

            var user = new User
            {
                Nom = Nom,
                Prenom = Prenom,
                Email = Email,
                Username = Username,
                RoleId = adminRole.Id,
                Status = UserStatus.Actif,
                DateCreation = DateTime.UtcNow
            };

            var result = await _userService.CreateAsync(user, Password, 0);

            if (result.Success)
            {
                ShowSuccess("Compte administrateur créé. Connectez-vous pour continuer.");
                await Task.Delay(1000);
                await _navigationService.GoToLoginAsync();
            }
            else
            {
                ShowError(result.Message ?? "Erreur lors de la création de l'administrateur.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Une erreur est survenue : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
