using System;
using System.Net.Http;
using System.Windows;
using redmineSupTool.Services;

namespace redmineSupTool
{
    public partial class SettingsDialog : Window
    {
        public RedmineSettings Settings { get; private set; }

        public SettingsDialog(RedmineSettings currentSettings)
        {
            InitializeComponent();
            Settings = currentSettings;
            UrlTextBox.Text = currentSettings.BaseUrl;
            ApiKeyTextBox.Text = currentSettings.ApiKey;
        }

        private string GetString(string key, params object[] args)
        {
            var res = Application.Current.TryFindResource(key) as string ?? key;
            return args.Length > 0 ? string.Format(res, args) : res;
        }

        private async void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UrlTextBox.Text))
            {
                MessageBox.Show(this, GetString("MsgEnterUrl"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ApiKeyTextBox.Text))
            {
                MessageBox.Show(this, GetString("MsgEnterApiKey"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tempSettings = new RedmineSettings
            {
                BaseUrl = UrlTextBox.Text.Trim(),
                ApiKey = ApiKeyTextBox.Text.Trim()
            };

            var service = new Services.RedmineService(tempSettings);
            try
            {
                var user = await service.GetCurrentUserAsync();
                MessageBox.Show(this, GetString("MsgConnectionSuccess"), GetString("TitleSuccess"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(this, GetString("MsgConnectionFailed", ex.Message), 
                    GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UrlTextBox.Text))
            {
                MessageBox.Show(this, GetString("MsgEnterUrl"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ApiKeyTextBox.Text))
            {
                MessageBox.Show(this, GetString("MsgEnterApiKey"), GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Settings.BaseUrl = UrlTextBox.Text.Trim();
            Settings.ApiKey = ApiKeyTextBox.Text.Trim();
            Settings.Save();

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
