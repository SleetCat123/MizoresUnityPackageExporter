# MizoresPackageExporter
![Image](Image1b.png)  
予め指定したファイル／フォルダをunitypackageとしてまとめて出力できるスクリプト（ScriptableObject）です。  
エクスポートの手間が減るほか、

* 必要なデータを入れ忘れる  
* 誤ったデータを混入させてしまう  

といったミスが起こる可能性を軽減してくれるかもしれません。
  
Package Managerで導入するときは以下のURLを使用してください。  
`https://github.com/SleetCat123/MizoresUnityPackageExporter.git?path=Assets/Mizore_Nekoyanagi/Util/MizoresPackageExporter`  
（2022-05-14：URLが変わりました）

## ◆使い方
（[Releases](https://github.com/SleetCat123/MizoresUnityPackageExporter/releases)からスクリプトのunitypackageをダウンロードしてインポート）  
1. Projectウィンドウ上で右クリック→Creare→MizoreNekoyanagi→UnityPackageExporter　でMizoresPackageExporterを作成  
2. 作成したMizoresPackageExporterの名前を変更（この名前がunitypackageの名前になります）  
3. インスペクタ上でエクスポート対象を指定  
4. Export to unitypackageボタンを押してエクスポート  
（MizoresPackageExporterを複数選択することでまとめて編集したりエクスポートしたりできます）  

## ◆インスペクタ
### ◇Objects
エクスポート対象となるAssetの一覧です。  
パスを直接指定して参照することもできます。  
（存在しないパスを指定することはできません）


### ◇Dynamic Path
エクスポート対象となるファイル／フォルダのパス一覧です。  
（存在しないパスを指定することもできますが、エクスポート時にエラーが出ます）

このパスの文字列中の %name% はMizoresPackageExporterのファイル名で置換されます。

例えば、**A　B　C**という名前の３つのMizoresPackageExporterのDynamic Pathに  
`Assets/Mizore_Nekoyanagi/%name%`  
というパスを書いた場合、それぞれ  
`Assets/Mizore_Nekoyanagi/A`
`Assets/Mizore_Nekoyanagi/B`
`Assets/Mizore_Nekoyanagi/C`  
をunitypackageにエクスポートします。

### ◇Dynamic Path Variables
左側の欄の文字列を%%で囲んでDynamic Pathに書くと、右側の欄に書かれている文字列に置き換えられます。  
（複数オブジェクト選択中の編集には未対応）  

例：左側の欄が`neko`、右側の欄が`ねこ`なら、Dynamic Path中の`%neko%`が`ねこ`という文字列に置換されます。


### ◇Version File
指定したテキストファイルにバージョンを記述することでunitypackage名にバージョンを付加できます。
（こちらからマージしました。 https://github.com/SleetCat123/MizoresUnityPackageExporter/pull/1 ）

### ◇Check
上記の設定項目で指定したファイル／フォルダが存在するかどうかを確認します。  

### ◇Export to unitypackage
上記の設定項目で指定したファイル／フォルダをプロジェクトフォルダ直下の MizorePackageExporter/ にunitypackageとして出力します。  
unitypackageの名前はMizoresPackageExporterのファイル名が使用されます。  
存在しないデータがある場合、処理を中断します。

### ◇Open
出力先フォルダを開きます。  
（プロジェクトフォルダ直下の MizorePackageExporter/）
