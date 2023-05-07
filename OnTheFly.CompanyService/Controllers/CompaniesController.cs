﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.Services;

namespace OnTheFly.CompanyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyConnection _companyConnection;
        private readonly PostOfficeService _postOfficeService;

        public CompaniesController(CompanyConnection companyConnection, PostOfficeService postOfficeService)
        {
            _companyConnection = companyConnection;
            _postOfficeService = postOfficeService;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetCompany()
        {
            if (_companyConnection.FindAll() == null)
            {
                return NotFound();
            }
            return JsonConvert.SerializeObject(_companyConnection.FindAll(), Formatting.Indented);
        }

        [HttpGet("{cnpj}")]
        public async Task<ActionResult<string>> GetCompany(string cnpj)
        {
            if (_companyConnection.FindOne(cnpj) == null)
            {
                return NotFound();
            }
            return JsonConvert.SerializeObject(_companyConnection.FindOne(cnpj), Formatting.Indented);
        }

        [HttpPost]
        public async Task<ActionResult<string>> PostCompany(CompanyDTO companyDTO)
        {
            if (companyDTO.Address == null)
            {
                return NotFound("A companhia não possui endereço!");
            }

            if (companyDTO.Address.Street == null)
            {
                return NotFound("O endereço da companhia não possui o endereço completo!");
            }

            companyDTO.Status = null;
            int aux = companyDTO.Address.Number;

            Address address = _postOfficeService.GetAddress(companyDTO.Address.Zipcode).Result;
            address.Number= aux;
            companyDTO.Address = address;

            return JsonConvert.SerializeObject(_companyConnection.Insert(companyDTO), Formatting.Indented);
        }
    }
}
