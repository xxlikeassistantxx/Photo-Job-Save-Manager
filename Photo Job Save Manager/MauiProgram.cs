using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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

            // Create configuration manually with correct Firebase settings
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Firebase:ApiKey"] = "AIzaSyBfA8aG8FZQwddu7ikKta-PKqWyimW-uxQ",
                    ["Firebase:ProjectId"] = "photo-job-manager",
                    ["Firebase:AuthDomain"] = "photo-job-manager.firebaseapp.com",
                    ["Firebase:StorageBucket"] = "photo-job-manager.firebasestorage.app"
                })
                .Build();

            // Register Services
            builder.Services.AddSingleton<IConfiguration>(configuration);
            builder.Services.AddSingleton<IAuthService>(provider => 
                new FirebaseAuthService(provider.GetRequiredService<IConfiguration>()));

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<ForgotPasswordViewModel>();
            builder.Services.AddTransient<MainDashboardViewModel>();
            builder.Services.AddTransient<AccountViewModel>();
            builder.Services.AddSingleton<JobTypesViewModel>();
            builder.Services.AddSingleton<SavedJobsViewModel>(provider => 
                new SavedJobsViewModel(provider.GetRequiredService<JobTypesViewModel>()));
            builder.Services.AddSingleton<CloudStorageViewModel>(provider => 
                new CloudStorageViewModel(provider.GetRequiredService<IAuthService>(), provider.GetRequiredService<JobTypesViewModel>()));
            builder.Services.AddTransient<JobTypeParameterViewModel>();
            builder.Services.AddSingleton<AddJobViewModel>();

            // Register Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<ForgotPasswordPage>();
            builder.Services.AddTransient<MainDashboardPage>();
            builder.Services.AddTransient<JobDetailsPage>();
            builder.Services.AddTransient<CloudStoragePage>();

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
