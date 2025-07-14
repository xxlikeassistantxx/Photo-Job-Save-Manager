using Photo_Job_Save_Manager.Services;

namespace Photo_Job_Save_Manager;

public partial class App : Application
{
    internal readonly IAuthService _authService;
    public static new App Current => (App)Application.Current;
    public IServiceProvider Services => _services;
    private static IServiceProvider _services;

    public App(IAuthService authService, IServiceProvider services)
    {
        InitializeComponent();
        _authService = authService;
        _services = services;
        MainPage = new AppShell();
        // Removed: CheckAuthenticationStatus();
    }
}