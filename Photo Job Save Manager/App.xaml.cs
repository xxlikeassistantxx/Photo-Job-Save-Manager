using Photo_Job_Save_Manager.Services;

namespace Photo_Job_Save_Manager;

public partial class App : Application
{
    internal readonly IAuthService _authService;
    public static new App Current => (App)Application.Current!;
    public IServiceProvider Services => _services;
    private static IServiceProvider _services = null!;

    public App(IAuthService authService, IServiceProvider services)
    {
        InitializeComponent();
        _authService = authService;
        _services = services;
        MainPage = new AppShell();
        
        // Run Firebase connectivity test on app startup
        _ = Task.Run(async () =>
        {
            try
            {
                var connectivityTest = services.GetRequiredService<FirebaseConnectivityTest>();
                var result = await connectivityTest.TestFirebaseConnectivityAsync();
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] App: Firebase connectivity test completed:");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] App: - Overall success: {result.IsOverallSuccess}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] App: - Configuration: {result.ConfigurationTest.IsSuccess} - {result.ConfigurationTest.Details}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] App: - Auth API: {result.AuthApiTest.IsSuccess} - {result.AuthApiTest.Details}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] App: - Database: {result.DatabaseTest.IsSuccess} - {result.DatabaseTest.Details}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] App: - Network: {result.NetworkTest.IsSuccess} - {result.NetworkTest.Details}");
                
                if (!result.IsOverallSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] App: ❌ Firebase connectivity issues detected!");
                    if (!string.IsNullOrEmpty(result.OverallError))
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] App: Overall error: {result.OverallError}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] App: ✅ All Firebase connectivity tests passed!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] App: Error running connectivity test: {ex.Message}");
            }
        });
    }
}