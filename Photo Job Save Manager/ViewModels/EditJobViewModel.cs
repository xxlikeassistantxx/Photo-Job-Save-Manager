using System.Collections.ObjectModel;
using System.Windows.Input;
using Photo_Job_Save_Manager.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Media;
using System.Diagnostics;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class EditJobViewModel : AddJobViewModel
    {
        public string JobId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ICommand UpdateCommand { get; }

        public EditJobViewModel()
        {
            UpdateCommand = new Command(async () => await UpdateAsync());
        }

        public void LoadJob(Job job)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - Loading job: {job.JobTypeId}");
            
            JobId = job.Id;
            JobTypeName = job.JobTypeId;
            CreatedAt = job.CreatedAt;
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - JobTypeName set to: {JobTypeName}");
            
            // Load the job type and populate fields
            LoadJobType(job.JobTypeId);
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - FieldEntries count: {FieldEntries.Count}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - Available job types: {string.Join(", ", App.Current.Services.GetService<JobTypesViewModel>()?.JobTypes.Select(jt => jt.Name) ?? new List<string>())}");
            
            // Populate existing values
            foreach (var fieldEntry in FieldEntries)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - Processing field: {fieldEntry.Field.Name}, Type: {fieldEntry.Field.FieldType}");
                
                if (job.Data.TryGetValue(fieldEntry.Field.Name, out var value))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - Found value for {fieldEntry.Field.Name}: {value}");
                    
                    if (fieldEntry.Field.FieldType == JobFieldType.Photo && value is List<string> photoPaths)
                    {
                        fieldEntry.PhotoFilePaths.Clear();
                        foreach (var path in photoPaths)
                            fieldEntry.PhotoFilePaths.Add(path);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - Set {photoPaths.Count} photo paths");
                    }
                    else if (fieldEntry.Field.FieldType == JobFieldType.Dropdown)
                    {
                        fieldEntry.SelectedValue = value?.ToString();
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - Set dropdown value: {fieldEntry.SelectedValue}");
                    }
                    else
                    {
                        fieldEntry.Value = value?.ToString();
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - Set text value: {fieldEntry.Value}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - No value found for field: {fieldEntry.Field.Name}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobViewModel.LoadJob - LoadJob completed");
        }

        private async Task UpdateAsync()
        {
            // Build the updated Job object
            var job = new Photo_Job_Save_Manager.Models.Job
            {
                Id = JobId,
                JobTypeId = JobTypeName,
                UserId = "", // TODO: set to current user when auth is integrated
                CreatedAt = CreatedAt,
                Data = FieldEntries.ToDictionary(f => f.Field.Name, f =>
                {
                    if (f.Field.FieldType == JobFieldType.Photo)
                        return (object?)f.PhotoFilePaths.ToList();
                    else if (f.Field.FieldType == JobFieldType.Dropdown)
                        return (object?)f.SelectedValue ?? "";
                    else
                        return (object?)f.Value ?? "";
                })
            };
            
            // Send the updated job to listeners
            WeakReferenceMessenger.Default.Send(new JobUpdatedMessage(job));
            
            // Show confirmation
            var summary = string.Join("\n", FieldEntries.Select(f =>
            {
                if (f.Field.FieldType == JobFieldType.Photo)
                    return $"{f.Field.Name}: {(f.PhotoFilePaths.Count > 0 ? string.Join(", ", f.PhotoFilePaths.Select(System.IO.Path.GetFileName)) : "<none>")}";
                else if (f.Field.FieldType == JobFieldType.Dropdown)
                    return $"{f.Field.Name}: {f.SelectedValue ?? "<none>"}";
                else
                    return $"{f.Field.Name}: {f.Value ?? "<none>"}";
            }));
            await Application.Current.MainPage.DisplayAlert("Job Updated", summary, "OK");
            await Shell.Current.GoToAsync("..", true);
        }
    }

    public class JobUpdatedMessage
    {
        public JobUpdatedMessage(Job job) => Job = job;
        public Job Job { get; }
    }
} 