using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirQualityWebAPI.Model
{
    interface ILogPoint
    {
        DateTime ReadDateTime { get; set; }
        int PM010 { get; set; }
        int PM025 { get; set; }
        int PM100 { get; set; }
    }
}
