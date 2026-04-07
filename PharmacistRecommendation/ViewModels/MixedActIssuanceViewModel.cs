using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Entities.Helpers;
using Entities.Models;
using Entities.Services;
using Entities.Services.Interfaces;
using PharmacistRecommendation.Helpers;
using PharmacistRecommendation.Helpers.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using Document = QuestPDF.Fluent.Document;
using IContainer = QuestPDF.Infrastructure.IContainer;
using MimeKit;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;


namespace PharmacistRecommendation.ViewModels
{
    [QueryProperty(nameof(Mode), "mode")]
    [QueryProperty(nameof(PrescriptionId), "PrescriptionId")]
    [QueryProperty(nameof(IsReportViewMode), "IsReportViewMode")]
    public partial class MixedActIssuanceViewModel : ObservableObject, IDisposable
    {
        private string PrescriptionsPath { get; set; }
        private string ReceiptsPath { get; set; }

        private readonly IPrescriptionService _prescriptionService;
        private readonly IAdministrationModeService _administrationModeService;
        private readonly IPharmacyService _pharmacyService;
        private readonly IImportConfigurationService _importService;
        private readonly IMedicationService _medicationService;
        private readonly IEmailConfigurationService _emailConfigurationService;
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;

        [ObservableProperty]
        string mode;

        partial void OnModeChanged(string value)
        {
            ShowWithPrescription = value == "mixed" || value == "withprescription";
            ShowWithoutPrescription = value == "mixed" || value == "withoutprescription";

            if (ShowWithPrescription && !ShowWithoutPrescription)
            {
                PageTitle = "Emitere act consecutiv prescripției";
            }
            else if (!ShowWithPrescription && ShowWithoutPrescription)
            {
                PageTitle = "Emitere act farmaceutic";
            }

        }

        private int pharmacyId { get; set; }
        [ObservableProperty]
        public bool showWithPrescription = true;
        [ObservableProperty]
        public bool showWithoutPrescription = true;
        [ObservableProperty]
        string pageTitle = "Emitere act mixt";
        [ObservableProperty]
        string? cardNumber;
        [ObservableProperty]
        string? patientName;
        [ObservableProperty]
        string? patientCnp;
        [ObservableProperty]
        string? caregiverName;
        [ObservableProperty]
        string? caregiverCnp;
        [ObservableProperty]
        string? patientDiagnosis;
        [ObservableProperty]
        string? usedMedications;
        [ObservableProperty]
        string? doctorStamp;
        [ObservableProperty]
        string? prescriptionSeries;
        [ObservableProperty]
        string? prescriptionNumber;
        [ObservableProperty]
        string? prescriptionDiagnosis;
        [ObservableProperty]
        string? symptoms;
        [ObservableProperty]
        string? suspicion;
        [ObservableProperty]
        string? pharmacistObservations;
        [ObservableProperty]
        ObservableCollection<PrescriptionDrugModel> medicationsWithPrescription = new();
        [ObservableProperty]
        ObservableCollection<ReceiptDrugModel> medicationsWithoutPrescription = new();
        [ObservableProperty]
        string notesToDoctor;
        [ObservableProperty]
        string pharmacistRecommendation;
        [ObservableProperty]
        ObservableCollection<string> pharmaceuticalServices = new() { "Aderență la tratament", "Administrare medicamente", "Consiliere", "Consiliere dispozitive medicale", "Consiliere OTC", "Consiliere RX", "Consiliere suplimente",
        "Farmacovigilenta", "Masurare parametri biologici", "Preparare medicamente", "Teste rapide", "Vaccinare"};
        [ObservableProperty]
        string selectedPharmaceuticalService = "Aderență la tratament";
        [ObservableProperty]
        ObservableCollection<AdministrationMode> administrationModes = new();
        [ObservableProperty]
        AdministrationMode? administrationMode;
        [ObservableProperty]
        private string totalQuantity; 

        [ObservableProperty]
        private string totalDays; 

        [ObservableProperty]
        private string? patientEmail;
        [ObservableProperty]
        bool canPrint = false;
        [ObservableProperty]
        bool isPrintButtonEnabled = false;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private string selectedMedication;

        public ObservableCollection<string> AllMedications { get; } = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> suggestions = new ObservableCollection<string>();

        public IRelayCommand<string> AddSuggestionCommand { get; }
        public Entry SearchEntryReference { get; set; }

        public ObservableCollection<string> FilteredMedications { get; } = new();
        public bool HasSuggestions => Suggestions != null && Suggestions.Count > 0;

        private CancellationTokenSource? _cts;

        private readonly IPharmacyCardService _pharmacyCardService;


        public MixedActIssuanceViewModel(IPrescriptionService prescriptionService, IAdministrationModeService administrationModeService, IPharmacyService pharmacyService,
            IImportConfigurationService importService, IMedicationService medicationService, IEmailConfigurationService emailConfigurationService, IPatientService patientService, IUserService userService, IPharmacyCardService pharmacyCardService)
        {
            _prescriptionService = prescriptionService ?? throw new ArgumentNullException(nameof(prescriptionService));
            _administrationModeService = administrationModeService ?? throw new ArgumentNullException(nameof(administrationModeService));
            _pharmacyService = pharmacyService ?? throw new ArgumentNullException(nameof(pharmacyService));
            _importService = importService ?? throw new ArgumentNullException(nameof(importService));
            _medicationService = medicationService ?? throw new ArgumentNullException(nameof(medicationService));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _emailConfigurationService = emailConfigurationService;
            _userService = userService;
            _pharmacyCardService = pharmacyCardService;

            AddSuggestionCommand = new RelayCommand<string>(AddSuggestionToText);

            _ = InitializeAsync();
            pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
        }

        private async Task InitializeAsync()
        {
            try
            {
                await LoadMedicationsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading medications: {ex.Message}");
            }

            try
            {
                await LoadAdministrationModes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading administration modes: {ex.Message}");
            }
        }


        [ObservableProperty]
        bool isReportViewMode = false;

        [ObservableProperty]
        private int prescriptionId;

        [ObservableProperty]
        private Prescription prescription;

        public bool IsSaveButtonVisible => true;

        partial void OnIsReportViewModeChanged(bool value)
        {
            OnPropertyChanged(nameof(IsSaveButtonVisible));
        }


        partial void OnPrescriptionIdChanged(int value)
        {
            if (value == 0) return;
            if (string.IsNullOrWhiteSpace(Mode)) return;

            _ = SafeRunAsync(() => LoadPrescriptionAsync(value));
        }

        partial void OnCardNumberChanged(string value)
        {
            if (IsReportViewMode) return;
            _ = SafeRunAsync(() => LoadPatientByCard(value));
        }

        private static async Task SafeRunAsync(Func<Task> action)
        {
            try { await action(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[SafeRunAsync] {ex}"); }
        }

        private async Task LoadPatientByCard(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 3)
                return;

            var patient = await _patientService.GetPatientByCardCodeAsync(cardNumber);
            if (patient != null)
            {
                PatientName = patient.FirstName + " " + patient.LastName;
                PatientEmail = patient.Email;
                PatientCnp = patient.Cnp;
            }
            else
            {
                PatientName = null;
                PatientEmail = null;
                PatientCnp = null;
            }
        }

        private List<string> _allMedicationsCache;

        public async Task LoadMedicationsAsync()
        {
            if (_allMedicationsCache != null)
                return;

            var all = await _medicationService.GetAllMedicationsAsync();
            _allMedicationsCache = all.Select(m => m.Denumire).ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AllMedications.Clear();
                foreach (var med in _allMedicationsCache)
                    AllMedications.Add(med);
            });
        }


        public void UpdateSuggestions(string text)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Suggestions.Clear();

                if (string.IsNullOrWhiteSpace(text))
                    return;

                var separators = new char[] { ' ', ',' };
                var parts = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                var lastWord = parts.LastOrDefault()?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(lastWord))
                    return;

                var filtered = AllMedications
                    .Where(m => m.IndexOf(lastWord, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(m => m)
                    .Take(20);

                foreach (var med in filtered)
                    Suggestions.Add(med);

                OnPropertyChanged(nameof(HasSuggestions));
            });
        }

        public void AddSuggestionToText(string suggestion)
        {
            var separators = new char[] { ' ', ',' };
            var parts = SearchText?.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(p => p.Trim())
                                   .ToList() ?? new List<string>();

            if (parts.Any())
                parts[parts.Count - 1] = suggestion;
            else
                parts.Add(suggestion);

            SearchText = string.Join(" ", parts);

            if (!string.IsNullOrWhiteSpace(UsedMedications))
                UsedMedications += " " + suggestion;
            else
                UsedMedications = suggestion;

            Suggestions.Clear();

            _ = SafeRunAsync(async () =>
            {
                await Task.Delay(50);
                MainThread.BeginInvokeOnMainThread(() => SearchEntryReference?.Focus());
            });
        }

        public async void FilterMedications(string searchText, PrescriptionDrugModel drug)
        {
            await LoadMedicationsAsync();

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(100, _cts.Token);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    drug.FilteredMedications.Clear();

                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        drug.ShowSuggestions = false;
                        return;
                    }

                    var filtered = AllMedications
                        .Where(m => m.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Take(10)
                        .ToList();

                    foreach (var med in filtered)
                    {
                        drug.FilteredMedications.Add(med);
                    }

                    drug.ShowSuggestions = drug.FilteredMedications.Any();

                    OnPropertyChanged(nameof(drug.FilteredMedications));
                    OnPropertyChanged(nameof(drug.ShowSuggestions));
                });
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error filtering medications: {ex.Message}");

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    drug.ShowSuggestions = false;
                });
            }
        }

        [RelayCommand]
        private async Task ImportDataAsync()
        {
            ReceiptsPath = Preferences.Get("ReceiptPath", string.Empty);
            PrescriptionsPath = Preferences.Get("PrescriptionPath", string.Empty);

            if (string.IsNullOrWhiteSpace(ReceiptsPath) || string.IsNullOrWhiteSpace(PrescriptionsPath))
            {
                await ShowAlert("Configurați calea directoarelor!");
                return;
            }

            try
            {
                string lastFolder = ReceiptImportService.GetLastDatedFolder(ReceiptsPath);
                if (lastFolder == null)
                {
                    await ShowAlert("Nu există fișier de bon de importat.");
                    return;
                }

                string logFile = ReceiptImportService.FindTextOrLogFile(lastFolder);
                if (logFile == null)
                {
                    await ShowAlert("Nu există fișier de bon de importat.");
                    return;
                }

                var medicationsWithPrescription = new List<PrescriptionDrugModel>();
                var medicationsWithoutPrescription = new List<ReceiptDrugModel>();
                PrescriptionImportModel patientInfo = null;

                if (ShowWithPrescription)
                {
                    string prescriptionFile = PrescriptionImportService.GetLastPrescriptionFile(PrescriptionsPath);
                    if (prescriptionFile == null)
                    {
                        await ShowAlert("Nu există fișier de prescripție de importat.");
                        return;
                    }

                    patientInfo = PrescriptionImportService.ImportFromXml(prescriptionFile);
                    PatientCnp = patientInfo.PatientCnp;
                    PrescriptionSeries = patientInfo.PrescriptionSeries;
                    PrescriptionNumber = patientInfo.PrescriptionNumber;
                    DoctorStamp = patientInfo.DoctorStamp;
                    PrescriptionDiagnosis = patientInfo.Diagnosis;

                    ImportDrugsFromLastCompensatedReceipt(logFile, medicationsWithPrescription);
                }
                else if (ShowWithoutPrescription)
                {
                    ImportDrugsFromLastTwoNonCompensatedReceipts(logFile, medicationsWithoutPrescription);
                }

                MedicationsWithPrescription.Clear();
                foreach (var drug in medicationsWithPrescription)
                    MedicationsWithPrescription.Add(drug);

                MedicationsWithoutPrescription.Clear();
                foreach (var drug in medicationsWithoutPrescription)
                    MedicationsWithoutPrescription.Add(drug);
            }
            catch (Exception ex)
            {
                await ShowAlert($"Eroare la import: {ex.Message}");
            }
        }

        private void ImportDrugsFromLastCompensatedReceipt(string logFilePath, List<PrescriptionDrugModel> medicationsWithPrescription)
        {
            var lines = File.ReadAllLines(logFilePath).ToList();
            var receipts = new List<List<string>>();
            var currentReceipt = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains("Adaug fisier in coada"))
                    currentReceipt = new List<string>();

                currentReceipt.Add(line);

                if (line.Contains("Bon fiscal inchis"))
                    receipts.Add(currentReceipt);
            }

            if (!receipts.Any()) return;

            var lastCompensated = receipts.LastOrDefault(r => r.Any(l => l.Contains("Compensat:")));
            if (lastCompensated == null) return;

            int index = 1;
            foreach (var line in lastCompensated.Where(l => l.Contains("Vanzare:")))
            {
                var idx = line.IndexOf("Vanzare:") + "Vanzare:".Length;
                var after = line.Substring(idx).Trim();
                var endIdx = after.IndexOf("->");
                var name = (endIdx > 0 ? after.Substring(0, endIdx) : after).Trim();

                medicationsWithPrescription.Add(new PrescriptionDrugModel
                {
                    Index = index++,
                    Name = name
                });
            }
        }



        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(PatientName) && string.IsNullOrWhiteSpace(CaregiverName))
            {
                await ShowAlert("Completează numele pacientului/aparținătorului!");
                return;
            }

            var pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
            Patient? patient = null;

            if (!string.IsNullOrWhiteSpace(CardNumber))
            {
                var card = await _pharmacyCardService.CreateCardAsync(
                    CardNumber, pharmacyId, PatientName ?? "-", "-", PatientCnp, null, PatientEmail, null, null, null);
                patient = card.Patient;
            }
            else if (!string.IsNullOrWhiteSpace(PatientName) || !string.IsNullOrWhiteSpace(PatientCnp))
            {
                var patientDto = new Patient
                {
                    FirstName = PatientName ?? "-",
                    LastName = "-",
                    Cnp = PatientCnp
                };
                patient = await _patientService.GetOrCreatePatientAsync(null, patientDto);
            }

            var prescription = new Prescription
            {
                PatientId = patient?.Id,
                PatientName = this.PatientName,
                PatientCnp = this.PatientCnp,
                CaregiverName = this.CaregiverName,
                CaregiverCnp = this.CaregiverCnp,
                Number = this.PrescriptionNumber,
                Series = this.PrescriptionSeries,
                Diagnostic = this.PrescriptionDiagnosis,
                DiagnosisMentionedByPatient = this.PatientDiagnosis,
                MedicamentsMentionedByPacient = this.UsedMedications,
                Symptoms = this.Symptoms,
                Suspicion = this.Suspicion,
                PharmacistObservations = this.PharmacistObservations,
                NotesToDoctor = this.NotesToDoctor,
                PharmacistRecommendation = this.PharmacistRecommendation,
                PharmaceuticalService = this.SelectedPharmaceuticalService,
                DoctorStamp = this.DoctorStamp,
                IssueDate = PrescriptionId > 0 && this.Prescription != null ? this.Prescription.IssueDate : DateTime.Now,
                PrescriptionMedications = this.MedicationsWithPrescription
                 .Select(m => new PrescriptionMedication
                 {
                     Name = m.Name,
                     Morning = m.Morning,
                     Noon = m.Noon,
                     Evening = m.Evening,
                     Night = m.Night,
                     AdministrationModeId = m.AdministrationMode?.Id ?? 1,
                     IsWithPrescription = true
                 })
                 .Concat(
                     this.MedicationsWithoutPrescription
                         .Select(m => new PrescriptionMedication
                         {
                             Name = m.Name,
                             Morning = m.Morning,
                             Noon = m.Noon,
                             Evening = m.Evening,
                             Night = m.Night,
                             AdministrationModeId = m.AdministrationMode?.Id ?? 1,
                             IsWithPrescription = false
                         })
                 )
                 .ToList()
            };

            int? oldPrescriptionId = PrescriptionId > 0 ? PrescriptionId : null;

            // Add the new prescription first. If this fails, the old one is untouched.
            await _prescriptionService.AddPrescriptionAsync(prescription);
            // Set backing field directly to avoid triggering OnPrescriptionIdChanged,
            // which would fire LoadPrescriptionAsync concurrently with DeletePrescriptionAsync below,
            // causing a concurrent DbContext operation crash.
            prescriptionId = prescription.Id;
            Prescription = prescription;

            // Only delete the old record after the new one is safely written.
            if (oldPrescriptionId.HasValue)
            {
                await _prescriptionService.DeletePrescriptionAsync(oldPrescriptionId.Value);
            }

            IsPrintButtonEnabled = true;
            await ShowAlert("Rețeta a fost salvată cu succes!");

            var pharmacy = await _pharmacyService.GetByIdAsync(pharmacyId);

            var exportDto = new PrescriptionExportDto
            {
                PharmacyName = pharmacy.Name,
                PharmacyAdress = pharmacy.Address,
                PharmacyCUI = pharmacy.CUI,
                PharmacyPhone = pharmacy.Phone,
                PharmacyEmail = pharmacy.Email,
                PacientCard = CardNumber,
                CardAderenta = "",
                RetetaType = showWithPrescription ? "Compensată" : "Necompensată",
                Note = pharmacistObservations,
                ConfirmareAdresareMedic = "Da",
                ConfirmareRidicareReteta = "Nu",
                MedicParafa = doctorStamp,
                MedicEmail = "",
                SerieReteta = prescriptionSeries,
                NrReteta = prescriptionNumber,
                Reteta = new List<MedicationExportDto>()
            };

            if (showWithPrescription)
            {
                exportDto.Reteta = MedicationsWithPrescription.Select(m => new MedicationExportDto
                {
                    MedicineName = m.Name,
                    MedicineMorning = m.Morning,
                    MedicineLunch = m.Noon,
                    MedicineEvening = m.Evening,
                    MedicineNight = m.Night,
                    MedicineAdministration = administrationModes
                        .FirstOrDefault(x => x.Id == m.AdministrationModeId)?.Name
                }).ToList();
            }
            else
            {
                exportDto.Reteta = MedicationsWithoutPrescription.Select(m => new MedicationExportDto
                {
                    MedicineName = m.Name,
                    MedicineMorning = m.Morning,
                    MedicineLunch = m.Noon,
                    MedicineEvening = m.Evening,
                    MedicineNight = m.Night,
                    MedicineAdministration = m.AdministrationMode?.ToString() ?? "Null"
                }).ToList();
            }
        }



        [RelayCommand]
        public async Task SelectMedicationAsync(string medName)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var drug in MedicationsWithPrescription)
                {
                    if (drug.ShowSuggestions && drug.FilteredMedications.Contains(medName))
                    {
                        drug.Name = medName;
                        drug.ShowSuggestions = false;
                        drug.FilteredMedications.Clear();
                        break;
                    }
                }
            });
        }

        [RelayCommand]
        private void AddMedicationWithPrescription()
        {
            MedicationsWithPrescription.Add(new PrescriptionDrugModel { Index = MedicationsWithPrescription.Count + 1 });
        }

        [RelayCommand]
        private void AddMedicationWithoutPrescription()
        {
            MedicationsWithoutPrescription.Add(new ReceiptDrugModel { Index = MedicationsWithoutPrescription.Count + 1 });
        }

        [RelayCommand]
        void DeleteMedicationWithPrescription(PrescriptionDrugModel item)
        {
            if (item != null && MedicationsWithPrescription.Contains(item))
            {
                MedicationsWithPrescription.Remove(item);
                for (int i = 0; i < MedicationsWithPrescription.Count; i++)
                {
                    MedicationsWithPrescription[i].Index = i + 1;
                }
            }
        }

        [RelayCommand]
        void DeleteMedicationWithoutPrescription(ReceiptDrugModel item)
        {
            if (item != null && MedicationsWithoutPrescription.Contains(item))
            {
                MedicationsWithoutPrescription.Remove(item);
                for (int i = 0; i < MedicationsWithoutPrescription.Count; i++)
                {
                    MedicationsWithoutPrescription[i].Index = i + 1;
                }
            }
        }

        [RelayCommand]
        private void NewRecommendation()
        {
            CardNumber = PatientName = PatientCnp = CaregiverName = CaregiverCnp = PatientDiagnosis = UsedMedications = "";
            DoctorStamp = PrescriptionSeries = PrescriptionNumber = PrescriptionDiagnosis = "";
            Symptoms = Suspicion = PharmacistObservations = NotesToDoctor = PharmacistRecommendation = "";
            MedicationsWithPrescription.Clear();
            MedicationsWithoutPrescription.Clear();
        }

        [RelayCommand]
        private async Task PrintAsync()
        {
            var _pharmacy = await _pharmacyService.GetByIdAsync(pharmacyId);

            var medsWithPrescription = MedicationsWithPrescription.Select(m => new ActPrintDocument.MedicationLine
            {
                Name = m.Name,
                Morning = m.Morning,
                Noon = m.Noon,
                Evening = m.Evening,
                Night = m.Night,
                AdministrationMode = m.AdministrationMode?.Name ?? ""
            }).ToList();

            var medsWithoutPrescription = MedicationsWithoutPrescription.Select(m => new ActPrintDocument.MedicationLine
            {
                Name = m.Name,
                Morning = m.Morning,
                Noon = m.Noon,
                Evening = m.Evening,
                Night = m.Night,
                AdministrationMode = m.AdministrationMode?.Name ?? ""
            }).ToList();

            var printDoc = new ActPrintDocument
            {
                PharmacyName = _pharmacy!.Name,
                PharmacyAddress = _pharmacy.Address!,
                PharmacyPhone = _pharmacy!.Phone!,
                Series = this.PrescriptionSeries!,
                Number = this.PrescriptionNumber!,
                IssueDate = DateTime.Now,
                PatientName = this.PatientName!,
                PatientCnp = this.PatientCnp!,
                PatientCard = this.CardNumber,
                CaregiverName = this.CaregiverName!,
                CaregiverCnp = this.CaregiverCnp!,
                ModeCode = mode switch
                {
                    "withprescription" => "AC",
                    "withoutprescription" => "AP",
                    "mixed" => "AM",
                    _ => "XX"
                },
                DoctorStamp = this.DoctorStamp!,
                Diagnostic = this.PrescriptionDiagnosis!,
                DiagnosisMentioned = this.PatientDiagnosis!,
                MedicationsMentioned = this.UsedMedications!,
                Symptoms = this.Symptoms!,
                Suspicion = this.Suspicion!,
                PharmacistObservations = this.PharmacistObservations!,
                NotesToDoctor = this.NotesToDoctor!,
                PharmacistRecommendation = this.PharmacistRecommendation,
                PharmaceuticalService = this.SelectedPharmaceuticalService,
                MedicationsWithPrescription = medsWithPrescription,
                MedicationsWithoutPrescription = medsWithoutPrescription
            };

            var pharmacistUser = await _userService.GetEffectivePharmacistAsync(SessionManager.CurrentUser);

            if (SessionManager.CurrentUser?.Role?.ToLower() == "pharmacist")
            {
                // current user e farmacist => afișăm doar el
                printDoc.PharmacistNameEffective = $"{SessionManager.CurrentUser.FirstName} {SessionManager.CurrentUser.LastName}";
                printDoc.AssistantName = null;
            }
            else
            {
                // current user e asistent => afișăm primul farmacist + asistent
                printDoc.PharmacistNameEffective = $"{pharmacistUser?.FirstName ?? "-"} {pharmacistUser?.LastName ?? "-"}";
                printDoc.AssistantName = $"{SessionManager.CurrentUser?.FirstName ?? "-"} {SessionManager.CurrentUser?.LastName ?? "-"}";
            }


            var pd = new System.Windows.Forms.PrintDialog();
            pd.Document = printDoc;
            if (pd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                printDoc.Print();
            }
        }

        [RelayCommand]
        private async Task<byte[]> CreatePrescriptionPdf()
        {
            var _pharmacy = await _pharmacyService.GetByIdAsync(pharmacyId);

            var medsWithPrescription = MedicationsWithPrescription.Select(m => new ActPdfDocument.MedicationLine
            {
                Name = m.Name,
                Morning = m.Morning,
                Noon = m.Noon,
                Evening = m.Evening,
                Night = m.Night,
                AdministrationMode = m.AdministrationMode?.Name ?? ""
            }).ToList();

            var medsWithoutPrescription = MedicationsWithoutPrescription.Select(m => new ActPdfDocument.MedicationLine
            {
                Name = m.Name,
                Morning = m.Morning,
                Noon = m.Noon,
                Evening = m.Evening,
                Night = m.Night,
                AdministrationMode = m.AdministrationMode?.Name ?? ""
            }).ToList();

            var pdfDoc = new ActPdfDocument
            {
                PharmacyName = _pharmacy!.Name,
                PharmacyAddress = _pharmacy.Address!,
                PharmacyPhone = _pharmacy!.Phone!,
                Series = this.PrescriptionSeries!,
                Number = this.PrescriptionNumber!,
                IssueDate = DateTime.Now,
                PatientName = this.PatientName!,
                PatientCnp = this.PatientCnp!,
                PatientCard = this.CardNumber,
                CaregiverName = this.CaregiverName!,
                CaregiverCnp = this.CaregiverCnp!,
                ModeCode = mode switch
                {
                    "withprescription" => "AC",
                    "withoutprescription" => "AP",
                    "mixed" => "AM",
                    _ => "XX"
                },
                DoctorStamp = this.DoctorStamp!,
                Diagnostic = this.PrescriptionDiagnosis!,
                DiagnosisMentioned = this.PatientDiagnosis!,
                MedicationsMentioned = this.UsedMedications!,
                Symptoms = this.Symptoms!,
                Suspicion = this.Suspicion!,
                PharmacistObservations = this.PharmacistObservations!,
                NotesToDoctor = this.NotesToDoctor!,
                PharmacistRecommendation = this.PharmacistRecommendation,
                PharmaceuticalService = this.SelectedPharmaceuticalService,
                MedicationsWithPrescription = medsWithPrescription,
                MedicationsWithoutPrescription = medsWithoutPrescription
            };

            byte[] pdfBytes = pdfDoc.GeneratePdf();

            //string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Prescription.pdf");
            //await File.WriteAllBytesAsync(filePath, pdfBytes);

            return pdfBytes;
        }

        [RelayCommand]
        private async Task SendEmailAsync()
        {
            string patientEmail = PatientEmail?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(patientEmail) || !patientEmail.Contains("@"))
            {
                patientEmail = await Shell.Current.DisplayPromptAsync(
                    "Adresă e-mail",
                    $"Pacientul {PatientName} nu are e-mail salvat.\nIntroduceți adresa:",
                    "OK", "Renunță", "ex: ion.popescu@mail.com");

                if (string.IsNullOrWhiteSpace(patientEmail) || !patientEmail.Contains("@"))
                {
                    await Shell.Current.DisplayAlert("Anulat", "Email invalid sau trimiterea a fost anulată.", "OK");
                    return;
                }
            }

            var config = await _emailConfigurationService.GetByPharmacyIdAsync(pharmacyId);
            if (config == null || string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(config.Password))
            {
                await Shell.Current.DisplayAlert("Eroare", "Configurarea email nu este completă.", "OK");
                return;
            }

            var pharmacy = await _pharmacyService.GetByIdAsync(pharmacyId);

            byte[] pdfBytes = await CreatePrescriptionPdf();

            var exportDto = new PrescriptionExportDto
            {
                PharmacyName = pharmacy.Name,
                PharmacyAdress = pharmacy.Address,
                PharmacyCUI = pharmacy.CUI,
                PharmacyPhone = pharmacy.Phone,
                PharmacyEmail = pharmacy.Email,
                PacientCard = CardNumber,
                CardAderenta = "",
                RetetaType = ShowWithPrescription && ShowWithoutPrescription ? "Mixtă" :
                             ShowWithPrescription ? "Compensată" : "Necompensată",
                Note = pharmacistObservations,
                ConfirmareAdresareMedic = "Da",
                ConfirmareRidicareReteta = "Nu",
                MedicParafa = doctorStamp,
                MedicEmail = "",
                SerieReteta = prescriptionSeries,
                NrReteta = prescriptionNumber,
                Reteta = MedicationsWithPrescription
                         .Select(m => new MedicationExportDto
                         {
                             MedicineName = m.Name,
                             MedicineMorning = m.Morning,
                             MedicineLunch = m.Noon,
                             MedicineEvening = m.Evening,
                             MedicineNight = m.Night,
                             MedicineAdministration = m.AdministrationMode?.Name ?? "",
                             MedicineTotalQuantity = m.TotalQuantity,
                             TreatmentPeriod = m.TotalDays
                         })
                         .Concat(MedicationsWithoutPrescription.Select(m => new MedicationExportDto
                         {
                             MedicineName = m.Name,
                             MedicineMorning = m.Morning,
                             MedicineLunch = m.Noon,
                             MedicineEvening = m.Evening,
                             MedicineNight = m.Night,
                             MedicineAdministration = m.AdministrationMode?.Name ?? "",
                             MedicineTotalQuantity = m.TotalQuantity,
                             TreatmentPeriod = m.TotalDays
                         }))
                         .ToList()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string jsonContent = JsonSerializer.Serialize(exportDto, options);

            string actName;
            switch (exportDto.RetetaType)
            {
                case "Mixtă":
                    actName = "Act Mixt";
                    break;
                case "Compensată":
                    actName = "Act consecutiv prescriptiei";
                    break;
                default:
                    actName = "Act farmaceutic";
                    break;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(pharmacy.Name, config.Username));
            message.To.Add(MailboxAddress.Parse(patientEmail));
            message.Subject = $"{actName}  {PatientName}";

            var builder = new BodyBuilder
            {
                TextBody = $"Bună ziua,\n\nVă trimitem rețeta în format PDF și JSON.\n\nCu stimă,\n{pharmacy.Name}, {pharmacy.Address}"
            };

            builder.Attachments.Add($"{actName}.pdf", pdfBytes);
            builder.Attachments.Add($"{actName}.json", System.Text.Encoding.UTF8.GetBytes(jsonContent));

            message.Body = builder.ToMessageBody();

            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                client.Timeout = 60000;
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(config.Username, config.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                await Shell.Current.DisplayAlert("Succes", "Emailul a fost trimis cu succes.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare", $"Trimiterea eșuată: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        private async Task OpenMedicationPopup(PrescriptionDrugModel drug)
        {
            if (_allMedicationsCache == null || !_allMedicationsCache.Any())
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Se încarcă...",
                    "Vă rugăm să așteptați câteva secunde.",
                    "OK");
                return;
            }

            var filtered = _allMedicationsCache
                            .Where(m => string.IsNullOrWhiteSpace(drug.Name) || m.Contains(drug.Name, StringComparison.OrdinalIgnoreCase))
                            .Take(10)
                            .ToArray();


            if (!filtered.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Info", "Nu există medicamente care să corespundă.", "OK");
                return;
            }

            string selected = null;
            if (filtered.Length == 1)
            {
                selected = filtered.First();
            }
            else
            {
                selected = await Application.Current.MainPage.DisplayActionSheet(
                    "Alege medicament",
                    "Anulează",
                    null,
                    filtered);
            }

            if (!string.IsNullOrEmpty(selected) && selected != "Anulare")
                drug.Name = selected;
        }

        [RelayCommand]
        private async Task OpenMedicationPopupReceipt(ReceiptDrugModel drug)
        {
            if (_allMedicationsCache == null || !_allMedicationsCache.Any())
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Se încarcă...",
                    "Vă rugăm să așteptați câteva secunde.",
                    "OK");
                return;
            }

            var filtered = _allMedicationsCache
                            .Where(m => string.IsNullOrWhiteSpace(drug.Name) || m.Contains(drug.Name, StringComparison.OrdinalIgnoreCase))
                            .Take(10)
                            .ToArray();


            if (!filtered.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Info", "Nu există medicamente care să corespundă.", "OK");
                return;
            }

            string selected = null;
            if (filtered.Length == 1)
            {
                selected = filtered.First();
            }
            else
            {
                selected = await Application.Current.MainPage.DisplayActionSheet(
                    "Selectați medicamentul",
                    "Anulare",
                    null,
                    filtered);
            }

            if (!string.IsNullOrEmpty(selected) && selected != "Anulare")
              drug.Name = selected;
        }

       

        private void ImportDrugsFromLastTwoNonCompensatedReceipts(string logFilePath, List<ReceiptDrugModel> medicationsWithoutPrescription)
        {
            var lines = File.ReadAllLines(logFilePath).ToList();
            var receipts = new List<List<string>>();
            var currentReceipt = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains("Adaug fisier in coada"))
                    currentReceipt = new List<string>();

                currentReceipt.Add(line);

                if (line.Contains("Bon fiscal inchis"))
                    receipts.Add(currentReceipt);
            }

            if (!receipts.Any()) return;

            var nonCompensatedReceipts = receipts.Where(r => !r.Any(l => l.Contains("Compensat:"))).ToList();
            if (!nonCompensatedReceipts.Any()) return;

            var receiptToImport = nonCompensatedReceipts.Count >= 2
                ? nonCompensatedReceipts.Last()
                : nonCompensatedReceipts.Last();

            int index = 1;
            foreach (var line in receiptToImport.Where(l => l.Contains("Vanzare:")))
            {
                var idx = line.IndexOf("Vanzare:") + "Vanzare:".Length;
                var after = line.Substring(idx).Trim();
                var endIdx = after.IndexOf("->");
                var name = (endIdx > 0 ? after.Substring(0, endIdx) : after).Trim();

                medicationsWithoutPrescription.Add(new ReceiptDrugModel
                {
                    Index = index++,
                    Name = name
                });
            }
        }

        private Task ShowAlert(string message)
        {
            return Shell.Current?.DisplayAlert("Info", message, "OK") ?? Task.CompletedTask;
        }

        private async Task LoadAdministrationModes()
        {
            try
            {
                var modes = await _administrationModeService.GetAllAsync();
                var activeModes = modes?.Where(c => c?.IsActive == true).ToList() ?? new List<AdministrationMode>();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    AdministrationModes = new ObservableCollection<AdministrationMode>(activeModes);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la încărcarea modurilor: {ex}");
            }
        }

        //Reports methods:

        private async Task LoadPrescriptionAsync(int id)
        {
            try
            {
                var presc = await _prescriptionService.GetPrescriptionByIdAsync(id);

            if (presc == null)
            {
                await Shell.Current.DisplayAlert("Debug", $"No prescription found for id={id}", "OK");
                return;
            }
            if (presc != null)
            {
                Prescription = presc;

                MedicationsWithPrescription = new ObservableCollection<PrescriptionDrugModel>(
                    presc.PrescriptionMedications
                        .Where(m => m.IsWithPrescription == true)
                        .Select((m, index) => new PrescriptionDrugModel
                        {
                            Index = index + 1,
                            Name = m.Name,
                            Morning = m.Morning,
                            Noon = m.Noon,
                            Evening = m.Evening,
                            Night = m.Night,
                            AdministrationModeId = m.AdministrationModeId,
                        }));

                MedicationsWithoutPrescription = new ObservableCollection<ReceiptDrugModel>(
                    presc.PrescriptionMedications
                        .Where(m => m.IsWithPrescription == false)
                        .Select((m, index) => new ReceiptDrugModel
                        {
                            Index = index + 1,
                            Name = m.Name,
                            Morning = m.Morning,
                            Noon = m.Noon,
                            Evening = m.Evening,
                            Night = m.Night,
                        }));
                PatientName = presc.PatientName;
                PatientCnp = presc.PatientCnp;
                CardNumber = presc.Patient?.PharmacyCards?.FirstOrDefault()?.Code ?? string.Empty;
                CaregiverName = presc.CaregiverName;
                CaregiverCnp = presc.CaregiverCnp;
                PrescriptionNumber = presc.Number;
                PrescriptionSeries = presc.Series;
                PrescriptionDiagnosis = presc.Diagnostic;
                PatientDiagnosis = presc.DiagnosisMentionedByPatient;
                UsedMedications = presc.MedicamentsMentionedByPacient;
                SearchText = presc.MedicamentsMentionedByPacient;
                Symptoms = presc.Symptoms;
                Suspicion = presc.Suspicion;
                PharmacistObservations = presc.PharmacistObservations;
                NotesToDoctor = presc.NotesToDoctor;
                PharmacistRecommendation = presc.PharmacistRecommendation;
                SelectedPharmaceuticalService = presc.PharmaceuticalService;
                DoctorStamp = presc.DoctorStamp;

                IsReadOnly = true;
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPrescriptionAsync FAILED: {ex}");
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        [ObservableProperty]
        private bool isReadOnly;

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}