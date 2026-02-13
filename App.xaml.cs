using System.Windows;

namespace redmineSupTool
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetLanguage();
        }

        private void SetLanguage()
        {
            string culture = System.Globalization.CultureInfo.CurrentUICulture.Name;
            string dictPath = culture.StartsWith("ja") 
                ? "Resources/Strings.ja-JP.xaml" 
                : "Resources/Strings.en-US.xaml";

            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri(dictPath, UriKind.Relative);
            
            // Remove existing lang dict if any, or just add
            // For simplicity in this app structure, we just add it. 
            // If dynamic switching at runtime was needed, we'd clear old one first.
            this.Resources.MergedDictionaries.Add(dict);
        }
    }
}
