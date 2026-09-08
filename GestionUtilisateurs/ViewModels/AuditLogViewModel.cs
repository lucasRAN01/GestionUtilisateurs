using System.Collections.ObjectModel;
using GestionUtilisateurs.DataAccess.Models;
using GestionUtilisateurs.DataAccess.Services;

namespace GestionUtilisateurs.ViewModels;

public class AuditLogViewModel : BaseViewModel
{
    private readonly IAuditService _auditService;

    private string _searchText = string.Empty;

    public AuditLogViewModel(IAuditService auditService)
    {
        _auditService = auditService;
        Title = "Journal d'audit";

        Items = new ObservableCollection<AuditLogItem>();
        SearchCommand = new AsyncRelayCommand(LoadItemsAsync);
    }

    public ObservableCollection<AuditLogItem> Items { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public AsyncRelayCommand SearchCommand { get; }

    public async Task OnAppearingAsync()
    {
        if (SearchCommand.CanExecute(null))
        {
            await LoadItemsAsync();
        }
    }

    private async Task LoadItemsAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ClearMessages();

        try
        {
            IReadOnlyList<AuditLog> logs;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                logs = await _auditService.GetAllAsync();
            }
            else
            {
                logs = await _auditService.SearchAsync(SearchText);
            }

            Items.Clear();
            foreach (var log in logs)
            {
                Items.Add(new AuditLogItem(log));
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement du journal : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public static string MapActionToLabel(AuditActionType actionType)
    {
        return actionType switch
        {
            AuditActionType.Connexion => "Connexion",
            AuditActionType.EchecConnexion => "Échec connexion",
            AuditActionType.Deconnexion => "Déconnexion",
            AuditActionType.Creation => "Création",
            AuditActionType.Modification => "Modification",
            AuditActionType.Suppression => "Suppression",
            AuditActionType.Activation => "Activation",
            AuditActionType.ResetMotDePasse => "Réinit. mot de passe",
            AuditActionType.AttributionRole => "Attribution rôle",
            _ => actionType.ToString(),
        };
    }
}

public class AuditLogItem
{
    public AuditLogItem(AuditLog log)
    {
        DateFormatted = log.Date.ToString("dd/MM/yyyy HH:mm");
        ActionLabel = AuditLogViewModel.MapActionToLabel(log.ActionType);
        Description = log.Description;
        ActorName = log.User?.Username ?? "Système";
        Target = log.TargetUsername;
        HasTarget = !string.IsNullOrEmpty(Target);
    }

    public string DateFormatted { get; }
    public string ActionLabel { get; }
    public string Description { get; }
    public string ActorName { get; }
    public string? Target { get; }
    public bool HasTarget { get; }
}
