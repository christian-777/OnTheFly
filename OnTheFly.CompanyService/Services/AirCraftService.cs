using Newtonsoft.Json;
using OnTheFly.Models;
using OnTheFly.Models.DTO;
using System.Text;
namespace OnTheFly.CompanyService.Services
{
    public class AircraftService
    {
        private HttpClient _httpClient = new HttpClient();
        public async Task<AirCraft> InsertAircraft(AirCraftDTO airCraftDTO)
        {
            HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(airCraftDTO), Encoding.UTF8, "application/json");
            HttpResponseMessage res = await _httpClient.PostAsync("https://localhost:5000/api/AirCraft/", httpContent);
            if (!res.IsSuccessStatusCode) return null;
            string content = await res.Content.ReadAsStringAsync();
            AirCraft? result = JsonConvert.DeserializeObject<AirCraft>(content);

            return result;
        }
    }
}