using daytoday.Core.DTOs;
using System.Net.Http.Json;

namespace daytoday.Services
{
    public class CalendarEventService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public CalendarEventService(IHttpClientFactory httpClientFactory, AuthService authService)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _authService = authService;
        }

        public async Task<List<CalendarEventDto>> GetCalendarEventsAsync()
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/CalendarEvent");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch calendar events: {response.StatusCode} {response.ReasonPhrase} - {errorContent}");
            }
            var calendarEvents = await response.Content.ReadFromJsonAsync<List<CalendarEventDto>>();
            return calendarEvents ?? new List<CalendarEventDto>();
        }

        public async Task<CalendarEventDto> GetCalendarEventAsync(Guid id)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/api/CalendarEvent/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch calendar event");
            var calendarEvent = await response.Content.ReadFromJsonAsync<CalendarEventDto>();
            return calendarEvent ?? throw new Exception("Calendar event not found");
        }

        public async Task<CalendarEventDto> CreateCalendarEventAsync(CalendarEventDto calendarEvent)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsJsonAsync("/api/CalendarEvent", calendarEvent);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to create calendar event: {response.StatusCode} {response.ReasonPhrase} - {responseContent}");
            var createdEvent = await response.Content.ReadFromJsonAsync<CalendarEventDto>();
            return createdEvent ?? throw new Exception("Failed to create calendar event");
        }

        public async Task<CalendarEventDto> UpdateCalendarEventAsync(CalendarEventDto calendarEvent)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PutAsJsonAsync($"/api/CalendarEvent/{calendarEvent.Id}", calendarEvent);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to update calendar event: {response.StatusCode} {response.ReasonPhrase} - {responseContent}");
            var updatedEvent = await response.Content.ReadFromJsonAsync<CalendarEventDto>();
            return updatedEvent ?? throw new Exception("Failed to update calendar event");
        }

        public async Task DeleteCalendarEventAsync(Guid id)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"/api/CalendarEvent/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete calendar event: {response.StatusCode} {response.ReasonPhrase} - {errorContent}");
            }
        }

    }
}
