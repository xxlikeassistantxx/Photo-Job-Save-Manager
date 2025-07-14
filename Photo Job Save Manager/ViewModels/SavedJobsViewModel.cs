using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Photo_Job_Save_Manager.Models;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Photo_Job_Save_Manager.ViewModels;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class SavedJobsViewModel : BaseViewModel
    {
        public ObservableCollection<Job> AllJobs { get; } = new();
        public ObservableCollection<JobType> JobTypes { get; }
        public ObservableCollection<Job> FilteredJobs { get; } = new();
        public ObservableCollection<Job> RecycleBinJobs { get; } = new();

        private JobType? _selectedJobType;
        public JobType? SelectedJobType
        {
            get => _selectedJobType;
            set
            {
                if (SetProperty(ref _selectedJobType, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectedJobType set: {value?.Name ?? "null"}");
                    UpdateFieldSearchTerms();
                    FilterJobs();
                }
            }
        }

        public string StatusFilterField { get; set; } = "Status"; // Field name to filter on (default 'Status')
        public string StatusFilter { get; set; } = string.Empty;
        public DateTime? DateCreatedStart { get; set; }
        public DateTime? DateCreatedEnd { get; set; }

        // Dictionary: field name -> search text
        public ObservableCollection<FieldSearchTermViewModel> FieldSearchTerms { get; } = new();

        public SavedJobsViewModel(JobTypesViewModel jobTypesViewModel)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SavedJobsViewModel constructed. JobTypes count: {jobTypesViewModel.JobTypes.Count}");
            JobTypes = jobTypesViewModel.JobTypes;
            LoadJobsFromStorage();
            LoadRecycleBinFromStorage();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<JobSavedMessage>(this, (r, m) =>
            {
                AllJobs.Add(m.Job);
                SaveJobsToStorage();
                FilterJobs();
            });
            
            // Register for job updates
            WeakReferenceMessenger.Default.Register<JobUpdatedMessage>(this, (r, m) =>
            {
                var existingJob = AllJobs.FirstOrDefault(j => j.Id == m.Job.Id);
                if (existingJob != null)
                {
                    var index = AllJobs.IndexOf(existingJob);
                    AllJobs[index] = m.Job;
                    SaveJobsToStorage();
                    FilterJobs();
                }
            });
        }

        private void SaveJobsToStorage()
        {
            try
            {
                var json = JsonSerializer.Serialize(AllJobs);
                Preferences.Set("saved_jobs", json);
            }
            catch { /* ignore */ }
        }

        private void LoadJobsFromStorage()
        {
            try
            {
                var json = Preferences.Get("saved_jobs", null);
                if (!string.IsNullOrEmpty(json))
                {
                    var jobs = JsonSerializer.Deserialize<List<Job>>(json);
                    if (jobs != null)
                    {
                        AllJobs.Clear();
                        foreach (var job in jobs)
                            AllJobs.Add(job);
                    }
                }
            }
            catch { /* ignore */ }
        }

        // Call this when a job type is selected
        private void UpdateFieldSearchTerms()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateFieldSearchTerms called. SelectedJobType: {SelectedJobType?.Name ?? "null"}");
            FieldSearchTerms.Clear();
            // List of field types that should have a search bar
            var searchableTypes = new[] {
                Photo_Job_Save_Manager.Models.JobFieldType.Text,
                Photo_Job_Save_Manager.Models.JobFieldType.Dropdown
                // Add future searchable types here, e.g.:
                // Photo_Job_Save_Manager.Models.JobFieldType.Price,
                // Photo_Job_Save_Manager.Models.JobFieldType.Status,
                // Photo_Job_Save_Manager.Models.JobFieldType.Date,
            };
            if (SelectedJobType != null)
            {
                foreach (var field in SelectedJobType.Fields)
                {
                    if (searchableTypes.Contains(field.FieldType))
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Adding search field: {field.Name}");
                        FieldSearchTerms.Add(new FieldSearchTermViewModel(field.Name, field.AllowedValues));
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] FieldSearchTerms count: {FieldSearchTerms.Count}");
            OnPropertyChanged(nameof(FieldSearchTerms));
        }

        // Call this when any search field changes
        public void SetFieldSearchTerm(string fieldName, string searchTerm)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SetFieldSearchTerm called: {fieldName} = '{searchTerm}'");
            var searchVm = FieldSearchTerms.FirstOrDefault(f => f.FieldName == fieldName);
            if (searchVm != null)
            {
                searchVm.SearchText = searchTerm;
                FilterJobs();
            }
        }

        private void FilterJobs()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] FilterJobs called. AllJobs count: {AllJobs.Count}, SelectedJobType: {SelectedJobType?.Name ?? "null"}");
            FilteredJobs.Clear();
            if (SelectedJobType == null)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] No SelectedJobType, returning early from FilterJobs.");
                return;
            }
            var jobsOfType = AllJobs.Where(j =>
                j.JobTypeId.Trim().Equals(SelectedJobType.Name.Trim(), System.StringComparison.OrdinalIgnoreCase)).ToList();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] jobsOfType count: {jobsOfType.Count}");
            foreach (var job in jobsOfType)
            {
                bool matches = true;
                // Field search filters
                foreach (var searchVm in FieldSearchTerms)
                {
                    var fieldName = searchVm.FieldName;
                    var search = searchVm.SearchText;
                    if (!string.IsNullOrEmpty(search))
                    {
                        if (!job.Data.TryGetValue(fieldName, out var value) || value == null ||
                            !value.ToString().Contains(search, System.StringComparison.OrdinalIgnoreCase))
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Job {job.Id} does not match field '{fieldName}' with search '{search}'.");
                            matches = false;
                            break;
                        }
                    }
                }
                // Status filter (generic: filter by any field name)
                if (matches && !string.IsNullOrEmpty(StatusFilter))
                {
                    if (!job.Data.TryGetValue(StatusFilterField, out var statusValue) ||
                        statusValue == null ||
                        !statusValue.ToString().Contains(StatusFilter, System.StringComparison.OrdinalIgnoreCase))
                    {
                        matches = false;
                    }
                }
                // Date range filter
                if (matches && DateCreatedStart.HasValue)
                {
                    if (job.CreatedAt < DateCreatedStart.Value)
                        matches = false;
                }
                if (matches && DateCreatedEnd.HasValue)
                {
                    if (job.CreatedAt > DateCreatedEnd.Value)
                        matches = false;
                }
                if (matches)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Job {job.Id} matches all search terms, adding to FilteredJobs.");
                    FilteredJobs.Add(job);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[DEBUG] FilteredJobs count after filtering: {FilteredJobs.Count}");
        }

        public void MoveJobToRecycleBin(Job job)
        {
            if (AllJobs.Contains(job))
            {
                AllJobs.Remove(job);
                RecycleBinJobs.Add(job);
                SaveJobsToStorage();
                SaveRecycleBinToStorage();
                FilterJobs();
            }
        }

        private void SaveRecycleBinToStorage()
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(RecycleBinJobs);
                Microsoft.Maui.Storage.Preferences.Set("recycle_bin_jobs", json);
            }
            catch { /* ignore */ }
        }

        private void LoadRecycleBinFromStorage()
        {
            try
            {
                var json = Microsoft.Maui.Storage.Preferences.Get("recycle_bin_jobs", null);
                if (!string.IsNullOrEmpty(json))
                {
                    var jobs = System.Text.Json.JsonSerializer.Deserialize<List<Job>>(json);
                    if (jobs != null)
                    {
                        RecycleBinJobs.Clear();
                        foreach (var job in jobs)
                            RecycleBinJobs.Add(job);
                    }
                }
            }
            catch { /* ignore */ }
        }

        public class FieldSearchTermViewModel : BaseViewModel
        {
            public string FieldName { get; }
            public List<string>? AllowedValues { get; }
            private string _searchText = string.Empty;
            public string SearchText
            {
                get => _searchText;
                set => SetProperty(ref _searchText, value);
            }
            public FieldSearchTermViewModel(string fieldName, List<string>? allowedValues = null)
            {
                FieldName = fieldName;
                AllowedValues = allowedValues;
            }
        }
    }
} 