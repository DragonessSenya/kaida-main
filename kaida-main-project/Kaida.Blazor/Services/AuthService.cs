using Blazored.LocalStorage;
using Kaida.Blazor.Models;

namespace Kaida.Blazor.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<bool> LoginAsync(LoginModel model)
        {
            var response = await _http.PostAsJsonAsync("https://your-auth-server.com/api/auth/login", model);
            if (!response.IsSuccessStatusCode) return false;

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (token?.Token == null) return false;

            await _localStorage.SetItemAsync("authToken", token.Token);
            return true;
        }
    }

}
