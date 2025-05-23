using Domain.Enums;
using Domain.Model;
using System.Net.Http.Json;

namespace TaskManager.Web.Services
{
    public class UserApiService
    {
        private readonly HttpClient _http;

        public UserApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var url = "/currentUser";

            return await _http.GetFromJsonAsync<User>(url);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            var url = "/allUsers";

            return await _http.GetFromJsonAsync<List<User>>(url);
        }
    }
}
