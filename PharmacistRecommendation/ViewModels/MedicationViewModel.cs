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
        private readonly SemaphoreSlim _loadingSemaphore = new(1, 1); // Add concurrency protection
        private string _searchText = string.Empty;
        private string _searchCode = string.Empty;
        private Medication? _selectedMedication;
        private bool _isLoading;
        private int _selectedFilter = 0; // 0 = All, 1 = Active, 2 = Inactive, 3 = Manual, 4 = CSV

        public MedicationViewModel(IMedicationService medicationService, IMedicationImportService importService, IServiceProvider serviceProvider)
        {
            _medicationService = medicationService ?? throw new ArgumentNullException(nameof(medicationService));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // Initialize collections - Changed to MedicationDisplayItem
            Medications = new ObservableCollection<MedicationDisplayItem>();

            // Initialize commands - Updated for MedicationDisplayItem
            LoadMedicationsCommand = new Command(async () => await LoadMedicationsAsync());
            SearchCommand = new Command(async () => await SearchMedicationsAsync());
            AddMedicationCommand = new Command(async () => await AddMedicationAsync());
            EditMedicationCommand = new Command<MedicationDisplayItem>(async (item) => await EditMedicationAsync(item?.Medication));
            DeleteMedicationCommand = new Command<MedicationDisplayItem>(async (item) => await DeleteMedicationAsync(item?.Medication));
            ImportCsvCommand = new Command(async () => await ImportCsvAsync());
            ToggleActiveStatusCommand = new Command<MedicationDisplayItem>(async (item) => await ToggleActiveStatusAsync(item));
        }

        public MedicationViewModel() : this(null!, null!, null!)
        {
            // This constructor should only be used by XAML designer
        }

        // Changed from ObservableCollection<Medication> to MedicationDisplayItem
        public ObservableCollection<MedicationDisplayItem> Medications { get; }

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

        // Enhanced filter property - renamed from SelectedStatusFilter
        public int SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter != value)
                {
                    _selectedFilter = value;
                    OnPropertyChanged();
                    // Use Task.Run with proper synchronization to avoid blocking the UI thread
                    _ = Task.Run(async () =>
                    {
                        await _loadingSemaphore.WaitAsync();
                        try
                        {
                            // Dispatch back to UI thread for the actual load operation
                            await MainThread.InvokeOnMainThreadAsync(async () => await LoadMedicationsAsync());
                        }
                        finally
                        {
                            _loadingSemaphore.Release();
                        }
                    });
                }
            }
        }

        public ICommand LoadMedicationsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddMedicationCommand { get; }
        public ICommand EditMedicationCommand { get; }
        public ICommand DeleteMedicationCommand { get; }
        public ICommand ImportCsvCommand { get; }
        public ICommand ToggleActiveStatusCommand { get; } // New command for toggling status

        // Helper method that does the actual data loading without semaphore management
        private async Task<List<Medication>> GetFilteredMedicationsAsync(string? searchTerm = null)
        {
            List<Medication> medications;
            
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                medications = await _medicationService.GetAllMedicationsAsync();
            }
            else
            {
                medications = await _medicationService.SearchMedicationsAsync(searchTerm);
            }

            // Apply enhanced filter
            return SelectedFilter switch
            {
                1 => medications.Where(m => m.IsActive).ToList(), // Active
                2 => medications.Where(m => !m.IsActive).ToList(), // Inactive
                3 => medications.Where(m => m.DataSource == "Manual").ToList(), // Manually added
                4 => medications.Where(m => m.DataSource != "Manual").ToList(), // Automatically added (CSV)
                _ => medications // All
            };
        }

        // Simplified LoadMedicationsAsync
        public async Task LoadMedicationsAsync()
        {
            if (_medicationService == null)
            {
                await ShowAlert("Error", "Medication service not initialized", "OK");
                return;
            }

            if (!await _loadingSemaphore.WaitAsync(100))
            {
                return;
            }

            try
            {
                IsLoading = true;
                var filteredMedications = await GetFilteredMedicationsAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Medications.Clear();
                    foreach (var medication in filteredMedications)
                    {
                        Medications.Add(new MedicationDisplayItem(medication, this));
                    }
                });
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", $"Failed to load medications: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                _loadingSemaphore.Release();
            }
        }

        // Simplified SearchMedicationsAsync
        public async Task SearchMedicationsAsync()
        {
            if (_medicationService == null)
            {
                await ShowAlert("Error", "Medication service not initialized", "OK");
                return;
            }

            if (!await _loadingSemaphore.WaitAsync(100))
            {
                return;
            }

            try
            {
                IsLoading = true;
                var searchTerm = !string.IsNullOrEmpty(SearchText) ? SearchText : SearchCode;
                var filteredMedications = await GetFilteredMedicationsAsync(searchTerm);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Medications.Clear();
                    foreach (var medication in filteredMedications)
                    {
                        Medications.Add(new MedicationDisplayItem(medication, this));
                    }
                });
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", $"Failed to search medications: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                _loadingSemaphore.Release();
            }
        }

        // New method to toggle active status
        public async Task ToggleActiveStatusAsync(MedicationDisplayItem? item)
        {
            if (item?.Medication == null)
            {
                await ShowAlert("Error", "No medication selected", "OK");
                return;
            }

            if (_medicationService == null)
            {
                await ShowAlert("Error", "Medication service not initialized", "OK");
                return;
            }

            var medication = item.Medication;
            var newStatus = !medication.IsActive;
            var statusText = newStatus ? "activ" : "inactiv";
            var name = medication.Denumire ?? "Unknown";

            var confirmed = await ShowAlert(
                "Confirmare Status",
                $"Doriți să marcați medicamentul '{name}' ca {statusText}?",
                "Da",
                "Nu"
            );

            if (confirmed)
            {
                try
                {
                    // Update the medication status
                    medication.IsActive = newStatus;
                    await _medicationService.UpdateMedicationAsync(medication);

                    // Update the display item
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        item.RefreshStatus();
                    });

                    await ShowAlert("Succes", $"Medicamentul a fost marcat ca {statusText}!", "OK");
                }
                catch (Exception ex)
                {
                    await ShowAlert("Error", $"Failed to update medication status: {ex.Message}", "OK");
                }
            }
        }

        // Enhanced ImportCsvAsync using existing interface methods
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

                    // Get current medications to check for inactive ones that might be reactivated
                    var existingMedications = await _medicationService.GetAllMedicationsAsync();
                    var inactiveToReactivate = new List<Medication>();

                    // Check for inactive medications that exist in the CSV
                    foreach (var csvRow in csvData)
                    {
                        if (string.IsNullOrWhiteSpace(csvRow.CodCIM)) continue;

                        var inactiveMedication = existingMedications
                            .FirstOrDefault(m => m.CodCIM == csvRow.CodCIM && !m.IsActive);

                        if (inactiveMedication != null)
                        {
                            inactiveToReactivate.Add(inactiveMedication);
                        }
                    }

                    // Preview the import using existing method
                    var previewResult = await _importService.PreviewCsvImportAsync(csvData);

                    // Show enhanced preview to user including reactivation info
                    var previewMessage = $"Import Preview:\n" +
                                       $"• New medications: {previewResult.AddedCount}\n" +
                                       $"• Updated medications: {previewResult.UpdatedCount}\n" +
                                       $"• Code changes: {previewResult.CodeChangesCount}\n" +
                                       $"• Manual conflicts: {previewResult.ConflictsCount}\n" +
                                       $"• Inactive medications to reactivate: {inactiveToReactivate.Count}\n" +
                                       $"• Total processed: {previewResult.ProcessedCount}";

                    if (previewResult.HasErrors)
                    {
                        previewMessage += $"\n• Errors: {previewResult.Errors.Count}";
                    }

                    // Handle reactivations first if any exist
                    if (inactiveToReactivate.Count > 0)
                    {
                        var reactivationMessage = $"{previewMessage}\n\n" +
                                                $"⚠️ {inactiveToReactivate.Count} inactive medications will be automatically reactivated because they were found in the new CSV:\n\n";

                        foreach (var med in inactiveToReactivate.Take(5)) // Show first 5
                        {
                            reactivationMessage += $"• {med.Denumire} ({med.CodCIM})\n";
                        }

                        if (inactiveToReactivate.Count > 5)
                        {
                            reactivationMessage += $"• ... and {inactiveToReactivate.Count - 5} more\n";
                        }

                        reactivationMessage += $"\nDo you want to proceed with reactivating these medications?";

                        var confirmReactivation = await ShowAlert("Reactivate Medications", reactivationMessage, "Da", "Nu");
                        if (!confirmReactivation)
                        {
                            return;
                        }
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
                        // Execute import using existing method
                        var importOptions = new CsvImportOptions
                        {
                            ImportDataSource = "CSV_Import",
                            UpdateManualMedications = previewResult.ManualMedicationConflicts.Any(c => c.UserWantsUpdate),
                            ConfirmCodeChanges = true
                        };

                        var result = await _importService.ExecuteCsvImportAsync(csvData, importOptions);

                        // Handle reactivations manually after import
                        int reactivatedCount = 0;
                        foreach (var medication in inactiveToReactivate)
                        {
                            try
                            {
                                medication.IsActive = true;
                                medication.UpdatedAt = DateTime.Now;
                                await _medicationService.UpdateMedicationAsync(medication);
                                reactivatedCount++;
                            }
                            catch (Exception ex)
                            {
                                result.Warnings.Add($"Failed to reactivate {medication.Denumire}: {ex.Message}");
                            }
                        }

                        // Handle resolved conflicts
                        if (previewResult.ManualMedicationConflicts.Any(c => c.UserWantsUpdate))
                        {
                            var conflictResult = await _importService.HandleManualMedicationConflictsAsync(
                                previewResult.ManualMedicationConflicts.Where(c => c.UserWantsUpdate).ToList());

                            result.ProcessedCount += conflictResult.ProcessedCount;
                        }

                        // Show enhanced results including reactivations
                        var resultMessage = $"Import Complete:\n" +
                                          $"• Added: {result.AddedCount}\n" +
                                          $"• Updated: {result.UpdatedCount}\n" +
                                          $"• Reactivated: {reactivatedCount}\n" +
                                          $"• Processed: {result.ProcessedCount}";

                        if (result.HasWarnings)
                        {
                            resultMessage += $"\n\nWarnings:\n";
                            foreach (var warning in result.Warnings)
                            {
                                resultMessage += $"• {warning}\n";
                            }
                        }

                        if (result.HasErrors)
                        {
                            resultMessage += $"\nErrors: {result.Errors.Count}";
                        }

                        await ShowAlert("Import Complete", resultMessage, "OK");

                        // Log the import results
                        System.Diagnostics.Debug.WriteLine($"Import Results: Added={result.AddedCount}, Updated={result.UpdatedCount}, Reactivated={reactivatedCount}, Processed={result.ProcessedCount}, Errors={result.Errors.Count}");

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

        // Rest of your existing methods remain the same...
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

        private async Task EditMedicationAsync(Medication? medication)
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

        private async Task DeleteMedicationAsync(Medication? medication)
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

                    // Remove from the collection on main thread
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var itemToRemove = Medications.FirstOrDefault(m => m.Medication.Id == medication.Id);
                        if (itemToRemove != null)
                        {
                            Medications.Remove(itemToRemove);
                        }
                    });

                    await ShowAlert("Success", "Medication deleted successfully!", "OK");
                }
                catch (Exception ex)
                {
                    await ShowAlert("Error", $"Failed to delete: {ex.Message}", "OK");
                }
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
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(title, message, accept);
                }
            });
        }

        private async Task<bool> ShowAlert(string title, string message, string accept, string cancel)
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                }
                return false;
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Dispose semaphore when ViewModel is destroyed
        ~MedicationViewModel()
        {
            _loadingSemaphore?.Dispose();
        }
    }

    // Enhanced MedicationDisplayItem wrapper class with clickable status and ViewModel reference
    public class MedicationDisplayItem : INotifyPropertyChanged
    {
        private readonly MedicationViewModel _viewModel;

        public Medication Medication { get; }

        public MedicationDisplayItem(Medication medication, MedicationViewModel viewModel)
        {
            Medication = medication;
            _viewModel = viewModel;
        }

        public string StatusText => Medication.IsActive ? "Activ" : "Inactiv";
        public string StatusColor => Medication.IsActive ? "Green" : "Red";
        public string CodCIM => Medication.CodCIM ?? "-";
        public string Denumire => Medication.Denumire ?? "Necunoscut";
        public string DCI => Medication.DCI ?? "DCI nedefinit";
        public string CodATC => Medication.CodATC ?? "-";
        public string SourceText => Medication.DataSource == "Manual" ? "Manual" : "CSV";
        public string SourceColor => Medication.DataSource == "Manual" ? "Blue" : "Orange";

        // Command for clicking on status
        public ICommand ToggleStatusCommand => _viewModel.ToggleActiveStatusCommand;

        // Method to refresh status display
        public void RefreshStatus()
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}