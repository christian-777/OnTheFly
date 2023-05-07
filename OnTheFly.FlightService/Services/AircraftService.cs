using Newtonsoft.Json;
using OnTheFly.Models;

namespace OnTheFly.FlightService.Services
{
    public class AircraftService
    {
        private HttpClient _httpClient = new HttpClient();

        public async Task<AirCraft> GetAircraft(string RAB)
        {
            HttpResponseMessage res = await _httpClient.GetAsync("https://localhost:5000/ByIATA/" + RAB);
            if (!res.IsSuccessStatusCode) return new AirCraft();

            string content = await res.Content.ReadAsStringAsync();
            AirCraft? result = JsonConvert.DeserializeObject<AirCraft>(content);

            return result;
        }
    }
}
