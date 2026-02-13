using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using redmineSupTool.Services;

namespace redmineSupTool.Views
{
    public partial class BulkIssueDialog : Window
    {
        private readonly RedmineService _redmineService;

        public BulkIssueDialog(RedmineService redmineService)
        {
            InitializeComponent();
            _redmineService = redmineService;
        }

        private string GetString(string key, params object[] args)
        {
            var res = Application.Current.TryFindResource(key) as string ?? key;
            return args.Length > 0 ? string.Format(res, args) : res;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ParentIssueIdTextBox.Text, out int rootParentId))
            {
                MessageBox.Show(GetString("MsgEnterTicketId"), 
                    GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var lines = IssueTitlesTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var issueItems = new List<HierarchicalIssue>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Count leading spaces/tabs to determine depth
                int leadingOffset = 0;
                foreach (char c in line)
                {
                    if (c == ' ') leadingOffset++;
                    else if (c == '\t') leadingOffset += 4; // Treat tab as 4 spaces for depth
                    else break;
                }
                
                string title = line.Trim();
                if (!string.IsNullOrEmpty(title))
                {
                    issueItems.Add(new HierarchicalIssue { Title = title, Depth = leadingOffset });
                }
            }

            if (issueItems.Count == 0)
            {
                MessageBox.Show(GetString("MsgEnterName"), 
                    GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Normalize depths to 0, 1, 2...
            var uniqueDepths = issueItems.Select(x => x.Depth).Distinct().OrderBy(x => x).ToList();
            foreach (var item in issueItems)
            {
                item.NormalizedDepth = uniqueDepths.IndexOf(item.Depth);
            }

            // UI State
            CreateButton.IsEnabled = false;
            ParentIssueIdTextBox.IsEnabled = false;
            DescriptionTextBox.IsEnabled = false;
            IssueTitlesTextBox.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = issueItems.Count;
            ProgressBar.Value = 0;

            try
            {
                StatusTextBlock.Text = GetString("StatusLoading");
                var rootIssue = await _redmineService.GetIssueAsync(rootParentId);
                if (rootIssue == null)
                {
                    MessageBox.Show(GetString("MsgParentIssueNotFound"), 
                        GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int successCount = 0;
                // Track the last created issue ID for each depth
                // Depth 0's parent is rootParentId
                var parentIdsByDepth = new Dictionary<int, int>();
                parentIdsByDepth[-1] = rootParentId;

                for (int i = 0; i < issueItems.Count; i++)
                {
                    var item = issueItems[i];
                    StatusTextBlock.Text = GetString("MsgCreatingIssues", i + 1, issueItems.Count);
                    
                    // Determine parent ID: look for the ID of the level above
                    int currentParentId = rootParentId;
                    if (item.NormalizedDepth > 0)
                    {
                        // Search for the actual parent: the closest preceding item with Depth < item.Depth
                        // But with normalized depths, it's just parentIdsByDepth[item.NormalizedDepth - 1]
                        if (parentIdsByDepth.TryGetValue(item.NormalizedDepth - 1, out int pId))
                        {
                            currentParentId = pId;
                        }
                    }

                    var issueData = new IssueData
                    {
                        ProjectId = rootIssue.Project?.Id,
                        ParentIssueId = currentParentId,
                        Subject = item.Title,
                        Description = DescriptionTextBox.Text,
                        TrackerId = rootIssue.Tracker?.Id
                    };

                    var createdId = await _redmineService.CreateIssueAsync(issueData);
                    if (createdId.HasValue)
                    {
                        successCount++;
                        // Store this ID as the potential parent for the next (deeper) level
                        parentIdsByDepth[item.NormalizedDepth] = createdId.Value;
                    }
                    
                    ProgressBar.Value = i + 1;
                }

                if (successCount == issueItems.Count)
                {
                    MessageBox.Show(GetString("MsgCreateSuccess"), 
                        GetString("TitleSuccess"), MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show(GetString("MsgCreateError"), 
                        GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetString("MsgCreateError")}\n{ex.Message}", 
                    GetString("TitleError"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CreateButton.IsEnabled = true;
                ParentIssueIdTextBox.IsEnabled = true;
                DescriptionTextBox.IsEnabled = true;
                IssueTitlesTextBox.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
                StatusTextBlock.Text = "";
            }
        }

        private class HierarchicalIssue
        {
            public string Title { get; set; } = "";
            public int Depth { get; set; }
            public int NormalizedDepth { get; set; }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
