using Photo_Job_Save_Manager.Services;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        public ICommand LogoutCommand { get; }
        public ICommand TestFirebaseConnectivityCommand { get; }

        public AccountViewModel(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
            Title = "Account";
            LogoutCommand = new Command(async () => await LogoutAsync());
            TestFirebaseConnectivityCommand = new Command(async () => await TestFirebaseConnectivityAsync());
        }

        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            Application.Current.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async Task TestFirebaseConnectivityAsync()
        {
            try
            {
                var connectivityTest = new FirebaseConnectivityTest(_configuration);
                var result = await connectivityTest.TestFirebaseConnectivityAsync();
                
                var message = $"Firebase Connectivity Test Results:\n\n" +
                             $"Configuration: {(result.ConfigurationTest.IsSuccess ? "✅" : "❌")} {result.ConfigurationTest.Details}\n\n" +
                             $"Auth API: {(result.AuthApiTest.IsSuccess ? "✅" : "❌")} {result.AuthApiTest.Details}\n\n" +
                             $"Database: {(result.DatabaseTest.IsSuccess ? "✅" : "❌")} {result.DatabaseTest.Details}\n\n" +
                             $"Network: {(result.NetworkTest.IsSuccess ? "✅" : "❌")} {result.NetworkTest.Details}\n\n" +
                             $"Overall: {(result.IsOverallSuccess ? "✅ All tests passed!" : "❌ Some tests failed")}";
                
                if (!result.IsOverallSuccess)
                {
                    message += $"\n\nErrors:\n";
                    if (!string.IsNullOrEmpty(result.ConfigurationTest.ErrorMessage))
                        message += $"Configuration: {result.ConfigurationTest.ErrorMessage}\n";
                    if (!string.IsNullOrEmpty(result.AuthApiTest.ErrorMessage))
                        message += $"Auth API: {result.AuthApiTest.ErrorMessage}\n";
                    if (!string.IsNullOrEmpty(result.DatabaseTest.ErrorMessage))
                        message += $"Database: {result.DatabaseTest.ErrorMessage}\n";
                    if (!string.IsNullOrEmpty(result.NetworkTest.ErrorMessage))
                        message += $"Network: {result.NetworkTest.ErrorMessage}\n";
                }
                
                await Application.Current.MainPage.DisplayAlert("Firebase Connectivity Test", message, "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to run connectivity test: {ex.Message}", "OK");
            }
        }
    }
} 