using System;

namespace AirQuality.SignalR
{
    interface ILogPoint
    {
        DateTime readDateTime { get; set; }
        int pM010 { get; set; }
        int pM025 { get; set; }
        int pM100 { get; set; }
    }
}
