# Bio-Tag UI Toolkit Style System Guide

## 概要

Bio-TagのUIシステムは、**単一のグローバルスタイルシート**を使用して、プロジェクト全体で一貫したデザインを提供します。すべてのUIスタイルが1つのファイルに集約され、保守性と一貫性を最大化しています。

## ファイル構成

```
Assets/UI Toolkit/
├── GlobalUIStyles.uss          # 唯一のスタイルシート（すべてのスタイル）
├── UI-System-Guide.md          # このガイドドキュメント
├── UIDocument/
│   ├── TitleDocument.uxml      # タイトル画面のUIレイアウト
│   └── GameUIDocument.uxml     # ゲーム画面のUIレイアウト
├── MyPanelSettings.asset       # UI Toolkit設定ファイル
├── MyPanelSettings2.asset      # UI Toolkit設定ファイル
└── UnityThemes/
    └── UnityDefaultRuntimeTheme.tss
```

**🎯 完全統合システム**: 
- `GlobalUIStyles.uss`が**唯一のスタイルシート**
- すべてのUI要素（タイトル・ゲーム・共通コンポーネント）のスタイルを含む
- 新しいスタイルは**必ず**`GlobalUIStyles.uss`に追加する
- 固有のスタイルシートは**完全に廃止**

## スタイルシステムの特徴

### 1. カラーパレット（統一）
- **プライマリ背景**: `#1a1a2e`
- **セカンダリ背景**: `#16213e`
- **アクセントカラー**: `#e94560` (赤)
- **紫アクセント**: `#533483`
- **テキストカラー**: `#ffffff` (プライマリ), `#f1f1f1` (セカンダリ)

### 2. 再利用可能なコンポーネントクラス

#### レイアウト
- `.full-screen-container` - フルスクリーンコンテナ
- `.centered-container` - 中央揃えコンテナ
- `.overlay-container` - オーバーレイコンテナ

#### パネル
- `.panel-primary` - メインパネル（角丸、ボーダー付き）
- `.panel-secondary` - サブパネル（小さなボーダー）
- `.panel-overlay` - オーバーレイパネル（半透明背景）

#### テキスト
- `.text-title` - タイトルテキスト（42px、赤色）
- `.text-heading` - 見出しテキスト（24px、白色）
- `.text-subheading` - 小見出しテキスト（18px、赤色）
- `.text-body` - 本文テキスト（16px、白色）
- `.text-caption` - キャプションテキスト（14px、グレー）

#### ボタン
- `.btn-primary` - プライマリボタン（紫背景、ホバーで赤）
- `.btn-secondary` - セカンダリボタン（ボーダー付き）

#### 入力フィールド
- `.input-field` - 統一された入力フィールドスタイル

#### ユーティリティ
- `.flex-row` / `.flex-column` - Flexbox方向
- `.justify-center` / `.justify-between` / `.justify-around` - 横軸揃え
- `.align-center` / `.align-start` / `.align-end` - 縦軸揃え
- `.hidden` / `.visible` - 表示・非表示
- `.pos-absolute` / `.pos-relative` - ポジション設定
- `.top-left` / `.top-right` / `.top-center` - 配置ユーティリティ

### 3. 状態管理クラス
メッセージタイプ用：
- `.info` - 情報メッセージ（紫）
- `.warning` - 警告メッセージ（オレンジ）
- `.success` - 成功メッセージ（緑）
- `.error` - エラーメッセージ（赤）

## 使用方法

### 1. 新しいUIコンポーネントの作成

```xml
<!-- UXMLファイルにはグローバルスタイルのみ含める -->
<Style src="project://database/Assets/UI%20Toolkit/GlobalUIStyles.uss" />

<!-- グローバルクラスを組み合わせて使用 -->
<engine:VisualElement class="panel-primary centered-container">
    <engine:Label text="Title" class="text-title" />
    <engine:Button text="Click Me" class="btn-primary" />
</engine:VisualElement>
```

### 2. カスタムスタイルの追加

新しいスタイルが必要な場合は、`GlobalUIStyles.uss`に直接追加：

```css
/* GlobalUIStyles.uss に追加 */

/* 新しいコンポーネントのスタイル */
.your-custom-component {
    background-color: #16213e; /* secondary-bg */
    border-color: #e94560; /* accent-color */
    border-radius: 10px;
    padding: 15px;
}

/* 既存クラスのオーバーライド */
.your-container .btn-primary {
    width: 250px; /* ボタンを大きくする */
}
```

### 3. C#コードでの動的スタイル変更

```csharp
// クラスの追加・削除
element.AddToClassList("success");
element.RemoveFromClassList("error");

// 表示・非表示の切り替え
element.AddToClassList("hidden");
element.RemoveFromClassList("hidden");

// 直接スタイル変更（必要最小限に）
element.style.display = DisplayStyle.None;
```

## ベストプラクティス

### 1. 一貫性の保持
- 新しいカラーを追加する前に、既存のカラーパレットで対応できないか確認
- グローバルクラスの組み合わせで目的のデザインを実現

### 2. 保守性の向上
- **すべてのスタイルをGlobalUIStyles.ussに集約**
- カラーコードの直接記述を避け、コメントで色の用途を記載
- セクションコメントで整理し、目的の場所を見つけやすくする

### 3. パフォーマンス
- 単一ファイルによりCSSロード時間を最小化
- 不要なスタイルオーバーライドを避ける
- グローバルクラスを活用してCSS量を削減

## トラブルシューティング

### スタイルが適用されない場合
1. UXMLファイルにGlobalUIStyles.ussが正しく読み込まれているか確認
2. クラス名のスペルミスがないか確認
3. 他のスタイルシートが誤って含まれていないか確認

### カスタマイズが効かない場合
1. CSS詳細度の問題 - より具体的なセレクタを使用
2. GlobalUIStyles.uss内での競合 - セクション順序を確認

## システムの利点

✅ **完全統合**: すべてのスタイルが1ファイルに集約  
✅ **高速ロード**: 単一ファイルでCSSロード時間最小化  
✅ **簡単保守**: スタイル変更が1箇所で完結  
✅ **一貫性確保**: プロジェクト全体で統一されたデザイン  
✅ **開発効率**: 新しいUI作成時の参照ファイルが明確

## 今後の拡張

新しいコンポーネントやテーマを追加する際は、GlobalUIStyles.ussに適切なセクションを追加し、一貫性を保ちながらシステムを発展させてください。