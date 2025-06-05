using Microsoft.Extensions.Logging;
using daytoday.Services;

namespace daytoday
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            Preferences.Remove("jwt_token");
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7157"); 
            });
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ProjectService>();
            builder.Services.AddSingleton<CalendarEventService>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
