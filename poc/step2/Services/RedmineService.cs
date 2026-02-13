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

        public async Task<string> CreateTimeEntryAsync(int issueId, decimal hours, int activityId, string? comments = null, string? spentOn = null)
        {
            var output = new System.Text.StringBuilder();
            
            try
            {
                string url = $"{_settings.BaseUrl.TrimEnd('/')}/time_entries.json";
                output.AppendLine($"Creating time entry: {url}");
                output.AppendLine($"  Issue ID: {issueId}");
                output.AppendLine($"  Hours: {hours}");
                output.AppendLine($"  Activity ID: {activityId}");
                if (!string.IsNullOrEmpty(comments))
                    output.AppendLine($"  Comments: {comments}");
                if (!string.IsNullOrEmpty(spentOn))
                    output.AppendLine($"  Spent On: {spentOn}");
                output.AppendLine();

                var requestData = new TimeEntryRequest
                {
                    TimeEntry = new TimeEntryData
                    {
                        IssueId = issueId,
                        Hours = hours,
                        ActivityId = activityId,
                        Comments = comments,
                        SpentOn = spentOn
                    }
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var jsonContent = JsonSerializer.Serialize(requestData, jsonOptions);
                output.AppendLine("Request JSON:");
                output.AppendLine(jsonContent);
                output.AppendLine();

                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                
                var responseString = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    output.AppendLine("✓ Success!");
                    output.AppendLine("Response:");
                    output.AppendLine(responseString);
                    
                    var result = JsonSerializer.Deserialize<TimeEntryResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result?.TimeEntry != null)
                    {
                        output.AppendLine();
                        output.AppendLine($"Time Entry ID: {result.TimeEntry.Id}");
                        output.AppendLine($"Created on: {result.TimeEntry.CreatedOn}");
                    }
                }
                else
                {
                    output.AppendLine($"✗ Error: HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                    output.AppendLine("Response:");
                    output.AppendLine(responseString);
                }
            }
            catch (Exception ex)
            {
                output.AppendLine($"✗ Exception: {ex.Message}");
                throw;
            }
            
            return output.ToString();
        }
    }

    // Time Entry Request/Response Models
    public class TimeEntryRequest
    {
        [JsonPropertyName("time_entry")]
        public TimeEntryData TimeEntry { get; set; } = new();
    }

    public class TimeEntryData
    {
        [JsonPropertyName("issue_id")]
        public int IssueId { get; set; }
        
        [JsonPropertyName("hours")]
        public decimal Hours { get; set; }
        
        [JsonPropertyName("activity_id")]
        public int ActivityId { get; set; }
        
        [JsonPropertyName("comments")]
        public string? Comments { get; set; }
        
        [JsonPropertyName("spent_on")]
        public string? SpentOn { get; set; }
    }

    public class TimeEntryResponse
    {
        [JsonPropertyName("time_entry")]
        public TimeEntryResult? TimeEntry { get; set; }
    }

    public class TimeEntryResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("created_on")]
        public string CreatedOn { get; set; } = string.Empty;
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
