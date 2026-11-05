using System.Net.Http.Headers;
using System.Net.Http.Json;
using AsistenteIA.Models;

namespace AsistenteIA.Services;

public class AiService
{
    private readonly HttpClient _client;
    private readonly ApiKeyService _keyService;

    public AiService(HttpClient client, ApiKeyService keyService)
    {
        _client = client;
        _keyService = keyService;
    }

    public async Task<string> AskAsync(
        string prompt, string model, CancellationToken token = default)
    {
        // 1. Validación del prompt (RF02)
        string cleanPrompt = prompt.Trim();
        if (cleanPrompt.Length < 5)
            throw new ArgumentException("La consulta es demasiado corta.");
        if (cleanPrompt.Length > 120)
            throw new ArgumentException("La consulta no puede superar 120 caracteres.");

        // 2. Clave desde SecureStorage
        string? apiKey = await _keyService.GetAsync();
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Configure la clave de la API.");

        // 3. Cuerpo JSON de la petición
        var requestBody = new ChatRequest
        {
            Model = model,
            Messages =
            [
                new ChatMessage
                {
                    Role = "system",
                    Content = "Eres un asistente académico. Responde de forma clara, " +
                              "indica incertidumbre y no inventes fuentes."
                },
                new ChatMessage { Role = "user", Content = cleanPrompt }
            ]
        };

        // 4. POST con autenticación Bearer (RF03)
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(requestBody);

        using HttpResponseMessage response = await _client.SendAsync(request, token);
        response.EnsureSuccessStatusCode();

        // 5. Deserializar y extraer el texto
        ChatResponse? result = await response.Content
            .ReadFromJsonAsync<ChatResponse>(cancellationToken: token);

        string? answer = result?.Choices.FirstOrDefault()?.Message.Content;
        return string.IsNullOrWhiteSpace(answer)
            ? "El servicio no devolvió contenido."
            : answer.Trim();
    }
}