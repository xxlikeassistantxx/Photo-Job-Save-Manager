using Photo_Job_Save_Manager.Models;
using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class JobTypeParameterPage : ContentPage
    {
        public JobTypeParameterPage()
        {
            InitializeComponent();
            BindingContext = App.Current.Services.GetService<JobTypeParameterViewModel>();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private void OnAddAllowedValueClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is JobTypeField field)
            {
                // Find the Entry in the same parent
                var parent = btn.Parent as Layout;
                if (parent != null)
                {
                    var entry = parent.Children.OfType<Entry>().FirstOrDefault();
                    if (entry != null && !string.IsNullOrWhiteSpace(entry.Text))
                    {
                        if (!field.AllowedValues.Contains(entry.Text))
                            field.AllowedValues.Add(entry.Text);
                        entry.Text = string.Empty;
                    }
                }
            }
        }

        private void OnRemoveAllowedValueClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string value)
            {
                // Find the JobTypeField in the BindingContext
                if (btn.BindingContext is JobTypeField field)
                {
                    if (field.AllowedValues.Contains(value))
                        field.AllowedValues.Remove(value);
                }
            }
        }
    }
} 