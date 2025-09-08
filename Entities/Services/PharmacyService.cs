using Entities.Models;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;

namespace Entities.Services
{
    public class PharmacyService : IPharmacyService
    {
        string Safe(string s) => string.IsNullOrWhiteSpace(s) ? "-" : s;
        private const string DEFAULT_CONSENT_TEMPLATE =
@"Declar că sunt de acord ca societatea {PharmacyName}, cu sediul în {PharmacyAddress}, având cod fiscal: {PharmacyFiscalCode} (denumită în continuare “Unitate Farmaceutică”) să fie autorizată să proceseze datele mele personale introduse în formularul de înregistrare client, precum și datele care sunt colectate în cadrul tranzacțiilor comerciale cu Unitate Farmaceutică în următoarele scopuri: generarea cardului, furnizarea de informații referitoare la campanii de marketing, oferte speciale, și/sau alte forme de publicitate, prin intermediul e-mail-ului, SMS-ului, telefonului, precum și contactarea în vederea desfășurării de sondaje de opinie a clienților.

Consimțământul dat prin prezenta declarație, în ceea ce privește prelucrarea datelor cu caracter personal, precum și furnizarea datelor menționate mai sus sunt voluntare.

Acest consimțământ poate fi revocat în orice moment, cu efect ulterior, printr-o notificare gratuită către Unitatea Farmaceutică.

Notificarea de revocare a consimțământului poate fi transmisă prin e-mail, sau direct la sediul Unității Farmaceutice.

Vă rugăm să aveți în vedere faptul că revocarea consimțământului nu afectează legalitatea utilizării datelor înainte de retragerea consimțământului.

Dacă consimțământul nu este acordat sau a fost revocat, datele personale nu vor fi utilizate în scopurile de mai sus."
.Replace("{PharmacyName}", Safe(_pharmacy.Name))
.Replace("{PharmacyAddress}", Safe(_pharmacy.Address))
.Replace("{PharmacyFiscalCode}", Safe(_pharmacy.CUI));


        private readonly IPharmacyRepository _repository;

        public PharmacyService(IPharmacyRepository repository)
        {
            _repository = repository;
        }
        public async Task<Pharmacy?> GetByIdAsync(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task AddPharmacyAsync(Pharmacy pharmacy)
        {
            pharmacy.ConsentTemplate = DEFAULT_CONSENT_TEMPLATE;
            await _repository.AddAsync(pharmacy);
        }

        public async Task<string> GetConsentTemplateAsync(int pharmacyId)
        {
            var pharmacy = await _repository.GetById(pharmacyId);
            if (pharmacy == null)
                throw new Exception("Farmacia nu există!");

            var template = string.IsNullOrWhiteSpace(pharmacy.ConsentTemplate)
                ? DEFAULT_CONSENT_TEMPLATE
                : pharmacy.ConsentTemplate;

            return FormatConsentTemplate(template, pharmacy);
        }

        public async Task UpdateConsentTemplateAsync(int pharmacyId, string newTemplate)
        {
            var pharmacy = await _repository.GetById(pharmacyId);
            if (pharmacy == null) throw new Exception("Farmacia nu există!");
            pharmacy.ConsentTemplate = newTemplate;
            await _repository.UpdatePharmacyAsync(pharmacy);
        }

        public async Task ResetConsentTemplateAsync(int pharmacyId)
        {
            var pharmacy = await _repository.GetById(pharmacyId);
            if (pharmacy == null)
                throw new Exception("Farmacia nu există!");

            pharmacy.ConsentTemplate = DEFAULT_CONSENT_TEMPLATE;
            await _repository.UpdatePharmacyAsync(pharmacy);
        }

        public static string FormatConsentTemplate(string template, Pharmacy pharmacy)
        {
            return template
                .Replace("{PharmacyName}", pharmacy.Name)
                .Replace("{PharmacyAddress}", pharmacy.Address ?? "")
                .Replace("{PharmacyFiscalCode}", pharmacy.CUI ?? "");
        }

        public async Task<bool> HasAnyPharmacyAsync()
        {
            return await _repository.HasAnyPharmacyAsync();
        }

        public async Task<int> GetPharmacyId()
        {
            return await _repository.GetPharmacyId();
        }
    }
}
