using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnTheFly.CompanyService.Services;
using OnTheFly.Connections;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using OnTheFly.PostOfficeService;

namespace OnTheFly.CompanyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyConnection _companyConnection;
        private readonly PostOfficesService _postOfficeService;
        private readonly AircraftService _aircraftService;

        public CompaniesController(CompanyConnection companyConnection, PostOfficesService postOfficeService)
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
            return JsonConvert.SerializeObject(_companyConnection.FindAll().Where(x => x.Status == true), Formatting.Indented);
        }

        [HttpGet("{cnpj}")]
        public async Task<ActionResult<string>> GetCompanyByCNPJ(string cnpj)
        {
            if (cnpj.Length != 14)
                return BadRequest("CNPJ informado não possui o formato necessário para realizar a pesquisa! Por favor, tente inserir novamente um número de CNPJ com 14 digitos numéricos (somente números)");

            for (int i = 0; i < cnpj.Length; i++)
            {
                if ((cnpj[i] != '0') && (cnpj[i] != '1') && (cnpj[i] != '2') && (cnpj[i] != '3') && (cnpj[i] != '4') && (cnpj[i] != '5') && (cnpj[i] != '6') && (cnpj[i] != '7') && (cnpj[i] != '8') && (cnpj[i] != '9'))
                {
                    return BadRequest("O CNPJ informado não possui apenas valores numéricos! Por favor, insira um valor de CNPJ que contenha 14 digitos numéricos (apenas números)");
                }
            }

            if (_companyConnection.FindAll().Where(x => (x.Status == true) && (x.Cnpj == cnpj)).FirstOrDefault() == null)
                return BadRequest("O CNPJ informado não existe ou está restrito");

            return JsonConvert.SerializeObject(_companyConnection.FindByCnpj(cnpj), Formatting.Indented);
        }

        [HttpPut("/nomefantasia/{cnpj},{nameOPT}", Name = "Atualização do Nome Fantasia")]
        public async Task<ActionResult<string>> UpdateNameOPT(string cnpj, string nameOPT)
        {
            if (_companyConnection.FindAll().Where(x => x.Cnpj == cnpj) == null)
                return NotFound();

            _companyConnection.UpdateNameOPT(cnpj, nameOPT);

            return Accepted();
        }

        [HttpPut("/status/{cnpj},{status}", Name = "Atualização do Status")]
        public async Task<ActionResult<string>> UpdateStatus(string cnpj, bool status)
        {
            if (_companyConnection.FindAll().Where(x => (x.Cnpj == cnpj) && (x.Status == false)).FirstOrDefault() is not null)
                return BadRequest("Não é possível fazer UPDATE neste documento pois o status dele está inativo!");

            _companyConnection.UpdateStatus(cnpj, status);

            return Accepted();
        }

        [HttpPost]
        public async Task<ActionResult<string>> PostCompany(CompanyDTO companyDTO)
        {
            try
            {
                #region Company
                if (companyDTO.Cnpj.Length != 14)
                    return BadRequest("A quantidade de digitos do CNPJ informado está incorreta! Por favor, tente inserir novamente um número de CNPJ com 14 digitos numéricos (somente números)");

                for (int i = 0; i < companyDTO.Cnpj.Length; i++)
                {
                    if ((companyDTO.Cnpj[i] != '0') && (companyDTO.Cnpj[i] != '1') && (companyDTO.Cnpj[i] != '2') && (companyDTO.Cnpj[i] != '3') && (companyDTO.Cnpj[i] != '4') && (companyDTO.Cnpj[i] != '5') && (companyDTO.Cnpj[i] != '6') && (companyDTO.Cnpj[i] != '7') && (companyDTO.Cnpj[i] != '8') && (companyDTO.Cnpj[i] != '9'))
                    {
                        return BadRequest("O CNPJ informado não possui apenas valores numéricos! Por favor, insira um valor de CNPJ que contenha 14 digitos numéricos (apenas números)");
                    }
                }

                if ((companyDTO.Name == "") || (companyDTO.Name == "string"))
                    return BadRequest("A razão social da companhia não foi informada! Por favor, insira um nome correspondente a razão social da companhia");

                if (companyDTO.Name.Length > 30)
                    return BadRequest("A quantidade de digitos da razão social informada está ultrapassando o valor máximo! Por favor, tente inserir novamente um nome de razão social com no máximo 30 caracteres");


                if ((companyDTO.NameOPT == "") || (companyDTO.NameOPT == "string"))
                    companyDTO.NameOPT = companyDTO.Name;
                #endregion

                #region Date
                if ((companyDTO.DtOpen.Year > DateTime.Now.Year) || ((companyDTO.DtOpen.Year == DateTime.Now.Year) && (companyDTO.DtOpen.Month > DateTime.Now.Month)) || ((companyDTO.DtOpen.Year == DateTime.Now.Year) && (companyDTO.DtOpen.Month == DateTime.Now.Month) && (companyDTO.DtOpen.Day > DateTime.Now.Day)))
                {
                    return BadRequest("A data informada é inválida! Por favor, informe uma data de abertura da companhia válida");
                }
                #endregion

                #region Address
                int auxNumber = companyDTO.Address.Number;
                string auxComplement = "";

                if ((companyDTO.Address.Complement != "string") && (companyDTO.Address.Complement != ""))
                    auxComplement = companyDTO.Address.Complement;

                if (companyDTO.Address.Zipcode.Length != 8)
                    return BadRequest("A quantidade de digitos informados para buscar o CEP não é válido! Por favor, insira 8 digitos numéricos para buscar as informações do seu endereço");

                for (int i = 0; i < companyDTO.Address.Zipcode.Length; i++)
                {
                    if ((companyDTO.Address.Zipcode[i] != '0') && (companyDTO.Address.Zipcode[i] != '1') && (companyDTO.Address.Zipcode[i] != '2') && (companyDTO.Address.Zipcode[i] != '3') && (companyDTO.Address.Zipcode[i] != '4') && (companyDTO.Address.Zipcode[i] != '5') && (companyDTO.Address.Zipcode[i] != '6') && (companyDTO.Address.Zipcode[i] != '7') && (companyDTO.Address.Zipcode[i] != '8') && (companyDTO.Address.Zipcode[i] != '9'))
                    {
                        return BadRequest("O CEP informado não possui apenas valores numéricos! Por favor, insira um valor de CEP que contenha 8 digitos numéricos (apenas números)");
                    }
                }

                Address address = _postOfficeService.GetAddress(companyDTO.Address.Zipcode).Result;

                if (address.Street == "")
                    address.Street = companyDTO.Address.Street;

                companyDTO.Address = address;

                if ((companyDTO.Address.Street == "") || (companyDTO.Address.Street == "string"))
                    return BadRequest("O CEP informado é um CEP municipal! É necessário informar também a rua do endereço");

                address.Number = auxNumber;

                if (auxComplement != "")
                    address.Complement = auxComplement;
                #endregion

                var insertCompany = _companyConnection.Insert(companyDTO);

                if (_aircraftService.InsertAircraft(companyDTO.AirCraftDTO) is null)
                    return BadRequest("Não foi possível inserir o avião!");

                return JsonConvert.SerializeObject(insertCompany);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpDelete("/companyactivated{cnpj}")]
        public async Task<ActionResult> DeleteCompany(string cnpj)
        {
            if (cnpj.Length != 14)
                return BadRequest("CNPJ informado não possui o formato necessário para realizar a deleção da companhia! Por favor, tente inserir novamente um número de CNPJ com 14 digitos numéricos (somente números)");

            for (int i = 0; i < cnpj.Length; i++)
            {
                if ((cnpj[i] != '0') && (cnpj[i] != '1') && (cnpj[i] != '2') && (cnpj[i] != '3') && (cnpj[i] != '4') && (cnpj[i] != '5') && (cnpj[i] != '6') && (cnpj[i] != '7') && (cnpj[i] != '8') && (cnpj[i] != '9'))
                {
                    return BadRequest("O CNPJ informado não possui apenas valores numéricos! Por favor, insira um valor de CNPJ que contenha 14 digitos numéricos (apenas números)");
                }
            }

            if (_companyConnection.FindByCnpj(cnpj) == null)
                return BadRequest("O CNPJ informado não foi encontrado!");

            _companyConnection.Delete(cnpj);

            return Ok();
        }

        [HttpDelete("/companyrestricted/{cnpj}")]
        public async Task<ActionResult> DeleteCompanyRestricted(string cnpj)
        {
            if (cnpj.Length != 14)
                return BadRequest("CNPJ informado não possui o formato necessário para realizar a deleção da companhia! Por favor, tente inserir novamente um número de CNPJ com 14 digitos numéricos (somente números)");

            for (int i = 0; i < cnpj.Length; i++)
            {
                if ((cnpj[i] != '0') && (cnpj[i] != '1') && (cnpj[i] != '2') && (cnpj[i] != '3') && (cnpj[i] != '4') && (cnpj[i] != '5') && (cnpj[i] != '6') && (cnpj[i] != '7') && (cnpj[i] != '8') && (cnpj[i] != '9'))
                {
                    return BadRequest("O CNPJ informado não possui apenas valores numéricos! Por favor, insira um valor de CNPJ que contenha 14 digitos numéricos (apenas números)");
                }
            }

            if (_companyConnection.FindByCnpjRestricted(cnpj) == null)
                return BadRequest("O CNPJ informado não foi encontrado!");

            _companyConnection.DeleteByRestricted(cnpj);

            return Ok();
        }
    }
}
