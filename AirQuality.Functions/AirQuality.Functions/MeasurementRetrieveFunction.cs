using System;
using System.Globalization;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using AirQuality.SignalR;
using AirQuality.TableStorageEntities;
using AirQuality.EventHub;
using Microsoft.Extensions.Logging;

namespace AzureFunctionBergenLuftInfoSignalR
{
    public static class MeasurementRetrieveFunction
    {
        [FunctionName("RetrieveMeasurementEventStoreAndPublish")]
        public static void Run(
            [EventHubTrigger("myEventHubMessage", Connection = "flow_events_IOTHUB")]string eventHubMessage,
            [Table("pms5003Data", Connection = "AzureWebJobsStorage")] ICollector<PointMeasurementEntity> LogPointEntries,
            ILogger logger)
        {

            logger.LogInformation($"Function started at: {DateTime.Now}");
            var retrievedPoint = GetReadingFromEventHub(eventHubMessage, logger);
            PersistToTableStorage(LogPointEntries, logger, retrievedPoint);
            SendToSignalRStream(retrievedPoint, logger);
        }

        private static void PersistToTableStorage(ICollector<PointMeasurementEntity> LogPointEntries, ILogger log, PointMeasurementEntity retrievedPoint)
        {
            LogPointEntries.Add(retrievedPoint);
            log.LogInformation($"Data persisted to Storage table");
        }

        private static PointMeasurementEntity GetReadingFromEventHub(string eventHubMessage, ILogger log)
        {
            PointMeasurementEntity point;
            RetrivedMessage message = JsonConvert.DeserializeObject<RetrivedMessage>(eventHubMessage);
            CultureInfo cultureInfo = new CultureInfo("nn-No");
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            DateTime readTimeNorwegianTimeZone = TimeZoneInfo.ConvertTime(message.ReadTime, timeZoneInfo);

            point = new PointMeasurementEntity("Torborg", readTimeNorwegianTimeZone, message.ReadTime)
            {
                PointPM10 = message.Pm001,
                PointPM100 = message.Pm100,
                PointPM25 = message.Pm025
            };

            log.LogInformation($"Received Message: {readTimeNorwegianTimeZone.ToString("g", cultureInfo)} PM0.1: {message.Pm001} Pm2.5: {message.Pm025} PM10: {message.Pm100}");
            return point;
        }

        private static void SendToSignalRStream(PointMeasurementEntity retrievedPoint, ILogger log)
        {
            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("http://bergenluftinfo.azurewebsites.net/livedatastream")
                .Build();

            try
            {
                connection.StartAsync().Wait();

                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                DateTime readTimeNorwegianTimeZone = TimeZoneInfo.ConvertTime(retrievedPoint.ReadDateTime, timeZoneInfo);

                LogPoint point = new LogPoint()
                {
                    readDateTime = readTimeNorwegianTimeZone,
                    pM010 = retrievedPoint.PointPM10,
                    pM025 = retrievedPoint.PointPM10,
                    pM100 = retrievedPoint.PointPM100
                };

                var result = connection.InvokeAsync("Send", "PointMeasurement", JsonConvert.SerializeObject(point));
                result.Wait();

                log.LogInformation($"Sent data: {JsonConvert.SerializeObject(point)}");
            }

            finally
            {
                connection.StopAsync().Wait();
            }
        }
    }
}
