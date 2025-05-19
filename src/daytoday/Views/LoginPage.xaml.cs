using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using daytoday.Services;

namespace daytoday.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = emailEntry.Text?.Trim();
        string password = passwordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            errorLabel.Text = "Podaj email i hasło.";
            errorLabel.IsVisible = true;
            return;
        }

        var loginData = new { Email = email, Password = password };
        string json = JsonSerializer.Serialize(loginData);

        using var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5067/");

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await client.PostAsync("api/Auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JwtLoginResponse>(jsonResult);

                if (!string.IsNullOrEmpty(result?.Token))
                {
                    Preferences.Set("jwt_token", result.Token);
                    
                    Application.Current.MainPage = new AppShell();
                    await Shell.Current.GoToAsync("//Dashboard");
                }
                else
                {
                    errorLabel.Text = "Brak tokenu w odpowiedzi.";
                    errorLabel.IsVisible = true;
                }
            }
            else
            {
                errorLabel.Text = "Logowanie nie powiodło się.";
                errorLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            errorLabel.Text = "Błąd: " + ex.Message;
            errorLabel.IsVisible = true;
        }
    }

    


    private class JwtLoginResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }

}