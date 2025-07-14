using Photo_Job_Save_Manager.Services;
using System.Windows.Input;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        public ICommand LogoutCommand { get; }

        public AccountViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Account";
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
            Application.Current.MainPage = new AppShell();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
} 