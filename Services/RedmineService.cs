using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace redmineSupTool.Services
{
    public class RedmineSettings
    {
        private static readonly string SettingsDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RedmineSupTool");
        private static readonly string SettingsPath = System.IO.Path.Combine(SettingsDir, "settings.json");

        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(BaseUrl) && !string.IsNullOrWhiteSpace(ApiKey);

        public void Save()
        {
            System.IO.Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(SettingsPath, json);
        }

        public static RedmineSettings Load()
        {
            if (System.IO.File.Exists(SettingsPath))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<RedmineSettings>(json) ?? new RedmineSettings();
                }
                catch { }
            }
            return new RedmineSettings();
        }
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


        public async Task<string> GetCurrentUserAsync()
        {
            string url = $"{_settings.BaseUrl.TrimEnd('/')}/users/current.json";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
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

        public async Task<List<Activity>> GetActivitiesAsync()
        {
            string url = $"{_settings.BaseUrl.TrimEnd('/')}/enumerations/time_entry_activities.json";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ActivitiesResponse>(jsonString, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return result?.TimeEntryActivities?.Where(a => a.Active).ToList() ?? new List<Activity>();
        }

        public async Task<List<TimeEntry>> GetTimeEntriesAsync(DateTime startDate, DateTime endDate)
        {
            var allEntries = new List<TimeEntry>();
            int offset = 0;
            int limit = 100;

            while (true)
            {
                var url = $"{_settings.BaseUrl.TrimEnd('/')}/time_entries.json" +
                          $"?user_id=me" +
                          $"&spent_on=%3E%3C{startDate:yyyy-MM-dd}%7C{endDate:yyyy-MM-dd}" +
                          $"&limit={limit}" +
                          $"&offset={offset}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TimeEntriesResponse>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.TimeEntries == null || result.TimeEntries.Count == 0)
                    break;

                allEntries.AddRange(result.TimeEntries);

                if (result.TimeEntries.Count < limit)
                    break;

                offset += limit;
            }

            return allEntries;
        }

        public async Task<string> UpdateTimeEntryAsync(int timeEntryId, decimal hours, int activityId, string? comments = null)
        {
            var output = new System.Text.StringBuilder();
            
            try
            {
                string url = $"{_settings.BaseUrl.TrimEnd('/')}/time_entries/{timeEntryId}.json";
                output.AppendLine($"Updating time entry #{timeEntryId}: {url}");

                var requestData = new TimeEntryRequest
                {
                    TimeEntry = new TimeEntryData
                    {
                        Hours = hours,
                        ActivityId = activityId,
                        Comments = comments
                    }
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var jsonContent = JsonSerializer.Serialize(requestData, jsonOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, content);
                
                var responseString = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    output.AppendLine("✓ Updated!");
                }
                else
                {
                    output.AppendLine($"✗ Error: HTTP {(int)response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                output.AppendLine($"✗ Exception: {ex.Message}");
                throw;
            }
            
            return output.ToString();
        }

        public async Task<IssueInfo?> GetIssueAsync(int issueId)
        {
            try
            {
                string url = $"{_settings.BaseUrl.TrimEnd('/')}/issues/{issueId}.json";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<IssueResponse>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.Issue;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int?> CreateIssueAsync(IssueData issue)
        {
            try
            {
                string url = $"{_settings.BaseUrl.TrimEnd('/')}/issues.json";
                var requestData = new IssueRequest { Issue = issue };
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var jsonContent = JsonSerializer.Serialize(requestData, jsonOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<IssueResponse>(responseString,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return result?.Issue?.Id;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    // Time Entry Models for GET API
    public class TimeEntriesResponse
    {
        [JsonPropertyName("time_entries")]
        public List<TimeEntry>? TimeEntries { get; set; }
    }

    public class TimeEntry
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("issue")]
        public IssueReference Issue { get; set; } = new();
        
        [JsonPropertyName("hours")]
        public decimal Hours { get; set; }
        
        [JsonPropertyName("activity")]
        public ActivityReference Activity { get; set; } = new();
        
        [JsonPropertyName("comments")]
        public string Comments { get; set; } = string.Empty;
        
        [JsonPropertyName("spent_on")]
        public string SpentOn { get; set; } = string.Empty;
    }

    public class IssueReference
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class ActivityReference
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    // Activity Models
    public class ActivitiesResponse
    {
        [JsonPropertyName("time_entry_activities")]
        public List<Activity>? TimeEntryActivities { get; set; }
    }

    public class Activity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
        
        [JsonPropertyName("active")]
        public bool Active { get; set; }
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

    // Issue Models
    public class IssueResponse
    {
        [JsonPropertyName("issue")]
        public IssueInfo? Issue { get; set; }
    }

    public class IssueInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("project")]
        public ProjectReference? Project { get; set; }

        [JsonPropertyName("tracker")]
        public TrackerReference? Tracker { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;
    }

    public class ProjectReference
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class TrackerReference
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class IssueRequest
    {
        [JsonPropertyName("issue")]
        public IssueData Issue { get; set; } = new();
    }

    public class IssueData
    {
        [JsonPropertyName("project_id")]
        public int? ProjectId { get; set; }

        [JsonPropertyName("parent_issue_id")]
        public int? ParentIssueId { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("tracker_id")]
        public int? TrackerId { get; set; }
    }
}
