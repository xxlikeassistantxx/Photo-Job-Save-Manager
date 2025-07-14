using System.Collections.ObjectModel;
using System.Windows.Input;
using Photo_Job_Save_Manager.Models;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class RecycleBinViewModel : BaseViewModel
    {
        public ObservableCollection<RecycleBinEntry> Entries { get; } = new();
        public ICommand RestoreEntryCommand { get; }
        public ICommand DeleteEntryCommand { get; }
        public ICommand CleanRecycleBinCommand { get; }

        public RecycleBinViewModel()
        {
            Title = "Recycle Bin";
            LoadRecycleBin();
            RestoreEntryCommand = new Command<RecycleBinEntry>(async (entry) => await RestoreEntryAsync(entry));
            DeleteEntryCommand = new Command<RecycleBinEntry>(async (entry) => await DeleteEntryAsync(entry));
            CleanRecycleBinCommand = new Command(async () => await CleanRecycleBinAsync());
        }

        private void LoadRecycleBin()
        {
            try
            {
                var json = Preferences.Get("recycle_bin", null);
                if (!string.IsNullOrEmpty(json))
                {
                    var entries = JsonSerializer.Deserialize<List<RecycleBinEntry>>(json);
                    if (entries != null)
                    {
                        Entries.Clear();
                        foreach (var e in entries)
                            Entries.Add(e);
                    }
                }
            }
            catch { /* ignore */ }
        }

        private void SaveRecycleBin()
        {
            try
            {
                var json = JsonSerializer.Serialize(Entries);
                Preferences.Set("recycle_bin", json);
            }
            catch { /* ignore */ }
        }

        private async Task RestoreEntryAsync(RecycleBinEntry entry)
        {
            // TODO: Implement restore logic for job types and jobs
            Entries.Remove(entry);
            SaveRecycleBin();
            await Application.Current.Windows[0].Page.DisplayAlert("Restored", "Entry restored (not yet implemented)", "OK");
        }

        private async Task DeleteEntryAsync(RecycleBinEntry entry)
        {
            Entries.Remove(entry);
            SaveRecycleBin();
            await Application.Current.Windows[0].Page.DisplayAlert("Deleted", "Entry permanently deleted.", "OK");
        }

        private async Task CleanRecycleBinAsync()
        {
            var confirm = await Application.Current.Windows[0].Page.DisplayAlert(
                "Clean Recycle Bin",
                "This will permanently delete all items in the recycle bin. Continue?",
                "Yes, Clean",
                "Cancel");
            if (!confirm) return;
            Entries.Clear();
            SaveRecycleBin();
            await Application.Current.Windows[0].Page.DisplayAlert("Recycle Bin Cleaned", "All items have been permanently deleted.", "OK");
        }
    }
} 