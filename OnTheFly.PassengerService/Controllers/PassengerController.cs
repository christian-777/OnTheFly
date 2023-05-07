﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;


namespace OnTheFly.PassengerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerConnection _passengerConnection;
        [HttpGet]
        public ActionResult<List<Passenger>> GetAll()
        {
            return _passengerConnection.FindAll(); ;
        }
        [HttpGet("{cpf}", Name = "GetCPF")]
        public ActionResult<Passenger> GetBycpf(string cpf)
        {
            return _passengerConnection.FindPassenger(cpf);
        }
        [HttpPost]
        public ActionResult Insert(PassengerDTO passengerdto)
        {
            if (passengerdto.CPF is null) return NoContent();

            _passengerConnection.Insert(passengerdto);
            return Ok();
        }
        [HttpPut]
        public ActionResult Update(string cpf, Passenger passenger)
        {
            _passengerConnection.Update(cpf, passenger);
            return Ok();
        }
        [HttpDelete]
        public ActionResult Delete(string cpf)
        {
            _passengerConnection.Delete(cpf);
            return Ok(); ;
        }
    }
}