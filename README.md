# MizoresPackageExporter
![Photo20220721215253](https://user-images.githubusercontent.com/37854026/180218003-c003bf99-c605-4ce0-a0ea-fd16de4fdfab.png)


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
***

### ◇Dynamic Path
エクスポート対象となるファイル／フォルダのパス一覧です。  
（存在しないパスを指定することもできますが、エクスポート時にエラーが出ます）

このパスの文字列中にある%で囲まれた文字列は下記DynamicPathVariablesで定義された文字列に置き換えられます。  
（例：`%name%`はMizoresPackageExporterのファイル名で置換されます）

### ★Dynamic Path設定の活用例
例えば、**A　B　C**という名前の３つのMizoresPackageExporterのDynamic Pathに  
`Assets/Mizore_Nekoyanagi/%name%`  
というパスを書いた場合、それぞれ  
`Assets/Mizore_Nekoyanagi/A`
`Assets/Mizore_Nekoyanagi/B`
`Assets/Mizore_Nekoyanagi/C`  
をunitypackageにエクスポートします。
***

### ◇Dynamic Path Preview
Dynamic Pathの文字列を実際にエクスポートされるファイルパスに変換して表示します。
***

### ◇Dynamic Path Variables
左側の欄の文字列を%%で囲んでDynamic Pathに書くと、右側の欄に書かれている文字列に置き換えられます。  
（複数オブジェクト選択中の編集には未対応）  

例：左側の欄が`neko`、右側の欄が`ねこ`なら、Dynamic Path中の`%neko%`が`ねこ`という文字列に置換されます。
***

### ◇References
この項目に指定されているファイル／フォルダがエクスポート対象の他Assetに参照されている場合、該当するファイルは自動的にエクスポート対象に追加されます。
***

### ◇Exclude Objects
この項目に指定されているファイル／フォルダはエクスポートから除外されます。
***

### ◇Excludes
入力した条件に一致するファイルパス／フォルダパスをエクスポートから除外します。  
%で囲まれた文字列はDynamicPathVariablesの文字列に置き換えられます。　　
右のボックスで除外パスの検索方法を変更できます。

- Disable：除外処理を行いません。
- Exact：文字列と完全に一致するファイルパス／フォルダパスをエクスポートから除外します。
- Partial：文字列と部分的に一致するファイルパス／フォルダパスをエクスポートから除外します。
- Partial_Ignore Case：アルファベットの大文字と小文字を区別せず、文字列と部分的に一致するファイルパス／フォルダパスをエクスポートから除外します。
- Regex：正規表現にマッチしたファイルパス／フォルダパスをエクスポートから除外します。
- Regex_Ignore Case：アルファベットの大文字と小文字を区別せず、正規表現にマッチしたファイルパス／フォルダパスをエクスポートから除外します。
***

### ◇Excludes Preview
Excludesの文字列を除外パスの検索で実際に使用される文字列に変換して表示します。
***

### ◇Version File
指定したテキストファイルにバージョンを記述することでunitypackage名にバージョンを付加できます。 
jsonファイルを指定した場合、"version"の文字列をバージョンとして使用します。  
Format欄を編集することでバージョンの表記方法を変更できます。
Format欄にある%で囲まれた文字列はDynamicPathVariablesの文字列に置き換えられます。
***
### ◇Package Name
エクスポートされるunitypackageの名前を指定できます。  
%で囲まれた文字列はDynamicPathVariablesの文字列に置き換えられます。
***

### ◇Batch Export（一括エクスポート）
v7で追加されました。  
指定したリストの文字列を順番に参照し、エクスポート処理を連続で行います。  
文字列は %batch% や %batchf% にセットされ、Dynamic PathやPackage Name内で使用できます。  

この機能を使用することで、複数のバリエーションがあるデータを一括でunitypackage化することができます。  

Batch Exportの有効時にはPackage Nameに %batch% または %batchf% を含める必要があります。

- Disable：Batch Exportを無効化します。
- Texts：リストの内容を文字列で指定します。
- Folder：指定したフォルダ内にあるファイル／フォルダの一覧をリストとして扱います。  
対象となるファイル／フォルダの名前は正規表現でフィルタリング可能です。
- File：指定したテキストファイルの内容をリストとして扱います。


### ★Folder設定の活用例
- Assets/Mizore_Nekoyanagi/Materials
- Assets/Mizore_Nekoyanagi/Red
- Assets/Mizore_Nekoyanagi/Green
- Assets/Mizore_Nekoyanagi/Blue

上記のような構成のフォルダを、以下のような内容のunitypackageとして出力したい場合を考えてみます。  
- Red.unitypackage
  - Materials
  - Red
- Green.unitypackage
  - Materials
  - Green
- Blue.unitypackage
  - Materials
  - Blue

`Materials`は全てのunitypackageに含まれて欲しく、それ以外の`Red` `Green` `Blue`は別々のunitypackageとして出力したいという状況です。

この例の場合、以下の設定にすることで上記のようなエクスポートを実現できます。
- Objects to export
  - `Assets/Mizore_Nekoyanagi/Materials`
- Dynamic Export Path
  - `%batch%`
- エクスポート設定
  - モード:`Folders`
  - フォルダ: `Assets/Mizore_Nekoyanagi`
  - 対象: `All`
  - 正規表現: `^(?!Materials).+$`
  - unitypackage名: `%batch%`

正規表現でMaterialsを除外することで、Batch Listに`Materials`が入ることを防いでいます。  
このとき、Batch Listは以下のようになっています。
- Red
- Green
- Blue

Dynamic Export Pathとunitypackage名に`%batch%`を指定し、Batch Listの文字列が順番に入るようにしています。  
この状態でエクスポートを行うことで、`Red.unitypackage` `Green.unitypackage` `Blue.unitypackage`という3つのunitypackageがエクスポートされます。  

（上記の設定は一例です。使用する際は実際のフォルダ構成に合わせて設定を行ってください）  
  
  
なおエクスポート一覧には、エクスポート設定の「フォルダ」内にある最新のファイル名／フォルダ名が自動的に反映されます。  
例えば、新たに`Purple` `Orange`という名前のファイルを`Assets/Mizore_Nekoyanagi/`に追加した場合、上記のような設定を行っていれば、追加設定を行うことなく`Purple.unitypackage` `Orange.unitypackage`がエクスポートされるようになります。

### ◇Override Settings
v7で追加されました。  
Batch Exportのリストの要素の設定を上書きできます。  
unitypackageごとにバージョン情報を個別で管理したり、出力ファイルの命名を変えるといったことが可能です。  
***

### ◇Export to unitypackage
上記の設定項目で指定したファイル／フォルダをプロジェクトフォルダ直下の MizorePackageExporter/ にunitypackageとして出力します。  
unitypackageの名前はMizoresPackageExporterのファイル名が使用されます。  
存在しないデータがある場合、処理を中断します。
***

### ◇Open
出力先フォルダを開きます。  
（プロジェクトフォルダ直下の MizorePackageExporter/）
