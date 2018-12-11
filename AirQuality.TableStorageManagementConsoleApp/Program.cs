using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;                         
using Microsoft.WindowsAzure.Storage;          
using Microsoft.WindowsAzure.Storage.Table;   
using System.Globalization;

namespace AzureTableStorageConsoleApp
{
    class Program
    {

        // 
        static void Main(string[] args)
        {

            //Console.WriteLine(GetLastInsertedHourValue());
            //Console.WriteLine(GetLastInsertedHourValueWithWCFDataSyntax());
            //GenerateHourlyStatValues(GetLastInsertedHourValueWithWCFDataSyntax());
            //Console.WriteLine(GetLastInsertedHourValueWithWCFDataSyntax()); 
            //Console.WriteLine(GetLastInsertedDayLogValue());
            GenerateDailyStatValues(GetLastInsertedDayLogValue());

            Console.Read();
        }

        private static void GenerateDailyStatValues(DateTime dateTime)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("pms5003Data");

            string queryString = $"PartitionKey eq 'Torborg' and RowKey gt '{dateTime.AddDays(1).ToString("o")}'";
            //Console.WriteLine(queryString);
            TableQuery<PointMeasurementEntity> query = new TableQuery<PointMeasurementEntity>().Where(queryString);
            List<PointMeasurementEntity> measuringPoints = table.ExecuteQuery(query).ToList();

            var dailyStat = from p in measuringPoints
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
            if (dailyStat.Last().Count < 288) dailyStat = dailyStat.Reverse().Skip(1).Reverse();

            dailyStat.ToList().ForEach(x => Console.WriteLine($"Dag: {x.Day.Date.ToShortDateString()} Max: {x.MaxPM025} Avg: {x.AvgPM025.ToString("F")} Min: {x.MinPM025} Antall: {x.Count}"));
            
            table = tableClient.GetTableReference("DayLogMeasurement");
            table.CreateIfNotExists();

            CultureInfo ci = new CultureInfo("nn-No");
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            foreach (var l in dailyStat)
            {
                DayLogMeasurementEntity point = new DayLogMeasurementEntity("Torborg", TimeZoneInfo.ConvertTime(l.Day, tzi))
                {
                    AvgPM10 = l.AvgPM010,
                    AvgPM25 = l.AvgPM025,
                    AvgPM100 = l.AvgPM100,
                    MaxPM10 = l.MaxPM010,
                    MaxPM100 = l.MaxPM100,
                    MaxPM25 = l.MaxPM025,
                    MinPM10 = l.MinPM010,
                    MinPM100 = l.MinPM100,
                    MinPM25 = l.MinPM025,
                    NumberOfPoints = l.Count
                };

                TableOperation insertOperation = TableOperation.InsertOrReplace(point); ;
                table.Execute(insertOperation);
            }

            Console.WriteLine($"Antall linjer: {measuringPoints.Count}");
        }

        private static void GenerateHourlyStatValues(DateTime dateTime)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("pms5003Data");
            table.CreateIfNotExists();

            // Query string - get alle point values since last received value:
            string queryString = $"PartitionKey eq 'Torborg' and RowKey gt '{dateTime.AddHours(1).ToString("o")}'";
            Console.WriteLine(queryString);
            TableQuery<PointMeasurementEntity> query = new TableQuery<PointMeasurementEntity>().Where(queryString);

            List<PointMeasurementEntity> measuringPoints = table.ExecuteQuery(query).ToList();

            // Generate hourly summary values

            var timer = from p in measuringPoints
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
            if (timer.Count() > 0)
            {
                if (timer.Last().Count < 12) timer = timer.Reverse().Skip(1).Reverse();
            }

            // Skriv ut - debug
            timer.ToList().ForEach(x => Console.WriteLine($"Dag: {x.Day} Max: {x.MaxPM100} Avg: {x.AvgPM100.ToString("F")} Min: {x.MinPM100} Antall: {x.Count}"));

            // Insert new Hour Log Values
            CloudTable HourLogtable = tableClient.GetTableReference("HourLogMeasurement");
            HourLogtable.CreateIfNotExists();

            CultureInfo ci = new CultureInfo("nn-No");
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

            foreach (var l in timer)
            {
                HourLogMeasurementEntity point = new HourLogMeasurementEntity("Torborg", l.Day)
                {
                    AvgPM10 = l.AvgPM010,
                    AvgPM25 = l.AvgPM025,
                    AvgPM100 = l.AvgPM100,
                    MaxPM10 = l.MaxPM010,
                    MaxPM100 = l.MaxPM100,
                    MaxPM25 = l.MaxPM025,
                    MinPM10 = l.MinPM010,
                    MinPM100 = l.MinPM100,
                    MinPM25 = l.MinPM025,
                    NumberOfPoints = l.Count
                };

                TableOperation insertOperation = TableOperation.InsertOrReplace(point);
                HourLogtable.Execute(insertOperation);
            }
            Console.WriteLine($"Antall linjer: {measuringPoints.Count}");
        }

        private static DateTime GetLastInsertedHourValueWithWCFDataSyntax()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("HourLogMeasurement");

            //Get Hourlog values from last 24 hours
            String dateString = DateTime.Now.AddDays(-80).ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            String queryFilter = $"PartitionKey eq 'Torborg' and RowKey gt '{dateString}'";

            TableQuery<HourLogMeasurementEntity> query = new TableQuery<HourLogMeasurementEntity>().Where(queryFilter);
            var hourLogList = new List<HourLogMeasurementEntity>();

            TableContinuationToken token = null;
            List<HourLogMeasurementEntity> allEntities = new List<HourLogMeasurementEntity>();

            do
            {
                var queryResponse = table.ExecuteQuerySegmentedAsync<HourLogMeasurementEntity>(query, token);
                queryResponse.Wait();
                allEntities.AddRange(queryResponse.Result.Results);
            }
            while (token != null);

            // Return DateTime from last read hourlog value
            return allEntities.OrderByDescending(x => x.ReadDateTime).Take(1).Single().ReadDateTime;
        }

        private static DateTime GetLastInsertedHourValue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("HourLogMeasurement");

            //Get Hourlog values from last 24 hours
            String dateString = DateTime.Now.AddDays(-2).ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            //String queryFilter = $"PartitionKey eq 'Torborg' and RowKey gt '{dateString}'";

            var query = from entity in table.CreateQuery<HourLogMeasurementEntity>()
                        where entity.PartitionKey.Equals("Torborg")
                        && entity.RowKey.CompareTo(dateString) > 0
                        select entity;

            // Return DateTime from last read hourlog value
            return query.ToList().OrderByDescending(x => x.ReadDateTime).Take(1).Single().ReadDateTime;
        }


        private static DateTime GetLastInsertedDayLogValue()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("DayLogMeasurement");

            //Get Hourlog values from last 24 hours
            String dateString = DateTime.Now.AddDays(-30).ToString("s", CultureInfo.InvariantCulture);
            String queryFilter = $"PartitionKey eq 'Torborg' and RowKey gt '{dateString}'";

            TableQuery<DayLogMeasurementEntity> query = new TableQuery<DayLogMeasurementEntity>().Where(queryFilter);
            var hourLogList = new List<DayLogMeasurementEntity>();

            TableContinuationToken token = null;
            List<DayLogMeasurementEntity> allEntities = new List<DayLogMeasurementEntity>();

            do
            {
                var queryResponse = table.ExecuteQuerySegmentedAsync<DayLogMeasurementEntity>(query, token);
                queryResponse.Wait();
                allEntities.AddRange(queryResponse.Result.Results);
            }
            while (token != null);

            // Return DateTime from last read hourlog value
            return allEntities.OrderByDescending(x => x.ReadDateTime).Take(1).Single().ReadDateTime;
        }
    }
}

