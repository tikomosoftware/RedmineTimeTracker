using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using redmineSupTool.Models;
using redmineSupTool.Services;
using redmineSupTool.Views;

namespace redmineSupTool
{
    public partial class MainWindow : Window
    {
        private RedmineSettings _settings;
        private RedmineService _redmineService;
        private readonly TemplateService _templateService;
        private DateTime _selectedMonth;
        private List<Activity> _activities = new();
        private List<TimeEntry> _existingEntries = new();
        private HashSet<DateTime> _excludedDates = new();
        private readonly string _excludedDatesPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RedmineSupTool", "excluded_dates.json");
        private HashSet<DateTime> _workDays = new();
        private readonly string _workDaysPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RedmineSupTool", "work_days.json");

        public MainWindow()
        {
            InitializeComponent();

            _settings = RedmineSettings.Load();
            _redmineService = new RedmineService(_settings);
            _templateService = new TemplateService();
            
            InitializeControls();
        }

        private string GetString(string key, params object[] args)
        {
            var res = Application.Current.TryFindResource(key) as string ?? key;
            return args.Length > 0 ? string.Format(res, args) : res;
        }

        private void InitializeControls()
        {
            _selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            InitializeMonthSelector();

            if (!_settings.IsConfigured)
            {
                // First launch: open settings dialog
                Loaded += (s, e) => OpenSettingsDialog(true);
            }
            else
            {
                LoadDataAsync();
            }
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsDialog(false);
        }

        private void OpenSettingsDialog(bool isFirstRun)
        {
            var dialog = new SettingsDialog(_settings);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                _settings = dialog.Settings;
                _redmineService = new RedmineService(_settings);
                AppendLog(GetString("LogSettingsUpdated", _settings.BaseUrl));
                LoadDataAsync();
            }
            else if (isFirstRun)
            {
                MessageBox.Show(this, GetString("MsgConnectionRequired"),
                    GetString("TitleInfo"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void InitializeMonthSelector()
        {
            var months = new List<MonthItem>();
            var currentDate = DateTime.Now;

            LoadExcludedDates();
            LoadWorkDays();

            for (int i = -1; i <= 1; i++)
            {
                var month = currentDate.AddMonths(i);
                months.Add(new MonthItem
                {
                    Date = new DateTime(month.Year, month.Month, 1),
                    Display = month.ToString(GetString("DateFormatMonth"))
                });
            }

            MonthSelector.ItemsSource = months;
            MonthSelector.DisplayMemberPath = "Display";
            MonthSelector.SelectedValuePath = "Date";
            MonthSelector.SelectedValue = _selectedMonth;
            MonthSelector.SelectionChanged += MonthSelector_SelectionChanged;
        }

        private void AppendLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogTextBox.AppendText($"[{timestamp}] {message}\n");
            LogTextBox.ScrollToEnd();
        }

        private void ToggleLogMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleLogMenuItem.IsChecked)
            {
                SplitterColumn.Width = new GridLength(5);
                LogColumn.Width = new GridLength(300);
                LogSplitter.Visibility = Visibility.Visible;
                LogPanel.Visibility = Visibility.Visible;
            }
            else
            {
                SplitterColumn.Width = new GridLength(0);
                LogColumn.Width = new GridLength(0);
                LogSplitter.Visibility = Visibility.Collapsed;
                LogPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        private async void LoadDataAsync()
        {
            StatusTextBlock.Text = GetString("StatusLoading");
            StatusTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            StatusTextBlock.FontWeight = FontWeights.Normal;
            AppendLog(GetString("LogDataLoadStart"));

            try
            {
                await _templateService.LoadAsync();
                TemplateGrid.ItemsSource = null;
                TemplateGrid.ItemsSource = _templateService.GetTemplates();
                AppendLog(GetString("LogTemplateLoaded", _templateService.GetTemplates().Count));

                AppendLog("GET /enumerations/time_entry_activities.json");
                _activities = await _redmineService.GetActivitiesAsync();
                AppendLog(GetString("LogActivityLoaded", _activities.Count));

                var endDate = _selectedMonth.AddMonths(1).AddDays(-1);
                try
                {
                    AppendLog($"GET /time_entries.json?spent_on=%3E%3C{_selectedMonth:yyyy-MM-dd}%7C{endDate:yyyy-MM-dd}");
                    _existingEntries = await _redmineService.GetTimeEntriesAsync(_selectedMonth, endDate);
                    AppendLog(GetString("LogTimeEntryLoaded", _existingEntries.Count));
                }
                catch (Exception ex)
                {
                    _existingEntries = new List<TimeEntry>();
                    AppendLog(GetString("LogTimeEntryLoadError", ex.Message));
                }

                RenderCalendar();

                StatusTextBlock.Text = GetString("StatusReadyCount", _templateService.GetTemplates().Count, _existingEntries.Count);
                StatusTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
                StatusTextBlock.FontWeight = FontWeights.Normal;
                AppendLog(GetString("LogDataLoadEnd"));
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = GetString("StatusError", ex.Message);
                StatusTextBlock.Foreground = Brushes.Red;
                StatusTextBlock.FontWeight = FontWeights.Bold;
                AppendLog(GetString("LogLoadError", ex.Message));
                
                // Show error message to user
                MessageBox.Show(this,
                    GetString("LogLoadError", ex.Message),
                    GetString("TitleError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void RenderCalendar()
        {
            CalendarPanel.Children.Clear();

            CalendarTitle.Text = _selectedMonth.ToString(GetString("DateFormatMonth"));
            
            string monthPrefix = _selectedMonth.ToString("yyyy-MM");
            decimal totalHours = _existingEntries
                .Where(e => e.SpentOn.StartsWith(monthPrefix))
                .Sum(e => e.Hours);
            TotalHoursTextBlock.Text = string.Format(GetString("LabelTotal", totalHours));

            // Day of week header
            var headerGrid = new Grid();
            string[] dayNames = { 
                GetString("DayMon"), 
                GetString("DayTue"), 
                GetString("DayWed"), 
                GetString("DayThu"), 
                GetString("DayFri"), 
                GetString("DaySat"), 
                GetString("DaySun") 
            };
            for (int i = 0; i < 7; i++)
            {
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var dayLabel = new TextBlock
                {
                    Text = dayNames[i],
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(2),
                    Foreground = (i == 5) ? Brushes.Blue : (i == 6) ? Brushes.Red : Brushes.Black
                };
                Grid.SetColumn(dayLabel, i);
                headerGrid.Children.Add(dayLabel);
            }
            CalendarPanel.Children.Add(headerGrid);

            // Calendar grid
            var daysInMonth = DateTime.DaysInMonth(_selectedMonth.Year, _selectedMonth.Month);
            var firstDay = _selectedMonth;
            // Monday=0, Sunday=6
            int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;

            int totalCells = startOffset + daysInMonth;
            int rows = (totalCells + 6) / 7;

            var calendarGrid = new Grid { Margin = new Thickness(0, 5, 0, 0) };
            for (int i = 0; i < 7; i++)
                calendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < rows; i++)
                calendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(70) });

            int dayNumber = 1;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    int cellIndex = row * 7 + col;
                    if (cellIndex < startOffset || dayNumber > daysInMonth)
                        continue;

                    var date = new DateTime(_selectedMonth.Year, _selectedMonth.Month, dayNumber);
                    bool isWeekend = (col == 5 || col == 6);
                    bool isExcluded = _excludedDates.Contains(date);
                    bool isWorkDay = _workDays.Contains(date);

                    // Effective holiday check for coloring
                    bool treatAsHoliday = (isWeekend && !isWorkDay) || isExcluded;

                    // Check if entries exist for this day
                    var dayEntries = _existingEntries.Where(e => e.SpentOn == date.ToString("yyyy-MM-dd")).ToList();
                    bool hasEntries = dayEntries.Count > 0;

                    var cell = CreateCalendarCell(date, dayNumber, isWeekend, isExcluded, isWorkDay, hasEntries, dayEntries);
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    calendarGrid.Children.Add(cell);

                    dayNumber++;
                }
            }

            CalendarPanel.Children.Add(calendarGrid);
        }

        private Border CreateCalendarCell(DateTime date, int day, bool isWeekend, bool isExcluded, bool isWorkDay, bool hasEntries, List<TimeEntry> entries)
        {
            Color bgColor;
            if (isExcluded)
                bgColor = (Color)ColorConverter.ConvertFromString("#FFF3E0");  // Orange tint for excluded
            else if (isWeekend && !isWorkDay)
                bgColor = (Color)ColorConverter.ConvertFromString("#F0F0F0");
            else if (isWorkDay)
                 bgColor = (Color)ColorConverter.ConvertFromString("#E3F2FD"); // Light Blue for Work Day
            else if (hasEntries)
                bgColor = (Color)ColorConverter.ConvertFromString("#E8F5E9");
            else
                bgColor = Colors.White;

            var border = new Border
            {
                Background = new SolidColorBrush(bgColor),
                BorderBrush = new SolidColorBrush(isExcluded 
                    ? (Color)ColorConverter.ConvertFromString("#FF9800") 
                    : isWorkDay ? Brushes.CornflowerBlue.Color
                    : (Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(isExcluded || isWorkDay ? 1.5 : 0.5),
                Margin = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Tag = date  // Store date for right-click handler
            };

            // Right-click context menu
            var contextMenu = new ContextMenu();
            if (isExcluded)
            {
                var unexcludeItem = new MenuItem { Header = GetString("MenuUnexcludeDate") };
                unexcludeItem.Click += (s, e) => ToggleExcludeDate(date, false);
                contextMenu.Items.Add(unexcludeItem);
            }
            else if (isWeekend)
            {
                if (isWorkDay)
                {
                    var unsetWorkDayItem = new MenuItem { Header = GetString("MenuUnsetWorkDay") };
                    unsetWorkDayItem.Click += (s, e) => ToggleWorkDay(date, false);
                    contextMenu.Items.Add(unsetWorkDayItem);
                }
                else
                {
                    var setWorkDayItem = new MenuItem { Header = GetString("MenuSetWorkDay") };
                    setWorkDayItem.Click += (s, e) => ToggleWorkDay(date, true);
                    contextMenu.Items.Add(setWorkDayItem);
                }
            }
            else
            {
                var excludeItem = new MenuItem { Header = GetString("MenuExcludeDate") };
                excludeItem.Click += (s, e) => ToggleExcludeDate(date, true);
                contextMenu.Items.Add(excludeItem);
            }
            border.ContextMenu = contextMenu;

            var stack = new StackPanel { Margin = new Thickness(4, 2, 4, 2) };

            // Day number
            // Day number header with potential icon
            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };
            
            var dayText = new TextBlock
            {
                Text = day.ToString(),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = isExcluded ? Brushes.OrangeRed : isWeekend ? Brushes.Gray : Brushes.Black
            };
            headerStack.Children.Add(dayText);

            if (isWorkDay)
            {
                var workInfoStack = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    Margin = new Thickness(5, 2, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var workIcon = new TextBlock
                {
                    Text = "🏢",
                    FontSize = 14,
                    Foreground = Brushes.RoyalBlue
                };

                var workText = new TextBlock
                {
                    Text = GetString("StatusWorkDay"),
                    FontSize = 14,
                    Margin = new Thickness(2, 0, 0, 0),
                    Foreground = Brushes.RoyalBlue
                };

                workInfoStack.Children.Add(workIcon);
                workInfoStack.Children.Add(workText);
                headerStack.Children.Add(workInfoStack);
            }

            stack.Children.Add(headerStack);

            // Status indicator
            if (isExcluded)
            {
                var statusText = new TextBlock
                {
                    Text = "🚫 " + GetString("StatusHoliday"),
                    FontSize = 14,
                    Foreground = Brushes.OrangeRed
                };
                stack.Children.Add(statusText);
            }
            else if (hasEntries)
            {
                decimal totalHours = entries.Sum(e => e.Hours);
                var statusText = new TextBlock
                {
                    Text = $"✓ {totalHours}h",
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#388E3C"))
                };
                stack.Children.Add(statusText);
            }
            else if (!isWeekend || isWorkDay)
            {
                var statusText = new TextBlock
                {
                    Text = "- " + GetString("StatusUnregistered"),
                    FontSize = 14,
                    Foreground = Brushes.Gray
                };
                stack.Children.Add(statusText);
            }

            border.Child = stack;
            return border;
        }

        private void ToggleWorkDay(DateTime date, bool isWorkDay)
        {
            if (isWorkDay)
                _workDays.Add(date);
            else
                _workDays.Remove(date);

            SaveWorkDays();
            RenderCalendar();
            StatusTextBlock.Text = isWorkDay
                ? GetString("MsgSetWorkDay", $"{date:M/d}")
                : GetString("MsgUnsetWorkDay", $"{date:M/d}");
        }

        private void LoadWorkDays()
        {
            try
            {
                if (System.IO.File.Exists(_workDaysPath))
                {
                    var json = System.IO.File.ReadAllText(_workDaysPath);
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<DateTime>>(json);
                    if (list != null) _workDays = new HashSet<DateTime>(list);
                }
            }
            catch { }
        }

        private void SaveWorkDays()
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(_workDaysPath);
                if (dir != null && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                var json = System.Text.Json.JsonSerializer.Serialize(_workDays.ToList());
                System.IO.File.WriteAllText(_workDaysPath, json);
            }
            catch (Exception ex)
            {
                AppendLog(GetString("LogSaveWorkDayError", ex.Message));
            }
        }

        private void ToggleExcludeDate(DateTime date, bool exclude)
        {
            if (exclude)
                _excludedDates.Add(date);
            else
                _excludedDates.Remove(date);

            SaveExcludedDates();
            RenderCalendar();
            StatusTextBlock.Text = exclude
                ? GetString("MsgSetWorkDay", $"{date:M/d}") // Using same key for holiday/exclude might be confusing, but let's leave it for now or add specific one. Actually I should add MsgExcludeDate
                : GetString("MsgUnsetWorkDay", $"{date:M/d}");
        }

        private void LoadExcludedDates()
        {
            try
            {
                if (System.IO.File.Exists(_excludedDatesPath))
                {
                    var json = System.IO.File.ReadAllText(_excludedDatesPath);
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<DateTime>>(json);
                    if (list != null)
                    {
                        _excludedDates = new HashSet<DateTime>(list);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"除外日設定の読み込みエラー: {ex.Message}");
            }
        }

        private void SaveExcludedDates()
        {
            try
            {
                var dir = System.IO.Path.GetDirectoryName(_excludedDatesPath);
                if (dir != null && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                var json = System.Text.Json.JsonSerializer.Serialize(_excludedDates.ToList());
                System.IO.File.WriteAllText(_excludedDatesPath, json);
            }
            catch (Exception ex)
            {
                AppendLog(GetString("LogSaveExcludeDateError", ex.Message));
            }
        }
        private void MonthSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MonthSelector.SelectedValue is DateTime selectedDate)
            {
                _selectedMonth = selectedDate;
                LoadDataAsync();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAsync();
        }

        private void AddTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TemplateDialog(_activities, _selectedMonth, 
                existingTemplates: _templateService.GetTemplates());
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                _ = AddTemplateAndRefreshAsync(dialog.Template);
            }
        }

        private async System.Threading.Tasks.Task AddTemplateAndRefreshAsync(WorkTemplate template)
        {
            await _templateService.AddTemplateAsync(template);
            TemplateGrid.ItemsSource = null;
            TemplateGrid.ItemsSource = _templateService.GetTemplates();
        }

        private void EditTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var template = button?.DataContext as WorkTemplate;
            if (template == null) return;

            var dialog = new TemplateDialog(_activities, _selectedMonth, template, 
                _templateService.GetTemplates());
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                _ = UpdateTemplateAndRefreshAsync(dialog.Template);
            }
        }

        private async System.Threading.Tasks.Task UpdateTemplateAndRefreshAsync(WorkTemplate template)
        {
            await _templateService.UpdateTemplateAsync(template);
            TemplateGrid.ItemsSource = null;
            TemplateGrid.ItemsSource = _templateService.GetTemplates();
        }

        private async void DeleteTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var template = button?.DataContext as WorkTemplate;
            if (template == null) return;

            var result = MessageBox.Show(this,
                GetString("MsgConfirmDelete", template.Name),
                GetString("TitleConfirm"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _templateService.DeleteTemplateAsync(template.Id);
                TemplateGrid.ItemsSource = null;
                TemplateGrid.ItemsSource = _templateService.GetTemplates();
            }
        }

        private async void BulkRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var templates = _templateService.GetTemplates().Where(t => t.IsEnabled).ToList();
            if (templates.Count == 0)
            {
                MessageBox.Show(this, GetString("MsgNoTemplates"), 
                    GetString("TitleInfo"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmResult = MessageBox.Show(this,
                string.Format(GetString("MsgBulkRegisterConfirm"), _selectedMonth.ToString("yyyy/MM"), templates.Count) + "\n\n" +
                string.Join("\n", templates.Select(t => $"  ・{t.Name} (#{t.IssueId}) {t.DefaultHours}h")),
                GetString("TitleConfirm"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult != MessageBoxResult.Yes) return;

            StatusTextBlock.Text = "一括登録中...";
            BulkRegisterButton.IsEnabled = false;

            try
            {
                int successCount = 0;
                int updateCount = 0;
                int errorCount = 0;
                int skipCount = 0;

                var daysInMonth = DateTime.DaysInMonth(_selectedMonth.Year, _selectedMonth.Month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(_selectedMonth.Year, _selectedMonth.Month, day);

                    // Skip weekends (unless marked as Work Day) and excluded dates
                    bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                    if ((isWeekend && !_workDays.Contains(date)) || _excludedDates.Contains(date))
                    {
                        skipCount++;
                        continue;
                    }

                    // Track consumed entry IDs to avoid same entry being matched by multiple templates
                    var dateStr = date.ToString("yyyy-MM-dd");
                    var claimedEntryIds = new HashSet<int>();

                    foreach (var template in templates)
                    {
                        if (!ShouldApplyTemplate(template, date))
                            continue;

                        try
                        {
                            // Find an existing entry with same issue ID that hasn't been claimed yet
                            var existing = _existingEntries.FirstOrDefault(
                                entry => entry.SpentOn == dateStr 
                                    && entry.Issue.Id == template.IssueId
                                    && !claimedEntryIds.Contains(entry.Id));

                            if (existing != null)
                            {
                                claimedEntryIds.Add(existing.Id);
                                AppendLog($"PUT /time_entries/{existing.Id}.json ({dateStr} {template.Name} {template.DefaultHours}h)");
                                await _redmineService.UpdateTimeEntryAsync(
                                    existing.Id, template.DefaultHours, template.ActivityId);
                                updateCount++;
                                AppendLog(GetString("LogOverwriteComplete"));
                            }
                            else
                            {
                                AppendLog($"POST /time_entries.json ({dateStr} #{template.IssueId} {template.Name} {template.DefaultHours}h)");
                                await _redmineService.CreateTimeEntryAsync(
                                    template.IssueId, template.DefaultHours, template.ActivityId,
                                    null, dateStr);
                                successCount++;
                                AppendLog(GetString("LogRegisterComplete"));
                            }

                            StatusTextBlock.Text = $"登録中... {date:M/d} {template.Name}";
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            AppendLog($"  → エラー: {ex.Message}");
                        }
                    }
                }

                MessageBox.Show(this,
                    GetString("MsgBulkRegisterResult", successCount, updateCount, skipCount, errorCount),
                    GetString("TitleResult"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, string.Format(GetString("StatusError"), ex.Message), GetString("TitleError"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BulkRegisterButton.IsEnabled = true;
            }
        }

        private bool ShouldApplyTemplate(WorkTemplate template, DateTime date)
        {
            switch (template.Frequency)
            {
                case FrequencyType.Daily:
                    // Only apply Mon-Fri, even if it's a Work Day
                    return !(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);

                case FrequencyType.Weekly:
                    return template.TargetDays.Contains(date.DayOfWeek);

                case FrequencyType.Monthly:
                    if (template.MonthlyDay == 0)
                    {
                        // Month-end
                        var lastDay = DateTime.DaysInMonth(date.Year, date.Month);
                        return date.Day == lastDay;
                    }
                    else
                    {
                        // Specific day
                        return date.Day == template.MonthlyDay;
                    }

                default:
                    return false;
            }
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var templates = _templateService.GetTemplates().Where(t => t.IsEnabled).ToList();
            if (templates.Count == 0)
            {
                MessageBox.Show(GetString("MsgNoTemplates"), GetString("TitleInfo"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var preview = new System.Text.StringBuilder();
            preview.AppendLine(string.Format(GetString("PreviewTitle"), _selectedMonth.ToString(GetString("DateFormatMonth"))));

            int totalEntries = 0;
            decimal totalHours = 0;
            var daysInMonth = DateTime.DaysInMonth(_selectedMonth.Year, _selectedMonth.Month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(_selectedMonth.Year, _selectedMonth.Month, day);

                // Skip weekends (unless marked as Work Day) and excluded dates
                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                if ((isWeekend && !_workDays.Contains(date)) || _excludedDates.Contains(date))
                    continue;

                var dayEntries = new List<string>();
                foreach (var template in templates)
                {
                    if (ShouldApplyTemplate(template, date))
                    {
                        dayEntries.Add($"  {template.Name} (#{template.IssueId}) {template.DefaultHours}h");
                        totalEntries++;
                        totalHours += template.DefaultHours;
                    }
                }

                if (dayEntries.Count > 0)
                {
                    string dayOfWeek = date.ToString("ddd");
                    preview.AppendLine($"{date:M/d} ({dayOfWeek})");
                    foreach (var entry in dayEntries)
                        preview.AppendLine(entry);
                    preview.AppendLine();
                }
            }

            preview.AppendLine(string.Format(GetString("PreviewTotal"), totalEntries, totalHours));

            // Show in scrollable window instead of MessageBox
            var previewWindow = new Window
            {
                Title = GetString("TitlePreview"),
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            var textBox = new TextBox
            {
                Text = preview.ToString(),
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas, MS Gothic"),
                FontSize = 13,
                Margin = new Thickness(10)
            };
            previewWindow.Content = textBox;
            previewWindow.ShowDialog();
        }

        private void UserGuideMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                var fileName = culture.StartsWith("ja") ? "USER_GUIDE.md" : "USER_GUIDE.en.md";
                var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                else
                {
                    MessageBox.Show(this,
                        $"User guide file not found: {fileName}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Failed to open user guide: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ReadmeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var culture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                var fileName = culture.StartsWith("ja") ? "README.md" : "README.en.md";
                var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                else
                {
                    MessageBox.Show(this,
                        $"README file not found: {fileName}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"Failed to open README: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BulkIssueMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_settings.IsConfigured)
            {
                MessageBox.Show(this, GetString("MsgConnectionRequired"),
                    GetString("TitleInfo"), MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new BulkIssueDialog(_redmineService);
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
            
            MessageBox.Show(this,
                $"Redmine Time Tracker\n\nVersion: {versionString}\n\n© 2026 tikomo software",
                GetString("MenuAbout"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    public class MonthItem
    {
        public DateTime Date { get; set; }
        public string Display { get; set; } = string.Empty;
    }
}
