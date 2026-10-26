using Microsoft.Extensions.Logging;
using AsistenteIA.Services;
using AsistenteIA.ViewModels;
using AsistenteIA.Views;

namespace AsistenteIA
{
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
    		builder.Services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri("https://gen.pollinations.ai/v1/"),
    Timeout = TimeSpan.FromSeconds(60)
});

            builder.Services.AddSingleton<ApiKeyService>();
            builder.Services.AddSingleton<AiService>();
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ConnectivityService>();
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ChatPage>();

#endif

            return builder.Build();
        }
    }
}
