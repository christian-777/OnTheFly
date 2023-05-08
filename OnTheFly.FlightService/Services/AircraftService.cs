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

        public async Task<AirCraft?> UpdateAircraft(string RAB, string date)
        {
            HttpResponseMessage res = await _httpClient.PutAsync("https://localhost:5000/api/AirCraft/" + RAB + ", " + date, null);
            if (!res.IsSuccessStatusCode) return null;

            string content = await res.Content.ReadAsStringAsync();
            AirCraft? result = JsonConvert.DeserializeObject<AirCraft>(content);

            return result;
        }
    }
}
