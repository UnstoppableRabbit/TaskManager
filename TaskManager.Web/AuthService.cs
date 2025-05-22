using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace TaskManager.Web
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly CustomAuthStateProvider _authProvider;

        public AuthService(HttpClient http, AuthenticationStateProvider authProvider)
        {
            _http = http;
            _authProvider = (CustomAuthStateProvider)authProvider;
        }

        public async Task<bool> Login(string login, string password)
        {
            var response = await _http.PostAsJsonAsync("/login", new { login, password });
            if (response.IsSuccessStatusCode)
            {
                await _authProvider.MarkUserAsAuthenticated(login, password);
                return true;
            }

            return false;
        }

        public async Task Logout() => await _authProvider.Logout();

        public async Task<bool> Register(string login, string password, string name)
        {
            var response = await _http.PostAsJsonAsync("/register", new { login, password, name });
            return response.IsSuccessStatusCode;
        }
    }
}
