using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirQualityWebAPI.Model
{
    public class LogPoint : ILogPoint
    {
        [JsonConverter(typeof(CustomDateTimeConverter))]

        public DateTime ReadDateTime { get; set; }
        public int PM010 { get; set; }
        public int PM025 { get; set; }
        public int PM100 { get; set; }
        public override string ToString()
        {
            return $" {ReadDateTime} : PM010: {PM010} PM025: {PM025} PM100: {PM100}"; 
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
