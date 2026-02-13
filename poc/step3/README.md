# Redmine Support Tool (WinUI 3)

Redmine API への接続テストを行うための WinUI 3 アプリケーションです。

## 技術スタック
- **言語**: C#
- **フレームワーク**: WinUI 3 (Windows App SDK)
- **ビルド**: .NET 9.0

## 機能
- **プロジェクト一覧取得**: `GET /projects.json` を呼び出し、結果をデバッグコンソールに表示します。
- **認証**: カスタムヘッダー `X-Redmine-API-Key` を使用します。

## 設定
`Services/RedmineService.cs` 内の `RedmineSettings` クラスで以下の情報を管理しています。
- **BaseURL**: `http://localhost:8080`
- **APIKey**: `YOUR_REF_API_KEY`

## 実行方法
1. Visual Studio または VS Code でプロジェクトを開きます。
2. デバッグ実行 (F5) します。
3. 「Get Projects」ボタンをクリックすると、デバッグ出力に JSON と解析結果が表示されます。
