using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AirQualityWebAPI.Repository;
using Microsoft.Extensions.Configuration;

namespace AirQualityWebAPI.Controllers
{

    public class AirQualityHourLogController : Controller
    {
        private IAirQualityMeasurementRepository _repository;
        private static IConfiguration _configuration { get; set; }

        public AirQualityHourLogController(IConfiguration configuration, IAirQualityMeasurementRepository repository)
        {
            _repository = repository;
            _configuration = configuration;
        }


        [HttpGet]
        [Route("api/[controller]")]
        public JsonResult Get([FromQuery] string StartDate, [FromQuery] string EndDate)
        {
            List<Model.HourSummary> HourLogList = new List<Model.HourSummary>();

            // Setup Parameters
            DateTime paramStartDate = DateTime.Now;
            DateTime paramEndDate = DateTime.Now;
            bool isDateQuery = false;

            if (!String.IsNullOrEmpty(StartDate))
            {
                paramStartDate = DateTime.Parse(StartDate);
                paramEndDate = DateTime.Parse(StartDate);
                isDateQuery = true;
            }

            if (!String.IsNullOrEmpty(EndDate))
            {
                paramEndDate = DateTime.Parse(EndDate);
                paramEndDate = paramEndDate.AddDays(1);
            }

            // Retrieve from Repository
            if (isDateQuery)
            {
            }
            else
            {
                var requireValuesTask = _repository.GetHourLogValuesLast7Days();
                requireValuesTask.Wait();
                HourLogList = requireValuesTask.Result;
            }

            return Json(HourLogList);
        }

    }
}
