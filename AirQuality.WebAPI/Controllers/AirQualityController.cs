using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AirQualityWebAPI.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AirQualityWebAPI.Controllers
{
    public class AirQualityController : Controller
    {
        private IAirQualityMeasurementRepository _repository;
        private static IConfiguration _configuration { get; set; }

        public AirQualityController(IConfiguration configuration, IAirQualityMeasurementRepository repository)
        {
            _repository = repository;
            _configuration = configuration;
        }

        // Get Usage:
        //http://localhost:62157/api/airquality?StartDate=2017-10-01&EndDate=2017-10-03 // 
        //http://localhost:62157/api/airquality

        [HttpGet]
        [Route("api/[controller]")]
        public JsonResult Get([FromQuery] string StartDate, [FromQuery] string EndDate)
        {
            List<Model.LogPoint> LogPointList = new List<Model.LogPoint>();

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
                var requireValuesTask = _repository.GetPointValuesFromToDate(paramStartDate, paramEndDate);
                requireValuesTask.Wait();
                LogPointList = requireValuesTask.Result;
            } else
            {
                var requireValuesTask = _repository.GetPointValuesLast24Hours();
                requireValuesTask.Wait();
                LogPointList = requireValuesTask.Result;
            }
            return Json(LogPointList);

        }
    }
}
