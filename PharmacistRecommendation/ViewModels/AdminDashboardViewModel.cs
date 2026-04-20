using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DTO;
using Entities.Models;
using Entities.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace PharmacistRecommendation.ViewModels
{
    public partial class AdminDashboardViewModel : ObservableObject
    {
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;
        private readonly IPrescriptionService _prescriptionService;
        private readonly IMonitoringService _monitoringService;

        public AdminDashboardViewModel(
            IPatientService patientService,
            IUserService userService,
            IPrescriptionService prescriptionService,
            IMonitoringService monitoringService)
        {
            _patientService = patientService;
            _userService = userService;
            _prescriptionService = prescriptionService;
            _monitoringService = monitoringService;

            PrescriptionFromDate = DateTime.Today.AddMonths(-3);
            PrescriptionToDate = DateTime.Today;
            MonitoringFromDate = DateTime.Today.AddMonths(-3);
            MonitoringToDate = DateTime.Today;

            _ = LoadPatientsAsync();
        }

        // ── TABS ─────────────────────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPatientsVisible))]
        [NotifyPropertyChangedFor(nameof(IsUsersVisible))]
        [NotifyPropertyChangedFor(nameof(IsPrescriptionsVisible))]
        [NotifyPropertyChangedFor(nameof(IsMonitoringsVisible))]
        string activeTab = "patients";

        public bool IsPatientsVisible     => ActiveTab == "patients";
        public bool IsUsersVisible        => ActiveTab == "users";
        public bool IsPrescriptionsVisible => ActiveTab == "prescriptions";
        public bool IsMonitoringsVisible  => ActiveTab == "monitorings";

        [RelayCommand]
        private async Task SwitchTabAsync(string tab)
        {
            if (ActiveTab == tab) return;
            CloseAllEditPanels();
            ActiveTab = tab;
            switch (tab)
            {
                case "patients":      await LoadPatientsAsync();      break;
                case "users":         await LoadUsersAsync();         break;
                case "prescriptions": await LoadPrescriptionsAsync(); break;
                case "monitorings":   await LoadMonitoringsAsync();   break;
            }
        }

        private void CloseAllEditPanels()
        {
            IsPatientEditVisible      = false;
            IsUserEditVisible         = false;
            IsPrescriptionEditVisible = false;
            IsMonitoringEditVisible   = false;
        }

        // ── PATIENTS ──────────────────────────────────────────────────────────────

        private List<Patient> _allPatients = new();

        [ObservableProperty] ObservableCollection<Patient> patients = new();
        [ObservableProperty] string patientSearchText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyEditVisible))]
        bool isPatientEditVisible;
        private Patient? _editingPatient;
        [ObservableProperty] string editPatientFirstName = string.Empty;
        [ObservableProperty] string editPatientLastName  = string.Empty;
        [ObservableProperty] string editPatientCnp       = string.Empty;
        [ObservableProperty] string editPatientCid       = string.Empty;
        [ObservableProperty] string editPatientEmail     = string.Empty;
        [ObservableProperty] string editPatientPhone     = string.Empty;
        [ObservableProperty] string editPatientGender    = string.Empty;

        public string[] GenderOptions { get; } = { "", "Masculin", "Feminin" };

        public bool IsAnyEditVisible => IsPatientEditVisible || IsUserEditVisible || IsPrescriptionEditVisible || IsMonitoringEditVisible;

        private async Task LoadPatientsAsync()
        {
            try
            {
                _allPatients = await _patientService.GetAllPatientsAsync();
                FilterPatients();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load patients: {ex.Message}");
            }
        }

        private void FilterPatients()
        {
            var search = PatientSearchText?.Trim().ToLower() ?? string.Empty;
            var result = string.IsNullOrEmpty(search)
                ? _allPatients
                : _allPatients.Where(p =>
                    (p.FirstName?.ToLower().Contains(search) == true) ||
                    (p.LastName?.ToLower().Contains(search)  == true) ||
                    (p.Cnp?.Contains(search)                 == true) ||
                    (p.CardNumber?.Contains(search)          == true)).ToList();

            Patients = new ObservableCollection<Patient>(result);
        }

        partial void OnPatientSearchTextChanged(string value) => FilterPatients();

        [RelayCommand]
        private void EditPatient(Patient patient)
        {
            _editingPatient      = patient;
            EditPatientFirstName = patient.FirstName ?? string.Empty;
            EditPatientLastName  = patient.LastName  ?? string.Empty;
            EditPatientCnp       = patient.Cnp       ?? string.Empty;
            EditPatientCid       = patient.Cid       ?? string.Empty;
            EditPatientEmail     = patient.Email      ?? string.Empty;
            EditPatientPhone     = patient.Phone      ?? string.Empty;
            var rawGender = patient.Gender ?? string.Empty;
            EditPatientGender = GenderOptions.Contains(rawGender) ? rawGender : string.Empty;
            IsPatientEditVisible = true;
        }

        [RelayCommand]
        private async Task SavePatientAsync()
        {
            if (_editingPatient == null) return;

            var oldFirstName = _editingPatient.FirstName ?? string.Empty;
            var oldLastName  = _editingPatient.LastName  ?? string.Empty;

            _editingPatient.FirstName = EditPatientFirstName.Trim();
            _editingPatient.LastName  = EditPatientLastName.Trim();
            _editingPatient.Cnp       = string.IsNullOrWhiteSpace(EditPatientCnp)    ? null : EditPatientCnp.Trim();
            _editingPatient.Cid       = string.IsNullOrWhiteSpace(EditPatientCid)    ? null : EditPatientCid.Trim();
            _editingPatient.Email     = string.IsNullOrWhiteSpace(EditPatientEmail)  ? null : EditPatientEmail.Trim();
            _editingPatient.Phone     = string.IsNullOrWhiteSpace(EditPatientPhone)  ? null : EditPatientPhone.Trim();
            _editingPatient.Gender    = string.IsNullOrWhiteSpace(EditPatientGender) ? null : EditPatientGender.Trim();

            await _patientService.UpdatePatientAsync(_editingPatient);

            // Keep denormalized PatientName on prescriptions in sync if the name changed
            var nameChanged = _editingPatient.FirstName != oldFirstName || _editingPatient.LastName != oldLastName;
            if (nameChanged)
            {
                var newPatientName = $"{_editingPatient.FirstName} {_editingPatient.LastName}".Trim();
                await _prescriptionService.UpdatePatientNameOnPrescriptionsAsync(_editingPatient.Id, newPatientName);
            }

            IsPatientEditVisible = false;
            _editingPatient = null;
            await LoadPatientsAsync();
        }

        [RelayCommand]
        private void CancelPatientEdit()
        {
            IsPatientEditVisible = false;
            _editingPatient = null;
        }

        // ── USERS ─────────────────────────────────────────────────────────────────

        private List<UserDTO> _allUsers = new();

        [ObservableProperty] ObservableCollection<UserDTO> users = new();
        [ObservableProperty] string userSearchText          = string.Empty;
        [ObservableProperty] string selectedUserRoleFilter  = "Toți";

        public string[] UserRoleFilters { get; } = { "Toți", "Pharmacist", "Assistant", "Admin" };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyEditVisible))]
        bool isUserEditVisible;
        private UserDTO? _editingUser;
        [ObservableProperty] string editUserFirstName       = string.Empty;
        [ObservableProperty] string editUserLastName        = string.Empty;
        [ObservableProperty] string editUserUsername        = string.Empty;
        [ObservableProperty] string editUserEmail           = string.Empty;
        [ObservableProperty] string editUserPhone           = string.Empty;
        [ObservableProperty] string editUserNcm             = string.Empty;
        [ObservableProperty] string editUserNewPassword     = string.Empty;
        [ObservableProperty] string editUserConfirmPassword = string.Empty;

        private async Task LoadUsersAsync()
        {
            try
            {
                _allUsers = (await _userService.GetAllUsersAsync()).ToList();
                FilterUsers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load users: {ex.Message}");
            }
        }

        private void FilterUsers()
        {
            var search = UserSearchText?.Trim().ToLower() ?? string.Empty;
            var result = _allUsers.AsEnumerable();

            if (SelectedUserRoleFilter != "Toți")
                result = result.Where(u => u.Role == SelectedUserRoleFilter);

            if (!string.IsNullOrEmpty(search))
                result = result.Where(u =>
                    (u.FirstName?.ToLower().Contains(search) == true) ||
                    (u.LastName?.ToLower().Contains(search)  == true) ||
                    (u.Username?.ToLower().Contains(search)  == true));

            Users = new ObservableCollection<UserDTO>(result);
        }

        partial void OnUserSearchTextChanged(string value)           => FilterUsers();
        partial void OnSelectedUserRoleFilterChanged(string value)   => FilterUsers();

        [RelayCommand]
        private void EditUser(UserDTO user)
        {
            _editingUser             = user;
            EditUserFirstName        = user.FirstName ?? string.Empty;
            EditUserLastName         = user.LastName  ?? string.Empty;
            EditUserUsername         = user.Username  ?? string.Empty;
            EditUserEmail            = user.Email     ?? string.Empty;
            EditUserPhone            = user.Phone     ?? string.Empty;
            EditUserNcm              = user.Ncm       ?? string.Empty;
            EditUserNewPassword      = string.Empty;
            EditUserConfirmPassword  = string.Empty;
            IsUserEditVisible        = true;
        }

        [RelayCommand]
        private async Task SaveUserAsync()
        {
            if (_editingUser == null) return;

            var newUsername = EditUserUsername.Trim();
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                await Shell.Current.DisplayAlert("Eroare", "Username-ul nu poate fi gol.", "OK");
                return;
            }
            if (await _userService.IsUsernameTakenAsync(newUsername, _editingUser.Id))
            {
                await Shell.Current.DisplayAlert("Eroare", $"Username-ul \"{newUsername}\" este deja folosit de alt utilizator.", "OK");
                return;
            }

            if (!string.IsNullOrWhiteSpace(EditUserNewPassword))
            {
                if (EditUserNewPassword != EditUserConfirmPassword)
                {
                    await Shell.Current.DisplayAlert("Eroare", "Parolele nu coincid.", "OK");
                    return;
                }
                if (EditUserNewPassword.Length < 6)
                {
                    await Shell.Current.DisplayAlert("Eroare", "Parola trebuie să aibă cel puțin 6 caractere.", "OK");
                    return;
                }
                _editingUser.Password = EditUserNewPassword;
            }
            else
            {
                _editingUser.Password = null;
            }

            _editingUser.FirstName = EditUserFirstName.Trim();
            _editingUser.LastName  = EditUserLastName.Trim();
            _editingUser.Username  = EditUserUsername.Trim();
            _editingUser.Email     = EditUserEmail.Trim();
            _editingUser.Phone     = EditUserPhone.Trim();
            _editingUser.Ncm       = EditUserNcm.Trim();

            await _userService.UpdateUserAsync(_editingUser);
            IsUserEditVisible = false;
            _editingUser = null;
            await LoadUsersAsync();
        }

        [RelayCommand]
        private void CancelUserEdit()
        {
            IsUserEditVisible = false;
            _editingUser = null;
        }

        // ── PRESCRIPTIONS ─────────────────────────────────────────────────────────

        private List<Prescription> _allPrescriptions = new();

        [ObservableProperty] ObservableCollection<Prescription> prescriptions = new();
        [ObservableProperty] string   prescriptionSearchText = string.Empty;
        [ObservableProperty] DateTime prescriptionFromDate;
        [ObservableProperty] DateTime prescriptionToDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyEditVisible))]
        bool isPrescriptionEditVisible;
        private Prescription? _editingPrescription;
        [ObservableProperty] string editPrescriptionDiagnosis      = string.Empty;
        [ObservableProperty] string editPrescriptionObservations   = string.Empty;
        [ObservableProperty] string editPrescriptionRecommendation = string.Empty;

        private async Task LoadPrescriptionsAsync()
        {
            try
            {
                _allPrescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
                FilterPrescriptions();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load prescriptions: {ex.Message}");
            }
        }

        private void FilterPrescriptions()
        {
            var search = PrescriptionSearchText?.Trim().ToLower() ?? string.Empty;
            var result = _allPrescriptions
                .Where(p => p.IssueDate.Date >= PrescriptionFromDate.Date &&
                            p.IssueDate.Date <= PrescriptionToDate.Date);

            if (!string.IsNullOrEmpty(search))
                result = result.Where(p =>
                    (p.PatientName?.ToLower().Contains(search) == true) ||
                    (p.PatientCnp?.Contains(search)            == true) ||
                    (p.Number?.Contains(search)                == true) ||
                    (p.Series?.Contains(search)                == true));

            Prescriptions = new ObservableCollection<Prescription>(
                result.OrderByDescending(p => p.IssueDate));
        }

        partial void OnPrescriptionSearchTextChanged(string value)  => FilterPrescriptions();
        partial void OnPrescriptionFromDateChanged(DateTime value)   => FilterPrescriptions();
        partial void OnPrescriptionToDateChanged(DateTime value)     => FilterPrescriptions();

        [RelayCommand]
        private void EditPrescription(Prescription prescription)
        {
            _editingPrescription             = prescription;
            EditPrescriptionDiagnosis        = prescription.Diagnostic                ?? string.Empty;
            EditPrescriptionObservations     = prescription.PharmacistObservations    ?? string.Empty;
            EditPrescriptionRecommendation   = prescription.PharmacistRecommendation  ?? string.Empty;
            IsPrescriptionEditVisible        = true;
        }

        [RelayCommand]
        private async Task SavePrescriptionAsync()
        {
            if (_editingPrescription == null) return;
            _editingPrescription.Diagnostic               = EditPrescriptionDiagnosis.Trim();
            _editingPrescription.PharmacistObservations   = EditPrescriptionObservations.Trim();
            _editingPrescription.PharmacistRecommendation = EditPrescriptionRecommendation.Trim();

            await _prescriptionService.UpdatePrescriptionAsync(_editingPrescription);
            IsPrescriptionEditVisible = false;
            _editingPrescription = null;
            await LoadPrescriptionsAsync();
        }

        [RelayCommand]
        private void CancelPrescriptionEdit()
        {
            IsPrescriptionEditVisible = false;
            _editingPrescription = null;
        }

        // ── MONITORING ────────────────────────────────────────────────────────────

        private List<Monitoring> _allMonitorings = new();

        [ObservableProperty] ObservableCollection<Monitoring> monitorings = new();
        [ObservableProperty] string   monitoringSearchText         = string.Empty;
        [ObservableProperty] string   selectedMonitoringTypeFilter = "Toate";
        [ObservableProperty] DateTime monitoringFromDate;
        [ObservableProperty] DateTime monitoringToDate;

        public string[] MonitoringTypeFilters { get; } = { "Toate", "cardio", "diabetes", "temperature" };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAnyEditVisible))]
        bool isMonitoringEditVisible;
        private Monitoring? _editingMonitoring;
        [ObservableProperty] string   editMonitoringNotes  = string.Empty;
        [ObservableProperty] decimal? editMonitoringHeight;
        [ObservableProperty] decimal? editMonitoringWeight;

        private async Task LoadMonitoringsAsync()
        {
            try
            {
                _allMonitorings = await _monitoringService.GetAllMonitoringsAsync();
                FilterMonitorings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load monitorings: {ex.Message}");
            }
        }

        private void FilterMonitorings()
        {
            var search = MonitoringSearchText?.Trim().ToLower() ?? string.Empty;
            var result = _allMonitorings
                .Where(m => m.MonitoringDate.Date >= MonitoringFromDate.Date &&
                            m.MonitoringDate.Date <= MonitoringToDate.Date);

            if (!string.IsNullOrEmpty(search))
                result = result.Where(m =>
                    (m.Patient?.FirstName?.ToLower().Contains(search) == true) ||
                    (m.Patient?.LastName?.ToLower().Contains(search)  == true) ||
                    (m.Patient?.Cnp?.Contains(search)                 == true));

            if (SelectedMonitoringTypeFilter != "Toate")
                result = result.Where(m => GetMonitoringType(m) == SelectedMonitoringTypeFilter);

            Monitorings = new ObservableCollection<Monitoring>(
                result.OrderByDescending(m => m.MonitoringDate));
        }

        partial void OnMonitoringSearchTextChanged(string value)          => FilterMonitorings();
        partial void OnSelectedMonitoringTypeFilterChanged(string value)   => FilterMonitorings();
        partial void OnMonitoringFromDateChanged(DateTime value)           => FilterMonitorings();
        partial void OnMonitoringToDateChanged(DateTime value)             => FilterMonitorings();

        [RelayCommand]
        private void EditMonitoring(Monitoring monitoring)
        {
            _editingMonitoring     = monitoring;
            EditMonitoringNotes    = monitoring.Notes  ?? string.Empty;
            EditMonitoringHeight   = monitoring.Height;
            EditMonitoringWeight   = monitoring.Weight;
            IsMonitoringEditVisible = true;
        }

        [RelayCommand]
        private async Task SaveMonitoringAsync()
        {
            if (_editingMonitoring == null) return;
            await _monitoringService.UpdateMonitoringBasicAsync(
                _editingMonitoring.Id,
                string.IsNullOrWhiteSpace(EditMonitoringNotes) ? null : EditMonitoringNotes.Trim(),
                EditMonitoringHeight,
                EditMonitoringWeight);

            IsMonitoringEditVisible = false;
            _editingMonitoring = null;
            await LoadMonitoringsAsync();
        }

        [RelayCommand]
        private void CancelMonitoringEdit()
        {
            IsMonitoringEditVisible = false;
            _editingMonitoring = null;
        }

        // ── EXPORT ────────────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task ExportToExcelAsync()
        {
            try
            {
                var patients      = await _patientService.GetAllPatientsAsync();
                var users         = (await _userService.GetAllUsersAsync()).ToList();
                var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync();
                var monitorings   = await _monitoringService.GetAllMonitoringsAsync();

                using var workbook = new XLWorkbook();

                // ── Sheet 1: Pacienți ─────────────────────────────────────────
                var ws1 = workbook.Worksheets.Add("Pacienți");
                string[] patientHeaders = { "Nume", "Prenume", "CNP", "CID", "Nr. Card", "Email", "Telefon", "Gen" };
                for (int c = 0; c < patientHeaders.Length; c++)
                    ws1.Cell(1, c + 1).Value = patientHeaders[c];
                for (int i = 0; i < patients.Count; i++)
                {
                    var p = patients[i];
                    ws1.Cell(i + 2, 1).Value = p.LastName;
                    ws1.Cell(i + 2, 2).Value = p.FirstName;
                    ws1.Cell(i + 2, 3).Value = p.Cnp;
                    ws1.Cell(i + 2, 4).Value = p.Cid;
                    ws1.Cell(i + 2, 5).Value = p.CardNumber;
                    ws1.Cell(i + 2, 6).Value = p.Email;
                    ws1.Cell(i + 2, 7).Value = p.Phone;
                    ws1.Cell(i + 2, 8).Value = p.Gender;
                }

                // ── Sheet 2: Utilizatori ──────────────────────────────────────
                var ws2 = workbook.Worksheets.Add("Utilizatori");
                string[] userHeaders = { "Nume", "Prenume", "Username", "Email", "Telefon", "Rol", "NCM" };
                for (int c = 0; c < userHeaders.Length; c++)
                    ws2.Cell(1, c + 1).Value = userHeaders[c];
                for (int i = 0; i < users.Count; i++)
                {
                    var u = users[i];
                    ws2.Cell(i + 2, 1).Value = u.LastName;
                    ws2.Cell(i + 2, 2).Value = u.FirstName;
                    ws2.Cell(i + 2, 3).Value = u.Username;
                    ws2.Cell(i + 2, 4).Value = u.Email;
                    ws2.Cell(i + 2, 5).Value = u.Phone;
                    ws2.Cell(i + 2, 6).Value = u.Role;
                    ws2.Cell(i + 2, 7).Value = u.Ncm;
                }

                // ── Sheet 3: Rețete / Acte ────────────────────────────────────
                var ws3 = workbook.Worksheets.Add("Rețete");
                string[] prescriptionHeaders = { "Data", "Pacient", "CNP Pacient", "Serie", "Nr.", "Serviciu farmaceutic", "Diagnostic", "Observații farmacist", "Recomandare farmacist" };
                for (int c = 0; c < prescriptionHeaders.Length; c++)
                    ws3.Cell(1, c + 1).Value = prescriptionHeaders[c];
                for (int i = 0; i < prescriptions.Count; i++)
                {
                    var pr = prescriptions[i];
                    ws3.Cell(i + 2, 1).Value = pr.IssueDate.ToString("dd.MM.yyyy");
                    ws3.Cell(i + 2, 2).Value = pr.PatientName;
                    ws3.Cell(i + 2, 3).Value = pr.PatientCnp;
                    ws3.Cell(i + 2, 4).Value = pr.Series;
                    ws3.Cell(i + 2, 5).Value = pr.Number;
                    ws3.Cell(i + 2, 6).Value = pr.PharmaceuticalService;
                    ws3.Cell(i + 2, 7).Value = pr.Diagnostic;
                    ws3.Cell(i + 2, 8).Value = pr.PharmacistObservations;
                    ws3.Cell(i + 2, 9).Value = pr.PharmacistRecommendation;
                }

                // ── Sheet 4: Monitorizări ─────────────────────────────────────
                var ws4 = workbook.Worksheets.Add("Monitorizări");
                string[] monitoringHeaders = { "Data", "Pacient", "CNP", "Tip", "Înălțime (cm)", "Greutate (kg)", "Note" };
                for (int c = 0; c < monitoringHeaders.Length; c++)
                    ws4.Cell(1, c + 1).Value = monitoringHeaders[c];
                for (int i = 0; i < monitorings.Count; i++)
                {
                    var m = monitorings[i];
                    ws4.Cell(i + 2, 1).Value = m.MonitoringDate.ToString("dd.MM.yyyy");
                    ws4.Cell(i + 2, 2).Value = $"{m.Patient?.LastName} {m.Patient?.FirstName}".Trim();
                    ws4.Cell(i + 2, 3).Value = m.Patient?.Cnp;
                    ws4.Cell(i + 2, 4).Value = GetMonitoringType(m);
                    if (m.Height.HasValue) ws4.Cell(i + 2, 5).Value = (double)m.Height.Value;
                    if (m.Weight.HasValue) ws4.Cell(i + 2, 6).Value = (double)m.Weight.Value;
                    ws4.Cell(i + 2, 7).Value = m.Notes;
                }

                // ── Style header rows ─────────────────────────────────────────
                foreach (var ws in workbook.Worksheets)
                {
                    var headerRow = ws.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#2D6CB0");
                    headerRow.Style.Font.FontColor = XLColor.White;
                    ws.Columns().AdjustToContents(1, patients.Count + 2);
                }

                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "RecomandareaFarmacistului");
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                workbook.SaveAs(filePath);

                await Shell.Current.DisplayAlert("Export reușit",
                    $"Fișierul a fost salvat la:\n{filePath}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Eroare",
                    $"Exportul nu a putut fi efectuat: {ex.Message}", "OK");
            }
        }

        // ── HELPERS ───────────────────────────────────────────────────────────────

        public static string GetMonitoringType(Monitoring m)
        {
            if (string.IsNullOrWhiteSpace(m.ParametersJson)) return "—";
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(m.ParametersJson);
                if (dict == null) return "—";
                if (dict.ContainsKey("MaxBloodPressure") || dict.ContainsKey("HeartRate")) return "cardio";
                if (dict.ContainsKey("BloodGlucose"))                                      return "diabetes";
                if (dict.ContainsKey("BodyTemperature"))                                   return "temperature";
            }
            catch { /* malformed JSON */ }
            return "—";
        }
    }
}
