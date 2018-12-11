using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AirQuality.SignalR
{
    public class LogPoint : ILogPoint
    {
        [JsonConverter(typeof(CustomDateTimeConverter))]

        public DateTime readDateTime { get; set; }
        public int pM010 { get; set; }
        public int pM025 { get; set; }
        public int pM100 { get; set; }

        public override string ToString()
        {
            return $" {readDateTime} : PM010: {pM010} PM025: {pM025} PM100: {pM100}";
        }

        class CustomDateTimeConverter : IsoDateTimeConverter
        {
            public CustomDateTimeConverter()
            {
                base.DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.001Z";
            }
        }
    }
}
