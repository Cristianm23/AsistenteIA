using System.Collections.ObjectModel;
using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private const int MaxPromptLength = 120;
    private readonly AiService _aiService;
    private readonly DatabaseService _db;
    private readonly ConnectivityService _connectivity;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private string prompt = string.Empty;
    [ObservableProperty] private string answer = string.Empty;
    public FormattedString FormattedAnswer { get; } = new();
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string selectedModel;
    [ObservableProperty] private PromptTemplate? selectedTemplate;

    public ObservableCollection<PromptTemplate> Templates { get; } =
        new(PromptTemplates.All);

    public ChatViewModel(AiService aiService, DatabaseService db,
                         ConnectivityService connectivity)
    {
        _aiService = aiService;
        _db = db;
        _connectivity = connectivity;
        // Modelo configurable, guardado en Preferences
        selectedModel = Preferences.Get("model", "openai/gpt-5.4-nano");
    }

    // Al elegir una plantilla, su texto pasa al prompt para poder editarlo
    partial void OnSelectedTemplateChanged(PromptTemplate? value)
    {
        if (value is not null)
            Prompt = value.Text;
    }

    partial void OnAnswerChanged(string value)
    {
        FormattedAnswer.Spans.Clear();
        AddFormattedAnswerSpans(value);
        OnPropertyChanged(nameof(FormattedAnswer));
    }

    private void AddFormattedAnswerSpans(string text)
    {
        int position = 0;

        while (position < text.Length)
        {
            int opening = text.IndexOf("**", position, StringComparison.Ordinal);
            if (opening < 0)
            {
                AddSpan(text[position..]);
                break;
            }

            if (opening > position)
                AddSpan(text[position..opening]);

            int closing = text.IndexOf("**", opening + 2, StringComparison.Ordinal);
            if (closing < 0)
            {
                AddSpan(text[opening..]);
                break;
            }

            FormattedAnswer.Spans.Add(new Span
            {
                Text = text[(opening + 2)..closing],
                FontAttributes = FontAttributes.Bold
            });

            position = closing + 2;
        }
    }

    private void AddSpan(string text)
    {
        if (!string.IsNullOrEmpty(text))
            FormattedAnswer.Spans.Add(new Span { Text = text });
    }

    [RelayCommand]
    private async Task AskAsync()
    {
        if (IsBusy) return;

        if (Prompt.Trim().Length > MaxPromptLength)
        {
            StatusMessage = $"La consulta no puede superar {MaxPromptLength} caracteres.";
            return;
        }

        if (!_connectivity.HasInternet())
        {
            StatusMessage = "Sin conexión a internet. Intente de nuevo más tarde.";
            return;
        }

        _cts = new CancellationTokenSource();
        IsBusy = true;
        StatusMessage = "Consultando el servicio...";

        try
        {
            // Releer por si el usuario cambió el modelo en Configuración
            SelectedModel = Preferences.Get("model", SelectedModel);

            Answer = await _aiService.AskAsync(Prompt, SelectedModel, _cts.Token);
            StatusMessage = "Respuesta recibida. Verifique la información.";
        }
        catch (OperationCanceledException)
        {
            // También cubre el tiempo agotado del HttpClient
            StatusMessage = "Consulta cancelada o tiempo agotado. Puede reintentar.";
        }
        catch (HttpRequestException ex)
        {
            StatusMessage = ex.StatusCode is System.Net.HttpStatusCode.Unauthorized
                                or System.Net.HttpStatusCode.Forbidden
                ? "Error de autorización: revise su clave en Configuración."
                : $"Error del servicio: {(int?)ex.StatusCode ?? 0}";
        }
        catch (ArgumentException ex)
        {
            StatusMessage = ex.Message;
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
            _cts?.Dispose();
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Answer))
        {
            StatusMessage = "No hay una respuesta para guardar.";
            return;
        }

        var record = new ConversationRecord
        {
            Prompt = Prompt.Trim(),
            Answer = Answer,
            CreatedAt = DateTime.UtcNow,
            Category = SelectedTemplate?.Category ?? "General",
            IsVerified = false
        };

        await _db.SaveAsync(record);
        StatusMessage = "Consulta guardada en el historial.";
    }
}