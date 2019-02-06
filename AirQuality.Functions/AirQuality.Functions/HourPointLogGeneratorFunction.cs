using System;
using System.Globalization;
using System.Linq;
using AirQuality.TableStorageEntities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunctionEventToTable
{
    public static class HourPointLogGeneratorFunction
    {
        // Function for generating summary from one hour of monitoring
        // Create Max,Min and Avg value from log points
        // Triggers two minutes past every hour
        // *******************************************************************************

        [FunctionName("HourPointLogGeneratorFunction")]
        public static void Run(
            [TimerTrigger("0 2 * * * *")]TimerInfo myTimer, 
            [Table("pms5003Data", Connection = "AzureWebJobsStorage")] IQueryable<PointMeasurementEntity> LogPointEntries, 
            [Table("HourLogMeasurement", Connection = "AzureWebJobsStorage")] IQueryable<HourLogMeasurementEntity> HourLogLookupEntries,
            [Table("HourLogMeasurement", Connection = "AzureWebJobsStorage")] ICollector<HourLogMeasurementEntity> HourLogInsertEntries,
            ILogger logger)
        {
            logger.LogInformation($"Timer trigger function executed at UTC: {DateTime.Now}");
            GenerateHourlyStatValues(GetLastInsertedHourValue(HourLogLookupEntries), LogPointEntries, HourLogInsertEntries, logger);
        }

        private static DateTime GetLastInsertedHourValue(IQueryable<HourLogMeasurementEntity> HourLogLookupEntries)
        {
            //Get Hourlog values from last 3 days
            var lastHourPointQuery = from entity in HourLogLookupEntries
                        where entity.PartitionKey.Equals("Torborg")
                        && entity.RowKey.CompareTo(DateTime.Now.AddDays(-8).ToString("s", CultureInfo.InvariantCulture)) > 0
                        select entity;

            // Return DateTime from last read hourlog value
            return DateTime.Parse(lastHourPointQuery.ToList().OrderByDescending(x => x.ReadDateTime).Take(1).Single().RowKey);
        }

        private static void GenerateHourlyStatValues(DateTime generateFromDateTime, IQueryable<PointMeasurementEntity> LogPointEntries, ICollector<HourLogMeasurementEntity> HourLogInsertEntries, ILogger logger)
        {
            CultureInfo norwegianCultureInfo = new CultureInfo("nn-No");
            TimeZoneInfo norwegianTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            logger.LogInformation($"Retrieved last inserted HourLog Value: {generateFromDateTime}");

            var logPointQuery = from point in LogPointEntries
                        where point.PartitionKey.Equals("Torborg")
                        && point.RowKey.CompareTo(generateFromDateTime.AddHours(1).ToString("o")) > 0
                        select point;

            logger.LogInformation($"Retrived {logPointQuery.ToList().Count()} logpoints from database");

            // Generate hourly summary values

            var hourStatSummary = from p in logPointQuery.ToList()
                        group p by new { p.ReadDateTime.Year, p.ReadDateTime.Month, p.ReadDateTime.Day, p.ReadDateTime.Hour } into grouping
                        select new
                        {
                            Day = new DateTime(grouping.Key.Year, grouping.Key.Month, grouping.Key.Day, grouping.Key.Hour, 0, 0),
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

            // Remove last if not one full hour 
            if (hourStatSummary.Count() > 0)
            {
                if (hourStatSummary.Last().Count < 12) hourStatSummary = hourStatSummary.Reverse().Skip(1).Reverse();
            }

            foreach (var hour in hourStatSummary)
            {
                HourLogMeasurementEntity hourLogPoint = new HourLogMeasurementEntity("Torborg", hour.Day)
                {
                    AvgPM10 = hour.AvgPM010,
                    AvgPM25 = hour.AvgPM025,
                    AvgPM100 = hour.AvgPM100,
                    MaxPM10 = hour.MaxPM010,
                    MaxPM100 = hour.MaxPM100,
                    MaxPM25 = hour.MaxPM025,
                    MinPM10 = hour.MinPM010,
                    MinPM100 = hour.MinPM100,
                    MinPM25 = hour.MinPM025,
                    NumberOfPoints = hour.Count
                };

                HourLogInsertEntries.Add(hourLogPoint);
                logger.LogInformation($"Inserted Entry: Hour: {hour.Day} Max25: {hour.MaxPM025} Avg25: {hour.AvgPM025.ToString("F")} Min25: {hour.MinPM025} Antall: {hour.Count}");
            }
        }
    }
}
