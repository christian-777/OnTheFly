using System.Text;
using Newtonsoft.Json;
using OnTheFly.Models;

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

        public async Task<AirCraft?> UpdateAircraft(string RAB, DateTime date)
        {
            // Configurando json para formatação de data no padrão UTC
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-ddThh:mm:ss.fffZ";
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            string jsonDate = JsonConvert.SerializeObject(date, jsonSettings);

            HttpContent httpContent = new StringContent(jsonDate, Encoding.UTF8, "application/json");

            HttpResponseMessage res = await _httpClient.PatchAsync("https://localhost:5000/api/AirCraft/" + RAB, httpContent);
            if (!res.IsSuccessStatusCode) return null;

            string content = await res.Content.ReadAsStringAsync();
            AirCraft? result = JsonConvert.DeserializeObject<AirCraft>(content);

            return result;
        }
    }
}
