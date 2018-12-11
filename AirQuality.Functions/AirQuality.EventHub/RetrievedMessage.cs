using System;
using System.Collections.Generic;
using System.Text;

namespace AirQuality.EventHub
{
    public class RetrivedMessage
    {
        public double Time { get; set; }

        public DateTime ReadTime
        {
            get
            {
                DateTime readTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                readTime = readTime.AddSeconds(Time).ToLocalTime();
                return readTime;
            }
        }

        public int Pm001 { get; set; }
        public int Pm025 { get; set; }
        public int Pm100 { get; set; }
    }
}
