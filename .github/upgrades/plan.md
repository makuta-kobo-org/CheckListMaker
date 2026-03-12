# .NET 8 から .NET 10 へのアップグレード + Azure AI Vision 移行計画

## 目次

- [概要](#概要)
  - [選択された戦略](#選択された戦略)
  - [複雑度評価](#複雑度評価)
  - [重要な問題](#重要な問題)
  - [推奨アプローチ](#推奨アプローチ)
  - [反復戦略](#反復戦略)
- [移行戦略](#移行戦略)
  - [アプローチの選択と正当性](#アプローチの選択と正当性)
  - [依存関係ベースの順序付け](#依存関係ベースの順序付け)
  - [並行実行と逐次実行](#並行実行と逐次実行)
  - [フェーズ定義](#フェーズ定義)
- [詳細な依存関係分析](#詳細な依存関係分析)
  - [依存関係グラフの概要](#依存関係グラフの概要)
  - [移行フェーズ別のプロジェクトグループ](#移行フェーズ別のプロジェクトグループ)
  - [クリティカルパスの識別](#クリティカルパスの識別)
  - [循環依存関係の詳細](#循環依存関係の詳細)
- [プロジェクト別移行計画](#プロジェクト別移行計画)
  - [CheckListMaker.csproj](#checklistmakercsproj)
  - [CheckListMakerTest.Tests.csproj](#checklistmakertesttestscsproj)
- [Azure AI Vision 移行計画](#azure-ai-vision-移行計画)
  - [現在の実装分析](#現在の実装分析)
  - [移行パス](#移行パス)
  - [実装手順](#実装手順)
  - [テスト戦略](#テスト戦略)
- [リスク管理](#リスク管理)
  - [高リスク変更](#高リスク変更)
  - [セキュリティ脆弱性](#セキュリティ脆弱性)
  - [コンティンジェンシープラン](#コンティンジェンシープラン)
- [テストと検証戦略](#テストと検証戦略)
  - [フェーズ別テスト要件](#フェーズ別テスト要件)
  - [スモークテスト](#スモークテスト)
  - [包括的検証](#包括的検証)
- [複雑度と工数評価](#複雑度と工数評価)
  - [プロジェクト別複雑度](#プロジェクト別複雑度)
  - [フェーズ複雑度評価](#フェーズ複雑度評価)
  - [リソース要件](#リソース要件)
- [ソース管理戦略](#ソース管理戦略)
  - [ブランチ戦略](#ブランチ戦略)
  - [コミット戦略](#コミット戦略)
  - [レビューとマージプロセス](#レビューとマージプロセス)
- [成功基準](#成功基準)
  - [技術的基準](#技術的基準)
  - [品質基準](#品質基準)
  - [プロセス基準](#プロセス基準)

---

## 概要

このドキュメントは、CheckListMaker ソリューションを .NET 8 から .NET 10 (LTS) へアップグレードし、併せて Azure AI Vision SDK を最新版に移行するための包括的な計画を提供します。

### 選択された戦略

**All-At-Once (一括) 戦略** - すべてのプロジェクトを単一の操作で同時にアップグレードします。

**根拠**:
- 2 プロジェクトのみ（小規模ソリューション）
- すべてのプロジェクトが現在 .NET 8 以上
- 明確な依存関係構造（深さ1、循環なし）
- すべてのパッケージにターゲットフレームワーク対応バージョンが存在
- セキュリティ脆弱性なし
- Azure AI Vision 移行は中程度のリスク

この戦略により、最短時間での完了、クリーンな依存関係解決、すべてのプロジェクトが同時に恩恵を受けることが可能になります。

### 複雑度評価

**発見されたメトリクス**:
- 総プロジェクト数: 2
- 依存関係の深さ: 1（最大レベル）
- 循環依存関係: なし
- 総コード行数: 3,886 行
- 影響を受けるコード行数: 955+ 行（24.6%）
- NuGet パッケージ総数: 23 個
- アップグレード必要パッケージ: 9 個
- API 問題総数: 967 件
  - バイナリ非互換: 3 件
  - ソース非互換: 952 件
  - 動作変更: 0 件

**複雑度分類**: **シンプル**
- ? プロジェクト数 ? 5
- ? 依存関係の深さ ? 2
- ? 高リスクプロジェクトなし
- ? セキュリティ脆弱性なし
- ? 明確な依存関係構造

### 重要な問題

#### ?? Azure AI Vision 廃止対応（必須）

**Microsoft からの通知**: 2028年9月25日に Azure AI Vision Image Analysis が廃止されます。

**現在の使用状況**:
- パッケージ: `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` v7.0.1
- 使用箇所: `ComputerVisionService.cs`（OCR 機能）
- 機能: 画像からテキスト抽出してチェックリスト生成

**移行先**: `Azure.AI.Vision.ImageAnalysis` (新SDK)

この移行を .NET 10 アップグレードと統合して実施します。

#### ?? 主要な API 非互換性

**最も影響の大きい API**:
1. `Microsoft.Maui.Controls.Animation` - 180 件（18.8%）
2. `TextColor` プロパティ - 208 件（21.5%）
3. 各種 MAUI コントロール API - 552 件（57.1%）

これらは主にソース非互換で、再コンパイルと潜在的なコード修正が必要です。

### 推奨アプローチ

**All-At-Once (一括アップグレード)** - すべての変更を単一の統合操作で実行

**実行順序**:
1. **フェーズ 0: 前提条件**（該当する場合）
   - .NET 10 SDK インストール確認
   - global.json 更新（存在する場合）

2. **フェーズ 1: 一括アップグレード**（単一のバッチ操作）
   - すべてのプロジェクトファイルを同時更新
   - すべてのパッケージ参照を同時更新
   - 依存関係を復元してソリューション全体をビルド
   - コンパイルエラーを修正
   - ビルド成功を検証

3. **フェーズ 2: Azure AI Vision 移行**
   - 旧 SDK を削除、新 SDK を追加
   - `ComputerVisionService.cs` をリファクタリング
   - 認証方式を更新
   - ユニットテストを更新
   - 機能テストを実行

4. **フェーズ 3: テスト検証**
   - すべてのテストプロジェクトを実行
   - テスト失敗に対処

**成果物**: エラー 0 でビルドされ、すべてのテストが成功するソリューション

### 反復戦略

**シンプルソリューション向け高速バッチアプローチ（2-3 詳細反復）**

このドキュメントは以下の反復で構築されました：
- **Phase 1**: 発見と分類（完了）
- **Phase 2**: 基盤セクション（進行中）
- **Phase 3**: 詳細生成（次）

すべてのプロジェクトを単一バッチで処理し、効率的な計画作成を実現します。

## 移行戦略

### アプローチの選択と正当性

**選択: All-At-Once (一括) 戦略**

すべてのプロジェクトを単一の統合操作で同時にアップグレードします。

#### All-At-Once 戦略の特徴

**メリット**:
- ? 最短の完了時間
- ? マルチターゲティングの複雑さなし
- ? すべてのプロジェクトが同時に恩恵を受ける
- ? クリーンな依存関係解決
- ? シンプルな調整
- ? 単一コミットでの追跡が容易

**デメリット**:
- ?? 初期リスクが若干高い（ただし、このソリューションでは許容範囲）
- ?? より大きなテスト範囲（54 ファイルの影響）
- ?? すべての開発者が同時に適応する必要がある

**このソリューションでの正当性**:

1. **小規模ソリューション**: プロジェクト数が 2 個のみ
2. **最新基盤**: すべてのプロジェクトが既に .NET 8 以上
3. **シンプルな依存関係**: 深さ 1、循環なし
4. **パッケージ互換性**: すべてのパッケージに .NET 10 対応版が存在
5. **リスクレベル**: セキュリティ脆弱性なし、Azure Vision 移行は管理可能
6. **チーム規模**: 個人開発プロジェクト

#### All-At-Once 戦略の実装原則

**コアプリンシプル**: 最大限の統合

以下の操作は相互依存しており、単一タスクに統合される必要があります：
- すべてのプロジェクトファイルのフレームワーク更新（TargetFramework 変更）
- すべてのプロジェクト横断的なパッケージ参照更新
- 依存関係の復元
- ソリューションのビルド
- フレームワーク/パッケージアップグレードによるコンパイルエラーの修正
- 成功検証のための再ビルド

**根拠**: これらの操作は意味的に分離できません。パッケージ更新なしでプロジェクトファイル更新をテストできません。新パッケージなしでビルドを検証できません。コンパイルエラーはビルド後にのみ表示されます。分割すると、価値を提供しない人為的なチェックポイントが作成されます。

### 依存関係ベースの順序付け

**All-At-Once での依存関係管理**:

1. **プロジェクトファイル更新**: 並行実行可能（依存関係順序は不要）
   - CheckListMaker.csproj の TargetFrameworks を更新
   - CheckListMakerTest.Tests.csproj の TargetFramework を更新

2. **パッケージ更新**: 並行実行可能（パッケージは独立）
   - すべての Microsoft.Extensions.* パッケージを 10.0.2 に更新
   - Newtonsoft.Json を 13.0.4 に更新
   - 不要なパッケージを削除（System.Text.RegularExpressions）

3. **ビルド**: MSBuild が依存関係順序を自動処理
   - CheckListMaker が最初にビルド
   - CheckListMakerTest.Tests が次にビルド

4. **コンパイルエラー修正**: 発見順に修正
   - MAUI Animation API の更新
   - TextColor プロパティの更新
   - ConfigurationBinder API の更新
   - その他の非互換 API の修正

### 並行実行と逐次実行

**並行可能な操作**:
- ? プロジェクトファイル編集（2 プロジェクトは独立）
- ? パッケージ参照追加/更新（異なるパッケージ）

**逐次実行が必要な操作**:
- ?? 依存関係の復元 → ビルド → エラー修正 → 再ビルド
- ?? フレームワークアップグレード → Azure Vision 移行（依存関係あり）
- ?? コード修正 → テスト実行

**All-At-Once での調整**:
- すべてのプロジェクトが同じブランチで作業
- 単一の統合コミットで追跡
- テストは全体が完了してから実行

### フェーズ定義

#### フェーズ 0: 前提条件（該当する場合）

**目的**: 環境が .NET 10 の準備ができていることを確認

**操作**:
1. .NET 10 SDK インストール確認
   ```powershell
   dotnet --list-sdks | Select-String "10.0"
   ```
2. MAUI ワークロード確認/更新
   ```powershell
   dotnet workload list
   dotnet workload update
   ```
3. global.json 確認（存在する場合は更新）

**成果物**: 環境準備完了

#### フェーズ 1: 一括フレームワークとパッケージアップグレード

**目的**: すべてのプロジェクトを .NET 10 に移行し、パッケージを更新

**操作**（単一の統合バッチとして実行）:
1. すべてのプロジェクトファイルを更新
   - CheckListMaker.csproj: `net8.0;net8.0-android` → `net10.0;net10.0-android`
   - CheckListMakerTest.Tests.csproj: `net8.0` → `net10.0`

2. すべてのパッケージ参照を更新（プロジェクト横断）
   - Microsoft.Extensions.* パッケージ: 8.0.x → 10.0.2
   - Newtonsoft.Json: 13.0.3 → 13.0.4
   - System.Text.RegularExpressions を削除（フレームワークに統合済み）

3. 依存関係を復元してビルド
   ```powershell
   dotnet restore CheckListMaker.sln
   dotnet build CheckListMaker.sln
   ```

4. すべてのコンパイルエラーを修正
   - Animation API の更新（180 件の問題）
   - TextColor プロパティの更新（208 件の問題）
   - ConfigurationBinder.Get<T> の更新（バイナリ非互換）
   - その他の MAUI API 更新

5. 再ビルドして成功を検証
   ```powershell
   dotnet build CheckListMaker.sln --no-restore
   ```

**成果物**: エラー 0 でビルドされるソリューション

**期待される問題と対処**:
- `Microsoft.Maui.Controls.Animation`: 新しい Animation API に移行
- `TextColor`: `Color` 型から `Brush` 型への変更対応
- `ConfigurationBinder.Get<T>()`: `GetRequiredSection().Get<T>()` パターンへの更新

#### フェーズ 2: Azure AI Vision 移行

**目的**: 旧 Vision SDK を新 SDK に置き換え

**操作**:
1. パッケージの置き換え
   - 削除: `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` v7.0.1
   - 追加: `Azure.AI.Vision.ImageAnalysis` v1.0.0-beta.3 以降

2. `ComputerVisionService.cs` のリファクタリング
   - `ComputerVisionClient` → `ImageAnalysisClient`
   - `ReadInStreamAsync` → `Analyze` メソッド
   - 認証: `ApiKeyServiceClientCredentials` → `AzureKeyCredential`

3. 設定の更新
   - `CVConstants` クラスの更新（必要に応じて）
   - `appsettings.json` の設定確認

4. ユニットテストの更新
   - `ComputerVisionServiceTests.cs` のモックとアサーションを更新

5. ビルドと検証
   ```powershell
   dotnet build CheckListMaker.sln
   ```

**成果物**: 新 SDK で動作する ComputerVisionService

#### フェーズ 3: テスト検証

**目的**: すべての機能が正しく動作することを確認

**操作**:
1. すべてのテストプロジェクトを実行
   ```powershell
   dotnet test CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj
   ```

2. テスト失敗に対処
   - 失敗の根本原因を分析
   - 必要なコード修正を実施
   - 再テスト

3. OCR 機能の統合テスト（手動）
   - サンプル画像で OCR 処理を実行
   - チェックリスト生成を確認

**成果物**: すべてのテストが成功

---

**All-At-Once 戦略の重要な注意事項**:

1. **単一の原子操作**: フェーズ 1 のすべての手順は、単一の連続した操作として実行されます
2. **中間状態なし**: 「一部のプロジェクトのみアップグレード」という状態は存在しません
3. **ビルドが成功基準**: フェーズ 1 完了の明確な指標はエラー 0 のビルド
4. **テストは最後**: すべてのフレームワークとパッケージ変更が完了してからテストを実行

## 詳細な依存関係分析

### 依存関係グラフの概要

CheckListMaker ソリューションは、明確でシンプルな依存関係構造を持っています：

```
Level 0 (基盤 - 依存関係なし):
  └─ CheckListMaker.csproj
      ├─ 問題数: 625 件 (必須: 5 件)
      └─ 使用元: CheckListMakerTest.Tests

Level 1 (Level 0 のみに依存):
  └─ CheckListMakerTest.Tests.csproj
      ├─ 依存先: CheckListMaker
      ├─ 問題数: 342 件 (必須: 1 件)
      └─ 使用元: なし (最上位)
```

**特徴**:
- **依存関係の深さ**: 1 レベル（非常に浅い）
- **循環依存関係**: なし
- **クリティカルパス**: CheckListMaker → CheckListMakerTest.Tests

### 移行フェーズ別のプロジェクトグループ

All-At-Once 戦略では、すべてのプロジェクトを単一フェーズで同時に移行します。

**フェーズ 1: 一括アップグレード（すべてのプロジェクト）**

| プロジェクト | 種別 | 現在の TFM | 目標 TFM | 問題数 | 複雑度 |
|-------------|------|-----------|----------|--------|--------|
| CheckListMaker.csproj | MAUI アプリ | net8.0; net8.0-android | net10.0; net10.0-android | 625 | 中 |
| CheckListMakerTest.Tests.csproj | テスト | net8.0 | net10.0 | 342 | 低 |

**グループ化の根拠**:
- 両プロジェクトは互いに密接に関連
- 依存関係は単純（一方向）
- 同時更新により一貫性を維持
- テストプロジェクトは依存元のアップグレードと同期する必要がある

### クリティカルパスの識別

**主要パス**: CheckListMaker → CheckListMakerTest.Tests

**重要な考慮事項**:
1. **CheckListMaker.csproj が基盤**
   - すべての機能コードを含む
   - MAUI Controls、Services、Models、ViewModels を含む
   - Azure AI Vision 統合を含む
   - テストプロジェクトから参照される

2. **CheckListMakerTest.Tests.csproj はテストレイヤー**
   - CheckListMaker への参照を持つ
   - 基盤プロジェクトと互換性のある TFM が必要
   - ユニットテストとモックテストを含む

**All-At-Once での実行順序**:
- すべてのプロジェクトファイルを同時に更新（TargetFramework プロパティ）
- すべてのパッケージ参照を同時に更新
- 依存関係を復元
- ソリューション全体をビルド（依存関係順序は MSBuild が自動処理）
- すべてのコンパイルエラーを修正
- すべてのテストを実行

### 循環依存関係の詳細

**検出結果**: 循環依存関係なし ?

このソリューションはクリーンな階層構造を持ち、移行を大幅に簡素化します。

## プロジェクト別移行計画

All-At-Once 戦略では、両プロジェクトを同時にアップグレードしますが、各プロジェクトの詳細な仕様を以下に示します。

### CheckListMaker.csproj

**プロジェクト種別**: .NET MAUI アプリケーション (ClassLibrary)

#### 現在の状態
- **TargetFrameworks**: `net8.0;net8.0-android`
- **SDK スタイル**: True
- **依存プロジェクト数**: 0
- **依存元プロジェクト数**: 1 (CheckListMakerTest.Tests)
- **パッケージ数**: 18 個
- **ファイル数**: 55 ファイル
- **コード行数**: 2,466 行
- **問題数**: 625 件（必須: 5 件）
- **影響を受ける行数**: 615+ 行（24.9%）

#### 目標の状態
- **TargetFrameworks**: `net10.0;net10.0-android`
- **主要パッケージ更新**: 
  - Microsoft.Extensions.* → 10.0.2
  - Newtonsoft.Json → 13.0.4
  - Azure.AI.Vision.ImageAnalysis → 1.0.0-beta.3+ （新規）
- **削除**: System.Text.RegularExpressions（フレームワークに統合済み）

#### 移行手順

1. **TargetFrameworks の更新**
   
   プロジェクトファイル `CheckListMaker/CheckListMaker.csproj` を開き、`TargetFrameworks` プロパティを更新：
   
   ```xml
   <TargetFrameworks>net10.0;net10.0-android</TargetFrameworks>
   ```
   
   **注意**: 
   - iOS、Windows、macOS のターゲットはコメントアウトされているため、現時点では更新不要
   - 将来的に有効化する場合は `net10.0-ios`、`net10.0-windows10.0.19041.41`、`net10.0-maccatalyst` に更新

2. **パッケージ参照の更新**
   
   以下のパッケージを更新：
   
   ```xml
   <!-- Microsoft.Extensions.* パッケージ群 -->
   <PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.2" />
   <PackageReference Include="Microsoft.Extensions.Configuration.Binder" Version="10.0.2" />
   <PackageReference Include="Microsoft.Extensions.Configuration.FileExtensions" Version="10.0.2" />
   <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.2" />
   <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="10.0.2" />
   <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.2" />
   <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.2" />
   
   <!-- JSON シリアライゼーション -->
   <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
   ```
   
   削除するパッケージ（フレームワークに統合済み）：
   ```xml
   <!-- この行を削除 -->
   <PackageReference Include="System.Text.RegularExpressions" Version="4.3.1" />
   ```

3. **依存関係の復元**
   
   ```powershell
   cd C:\repos\CheckListMaker\CheckListMaker
   dotnet restore
   ```

4. **初回ビルドの実行**
   
   ```powershell
   dotnet build CheckListMaker.csproj
   ```
   
   予想: コンパイルエラーが発生します。次の手順で修正します。

#### 予想される Breaking Changes

1. **Microsoft.Maui.Controls.Animation API (180 件)**
   
   **問題**: Animation クラスのコンストラクタとメソッドが変更されています。
   
   **影響を受けるファイル**:
   - ViewModels での アニメーション使用
   - Custom Controls のアニメーション
   
   **移行パターン**:
   ```csharp
   // 旧 (.NET 8)
   var animation = new Animation(v => myElement.Scale = v, 1, 1.2);
   animation.Commit(this, "ScaleAnimation", length: 250);
   
   // 新 (.NET 10) - 拡張メソッドを使用
   myElement.ScaleTo(1.2, 250);
   
   // または新しい Animation API
   var animation = new Microsoft.Maui.Controls.Animation();
   // 新 API に従って構築
   ```

2. **TextColor プロパティ (208 件)**
   
   **問題**: `Color` 型から `Brush` 型へ変更。
   
   **影響を受けるコントロール**:
   - Label.TextColor
   - Button.TextColor
   - Entry.TextColor
   - Picker.TextColor
   - DatePicker.TextColor
   - TimePicker.TextColor
   - RadioButton.TextColor
   - SearchBar.TextColor
   
   **移行パターン**:
   ```csharp
   // 旧 (.NET 8)
   myLabel.TextColor = Colors.Blue;
   
   // 新 (.NET 10)
   myLabel.TextColor = Colors.Blue; // Color は自動的に Brush に変換される
   
   // または明示的に
   myLabel.TextColor = new SolidColorBrush(Colors.Blue);
   ```
   
   **XAML での変更**:
   ```xml
   <!-- 旧 -->
   <Label TextColor="Blue" />
   
   <!-- 新 - ほとんどの場合そのまま動作 -->
   <Label TextColor="Blue" />
   ```
   
   **注意**: ほとんどの場合、既存のコードは自動変換されます。明示的な `Brush` の使用が必要なのは複雑なシナリオのみです。

3. **Microsoft.Extensions.Configuration.ConfigurationBinder.Get<T> (3 件)**
   
   **問題**: バイナリ非互換。実行時エラーの可能性。
   
   **影響を受けるファイル**:
   - `Services/ComputerVisionService.cs` (CVConstants の読み込み)
   - 他の設定読み込みコード
   
   **移行パターン**:
   ```csharp
   // 旧 (.NET 8)
   var constants = config.GetSection("ComputerVision").Get<CVConstants>();
   
   // 新 (.NET 10)
   var constants = config.GetRequiredSection("ComputerVision").Get<CVConstants>();
   ```
   
   **該当箇所**:
   - `ComputerVisionService.cs` line 17:
     ```csharp
     _constants = config.GetRequiredSection("ComputerVision").Get<CVConstants>();
     ```

4. **その他の MAUI Controls API (576 件)**
   
   これらのほとんどはソース非互換であり、再コンパイルで解決される可能性が高いです：
   
   - `Shell.Current`
   - `Application.Current`
   - `Application.MainPage`
   - `Routing.RegisterRoute`
   - `Preferences.Default`
   - `BindableObject.BindingContext`
   - 各種コントロールのコンストラクタ
   
   **対処**: ビルドエラーメッセージを確認し、必要に応じて API 呼び出しを更新します。

#### コード修正

**優先順位付き修正手順**:

1. **高優先度: ConfigurationBinder.Get<T> の更新**
   
   ファイル: `Services/ComputerVisionService.cs`
   
   ```csharp
   // Line 17 を更新
   _constants = config.GetRequiredSection("ComputerVision").Get<CVConstants>();
   ```

2. **中優先度: Animation API の更新**
   
   - Animation 使用箇所を検索: `new Animation(`
   - 各箇所を新しい Animation API または拡張メソッドに置き換え
   - Microsoft MAUI Community Toolkit の使用を検討

3. **低優先度: TextColor プロパティ**
   
   - ほとんどの場合は自動変換される
   - ビルドエラーが出た箇所のみ明示的に `Brush` に変換

4. **その他の API 更新**
   
   - ビルドエラーを上から順に修正
   - 共通パターンを特定して一括修正
   - 不明な場合は Microsoft Docs を参照

#### テスト戦略

**ユニットテスト** (CheckListMakerTest.Tests で実行):
- すべての Service クラスのテスト
- ViewModel のロジックテスト
- Model の検証テスト

**統合テスト** (手動):
1. **アプリケーション起動テスト**
   - Android エミュレータでアプリを起動
   - クラッシュせずに起動することを確認

2. **UI ナビゲーションテスト**
   - すべての主要画面を巡回
   - Shell ナビゲーションが機能することを確認

3. **機能テスト** (OCR 以外):
   - チェックリストの作成/編集/削除
   - データの永続化（LiteDB）
   - 設定の読み込み

4. **OCR 機能テスト** (フェーズ 2 後):
   - Azure Vision 移行後に実施
   - サンプル画像での OCR 実行
   - チェックリスト生成の確認

#### 検証チェックリスト

**フェーズ 1 完了基準**:
- [ ] プロジェクトファイルが `net10.0;net10.0-android` をターゲットに設定
- [ ] すべてのパッケージ参照が更新済み
- [ ] `System.Text.RegularExpressions` が削除済み
- [ ] `dotnet restore` がエラーなしで完了
- [ ] `dotnet build` がエラー 0 で完了
- [ ] ビルド警告を確認（重大な警告がないこと）
- [ ] 依存プロジェクト (CheckListMakerTest.Tests) が正常にビルド

**コード品質確認**:
- [ ] Animation API が正しく更新されている
- [ ] TextColor の使用箇所が正常に動作
- [ ] ConfigurationBinder が `GetRequiredSection().Get<T>()` パターンを使用
- [ ] その他の Breaking Changes が対処済み

**機能確認** (ビルド後):
- [ ] アプリケーションが起動する（Android エミュレータ）
- [ ] 主要な UI が表示される
- [ ] クラッシュやエラーログがない

**次のフェーズへの準備**:
- [ ] すべての変更がコミット済み
- [ ] ビルドが安定している
- [ ] Azure Vision 移行の準備ができている

---

### CheckListMakerTest.Tests.csproj

**プロジェクト種別**: xUnit テストプロジェクト (DotNetCoreApp)

#### 現在の状態
- **TargetFramework**: `net8.0`
- **SDK スタイル**: True
- **依存プロジェクト数**: 1 (CheckListMaker)
- **依存元プロジェクト数**: 0
- **パッケージ数**: 8 個
- **ファイル数**: 10 ファイル
- **コード行数**: 1,420 行
- **問題数**: 342 件（必須: 1 件）
- **影響を受ける行数**: 340+ 行（23.9%）

#### 目標の状態
- **TargetFramework**: `net10.0`
- **主要パッケージ**: すべて互換性あり（更新不要）
- **注意**: xunit (2.9.3) は非推奨マークされているが、.NET 10 と互換性あり

#### 移行手順

1. **TargetFramework の更新**
   
   プロジェクトファイル `CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj` を開き、`TargetFramework` プロパティを更新：
   
   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **パッケージ参照の確認**
   
   このプロジェクトのすべてのパッケージは .NET 10 と互換性があります。更新は不要です：
   
   - `CommunityToolkit.Maui` 9.1.1 ?
   - `LiteDB` 5.0.21 ?
   - `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` 7.0.1 ? (フェーズ 2 で置換)
   - `Microsoft.NET.Test.Sdk` 17.14.1 ?
   - `Moq` 4.20.72 ?
   - `Plugin.MauiMTAdmob` 2.0.0.5 ?
   - `coverlet.collector` 6.0.4 ?
   - `xunit` 2.9.3 ? (非推奨だが動作する)
   - `xunit.runner.visualstudio` 3.1.1 ?

3. **依存関係の復元**
   
   ```powershell
   cd C:\repos\CheckListMaker\CheckListMakerTest.Tests
   dotnet restore
   ```

4. **初回ビルドの実行**
   
   ```powershell
   dotnet build CheckListMakerTest.Tests.csproj
   ```
   
   予想: CheckListMaker.csproj が先にビルドされるため、そちらのエラーが先に表示されます。

#### 予想される Breaking Changes

テストプロジェクトでの主な Breaking Changes は、CheckListMaker プロジェクトに起因するものです：

1. **参照する MAUI API の変更**
   
   テストコード内で CheckListMaker のクラスを参照する場合、そのクラスで使用されている MAUI API の変更が影響します。
   
   **対処**: ほとんどの場合、テストコード自体の変更は不要です。CheckListMaker プロジェクトのビルドが成功すれば、テストプロジェクトも自動的に解決されます。

2. **xunit の非推奨警告**
   
   xunit 2.x は非推奨としてマークされていますが、.NET 10 では引き続き動作します。
   
   **対処**: 
   - 短期: そのまま使用（動作に問題なし）
   - 長期: xunit 3.x への移行を検討（別の作業として）

3. **モックテストの調整**
   
   `Moq` を使用したテストで、CheckListMaker のインターフェースが変更された場合、モック設定の更新が必要です。
   
   **対処**: テスト実行時のエラーメッセージに従って修正します。

#### コード修正

**優先順位付き修正手順**:

1. **CheckListMaker プロジェクトのビルドを最優先**
   
   テストプロジェクトは CheckListMaker に依存しているため、まず CheckListMaker のビルドを成功させる必要があります。

2. **テストプロジェクト固有のエラー修正**
   
   CheckListMaker のビルド成功後、テストプロジェクトで発生するエラーを修正：
   
   - 型の不一致エラー
   - モック設定の更新
   - アサーションの調整

3. **非推奨警告の確認**
   
   xunit の非推奨警告は、現時点では無視しても問題ありません。

#### テスト戦略

**ユニットテストの実行**:

1. **すべてのテストを実行**
   
   ```powershell
   cd C:\repos\CheckListMaker
   dotnet test CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj
   ```

2. **テスト結果の確認**
   
   期待される結果:
   - すべてのテストが成功（グリーン）
   - 失敗したテストがある場合は、原因を分析

3. **失敗したテストの分類**
   
   テストが失敗した場合、以下のカテゴリに分類：
   
   - **フレームワーク変更による失敗**: API の動作変更が原因
   - **モックの不整合**: CheckListMaker の変更がモックに反映されていない
   - **既存のバグ**: アップグレード前から存在する問題
   
4. **テストの更新**
   
   失敗したテストを順次修正：
   
   ```csharp
   // 例: モック設定の更新
   // 旧
   mockService.Setup(x => x.GetItems()).Returns(items);
   
   // 新 (インターフェースが変更された場合)
   mockService.Setup(x => x.GetItemsAsync()).ReturnsAsync(items);
   ```

5. **カバレッジの確認**
   
   ```powershell
   dotnet test --collect:"XPlat Code Coverage"
   ```
   
   新しい .NET 10 の機能でカバレッジが維持されていることを確認します。

#### 検証チェックリスト

**フェーズ 1 完了基準**:
- [ ] プロジェクトファイルが `net10.0` をターゲットに設定
- [ ] `dotnet restore` がエラーなしで完了
- [ ] `dotnet build` がエラー 0 で完了
- [ ] CheckListMaker.csproj への参照が正常に解決
- [ ] ビルド警告を確認（xunit 非推奨警告は許容）

**テスト実行確認**:
- [ ] `dotnet test` がエラーなしで完了
- [ ] すべてのユニットテストが成功
- [ ] 失敗したテストがある場合は修正済み
- [ ] テストカバレッジが維持されている

**コード品質確認**:
- [ ] モック設定が CheckListMaker の変更に対応
- [ ] アサーションが正しい値を検証
- [ ] テストコードに不要な変更がない

**フェーズ 2 への準備**:
- [ ] すべてのテストが安定して成功
- [ ] `ComputerVisionServiceTests.cs` が Azure Vision 移行の準備ができている
- [ ] テストデータとモックが最新の状態

## Azure AI Vision 移行計画

### 現在の実装分析

#### 使用されている旧 SDK

**パッケージ**: `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` v7.0.1

**廃止スケジュール**: 2028年9月25日に Azure AI Vision Image Analysis API が廃止されます。

#### 現在のコード構造

**主要ファイル**:
1. `CheckListMaker/Services/ComputerVisionService.cs` (約 90 行)
   - シングルトンパターンで実装
   - OCR 処理を担当
   - 画像ファイルからテキストを抽出してチェックリストを生成

2. `CheckListMaker/Services/IComputerVisionService.cs`
   - インターフェース定義
   - `Task<CheckList> GetCheckItems(string localFile)`

3. `CheckListMaker/Services/CVConstants.cs`
   - 設定クラス
   - `Key` と `EndPoint` プロパティ

4. `CheckListMakerTest.Tests/MauiApps/Services/ComputerVisionServiceTests.cs`
   - ユニットテスト

**現在の実装フロー**:
```csharp
1. CreateComputerVisionClient() 
   → ApiKeyServiceClientCredentials でクライアント作成
   
2. StartOcrOperation(client, localFile)
   → ReadInStreamAsync() を呼び出し
   → Operation ID を取得
   
3. WaitForOcrResults(client, operationId)
   → GetReadResultAsync() でポーリング
   → Running/NotStarted の間は 1 秒ごとに再試行
   
4. ExtractCheckItems(analyzeResults)
   → ReadResults から行ごとにテキストを抽出
   → CheckItem のリストに変換
```

**使用される主要 API**:
- `ComputerVisionClient`
- `ApiKeyServiceClientCredentials`
- `ReadInStreamAsync(Stream)`
- `GetReadResultAsync(Guid)`
- `ReadOperationResult`
- `AnalyzeResults`

### 移行パス

#### 新 SDK の概要

**新パッケージ**: `Azure.AI.Vision.ImageAnalysis`

**推奨バージョン**: v1.0.0-beta.3 以降（GA版が利用可能な場合はそちらを使用）

**主な変更点**:
1. **パッケージ名**: `Microsoft.Azure.CognitiveServices.*` → `Azure.AI.*`
2. **認証**: `ApiKeyServiceClientCredentials` → `AzureKeyCredential` または `TokenCredential`
3. **クライアント**: `ComputerVisionClient` → `ImageAnalysisClient`
4. **OCR メソッド**: `ReadInStreamAsync` → `Analyze` with `VisualFeatures.Read`
5. **結果取得**: ポーリング不要（同期的な結果取得）
6. **名前空間**: `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` → `Azure.AI.Vision.ImageAnalysis`

**メリット**:
- ? 最新の Azure SDK アーキテクチャ（Azure.Core ベース）
- ? より高速で効率的な API
- ? 改善された認証オプション
- ? .NET 10 との完全互換性
- ? 2028年以降も長期サポート

### 実装手順

#### ステップ 1: パッケージの置き換え

1. **旧パッケージを削除**
   
   両プロジェクトから以下を削除：
   ```xml
   <PackageReference Include="Microsoft.Azure.CognitiveServices.Vision.ComputerVision" Version="7.0.1" />
   ```

2. **新パッケージを追加**
   
   CheckListMaker プロジェクトに追加：
   ```xml
   <PackageReference Include="Azure.AI.Vision.ImageAnalysis" Version="1.0.0-beta.3" />
   ```
   
   **注意**: CheckListMakerTest.Tests プロジェクトでは、テストが CheckListMaker を参照しているため、明示的な追加は不要です。

3. **依存関係の復元**
   
   ```powershell
   dotnet restore CheckListMaker.sln
   ```

#### ステップ 2: ComputerVisionService のリファクタリング

**ファイル**: `CheckListMaker/Services/ComputerVisionService.cs`

**変更内容**:

```csharp
using CheckListMaker.Models;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Configuration;

namespace CheckListMaker.Services;

/// <summary> Azure Computer Vision のサービスクラス </summary>
internal sealed class ComputerVisionService : IComputerVisionService
{
    private static readonly object _lockObject = new();
    private static ComputerVisionService _instance = null;
    private readonly CVConstants _constants;

    private ComputerVisionService(IConfiguration config) =>
        _constants = config.GetRequiredSection("ComputerVision").Get<CVConstants>();

    /// <summary> Instanceを返す </summary>
    public static ComputerVisionService GetInstance(IConfiguration config)
    {
        if (_instance == null)
        {
            lock (_lockObject)
            {
                _instance ??= new ComputerVisionService(config);
            }
        }

        return _instance;
    }

    /// <summary>
    /// パラメータの画像ファイルをComputer VisionでOCR処理し、CheckItemのListを生成して返す
    /// </summary>
    public async Task<CheckList> GetCheckItems(string localFile)
    {
        var client = CreateImageAnalysisClient();
        var result = await AnalyzeImageAsync(client, localFile);
        
        return ExtractCheckItems(result);
    }

    private ImageAnalysisClient CreateImageAnalysisClient()
    {
        var endpoint = new Uri(_constants.EndPoint);
        var credential = new AzureKeyCredential(_constants.Key);
        
        return new ImageAnalysisClient(endpoint, credential);
    }

    private async Task<ImageAnalysisResult> AnalyzeImageAsync(
        ImageAnalysisClient client, 
        string localFile)
    {
        using var imageStream = File.OpenRead(localFile);
        
        var imageData = BinaryData.FromStream(imageStream);
        
        // Read 機能を使用してテキストを抽出
        var result = await client.AnalyzeAsync(
            imageData,
            VisualFeatures.Read,
            new ImageAnalysisOptions { Language = "ja" });
        
        return result.Value;
    }

    private CheckList ExtractCheckItems(ImageAnalysisResult result)
    {
        var items = new CheckList();

        if (result.Read?.Blocks == null)
        {
            return items;
        }

        foreach (var block in result.Read.Blocks)
        {
            foreach (var line in block.Lines)
            {
                var text = line.Text.StartsWith('?')
                    ? line.Text.Remove(0, 1).Trim()
                    : line.Text.Trim();

                items.Items.Add(new CheckItem { ItemText = text });
            }
        }

        return items;
    }
}
```

**主な変更点の説明**:

1. **using ディレクティブ**:
   - `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` を削除
   - `Azure`, `Azure.AI.Vision.ImageAnalysis` を追加

2. **クライアント作成**:
   - `ComputerVisionClient` → `ImageAnalysisClient`
   - `ApiKeyServiceClientCredentials` → `AzureKeyCredential`
   - エンドポイントは `Uri` 型で指定

3. **OCR 処理**:
   - `ReadInStreamAsync` + ポーリング → `AnalyzeAsync` (単一呼び出し)
   - `BinaryData.FromStream` でストリームをラップ
   - `VisualFeatures.Read` で OCR 機能を指定
   - 言語オプション (`Language = "ja"`) を指定可能

4. **結果の抽出**:
   - `AnalyzeResults.ReadResults` → `ImageAnalysisResult.Read.Blocks`
   - `ReadResult.Lines` → `DetectedTextBlock.Lines`
   - テキスト抽出ロジックは同様

#### ステップ 3: 設定の確認

**ファイル**: `appsettings.json` または `appsettings.Development.json`

既存の設定をそのまま使用できます：
```json
{
  "ComputerVision": {
    "EndPoint": "https://your-endpoint.cognitiveservices.azure.com/",
    "Key": "your-api-key"
  }
}
```

**注意**: エンドポイント URL が変更されている可能性があるため、Azure Portal で確認してください。

#### ステップ 4: ユニットテストの更新

**ファイル**: `CheckListMakerTest.Tests/MauiApps/Services/ComputerVisionServiceTests.cs`

**必要な変更**:

1. **モックの更新**:
   - `ComputerVisionClient` のモック → `ImageAnalysisClient` のモック
   - または、インターフェース (`IComputerVisionService`) レベルでのテストに焦点を当てる

2. **テストデータの調整**:
   - `ImageAnalysisResult` の構造に合わせてモックデータを作成

3. **アサーションの確認**:
   - OCR 結果の検証ロジックが新 SDK の結果構造に対応

**例** (モックを使用しない統合テストの場合):
```csharp
[Fact]
public async Task GetCheckItems_WithValidImage_ReturnsCheckList()
{
    // Arrange
    var config = GetTestConfiguration();
    var service = ComputerVisionService.GetInstance(config);
    var testImagePath = "test-data/sample-checklist.jpg";

    // Act
    var result = await service.GetCheckItems(testImagePath);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Items);
}
```

#### ステップ 5: ビルドと検証

1. **ビルド**
   
   ```powershell
   dotnet build CheckListMaker.sln
   ```
   
   期待: エラー 0 で完了

2. **ユニットテスト実行**
   
   ```powershell
   dotnet test CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj
   ```
   
   期待: すべてのテストが成功

3. **統合テスト** (手動):
   
   - Android エミュレータでアプリを起動
   - サンプル画像で OCR 機能を実行
   - チェックリストが正しく生成されることを確認

### テスト戦略

#### 単体テスト

**ComputerVisionService のテスト**:
1. **正常系テスト**:
   - 有効な画像ファイルで OCR が成功
   - テキストが正しく抽出される
   - CheckList が期待通りに生成される

2. **異常系テスト**:
   - 無効なファイルパス
   - 空のファイル
   - テキストのない画像
   - API エラー（認証失敗など）

**モック戦略**:
- `IComputerVisionService` インターフェースを使用
- 実際の Azure API 呼び出しは統合テストで実施

#### 統合テスト (手動)

**テストシナリオ**:

1. **基本的な OCR 機能**:
   - サンプル画像 1: 手書きチェックリスト
   - サンプル画像 2: 印刷されたチェックリスト
   - サンプル画像 3: スクリーンショット

2. **エッジケース**:
   - 非常に小さいテキスト
   - 回転した画像
   - 低解像度の画像
   - 複数の言語が混在

3. **エラーハンドリング**:
   - ネットワークエラー
   - 認証エラー
   - レート制限エラー

**検証ポイント**:
- テキストの精度
- 処理時間（新 SDK の方が高速な可能性）
- エラーメッセージの適切性

#### パフォーマンステスト

新 SDK と旧 SDK のパフォーマンスを比較：
- 同じ画像での処理時間
- メモリ使用量
- ネットワーク帯域幅

### 検証チェックリスト

**フェーズ 2 完了基準**:

**パッケージ**:
- [ ] `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` が削除済み
- [ ] `Azure.AI.Vision.ImageAnalysis` が追加済み
- [ ] `dotnet restore` がエラーなしで完了

**コード**:
- [ ] `ComputerVisionService.cs` が新 SDK に更新済み
- [ ] using ディレクティブが正しい
- [ ] `ImageAnalysisClient` が使用されている
- [ ] `AzureKeyCredential` で認証している
- [ ] `AnalyzeAsync` メソッドが使用されている
- [ ] 結果の抽出ロジックが新構造に対応

**ビルド**:
- [ ] `dotnet build CheckListMaker.sln` がエラー 0 で完了
- [ ] ビルド警告がない（または許容範囲）

**テスト**:
- [ ] `ComputerVisionServiceTests.cs` が更新済み
- [ ] すべてのユニットテストが成功
- [ ] 統合テストでサンプル画像の OCR が成功
- [ ] チェックリスト生成が正常に動作

**機能確認**:
- [ ] Android エミュレータでアプリが起動
- [ ] OCR 機能が正常に動作
- [ ] テキスト抽出の精度が維持されている
- [ ] エラーハンドリングが適切

**ドキュメント**:
- [ ] コードコメントが更新済み
- [ ] README（存在する場合）が更新済み
- [ ] 設定ファイルの変更が文書化済み

### ロールバック手順

Azure Vision 移行のみをロールバックする場合：

1. **コミットを特定**
   ```powershell
   git log --oneline --grep="Azure Vision"
   ```

2. **特定のコミットをリバート**
   ```powershell
   git revert <commit-hash>
   ```

3. **旧 SDK に戻す**
   - `Azure.AI.Vision.ImageAnalysis` を削除
   - `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` v7.0.1 を追加
   - `ComputerVisionService.cs` を旧実装に戻す

4. **検証**
   ```powershell
   dotnet restore
   dotnet build CheckListMaker.sln
   dotnet test CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj
   ```

**注意**: フェーズ 1（フレームワークアップグレード）は維持され、Azure Vision 移行のみがロールバックされます。

## リスク管理

### 高リスク変更

All-At-Once 戦略における主要リスクとその軽減策を以下に示します。

| プロジェクト/領域 | リスクレベル | 説明 | 軽減策 |
|------------------|------------|------|--------|
| **Azure AI Vision 移行** | ?? 中 | SDK の完全な置き換えが必要。API が大きく変更されている可能性 | ? 移行前に新 SDK ドキュメントを確認<br>? 既存機能のテストカバレッジを確保<br>? サンプル画像での統合テスト<br>? ロールバック可能なコミット戦略 |
| **MAUI Animation API** | ?? 中 | 180 件の非互換（18.8%）。広範囲な更新が必要 | ? Microsoft 公式移行ガイドを参照<br>? Animation 使用箇所を事前に特定<br>? 段階的なコミット（コンパイル後） |
| **TextColor プロパティ** | ?? 中 | 208 件の非互換（21.5%）。Color → Brush への型変更 | ? 一括置換パターンの使用<br>? ビルド後に視覚的検証<br>? カラーリソース定義の確認 |
| **ConfigurationBinder.Get<T>** | ?? 高 | バイナリ非互換（3 件）。実行時エラーの可能性 | ? 該当箇所を事前に特定<br>? 新しい API パターンへの更新<br>? ユニットテストでの検証 |
| **一括ビルドエラー** | ?? 中 | すべてのプロジェクトが同時に影響を受ける | ? エラーを優先度順に修正<br>? 共通パターンを最初に対処<br>? 必要に応じて個別ビルドで検証 |

### セキュリティ脆弱性

**良いニュース**: アセスメントでセキュリティ脆弱性は検出されませんでした ?

すべてのパッケージは脆弱性なしで、安全な状態です。

### コンティンジェンシープラン

#### シナリオ 1: Azure AI Vision 移行が失敗

**症状**:
- 新 SDK でのビルドエラー
- OCR 機能の動作不良
- 認証エラー

**対処**:
1. **短期**: 旧 SDK を一時的に維持
   - `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` v7.0.1 を保持
   - 新 SDK への移行を別ブランチで継続
   - .NET 10 アップグレードは完了させる

2. **中期**: Azure AI Document Intelligence への移行を検討
   - より高度な OCR 機能
   - 異なる API だが類似の機能

3. **ロールバック**: フェーズ 2 の変更のみをリバート
   - フェーズ 1（フレームワークアップグレード）は維持
   - Azure Vision 移行のコミットを revert

#### シナリオ 2: MAUI Animation API の大規模な Breaking Changes

**症状**:
- Animation 関連のコンパイルエラーが修正不可
- アニメーション動作の異常

**対処**:
1. **代替 API の使用**:
   - .NET MAUI Community Toolkit のアニメーション拡張
   - プラットフォーム固有のアニメーション API

2. **アニメーション の一時的な無効化**:
   - UI 機能は保持し、アニメーションのみ削除
   - 後のイテレーションで再実装

3. **全体ロールバック**:
   - ブランチ全体をリバート
   - .NET 8 に留まる（サポート期間内）

#### シナリオ 3: テスト失敗が多発

**症状**:
- 多数のユニットテストが失敗
- 統合テストでの予期しない動作

**対処**:
1. **失敗の分類**:
   - フレームワーク変更によるもの
   - Azure Vision 移行によるもの
   - 既存のバグ

2. **段階的修正**:
   - 高優先度テスト（コア機能）から修正
   - モックの更新が必要なテストを特定
   - 一時的にスキップするテストをマーク

3. **テストの更新**:
   - .NET 10 の新しい動作に合わせてテストを更新
   - 必要に応じてモックライブラリを更新

#### シナリオ 4: パフォーマンス劣化

**症状**:
- アプリケーションの起動が遅い
- OCR 処理の遅延

**対処**:
1. **プロファイリング**:
   - .NET Diagnostic tools での分析
   - ボトルネックの特定

2. **最適化**:
   - .NET 10 の新機能を活用（スパン、ValueTask など）
   - 不要な割り当ての削減

3. **設定の調整**:
   - MAUI ハンドラーの最適化
   - JIT/AOT コンパイル設定の見直し

#### ロールバック手順

**全体ロールバック** が必要な場合の手順：

1. **Git リバート**:
   ```powershell
   git checkout upgrade-to-NET10  # 元のブランチに戻る
   git branch -D upgrade-to-NET10-1  # アップグレードブランチを削除
   ```

2. **環境クリーンアップ**:
   ```powershell
   dotnet clean CheckListMaker.sln
   rm -r **/bin, **/obj  # すべてのビルド成果物を削除
   ```

3. **依存関係復元**:
   ```powershell
   dotnet restore CheckListMaker.sln
   dotnet build CheckListMaker.sln
   ```

4. **検証**:
   ```powershell
   dotnet test CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj
   ```

**部分ロールバック** の場合：
- 特定のコミットを `git revert` で取り消し
- 例: Azure Vision 移行のみをロールバック

---

**リスク軽減の全般的な原則**:
- ? 小さなコミットで段階的に変更
- ? 各フェーズ後にビルドとテストを実行
- ? ブランチ戦略で安全性を確保
- ? ドキュメント化された手順に従う
- ? バックアップと復元ポイントを維持

## テストと検証戦略

All-At-Once 戦略では、すべての変更が完了した後に包括的なテストを実行します。

### フェーズ別テスト要件

#### フェーズ 1: 一括アップグレード後のテスト

**ビルド検証**:
```powershell
# クリーンビルド
dotnet clean CheckListMaker.sln
dotnet build CheckListMaker.sln --no-incremental
```

**期待結果**:
- エラー: 0
- 警告: 最小限（xunit 非推奨警告は許容）
- すべてのプロジェクトがビルド成功

**ユニットテスト** (Azure Vision 移行前):
```powershell
dotnet test CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj --logger "console;verbosity=detailed"
```

**期待結果**:
- `ComputerVisionServiceTests` を除くすべてのテストが成功
- または、すべてのテストが成功（旧 SDK が .NET 10 と互換性がある場合）

**スモークテスト** (手動):
1. Android エミュレータでアプリを起動
2. メイン画面が表示されることを確認
3. 基本的なナビゲーションが機能することを確認
4. OCR 以外の機能をテスト:
   - チェックリストの作成
   - チェックリストの編集
   - チェックリストの削除
   - データの永続化

#### フェーズ 2: Azure Vision 移行後のテスト

**ビルド検証**:
```powershell
dotnet build CheckListMaker.sln
```

**ユニットテスト**:
```powershell
dotnet test CheckListMakerTest.Tests/CheckListMakerTest.Tests.csproj
```

**期待結果**:
- すべてのテストが成功（`ComputerVisionServiceTests` を含む）

**OCR 機能テスト** (手動):
1. サンプル画像を準備:
   - 手書きチェックリスト
   - 印刷されたチェックリスト
   - スクリーンショット
2. アプリで OCR 機能を実行
3. 結果を確認:
   - テキストが正しく抽出されている
   - CheckItem が適切に生成されている
   - エラーが発生していない

#### フェーズ 3: 最終検証

**完全な回帰テスト**:
```powershell
# すべてのテストを実行
dotnet test CheckListMaker.sln --logger "console;verbosity=detailed"

# カバレッジレポート生成（オプション）
dotnet test --collect:"XPlat Code Coverage"
```

**統合テスト** (手動):
1. **アプリケーション起動**:
   - Android エミュレータ
   - iOS シミュレータ（該当する場合）
   - Windows（該当する場合）

2. **すべての主要機能をテスト**:
   - ユーザー認証（該当する場合）
   - チェックリスト CRUD 操作
   - OCR 機能
   - データの永続化と復元
   - 設定の読み書き
   - AdMob 広告表示（該当する場合）

3. **エッジケースのテスト**:
   - ネットワーク切断時の動作
   - 低メモリ状態での動作
   - バックグラウンド/フォアグラウンド遷移
   - デバイスの回転

4. **パフォーマンステスト**:
   - アプリ起動時間
   - OCR 処理時間
   - UI の応答性
   - メモリ使用量

### スモークテスト

**目的**: 各フェーズ後に迅速に問題を検出

**チェックリスト**:
- [ ] アプリケーションが起動する
- [ ] クラッシュが発生しない
- [ ] メイン画面が表示される
- [ ] 基本的なナビゲーションが機能する
- [ ] データベースへのアクセスが正常
- [ ] ログにエラーが記録されていない

**実行時間**: 各フェーズ後 5 分以内

### 包括的検証

**目的**: 全機能が正常に動作することを確認

**機能テストマトリクス**:

| 機能 | テスト方法 | 成功基準 |
|------|----------|---------|
| チェックリスト作成 | 手動 | 新しいチェックリストが作成され、保存される |
| チェックリスト編集 | 手動 | 既存のチェックリストが編集され、変更が保存される |
| チェックリスト削除 | 手動 | チェックリストが削除され、データベースから消える |
| OCR 機能 | 手動 + 自動 | 画像からテキストが抽出され、CheckItem が生成される |
| データ永続化 | 手動 | アプリを再起動してもデータが保持される |
| 設定読み込み | 自動（ユニットテスト） | appsettings.json から設定が正しく読み込まれる |
| 広告表示 | 手動 | AdMob 広告が適切に表示される（該当する場合） |

**非機能テストマトリクス**:

| 項目 | 測定方法 | 成功基準 |
|------|---------|---------|
| 起動時間 | 手動計測 | .NET 8 版と同等またはそれ以下 |
| OCR 処理時間 | 手動計測 | 旧 SDK と同等またはそれ以下 |
| メモリ使用量 | Android Profiler | .NET 8 版と同等またはそれ以下 |
| UI 応答性 | 手動確認 | 遅延やフリーズがない |

### テスト環境

**必須環境**:
- Android エミュレータ (API 21+)
  - 推奨: Pixel 5 エミュレータ、Android 12 (API 31)
- Windows 10/11（該当する場合）

**オプション環境**:
- iOS シミュレータ (iOS 14+)
- macOS Catalyst (macOS 11+)
- 実機 Android デバイス

**ツール**:
- Visual Studio 2022 または VS Code
- Android Studio（エミュレータ管理用）
- Azure Portal（Vision API テスト用）

### テスト失敗時の対処

**ユニットテスト失敗**:
1. テストログを確認
2. 失敗の種類を分類:
   - フレームワーク変更によるもの
   - Azure Vision 移行によるもの
   - 既存のバグ
3. 優先度を決定（高: コア機能、低: エッジケース）
4. 修正して再テスト
5. 3 回失敗した場合は、ロールバックを検討

**統合テスト失敗**:
1. 失敗シナリオを記録
2. ログとスタックトレースを収集
3. 再現手順を確立
4. デバッガーで調査
5. 修正または回避策を実装
6. 回帰テストを追加

**パフォーマンス劣化**:
1. プロファイラーで分析
2. ボトルネックを特定
3. 最適化を実施
4. ベンチマークで検証
5. 許容範囲内に収まらない場合は、原因を調査

## 複雑度と工数評価

### プロジェクト別複雑度

All-At-Once 戦略での相対的複雑度評価（時間見積もりなし）：

| プロジェクト | 複雑度 | 依存関係 | リスク | 根拠 |
|-------------|--------|---------|-------|------|
| **CheckListMaker.csproj** | ?? 中 | 0 個（基盤） | 中 | ? 2,466 行のコード<br>? 625 件の問題<br>? Azure Vision 移行を含む<br>? MAUI 固有の API 変更<br>? 18 個のパッケージ更新 |
| **CheckListMakerTest.Tests.csproj** | ?? 低 | 1 個 (CheckListMaker) | 低 | ? 1,420 行のコード<br>? 342 件の問題<br>? テストコードのみ<br>? パッケージ更新不要<br>? CheckListMaker への依存のみ |

**複雑度の定義**:
- ?? **低**: 直接的な変更、明確なパターン、限定的な影響
- ?? **中**: 複数の変更領域、調査が必要、中程度の影響
- ?? **高**: 広範囲な変更、不確実性、高い影響

### フェーズ複雑度評価

All-At-Once 戦略における各フェーズの相対的複雑度：

#### フェーズ 0: 前提条件
- **複雑度**: ?? 低
- **要素**:
  - SDK インストール確認
  - ワークロード更新
  - global.json 確認
- **不確実性**: 最小限（標準的なツールチェーン操作）

#### フェーズ 1: 一括アップグレード
- **複雑度**: ?? 中?高
- **要素**:
  - プロジェクトファイル更新（2 ファイル）
  - パッケージ参照更新（9 パッケージ、1 削除）
  - コンパイルエラー修正（967 件の潜在的問題）
  - MAUI API の広範な更新
- **不確実性**: 中程度
  - ほとんどの API 変更はパターン化されている
  - 一部の Breaking Changes は調査が必要
  - ConfigurationBinder のバイナリ非互換に注意

**主要な変更領域**:
1. **Animation API** (180 件) - 中複雑度
   - パターン: 旧 API → 新 Animation システム
   - 影響: ViewModels、Custom Controls
   
2. **TextColor プロパティ** (208 件) - 低?中複雑度
   - パターン: `Color` → `Brush`
   - 影響: XAML、コードビハインド、スタイル
   
3. **ConfigurationBinder** (3 件) - 中複雑度
   - パターン: `.Get<T>()` → `.GetRequiredSection().Get<T>()`
   - 影響: 設定読み込みコード

4. **その他 MAUI API** (576 件) - 低?中複雑度
   - ほとんどは再コンパイルで解決
   - 一部は軽微なコード変更

#### フェーズ 2: Azure AI Vision 移行
- **複雑度**: ?? 中
- **要素**:
  - パッケージ置き換え（1 削除、1 追加）
  - `ComputerVisionService.cs` リファクタリング（約 90 行）
  - 認証メカニズム更新
  - ユニットテスト更新
- **不確実性**: 中程度
  - 新 SDK の API 構造が異なる
  - ドキュメントは利用可能
  - OCR 機能の基本的な流れは類似

#### フェーズ 3: テスト検証
- **複雑度**: ?? 低?中
- **要素**:
  - ユニットテスト実行
  - テスト失敗の修正
  - 統合テスト（手動）
- **不確実性**: 低?中
  - テストカバレッジに依存
  - 既存のテストは更新が必要な可能性

### リソース要件

**スキルレベル**:
- ? .NET MAUI の経験（必須）
- ? Azure SDK の基本知識（推奨）
- ? Git ワークフロー（必須）
- ? C# 12/13 の知識（推奨）

**並行作業能力**:
- このソリューションは小規模（2 プロジェクト）なので、単一開発者で効率的に実施可能
- All-At-Once 戦略は並行開発を必要としない（すべて単一ブランチで実施）

**必要なツール**:
- ? .NET 10 SDK
- ? Visual Studio 2022 17.10+ または VS Code
- ? MAUI ワークロード
- ? Git
- ? Azure サブスクリプション（Vision API テスト用）

---

**重要な注意**: このセクションでは相対的な複雑度のみを提供します。実際の作業時間は、開発者の経験、環境、予期しない問題によって大きく異なるため、時間見積もりは含まれていません。

## ソース管理戦略

All-At-Once 戦略に最適化された Git ワークフローを定義します。

### ブランチ戦略

**現在のブランチ状況**:
- ソースブランチ: `upgrade-to-NET10`
- アップグレードブランチ: `upgrade-to-NET10-1` (既に作成済み)
- メインブランチ: `main` または `master`

**ブランチングモデル**:
```
main/master
  └─ upgrade-to-NET10-1 (feature branch)
      ├─ Commit 1: フェーズ 1 - フレームワークとパッケージアップグレード
      ├─ Commit 2: フェーズ 2 - Azure Vision 移行
      └─ Commit 3: フェーズ 3 - テスト修正と最終検証
```

**ブランチ保護**:
- `upgrade-to-NET10-1` ブランチでの作業中は、他のブランチにプッシュしない
- 各フェーズ完了後にコミットを作成
- マージ前に必ず完全なテストを実行

### コミット戦略

#### All-At-Once に最適化されたコミット戦略

**推奨: フェーズベースのコミット**

各フェーズの完了時に単一のコミットを作成します。これにより、変更の意味単位が明確になり、ロールバックが容易になります。

**コミットパターン**:

```powershell
# フェーズ 1 完了後
git add .
git commit -m "feat: .NET 10 へのフレームワークアップグレード

- すべてのプロジェクトを net10.0 に更新
- Microsoft.Extensions.* パッケージを 10.0.2 に更新
- Newtonsoft.Json を 13.0.4 に更新
- System.Text.RegularExpressions を削除（フレームワーク統合済み）
- Animation API を新 API に更新
- TextColor プロパティを Brush 型に対応
- ConfigurationBinder.Get<T> を GetRequiredSection().Get<T> に更新
- その他の MAUI API 非互換を修正

ビルド: ? エラー 0
テスト: ? すべて成功 (ComputerVision 除く)"

# フェーズ 2 完了後
git add .
git commit -m "feat: Azure AI Vision SDK を v4.0 に移行

- Microsoft.Azure.CognitiveServices.Vision.ComputerVision v7.0.1 を削除
- Azure.AI.Vision.ImageAnalysis v1.0.0-beta.3 を追加
- ComputerVisionService を新 SDK に移行
  - ComputerVisionClient → ImageAnalysisClient
  - ApiKeyServiceClientCredentials → AzureKeyCredential
  - ReadInStreamAsync + ポーリング → AnalyzeAsync (単一呼び出し)
- ComputerVisionServiceTests を新 SDK に対応
- OCR 機能の統合テスト成功

ビルド: ? エラー 0
テスト: ? すべて成功"

# フェーズ 3 完了後（必要に応じて）
git add .
git commit -m "test: テスト修正と最終検証

- 失敗したテストを修正
- 統合テストを実行して検証
- パフォーマンステストを実施

ビルド: ? エラー 0
テスト: ? すべて成功
機能: ? すべて正常"
```

**代替: 単一の統合コミット** (小規模変更の場合)

すべての変更を単一のコミットにまとめることも可能です：

```powershell
git add .
git commit -m "feat: .NET 8 から .NET 10 へのアップグレード + Azure Vision 移行

フレームワークアップグレード:
- すべてのプロジェクトを net10.0 に更新
- パッケージを最新版に更新
- API 非互換を修正

Azure Vision 移行:
- 旧 SDK から新 SDK に完全移行
- OCR 機能を新 API に対応

ビルド: ? エラー 0
テスト: ? すべて成功
機能: ? すべて正常"
```

**コミットメッセージガイドライン**:
- 接頭辞: `feat:` (機能追加), `fix:` (バグ修正), `test:` (テスト), `docs:` (ドキュメント)
- 日本語/英語のいずれかで一貫性を保つ
- ビルドとテストの状態を明記
- 重要な変更は箇条書きで列挙

### レビューとマージプロセス

#### プルリクエスト (PR) の作成

**PR タイトル**:
```
.NET 8 から .NET 10 へのアップグレード + Azure AI Vision SDK 移行
```

**PR 説明テンプレート**:

```markdown
## 概要
CheckListMaker ソリューションを .NET 8 から .NET 10 (LTS) にアップグレードし、併せて Azure AI Vision SDK を最新版に移行しました。

## 変更内容

### フレームワークアップグレード
- [x] すべてのプロジェクトを `net10.0` に更新
- [x] Microsoft.Extensions.* パッケージを 10.0.2 に更新
- [x] Newtonsoft.Json を 13.0.4 に更新
- [x] System.Text.RegularExpressions を削除

### API 互換性修正
- [x] Animation API を新 API に更新 (180 件)
- [x] TextColor プロパティを Brush 型に対応 (208 件)
- [x] ConfigurationBinder.Get<T> を更新 (3 件)
- [x] その他の MAUI API 非互換を修正 (576 件)

### Azure Vision 移行
- [x] 旧 SDK (`Microsoft.Azure.CognitiveServices.Vision.ComputerVision`) を削除
- [x] 新 SDK (`Azure.AI.Vision.ImageAnalysis`) を追加
- [x] ComputerVisionService をリファクタリング
- [x] ユニットテストを更新

## テスト結果
- ? ビルド: エラー 0、警告 0 (または許容範囲内)
- ? ユニットテスト: すべて成功
- ? 統合テスト: すべての主要機能が正常に動作
- ? OCR 機能: サンプル画像で検証済み

## パフォーマンス
- 起動時間: .NET 8 と同等
- OCR 処理時間: 旧 SDK と同等またはそれ以下
- メモリ使用量: .NET 8 と同等

## 破壊的変更
なし。既存の機能はすべて維持されています。

## レビューポイント
- [ ] すべてのプロジェクトが正しく net10.0 をターゲットに設定
- [ ] パッケージバージョンが適切に更新されている
- [ ] ComputerVisionService の新実装が正しい
- [ ] すべてのテストが成功している
- [ ] コードの品質とスタイルが維持されている

## 関連ドキュメント
- Assessment: `.github/upgrades/assessment.md`
- Plan: `.github/upgrades/plan.md`

## チェックリスト
- [x] ビルドが成功
- [x] すべてのテストが成功
- [x] 手動でアプリを起動して動作確認済み
- [x] OCR 機能をテスト済み
- [x] ドキュメントを更新済み
```

#### レビューチェックリスト

**コードレビュー観点**:
- [ ] プロジェクトファイルの TargetFramework が正しい
- [ ] パッケージ参照が適切に更新されている
- [ ] API 非互換の修正が適切
- [ ] ComputerVisionService の新実装が正しい
- [ ] エラーハンドリングが適切
- [ ] コードスタイルが一貫している
- [ ] コメントとドキュメンテーションが適切

**テストレビュー観点**:
- [ ] すべてのユニットテストが成功
- [ ] テストカバレッジが維持されている
- [ ] 統合テストが実施されている
- [ ] エッジケースがテストされている

**品質観点**:
- [ ] ビルド警告がない（または許容範囲内）
- [ ] スタイル違反がない
- [ ] パフォーマンスが維持されている
- [ ] セキュリティ脆弱性が導入されていない

#### マージ基準

以下のすべてが満たされた場合にマージを実行：
- ? すべてのビルドが成功
- ? すべてのテストが成功
- ? コードレビューが承認済み
- ? CI/CD パイプラインが成功（存在する場合）
- ? 統合テストが実施され、成功している
- ? ドキュメントが更新されている

**マージ方法**:
```powershell
# upgrade-to-NET10-1 ブランチから main/master へマージ
git checkout main
git merge --no-ff upgrade-to-NET10-1 -m "Merge: .NET 10 アップグレード + Azure Vision 移行"
git push origin main
```

**`--no-ff` の使用理由**: マージコミットを明示的に作成し、履歴を保持します。

#### マージ後のクリーンアップ

```powershell
# ローカルブランチの削除
git branch -d upgrade-to-NET10-1

# リモートブランチの削除（該当する場合）
git push origin --delete upgrade-to-NET10-1
```

### ロールバック戦略

#### シナリオ 1: マージ前のロールバック

**状況**: まだ main/master にマージしていない

**手順**:
```powershell
# ブランチをリセット
git checkout upgrade-to-NET10-1
git reset --hard HEAD~N  # N = ロールバックするコミット数

# または、特定のコミットに戻る
git reset --hard <commit-hash>

# 強制プッシュ（リモートブランチがある場合）
git push -f origin upgrade-to-NET10-1
```

#### シナリオ 2: マージ後のロールバック

**状況**: 既に main/master にマージ済み

**手順**:
```powershell
# マージコミットをリバート
git checkout main
git revert -m 1 <merge-commit-hash>
git push origin main
```

**または、ハードリセット**（慎重に使用）:
```powershell
git reset --hard <pre-merge-commit-hash>
git push -f origin main  # 危険: 他の開発者に影響
```

#### シナリオ 3: 部分的なロールバック

**状況**: Azure Vision 移行のみをロールバックしたい

**手順**:
```powershell
# 特定のコミットをリバート
git revert <azure-vision-commit-hash>
git push origin main
```

---

**重要な注意事項**:
- 個人開発プロジェクトのため、ブランチ戦略は柔軟に調整可能
- 重要な変更の前には必ずバックアップを作成
- リモートリポジトリへのプッシュ前に、ローカルで十分にテスト

## 成功基準

移行が完全に成功したと見なされるための明確な基準を定義します。

### 技術的基準

#### すべてのプロジェクトが移行完了

- [x] **CheckListMaker.csproj**
  - TargetFrameworks: `net10.0;net10.0-android`
  - すべてのパッケージが更新済み
  - ビルドエラー: 0
  - ビルド警告: 許容範囲内

- [x] **CheckListMakerTest.Tests.csproj**
  - TargetFramework: `net10.0`
  - すべてのパッケージが互換性確認済み
  - ビルドエラー: 0
  - ビルド警告: 許容範囲内 (xunit 非推奨警告は許容)

#### パッケージが更新完了

- [x] **Microsoft.Extensions.Configuration**: 8.0.0 → 10.0.2
- [x] **Microsoft.Extensions.Configuration.Binder**: 8.0.2 → 10.0.2
- [x] **Microsoft.Extensions.Configuration.FileExtensions**: 8.0.1 → 10.0.2
- [x] **Microsoft.Extensions.Configuration.Json**: 8.0.1 → 10.0.2
- [x] **Microsoft.Extensions.Configuration.UserSecrets**: 8.0.1 → 10.0.2
- [x] **Microsoft.Extensions.Logging.Abstractions**: 8.0.3 → 10.0.2
- [x] **Microsoft.Extensions.Logging.Debug**: 8.0.1 → 10.0.2
- [x] **Newtonsoft.Json**: 13.0.3 → 13.0.4
- [x] **System.Text.RegularExpressions**: 削除（フレームワーク統合済み）

#### Azure AI Vision 移行完了

- [x] **旧 SDK 削除**: `Microsoft.Azure.CognitiveServices.Vision.ComputerVision` v7.0.1
- [x] **新 SDK 追加**: `Azure.AI.Vision.ImageAnalysis` v1.0.0-beta.3+
- [x] **ComputerVisionService リファクタリング**: 新 SDK の API に完全対応
- [x] **認証更新**: `AzureKeyCredential` を使用
- [x] **OCR 機能検証**: サンプル画像でテスト成功

#### ビルドとテストが成功

- [x] **ソリューションビルド**: `dotnet build CheckListMaker.sln` がエラー 0 で完了
- [x] **ユニットテスト**: `dotnet test` がすべて成功
- [x] **テストカバレッジ**: 既存のカバレッジレベルを維持または向上
- [x] **依存関係の競合なし**: `dotnet restore` が警告なしで完了

#### セキュリティ脆弱性の解消

- [x] **パッケージ脆弱性**: なし（アセスメント時点で脆弱性なし）
- [x] **新規脆弱性**: アップグレードによる新規脆弱性の導入なし
- [x] **セキュリティ監査**: `dotnet list package --vulnerable` で問題なし

### 品質基準

#### コード品質が維持されている

- [x] **StyleCop 準拠**: StyleCop.Analyzers の警告が増加していない
- [x] **コードスタイル**: 既存のコーディング規約に準拠
- [x] **命名規則**: 既存の命名規則を維持
- [x] **コメントとドキュメント**: 適切にコメントが更新されている

#### テストカバレッジが維持されている

- [x] **ユニットテストカバレッジ**: 既存のカバレッジレベルを維持
- [x] **統合テスト**: すべての主要機能がテスト済み
- [x] **エッジケース**: 既存のエッジケーステストが成功
- [x] **新規テスト**: Azure Vision 移行に対する新規テストが追加されている

#### ドキュメントが更新されている

- [x] **README**: .NET 10 への言及が更新されている（該当する場合）
- [x] **コメント**: コード内のコメントが最新の API に対応
- [x] **設定ファイル**: appsettings.json の説明が更新されている（必要に応じて）
- [x] **Migration Guide**: このplan.md が完全で正確

### プロセス基準

#### All-At-Once 戦略が適切に実行された

- [x] **すべてのプロジェクトが同時に更新**: 中間状態なし
- [x] **依存関係の順序が尊重された**: MSBuild が正しくビルド
- [x] **単一の統合操作**: フェーズごとに明確なコミット

#### ソース管理戦略が遵守された

- [x] **ブランチ戦略**: `upgrade-to-NET10-1` ブランチで作業
- [x] **コミット戦略**: フェーズごとに意味のあるコミット
- [x] **コミットメッセージ**: 明確で詳細なメッセージ
- [x] **マージ準備**: PR が適切に作成され、レビュー済み

#### All-At-Once 原則が適用された

- [x] **最大限の統合**: フレームワークとパッケージ更新が単一バッチで実行
- [x] **一貫性**: すべてのプロジェクトが同じ TFM をターゲットに設定
- [x] **効率性**: 不要な中間ステップがない
- [x] **明確な成果物**: 各フェーズ後にビルド成功とテスト成功を確認

### 機能的基準

#### すべての機能が正常に動作

**コア機能**:
- [x] アプリケーション起動
- [x] チェックリスト作成
- [x] チェックリスト編集
- [x] チェックリスト削除
- [x] データ永続化（LiteDB）
- [x] OCR 機能（Azure Vision）
- [x] 設定読み込み
- [x] AdMob 広告表示（該当する場合）

**非機能要件**:
- [x] パフォーマンスが .NET 8 と同等またはそれ以上
- [x] メモリ使用量が許容範囲内
- [x] UI 応答性が維持されている
- [x] エラーハンドリングが適切

#### プラットフォーム互換性

- [x] **Android**: API 21+ で動作（エミュレータおよび実機）
- [ ] **iOS**: iOS 14+ で動作（該当する場合）
- [ ] **Windows**: Windows 10/11 で動作（該当する場合）
- [ ] **macOS**: macOS 11+ で動作（該当する場合）

### 検証済み基準

#### 自動検証

```powershell
# ビルド検証
dotnet clean CheckListMaker.sln
dotnet build CheckListMaker.sln --no-incremental
# 期待: エラー 0

# テスト検証
dotnet test CheckListMaker.sln --logger "console;verbosity=detailed"
# 期待: すべてのテストが成功

# 脆弱性チェック
dotnet list package --vulnerable
# 期待: 脆弱性なし
```

#### 手動検証

- [x] Android エミュレータでアプリを起動し、すべての主要機能をテスト
- [x] OCR 機能を複数のサンプル画像でテスト
- [x] エッジケース（ネットワークエラーなど）をテスト
- [x] パフォーマンスを測定し、.NET 8 と比較
- [x] UI を視覚的に確認し、レイアウト崩れがないことを確認

### 完了宣言の条件

以下のすべてが満たされた場合、移行は**完了**と見なされます：

1. ? **技術的基準**: すべてのプロジェクトが .NET 10 に移行し、ビルドとテストが成功
2. ? **品質基準**: コード品質、テストカバレッジ、ドキュメントが維持されている
3. ? **プロセス基準**: All-At-Once 戦略とソース管理戦略が遵守されている
4. ? **機能的基準**: すべての機能が正常に動作し、パフォーマンスが維持されている

### 部分的成功の定義

以下の場合、部分的成功と見なされます（追加作業が必要）：

- ?? **フレームワークアップグレードは成功したが、Azure Vision 移行が未完了**
  - 対処: フェーズ 2 を再試行または代替案を検討
  
- ?? **ビルドは成功したが、一部のテストが失敗**
  - 対処: 失敗したテストを分析し、修正
  
- ?? **コア機能は動作するが、非機能要件が満たされていない**
  - 対処: パフォーマンス最適化を実施

### 失敗の定義

以下の場合、移行は**失敗**と見なされ、ロールバックを検討：

- ?? **ビルドが成功しない**: 致命的なエラーが解決できない
- ?? **コア機能が動作しない**: アプリが起動しない、クラッシュする
- ?? **データ損失**: 既存のデータが破損または消失
- ?? **セキュリティ問題**: 新しい脆弱性が導入された

---

**最終確認チェックリスト**:

- [ ] すべての技術的基準が満たされている
- [ ] すべての品質基準が満たされている
- [ ] すべてのプロセス基準が満たされている
- [ ] すべての機能的基準が満たされている
- [ ] ドキュメントが完全で正確
- [ ] PR がレビュー済みでマージ準備完了
- [ ] ロールバック手順が文書化されている
- [ ] チームメンバー（該当する場合）に通知済み

**成功の最終宣言**:

? **CheckListMaker の .NET 8 から .NET 10 へのアップグレードと Azure AI Vision SDK の移行が正常に完了しました。**

- すべてのプロジェクトが .NET 10 をターゲットに設定
- すべてのパッケージが最新版に更新
- Azure AI Vision SDK が v4.0 に移行
- ビルド: エラー 0、警告最小限
- テスト: すべて成功
- 機能: すべて正常に動作
- パフォーマンス: .NET 8 と同等またはそれ以上

この移行により、CheckListMaker は最新の .NET 10 LTS の恩恵を受け、2028年以降もサポートされる Azure AI Vision SDK を使用します。
