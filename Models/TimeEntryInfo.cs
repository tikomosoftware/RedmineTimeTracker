using System;
using System.Text.Json.Serialization;

namespace redmineSupTool.Models
{
    /// <summary>
    /// 時間登録情報
    /// </summary>
    public class TimeEntryInfo
    {
        /// <summary>
        /// Redmineの時間登録ID（null=未登録）
        /// </summary>
        public int? TimeEntryId { get; set; }
        
        public int IssueId { get; set; }
        
        public decimal Hours { get; set; }
        
        public int ActivityId { get; set; }
        
        public string ActivityName { get; set; } = string.Empty;
        
        public string Comments { get; set; } = string.Empty;
        
        public DateTime SpentOn { get; set; }
    }
}
