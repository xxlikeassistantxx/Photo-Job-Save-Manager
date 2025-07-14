using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnUsernameCompleted(object sender, EventArgs e)
        {
            EmailEntry.Focus();
        }

        private void OnEmailCompleted(object sender, EventArgs e)
        {
            PasswordEntry.Focus();
        }

        private void OnPasswordCompleted(object sender, EventArgs e)
        {
            ConfirmPasswordEntry.Focus();
        }

        private void OnConfirmPasswordCompleted(object sender, EventArgs e)
        {
            if (BindingContext is RegisterViewModel viewModel)
            {
                viewModel.RegisterCommand.Execute(null);
            }
        }
    }
} 