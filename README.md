# MizoresPackageExporter
予め指定したファイル／フォルダをunitypackageとしてまとめて出力できるようになるスクリプトです。  
エクスポートの手間が減るほか、

* 必要なデータを入れ忘れる  
* 誤ったデータを混入させてしまう  

といったミスが起こる可能性を軽減してくれるかもしれません。
  
Package Managerで導入するときは以下のURLを使用してください。
`https://github.com/SleetCat123/MizoresUnityPackageExporter.git?path=Assets/Mizore_Nekoyanagi/Util`

## ◆使い方
1. Projectウィンドウ上で右クリック→Creare→MizoreNekoyanagi→UnityPackageExporter　でMizoresPackageExporterを作成  
2. MizoresPackageExporterの名前を変更（この名前がunitypackageの名前になります）  
3. インスペクタ上でエクスポート対象を指定  
4. Export to unitypackageボタンを押してエクスポート  
（MizoresPackageExporterを複数選択することでまとめて編集したりエクスポートしたりできます）  

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

### ◇Export to unitypackage
指定したファイル／フォルダをプロジェクトフォルダ直下の MizorePackageExporter/ にunitypackageとして出力します。  
unitypackageの名前はMizoresPackageExporterのファイル名が使用されます。  
存在しないデータがある場合、処理を中断します。

### ◇Open
出力先フォルダを開きます。  
（プロジェクトフォルダ直下の MizorePackageExporter/）
