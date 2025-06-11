using System.Net.Http.Json;

namespace daytoday.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private string? _token;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("API");
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
                throw new Exception("Login failed");

            _token = await response.Content.ReadFromJsonAsync<string>();
            Preferences.Set("jwt_token", _token); 
            return _token!;
        }

        public string? GetToken() => Preferences.Get("jwt_token", null); 
    }
}
