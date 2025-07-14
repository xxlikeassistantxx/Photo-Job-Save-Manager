using Microsoft.Extensions.Logging;
using Photo_Job_Save_Manager.Services;
using Photo_Job_Save_Manager.ViewModels;
using Photo_Job_Save_Manager.Views;
using Photo_Job_Save_Manager.Converters;
using CommunityToolkit.Maui;

namespace Photo_Job_Save_Manager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>(provider => new App(provider.GetRequiredService<IAuthService>(), provider))
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<IAuthService, FirebaseAuthService>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<ForgotPasswordViewModel>();
            builder.Services.AddTransient<MainDashboardViewModel>();
            builder.Services.AddTransient<AccountViewModel>();
            builder.Services.AddSingleton<JobTypesViewModel>();
            builder.Services.AddSingleton<SavedJobsViewModel>();
            builder.Services.AddTransient<JobTypeParameterViewModel>();
            builder.Services.AddTransient<AddJobViewModel>();

            // Register Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ForgotPasswordPage>();
            builder.Services.AddTransient<MainDashboardPage>();
            builder.Services.AddTransient<JobDetailsPage>();

            // Register Converters
            builder.Services.AddSingleton<IValueConverter, BoolToColorConverter>();
            builder.Services.AddSingleton<IValueConverter, InverseBoolConverter>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
