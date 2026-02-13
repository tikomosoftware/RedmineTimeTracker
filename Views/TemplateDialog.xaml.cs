using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using redmineSupTool.Models;
using redmineSupTool.Services;

namespace redmineSupTool
{
    public partial class TemplateDialog : Window
    {
        private readonly List<Activity> _activities;
        private readonly WorkTemplate? _existingTemplate;
        private readonly DateTime _selectedMonth;
        private readonly List<WorkTemplate> _existingTemplates;

        public new WorkTemplate Template { get; private set; } = new();

        public TemplateDialog(List<Activity> activities, DateTime selectedMonth, 
            WorkTemplate? existingTemplate = null, List<WorkTemplate>? existingTemplates = null)
        {
            InitializeComponent();
            _activities = activities;
            _selectedMonth = selectedMonth;
            _existingTemplate = existingTemplate;
            _existingTemplates = existingTemplates ?? new List<WorkTemplate>();

            InitializeControls();

            if (_existingTemplate != null)
            {
                LoadTemplate(_existingTemplate);
                Title = GetString("TemplateTitleEdit");
            }
        }

        private string GetString(string key, params object[] args)
        {
            var res = Application.Current.TryFindResource(key) as string ?? key;
            return args.Length > 0 ? string.Format(res, args) : res;
        }

        private void InitializeControls()
        {
            // Hours: 0.5 step
            var hours = new List<decimal>();
            for (decimal h = 0.5m; h <= 8.0m; h += 0.5m)
                hours.Add(h);
            HoursComboBox.ItemsSource = hours;
            HoursComboBox.SelectedItem = 0.5m;

            // Activities
            ActivityComboBox.ItemsSource = _activities;
            if (_activities.Count > 0)
                ActivityComboBox.SelectedIndex = 0;

            // Frequency
            FrequencyComboBox.Items.Add(GetString("FreqDaily"));
            FrequencyComboBox.Items.Add(GetString("FreqWeekly"));
            FrequencyComboBox.Items.Add(GetString("FreqMonthly"));
            FrequencyComboBox.SelectedIndex = 0;

            // Weekday panel hidden by default
            WeekdayPanel.Visibility = Visibility.Collapsed;

            // Monthly day selector - show day of week for selected month
            UpdateMonthlyDayList();
            MonthlyDayLabel.Visibility = Visibility.Collapsed;
            MonthlyDayComboBox.Visibility = Visibility.Collapsed;
        }

        private void UpdateMonthlyDayList()
        {
            string[] dayNames = { 
                GetString("DaySun"), 
                GetString("DayMon"), 
                GetString("DayTue"), 
                GetString("DayWed"), 
                GetString("DayThu"), 
                GetString("DayFri"), 
                GetString("DaySat") 
            };
            var daysInMonth = DateTime.DaysInMonth(_selectedMonth.Year, _selectedMonth.Month);
            var days = new List<string> { GetString("DayEndofMonth") };
            for (int d = 1; d <= daysInMonth; d++)
            {
                var date = new DateTime(_selectedMonth.Year, _selectedMonth.Month, d);
                var dow = dayNames[(int)date.DayOfWeek];
                days.Add($"{d}（{dow}）");
            }
            MonthlyDayComboBox.ItemsSource = days;
            MonthlyDayComboBox.SelectedIndex = 0;
        }

        private void LoadTemplate(WorkTemplate template)
        {
            NameTextBox.Text = template.Name;
            IssueIdTextBox.Text = template.IssueId.ToString();
            HoursComboBox.SelectedItem = template.DefaultHours;

            var activity = _activities.FirstOrDefault(a => a.Id == template.ActivityId);
            if (activity != null)
                ActivityComboBox.SelectedItem = activity;

            FrequencyComboBox.SelectedIndex = (int)template.Frequency;

            if (template.Frequency == FrequencyType.Weekly)
            {
                ChkMon.IsChecked = template.TargetDays.Contains(DayOfWeek.Monday);
                ChkTue.IsChecked = template.TargetDays.Contains(DayOfWeek.Tuesday);
                ChkWed.IsChecked = template.TargetDays.Contains(DayOfWeek.Wednesday);
                ChkThu.IsChecked = template.TargetDays.Contains(DayOfWeek.Thursday);
                ChkFri.IsChecked = template.TargetDays.Contains(DayOfWeek.Friday);
            }
            else if (template.Frequency == FrequencyType.Monthly)
            {
                MonthlyDayComboBox.SelectedIndex = template.MonthlyDay; // 0=月末, 1-31=日付
            }
        }

        private void FrequencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WeekdayPanel == null) return;

            // Show weekday panel only for Weekly, monthly day for Monthly
            WeekdayPanel.Visibility = (FrequencyComboBox.SelectedIndex == 1) 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            var showMonthly = (FrequencyComboBox.SelectedIndex == 2);
            MonthlyDayLabel.Visibility = showMonthly ? Visibility.Visible : Visibility.Collapsed;
            MonthlyDayComboBox.Visibility = showMonthly ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(this, GetString("MsgEnterName"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(IssueIdTextBox.Text, out int issueId) || issueId <= 0)
            {
                MessageBox.Show(this, GetString("MsgEnterIssueId"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Duplicate ticket ID check
            var duplicates = _existingTemplates
                .Where(t => t.IssueId == issueId && (_existingTemplate == null || t.Id != _existingTemplate.Id))
                .ToList();
            if (duplicates.Count > 0)
            {
                var names = string.Join(", ", duplicates.Select(t => t.Name));
                var result = MessageBox.Show(this,
                    string.Format(GetString("MsgDuplicateId"), issueId, names),
                    GetString("TitleDuplicate"), MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No) return;
            }

            if (HoursComboBox.SelectedItem == null)
            {
                MessageBox.Show(this, GetString("MsgEnterHours"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ActivityComboBox.SelectedItem == null)
            {
                MessageBox.Show(this, GetString("MsgSelectActivity"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedActivity = (Activity)ActivityComboBox.SelectedItem;
            var frequency = (FrequencyType)FrequencyComboBox.SelectedIndex;

            var targetDays = new List<DayOfWeek>();
            if (frequency == FrequencyType.Weekly)
            {
                if (ChkMon.IsChecked == true) targetDays.Add(DayOfWeek.Monday);
                if (ChkTue.IsChecked == true) targetDays.Add(DayOfWeek.Tuesday);
                if (ChkWed.IsChecked == true) targetDays.Add(DayOfWeek.Wednesday);
                if (ChkThu.IsChecked == true) targetDays.Add(DayOfWeek.Thursday);
                if (ChkFri.IsChecked == true) targetDays.Add(DayOfWeek.Friday);

                if (targetDays.Count == 0)
                {
                    MessageBox.Show(this, GetString("MsgSelectDay"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            Template = new WorkTemplate
            {
                Id = _existingTemplate?.Id ?? Guid.NewGuid(),
                Name = NameTextBox.Text.Trim(),
                IssueId = issueId,
                DefaultHours = (decimal)HoursComboBox.SelectedItem,
                ActivityId = selectedActivity.Id,
                ActivityName = selectedActivity.Name,
                Frequency = frequency,
                TargetDays = targetDays,
                MonthlyDay = (frequency == FrequencyType.Monthly) ? MonthlyDayComboBox.SelectedIndex : 0,
                IsEnabled = _existingTemplate?.IsEnabled ?? true
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
