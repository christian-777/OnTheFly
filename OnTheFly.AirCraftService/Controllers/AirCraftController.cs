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
        private AirCraftConnection _airCraftConnection;
        private CompanyService _companyService;

        public AirCraftController(AirCraftConnection aircraftConnection, CompanyService companyService)
        {
            _airCraftConnection = aircraftConnection;
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AirCraft>>> GetAirCraft()
        {
            if (_airCraftConnection.FindAll().Count == 0)
            {
                return NotFound("Nenhuma companhia cadastrada");
            }
            return _airCraftConnection.FindAll();
        }

        [HttpGet("{RAB}")]
        public async Task<ActionResult<AirCraft>> GetAirCraftByRAB(string RAB)
        {
            return _airCraftConnection.FindByRAB(RAB);
        }

        [HttpPost]
        public async Task<ActionResult<string>> PostAirCraft(AirCraftDTO airCraftDTO)
        {
            #region company
            Company? company = await _companyService.GetCompany(airCraftDTO.Company);
            if (company == null) return NotFound("Companhia não encontrada");
            #endregion

            #region rab
            string rab = airCraftDTO.RAB.Replace("-", "");
            if (rab.Length != 5)
                return BadRequest("Quantidade de caracteres de RAB inválida");

            if (!AirCraft.RABValidation(rab))
                return BadRequest("RAB inválido");
            #endregion

            #region date
            DateTime dateRegistry;
            try
            {
                dateRegistry = DateTime.Parse(airCraftDTO.DtRegistry.Year + "/" + airCraftDTO.DtRegistry.Month + "/" + airCraftDTO.DtRegistry.Day);
            }
            catch
            {
                return BadRequest("A data informada é inválida! Por favor, informe uma data de registro de avião válida");
            }

            DateTime? dateLastFlight;
            if (airCraftDTO.DtLastFlight == null) dateLastFlight = null;
            else
            {
                try
                {
                    dateLastFlight = DateTime.Parse(airCraftDTO.DtLastFlight.Year + "/" + airCraftDTO.DtLastFlight.Month + "/" + airCraftDTO.DtLastFlight.Day);
                }
                catch
                {
                    return BadRequest("A data informada é inválida! Por favor, informe uma data de último voo de avião válida");
                }
                DateTime last = dateLastFlight.Value;
                if (dateRegistry.Subtract(last).TotalDays > 0)
                    return BadRequest("O último voo não pode ser antes da data de registro do avião");
            }
            #endregion

            AirCraft airCraft = new AirCraft()
            {
                Capacity = airCraftDTO.Capacity,
                Company = company,
                DtLastFlight = dateLastFlight,
                DtRegistry = dateRegistry,
                RAB = airCraftDTO.RAB
            };

            var insertAircraft = _airCraftConnection.Insert(airCraft);
            if (insertAircraft != null)
                return Created("", "Inserido com sucesso!\n\n" + JsonConvert.SerializeObject(insertAircraft, Formatting.Indented));
            return BadRequest("Erro ao inserir avião!");
        }

        [HttpPut("{RAB}")]
        public async Task<ActionResult<string>> PutAirCraft(string RAB, AirCraft airCraft)
        {
            AirCraft? existingAircraft = _airCraftConnection.FindOne(RAB);
            if (existingAircraft == null) return NotFound();
            airCraft.Id = existingAircraft.Id;

            Company? company = CompanyService.GetCompany(airCraft.Company.Cnpj).Result;
            if (company == null)
                return NoContent();

            airCraft.Company = company;

            return JsonConvert.SerializeObject(_airCraftConnection.Update(RAB, airCraft), Formatting.Indented);
        }

        [HttpPatch("{RAB}")]
        public async Task<ActionResult<string>> PatchAircraftDate(string RAB, [FromBody] DateTime DtLastFlight)
        {
            if (RAB == null || DtLastFlight == null) return NoContent();

            return JsonConvert.SerializeObject(_airCraftConnection.PatchDate(RAB, DtLastFlight), Formatting.Indented);
        }

        [HttpDelete("{RAB}")]
        public async Task<ActionResult> DeleteAirCraft(string RAB)
        {
            if (_airCraftConnection.Delete(RAB) == null)
                return NotFound();
            return Ok();

        }
    }
}
