using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnTheFly.Connections;
using OnTheFly.Models;
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
        public async Task<ActionResult<List<Company>>> GetCompany()
        {
            if (_companyConnection.FindAll() == null)
            {
                return NotFound();
            }
            return _companyConnection.FindAll();
        }

        [HttpPost]
        public async Task<ActionResult<Company>> PostCompany(Company company)
        {
            if (company.Address == null)
            {
                return NotFound("A companhia não possui endereço!");
            }

            if (company.Address.Street == null)
            {
                return NotFound("O endereço da companhia não possui o endereço completo!");
            }

            Address address = _postOfficeService.GetAddress(company.Address.Zipcode).Result;
            company.Address = address;

            _companyConnection.Insert(company);



            return company;
        }
    }
}
