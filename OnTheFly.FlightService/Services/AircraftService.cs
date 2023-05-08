using System.Text;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using MongoDB.Driver;
using Newtonsoft.Json;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.FlightService.Services
{
    public class AircraftService
    {
        private HttpClient _httpClient = new HttpClient();

        public async Task<AirCraft?> GetAircraft(string RAB)
        {
            HttpResponseMessage res = await _httpClient.GetAsync("https://localhost:5000/api/AirCraft/" + RAB);
            if (!res.IsSuccessStatusCode) return null;

            string content = await res.Content.ReadAsStringAsync();
            AirCraft? result = JsonConvert.DeserializeObject<AirCraft>(content);

            return result;
        }

        public async Task<AirCraft?> UpdateAircraft(string RAB, AirCraft aircraft)
        {
            Company company = aircraft.Company;
            CompanyDTO companyDTO = new CompanyDTO()
            {
                Id = company.Id,
                Address = company.Address,
                Cnpj = company.Cnpj,
                DtOpen = new DateDTO()
                {
                    Year = company.DtOpen.Year,
                    Month = company.DtOpen.Month,
                    Day = company.DtOpen.Day
                },
                Name = company.Name,
                NameOPT = company.NameOPT,
                Status = company.Status
            };
            AirCraftDTO airCraftDTO = new AirCraftDTO
            {
                RAB = RAB,
                Capacity = aircraft.Capacity,
                Company = companyDTO,
                DtLastFlight = new DateDTO
                {
                    Year = aircraft.DtLastFlight.Value.Year,
                    Month = aircraft.DtLastFlight.Value.Month,
                    Day = aircraft.DtLastFlight.Value.Day,
                },
                DtRegistry = new DateDTO
                {
                    Year = aircraft.DtRegistry.Year,
                    Month = aircraft.DtRegistry.Month,
                    Day = aircraft.DtRegistry.Day,
                }
            };

            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(airCraftDTO), Encoding.UTF8, "application/json");
            HttpResponseMessage res = await _httpClient.PutAsync("https://localhost:5000/api/AirCraft/" + RAB, httpContent);
            if (!res.IsSuccessStatusCode) return null;

            string content = await res.Content.ReadAsStringAsync();
            AirCraft? result = JsonConvert.DeserializeObject<AirCraft>(content);

            return result;
        }
    }
}
