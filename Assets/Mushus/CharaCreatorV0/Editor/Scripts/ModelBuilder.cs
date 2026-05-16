using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Mushus.CharaCreatorV0
{
    internal class ModelBuilder
    {
        private static string previewObjectName = "Preview";

        internal static void Preview(CharaCreator cc)
        {
            var preview = cc.transform.Find(previewObjectName);
            if (preview != null)
            {
                GameObject.DestroyImmediate(preview.gameObject);
            }

            var go = new GameObject(previewObjectName);
            var dist = go.transform;
            dist.SetParent(cc.transform);
            BuildPreview(cc, dist);
        }

        internal static void BuildPreview(CharaCreator cc, Transform dist)
        {
            // HACK: すべて消して作り直しは微妙なので、変更されたパーツだけを更新するようにしたい
            DestroyAllChildren(dist);

            var armature = new GameObject(Constant.ArmatureName).transform;
            armature.SetParent(dist, true);

            var mesh = new GameObject("Mesh").transform;
            mesh.SetParent(dist, true);

            var config = cc.GetConfig();

            var combineParts = ListCombineParts(cc, config);

            CombineArmatures(armature, combineParts);
            CombineMeshes(cc, config, mesh, armature, combineParts);
            CombineMeshTextures(cc, mesh, combineParts);
            // CombineAllTexture(cc, combineParts);
        }

        internal static void Build(CharaCreator cc)
        {
            var avater = new GameObject("Avater");

            var config = cc.GetConfig();

            var armature = new GameObject("Armature");
            armature.transform.SetParent(avater.transform, true);

            var mesh = new GameObject("Body");
            mesh.transform.SetParent(avater.transform, true);

            var combineParts = ListCombineParts(cc, config);
            var textureLayout = PlanTextureLayout(combineParts);
            CombineArmatures(armature.transform, combineParts);
            var renderer = mesh.AddComponent<SkinnedMeshRenderer>();
            CombineAllMesh(renderer, armature.transform, textureLayout);
            var texture = CombineAllTexture(cc, textureLayout);
            renderer.sharedMaterial = new Material(Shader.Find("Standard"))
            {
                mainTexture = texture,
            };
            LayerCombiner.ExportTexture2D(texture, "Assets/test.png");
            Exporter.ExportBinaryFBX("Assets/test.fbx", avater);
        }

        private static void DestroyAllChildren(Transform transform)
        {
            foreach (var child in transform.GetComponentsInChildren<Transform>())
            {
                if (child == transform) continue;
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        private static void CombineArmatures(Transform armature, IEnumerable<CombinePart> combineParts)
        {
            foreach (var combinePart in combineParts)
            {
                CombineArmature(armature, combinePart.Definition);
            }
        }

        private static void CombineArmature(Transform distArmature, Part def)
        {
            if (def.Models == null) return;
            foreach (var modelDef in def.Models)
            {

                if (distArmature.childCount == 0)
                {
                    // 最初のボーン
                    var armature = modelDef.Armature;
                    var rootBone = armature.GetChild(0);
                    var rootBoneInstance = GameObject.Instantiate(rootBone, rootBone.position, Quaternion.identity, distArmature);
                    rootBoneInstance.name = rootBone.name;
                }
                else
                {
                    // 2つ目以降のボーン
                    var srcBone = FindTransform(modelDef.Armature, modelDef.RootBone);
                    var distBone = FindTransform(distArmature, modelDef.RootBone);
                    CombineBone(srcBone, distBone);
                }
            }
        }

        private static void CombineBone(Transform srcBone, Transform dstBone)
        {
            var distBoneChildren = new Dictionary<string, Transform>();
            for (var i = 0; i < dstBone.childCount; i++)
            {
                var child = dstBone.GetChild(i);
                distBoneChildren.Add(child.name, child);
            }

            for (var i = 0; i < srcBone.childCount; i++)
            {
                var srcChild = srcBone.GetChild(i);
                if (distBoneChildren.ContainsKey(srcChild.name))
                {
                    var dstChild = distBoneChildren[srcChild.name];
                    CombineBone(srcChild, dstChild);
                }
                else
                {
                    var srcBoneInstance = GameObject.Instantiate(srcChild, dstBone);
                    srcBoneInstance.name = srcChild.name;
                    srcBoneInstance.localPosition = srcChild.localPosition;
                    srcBoneInstance.localRotation = srcChild.localRotation;
                }
            }
        }

        private static void CombineMeshes(CharaCreator cc, Config config, Transform mesh, Transform armature, IEnumerable<CombinePart> combineParts)
        {
            foreach (var combinePart in combineParts)
            {
                var myPartPath = combinePart.Path;
                var distMesh = new GameObject(CreateDstMeshNameByPath(myPartPath)).transform;
                distMesh.SetParent(mesh);

                CombineMesh(cc, config, distMesh, armature, combinePart);
            }
        }

        private static void CombineMesh(CharaCreator cc, Config config, Transform distMesh, Transform armature, CombinePart combinePart)
        {
            var def = combinePart.Definition;
            var material = CreateMaterial(config.Sharder);
            if (def == null || def.Models == null) return;
            foreach (var modelDef in def.Models)
            {
                var srcArmature = modelDef.Armature;
                var srcRootBone = FindTransform(srcArmature, modelDef.RootBone);
                foreach (var mesh in modelDef.Meshes)
                {
                    var targetRootBone = FindTransform(armature, modelDef.RootBone);
                    var position = targetRootBone.TransformPoint(srcRootBone.InverseTransformPoint(mesh.transform.position));
                    var rotation = Quaternion.Euler(targetRootBone.TransformDirection(srcRootBone.InverseTransformDirection(mesh.transform.rotation.eulerAngles)));
                    var meshInstance = GameObject.Instantiate(mesh, position, rotation, distMesh);
                    meshInstance.name = mesh.name;
                    RemapBones(meshInstance, armature);

                    var renderer = meshInstance.GetComponent<SkinnedMeshRenderer>();
                    renderer.materials = renderer.sharedMaterials.Select(m => material).ToArray();
                    foreach (var morph in def.Morphs)
                    {
                        var morphKey = combinePart.Path + "//" + morph.Key;
                        var morphProp = cc.FindMorph(morphKey);
                        var blendShapeDefinition = morph.BlendShapes[morphProp.Index];
                        if (blendShapeDefinition.Key == Constant.NoneKey) continue;
                        var blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(blendShapeDefinition.Key);
                        renderer.SetBlendShapeWeight(blendShapeIndex, 100 * morphProp.Value);
                    }
                }
            }
        }

        private static void RemapBones(GameObject mesh, Transform armature)
        {
            var bonesTable = armature.GetComponentsInChildren<Transform>().ToDictionary(b => b.name);
            var renderer = mesh.GetComponent<SkinnedMeshRenderer>();
            renderer.rootBone = bonesTable[renderer.rootBone.name];
            renderer.bones = renderer.bones.Select(b => bonesTable[b.name]).ToArray();
        }

        private static void CombineMeshTextures(CharaCreator cc, Transform mesh, IEnumerable<CombinePart> combineParts)
        {
            foreach (var combinePart in combineParts)
            {
                var myPartPath = combinePart.Path;
                var dstMesh = mesh.Find(CreateDstMeshNameByPath(myPartPath));

                var texture = CreatePartTexture(cc, combinePart.Definition, myPartPath);

                if (texture == null) continue;

                for (var i = 0; i < dstMesh.childCount; i++)
                {
                    var renderedMesh = dstMesh.GetChild(i);
                    var renderer = renderedMesh.GetComponent<SkinnedMeshRenderer>();
                    renderer.sharedMaterial.mainTexture = texture;
                }
            }
        }

        private static Texture2D CreatePartTexture(CharaCreator cc, Part partDef, string partPath)
        {
            var textureDef = partDef.Textures;
            if (textureDef == null) return null;

            var layers = textureDef.SelectMany(layerVariant =>
            {
                var layerPath = $"{partPath}//{layerVariant.Key}";
                var layer = cc.FindLayer(layerPath);
                var layerDef = layerVariant.Variants[layer.index];

                return layerDef.Textures.Select(tex =>
                {
                    var texturePath = $"{partPath}//{tex.Key}";
                    var texture = cc.Textures.FirstOrDefault(t => t.path == texturePath);
                    if (texture == null) texture = cc.Textures[0];
                    var color = texture.IsSelectedColor ? texture.color : cc.GetDefaultColor(tex.ColorKey) ?? (tex.Color != null ? tex.Color : Color.white);
                    return new CombineLayer(texture: tex.Texture, color: color, blendMode: (int)tex.BlendMode);
                });
            });

            var size = new Vector2Int(1024, 1024);
            return LayerCombiner.Combine(size, layers.ToArray());
        }

        private static Transform FindTransform(Transform transform, string name)
        {
            return transform.GetComponentsInChildren<Transform>().First(t => t.name == name);
        }

        private static PartProperty FindPartProperty(PartProperty[] parts, string path)
        {
            return parts.FirstOrDefault(p => p.path == path);
        }

        private static Material CreateMaterial(Surface config)
        {
            var sharder = Shader.Find(config.Name);
            var material = new Material(sharder);
            material.SetColor("_Color", Color.white);
            return material;
        }

        private static string CreateDstMeshNameByPath(string path)
        {
            // GameObjectを検索するときは/が入っていると子要素を検索しようとしてしまうので、/を__に置き換える
            return path.Replace("/", "__");
        }

        internal class CombinePart
        {
            internal readonly string Path;
            internal readonly Part Definition;
            internal readonly PartProperty PartProperty;
            internal readonly IEnumerable<CombineBlendShape> BlendShapes;

            internal CombinePart(string path, Part definition, PartProperty partProperty, IEnumerable<CombineBlendShape> blendShapes)
            {
                Path = path;
                Definition = definition;
                PartProperty = partProperty;
                BlendShapes = blendShapes;
            }
        }

        internal class CombineBlendShape
        {
            internal readonly string Name;
            internal readonly BlendShapeDefinition BlendShapeDefinition;
            internal readonly Morph MorphDefinition;
            internal readonly float Value;
            internal CombineBlendShape(string name, Morph morph, BlendShapeDefinition blendShape, float value)
            {
                Name = name;
                MorphDefinition = morph;
                BlendShapeDefinition = blendShape;
                Value = value;
            }
        }

        // 結合するパーツ一覧を作る
        private static IEnumerable<CombinePart> ListCombineParts(CharaCreator cc, Config config)
        {
            return ListCobinePartsInternal(cc, config.Parts);
        }

        private static List<CombinePart> ListCobinePartsInternal(CharaCreator cc, PartVariant variant, string parentPartPath = null)
        {
            var myPartPath = parentPartPath != null ? $"{parentPartPath}/{variant.Key}" : variant.Key;

            var myPartProperty = FindPartProperty(cc.Parts, myPartPath);
            var myPartDefinitions = variant.parts;
            var myPartDefinition = myPartDefinitions[myPartProperty.index];

            var combineParts = new List<CombinePart>();

            // 自身のパーツ
            {
                var combineBlendShapes = new List<CombineBlendShape>();
                foreach (var morph in myPartDefinition.Morphs)
                {
                    var morphKey = myPartPath + "//" + morph.Key;
                    var morphProp = cc.FindMorph(morphKey);
                    var blendShapeDefinition = morph.BlendShapes[morphProp.Index];
                    if (blendShapeDefinition.Key == Constant.NoneKey) continue;
                    combineBlendShapes.Add(new CombineBlendShape(blendShapeDefinition.Key, morph, blendShapeDefinition, morphProp.Value));
                }

                var conbinePart = new CombinePart(myPartPath, myPartDefinition, myPartProperty, combineBlendShapes);
                combineParts.Add(conbinePart);
            }

            // 子供のパーツ
            {
                foreach (var childVariant in myPartDefinition.Children)
                {
                    var childrenCombinePart = ListCobinePartsInternal(cc, childVariant, myPartPath);
                    combineParts.AddRange(childrenCombinePart);
                }
            }
            return combineParts;
        }

        // 1枚のテクスチャにまとめる
        private static Texture2D CombineAllTexture(CharaCreator cc, TextureLayout layout)
        {
            var textureSizeFloat = (float)layout.TextureSize;
            var partCount = layout.Blocks.Length;
            var layers = new List<CombineLayer>();
            for (var i = 0; i < partCount; i++)
            {
                var block = layout.Blocks[i];
                var offsetInt = block.Offset;
                var part = block.Part;
                var texture = CreatePartTexture(cc, part.Definition, part.Path);
                var offset = new Rect(
                    (float)offsetInt.x / textureSizeFloat,
                    (float)offsetInt.y / textureSizeFloat,
                    (float)offsetInt.width / textureSizeFloat,
                    (float)offsetInt.height / textureSizeFloat
                );
                layers.Add(new CombineLayer(texture: texture, offset: offset));
            }

            return LayerCombiner.Combine(new Vector2Int(1024, 1024), layers.ToArray());
        }

        // テクスチャのレイアウト
        internal class TextureLayout
        {
            // テクスチャサイズ
            internal readonly int TextureSize;
            // テクスチャのレイアウトのブロック
            internal readonly TextureLayoutBlock[] Blocks;

            internal TextureLayout(int textureSize, TextureLayoutBlock[] blocks)
            {
                TextureSize = textureSize;
                Blocks = blocks;
            }
        }

        // テクスチャのレイアウトのブロック
        internal class TextureLayoutBlock
        {
            // テクスチャ上でのブロックの位置
            internal readonly RectInt Offset;
            // 対象パーツ
            internal readonly CombinePart Part;
            internal TextureLayoutBlock(RectInt offset, CombinePart part)
            {
                Offset = offset;
                Part = part;
            }
        }

        // テクスチャレイアウトをつくる
        private static TextureLayout PlanTextureLayout(IEnumerable<CombinePart> combineParts)
        {
            // 戦略:
            // 1. テクスチャが大きいものから順番に左上に詰めていく
            // 2. 箱に入らなくなったら箱を拡張する
            var sotedCombineTexture = SortCombinePartsByTextureSize(combineParts);
            var currentTextureSize = 0;
            // 置ける位置は詰めたテクスチャブロックの凹部分の座標
            var placeablePositions = new List<Vector2Int>(){
                new Vector2Int(0, 0),
            };
            // combinePartsのindexに対応したテクスチャをおいた位置
            var blocks = new List<TextureLayoutBlock>();
            for (var i = 0; i < sotedCombineTexture.Length; i++)
            {
                var combinePart = sotedCombineTexture[i];
                var partTextureSize = combinePart.Definition.TextureSize.Size();

                var placeIndex = placeablePositions.FindIndex(p => p.x < currentTextureSize && p.y < currentTextureSize);
                if (placeIndex == -1) placeIndex = 0;

                var placePosition = placeablePositions[placeIndex];
                // テクスチャの配置位置決定
                var offset = new RectInt(placePosition.x, placePosition.y, partTextureSize, partTextureSize);
                blocks.Add(new TextureLayoutBlock(offset, combinePart));
                // テクスチャブロックを配置すると凹部分が下と右にできる可能性がある

                // 下部分
                {
                    var nextPlaceableIndex = placeIndex + 1;
                    // なるべく左上を優先するため、水平の位置が配置可能な場所が既にある場合は追加しない
                    if (nextPlaceableIndex >= placeablePositions.Count || placeablePositions[nextPlaceableIndex].y != placePosition.y + partTextureSize)
                    {
                        placeablePositions.Insert(nextPlaceableIndex, new Vector2Int(placePosition.x, placePosition.y + partTextureSize));
                    }
                }

                // indexの計算の関係上ここで配置済みのテクスチャブロックの位置を削除
                placeablePositions.RemoveAt(placeIndex);

                // 右部分
                {
                    var prevPlaceableIndex = placeIndex - 1;
                    // なるべく左上を優先するため、垂直の位置が配置可能な場所が既にある場合は追加しない
                    if (prevPlaceableIndex < 0 || placeablePositions[prevPlaceableIndex].x != placePosition.x + partTextureSize)
                    {
                        placeablePositions.Insert(placeIndex, new Vector2Int(placePosition.x + partTextureSize, placePosition.y));
                    }
                }

                // 置ける位置がなくなったら箱を拡張する
                // NOTE: 横方向が優勢のため、拡張時はX方向のみの確認でOK
                currentTextureSize = Math.Max(currentTextureSize, placePosition.x + partTextureSize);
            }

            return new TextureLayout(currentTextureSize, blocks.ToArray());
        }

        private static CombinePart[] SortCombinePartsByTextureSize(IEnumerable<CombinePart> combineParts)
        {
            return combineParts.OrderBy(p => -(int)p.Definition.TextureSize).ToArray();
        }

        private static void CombineAllMesh(SkinnedMeshRenderer dstRenderer, Transform armature, TextureLayout layout)
        {
            var textureSize = (float)layout.TextureSize;

            var rootBone = armature.GetChild(0);
            var bones = rootBone.GetComponentsInChildren<Transform>();
            var boneTable = bones.ToDictionary(b => b.name, b => b);
            var combineBoneIndexTable = bones
                .Select((b, i) => new KeyValuePair<string, int>(b.name, i))
                .ToDictionary(p => p.Key, p => p.Value);

            var blendShapeNames = GetCombineBlendShape(layout.Blocks.Select(b => b.Part.Definition));
            var blendShapes = blendShapeNames.Select(n => new BlendShape() { Name = n }).ToArray();

            var subMeshCount = 1;

            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            var boneWeights = new List<BoneWeight>();
            var weight = new List<BoneWeight>();
            var colors = new List<Color>();
            var colors32 = new List<Color32>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();

            // TODO: ブレンドシェイプの実装
            foreach (var block in layout.Blocks)
            {
                var part = block.Part;
                var models = part.Definition.Models;
                var morphs = part.Definition.Morphs;
                foreach (var model in models)
                {
                    var srcArmature = model.Armature;
                    var srcRootBone = FindTransform(srcArmature, model.RootBone);
                    var targetRootBone = FindTransform(armature, model.RootBone);
                    foreach (var mesh in model.Meshes)
                    {
                        var renderer = mesh.GetComponent<SkinnedMeshRenderer>();
                        var srcMesh = renderer.sharedMesh;
                        var srcTriangles = srcMesh.triangles;

                        var defaultOnlyBlendShape = new BlendShapeDelta(srcMesh.vertexCount);
                        // シェイプキーのベイク
                        var combineDeltaVertices = new Vector3[srcMesh.vertexCount];
                        var combineDeltaNormals = new Vector3[srcTriangles.Length];
                        foreach (var combineBlendShape in part.BlendShapes)
                        {
                            var index = srcMesh.GetBlendShapeIndex(combineBlendShape.Name);
                            var frameIndex = srcMesh.GetBlendShapeFrameCount(index) - 1;

                            var deltaVertices = new Vector3[srcMesh.vertexCount];
                            var deltaNormals = new Vector3[srcMesh.vertexCount];
                            var deltaTangents = new Vector3[srcMesh.vertexCount];
                            srcMesh.GetBlendShapeFrameVertices(index, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                            for (var i = 0; i < deltaVertices.Length; i++)
                            {
                                combineDeltaVertices[i] += deltaVertices[i] * combineBlendShape.Value;
                            }
                            for (var i = 0; i < srcTriangles.Length; i++)
                            {
                                combineDeltaNormals[i] += deltaNormals[srcTriangles[i]] * combineBlendShape.Value;
                            }
                            if (combineBlendShape.MorphDefinition.DefaultOnly)
                            {
                                for (var i = 0; i < deltaVertices.Length; i++)
                                {
                                    defaultOnlyBlendShape.DeltaVertices[i] += deltaVertices[i] * combineBlendShape.Value;
                                    defaultOnlyBlendShape.DeltaNormals[i] += deltaNormals[i] * combineBlendShape.Value;
                                    defaultOnlyBlendShape.DeltaTangents[i] += deltaTangents[i] * combineBlendShape.Value;
                                }
                            }
                        }

                        // ルートボーンの位置を考慮した頂点変換
                        var vertexTransform = targetRootBone.localToWorldMatrix * srcRootBone.worldToLocalMatrix;
                        var newVertex = srcMesh.vertices.Select((v, i) => vertexTransform.MultiplyPoint(v + combineDeltaVertices[i]));

                        // NOTE: 法線の変換は逆転置行列で計算するのが知られている
                        var normalTransform = vertexTransform.inverse.transpose;
                        var newNormals = srcMesh.normals.Select((n, i) => normalTransform.MultiplyVector(n + combineDeltaNormals[i]));

                        // UV
                        var uvScale = (float)part.Definition.TextureSize.Size() / textureSize;
                        // NOTE: blockの原点は左上を基準にしているが、UVは原点が左下
                        var offsetU = (float)block.Offset.x / textureSize;
                        var offsetV = 1 - (float)block.Offset.yMax / textureSize;
                        var newUv = srcMesh.uv.Select(u => new Vector2(u.x * uvScale + offsetU, u.y * uvScale + offsetV));

                        // ボーンの変更に伴い、ボーンのインデックスを修正する
                        var bonesTable = renderer.bones
                            .Select((b, i) => new KeyValuePair<int, int>(i, combineBoneIndexTable[b.name]))
                            .ToDictionary(p => p.Key, p => p.Value);
                        var newBoneWeights = srcMesh.boneWeights.Select(bw =>
                        {
                            return new BoneWeight()
                            {
                                boneIndex0 = bonesTable[bw.boneIndex0],
                                boneIndex1 = bonesTable[bw.boneIndex1],
                                boneIndex2 = bonesTable[bw.boneIndex2],
                                boneIndex3 = bonesTable[bw.boneIndex3],
                                weight0 = bw.weight0,
                                weight1 = bw.weight1,
                                weight2 = bw.weight2,
                                weight3 = bw.weight3,
                            };
                        });

                        // 頂点のインデックスがずれるので、インデックスを修正する
                        var triangleOffsetIndex = vertices.Count;
                        var newTriangles = srcMesh.triangles.Select(t => t + triangleOffsetIndex);

                        {// メッシュ情報を統合する
                            vertices.AddRange(newVertex);
                            uv.AddRange(newUv);
                            triangles.AddRange(newTriangles);
                            boneWeights.AddRange(newBoneWeights);
                            colors.AddRange(srcMesh.colors);
                            colors32.AddRange(srcMesh.colors32);
                            normals.AddRange(newNormals);
                            tangents.AddRange(srcMesh.tangents);
                        }

                        foreach (var blendShape in blendShapes)
                        {
                            var delta = MeshUtil.GetBlendShape(srcMesh, blendShape.Name) - defaultOnlyBlendShape;
                            blendShape.Append(delta);
                        }
                    }
                }
            }

            var bindposes = bones.Select(b => b.worldToLocalMatrix * Matrix4x4.identity).ToArray();

            var combineMeshes = new Mesh()
            {
                name = "Body",
                vertices = vertices.ToArray(),
                uv = uv.ToArray(),
                triangles = triangles.ToArray(),
                bindposes = bindposes,
                boneWeights = boneWeights.ToArray(),
                colors = colors.ToArray(),
                colors32 = colors32.ToArray(),
                normals = normals.ToArray(),
                subMeshCount = subMeshCount,
                tangents = tangents.ToArray(),
            };

            // 以下のコードを追加するとFBXを書き出すときに次のエラーが出る
            // 書き出しには成功しているので支障はないが、原因を調べたい
            // Next vertex not found in CheckWinding()
            foreach (var blendShape in blendShapes)
            {
                blendShape.AppendTo(combineMeshes);
            }

            {
                combineMeshes.RecalculateTangents();
                dstRenderer.sharedMesh = combineMeshes;
                dstRenderer.rootBone = rootBone;
                dstRenderer.bones = bones;
            };
        }

        internal static IEnumerable<string> GetCombineBlendShape(IEnumerable<Part> parts)
        {
            var blendShapeNameSet = new HashSet<string>();
            foreach (var part in parts)
            {
                var morphs = part.Morphs;
                var morphTargetBlendShapeSet = new HashSet<string>();
                foreach (var morph in part.Morphs)
                {
                    foreach (var morphBlendShape in morph.BlendShapes)
                    {
                        morphTargetBlendShapeSet.Add(morphBlendShape.Key);
                    }
                }

                foreach (var model in part.Models)
                {
                    foreach (var mesh in model.Meshes)
                    {
                        var renderer = mesh.GetComponent<SkinnedMeshRenderer>();
                        var meshInstance = renderer.sharedMesh;
                        var blendShapeCount = meshInstance.blendShapeCount;
                        for (var i = 0; i < blendShapeCount; i++)
                        {
                            var blendShapeName = meshInstance.GetBlendShapeName(i);
                            if (!morphTargetBlendShapeSet.Contains(blendShapeName))
                            {
                                blendShapeNameSet.Add(blendShapeName);
                            }
                        }
                    }
                }
            }
            return blendShapeNameSet.OrderBy(n => n);
        }

        internal static class MeshUtil
        {
            public static BlendShapeDelta FindBlendShape(Mesh mesh, string name)
            {
                var vertexCount = mesh.vertexCount;
                var index = mesh.GetBlendShapeIndex(name);
                if (index == -1)
                {
                    return new BlendShapeDelta(vertexCount);
                }
                var frameIndex = mesh.GetBlendShapeFrameCount(index) - 1;
                var deltaVertices = new Vector3[vertexCount];
                var deltaNormals = new Vector3[vertexCount];
                var deltaTangents = new Vector3[vertexCount];
                mesh.GetBlendShapeFrameVertices(index, frameIndex, deltaVertices, deltaNormals, deltaTangents);
                return new BlendShapeDelta(deltaVertices, deltaNormals, deltaTangents);
            }

            public static BlendShapeDelta GetBlendShape(Mesh mesh, string name)
            {
                var index = mesh.GetBlendShapeIndex(name);
                if (index == -1)
                {
                    return new BlendShapeDelta(mesh.vertexCount);
                }
                var frameIndex = mesh.GetBlendShapeFrameCount(index) - 1;
                var deltaVertices = new Vector3[mesh.vertexCount];
                var deltaNormals = new Vector3[mesh.vertexCount];
                var deltaTangents = new Vector3[mesh.vertexCount];
                mesh.GetBlendShapeFrameVertices(index, frameIndex, deltaVertices, deltaNormals, deltaTangents);
                return new BlendShapeDelta(deltaVertices, deltaNormals, deltaTangents);
            }
        }

        internal class BlendShapeDelta
        {
            internal Vector3[] DeltaVertices;
            internal Vector3[] DeltaNormals;
            internal Vector3[] DeltaTangents;

            internal BlendShapeDelta(Vector3[] deltaVertices, Vector3[] deltaNormals, Vector3[] deltaTangents)
            {
                DeltaVertices = deltaVertices;
                DeltaNormals = deltaNormals;
                DeltaTangents = deltaTangents;
            }

            internal BlendShapeDelta(int vertexCount)
            {
                DeltaVertices = new Vector3[vertexCount];
                DeltaNormals = new Vector3[vertexCount];
                DeltaTangents = new Vector3[vertexCount];
            }

            internal void ApplyVertices(Vector3[] vertecies, float strength = 1)
            {
                for (var i = 0; i < DeltaVertices.Length; i++)
                {
                    vertecies[i] += DeltaVertices[i] * strength;
                }
            }

            internal void ApplyNormals(int[] triangles, Vector3[] normals, float strength = 1)
            {
                for (var i = 0; i < triangles.Length; i++)
                {
                    var vertexIndex = triangles[i];
                    normals[i] += DeltaNormals[vertexIndex] * strength;
                }
            }

            internal void ApplyTangents(int[] triangles, Vector4[] tangents, float strength = 1)
            {
                for (var i = 0; i < triangles.Length; i++)
                {
                    var vertexIndex = triangles[i];
                    var targetDeltaTangent = DeltaTangents[vertexIndex] * strength;
                    tangents[i].x += targetDeltaTangent.x;
                    tangents[i].y += targetDeltaTangent.y;
                    tangents[i].z += targetDeltaTangent.z;
                }
            }

            private static BlendShapeDelta Add(BlendShapeDelta a, BlendShapeDelta b, float strength)
            {
                var deltaVertices = new Vector3[a.DeltaVertices.Length];
                var deltaNormals = new Vector3[a.DeltaNormals.Length];
                var deltaTangents = new Vector3[a.DeltaTangents.Length];
                for (var i = 0; i < deltaVertices.Length; i++)
                {
                    deltaVertices[i] = a.DeltaVertices[i] + b.DeltaVertices[i] * strength;
                    deltaNormals[i] = a.DeltaNormals[i] + b.DeltaNormals[i] * strength;
                    deltaTangents[i] = a.DeltaTangents[i] + b.DeltaTangents[i] * strength;
                }
                return new BlendShapeDelta(deltaVertices, deltaNormals, deltaTangents);
            }

            public static BlendShapeDelta operator +(BlendShapeDelta a, BlendShapeDelta b)
            {
                return Add(a, b, 1);
            }

            public static BlendShapeDelta operator -(BlendShapeDelta a, BlendShapeDelta b)
            {
                return Add(a, b, -1);
            }
        }

        internal class BlendShape
        {
            internal string Name = "";
            internal List<Vector3> DeltaVertices = new List<Vector3>();
            internal List<Vector3> DeltaNormals = new List<Vector3>();
            internal List<Vector3> DeltaTangents = new List<Vector3>();

            // ブレンドシェープをMeshに追加します
            internal void AppendTo(Mesh mesh)
            {
                mesh.AddBlendShapeFrame(Name, 100, DeltaVertices.ToArray(), DeltaNormals.ToArray(), DeltaTangents.ToArray());
            }

            internal void Append(BlendShapeDelta delta)
            {
                DeltaVertices.AddRange(delta.DeltaVertices);
                DeltaNormals.AddRange(delta.DeltaNormals);
                DeltaTangents.AddRange(delta.DeltaTangents);
            }
        }
    }
}