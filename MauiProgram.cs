using Microsoft.Extensions.Logging;
using Tasker099.Services;
using Tasker099.ViewModel;

namespace Tasker099
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

            builder.Services.AddSingleton<TaskerService>();

            builder.Services.AddSingleton<TaskersViewModel>();
            builder.Services.AddTransient<TaskerDetailsViewModel>();
            builder.Services.AddTransient<TaskerAddViewModel>();

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<DetailsPage>();
            builder.Services.AddTransient<AddPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
