using System.Text;
using Newtonsoft.Json;
using OnTheFly.Models;

namespace OnTheFly.SaleService.Services
{
    public class FlightService
    {
        private HttpClient _httpClient = new HttpClient();

        public async Task<Flight> GetFlight(string IATA, string RAB, DateTime departure)
        {
            try
            {
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
                jsonSettings.DateFormatString = "yyyy-MM-ddThh:mm:ss.fffZ";
                jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                string jsonDate = JsonConvert.SerializeObject(departure, jsonSettings);

                

                HttpResponseMessage res = await _httpClient.GetAsync("https://localhost:5003/api/Flight/" + IATA + ", " + RAB + ", " + jsonDate);
                if (!res.IsSuccessStatusCode) return new Flight();

                string content = await res.Content.ReadAsStringAsync();
                Flight? result = JsonConvert.DeserializeObject<Flight>(content);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Flight> PatchFlight(string IATA, string RAB, DateTime departure, int salesNumber)
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
