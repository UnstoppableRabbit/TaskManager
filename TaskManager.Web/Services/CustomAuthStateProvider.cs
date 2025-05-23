using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace TaskManager.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var login = await _localStorage.GetItemAsync<string>("login");
            var password = await _localStorage.GetItemAsync<string>("password");

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name.ToString(), login) }, "Basic");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task MarkUserAsAuthenticated(string login, string password)
        {
            await _localStorage.SetItemAsync("login", login);
            await _localStorage.SetItemAsync("password", password);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("login");
            await _localStorage.RemoveItemAsync("password");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
