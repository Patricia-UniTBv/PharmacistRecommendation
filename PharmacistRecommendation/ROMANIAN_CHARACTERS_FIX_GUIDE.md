# Romanian Special Characters Restoration Guide

## ?? **Romanian Special Characters to Restore**

The following characters need to be restored throughout the application:

| Incorrect | Correct | Unicode |
|-----------|---------|---------|
| `a` | `?` | U+0103 |
| `A` | `?` | U+0102 |
| `a` | `â` | U+00E2 |
| `A` | `Â` | U+00C2 |
| `i` | `î` | U+00EE |
| `I` | `Î` | U+00CE |
| `s` | `?` | U+0219 |
| `S` | `?` | U+0218 |
| `t` | `?` | U+021B |
| `T` | `?` | U+021A |

## ?? **Common Romanian Words to Fix**

### **A. Authentication & Users**
- `Autentificare` ? needs no change
- `Utilizatori` ? needs no change
- `Parola` ? `Parola` (might need ? if "Parol?")
- `Confirmare` ? needs no change
- `Inregistrare` ? `Înregistrare`
- `Configurare` ? needs no change

### **B. Pharmacy Terms**
- `Farmacie` ? needs no change
- `Farmacist` ? needs no change
- `Prescriptie` ? `Prescrip?ie`
- `Reteta` ? `Re?et?`
- `Medicament` ? needs no change
- `Pacient` ? needs no change
- `Diagnostic` ? needs no change

### **C. Common UI Text**
- `Salvare` ? `Salvare` or `Salveaz?`
- `Anulare` ? `Anulare` or `Anuleaz?`
- `Sterge` ? `?terge`
- `Adauga` ? `Adaug?`
- `Modifica` ? `Modific?`
- `Inchide` ? `Închide`
- `Cauta` ? `Caut?`
- `Actualizare` ? `Actualizare` or `Actualizeaz?`
- `Tip?ritur?` ? `Tip?re?te`

### **D. Messages**
- `Va rugam` ? `V? rug?m`
- `Nu s-a putut` ? `Nu s-a putut`
- `Eroare` ? needs no change
- `Succes` ? needs no change
- `Atentie` ? `Aten?ie`
- `Confirmare` ? needs no change
- `Introduceti` ? `Introduce?i`
- `Selectati` ? `Selecta?i`

### **E. Actions**
- `Testare` ? `Testare` or `Testeaz?`
- `Creare` ? `Creare` or `Creeaz?`
- `Salvare` ? `Salveaz?`
- `Printare` ? `Tip?re?te`
- `Finalizare` ? `Finalizare` or `Finalizeaz?`

### **F. Database/Fields**
- `Denumire` ? needs no change
- `Concentratie` ? `Concentra?ie`
- `Valabilitate` ? needs no change
- `Actiune terapeutica` ? `Ac?iune terapeutic?`
- `Firma producatoare` ? `Firm? produc?toare`

## ?? **Files That Need Attention**

### **High Priority (User-Facing)**

1. **Setup Wizards**
   - `ServerSetupWizard.xaml` ? CRITICAL
   - `ServerSetupWizardViewModel.cs` ? CRITICAL
   - `ClientSetupWizard.xaml` ? CRITICAL
   - `ClientSetupWizardViewModel.cs` ? CRITICAL

2. **Login/Authentication**
   - `LoginView.xaml`
   - `LoginViewModel.cs`
   - `LoginAddUserView.xaml`

3. **Main Views**
   - `CardConfigurationView.xaml`
   - `CardConfigurationViewModel.cs`
   - `MixedActIssuanceView.xaml`
   - `MixedActIssuanceViewModel.cs`
   - `MonitoringView.xaml`
   - `MonitoringViewModel.cs`

4. **Configuration Views**
   - `GdprConfigurationView.xaml`
   - `EmailConfigurationView.xaml`
   - `ImportConfigurationView.xaml`
   - `ServerConfigurationView.xaml`
   - `AddPharmacyView.xaml`

5. **User Management**
   - `UsersManagementView.xaml`
   - `UsersManagementViewModel.cs`
   - `PharmacistConfigurationView.xaml`

6. **Medication Management**
   - `MedicationView.xaml`
   - `MedicationViewModel.cs`
   - `AddEditMedicationView.xaml`
   - `AddEditMedicationViewModel.cs`

7. **Reports**
 - `ReportsView.xaml`
   - `ReportsViewModel.cs`

### **Medium Priority (Backend/Services)**

8. **Services**
   - `PharmacyService.cs`
   - `UserService.cs`
   - `EmailConfigurationService.cs`
   - `MedicationService.cs`

9. **Helpers**
   - `ActPrintDocument.cs` (for printing)
   - `ActPdfDocument.cs` (for PDF generation)

10. **Database Entities**
    - `Medication.cs`
    - `Prescription.cs`
    - `Patient.cs`

## ??? **Manual Fixes Required**

Since automated replacement is risky (might replace wrong instances), here's the priority order:

### **Phase 1: Setup Wizards (URGENT - First Run Experience)**

#### `ServerSetupWizard.xaml`
```xml
<!-- OLD -->
<Label Text="Bine ati venit!" />
<Label Text="Configurare Server Farmacie" />
<Label Text="Pregatim serverul dumneavoastra" />
<Label Text="Locatia: localhost\SQLEXPRESS" />
<Label Text="Baza de date: PharmacistRecommendationDB" />
<Label Text="Procesul dureaza aproximativ 2-3 minute." />
<Button Text="Creare Baza de Date" />
<Label Text="Va rugam sa asteptati... Nu inchideti aplicatia." />
<Button Text="Finalizare" />

<!-- NEW -->
<Label Text="Bine a?i venit!" />
<Label Text="Configurare Server Farmacie" />
<Label Text="Preg?tim serverul dumneavoastr?" />
<Label Text="Loca?ia: localhost\SQLEXPRESS" />
<Label Text="Baza de date: PharmacistRecommendationDB" />
<Label Text="Procesul dureaz? aproximativ 2-3 minute." />
<Button Text="Creare Baz? de Date" />
<Label Text="V? rug?m s? a?tepta?i... Nu închide?i aplica?ia." />
<Button Text="Finalizare" />
```

#### `ServerSetupWizardViewModel.cs`
```csharp
// OLD
ProgressMessage = "Verificare fisier baza de date...";
ProgressMessage = "Conectare la SQL Server...";
ProgressMessage = "Pregatire baza de date...";
ProgressMessage = "Se creaza baza de date... (acest proces poate dura cateva minute)";
ProgressMessage = "Verificare baza de date...";
ProgressMessage = "Finalizare configurare...";
StatusMessage = "Baza de date a fost creata cu succes!\n\nServerul dumneavoastra este gata de utilizare.";

// NEW
ProgressMessage = "Verificare fi?ier baz? de date...";
ProgressMessage = "Conectare la SQL Server...";
ProgressMessage = "Preg?tire baz? de date...";
ProgressMessage = "Se creeaz? baza de date... (acest proces poate dura câteva minute)";
ProgressMessage = "Verificare baz? de date...";
ProgressMessage = "Finalizare configurare...";
StatusMessage = "Baza de date a fost creat? cu succes!\n\nServerul dumneavoastr? este gata de utilizare.";
```

### **Phase 2: Client Setup Wizard**

#### `ClientSetupWizard.xaml`
```xml
<!-- Similar fixes as ServerSetupWizard.xaml -->
<Label Text="Conectare la Serverul Farmaciei" />
<Label Text="Adresa Server" />
<Label Text="Parol? pentru utilizator appuser" />
<Button Text="Testeaz? Conexiunea" />
<Label Text="Conexiune reu?it?!" />
```

### **Phase 3: Main Application Views**

Use Find & Replace with regex in Visual Studio:

**Find:** `(Va rugam|Rugam)`  
**Replace:** `V? rug?m`

**Find:** `(Sterge|sterge)`  
**Replace:** `?terge`

**Find:** `(Adauga|adauga)`  
**Replace:** `Adaug?`

**Find:** `(Modifica|modifica)`  
**Replace:** `Modific?`

**Find:** `(Prescriptie|prescriptie)`  
**Replace:** `Prescrip?ie`

**Find:** `(Reteta|reteta)`  
**Replace:** `Re?et?`

**Find:** `(Concentratie|concentratie)`  
**Replace:** `Concentra?ie`

## ?? **Visual Studio Find & Replace Settings**

1. Press `Ctrl+Shift+H` (Replace in Files)
2. **Look in:** Entire Solution
3. **File types:** `*.xaml;*.cs`
4. **Use:** Regular expressions ?
5. **Match case:** ?

## ?? **Important Notes**

1. **Backup First**: Commit all changes before starting
2. **Test After Each Phase**: Build and test the app
3. **Be Careful With**:
 - Database field names (might cause migration issues)
   - Code identifiers (variable names, class names)
   - Comments are OK to change
4. **Focus On**:
   - User-facing text (XAML files)
   - Display messages (ViewModels)
   - Print documents (ActPrintDocument.cs)
   - Alerts and notifications

## ?? **Checklist**

- [ ] Phase 1: Setup Wizards (ServerSetupWizard, ClientSetupWizard)
- [ ] Phase 2: Login & Authentication
- [ ] Phase 3: Main Views (Card Configuration, Mixed Act Issuance)
- [ ] Phase 4: Configuration Views
- [ ] Phase 5: User Management
- [ ] Phase 6: Medication Management
- [ ] Phase 7: Reports
- [ ] Phase 8: Services (user-facing messages)
- [ ] Phase 9: Print Documents
- [ ] Phase 10: Test entire application

## ?? **Priority Order**

1. **CRITICAL** - Setup Wizards (first impression)
2. **HIGH** - Login & Main application screens
3. **MEDIUM** - Configuration screens
4. **LOW** - Backend services (only user-facing messages)

## ?? **Common Replacements Reference**

```
ati ? a?i
dumneavoastra ? dumneavoastr?
rugam ? rug?m
asteptati ? a?tepta?i
inchideti ? închide?i
creaza ? creeaz?
dureaza ? dureaz?
verific ? verific?
conectare ? conectare (OK)
pregatire ? preg?tire
finalizare ? finalizare (OK)
salvare ? salvare / salveaz?
tip?re?te ? tip?re?te
?terge ? ?terge
adaug? ? adaug?
modific? ? modific?
```

This should be done manually file-by-file to ensure accuracy!
