using System;
using System.Linq;
using System.Windows;
using redmineSupTool.Services;

namespace redmineSupTool
{
    public partial class MainWindow : Window
    {
        private readonly RedmineService _redmineService;

        public MainWindow()
        {
            InitializeComponent();
            
            var settings = new RedmineSettings();
            _redmineService = new RedmineService(settings);
            
            // 起動時に作業分類を取得
            LoadActivitiesAsync();
        }

        private async void LoadActivitiesAsync()
        {
            try
            {
                var activities = await _redmineService.GetActivitiesAsync();
                ActivityComboBox.ItemsSource = activities;
                
                // デフォルトの作業分類を選択
                var defaultActivity = activities.FirstOrDefault(a => a.IsDefault);
                if (defaultActivity != null)
                {
                    ActivityComboBox.SelectedItem = defaultActivity;
                }
                else if (activities.Any())
                {
                    ActivityComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"作業分類の取得に失敗: {ex.Message}";
            }
        }

        private void RefreshActivitiesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadActivitiesAsync();
        }

        private async void GetProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Connecting...";
            GetProjectsButton.IsEnabled = false;
            OutputTextBox.Clear();

            try
            {
                var output = await _redmineService.GetProjectsAsync();
                OutputTextBox.Text = output;
                StatusTextBlock.Text = "Success!";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
                OutputTextBox.Text = $"Error: {ex.Message}\n{ex.StackTrace}";
            }
            finally
            {
                GetProjectsButton.IsEnabled = true;
            }
        }

        private async void RegisterTimeButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Registering time entry...";
            RegisterTimeButton.IsEnabled = false;
            OutputTextBox.Clear();

            try
            {
                // 入力検証
                if (!int.TryParse(TicketIdTextBox.Text, out int issueId))
                {
                    OutputTextBox.Text = "✗ エラー: チケットIDは数値で入力してください。";
                    StatusTextBlock.Text = "Error: Invalid input";
                    return;
                }

                if (!decimal.TryParse(HoursTextBox.Text, out decimal hours) || hours <= 0)
                {
                    OutputTextBox.Text = "✗ エラー: 作業時間は正の数値で入力してください。";
                    StatusTextBlock.Text = "Error: Invalid input";
                    return;
                }

                if (ActivityComboBox.SelectedValue == null)
                {
                    OutputTextBox.Text = "✗ エラー: 作業分類を選択してください。";
                    StatusTextBlock.Text = "Error: No activity selected";
                    return;
                }
                int activityId = (int)ActivityComboBox.SelectedValue;

                string? comments = string.IsNullOrWhiteSpace(CommentsTextBox.Text) ? null : CommentsTextBox.Text;
                string? spentOn = null;
                
                if (SpentOnDatePicker.SelectedDate.HasValue)
                {
                    spentOn = SpentOnDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                }

                var output = await _redmineService.CreateTimeEntryAsync(issueId, hours, activityId, comments, spentOn);
                OutputTextBox.Text = output;
                StatusTextBlock.Text = "Success!";

                // 成功時は入力欄をクリア（作業分類は維持）
                TicketIdTextBox.Clear();
                HoursTextBox.Clear();
                CommentsTextBox.Clear();
                SpentOnDatePicker.SelectedDate = null;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error: {ex.Message}";
                OutputTextBox.Text += $"\n\n✗ Exception: {ex.Message}\n{ex.StackTrace}";
            }
            finally
            {
                RegisterTimeButton.IsEnabled = true;
            }
        }
    }
}
