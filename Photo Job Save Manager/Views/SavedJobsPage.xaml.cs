using Photo_Job_Save_Manager.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Windows.Input;
using Photo_Job_Save_Manager.Models;
using System.Collections.Generic;

namespace Photo_Job_Save_Manager.Views
{
    public partial class SavedJobsPage : ContentPage
    {
        public SavedJobsPage() : this(App.Current.Services.GetService<SavedJobsViewModel>()) { }

        public SavedJobsPage(SavedJobsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SavedJobsPage constructed. BindingContext: {BindingContext?.GetType().Name ?? "null"}");
        }

        private async void OnJobSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Photo_Job_Save_Manager.Models.Job job)
            {
                // Navigate to JobDetailsPage, passing the job ID (relative route for correct back navigation)
                await Shell.Current.GoToAsync($"JobDetailsPage?jobId={job.Id}");
            }
            ((CollectionView)sender).SelectedItem = null;
        }

        private void OnJobNameSearchChanged(object sender, TextChangedEventArgs e)
        {
            // This is handled automatically by the binding to JobNameSearchText in the ViewModel
            // The TextChanged event fires and updates the binding, which triggers filtering
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Job name search changed to: {e.NewTextValue}");
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is SavedJobsViewModel.FieldSearchTermViewModel fieldVm && BindingContext is SavedJobsViewModel vm)
            {
                vm.SetFieldSearchTerm(fieldVm.FieldName, entry.Text);
            }
        }

        public ICommand EditJobCommand => new Command<Photo_Job_Save_Manager.Models.Job>(async (job) =>
        {
            if (job != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobCommand - Navigating to EditJobPage with jobId: {job.Id}");
                await Shell.Current.GoToAsync("EditJobPage", new Dictionary<string, object>
                {
                    { "jobId", job.Id }
                });
            }
        });

        public ICommand UploadToCloudCommand => new Command<Photo_Job_Save_Manager.Models.Job>(async (job) =>
        {
            if (job != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] UploadToCloudCommand - Uploading job: {job.JobName}");
                    
                    // Get the CloudStorageViewModel from the service provider
                    var cloudStorageViewModel = App.Current.Services.GetService<CloudStorageViewModel>();
                    if (cloudStorageViewModel != null)
                    {
                        await cloudStorageViewModel.UploadJobFromSavedJobs(job);
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            "Cloud storage service is not available.",
                            "OK");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] UploadToCloudCommand error: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert(
                        "Upload Failed",
                        $"Could not upload job to cloud: {ex.Message}",
                        "OK");
                }
            }
        });

        public ICommand RemoveJobCommand => new Command<Job>(async (job) =>
        {
            if (job == null) return;
            // 3-step confirmation
            for (int i = 1; i <= 3; i++)
            {
                var confirm = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Delete",
                    $"Are you sure you want to delete this job? ({i}/3)",
                    i < 3 ? "Next" : "Delete",
                    "Cancel");
                if (!confirm) return;
            }
            // TODO: Move job to recycle bin (to be implemented)
            if (BindingContext is SavedJobsViewModel vm)
            {
                vm.MoveJobToRecycleBin(job);
            }
        });
    }
} 