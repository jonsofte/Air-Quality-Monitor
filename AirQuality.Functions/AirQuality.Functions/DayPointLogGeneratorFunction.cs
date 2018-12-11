using System;
using System.Globalization;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionEventToTable
{
    // Function for generating summary from one day of monitoring
    // Create Max,Min and Avg value from log points
    // *******************************************************************************

    public static class DayPointLogGeneratorFunction
    {

        [FunctionName("DayPointLogGeneratorFunction")]
        public static void Run([TimerTrigger("0 5 3 * * *")]TimerInfo myTimer,
            [Table("pms5003Data", Connection = "AzureWebJobsStorage")] IQueryable<PointMeasurementEntity> LogPointEntries,
            [Table("DayLogMeasurement", Connection = "AzureWebJobsStorage")] IQueryable<DayLogMeasurementEntity> HourLogLookupEntries,
            [Table("DayLogMeasurement", Connection = "AzureWebJobsStorage")] ICollector<DayLogMeasurementEntity> HourLogInsertEntries,
            TraceWriter log)
        {
            log.Info($"Timer trigger function executed at UTC: {DateTime.Now}");
            GenerateDailyStatValues(GetLastInsertedDayValue(HourLogLookupEntries), LogPointEntries, HourLogInsertEntries, log);
        }

        // Get time from last inserted value
        private static DateTime GetLastInsertedDayValue(IQueryable<DayLogMeasurementEntity> DayLogLookupEntries)
        {
            //Get Hourlog values from last 3 days

            var lastDayPointQuery = from entity in DayLogLookupEntries
                                     where entity.PartitionKey.Equals("Torborg")
                                     && entity.RowKey.CompareTo(DateTime.Now.AddDays(-10).ToString("s", CultureInfo.InvariantCulture)) > 0
                                     select entity;

            // Return DateTime from last read hourlog value
            return DateTime.Parse(lastDayPointQuery.ToList().OrderByDescending(x => x.ReadDateTime).Take(1).Single().RowKey);
        }

        // Generate values and insert to Table Storage
        private static void GenerateDailyStatValues(DateTime generateFromDateTime, IQueryable<PointMeasurementEntity> LogPointEntries, ICollector<DayLogMeasurementEntity> DayLogInsertEntries, TraceWriter log)
        {
            CultureInfo norwegianCultureInfo = new CultureInfo("nn-No");
            TimeZoneInfo norwegianTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            log.Info($"Retrieved last inserted DayLog Value: {generateFromDateTime}");

            var logPointQuery = from point in LogPointEntries
                                where point.PartitionKey.Equals("Torborg")
                                && point.RowKey.CompareTo(generateFromDateTime.AddDays(1).ToString("o")) > 0
                                select point;

            log.Info($"Retrived {logPointQuery.ToList().Count()} logpoints from database");

            var dailyStat = from p in logPointQuery.ToList()
                            group p by new { p.ReadDateTime.Year, p.ReadDateTime.Month, p.ReadDateTime.Day } into grouping
                            select new
                            {
                                Day = new DateTime(grouping.Key.Year, grouping.Key.Month, grouping.Key.Day, 0, 0, 0),
                                Count = grouping.Count(),
                                AvgPM100 = grouping.Average(x => x.PointPM100),
                                MaxPM100 = grouping.Max(x => x.PointPM100),
                                MinPM100 = grouping.Min(x => x.PointPM100),
                                AvgPM025 = grouping.Average(x => x.PointPM25),
                                MaxPM025 = grouping.Max(x => x.PointPM25),
                                MinPM025 = grouping.Min(x => x.PointPM25),
                                AvgPM010 = grouping.Average(x => x.PointPM10),
                                MaxPM010 = grouping.Max(x => x.PointPM10),
                                MinPM010 = grouping.Min(x => x.PointPM10),
                            };

            // Remove last entry if not one full day
            if (dailyStat.Count() > 0)
            {
                if (dailyStat.Last().Count < 288) dailyStat = dailyStat.Reverse().Skip(1).Reverse();
            }

            // Insert entries
            foreach (var day in dailyStat)
            {
                DayLogMeasurementEntity dayLogPoint = new DayLogMeasurementEntity("Torborg", day.Day)
                {
                    AvgPM10 = day.AvgPM010,
                    AvgPM25 = day.AvgPM025,
                    AvgPM100 = day.AvgPM100,
                    MaxPM10 = day.MaxPM010,
                    MaxPM100 = day.MaxPM100,
                    MaxPM25 = day.MaxPM025,
                    MinPM10 = day.MinPM010,
                    MinPM100 = day.MinPM100,
                    MinPM25 = day.MinPM025,
                    NumberOfPoints = day.Count
                };

                DayLogInsertEntries.Add(dayLogPoint);                
                log.Info($"Inserted Entry: Day: {day.Day.Date.ToShortDateString()} Max25: {day.MaxPM025} Avg25: {day.AvgPM025.ToString("F")} Min25: {day.MinPM025} Antall: {day.Count}");
            }
        }
    }
}
