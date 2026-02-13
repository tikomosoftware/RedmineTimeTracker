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
    }
}
