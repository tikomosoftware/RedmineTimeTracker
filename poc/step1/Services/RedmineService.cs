using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace redmineSupTool.Services
{
    public class RedmineSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:8080";
        public string ApiKey { get; set; } = "YOUR_REDMINE_API_KEY";
    }

    public class RedmineService
    {
        private readonly HttpClient _httpClient;
        private readonly RedmineSettings _settings;

        public RedmineService(RedmineSettings settings)
        {
            _settings = settings;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Redmine-API-Key", _settings.ApiKey);
        }

        public async Task<string> GetProjectsAsync()
        {
            var output = new System.Text.StringBuilder();
            
            try
            {
                string url = $"{_settings.BaseUrl.TrimEnd('/')}/projects.json";
                output.AppendLine($"Connecting to: {url}");
                output.AppendLine();

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();
                
                output.AppendLine("--- Projects JSON Response ---");
                output.AppendLine(jsonString);
                output.AppendLine("------------------------------");
                output.AppendLine();

                // デシリアライズのテスト（構造は最小限）
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var result = JsonSerializer.Deserialize<ProjectListResponse>(jsonString, options);

                if (result?.Projects != null)
                {
                    output.AppendLine($"Found {result.Projects.Count} projects:");
                    output.AppendLine();
                    foreach (var project in result.Projects)
                    {
                        output.AppendLine($"  • {project.Name} (ID: {project.Id})");
                    }
                }
            }
            catch (Exception ex)
            {
                output.AppendLine($"Error: {ex.Message}");
                throw;
            }
            
            return output.ToString();
        }
    }

    public class ProjectListResponse
    {
        [JsonPropertyName("projects")]
        public List<Project> Projects { get; set; } = new();

        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }
    }

    public class Project
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; } = string.Empty;
    }
}
