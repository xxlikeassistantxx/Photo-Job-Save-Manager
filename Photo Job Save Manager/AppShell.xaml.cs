using Photo_Job_Save_Manager.Views;

namespace Photo_Job_Save_Manager;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
        Routing.RegisterRoute(nameof(MainDashboardPage), typeof(MainDashboardPage));
        Routing.RegisterRoute(nameof(JobTypeParameterPage), typeof(JobTypeParameterPage));
        Routing.RegisterRoute(nameof(AddJobPage), typeof(AddJobPage));
        Routing.RegisterRoute("JobDetailsPage", typeof(Views.JobDetailsPage));
        Routing.RegisterRoute("PhotoViewerPage", typeof(Views.PhotoViewerPage));
        Routing.RegisterRoute("EditJobPage", typeof(Views.EditJobPage));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Only run once, when Shell is first shown
        if (Current.CurrentPage is ShellContent)
        {
            var app = Application.Current as App;
            var authService = app?._authService;
            if (authService != null)
            {
                var isLoggedIn = await authService.IsUserLoggedInAsync();
                if (!isLoggedIn)
                {
                    await Shell.Current.GoToAsync("//LoginPage");
                }
                else
                {
                    var user = await authService.GetCurrentUserAsync();
                    if (user != null && (DateTime.Now - user.CreatedAt).TotalDays > 365)
                    {
                        await authService.LogoutAsync();
                        await Shell.Current.GoToAsync("//LoginPage");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync("//MainDashboardPage");
                    }
                }
            }
        }
    }
}
