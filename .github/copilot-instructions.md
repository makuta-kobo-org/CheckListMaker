# Copilot Instructions (Project Root)

本プロジェクトで GitHub Copilot / Copilot Chat が従う基本方針を定義します。MAUI（現在 .NET 8、近々 .NET 10 へ移行予定）前提。Xamarin.Forms に関する提案は行わないでください（MAUI 等価がある場合のみ）。

## 1. 目的 / スコープ
- チェックリスト作成アプリの開発・保守（個人開発）
- 対象プラットフォーム: Android / iOS / Windows / macOS (MAUI)
- リポジトリ: GitHub

## 2. 技術スタック
- .NET: .NET 8（C# 12）→ 近々 .NET 10（C# 13 予定）にアップグレード
- UI: .NET MAUI (XAML + MVVM)
- DI/構成: `MauiProgram` でのサービス登録（必要に応じて拡張）
- 設定: `appsettings.*.json`（開発/本番/テスト）
- リソース/多言語化: `Resources/*.resx` を使用（文字列直書き禁止）
- テスト: xUnit（UI 以外の単体テストを重視）

## 3. 設計原則
- アーキテクチャ: MVVM
  - `View` は表示・軽微なイベント配線のみ。ロジックは `ViewModel` へ。
  - 共通ロジックは `Services`（インターフェース）へ抽出し DI で提供。
- スレッド/非同期:
  - UI ブロック禁止。`async/await` を徹底、UI 更新は `MainThread`/`Dispatcher` を使用。
- 例外/エラー:
  - ドメイン/アプリ例外は `ExceptionBase` を基底に整理。
  - 失敗時はユーザー通知（`AlertService` 等）とログの両立。
- 設定/シークレット:
  - API キー等は構成/Secret 管理（環境変数や Secret）に分離。コード・XAML に直書き禁止。
- 国際化/アクセシビリティ:
  - テキストは resx。アクセシビリティ（コントラスト/読み上げ）に配慮。

## 4. コーディング規約
- 既存の `.editorconfig` に従う（命名/スタイル/解析警告）。
- コメント/ドキュメンテーションは日本語。公開 API は XML ドキュメント化。
- 名前付け:
  - View: `*.xaml` / `*.xaml.cs`
  - ViewModel: `*ViewModel`
  - ページ遷移は `AppShell` とルート名を一元管理。

## 5. MAUI 実装ガイド
- XAML バインディングを優先（コードビハインドは最小限）。
- プラットフォーム依存コードは `Platforms/*` に分離し、必要に応じて `#if` ガード。
- ホットリロードを活用。長時間同期処理は禁止。
- 画像/色/スタイルは `Resources/*` に集中管理。

## 6. 依存ライブラリ（方針）
- 必要最小限を原則。追加時は目的/代替案/サイズを比較。
- 候補例（必要時のみ）:
  - CommunityToolkit.Mvvm / Maui.Toolkit
  - テスト: FluentAssertions 等
  - UI: 必要時に限定（例: マテリアル系コントロール）

## 7. テスト方針
- 単体テスト: モデル/サービス/ロジック中心。I/O は抽象化し注入可能に。
- 命名: `MethodName_State_ExpectedBehavior` 形式推奨。
- 非同期テストは `async Task` で記述し、タイムアウトを適切に設定。

## 8. Git/CI
- 小さなコミットで意味単位に分割。コミットメッセージは英語/日本語いずれも可だが一貫性を保つ。
- ブランチ: 機能/修正ごとに topic ブランチ（例: `feature/*`, `fix/*`）。
- CI: 将来的な導入を想定（例: ビルド/テスト/ストア署名）。

## 9. Copilot Chat へのリクエスト指針
- 出力言語: 日本語で説明、コードは C# を既存スタイルに合わせる。
- 変更提案は差分最小・安全第一。既存構造/命名を尊重。
- 生成時に前提を明示:
  - 対象プロジェクト/ファイル/クラス
  - 入出力/状態遷移/UI 仕様
  - 例外/エッジケース
- 生成コードにはテスト観点（失敗シナリオ/境界値）も示す。

## 10. 禁則事項
- Xamarin.Forms 向け API/サンプルの提案。
- 同期 I/O や UI スレッドブロック。
- 機微情報の貼付/ハードコード。

## 11. MCP/ツール連携（任意）
- MCP が有効な場合:
  - ビルド/テスト実行、ログ収集、設定確認をチャットから要請可能。
  - 実行時はコマンド/ログ要約と影響範囲を併記。

---
補足: 本ファイルはプロジェクト標準の上位方針です。詳細な指針は必要に応じて `.github/instructions/*.instructions.md` に分割し、Copilot のカスタム指示で読み込ませます。

## 12. .NET アップグレード（8 → 10）方針とコミュニケーション指針
- 目的: LTS 以降のランタイム/SDKに合わせ、MAUI/依存ライブラリの互換性を維持しつつ性能・機能を向上。

### ドキュメント言語規約
- **すべての生成ドキュメント（plan.md, tasks.md, README 等）は日本語で記述する**
  - 見出し、本文、テーブルヘッダー、リスト項目：すべて日本語
  - コード例、コマンド、パッケージ名、API 名：英語のまま
  - 技術用語：必要に応じて英語を併記（例: ターゲットフレームワーク (Target Framework)）
- **例外**: `assessment.md` はツール自動生成のため英語（制御対象外）

### 対応の基本手順（Copilot へ依頼する際は、以下の情報を必ず併記）
- 対象プロジェクトと `TargetFrameworks` の現状（例: `net8.0-android;net8.0-ios;net8.0-windows10.0.*;net8.0-maccatalyst`）
- 予定する `TargetFrameworks`（例: `net10.0-android;net10.0-ios;net10.0-windows10.0.*;net10.0-maccatalyst`）
- 使用 SDK/ワークロードのバージョン（MAUI/Android/iOS/Windows）
- 主要ライブラリのバージョン制約（`CommunityToolkit.Mvvm` など）
- 既知のブレイキングチェンジや API 変更が疑われる箇所（例: `MainThread`, `Dispatcher`, `Handlers` 周辺）

### 生成/提案のルール
- MAUI の `Platforms/*` と `MauiProgram` 設定の差分を最小化し、互換性維持を優先。
- SDK/Workload の更新手順（インストーラー/`dotnet workload install`）は具体的コマンド・確認方法を提示。
- 失敗時のロールバック案（`global.json` で SDK ピン止め、`TargetFrameworks` を段階的に切替）も併記。

### 検証とログ
- ビルド/テスト/デプロイ対象ごとにログ取得方法を明示（VS の出力ウィンドウ、`binlog`、テスト結果）。
- Android/iOS はエミュレータ/実機の OS バージョンも記載して依頼する。
