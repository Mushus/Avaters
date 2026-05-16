# Cleanup Notes 2026-05-15

## 実施した整理

- `Candy`, `Krona`, `Soni` の配布 Prefab を `Prefabs/` 配下へ移動した。
- `CandyDev`, `KronaDev`, `SoniDev`, `TreeDragonDev` の Dev 用 Scene を `Scenes/` 配下へ移動した。
- `Calop` と `Krona` の配布フォルダ直下にあった `SampleScene.unity` を Dev 側へ移動した。
- `docs/avatar-development-standard.md` を無印/Mobile の 2 系統前提に更新した。

## 生成した棚卸し

- `mushus-package-inventory-20260515.csv`: パッケージごとのファイル数、FBX、Prefab、Scene の棚卸し。
- `mushus-package-review-20260515.csv`: 標準構成から見た要確認フラグ。
- `mushus-prefab-source-map-20260515.csv`: Scene/Prefab が参照している source prefab GUID と参照先。

## AAC V0 -> V1 migration

解消済み。Unity compile は成功する。

一時的には `Assets/Mushus/CandyDev/Animations/` 内の Editor スクリプトが `AnimatorAsCode.V0` を参照していたため、`vrc-avaters` の `wip` ブランチが指していた submodule を確認した。

その後、公式 migration guide に合わせて Candy の AAC スクリプトを V1 へ移行した。

対象:

- `Assets/Mushus/CandyDev/Animations/AacInspector.cs`
- `Assets/Mushus/CandyDev/Animations/CandyFx.cs`

対応:

- `vrc-avaters` の `wip` ブランチが指していた submodule `unity/Assets/AnimatorAsCodeFramework` を確認した。
- submodule の実体は `hai-vr/av3-animator-as-code` commit `e31fbb15868710b609ffaa52f1f7a04d423df6b4`。
- その commit から `Framework` のみを一時復元した。
- `CandyDev` の V0 参照を V1 API へ移行した。
- V0 参照がなくなったため、一時復元した `Assets/AnimatorAsCodeFramework` は削除した。
- `uloop compile` は Error 0 / Warning 0。

主な変更:

- `AnimatorAsCode.V0` -> `AnimatorAsCode.V1`
- `AacV0.Create` -> `AacV1.Create`
- `AacConfiguration.AvatarDescriptor` -> `.WithAvatarDescriptor(avatar)`
- `ContainerMode = AacConfiguration.Container.Everything`
- VRChat 拡張として `AnimatorAsCode.V1.VRC` と `AnimatorAsCode.V1.VRCDestructiveWorkflow` を使用
- `AacFlState.TrackingElement` -> `AacAv3.Av3TrackingElement`
- `MotionTime(...)` -> `WithMotionTime(...)`

## Missing source prefab GUID

複数シーンが、現 Assets 内に存在しない source prefab GUID を参照している。

特に `2cd7c2d73a12a214b930125a1ca4ed33` は複数パッケージに出ているため、旧共通サンプル Prefab か、復元漏れの可能性がある。

主な該当:

- `Assets/Mushus/CandyDev/Scenes/SampleScene.unity`
- `Assets/Mushus/NozakiNenekoDev/Scenes/SampleScene.unity`
- `Assets/Mushus/NyantanDev/Scenes/SampleScene.unity`
- `Assets/Mushus/NyantanDev/Scenes/SampleScene 2.unity`
- `Assets/Mushus/ReptanDev/Scenes/SampleScene.unity`
- `Assets/Mushus/TreeDragon/SampleScene.unity`
- `Assets/Mushus/TreeDragonDev/Scenes/SampleScene.unity`

その他:

- `34b798b493d535440b179813c5bfb52f`: `Assets/Mushus/CosmicDragonDev/Scenes/Test.unity`
- `912f4f3a3e720c54db624b290149135b`: `Assets/Mushus/TreeDragonDev/Scenes/dev.unity`

## 次に見るべき候補

Prefab が無いが Dev Scene から FBX を参照しているもの:

- `Betty`: `BettyDev/Scenes/betty.unity` -> `Betty/256fes.fbx`
- `CactusBunny`: `CactusBunnyDev/Scenes/SampleScene.unity` -> `CactusBunny/Models/CactusBunny.fbx`
- `Calop`: `CalopDev/Scenes/SampleScene.unity` -> `Calop/Calop.fbx`
- `Cynthea`: `CyntheaDev/Scenes/Cynthea.unity` -> `Cynthea/Models/Cynthea.fbx`
- `SexyDicon`: `SexyDiconDev/Scenes/SampleScene.unity` -> `SexyDicon/Models/SexyDicon.fbx`
- `Shiro`: `ShiroDev/Scenes/SampleScene.unity` -> `Shiro/Models/Shiro.fbx`
- `SweetsDragon`: `SweetsDragonDev/Scenes/SampleScene.unity` -> `SweetsDragon/Models/SweetsDragon.fbx`
- `WhippedCream`: `WhippedCreamDev/Scenes/SampleScene.unity` -> `WhippedCream/Models/WhippedCream.fbx`
- `Yuki`: `YukiDev/Scenes/SampleScene.unity` -> `Yuki/2022-03-21.fbx`

自動 Prefab 化したもの:

- `CactusBunny`: `Assets/Mushus/CactusBunny/Prefabs/CactusBunny.prefab`
- `Calop`: `Assets/Mushus/Calop/Prefabs/Calop.prefab`
- `Cynthea`: `Assets/Mushus/Cynthea/Prefabs/Cynthea.prefab`
- `SexyDicon`: `Assets/Mushus/SexyDicon/Prefabs/SexyDicon.prefab`
- `Shiro`: `Assets/Mushus/Shiro/Prefabs/Shiro.prefab`
- `SweetsDragon`: `Assets/Mushus/SweetsDragon/Prefabs/SweetsDragon.prefab`
- `WhippedCream`: `Assets/Mushus/WhippedCream/Prefabs/WhippedCream.prefab`
- `Yuki`: `Assets/Mushus/Yuki/Prefabs/Yuki.prefab`

保留:

- `Betty`: `BettyDev/Scenes/betty.unity` に `Betty/256fes.fbx` の instance はあるが、VRChat Avatar Descriptor が見つからないため自動 Prefab 化しない。
