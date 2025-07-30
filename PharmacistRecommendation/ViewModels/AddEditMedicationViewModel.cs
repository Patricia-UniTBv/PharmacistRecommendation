using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Entities.Models;
using Entities.Services.Interfaces;

namespace PharmacistRecommendation.ViewModels
{
    public class AddEditMedicationViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IMedicationService _medicationService;
        private bool _isLoading;
        private bool _isEditMode;
        private Medication? _originalMedication;
        private TaskCompletionSource<bool>? _completionSource;
        private System.Threading.Timer? _autoSaveTimer;
        private CancellationTokenSource? _validationCancellation; // Add this for validation cancellation

        // Medication properties
        private string _codCIM = string.Empty;
        private string _denumire = string.Empty;
        private string _dci = string.Empty; // Fixed: changed from _ci to _dci
        private string _formaFarmaceutica = string.Empty;
        private string _concentratia = string.Empty;
        private string _firmaProducatoare = string.Empty;
        private string _firmaDetinatoare = string.Empty;
        private string _codATC = string.Empty;
        private string _actiuneTerapeutica = string.Empty;
        private string _prescriptie = string.Empty;
        private string _nrData = string.Empty;
        private string _ambalaj = string.Empty;
        private string _volumAmbalaj = string.Empty;
        private string _valabilitate = string.Empty;
        private bool _hasBulina;
        private bool _hasDiez;
        private bool _hasStea;
        private bool _hasTriunghi;
        private bool _hasDreptunghi;

        // Validation properties
        private string _codCIMError = string.Empty;
        private string _denumireError = string.Empty;
        private string _codATCError = string.Empty;

        // Loading properties
        private string _loadingMessage = "Se salvează...";

        public AddEditMedicationViewModel(IMedicationService medicationService)
        {
            _medicationService = medicationService;
            
            SaveCommand = new Command(async () => await SaveAsync(), () => !IsLoading);
            CancelCommand = new Command(async () => await CancelAsync());
        }

        // Public Properties
        public string PageTitle => _isEditMode ? "Editează Medicament" : "Adaugă Medicament";
        public string SaveButtonText => _isEditMode ? "Actualizează" : "Salvează";

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;
                OnPropertyChanged();
            }
        }

        // Medication Properties
        public string CodCIM
        {
            get => _codCIM;
            set
            {
                _codCIM = value;
                OnPropertyChanged();
                ValidateCodCIM();
                StartAutoSave();
            }
        }

        public string Denumire
        {
            get => _denumire;
            set
            {
                _denumire = value;
                OnPropertyChanged();
                ValidateDenumire();
                StartAutoSave();
            }
        }

        public string DCI
        {
            get => _dci;
            set
            {
                _dci = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string FormaFarmaceutica     
        {
            get => _formaFarmaceutica;
            set
            {
                _formaFarmaceutica = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string Concentratia
        {
            get => _concentratia;
            set
            {
                _concentratia = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string FirmaProducatoare
        {
            get => _firmaProducatoare;
            set
            {
                _firmaProducatoare = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string FirmaDetinatoare
        {
            get => _firmaDetinatoare;
            set
            {
                _firmaDetinatoare = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string CodATC
        {
            get => _codATC;
            set
            {
                _codATC = value;
                OnPropertyChanged();
                ValidateCodATC();
                StartAutoSave();
            }
        }

        public string ActiuneTerapeutica
        {
            get => _actiuneTerapeutica;
            set
            {
                _actiuneTerapeutica = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string Prescriptie
        {
            get => _prescriptie;
            set
            {
                _prescriptie = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string NrData
        {
            get => _nrData;
            set
            {
                _nrData = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string Ambalaj
        {
            get => _ambalaj;
            set
            {
                _ambalaj = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string VolumAmbalaj
        {
            get => _volumAmbalaj;
            set
            {
                _volumAmbalaj = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public string Valabilitate
        {
            get => _valabilitate;
            set
            {
                _valabilitate = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public bool HasBulina
        {
            get => _hasBulina;
            set
            {
                _hasBulina = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public bool HasDiez
        {
            get => _hasDiez;
            set
            {
                _hasDiez = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public bool HasStea
        {
            get => _hasStea;
            set
            {
                _hasStea = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public bool HasTriunghi
        {
            get => _hasTriunghi;
            set
            {
                _hasTriunghi = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        public bool HasDreptunghi
        {
            get => _hasDreptunghi;
            set
            {
                _hasDreptunghi = value;
                OnPropertyChanged();
                StartAutoSave();
            }
        }

        // Validation Properties
        public string CodCIMError
        {
            get => _codCIMError;
            set
            {
                _codCIMError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCodCIMError));
            }
        }

        public string DenumireError
        {
            get => _denumireError;
            set
            {
                _denumireError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDenumireError));
            }
        }

        public string CodATCError
        {
            get => _codATCError;
            set
            {
                _codATCError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCodATCError));
            }
        }

        public bool HasCodCIMError => !string.IsNullOrEmpty(CodCIMError);
        public bool HasDenumireError => !string.IsNullOrEmpty(DenumireError);
        public bool HasCodATCError => !string.IsNullOrEmpty(CodATCError);

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Public Methods
        public async Task<bool> ShowForAddAsync()
        {
            _isEditMode = false;
            _completionSource = new TaskCompletionSource<bool>();
            await ClearFormAsync();
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
            return await _completionSource.Task;
        }

        public async Task<bool> ShowForEditAsync(Medication medication)
        {
            _isEditMode = true;
            _originalMedication = medication;
            _completionSource = new TaskCompletionSource<bool>();
            LoadMedicationData(medication);
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
            return await _completionSource.Task;
        }

        // Private Methods
        private async Task ClearFormAsync()
        {
            // Clear form data
            CodCIM = string.Empty;
            Denumire = string.Empty;
            DCI = string.Empty;
            FormaFarmaceutica = string.Empty;
            Concentratia = string.Empty;
            FirmaProducatoare = string.Empty;
            FirmaDetinatoare = string.Empty;
            CodATC = string.Empty;
            ActiuneTerapeutica = string.Empty;
            Prescriptie = string.Empty;
            NrData = string.Empty;
            Ambalaj = string.Empty;
            VolumAmbalaj = string.Empty;
            Valabilitate = string.Empty;
            HasBulina = false;
            HasDiez = false;
            HasStea = false;
            HasTriunghi = false;
            HasDreptunghi = false;
            
            // Clear validation errors
            CodCIMError = string.Empty;
            DenumireError = string.Empty;
            CodATCError = string.Empty;
            
            // Try to restore draft if available
            await RestoreFormDraftAsync();
        }

        private void LoadMedicationData(Medication medication)
        {
            CodCIM = medication.CodCIM ?? string.Empty;
            Denumire = medication.Denumire ?? string.Empty;
            DCI = medication.DCI ?? string.Empty;
            FormaFarmaceutica = medication.FormaFarmaceutica ?? string.Empty;
            Concentratia = medication.Concentratia ?? string.Empty;
            FirmaProducatoare = medication.FirmaProducatoare ?? string.Empty;
            FirmaDetinatoare = medication.FirmaDetinatoare ?? string.Empty;
            CodATC = medication.CodATC ?? string.Empty;
            ActiuneTerapeutica = medication.ActiuneTerapeutica ?? string.Empty;
            Prescriptie = medication.Prescriptie ?? string.Empty;
            NrData = medication.NrData ?? string.Empty;
            Ambalaj = medication.Ambalaj ?? string.Empty;
            VolumAmbalaj = medication.VolumAmbalaj ?? string.Empty;
            Valabilitate = medication.Valabilitate ?? string.Empty;
            HasBulina = medication.Bulina == "X";
            HasDiez = medication.Diez == "X";
            HasStea = medication.Stea == "X";
            HasTriunghi = medication.Triunghi == "X";
            HasDreptunghi = medication.Dreptunghi == "X";
        }

        private void ValidateCodCIM()
        {
            if (string.IsNullOrWhiteSpace(CodCIM))
            {
                CodCIMError = "Cod CIM este obligatoriu";
            }
            else if (CodCIM.Length < 3)
            {
                CodCIMError = "Cod CIM trebuie să aibă cel puțin 3 caractere";
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(CodCIM, @"^[A-Z0-9]+$"))
            {
                CodCIMError = "Cod CIM poate conține doar litere mari și cifre";
            }
            else
            {
                CodCIMError = string.Empty;
                // FIXED: Remove real-time validation that causes threading issues
                // _ = Task.Run(async () => await ValidateCodCIMUniquenessAsync());
            }
        }

        // FIXED: Remove async uniqueness validation to prevent threading issues
        // The uniqueness check will only happen on form submission
        private async Task<bool> CheckCodCIMUniquenessAsync()
        {
            if (string.IsNullOrWhiteSpace(_codCIM)) return true;
            
            try
            {
                var existingMedications = await _medicationService.SearchMedicationsAsync(_codCIM);
                return !existingMedications.Any(m => 
                    m.Id != (_originalMedication?.Id ?? 0) && 
                    string.Equals(m.CodCIM, _codCIM, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating CodCIM uniqueness: {ex.Message}");
                return true; // Allow save if validation fails
            }
        }

        private void ValidateCodATC()
        {
            if (!string.IsNullOrWhiteSpace(CodATC))
            {
                // ATC codes follow a specific pattern: A10BA02
                if (!System.Text.RegularExpressions.Regex.IsMatch(CodATC, @"^[A-Z][0-9]{2}[A-Z]{2}[0-9]{2}$"))
                {
                    CodATCError = "Format invalid (ex: A10BA02)";
                }
                else
                {
                    CodATCError = string.Empty;
                }
            }
            else
            {
                CodATCError = string.Empty;
            }
        }

        private void ValidateDenumire()
        {
            if (string.IsNullOrWhiteSpace(Denumire))
            {
                DenumireError = "Denumirea este obligatorie";
            }
            else if (Denumire.Length < 2)
            {
                DenumireError = "Denumirea trebuie să aibă cel puțin 2 caractere";
            }
            else
            {
                DenumireError = string.Empty;
            }
        }

        private async Task<bool> ValidateFormAsync()
        {
            ValidateCodCIM();
            ValidateDenumire();
            ValidateCodATC();

            // Check CodCIM uniqueness only on save
            var isUnique = await CheckCodCIMUniquenessAsync();
            if (!isUnique)
            {
                CodCIMError = "Cod CIM există deja în baza de date";
            }

            return !HasCodCIMError && !HasDenumireError && !HasCodATCError;
        }

        private async Task SaveAsync()
        {
            if (!await ValidateFormAsync())
            {
                await ShowAlert("Eroare Validare", "Vă rugăm să corectați erorile de validare.", "OK");
                return;
            }

            IsLoading = true;
            LoadingMessage = _isEditMode ? "Se actualizează..." : "Se salvează...";
            
            try
            {
                var medication = CreateMedicationFromForm();

                if (_isEditMode && _originalMedication != null)
                {
                    medication.Id = _originalMedication.Id;
                    medication.CreatedAt = _originalMedication.CreatedAt;
                    medication.DataSource = _originalMedication.DataSource;
                    await _medicationService.UpdateMedicationAsync(medication);
                    await ShowAlert("Succes", "Medicamentul a fost actualizat cu succes!", "OK");
                }
                else
                {
                    medication.DataSource = "Manual";
                    await _medicationService.AddMedicationAsync(medication);
                    await ShowAlert("Succes", "Medicamentul a fost adăugat cu succes!", "OK");
                }

                // Clear auto-save draft after successful save
                await ClearFormDraftAsync();
                
                _completionSource?.SetResult(true);
            }
            catch (Exception ex)
            {
                await ShowAlert("Eroare", $"A apărut o eroare la salvarea medicamentului: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "Se salvează...";
            }
        }

        private async Task CancelAsync()
        {
            var hasChanges = _isEditMode ? HasChanges() : HasFormData();
            
            if (hasChanges)
            {
                var result = await ShowAlert("Confirmare", 
                    "Aveți modificări nesalvate. Sigur doriți să ieșiți?", "Da", "Nu");
                if (!result)
                    return;
            }

            await ClearFormDraftAsync();
            _completionSource?.SetResult(false);
        }

        private bool HasFormData()
        {
            return !string.IsNullOrWhiteSpace(CodCIM) ||
                   !string.IsNullOrWhiteSpace(Denumire) ||
                   !string.IsNullOrWhiteSpace(DCI) ||
                   !string.IsNullOrWhiteSpace(FormaFarmaceutica) ||
                   !string.IsNullOrWhiteSpace(Concentratia) ||
                   !string.IsNullOrWhiteSpace(CodATC);
        }

        private bool HasChanges()
        {
            if (_originalMedication == null) return false;

            return CodCIM != (_originalMedication.CodCIM ?? string.Empty) ||
                   Denumire != (_originalMedication.Denumire ?? string.Empty) ||
                   DCI != (_originalMedication.DCI ?? string.Empty) ||
                   FormaFarmaceutica != (_originalMedication.FormaFarmaceutica ?? string.Empty) ||
                   Concentratia != (_originalMedication.Concentratia ?? string.Empty) ||
                   FirmaProducatoare != (_originalMedication.FirmaProducatoare ?? string.Empty) ||
                   FirmaDetinatoare != (_originalMedication.FirmaDetinatoare ?? string.Empty) ||
                   CodATC != (_originalMedication.CodATC ?? string.Empty) ||
                   ActiuneTerapeutica != (_originalMedication.ActiuneTerapeutica ?? string.Empty) ||
                   Prescriptie != (_originalMedication.Prescriptie ?? string.Empty) ||
                   NrData != (_originalMedication.NrData ?? string.Empty) ||
                   Ambalaj != (_originalMedication.Ambalaj ?? string.Empty) ||
                   VolumAmbalaj != (_originalMedication.VolumAmbalaj ?? string.Empty) ||
                   Valabilitate != (_originalMedication.Valabilitate ?? string.Empty) ||
                   HasBulina != (_originalMedication.Bulina == "X") ||
                   HasDiez != (_originalMedication.Diez == "X") ||
                   HasStea != (_originalMedication.Stea == "X") ||
                   HasTriunghi != (_originalMedication.Triunghi == "X") ||
                   HasDreptunghi != (_originalMedication.Dreptunghi == "X");
        }

        private Medication CreateMedicationFromForm()
        {
            return new Medication
            {
                CodCIM = string.IsNullOrWhiteSpace(CodCIM) ? null : CodCIM.Trim(),
                Denumire = string.IsNullOrWhiteSpace(Denumire) ? null : Denumire.Trim(),
                DCI = string.IsNullOrWhiteSpace(DCI) ? null : DCI.Trim(),
                FormaFarmaceutica = string.IsNullOrWhiteSpace(FormaFarmaceutica) ? null : FormaFarmaceutica.Trim(),
                Concentratia = string.IsNullOrWhiteSpace(Concentratia) ? null : Concentratia.Trim(),
                FirmaProducatoare = string.IsNullOrWhiteSpace(FirmaProducatoare) ? null : FirmaProducatoare.Trim(),
                FirmaDetinatoare = string.IsNullOrWhiteSpace(FirmaDetinatoare) ? null : FirmaDetinatoare.Trim(),
                CodATC = string.IsNullOrWhiteSpace(CodATC) ? null : CodATC.Trim(),
                ActiuneTerapeutica = string.IsNullOrWhiteSpace(ActiuneTerapeutica) ? null : ActiuneTerapeutica.Trim(),
                Prescriptie = string.IsNullOrWhiteSpace(Prescriptie) ? null : Prescriptie.Trim(),
                NrData = string.IsNullOrWhiteSpace(NrData) ? null : NrData.Trim(),
                Ambalaj = string.IsNullOrWhiteSpace(Ambalaj) ? null : Ambalaj.Trim(),
                VolumAmbalaj = string.IsNullOrWhiteSpace(VolumAmbalaj) ? null : VolumAmbalaj.Trim(),
                Valabilitate = string.IsNullOrWhiteSpace(Valabilitate) ? null : Valabilitate.Trim(),
                Bulina = HasBulina ? "X" : null,
                Diez = HasDiez ? "X" : null,
                Stea = HasStea ? "X" : null,
                Triunghi = HasTriunghi ? "X" : null,
                Dreptunghi = HasDreptunghi ? "X" : null,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true
            };
        }

        // SIMPLIFIED: Auto-save without database calls to prevent threading issues
        private void StartAutoSave()
        {
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = new System.Threading.Timer(
                async _ => await AutoSaveFormAsync(), 
                null, 
                TimeSpan.FromSeconds(30), 
                Timeout.InfiniteTimeSpan);
        }

        private async Task AutoSaveFormAsync()
        {
            if (!HasFormData()) return;
            
            try
            {
                var formData = new
                {
                    CodCIM, Denumire, DCI, FormaFarmaceutica, Concentratia,
                    CodATC, ActiuneTerapeutica, Prescriptie,
                    Timestamp = DateTime.Now
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(formData);
                Preferences.Set("medication_draft", json);
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    System.Diagnostics.Debug.WriteLine("Form auto-saved");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error auto-saving form: {ex.Message}");
            }
        }

        private async Task RestoreFormDraftAsync()
        {
            try
            {
                var json = Preferences.Get("medication_draft", string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    var restore = await ShowAlert("Salvare Găsită", 
                        "S-a găsit o salvare automată. Doriți să o restaurați?", "Da", "Nu");
                    
                    if (restore)
                    {
                        await ClearFormDraftAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring form draft: {ex.Message}");
                await ClearFormDraftAsync();
            }
        }

        private async Task ClearFormDraftAsync()
        {
            try
            {
                Preferences.Remove("medication_draft");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing form draft: {ex.Message}");
            }
        }

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

        public void Dispose()
        {
            _validationCancellation?.Cancel();
            _validationCancellation?.Dispose();
            _autoSaveTimer?.Dispose();
            _autoSaveTimer = null;
        }
    }
}