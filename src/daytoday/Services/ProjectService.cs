using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using daytoday.Core.DTOs;
using System.Net.Http.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace daytoday.Services
{
    public class ProjectService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public ProjectService(IHttpClientFactory httpClientFactory, AuthService authService)
        {
            _httpClient = httpClientFactory.CreateClient("API");
            _authService = authService;
        }

        public async Task<List<ProjectDto>> GetProjectsAsync()
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("/api/project");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch projects: {response.StatusCode} {response.ReasonPhrase} - {errorContent}");
            }
            var projects = await response.Content.ReadFromJsonAsync<List<ProjectDto>>();
            return projects ?? new List<ProjectDto>();
        }

        public async Task<ProjectDto> GetProjectAsync(Guid id)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/api/project/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch project");
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
            return project ?? throw new Exception("Project not found");
        }

        public async Task<ProjectDto> CreateProjectAsync(ProjectDto project)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsJsonAsync("/api/project", project);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create project: {response.StatusCode} {response.ReasonPhrase} - {responseContent}");
            }
            var projectId = JsonConvert.DeserializeObject<Guid>(responseContent);
            return await GetProjectAsync(projectId);  
        }

        public async Task<ProjectDto> UpdateProjectAsync(ProjectDto project)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PutAsJsonAsync($"/api/project/{project.Id}", project);
            if (!response.IsSuccessStatusCode) { 
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update project: {response.StatusCode} {response.ReasonPhrase} - {errorContent}"); 
            }
            var updatedProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
            return updatedProject ?? throw new Exception("Failed to update project");
        }

        public async Task DeleteProjectAsync(Guid id)
        {
            var token = _authService.GetToken();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"/api/project/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to delete project");
        }
    }
}
