using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using redmineSupTool.Models;

namespace redmineSupTool.Services
{
    /// <summary>
    /// テンプレートの保存・読込を管理するサービス
    /// </summary>
    public class TemplateService
    {
        private readonly string _dataFilePath;
        private List<WorkTemplate> _templates = new();

        public TemplateService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RedmineSupTool");
            
            Directory.CreateDirectory(appDataPath);
            _dataFilePath = Path.Combine(appDataPath, "templates.json");
        }

        /// <summary>
        /// テンプレート一覧を取得
        /// </summary>
        public List<WorkTemplate> GetTemplates()
        {
            return _templates;
        }

        /// <summary>
        /// テンプレートをファイルから読み込み
        /// </summary>
        public async Task LoadAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                _templates = GetDefaultTemplates();
                await SaveAsync();
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_dataFilePath);
                var data = JsonSerializer.Deserialize<TemplateData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                _templates = data?.Templates ?? new List<WorkTemplate>();
            }
            catch (Exception)
            {
                _templates = GetDefaultTemplates();
            }
        }

        /// <summary>
        /// テンプレートをファイルに保存
        /// </summary>
        public async Task SaveAsync()
        {
            var data = new TemplateData { Templates = _templates };
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            await File.WriteAllTextAsync(_dataFilePath, json);
        }

        /// <summary>
        /// テンプレートを追加
        /// </summary>
        public async Task AddTemplateAsync(WorkTemplate template)
        {
            _templates.Add(template);
            await SaveAsync();
        }

        /// <summary>
        /// テンプレートを更新
        /// </summary>
        public async Task UpdateTemplateAsync(WorkTemplate template)
        {
            var existing = _templates.FirstOrDefault(t => t.Id == template.Id);
            if (existing != null)
            {
                var index = _templates.IndexOf(existing);
                _templates[index] = template;
                await SaveAsync();
            }
        }

        /// <summary>
        /// テンプレートを削除
        /// </summary>
        public async Task DeleteTemplateAsync(Guid id)
        {
            _templates.RemoveAll(t => t.Id == id);
            await SaveAsync();
        }

        /// <summary>
        /// デフォルトテンプレート（初回起動時用）
        /// </summary>
        private List<WorkTemplate> GetDefaultTemplates()
        {
            return new List<WorkTemplate>();
        }

        private class TemplateData
        {
            public List<WorkTemplate> Templates { get; set; } = new();
        }
    }
}
