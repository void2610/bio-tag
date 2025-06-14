# Unity MCP Server

Unity Editor用のMCP (Model Context Protocol) サーバー実装です。LLM AgentがUnityのコンパイル機能を外部から操作できるようになります。

## 機能

### MCP ツール
- **unity_compile**: Unityスクリプトコンパイルの実行
- **unity_get_compile_status**: コンパイル状態の取得（エラー数、警告数、所要時間など）
- **unity_get_diagnostics**: 詳細なコンパイル診断情報の取得（エラーメッセージ、ファイル位置など）
- **unity_get_project_status**: 包括的プロジェクト状況レポート（NEW!）

### IDE統合ツール
- **mcp__ide__getDiagnostics**: 特定ファイルまたは全体プロジェクトの診断情報をリアルタイム取得

## セットアップ

### 1. Unity Editor側の準備

1. Unity Editor でプロジェクトを開く
2. `Tools > Unity MCP Server` を選択
3. `Start Server` ボタンをクリック
4. ポート 8080 でサーバーが起動することを確認

### 2A. Claude Desktop設定（GUI版使用時）

1. `claude_desktop_config.example.json` をコピーして設定ファイルを作成：
   ```bash
   cp unity-mcp-server/claude_desktop_config.example.json claude_desktop_config.json
   ```

2. `claude_desktop_config.json` を編集し、`<ABSOLUTE_PATH_TO_PROJECT>` を実際のプロジェクトパスに置換：
   ```json
   {
     "mcpServers": {
       "unity-compiler": {
         "command": "node",
         "args": ["/path/to/your/project/unity-mcp-server/unity-mcp-bridge.js"],
         "env": {
           "UNITY_MCP_PORT": "8080",
           "UNITY_MCP_HOST": "localhost"
         }
       }
     }
   }
   ```

3. Claude Desktop設定ディレクトリにコピー：
   ```bash
   cp claude_desktop_config.json ~/Library/Application\ Support/Claude/claude_desktop_config.json
   ```

### 2B. Claude Code（CLI版）設定

**重要**: Claude CodeのCLI版では、MCPサーバーが自動的に認識されるため、追加の設定は不要です。プロジェクトディレクトリでClaude Codeを起動するだけで利用可能になります。

```bash
# プロジェクトディレクトリでClaude Codeを起動
cd /Users/shuya/Documents/GitHub/bio-tag
claude-code
```

MCPサーバーが正常に動作している場合、以下のツールが自動的に利用可能になります：
- `mcp__ide__getDiagnostics`
- `unity_compile` 
- `unity_get_compile_status`
- `unity_get_diagnostics`
- `unity_get_project_status`

### 3. 動作確認

Unity Editor でMCPサーバーを起動し、Claude Code/Claude Desktop を起動後、MCPツールが使用可能になります。

## 使用方法

### Claude Code（CLI版）での使用

Claude Codeでは、ツールを直接呼び出すことはできませんが、自然言語でリクエストすることで自動的に適切なMCPツールが実行されます：

```
# 例：コンパイル状況を確認したい場合
「Unity プロジェクトのコンパイル状況を確認して」

# 例：特定ファイルのエラーを確認したい場合  
「GSRMock.cs ファイルのエラーを確認して」

# 例：プロジェクト全体の健全性を確認したい場合
「Unity プロジェクト全体の状況レポートを作成して」
```

### 利用可能なツールと用途

#### 1. リアルタイム診断
- **`mcp__ide__getDiagnostics`**: ファイル編集時の即座なエラー検出
  ```
  # 特定ファイルの診断
  mcp__ide__getDiagnostics(uri: "file:///path/to/file.cs")
  
  # 全体プロジェクトの診断
  mcp__ide__getDiagnostics()
  ```

#### 2. コンパイル制御
- **`unity_compile`**: 手動コンパイル実行
- **`unity_get_compile_status`**: 基本的なコンパイル状況
- **`unity_get_diagnostics`**: 詳細なエラー・警告情報

#### 3. 包括的プロジェクト分析（NEW!）
- **`unity_get_project_status`**: 全体的なプロジェクト健全性レポート
  
  **出力例**:
  ```
  Unity Project Status Report
  ==========================================
  Overall Health: ❌ ERRORS
  Compilation Status: CompilationFailed
  Duration: 1234ms
  Last Updated: 2024-12-14T10:30:00Z

  Statistics:
  • Errors: 2
  • Warnings: 1
  • Total Issues: 3

  🚨 ERRORS (2):
    • ; が必要です (GSRMock.cs:11)
    • シンボル 'undefinedVariable' を解決できません (GSRMock.cs:13)

  ⚠️ WARNINGS (1):
    • フィールドをデフォルト値で初期化するのは冗長です (GSRMock.cs:6)
  ```

## 使用例とワークフロー

### 典型的な開発ワークフロー

1. **リアルタイム開発**：
   - ファイル編集時に`mcp__ide__getDiagnostics`が自動実行
   - 即座にエラー・警告が表示される

2. **大きな変更後の確認**：
   - 「プロジェクト全体の状況を確認して」→ `unity_get_project_status`実行
   - 包括的なレポートで全体的な健全性を把握

3. **手動コンパイル**：
   - 「Unityでコンパイルを実行して」→ `unity_compile`実行
   - コンパイル完了後、自動的に状況確認

## トラブルシューティング

### Unity MCP Server not running エラー
1. Unity Editor で `Tools > Unity MCP Server` を開く
2. `Start Server` をクリック
3. ポート 8080 が他のプロセスで使用されていないか確認
4. Unity Editor のコンソールにサーバー起動メッセージが表示されることを確認

### Claude Code でMCPツールが利用できない
1. Unity Editor でMCPサーバーが起動していることを確認
2. プロジェクトディレクトリでClaude Codeを起動していることを確認
3. `mcp` コマンドで利用可能なサーバーを確認

### 接続エラー
- Unity Editor が起動していることを確認
- ファイアウォール設定でlocalhost通信が許可されていることを確認（macOS/Windows）
- Unity Editor のバージョンが6000.0.42f1以降であることを確認

### MCPサーバーがUnity Editor内で見つからない場合
1. `Assets/Editor/` ディレクトリが存在することを確認
2. 以下のファイルが存在することを確認：
   - `Assets/Editor/MCPServer.cs`
   - `Assets/Editor/UnityMCPWindow.cs`
   - `Assets/Editor/CompilationStatusTracker.cs`
3. Unity Editor でスクリプトが正常にコンパイルされていることを確認

## @unity-tools/ との比較

Unity MCPサーバーは従来の `@unity-tools/` ツールセットを置き換え、より高機能な開発体験を提供します：

### MCPサーバーの利点
- ✅ **リアルタイム診断**: ファイル編集と同時にエラー検出
- ✅ **統合性**: Claude Code内で直接診断情報取得
- ✅ **詳細情報**: 行番号、列番号、重要度レベル付きエラー報告
- ✅ **包括的レポート**: プロジェクト全体の健全性を一目で把握
- ✅ **自動実行**: 自然言語リクエストで適切なツールが自動選択

### 使い分けの推奨
- **日常的な開発**: Unity MCPサーバー（リアルタイム診断）
- **CI/CD**: `@unity-tools/`（スタンドアロン実行）
- **緊急時**: `@unity-tools/`（Unity Editor外からの診断）