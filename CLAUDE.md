常に日本語で回答してください。
## プロジェクト概要

Unity Package Exporter - Unity用のパッケージエクスポートツール。事前に指定したファイル/フォルダを`.unitypackage`として一括エクスポートできるScriptableObject。

## 開発環境

- **Unity バージョン**: 2019.4.31f1
- **ターゲット**: .NET Framework 4.7.1
- **C#言語バージョン**: latest
- **パッケージ形式**: Unity Package Manager対応 (version 8.0.0)

## 主要なコマンド

### ビルドコマンド
```bash
# C#プロジェクトのビルド
msbuild MizoresPackageExporter.csproj /p:Configuration=Release
```

### Unity固有のワークフロー
- Unity エディタ内でのパッケージエクスポート: `AssetDatabase.ExportPackage()`
- バッチエクスポート機能による一括処理
- エディタ拡張機能の開発とテスト

## アーキテクチャ概要

### ディレクトリ構造
```
Packages/com.mizore-nekoyanagi.utils.mizore-package-exporter/
├── PackageExporter/        # コア機能（MizoresPackageExporter.cs）
├── EditorScript/          # エディタ拡張（MizoresPackageExporterEditor.cs）
├── Enums/                 # 列挙型定義
├── PostProcess/           # エクスポート後処理プラグイン
├── Texts/                 # 多言語対応テキスト
└── Utils/                 # ユーティリティ
```

### 重要なクラス
- **MizoresPackageExporter** (ScriptableObject) - メインのエクスポート設定クラス
- **MizoresPackageExporterEditor** (CustomEditor) - カスタムエディタ
- **ExportPostProcess** (abstract) - ポストプロセス基底クラス
- **GUI_***クラス群 - 各機能のGUIコンポーネント

### 主要パターン
1. **コンポーネント化されたGUI**: 各機能のGUIは独立したクラスとして実装
2. **動的パス変換**: `%name%`、`%version%`などの変数を実行時に置換
3. **バッチエクスポート**: 複数パッケージの一括エクスポート機能
4. **非同期処理**: エクスポート処理は`Task`を使用した非同期実行

## 新機能追加時の指針

### GUI要素の追加
1. `EditorScript/EditorOnly/MizoresPackageExporterGUI/`に`GUI_XXX.cs`を作成
2. `GUI_XXX.Draw()`メソッドを実装
3. `MizoresPackageExporterEditor`から呼び出し

### 新しい設定項目
1. `MizoresPackageExporter`クラスにフィールド追加
2. 必要に応じて`OnBeforeSerialize`/`OnAfterDeserialize`を更新
3. バージョン互換性のため`CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION`を更新

### 動的変数の追加
1. `ConvertDynamicPath`メソッドに新しい置換処理を追加
2. `ExporterConsts_Keys`に定数を定義

## コーディング規約

- 名前空間: `MizoreNekoyanagi.PublishUtil.PackageExporter`
- エディタ専用コードは`#if UNITY_EDITOR`で囲む
- 日本語コメントを使用（プロジェクトの慣習）
- ファイルパスは常にスラッシュ（`/`）に統一
- 非同期処理では`Task.Delay`で適切に処理を分割

## 拡張性機能

### PostProcessScript
エクスポート後の追加処理をプラグイン形式で実装可能:
```csharp
public abstract class ExportPostProcess {
    public abstract void OnExported(MizoresPackageExporter exporter, 
        string exportPath, FilePathList list, ExporterEditorLogs logs);
}
```

### 多言語対応
- `ExporterTexts`クラスでテキストを一元管理
- `jp.csv`、`en.csv`から言語別テキストを読み込み

## 注意点

- Unity API呼び出しはメインスレッドで実行
- エラーハンドリングは`ExporterEditorLogs`に記録
- `ISerializationCallbackReceiver`を実装しているクラスでは、Dictionary等のシリアライズに注意
- バージョン管理機能（`packageExporterVersion`）でデータ移行に対応

## 主要なワークフロー

1. **設定作成**: ScriptableObjectとして`MizoresPackageExporter`を作成
2. **エクスポート対象設定**: ファイル/フォルダを指定、除外条件を設定
3. **バッチエクスポート設定**: 複数パッケージの一括処理設定
4. **エクスポート実行**: `AssetDatabase.ExportPackage()`による`.unitypackage`生成
5. **ポストプロセス**: 必要に応じて追加処理を実行