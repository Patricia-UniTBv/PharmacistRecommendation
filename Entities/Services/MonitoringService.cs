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
                var dict = string.IsNullOrWhiteSpace(m.ParametersJson)
                    ? new Dictionary<string, JsonElement>()
                    : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(m.ParametersJson!)!;

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
    }
}
