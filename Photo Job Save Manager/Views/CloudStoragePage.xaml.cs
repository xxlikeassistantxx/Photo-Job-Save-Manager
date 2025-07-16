using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class CloudStoragePage : ContentPage
    {
        public CloudStoragePage() : this(App.Current.Services.GetService<CloudStorageViewModel>()) { }

        public CloudStoragePage(CloudStorageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] CloudStoragePage constructed. BindingContext: {BindingContext?.GetType().Name ?? "null"}");
        }
    }
} 