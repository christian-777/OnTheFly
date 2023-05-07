﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.FlightService.Services;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.FlightService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        private FlightConnection _flight;
        private Services.AirportService _airport;
        private Services.AircraftService _aircraft;

        public FlightController(FlightConnection flight, Services.AirportService airport, AircraftService aircraft)
        {
            _flight = flight;
            _airport = airport;
            _aircraft = aircraft;
        }

        [HttpGet]
        public ActionResult<string> Get(string IATA, string RAB, DateTime? departure)
        {
            if (IATA == null || RAB == null || departure == null) return NoContent();
            Flight? flight = _flight.Get(IATA, RAB, departure.Value);

            return JsonConvert.SerializeObject(flight, Formatting.Indented);
        }

        [HttpPost]
        public ActionResult Insert(FlightDTO flightDTO)
        {
            if (flightDTO == null) return NoContent();
            flightDTO.Status = true;

            // Verificar se airport existe e é válido
            Airport? airport = _airport.GetValidDestiny(flightDTO.Destiny.IATA).Result;
            if (airport == null) return NotFound();
            if (airport.Country != "BR") return Unauthorized();

            flightDTO.Destiny = airport;

            // Verificar se aircraft existe e é válido
            AirCraft? aircraft = _aircraft.GetAircraft(flightDTO.Plane.RAB).Result;
            if (aircraft == null ) return NotFound();
            if (aircraft.Company.Status == false) return Unauthorized();

            // Inserção de flight
            Flight? flight = _flight.Insert(flightDTO, aircraft, airport);

            if (flight == null) return BadRequest();
            return Ok();
        }

        [HttpDelete("{IATA:length(3)}, {RAB:length(6)}, {departure}")]
        public ActionResult Delete(string IATA, string RAB, DateTime? departure)
        {
            if (IATA == null || RAB == null || departure == null) return NoContent();
            Flight? flight = _flight.Delete(IATA, RAB, departure.Value);

            if (flight == null) return NotFound();
            return Ok();
        }
    }
}
