# ChangeLog
## [8.0.0] (
- add: References検索対象から除外するファイルを指定できるようにした
- add: ObjectとDynamicPathで指定されたAssetがReferencesを検索するかどうかを個別に指定できるようにした
- add: DynamicPathで指定されたパスにAssetが存在するならObjectFieldで表示するようにした
- add: Objectsなどの項目にもフォルダ／ファイルの選択ボタンを追加
- change: フォルダ／ファイルの選択ボタンの表示を変更
- change: PostProcessScriptの選択をPopupにした
- fix: MizoresPackageExporterを新規作成した直後にエラーが出てしまうのを修正

## [7.2.0] (2024-01-16)
- add: （上級者向け機能）エクスポート後に所定のinterfaceを継承するスクリプトを実行できる機能を追加
- add: 複数オブジェクト編集に対応していない項目にHelpBoxを表示するようにした
- add: DynamicPathで%..name%のように書くと、現在のパスから.の個数だけ上の階層にあるフォルダの名前を取得するようにした
- change: DynamicPathのプレビューで%batch%が表示されるようにした
- change: BatchExport設定がFoldersでフォルダが未指定のとき、Exporterがあるディレクトリを使用するようにした
- fix: ファイルをDynamicPathにドラッグ＆ドロップして追加した時に早退パスへの変換が行われていなかったのを修正

## [7.1.1] (2023-12-22)
- fix: ビルド時にエラーが出てしまうのを修正
- fix: スクリプトの文字コードが一部Shift-JISになっていたのを修正

## [7.1.0] (2023-12-22)
- add: DynamicPathを相対パスに対応。
Dynamic Export Pathのタイトル部分か、各項目の数字部分を右クリックすることで相対パス／絶対パスを相互に変換できます。
- change: Folder／Fileボタンでの選択時、フォルダ／ファイルが存在しない場合はPackageExporterがあるフォルダを表示するようにした

## [7.0.1] (2023-12-08)
- change: Package Manager対応に伴いフォルダ構成を変更

## [v7] (2023-11-20
- fix: エクスポートログに改行が含まれていると表示が崩れてしまっていたのを修正  
- fix:  Excludesにフォルダを指定した場合に機能していなかったのを修正  
- change: エクスポート対象ファイルの表示方法をツリー表示に変更
- change: エクスポートが中断されたログにファイルパスを書くようにした
- add: 複数のunitypackageを一括で出力できる機能（Batch Export）を追加
- add: 一部設定を右クリックしてコピー＆ペーストできるようにした
- add: UIのテキストをcsvから読み込むようにした
- add: UIのテキストの言語切替を実装
- add: %date:yyyyMMdd%のように書くと現在の日付をformatして表示できるようにした
  
◆内部処理
- change: SingleとMultipleエディターのソースファイルを共通化
- change: その他ソースコード整理
- add: Exporterに本体のバージョン情報を記憶し、異なるバージョンで作成されたオブジェクトを検知・データ移行できるようにした