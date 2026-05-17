# Avatar Development Standard

このリポジトリは、`Assets/Mushus` 以下を自作アバターパッケージ置き場として扱う。復元作業中は、先にこの標準に照らして分類し、ファイル移動や削除は判定後に行う。

## 基本方針

- アバターは Mobile-first で作る。
- 配布用 Prefab は必ず `Assets/Mushus/<Package>/Prefabs/` に置く。
- サンプルアップロード用の Variant とシーンは `Assets/Mushus/<Package>Dev/` に置く。
- Material は基本 `VRChat/Mobile/StandardToon` を使う。
- Texture 品質差は別ファイルを量産せず、Platform Override で管理する。
- 配布は無印版と Mobile 版の 2 系統を基本にし、どちらも同じ配布パッケージに同梱する。
- 新規命名では `Quest` を使わず `Mobile` を使う。既存の `Quest` 名は参照が安定するまで無理に改名しない。
- 新規命名では `LP` より `LowPoly` を使う。
- `LP` / `LowPoly` は軽量版の別プロダクトとして扱う。名前が近くても無印版へ統合しない。
- `.meta` は Unity の参照維持に必須なので、ファイル移動は Unity Editor 上か、`.meta` とセットで行う。

## パッケージ構造

標準の配布パッケージ:

```text
Assets/Mushus/<Package>/
  Animations/
  Controllers/
  Expressions/ (ExMenu, Parameters, 関連Iconsをここに集約)
  Materials/
  Models/
  Prefabs/
  Scenes/ (Blueprint IDをクリアしたクリーンなSampleScene.unityを置く)
  Scripts/
  Shaders/
  Textures/
```

標準の開発・サンプルパッケージ:

```text
  Animations/ (AAC生成スクリプト等)
  Prefabs/
  Scenes/
    AAC.unity (AAC作成・編集用)
```

`<Package>Dev/Prefabs/` には、個人的なアップロード設定を含むVariantなどを置く。配布するPrefabは `<Package>/Prefabs/` に置く。

## 命名

- 新規パッケージ名は PascalCase にする。例: `Windra`, `NozakiNeneko`
- サンプル・アップロード用は `<Package>Dev` にする。古い `<Package>Sample` は復元時に `<Package>Dev` へ寄せる候補にする。
- 古い版は `<Package>V0` のように残してよいが、現行版から参照しない。
- バックアップ復元由来は `<Package>_Backup_<timestamp>` ではなく、内容確認後に `_conflicts` または正規パッケージへ分類する。
- `Avater` など既存の誤字ファイル名は、Prefab 参照が安定するまでは無理に直さない。新規作成分から `Avatar` を使う。

## 配布 Prefab の条件

`Assets/Mushus/<Package>/Prefabs/` に置く Prefab は次を満たす。

- VRChat Avatar Descriptor が設定されている。
- Mobile 向けに破綻しない Material と Texture 設定になっている。
- 無印版と Mobile 版を配布する場合、どちらも同じ `<Package>/Prefabs/` に置く。
- Mobile 版のファイル名は新規作成分から `<Package>Mobile.prefab` のようにする。
- サンプルシーン専用の Light、Camera、床、アップロード補助 Object を含めない。
- 参照先が同じ `<Package>` 配下、または明示的な共通フォルダだけになっている。

## Dev 側の条件

`Assets/Mushus/<Package>Dev/` は個人的なアップロード設定の保存と、AAC等の開発資産の維持場所とする。

- `Scenes/` には開発用シーンを置く（AAC作成用の `AAC.unity` など）。配布用の確認シーンは `<Package>/Scenes/` 側に置く。
- `Animations/` には AAC (Animator As Code) の生成スクリプトなどの開発用資産を置く。配布用フォルダには含めない。
- `Prefabs/` には個人のアップロード用設定等を含んだ Variant を置く。
- Dev 側から配布側へ参照してよい。
- 配布側から Dev 側へ参照してはいけない。

## 共通アセット

共通アセットはむやみに増やさず、次の用途に限る。

- `Assets/Mushus/Icons/`: 複数パッケージで共有する UI アイコン。
- `Assets/Mushus/Scripts/`: 複数パッケージで共有する Editor/生成補助スクリプト。

各アバター固有の Texture、Material、Animation、Expression、Prefab は各 `<Package>` 配下に置く。

## 復元作業の判定ルール

復元中のフォルダは、次の順で判定する。

1. 現行パッケージか
2. Dev/Sample か
3. 古い版として残すか
4. 競合・バックアップとして隔離するか
5. 削除候補か

判定の目安:

| 状態 | 行き先 |
| --- | --- |
| `<Package>/Prefabs/*.prefab` があり、現行販売・配布対象 | `Assets/Mushus/<Package>/` |
| シーンやアップロード Variant が主 | `Assets/Mushus/<Package>Dev/` |
| `<Package>Sample` | `Assets/Mushus/<Package>Dev/` へ移行候補 |
| `<Package>Quest` / `<Package>Mobile` 相当 | 同じ `<Package>` 配下の Mobile 対応版として整理。最終命名は `Mobile` |
| `<Package>LP` / `<Package>LowPoly` | 別プロダクトとして維持 |
| `Backup`, `FromDra`, タイムスタンプ付き | 内容確認まで `_conflicts` または一時保留 |
| 参照切れが多いが過去版として必要 | `<Package>V0/` |
| 配布物にも Products にも不要 | 削除候補リストへ入れてから削除 |

## 復元時の作業手順

1. `git status --short` を保存して、現状の差分を把握する。
2. 1 パッケージだけ選ぶ。
3. `<Package>` と `<Package>Dev` のペアを作る。
4. 配布 Prefab を `<Package>/Prefabs/` に寄せる。
5. サンプル Variant と Scene を `<Package>Dev/` に寄せる。
6. Unity で Console エラー、Prefab Missing、Material shader、Texture override を確認する。
7. 問題なければそのパッケージ単位でコミットする。

## チェックリスト

- [ ] `<Package>/Prefabs/` に配布 Prefab がある。
- [ ] `<Package>Dev/Scenes/` に確認用シーンがある。
- [ ] `<Package>Dev/Prefabs/` の Prefab は Variant である。
- [ ] 配布 Prefab から Dev 側を参照していない。
- [ ] Material は原則 `VRChat/Mobile/StandardToon`。
- [ ] Texture の PC/Android 差分は Platform Override。
- [ ] Missing script / Missing material / Missing prefab がない。
- [ ] Products 側の `booth/` と `src/` が必要に応じて残っている。
