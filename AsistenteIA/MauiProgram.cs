using Microsoft.Extensions.Logging;
using AsistenteIA.Services;
using AsistenteIA.ViewModels;
using AsistenteIA.Views;

namespace AsistenteIA;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // HttpClient reutilizable para la API de IA
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("https://gen.pollinations.ai/v1/"),
            Timeout = TimeSpan.FromSeconds(60)
        });

        // Servicios
        builder.Services.AddSingleton<ApiKeyService>();
        builder.Services.AddSingleton<AiService>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<ConnectivityService>();

        // ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<DetailViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Páginas
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}