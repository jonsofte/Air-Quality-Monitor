using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirQualityWebAPI.Model
{
    public class HourSummary : IHourSummary
    {
        public DateTime ReadDateTime { get; set; }  // Time at Read
        public int NumberOfPoints { get; set; }     // Number of measurement points in group 
        public double AvgPM10 { get; set; }        // Average of PM1.0, ug/m3
        public double AvgPM25 { get; set; }        // Average concentration of PM2.5, ug/m3
        public double AvgPM100 { get; set; }       // Average concentration of PM10.0, ug/m3
        public int MaxPM10 { get; set; }            // Maximum concentration of PM1.0, ug/m3
        public int MaxPM25 { get; set; }            // Maximum concentration of PM2.5, ug/m3
        public int MaxPM100 { get; set; }           // Maximum concentration of PM10.0, ug/m3
        public int MinPM10 { get; set; }            // Minimum concentration of PM1.0, ug/m3
        public int MinPM25 { get; set; }            // Minimum concentration of PM2.5, ug/m3
        public int MinPM100 { get; set; }           // Minimum concentration of PM10.0, ug/m3
    }
}
