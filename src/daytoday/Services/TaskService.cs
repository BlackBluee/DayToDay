using daytoday.Core.DTOs;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace daytoday.Services
{
    public class TaskService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public TaskService(IHttpClientFactory httpClientFactory, AuthService authService)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _authService = authService;
        }

        public async Task<List<TaskDto>> GetTasksAsync(Guid projectId)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/api/task?projectId={projectId}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch tasks: {response.StatusCode} {response.ReasonPhrase} - {errorContent}");
            }
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
            return tasks ?? new List<TaskDto>();
        }

        public async Task<TaskDto> GetTaskAsync(Guid id)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/api/task/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch task");
            var task = await response.Content.ReadFromJsonAsync<TaskDto>();
            return task ?? throw new Exception("Task not found");
        }

        public async Task<TaskDto> CreateTaskAsync(TaskDto task)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            task.Id = Guid.Empty;

            var response = await _httpClient.PostAsJsonAsync("/api/task", task);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create task: {response.StatusCode} {response.ReasonPhrase} - {responseContent}");
            }

            var createdTask = JsonConvert.DeserializeObject<TaskDto>(responseContent);
            if (createdTask == null)
                throw new Exception("Failed to deserialize created task.");

            return createdTask;
        }

        public async Task<TaskDto> UpdateTaskAsync(TaskDto task)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PutAsJsonAsync($"/api/task/{task.Id}", task);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update task: {response.StatusCode} {response.ReasonPhrase} - {errorContent}");
            }
            return await GetTaskAsync(task.Id);
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"/api/task/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete task: {response.StatusCode} {response.ReasonPhrase} - {errorContent}");
            }
        }
    }
}
