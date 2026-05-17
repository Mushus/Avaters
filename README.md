C:\Users\wyndf\Documents\unity\Avaters に、各ブランチの内容を統合しました。既存の Unity プロジェクトは削除せず、Assets と Products を追加する形です。

やったこと:

unity/Assets/* → Avaters\Assets\*
booth/* / src/* → Avaters\Products\<名前>\booth|src
既存ファイルはハッシュ比較
同一内容はスキップ
差分ありの同名ファイルは上書きせず _conflicts に退避
LFS ポインタ文字列の残りは検出なし
元の vrc-avaters repo はクリーンなまま
結果:

コピー: 1967
同一内容スキップ: 729
衝突退避: 20
Assets 内ファイル数: 1571
Products 内ファイル数: 406
主な出力:

統合先: C:\Users\wyndf\Documents\unity\Avaters
レポート: C:\Users\wyndf\Documents\unity\Avaters\_migration_reports\merge-20260514-005320.csv
衝突退避: C:\Users\wyndf\Documents\unity\Avaters\_conflicts\merge-20260514-005320
再実行用スクリプト: C:\Users\wyndf\Documents\unity\Avaters\_migration_reports\merge-branches-to-avaters.ps1
衝突が出たのは jack-o-nyantan2, reptan, whip-lp, wip だけで、内容的にも派生/共有アセットっぽいものです。booth と src はアバター別に分離できています。

## Git LFS / S3 setup

This project stores large binary assets with Git LFS using `lfs-s3`.

### Requirements

- Git LFS: https://git-lfs.com/
- lfs-s3: https://github.com/nicolas-graves/lfs-s3
- AWS credentials with read/write access to the S3 bucket

### Local Git config

```powershell
git lfs install
git config lfs.customtransfer.lfs-s3.path "lfs-s3.exe"
git config lfs.customtransfer.lfs-s3.args "--bucket avaters-863657440723-ap-northeast-1-an --region ap-northeast-1 --endpoint https://s3.ap-northeast-1.amazonaws.com"
git config lfs.standalonetransferagent lfs-s3
```

### S3 bucket

- Bucket: `avaters-863657440723-ap-northeast-1-an`
- Region: `ap-northeast-1`
- Public access: blocked
- Encryption: SSE-S3
- Lifecycle: objects of 128 KB or larger transition to `INTELLIGENT_TIERING` immediately

### 必要なツール (Dependencies)

このプロジェクトでは、アバターのビルドや自動化スクリプトのために以下のツールに暗黙的に依存しています。

#### 1. Unity / VRChat 開発環境
- **Unity**: `2022.3.22f1` (ProjectSettingsに準拠)
- **VRChat Creator Companion (VCC)**: VRChat SDK (Base/Avatars) および VPMパッケージの管理に必要
- **Animator As Code (AAC) V1**: アバターのアニメーション構築自動化ツール (VPMパッケージ)

#### 2. 画像処理・サムネイル自動生成 (Python)
Unityエディタ拡張（`AvatarThumbnailGenerator`等）からPythonスクリプトを呼び出して画像を生成するため必要です。
- **Python 3.x**
- **Pillow (PIL)**: 画像処理ライブラリ
  ```powershell
  pip install Pillow
  ```

#### 3. その他のユーティリティ (Node.js)
  ```powershell
  # Node.js と npm がインストールされていること
  npm install -g @jackwener/opencli
  ```