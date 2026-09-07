using GameSaveEditor.Services;
using Microsoft.Extensions.Logging;

namespace GameSaveEditor
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SQLitePCL.Batteries_V2.Init();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<MemoryService>();
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<BackupService>();
            builder.Services.AddSingleton<AppSettingsService>();
            builder.Services.AddSingleton<KeyVaultService>();
            builder.Services.AddSingleton<ProfileService>();
            builder.Services.AddSingleton<DatabaseProbeService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
