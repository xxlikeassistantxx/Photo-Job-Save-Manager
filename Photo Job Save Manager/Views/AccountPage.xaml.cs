using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage(AccountViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public AccountPage() : this(((App)Application.Current).Services.GetService<AccountViewModel>()) { }
    }
} 