using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Entities.Services.Interfaces;
using Entities.Models;
using PharmacistRecommendation.Views;

namespace PharmacistRecommendation.ViewModels
{
    public class MedicationViewModel : INotifyPropertyChanged
    {
        private readonly IMedicationService _medicationService;
        private readonly IMedicationImportService _importService;
        private readonly IServiceProvider _serviceProvider;
        private string _searchText = string.Empty;
        private string _searchCode = string.Empty;
        private Medication? _selectedMedication;
        private bool _isLoading;

        public MedicationViewModel(IMedicationService medicationService, IMedicationImportService importService, IServiceProvider serviceProvider)
        {
            _medicationService = medicationService ?? throw new ArgumentNullException(nameof(medicationService));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Initialize collections
            Medications = new ObservableCollection<Medication>();

            // Initialize commands
            LoadMedicationsCommand = new Command(async () => await LoadMedicationsAsync());
            SearchCommand = new Command(async () => await SearchMedicationsAsync());
            AddMedicationCommand = new Command(async () => await AddMedicationAsync());
            EditMedicationCommand = new Command<Medication>(async (medication) => await EditMedicationAsync(medication));
            DeleteMedicationCommand = new Command<Medication>(async (medication) => await DeleteMedicationAsync(medication));
            ImportCsvCommand = new Command(async () => await ImportCsvAsync());
        }

        public MedicationViewModel() : this(null!, null!, null!)
        {
            // This constructor should only be used by XAML designer
        }

        public ObservableCollection<Medication> Medications { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public string SearchCode
        {
            get => _searchCode;
            set
            {
                _searchCode = value;
                OnPropertyChanged();
            }
        }

        public Medication? SelectedMedication
        {
            get => _selectedMedication;
            set
            {
                _selectedMedication = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadMedicationsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddMedicationCommand { get; }
        public ICommand EditMedicationCommand { get; }
        public ICommand DeleteMedicationCommand { get; }
        public ICommand ImportCsvCommand { get; }

        public async Task LoadMedicationsAsync()
        {
            if (_medicationService == null)
            {
                await ShowAlert("Error", "Medication service not initialized", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                var medications = await _medicationService.GetAllMedicationsAsync();
                Medications.Clear();

                foreach (var medication in medications)
                {
                    Medications.Add(medication);
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", $"Failed to load medications: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SearchMedicationsAsync()
        {
            if (_medicationService == null)
            {
                await ShowAlert("Error", "Medication service not initialized", "OK");
                return;
            }

            IsLoading = true;
            try
            {
                var searchTerm = !string.IsNullOrEmpty(SearchText) ? SearchText : SearchCode;
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    await LoadMedicationsAsync();
                    return;
                }

                var medications = await _medicationService.SearchMedicationsAsync(searchTerm);
                Medications.Clear();

                foreach (var medication in medications)
                {
                    Medications.Add(medication);
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", $"Failed to search medications: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddMedicationAsync()
        {
            try
            {
                var addEditView = _serviceProvider.GetRequiredService<AddEditMedicationView>();
                var addEditViewModel = _serviceProvider.GetRequiredService<AddEditMedicationViewModel>();

                addEditView.BindingContext = addEditViewModel;

                // Show the add medication page
                await Application.Current.MainPage.Navigation.PushAsync(addEditView);

                // Wait for user to complete the add operation
                var result = await addEditViewModel.ShowForAddAsync();

                // Close the add medication page
                await Application.Current.MainPage.Navigation.PopAsync();

                // If medication was added successfully, refresh the list
                if (result)
                {
                    await LoadMedicationsAsync();
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", $"Failed to show add medication page: {ex.Message}", "OK");
            }
        }

        private async Task EditMedicationAsync(Medication medication)
        {
            if (medication == null)
            {
                await ShowAlert("Error", "No medication selected", "OK");
                return;
            }

            try
            {
                var addEditView = _serviceProvider.GetRequiredService<AddEditMedicationView>();
                var addEditViewModel = _serviceProvider.GetRequiredService<AddEditMedicationViewModel>();

                addEditView.BindingContext = addEditViewModel;

                // Show the edit medication page
                await Application.Current.MainPage.Navigation.PushAsync(addEditView);

                // Wait for user to complete the edit operation
                var result = await addEditViewModel.ShowForEditAsync(medication);

                // Close the edit medication page
                await Application.Current.MainPage.Navigation.PopAsync();

                // If medication was updated successfully, refresh the list
                if (result)
                {
                    await LoadMedicationsAsync();
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", $"Failed to show edit medication page: {ex.Message}", "OK");
            }
        }

        private async Task DeleteMedicationAsync(Medication medication)
        {
            if (medication == null)
            {
                await ShowAlert("Error", "No medication selected", "OK");
                return;
            }

            if (_medicationService == null)
            {
                await ShowAlert("Error", "Medication service not initialized", "OK");
                return;
            }

            var name = medication.Denumire ?? "Unknown";
            var result = await ShowAlert("Confirm", $"Delete medication: {name}?", "Yes", "No");

            if (result)
            {
                try
                {
                    await _medicationService.DeleteMedicationAsync(medication.Id);
                    Medications.Remove(medication);
                    await ShowAlert("Success", "Medication deleted successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await ShowAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
            }
        }

        private async Task ImportCsvAsync()
        {
            try
            {
                var fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select CSV or Excel file",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS, new[] { "public.comma-separated-values-text", "org.openxmlformats.spreadsheetml.sheet" } },
                        { DevicePlatform.Android, new[] { "text/csv", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                        { DevicePlatform.WinUI, new[] { ".csv", ".xlsx" } },
                        { DevicePlatform.macOS, new[] { "csv", "xlsx" } }
                    })
                });

                if (fileResult != null)
                {
                    IsLoading = true;

                    using var stream = await fileResult.OpenReadAsync();

                    // Parse the file
                    List<CsvMedicationRow> csvData;
                    if (fileResult.FileName.EndsWith(".xlsx"))
                    {
                        csvData = await _importService.ParseExcelFileAsync(stream);
                    }
                    else
                    {
                        csvData = await _importService.ParseCsvFileAsync(stream);
                    }

                    // Preview the import
                    var previewResult = await _importService.PreviewCsvImportAsync(csvData);

                    // Show preview to user
                    var previewMessage = $"Import Preview:\n" +
                                       $"• New medications: {previewResult.AddedCount}\n" +
                                       $"• Updated medications: {previewResult.UpdatedCount}\n" +
                                       $"• Code changes: {previewResult.CodeChangesCount}\n" +
                                       $"• Manual conflicts: {previewResult.ConflictsCount}\n" +
                                       $"• Total processed: {previewResult.ProcessedCount}";

                    if (previewResult.HasErrors)
                    {
                        previewMessage += $"\n• Errors: {previewResult.Errors.Count}";
                    }

                    // Handle manual conflicts if they exist
                    if (previewResult.HasConflicts)
                    {
                        var conflictMessage = $"{previewMessage}\n\n" +
                                            $"⚠️ There are {previewResult.ConflictsCount} manual medication conflicts that need your attention.\n\n" +
                                            $"Do you want to resolve these conflicts now?";

                        var resolveConflicts = await ShowAlert("Manual Conflicts Detected", conflictMessage, "Resolve Conflicts", "Cancel");

                        if (resolveConflicts)
                        {
                            // Show conflict resolution UI
                            var resolvedConflicts = await ShowConflictResolutionAsync(previewResult.ManualMedicationConflicts);

                            if (resolvedConflicts != null)
                            {
                                // Update the conflicts with user decisions
                                previewResult.ManualMedicationConflicts.Clear();
                                previewResult.ManualMedicationConflicts.AddRange(resolvedConflicts);
                            }
                            else
                            {
                                // User cancelled conflict resolution
                                return;
                            }
                        }
                        else
                        {
                            // User cancelled
                            return;
                        }
                    }

                    // Ask to proceed with import
                    var proceed = await ShowAlert("Import Preview", previewMessage, "Proceed", "Cancel");

                    if (proceed)
                    {
                        // Execute import
                        var importOptions = new CsvImportOptions
                        {
                            ImportDataSource = "CSV_Import",
                            UpdateManualMedications = previewResult.ManualMedicationConflicts.Any(c => c.UserWantsUpdate),
                            ConfirmCodeChanges = true
                        };

                        var result = await _importService.ExecuteCsvImportAsync(csvData, importOptions);

                        // Handle resolved conflicts
                        if (previewResult.ManualMedicationConflicts.Any(c => c.UserWantsUpdate))
                        {
                            var conflictResult = await _importService.HandleManualMedicationConflictsAsync(
                                previewResult.ManualMedicationConflicts.Where(c => c.UserWantsUpdate).ToList());

                            result.ProcessedCount += conflictResult.ProcessedCount;
                        }

                        // Show results
                        var resultMessage = $"Import Complete:\n" +
                                          $"• Added: {result.AddedCount}\n" +
                                          $"• Updated: {result.UpdatedCount}\n" +
                                          $"• Processed: {result.ProcessedCount}";

                        if (result.HasErrors)
                        {
                            resultMessage += $"\n• Errors: {result.Errors.Count}";
                        }

                        await ShowAlert("Import Complete", resultMessage, "OK");

                        // Refresh the medication list
                        await LoadMedicationsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Import Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<List<MedicationConflict>?> ShowConflictResolutionAsync(List<MedicationConflict> conflicts)
        {
            try
            {
                var conflictView = _serviceProvider.GetRequiredService<ConflictResolutionView>();
                var conflictViewModel = _serviceProvider.GetRequiredService<ConflictResolutionViewModel>();

                conflictView.BindingContext = conflictViewModel;

                // Show the conflict resolution page
                await Application.Current.MainPage.Navigation.PushAsync(conflictView);

                // Wait for user to resolve conflicts
                var resolvedConflicts = await conflictViewModel.ShowConflictsAsync(conflicts);

                // Close the conflict resolution page
                await Application.Current.MainPage.Navigation.PopAsync();

                return resolvedConflicts;
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", $"Failed to show conflict resolution: {ex.Message}", "OK");
                return null;
            }
        }

        // Helper method to safely show alerts
        private async Task ShowAlert(string title, string message, string accept)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, accept);
            }
        }

        private async Task<bool> ShowAlert(string title, string message, string accept, string cancel)
        {
            if (Application.Current?.MainPage != null)
            {
                return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            }
            return false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}