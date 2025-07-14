using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnEmailCompleted(object sender, EventArgs e)
        {
            PasswordEntry.Focus();
        }

        private void OnPasswordCompleted(object sender, EventArgs e)
        {
            if (BindingContext is LoginViewModel viewModel)
            {
                viewModel.LoginCommand.Execute(null);
            }
        }
    }
} 