# 配布物構成標準 (Distribution Standard)

本ドキュメントでは、アバター配布物（`Products/` 配下）のディレクトリ構造およびファイル構成の標準を定義します。

## 1. ディレクトリ構造

各アバターごとに以下の構造を維持してください。

```text
Products/
  <AvatarName>/
    <AvatarName>.zip      - 下記の `<AvatarName>/` フォルダをそのまま圧縮したもの
    <AvatarName>/         - 配布物の内容（展開時の状態）
      README.txt          - 利用規約、導入方法、連絡先など
      <AvatarName>.unitypackage - Unityインポート用ファイル
      src/                - 改造用ソースデータ
        <AvatarName>.blend - Blenderソースファイル（ボーン、ウェイト設定済み）
        Textures/         - 編集用テクスチャデータ（PSD, CLIP, 等）
        Original/         - 元データ形式（FBX, PNG, 等、改変のベースとなるファイル）
    booth/                - BOOTH管理用
      1.png, 2.png...    - サムネイル画像（BOOTHの掲載順に命名）
      Assets/             - サムネイル作成に使用したプロジェクトファイルや素材など
```

## 2. ファイル作成ルール

### 2.1 README.txt
少なくとも以下の内容を含めてください。
- アバター名
- バージョンおよび更新履歴
- 利用規約（または規約ページへのリンク）
- 内容物一覧
- 連絡先

### 2.2 src ディレクトリ
- 改造を行うユーザー向けのソースファイルを格納します。
- `src/` という名称は、Unityパッケージ（`.unitypackage`）の元となる「改造用ソースデータ」を指します。

### 2.3 booth ディレクトリ
- 画像ファイルは `1.png`, `2.png` のように、BOOTH上で表示したい順番で命名してください。
- `Assets/` には、将来的な修正のために使用したツールや素材を格納してください。

## 3. パッケージ化
- `Products/<AvatarName>/<AvatarName>/` フォルダ自体を zip 圧縮し、`<AvatarName>.zip` として `Products/<AvatarName>/` 直下に配置してください。
- これにより、ユーザーが展開した際にアバター名のフォルダが一つだけ作成され、その中に `README.txt`, `.unitypackage`, `src/` が並んでいる理想的な形になります。
