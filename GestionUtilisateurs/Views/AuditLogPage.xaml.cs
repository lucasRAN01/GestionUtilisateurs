using GestionUtilisateurs.ViewModels;

namespace GestionUtilisateurs.Views;

public partial class AuditLogPage : ContentPage
{
    private readonly AuditLogViewModel _viewModel;

    public AuditLogPage(AuditLogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearingAsync();
    }
}