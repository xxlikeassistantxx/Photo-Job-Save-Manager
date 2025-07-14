using Microsoft.Maui.Controls;
using System;

namespace Photo_Job_Save_Manager.Views
{
    [QueryProperty(nameof(FilePath), "filePath")]
    public partial class PhotoViewerPage : ContentPage
    {
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                if (!string.IsNullOrWhiteSpace(_filePath))
                {
                    FullImage.Source = ImageSource.FromFile(_filePath);
                }
            }
        }
        private string _filePath;

        public PhotoViewerPage()
        {
            InitializeComponent();
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..", true);
        }
    }
} 