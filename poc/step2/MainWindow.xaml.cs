using System;
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

                if (!int.TryParse(ActivityIdTextBox.Text, out int activityId))
                {
                    OutputTextBox.Text = "✗ エラー: 作業分類IDは数値で入力してください。";
                    StatusTextBlock.Text = "Error: Invalid input";
                    return;
                }

                string? comments = string.IsNullOrWhiteSpace(CommentsTextBox.Text) ? null : CommentsTextBox.Text;
                string? spentOn = null;
                
                if (SpentOnDatePicker.SelectedDate.HasValue)
                {
                    spentOn = SpentOnDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                }

                var output = await _redmineService.CreateTimeEntryAsync(issueId, hours, activityId, comments, spentOn);
                OutputTextBox.Text = output;
                StatusTextBlock.Text = "Success!";

                // 成功時は入力欄をクリア
                TicketIdTextBox.Clear();
                HoursTextBox.Clear();
                ActivityIdTextBox.Clear();
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
