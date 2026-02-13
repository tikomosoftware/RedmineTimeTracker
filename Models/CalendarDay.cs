using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace redmineSupTool.Models
{
    /// <summary>
    /// カレンダーの1日分のデータ
    /// </summary>
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        
        public bool IsWeekend { get; set; }
        
        public bool IsHoliday { get; set; }
        
        public bool IsExcluded { get; set; }
        
        public List<TimeEntryInfo> Entries { get; set; } = new();
        
        /// <summary>
        /// 登録状況: None=未登録, Partial=一部登録, Complete=すべて登録済み
        /// </summary>
        public RegistrationStatus Status { get; set; }
    }
    
    public enum RegistrationStatus
    {
        None,       // 未登録
        Partial,    // 一部登録済み
        Complete    // 全て登録済み
    }
}
