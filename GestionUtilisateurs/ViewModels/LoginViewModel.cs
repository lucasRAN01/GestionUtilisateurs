using GestionUtilisateurs.Services;
using GestionUtilisateurs.DataAccess.Services;

namespace GestionUtilisateurs.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IDatabaseInitializer _dbInitializer;
    private readonly INavigationService _navigationService;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isPasswordVisible;

    public LoginViewModel(
        IAuthService authService,
        IDatabaseInitializer dbInitializer,
        INavigationService navigationService)
    {
        _authService = authService;
        _dbInitializer = dbInitializer;
        _navigationService = navigationService;

        Title = "Connexion";

        LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync);
        TogglePasswordCommand = new AsyncRelayCommand(ExecuteTogglePasswordAsync);
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

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => SetProperty(ref _isPasswordVisible, value);
    }

    public AsyncRelayCommand LoginCommand { get; }

    public AsyncRelayCommand TogglePasswordCommand { get; }

    public async Task OnAppearingAsync()
    {
        ClearMessages();
        Username = string.Empty;
        Password = string.Empty;
        IsPasswordVisible = false;

        bool hasAdmin = await _dbInitializer.HasAdminAccountAsync();
        if (!hasAdmin)
        {
            await _navigationService.GoToSetupAdminAsync();
        }
    }

    private async Task ExecuteLoginAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("Le nom d'utilisateur est requis.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("Le mot de passe est requis.");
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _authService.LoginAsync(Username, Password);

            if (result.Success)
            {
                await _navigationService.GoToDashboardAsync();
            }
            else
            {
                ShowError(result.Message ?? "Identifiants incorrects.");
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

    private Task ExecuteTogglePasswordAsync()
    {
        IsPasswordVisible = !IsPasswordVisible;
        return Task.CompletedTask;
    }
}
