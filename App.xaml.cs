using System.Linq;
using System.Windows;

namespace redmineSupTool
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetLanguage(e.Args);
        }

        private void SetLanguage(string[] args)
        {
            // Support both -en and --en
            bool forceEnglish = args.Contains("-en") || args.Contains("--en");

            string culture = forceEnglish ? "en-US" : System.Globalization.CultureInfo.CurrentUICulture.Name;
            string dictPath = culture.StartsWith("ja") 
                ? "Resources/Strings.ja-JP.xaml" 
                : "Resources/Strings.en-US.xaml";

            ResourceDictionary dict = new ResourceDictionary { Source = new Uri(dictPath, UriKind.Relative) };
            
            // Clear existing to avoid conflicts if previously set
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(dict);
        }
    }
}
