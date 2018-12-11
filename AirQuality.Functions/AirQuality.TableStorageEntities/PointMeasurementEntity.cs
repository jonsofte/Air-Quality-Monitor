using System;
using Microsoft.WindowsAzure.Storage.Table; 

namespace AirQuality.TableStorageEntities
{

    public class PointMeasurementEntity : TableEntity
    {
        public PointMeasurementEntity(string location)
        {
            this.PartitionKey = location;
            this.RowKey = DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            ReadDateTime = DateTime.Now;
        }

        public PointMeasurementEntity(string location, DateTime LocalReadTime, DateTime UtcReadTime)
        {
            this.PartitionKey = location;
            this.RowKey = LocalReadTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
            ReadDateTime = UtcReadTime;
        }

        public PointMeasurementEntity()
        {
        }

        public DateTime ReadDateTime { get; set; }  // Time at Read
        public int PointPM10 { get; set; }          // Concentration of PM1.0, ug/m3
        public int PointPM25 { get; set; }          // Concentration of PM2.5, ug/m3
        public int PointPM100 { get; set; }         // Concentration of PM10.0, ug/m3
    }
}
