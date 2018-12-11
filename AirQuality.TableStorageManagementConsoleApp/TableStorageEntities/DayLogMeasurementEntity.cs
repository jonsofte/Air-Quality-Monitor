using System;
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

namespace AzureTableStorageConsoleApp
{
    //
    //  https://docs.microsoft.com/en-us/dotnet/api/microsoft.windowsazure.storage.table.tableentity
    //***************

    // Statistics for one hour of logging - Min, Max and Average value of measurements

    public class DayLogMeasurementEntity : TableEntity
    {
        public DayLogMeasurementEntity(string location)
        {
            this.PartitionKey = location;
            this.RowKey = DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            ReadDateTime = DateTime.Now;
        }

        public DayLogMeasurementEntity(string location, DateTime MeasureTime)
        {
            this.PartitionKey = location;
            this.RowKey = MeasureTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            ReadDateTime = MeasureTime;
        }

        public DayLogMeasurementEntity()
        {
        }

        public DateTime ReadDateTime { get; set; }  // Time at Read
        public int NumberOfPoints { get; set; }     // Number of measurement points in group 
        public double AvgPM10 { get; set; }            // Average of PM1.0, ug/m3
        public double AvgPM25 { get; set; }            // Average concentration of PM2.5, ug/m3
        public double AvgPM100 { get; set; }           // Average concentration of PM10.0, ug/m3
        public int MaxPM10 { get; set; }            // Maximum concentration of PM1.0, ug/m3
        public int MaxPM25 { get; set; }            // Maximum concentration of PM2.5, ug/m3
        public int MaxPM100 { get; set; }           // Maximum concentration of PM10.0, ug/m3
        public int MinPM10 { get; set; }            // Minimum concentration of PM1.0, ug/m3
        public int MinPM25 { get; set; }            // Minimum concentration of PM2.5, ug/m3
        public int MinPM100 { get; set; }           // Minimum concentration of PM10.0, ug/m3
    }
}
