using System.Collections.ObjectModel;
using GestionUtilisateurs.DataAccess.Models;
using GestionUtilisateurs.DataAccess.Services;
using GestionUtilisateurs.Services;

namespace GestionUtilisateurs.ViewModels;

public class UserListItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public UserListItem(User user)
    {
        Id = user.Id;
        FullName = $"{user.Prenom} {user.Nom}";
        Email = user.Email;
        Username = user.Username;
        RoleName = user.Role?.Nom ?? "";
        IsActive = user.Status == UserStatus.Actif;
        StatusLabel = IsActive ? "Actif" : "Inactif";
    }
}

public class UserListViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    private List<UserListItem> _allUsers = new();

    public UserListViewModel(
        IUserService userService,
        IAuthService authService,
        INavigationService navigationService)
    {
        _userService = userService;
        _authService = authService;
        _navigationService = navigationService;

        Title = "Liste des utilisateurs";

        RefreshCommand = new AsyncRelayCommand(ExecuteRefreshAsync);
        SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync);
        AddUserCommand = new AsyncRelayCommand(ExecuteAddUserAsync);
        EditUserCommand = new AsyncRelayCommand<int>(ExecuteEditUserAsync);
        ToggleStatusCommand = new AsyncRelayCommand<int>(ExecuteToggleStatusAsync);
        DeleteCommand = new AsyncRelayCommand<int>(ExecuteDeleteAsync);
    }

    public ObservableCollection<UserListItem> Users { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilter();
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand AddUserCommand { get; }
    public AsyncRelayCommand<int> EditUserCommand { get; }
    public AsyncRelayCommand<int> ToggleStatusCommand { get; }
    public AsyncRelayCommand<int> DeleteCommand { get; }

    public async Task OnAppearingAsync()
    {
        await ExecuteRefreshAsync();
    }

    private async Task ExecuteRefreshAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        ClearMessages();

        try
        {
            var users = await _userService.GetAllAsync();
            _allUsers = users.Select(u => new UserListItem(u)).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task ExecuteSearchAsync()
    {
        ApplyFilter();
        return Task.CompletedTask;
    }

    private Task ExecuteAddUserAsync()
    {
        return _navigationService.GoToUserDetailAsync(0);
    }

    private async Task ExecuteEditUserAsync(int userId)
    {
        if (userId == 0) return;
        await _navigationService.GoToUserDetailAsync(userId);
    }

    private async Task ExecuteToggleStatusAsync(int userId)
    {
        if (userId == 0 || IsBusy) return;

        IsBusy = true;
        ClearMessages();

        try
        {
            var actorId = _authService.CurrentUserId ?? 0;
            var result = await _userService.ToggleStatusAsync(userId, actorId);

            if (result.Success)
                await ExecuteRefreshAsync();
            else
                ShowError(result.Message ?? "Erreur lors du changement de statut.");
        }
        catch (Exception ex)
        {
            ShowError($"Erreur : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExecuteDeleteAsync(int userId)
    {
        if (userId == 0 || IsBusy) return;

        IsBusy = true;
        ClearMessages();

        try
        {
            var actorId = _authService.CurrentUserId ?? 0;
            var result = await _userService.DeleteAsync(userId, actorId);

            if (result.Success)
                await ExecuteRefreshAsync();
            else
                ShowError(result.Message ?? "Erreur lors de la suppression.");
        }
        catch (Exception ex)
        {
            ShowError($"Erreur : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        Users.Clear();

        var filtered = string.IsNullOrWhiteSpace(_searchText)
            ? _allUsers
            : _allUsers.Where(u =>
                ContainsInsensitive(u.FullName, _searchText) ||
                ContainsInsensitive(u.Email, _searchText) ||
                ContainsInsensitive(u.Username, _searchText) ||
                ContainsInsensitive(u.RoleName, _searchText))
            .ToList();

        foreach (var user in filtered)
            Users.Add(user);
    }

    private static bool ContainsInsensitive(string source, string filter)
    {
        return source.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}
