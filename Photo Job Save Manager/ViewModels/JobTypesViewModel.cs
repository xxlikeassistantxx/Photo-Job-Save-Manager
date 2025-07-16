using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Photo_Job_Save_Manager.Models;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class JobTypesViewModel : BaseViewModel
    {
        public ObservableCollection<JobType> JobTypes { get; } = new();
        public ICommand AddJobTypeCommand { get; }
        public ICommand DeleteJobTypeCommand { get; }

        public JobTypesViewModel()
        {
            Title = "Job Types";
            LoadJobTypesFromStorage();
            AddJobTypeCommand = new Command(async () => await GoToAddJobTypePageAsync());
            DeleteJobTypeCommand = new Command<JobType>(async (jobType) => await DeleteJobTypeAsync(jobType));
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<JobTypeCreatedMessage>(this, (r, m) =>
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesViewModel: Received JobTypeCreatedMessage for '{m.JobType.Name}'");
                JobTypes.Add(m.JobType);
                SaveJobTypesToStorage();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesViewModel: JobTypes count after adding: {JobTypes.Count}");
            });
        }

        private async Task GoToAddJobTypePageAsync()
        {
            var unique = Guid.NewGuid().ToString();
            await Shell.Current.GoToAsync($"{nameof(Views.JobTypeParameterPage)}?new={unique}");
        }

        private async Task DeleteJobTypeAsync(JobType jobType)
        {
            if (jobType == null) return;

            // Step 1: First confirmation
            var firstConfirm = await Application.Current.Windows[0].Page.DisplayAlert(
                "Delete Job Type",
                $"Are you sure you want to delete the job type '{jobType.Name}'?",
                "Yes, Delete",
                "Cancel");

            if (!firstConfirm) return;

            // Step 2: Second confirmation
            var secondConfirm = await Application.Current.Windows[0].Page.DisplayAlert(
                "Confirm Deletion",
                $"This will permanently delete '{jobType.Name}' and all its field configurations. This action cannot be undone.",
                "I Understand, Delete",
                "Cancel");

            if (!secondConfirm) return;

            // Step 3: Final confirmation
            var finalConfirm = await Application.Current.Windows[0].Page.DisplayAlert(
                "Final Warning",
                $"You are about to permanently delete '{jobType.Name}'. This will affect any saved jobs using this job type. Are you absolutely sure?",
                "Yes, Delete Permanently",
                "Cancel");

            if (!finalConfirm) return;

            // Delete the job type
            JobTypes.Remove(jobType);
            SaveJobTypesToStorage();

            await Application.Current.Windows[0].Page.DisplayAlert(
                "Job Type Deleted",
                $"The job type '{jobType.Name}' has been permanently deleted.",
                "OK");
        }

        private void SaveJobTypesToStorage()
        {
            try
            {
                var json = JsonSerializer.Serialize(JobTypes);
                Preferences.Set("job_types", json);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesViewModel: Saved {JobTypes.Count} job types to storage");
            }
            catch (Exception ex) 
            { 
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesViewModel: Error saving job types: {ex.Message}");
            }
        }

        private void LoadJobTypesFromStorage()
        {
            try
            {
                var json = Preferences.Get("job_types", null);
                if (!string.IsNullOrEmpty(json))
                {
                    var types = JsonSerializer.Deserialize<List<JobType>>(json);
                    if (types != null)
                    {
                        JobTypes.Clear();
                        foreach (var jt in types)
                        {
                            // Ensure backward compatibility: add Name field if it doesn't exist
                            var nameField = jt.Fields.FirstOrDefault(f => f.Name == "Name");
                            if (nameField == null)
                            {
                                // Add Name field as the first field
                                var newNameField = new JobTypeField
                                {
                                    Name = "Name",
                                    IsRequired = true,
                                    FieldType = JobFieldType.Text
                                };
                                jt.Fields.Insert(0, newNameField);
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Added Name field to existing job type: {jt.Name}");
                            }
                            else
                            {
                                // Ensure existing Name field is required
                                nameField.IsRequired = true;
                            }
                            
                            JobTypes.Add(jt);
                        }
                        
                        // Save back to storage with updated job types
                        SaveJobTypesToStorage();
                        
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesViewModel: Loaded {JobTypes.Count} job types from storage: {string.Join(", ", JobTypes.Select(jt => jt.Name))}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] JobTypesViewModel: No job types found in storage");
                }
            }
            catch (Exception ex) 
            { 
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesViewModel: Error loading job types: {ex.Message}");
            }
        }
    }
} 