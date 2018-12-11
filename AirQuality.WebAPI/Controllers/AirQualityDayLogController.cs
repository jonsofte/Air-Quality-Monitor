using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AirQualityWebAPI.Repository;
using Microsoft.Extensions.Configuration;

namespace AirQualityWebAPI.Controllers
{
    public class AirQualityDayLogController : Controller
    {
        private IAirQualityMeasurementRepository _repository;
        private static IConfiguration _configuration { get; set; }

        public AirQualityDayLogController(IConfiguration configuration, IAirQualityMeasurementRepository repository)
        {
            _repository = repository;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("api/[controller]")]
        public JsonResult Get([FromQuery] string NumberOfDays)
        {
            List<Model.DaySummary> DayLogList = new List<Model.DaySummary>();

            if (String.IsNullOrEmpty(NumberOfDays))
            {
                NumberOfDays = "90";
            }

            // Retrieve from Repository
            var requireValuesTask = _repository.GetDayLogValuesLast90Days();
            requireValuesTask.Wait();
            DayLogList = requireValuesTask.Result;

            return Json(DayLogList);
        }
    }
}
