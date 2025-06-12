using DTO;
using Entities.Data;
using Entities.Helpers;
using Entities.Models;
using Entities.Parameters;
using Entities.Repository.Interfaces;
using Entities.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entities.Services
{
    public class MonitoringService: IMonitoringService
    {
        private readonly IMonitoringRepository _repo;
        public MonitoringService(IMonitoringRepository repo) => _repo = repo;
        public async Task<int> AddMonitoringAsync(MonitoringDTO dto, int loggedInUserId)
        {
            var entity = new Monitoring
            {
                PatientId = dto.PatientId,
                CardId = dto.CardId,
                MonitoringDate = dto.MonitoringDate,
                Height = dto.Height,
                Weight = dto.Weight,
                Notes = dto.Notes,
                ParametersJson = JsonSerializer.Serialize(new
                {
                    dto.MaxBloodPressure,
                    dto.MinBloodPressure,
                    dto.HeartRate,
                    dto.PulseOximetry,
                    dto.BloodGlucose,
                    dto.BodyTemperature
                }),
               // UserId = loggedInUserId           
            };

            return await _repo.AddAsync(entity);
        }

        public async Task<IEnumerable<HistoryRowDto>> GetHistoryAsync(
        int patientId, DateTime from, DateTime to)
        {
            var entities = await _repo.GetByPatientAndRangeAsync(patientId, from, to);

            var rows = new List<HistoryRowDto>();

            foreach (var m in entities)
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(m.ParametersJson!)
                           ?? new Dictionary<string, JsonElement>();

                rows.Add(new HistoryRowDto
                {
                    Date = m.MonitoringDate,
                    Height = m.Height,
                    Weight = m.Weight,
                    MaxBloodPressure = dict.GetIntOrNull("MaxBloodPressure"),
                    MinBloodPressure = dict.GetIntOrNull("MinBloodPressure"),
                    HeartRate = dict.GetIntOrNull("HeartRate"),
                    PulseOximetry = dict.GetIntOrNull("PulseOximetry"),
                    BloodGlucose = dict.GetDecimalOrNull("BloodGlucose"),
                    BodyTemperature = dict.GetDecimalOrNull("BodyTemperature")
                });
            }

            return rows;
        }

        // public async Task<IEnumerable<CardioMonitoringDTO>> GetCardioAsync(
        // int patientId, DateTime start, DateTime end)
        // {
        //     var list = await _repo.GetByPatientAndRangeAsync(patientId, start, end, "cardio");
        //     return list.Select(m =>
        //     {
        //         var p = JsonSerializer.Deserialize<CardioParams>(m.ParametersJson!)!;
        //         return new CardioMonitoringDTO(
        //             m.MonitoringDate, p.MaxBloodPressure, p.MinBloodPressure,
        //             p.HeartRate, p.PulseOximetry, m.Height, m.Weight);
        //     });
        // }

        // public async Task<IEnumerable<DiabetesMonitoringDTO>> GetDiabetesAsync(
        //int patientId, DateTime start, DateTime end)
        // {
        //     var list = await _repo.GetByPatientAndRangeAsync(patientId, start, end, "diabetes");
        //     return list.Select(m =>
        //     {
        //         var p = JsonSerializer.Deserialize<DiabetesParams>(m.ParametersJson!)!;
        //         return new DiabetesMonitoringDTO(
        //             m.MonitoringDate, p.BloodGlucose, m.Height, m.Weight);
        //     });
        // }

        // public async Task<IEnumerable<TemperatureMonitoringDTO>> GetTemperatureAsync(
        //     int patientId, DateTime start, DateTime end)
        // {
        //     var list = await _repo.GetByPatientAndRangeAsync(patientId, start, end, "temperature");
        //     return list.Select(m =>
        //     {
        //         var p = JsonSerializer.Deserialize<TemperatureParams>(m.ParametersJson!)!;
        //         return new TemperatureMonitoringDTO(
        //             m.MonitoringDate, p.BodyTemperature, m.Height, m.Weight);
        //     });
        // }

        // ---------- helper ----------
        private static string BuildParametersJson(MonitoringDTO dto) =>
            dto.MonitoringType.ToLowerInvariant() switch
            {
                "cardio" => JsonSerializer.Serialize(new CardioParams(
                                     dto.MaxBloodPressure, dto.MinBloodPressure,
                                     dto.HeartRate, dto.PulseOximetry)),
                "diabetes" => JsonSerializer.Serialize(new DiabetesParams(dto.BloodGlucose)),
                "temperature" => JsonSerializer.Serialize(new TemperatureParams(dto.BodyTemperature)),
                _ => "{}"
            };

        //public async Task SaveMonitoringAsync(Monitoring monitoring)
        //{
        //    try
        //    {
        //        await _monitoringRepository.AddMonitoringAsync(monitoring);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Eroare la adăugarea monitorizării: {ex.Message}");
        //        throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea.", ex);
        //    }
        //}

        //public async Task SaveCardioMonitoringAsync(CardioMonitoring cardio)
        //{
        //    try
        //    {
        //        if (cardio != null)
        //        {
        //            await _monitoringRepository.AddCardioMonitoringAsync(cardio);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Eroare la adăugarea monitorizării cardio: {ex.Message}");
        //        throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea cardio.", ex);
        //    }
        //}

        //public async Task SaveTemperatureMonitoringAsync(TemperatureMonitoring temperature)
        //{
        //    try
        //    {
        //        if (temperature != null)
        //        {
        //            await _monitoringRepository.AddTemperatureMonitoringAsync(temperature);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Eroare la adăugarea monitorizării de temperatură: {ex.Message}");
        //        throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea de temperatură.", ex);
        //    }
        //}

        //public async Task SaveDiabetesMonitoringAsync(DiabetesMonitoring diabetes)
        //{
        //    try
        //    {
        //        if (diabetes != null)
        //        {
        //            await _monitoringRepository.AddDiabetesMonitoringAsync(diabetes);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Eroare la adăugarea monitorizării de diabet: {ex.Message}");
        //        throw new InvalidOperationException("Nu s-a putut adăuga monitorizarea de diabet.", ex);
        //    }
        //}

    }
}
