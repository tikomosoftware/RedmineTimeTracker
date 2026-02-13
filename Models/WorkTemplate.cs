using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace redmineSupTool.Models
{
    /// <summary>
    /// 作業テンプレート（定期的な会議など）
    /// </summary>
    public class WorkTemplate
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("issueId")]
        public int IssueId { get; set; }
        
        [JsonPropertyName("defaultHours")]
        public decimal DefaultHours { get; set; }
        
        [JsonPropertyName("activityId")]
        public int ActivityId { get; set; }
        
        [JsonPropertyName("activityName")]
        public string ActivityName { get; set; } = string.Empty;
        
        [JsonPropertyName("frequency")]
        public FrequencyType Frequency { get; set; }
        
        [JsonPropertyName("targetDays")]
        public List<DayOfWeek> TargetDays { get; set; } = new();
        
        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; } = true;

        [JsonPropertyName("monthlyDay")]
        public int MonthlyDay { get; set; } = 0;  // 0=月末, 1-31=指定日

        [JsonIgnore]
        public string FrequencyDisplay => Frequency switch
        {
            FrequencyType.Daily => "毎日 (月～金)",
            FrequencyType.Weekly => "週次（" + string.Join(",", TargetDays.ConvertAll(d => d switch {
                DayOfWeek.Monday => "月",
                DayOfWeek.Tuesday => "火",
                DayOfWeek.Wednesday => "水",
                DayOfWeek.Thursday => "木",
                DayOfWeek.Friday => "金",
                _ => d.ToString()
            })) + "）",
            FrequencyType.Monthly => MonthlyDay == 0 ? "月次（月末）" : $"月次（{MonthlyDay}日）",
            _ => ""
        };
    }
    
    public enum FrequencyType
    {
        Daily,      // 毎日（平日のみ）
        Weekly,     // 毎週（特定曜日）
        Monthly     // 月次
    }
}
