using Microsoft.Maui.Controls;
using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class RecycleBinPage : ContentPage
    {
        public RecycleBinPage()
        {
            InitializeComponent();
            BindingContext = new RecycleBinViewModel();
        }
    }
} 