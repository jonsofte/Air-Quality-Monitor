using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AirQualityWebAPI.Model;

namespace AirQualityWebAPI.Repository
{
    public interface IAirQualityMeasurementRepository
    {
        Task<List<LogPoint>> GetPointValuesFromToDate(DateTime startDate, DateTime endDate);
        Task<List<LogPoint>> GetPointValuesLast24Hours();
        Task<List<HourSummary>> GetHourLogValuesLast7Days();
        Task<List<DaySummary>> GetDayLogValuesLast90Days();

    }
}
