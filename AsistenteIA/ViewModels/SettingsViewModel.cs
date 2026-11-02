using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ApiKeyService _keyService;
    private readonly AiService _aiService;
    private readonly ConnectivityService _connectivity;

    [ObservableProperty] private string apiKeyInput = string.Empty;
    [ObservableProperty] private string keyStatus = "Clave registrada: No";
    [ObservableProperty] private string selectedModel;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    public SettingsViewModel(ApiKeyService keyService, AiService aiService,
                             ConnectivityService connectivity)
    {
        _keyService = keyService;
        _aiService = aiService;
        _connectivity = connectivity;
        selectedModel = Preferences.Get("model", "openai/gpt-5.4-nano");
    }

    // Solo indica si hay clave; nunca la muestra
    public async Task RefreshKeyStatusAsync()
    {
        try
        {
            string? key = await _keyService.GetAsync();
            KeyStatus = string.IsNullOrWhiteSpace(key)
                ? "Clave registrada: No"
                : "Clave registrada: Sí";
        }
        catch
        {
            KeyStatus = "Clave registrada: No";
        }
    }

    [RelayCommand]
    private async Task SaveKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKeyInput))
        {
            StatusMessage = "Escriba la clave antes de guardar.";
            return;
        }

        await _keyService.SaveAsync(ApiKeyInput);
        ApiKeyInput = string.Empty;   // limpia el campo, la clave no queda en pantalla
        await RefreshKeyStatusAsync();
        StatusMessage = "Clave guardada de forma segura.";
    }

    [RelayCommand]
    private async Task DeleteKeyAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Eliminar clave",
            "¿Seguro que desea eliminar la clave guardada?",
            "Eliminar", "Cancelar");

        if (!confirm) return;

        _keyService.Remove();
        await RefreshKeyStatusAsync();
        StatusMessage = "Clave eliminada.";
    }

    [RelayCommand]
    private void SaveModel()
    {
        string model = SelectedModel.Trim();
        if (string.IsNullOrEmpty(model))
        {
            StatusMessage = "Escriba el identificador del modelo.";
            return;
        }

        Preferences.Set("model", model);
        SelectedModel = model;
        StatusMessage = "Modelo guardado.";
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (IsBusy) return;

        if (!_connectivity.HasInternet())
        {
            StatusMessage = "Sin conexión a internet.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Probando conexión...";

        try
        {
            // Consulta breve, no se muestra la clave ni la respuesta completa
            await _aiService.AskAsync("Responde solo: ok", SelectedModel.Trim());
            StatusMessage = "Conexión exitosa.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Tiempo agotado. Intente de nuevo.";
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = ex.StatusCode is System.Net.HttpStatusCode.Unauthorized
                                or System.Net.HttpStatusCode.Forbidden
                ? "Error de autorización: revise su clave."
                : $"Error del servicio: {(int?)ex.StatusCode ?? 0}";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception)
        {
            StatusMessage = "Ocurrió un error inesperado.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}