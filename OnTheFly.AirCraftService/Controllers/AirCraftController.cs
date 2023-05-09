using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using OnTheFly.AirCraftService.Services;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.AirCraftService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirCraftController : ControllerBase
    {
        private AirCraftConnection _airCraftConnection = new AirCraftConnection();

        [HttpGet]
        public async Task<ActionResult<string>> GetAirCraft()
        {
            var aircrafts = _airCraftConnection.FindAll();
            if (aircrafts == null)
                return NoContent();
            return JsonConvert.SerializeObject(aircrafts, Formatting.Indented);
        }
        [HttpGet("{RAB}")]
        public async Task<ActionResult<string>> GetAirCraft(string RAB)
        {
            var aircraft = _airCraftConnection.FindOne(RAB);
            if (aircraft == null)
                return NotFound();
            return JsonConvert.SerializeObject(aircraft, Formatting.Indented);
        }

        [HttpPost]
        public async Task<ActionResult<string>> PostAirCraft(AirCraftDTO airCraftDTO)
        {
            var company = await CompanyService.GetCompany(airCraftDTO.Company);
            if (company == null)
                return BadRequest();

            var rab = airCraftDTO.RAB.Replace("-", "");
            if (rab.Length != 5)
                return BadRequest();

            if (!AirCraft.RABValidation(rab))
                return BadRequest("invalid RAB");

            if (airCraftDTO.DtLastFlight != null)
            {
                DateTime last = airCraftDTO.DtLastFlight.Value;
                if (airCraftDTO.DtRegistry.Subtract(last).TotalDays > 0 )
                    return BadRequest();
            }
           


            AirCraft airCraft = new AirCraft()
            {
                Capacity = airCraftDTO.Capacity,
                Company = company,
                DtLastFlight= airCraftDTO.DtLastFlight,
                DtRegistry= airCraftDTO.DtRegistry,
                RAB=airCraftDTO.RAB
            };
            
            try
            {
                var inserted = _airCraftConnection.Insert(airCraft);
                return JsonConvert.SerializeObject(inserted, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{RAB}")]
        public async Task<ActionResult<string>> PostAirCraft(string RAB, AirCraft airCraft)
        {
            if (CompanyService.GetCompany(airCraft.Company.Cnpj) == null)
                return NoContent();

            return JsonConvert.SerializeObject(_airCraftConnection.Update(RAB, airCraft), Formatting.Indented);
        }

        [HttpDelete("{RAB}")]
        public async Task<ActionResult> DeleteAirCraft(string rab)
        {
            if(_airCraftConnection.Delete(rab)==null)
                return NotFound();
            return Ok();

        }
    }
}
