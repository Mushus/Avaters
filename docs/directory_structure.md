# プロジェクトのディレクトリ構造

本プロジェクトのリポジトリ構成と、各ディレクトリの役割について説明します。

## リポジトリルート

| ディレクトリ/ファイル | 役割 |
| --- | --- |
| `Assets/` | Unityプロジェクトのメインアセット。 |
| `Products/` | Unity外で管理される配布物や編集用ソースファイル（Blender, PSD等）。 |
| `docs/` | プロジェクトの標準規格や構成に関するドキュメント。 |
| `ProjectSettings/` | Unityのプロジェクト設定。 |
| `Packages/` | Unity Package Managerの定義と設定。 |
| `README.md` | プロジェクトの概要と環境構築手順。 |
| `AGENTS.md` | AIアシスタント向けのプロジェクト固有ルール。 |

---

## Assets/Mushus (自作アセット)

自作のアバターおよび関連ツールはすべて `Assets/Mushus` 配下で管理されます。

### 1. アバターパッケージ (`<AvatarName>/`)
配布用のクリーンなアセットを格納します。

- **Animations**: アニメーションクリップ（FX, Gesture, Locomotion等）。
- **Controllers**: Animator Controller。
- **Expressions**: VRChatのExpression MenuおよびParameters。
- **Materials**: マテリアル。
- **Models**: 3Dモデルファイル（FBX）。※旧構成の `Fbx` フォルダは順次こちらへ移行します。
- **Prefabs**: 配布用Prefab。
- **Scripts**: アバター固有のスクリプト。
- **Shaders**: アバター固有のシェーダー。
- **Textures**: テクスチャ。

### 2. 開発用パッケージ (`<AvatarName>Dev/`)
配布パッケージには含めない、アップロード確認やサンプルシーンを格納します。

- **`<AvatarName>Dev/` (開発・サンプル用)**
    - `Scenes`: セットアップ済みのサンプルアバター用シーン。
        - `Sample.unity`: アップロード確認用。
        - `AAC.unity`: AAC作成・編集用。
    - `Animations`: AAC (Animator As Code) の生成スクリプトなどの開発用資産。開発用パッケージには含めないもの。
    - `Prefabs`: 配布用Prefabを元にした、アップロード設定済みのVariant。

### 3. プロジェクト共通資産
- **Editor**: プロジェクト全体で使用するエディタ拡張スクリプト。
- **Scripts**: プロジェクト全体で使用する共通実行時スクリプト（例: `AvatarCameraCapture.cs`）。
- **Icons**: 各アバターやメニューで使用する共通アイコン資産。
- **Capture**: スクリーンショット撮影ツール等に関連する資産。
- **ScenesDev**: 共通のテスト環境や開発用シーン。

---

## Products (配布・ソースファイル)

Unityプロジェクト内には含めないが、管理が必要なファイルをアバターごとに格納します。

- **`<AvatarName>/`**
    - `<AvatarName>.zip`: 配布用パッケージ（下記の `<AvatarName>/` フォルダを圧縮したもの）。
    - **`<AvatarName>/`**: 配布物の内容（展開時の状態）。
    - `README.txt`: 利用規約、導入方法等。
    - `<AvatarName>.unitypackage`: Unityインポート用ファイル。
    - **`src/`**: 改造用ソースデータ（Blender, PSD, 各種書き出しデータ等）。
        - ※ `src/` 内は外部ストレージ（Google Drive等）との同期を想定し、特定の構造を強制しません。
    - **`booth/`**: BOOTH管理用。
        - `1.png`, `2.png`...: サムネイル画像（順番通りに命名）。
        - `Assets/`: サムネイル作成に使用した素材など。

---

## 運用ルール
- アセットの移動や改名は Unity Editor 上で行い、`.meta` ファイルの整合性を維持してください。
- 配布用パッケージ (`<AvatarName>/`) には、開発用のシーンや中間ファイルを含めないでください。
- 詳細は [Avatar Development Standard](avatar-development-standard.md) および [Distribution Standard](distribution-standard.md) を参照してください。
