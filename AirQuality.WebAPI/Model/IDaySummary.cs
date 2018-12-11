using System;

namespace AirQualityWebAPI.Model
{
    interface IDaySummary
    {
        DateTime ReadDateTime { get; set; }  // Time at Read
        int NumberOfPoints { get; set; }     // Number of measurement points in group 
        double AvgPM10 { get; set; }        // Average of PM1.0, ug/m3
        double AvgPM25 { get; set; }        // Average concentration of PM2.5, ug/m3
        double AvgPM100 { get; set; }       // Average concentration of PM10.0, ug/m3
        int MaxPM10 { get; set; }            // Maximum concentration of PM1.0, ug/m3
        int MaxPM25 { get; set; }            // Maximum concentration of PM2.5, ug/m3
        int MaxPM100 { get; set; }           // Maximum concentration of PM10.0, ug/m3
        int MinPM10 { get; set; }            // Minimum concentration of PM1.0, ug/m3
        int MinPM25 { get; set; }            // Minimum concentration of PM2.5, ug/m3
        int MinPM100 { get; set; }           // Minimum concentration of PM10.0, ug/m3
    }
}
