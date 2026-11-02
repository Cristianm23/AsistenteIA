using AsistenteIA.ViewModels;

namespace AsistenteIA.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    // Actualiza "Clave registrada: Sí/No" al abrir la pestaña
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshKeyStatusAsync();
    }
}