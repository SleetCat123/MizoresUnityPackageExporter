# ChangeLog
## [v7.0.1] (2023-12-08)
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