using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AirQualityWebAPI.Model;
using Microsoft.Azure;                          
using Microsoft.WindowsAzure.Storage;           
using Microsoft.WindowsAzure.Storage.Table;
using AirQualityWebAPI.AzureTableStorage;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace AirQualityWebAPI.Repository
{
    public class AirQualityMeasurementRepository : IAirQualityMeasurementRepository
    {
        private static IConfiguration Configuration { get; set; }
        private String connectionString;
        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        //private CloudTable airQualityTable;


        public AirQualityMeasurementRepository(IConfiguration configuration)
        {
            Configuration = configuration;
            connectionString = Configuration["StorageConnectionString"];
            storageAccount = CloudStorageAccount.Parse(connectionString);
            tableClient = storageAccount.CreateCloudTableClient();
        }

        // Get all log values from last 24 hours
        public async Task<List<LogPoint>> GetPointValuesLast24Hours()
        {
            var airQualityTable = tableClient.GetTableReference("pms5003Data");

            TimeZoneInfo norwegianTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            CultureInfo norwegianCultureInfo = new CultureInfo("nn-No");

            DateTime utcTime = DateTime.Now.ToUniversalTime();
            DateTime norwegianTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, norwegianTimeZoneInfo);

            string timeString = norwegianTime.AddDays(-1).ToString("s", norwegianCultureInfo);

            // Retrieve log points from table storage

            TableQuery<PointMeasurementEntity> query = new TableQuery<PointMeasurementEntity>().Where($"PartitionKey eq 'Torborg' and RowKey gt '{timeString}'");
            var measurementLogPointList = new List<LogPoint>();
            TableContinuationToken continuationToken = null;
            List<PointMeasurementEntity> allEntities = new List<PointMeasurementEntity>();

            do
            {
                var queryResponse = await airQualityTable.ExecuteQuerySegmentedAsync<PointMeasurementEntity>(query, continuationToken);
                continuationToken = queryResponse.ContinuationToken;
                allEntities.AddRange(queryResponse.Results);
            }

            while (continuationToken != null);

            foreach (PointMeasurementEntity pme in allEntities)
            {
                measurementLogPointList.Add(new LogPoint()
                {
                    PM010 = pme.PointPM10,
                    PM025 = pme.PointPM10,
                    PM100 = pme.PointPM100,
                    ReadDateTime = DateTime.Parse(pme.RowKey)
                    });
            }
            return measurementLogPointList;

        }

        // Get all log values between two dates
        public async Task<List<LogPoint>> GetPointValuesFromToDate(DateTime startDate, DateTime endDate)
        {
            var airQualityTable = tableClient.GetTableReference("pms5003Data");

            // Build query filter
            String startDateString = startDate.Date.ToString("yyyy-MM-dd");
            String endDateString = endDate.Date.ToString("yyyy-MM-dd");
            String queryFilter = $"PartitionKey eq 'Torborg' and RowKey gt '{startDateString}' and RowKey lt '{endDateString}'";

            // Excecute Query
            TableQuery<PointMeasurementEntity> query = new TableQuery<PointMeasurementEntity>().Where(queryFilter);
            var logPointList = new List<LogPoint>();
            TableContinuationToken continuationToken = null;
            List<PointMeasurementEntity> PointMeasurementList = new List<PointMeasurementEntity>();

            do
            {
                var queryResponse = await airQualityTable.ExecuteQuerySegmentedAsync<PointMeasurementEntity>(query, continuationToken);
                continuationToken = queryResponse.ContinuationToken;
                PointMeasurementList.AddRange(queryResponse.Results);
            }

            while (continuationToken != null);

            CultureInfo norwegianCultureInfo = new CultureInfo("nn-No");
            TimeZoneInfo norwegianTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            foreach (PointMeasurementEntity pme in PointMeasurementList)
            {
                if (!(pme.PointPM10 == 0 || pme.PointPM100 == 0 || (pme.PointPM25 == 0)))
                    logPointList.Add(new LogPoint()
                    {
                        PM010 = pme.PointPM10,
                        PM025 = pme.PointPM25,
                        PM100 = pme.PointPM100,
                        ReadDateTime = pme.ReadDateTime
                    });
            }                     
            return logPointList;            
        }

        // Get day summary values from last 90 days
        public async Task<List<DaySummary>> GetDayLogValuesLast90Days()
        {
            CultureInfo norwegianCultureInfo = new CultureInfo("nn-No");

            var airQualityTable = tableClient.GetTableReference("DayLogMeasurement");
            String dateString = DateTime.Now.AddDays(-90).ToString("s", norwegianCultureInfo);
            String queryFilter = $"PartitionKey eq 'Torborg' and RowKey gt '{dateString}'";

            // Excecute Query
            TableQuery<HourLogMeasurementEntity> query = new TableQuery<HourLogMeasurementEntity>().Where(queryFilter);
            var daySummaryList = new List<DaySummary>();
            TableContinuationToken token = null;
            List<HourLogMeasurementEntity> allEntities = new List<HourLogMeasurementEntity>();

            do
            {
                var queryResponse = await airQualityTable.ExecuteQuerySegmentedAsync<HourLogMeasurementEntity>(query, token);
                token = queryResponse.ContinuationToken;
                allEntities.AddRange(queryResponse.Results);
            }

            while (token != null);

            foreach (HourLogMeasurementEntity entity in allEntities)
            {
                    daySummaryList.Add(new DaySummary()
                    {
                        AvgPM10 = entity.AvgPM10,
                        AvgPM100 = entity.AvgPM100,
                        AvgPM25 = entity.AvgPM25,
                        MaxPM10 = entity.MaxPM10,
                        MaxPM25 = entity.MaxPM25,
                        MaxPM100 = entity.MaxPM100,
                        MinPM10 = entity.MinPM10,
                        MinPM100 = entity.MinPM100,
                        MinPM25 = entity.MinPM25,
                        NumberOfPoints = entity.NumberOfPoints,
                        ReadDateTime = entity.ReadDateTime
                    });
            }
            return daySummaryList;
        }

        // Get hour summary values from last 7 days
        public async Task<List<HourSummary>> GetHourLogValuesLast7Days()
        {

            // Create Query Filter for last 7 days
            var airQualityTable = tableClient.GetTableReference("HourLogMeasurement");
            String dateString = DateTime.Now.AddDays(-7).ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            String queryFilter = $"PartitionKey eq 'Torborg' and RowKey gt '{dateString}'";

            // Execute Query
            TableQuery<HourLogMeasurementEntity> query = new TableQuery<HourLogMeasurementEntity>().Where(queryFilter);
            var hourSummaryList = new List<HourSummary>();
            TableContinuationToken token = null;
            List<HourLogMeasurementEntity> allEntities = new List<HourLogMeasurementEntity>();

            do
            {
                var queryResponse = await airQualityTable.ExecuteQuerySegmentedAsync<HourLogMeasurementEntity>(query, token);
                token = queryResponse.ContinuationToken;
                allEntities.AddRange(queryResponse.Results);
            }

            while (token != null);

            //CultureInfo norwegianCultureInfo = new CultureInfo("nn-No");
            TimeZoneInfo norwegianTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            foreach (HourLogMeasurementEntity hourLogEntity in allEntities)
            {

                hourSummaryList.Add(new HourSummary()
                {
                    AvgPM10 = hourLogEntity.AvgPM10,
                    AvgPM100 = hourLogEntity.AvgPM100,
                    AvgPM25 = hourLogEntity.AvgPM25,
                    MaxPM10 = hourLogEntity.MaxPM10,
                    MaxPM25 = hourLogEntity.MaxPM25,
                    MaxPM100 = hourLogEntity.MaxPM100,
                    MinPM10 = hourLogEntity.MinPM10,
                    MinPM100 = hourLogEntity.MinPM100,
                    MinPM25 = hourLogEntity.MinPM25,
                    NumberOfPoints = hourLogEntity.NumberOfPoints,
                    ReadDateTime = TimeZoneInfo.ConvertTimeFromUtc(hourLogEntity.ReadDateTime, norwegianTimeZoneInfo)
            });
            }
            return hourSummaryList;
        }
    }
}
