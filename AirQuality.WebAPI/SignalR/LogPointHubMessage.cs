using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AirQualityWebAPI.HubSignalR
{
    public class LogPointHubMessage : Hub
    {
        public Task Send(string measurementType,string MeasurementValues)
        {
            if (measurementType == "PointMeasurement") { 
                return Clients.All.SendAsync("PointMeasurement", MeasurementValues);
            }
            else if (measurementType == "HourMeasurement")
            {
                return Clients.All.SendAsync("HourMeasurement", MeasurementValues);
            }
            else if (measurementType == "DayMeasurement")
            {
                return Clients.All.SendAsync("DayMeasurement", MeasurementValues);
            }
            else
            {
                throw new ApplicationException($"Unknown Measurement Type: {measurementType}");
            }
        }
    }
}