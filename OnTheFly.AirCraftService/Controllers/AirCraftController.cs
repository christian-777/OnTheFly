using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
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
            var company = await CompanyService.GetCompany(airCraftDTO.Company.Cnpj);
            if (company == null)
                return NoContent();
            if (company.Status == null)
                company.Status = true;

            CompanyService.PutCompany(company);

            return JsonConvert.SerializeObject(_airCraftConnection.Insert(airCraftDTO), Formatting.Indented);
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
