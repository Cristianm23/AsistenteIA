using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class HomeViewModel
{
    // Rutas absolutas ("//") para cambiar de pestaña en el TabBar
    [RelayCommand]
    private async Task GoToChatAsync() =>
        await Shell.Current.GoToAsync("//ChatPage");

    [RelayCommand]
    private async Task GoToHistoryAsync() =>
        await Shell.Current.GoToAsync("//HistoryPage");
}