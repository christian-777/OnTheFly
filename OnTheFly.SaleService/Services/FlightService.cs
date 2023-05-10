using System.Text;
using Microsoft.OpenApi.Extensions;
using MongoDB.Bson;
using Newtonsoft.Json;
using OnTheFly.Models;
using OnTheFly.Models.DTO;

namespace OnTheFly.SaleService.Services
{
    public class FlightService
    {
        private HttpClient _httpClient = new HttpClient();

        public async Task<Flight> GetFlight(string IATA, string RAB, DateDTO departure)
        {
            try
            {
                string date=departure.Year+"-"+departure.Month+"-"+departure.Day;
                HttpResponseMessage res = await _httpClient.GetAsync("https://localhost:5003/api/Flight/"+IATA+","+ RAB+","+ date);
                if (!res.IsSuccessStatusCode) return null;

                string content = await res.Content.ReadAsStringAsync();
                Flight? result = JsonConvert.DeserializeObject<Flight>(content);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Flight> PatchFlight(string IATA, string RAB, BsonDateTime departure, int salesNumber)
        {
            try
            {
                HttpContent httpContent = new StringContent("", Encoding.UTF8, "application/json");
                HttpResponseMessage res = await _httpClient.PatchAsync("https://localhost:5003/api/Flight/" + IATA + ", " + RAB + ", " + departure + ", " + salesNumber, httpContent);
                if (!res.IsSuccessStatusCode) return new Flight();

                string content = await res.Content.ReadAsStringAsync();
                Flight? result = JsonConvert.DeserializeObject<Flight>(content);

                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
