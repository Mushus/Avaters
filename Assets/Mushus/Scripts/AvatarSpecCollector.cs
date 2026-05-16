using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Mushus.DistributionTools
{
    [System.Serializable]
    public class AvatarSpec
    {
        public string Name;
        public int PolyCount;
        public int MeshCount;
        public int MaterialCount;
        public List<string> Shaders = new List<string>();
        public int PhysBoneCount;
        public List<string> LipSyncShapes = new List<string>();
        public List<string> FaceShapes = new List<string>();
        public bool HasEyeLook;
        public int ExpressionParameters;
        public int ExpressionMenuCount;
    }

    public static class AvatarSpecCollector
    {
        public static AvatarSpec Collect(GameObject avatarRoot)
        {
            var spec = new AvatarSpec { Name = avatarRoot.name };

            // メッシュ情報の収集
            var skinnedMeshes = avatarRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshes = avatarRoot.GetComponentsInChildren<MeshFilter>(true);

            spec.MeshCount = skinnedMeshes.Length + meshes.Length;
            
            foreach (var smr in skinnedMeshes)
            {
                if (smr.sharedMesh != null)
                {
                    spec.PolyCount += smr.sharedMesh.triangles.Length / 3;
                }
                foreach (var mat in smr.sharedMaterials)
                {
                    if (mat != null)
                    {
                        spec.MaterialCount++;
                        if (!spec.Shaders.Contains(mat.shader.name))
                            spec.Shaders.Add(mat.shader.name);
                    }
                }
            }

            foreach (var mf in meshes)
            {
                if (mf.sharedMesh != null)
                {
                    spec.PolyCount += mf.sharedMesh.triangles.Length / 3;
                }
                var renderer = mf.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    foreach (var mat in renderer.sharedMaterials)
                    {
                        if (mat != null)
                        {
                            spec.MaterialCount++;
                            if (!spec.Shaders.Contains(mat.shader.name))
                                spec.Shaders.Add(mat.shader.name);
                        }
                    }
                }
            }

            // PhysBone情報の収集
            var physBones = avatarRoot.GetComponentsInChildren<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>(true);
            spec.PhysBoneCount = physBones.Length;

            // VRChat Descriptor情報の収集
            var descriptor = avatarRoot.GetComponent<VRCAvatarDescriptor>();
            if (descriptor != null)
            {
                spec.HasEyeLook = descriptor.enableEyeLook;
                
                // LipSync
                if (descriptor.lipSync == VRCAvatarDescriptor.LipSyncStyle.VisemeBlendShape && descriptor.VisemeSkinnedMesh != null)
                {
                    // VRChatの規定のViseme順（sil, pp, ff, th, dd, kk, ch, ss, nn, rr, aa, ee, ih, oh, ou）
                    foreach (var shape in descriptor.VisemeBlendShapes)
                    {
                        if (!string.IsNullOrEmpty(shape))
                            spec.LipSyncShapes.Add(shape);
                    }
                }

                // Expressions
                if (descriptor.expressionParameters != null)
                    spec.ExpressionParameters = descriptor.expressionParameters.parameters.Length;
                
                if (descriptor.expressionsMenu != null)
                    spec.ExpressionMenuCount = descriptor.expressionsMenu.controls.Count;

                // Face Shapes (雑多に取得する例として、MeshのBlendShapeからLipSync以外を抽出)
                if (descriptor.VisemeSkinnedMesh != null)
                {
                    var mesh = descriptor.VisemeSkinnedMesh.sharedMesh;
                    for (int i = 0; i < mesh.blendShapeCount; i++)
                    {
                        string shapeName = mesh.GetBlendShapeName(i);
                        // vrc. や viseme などの名前を含むものはリップシンク関連として除外
                        if (!spec.LipSyncShapes.Contains(shapeName) && 
                            !shapeName.ToLower().Contains("vrc.") && 
                            !shapeName.ToLower().Contains("viseme"))
                        {
                            spec.FaceShapes.Add(shapeName);
                        }
                    }
                }
            }

            return spec;
        }
    }
}
