// このスクリプトによってFXレイヤーを作成しています
// スクリプトの使い方は下に記載しています

#if UNITY_EDITOR
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using AnimatorAsCode.V0;

namespace Neko.FX
{
    [CustomEditor(typeof(FXHands), true)]
    public class FXHandsEditor : Editor
    {
        private const string SystemName = "Fx";

        public override void OnInspectorGUI()
        {
            AacFX.InspectorTemplate(this, serializedObject, "assetKey", Create, Remove);
        }

        // ハンドサインの定義
        public static Gesture GestureNone = new Gesture(0, "None");
        public static Gesture GestureFist = new Gesture(1, "Fist");
        public static Gesture GestureOpen = new Gesture(2, "Open");
        public static Gesture GesturePoint = new Gesture(3, "Point");
        public static Gesture GesturePeace = new Gesture(4, "Peace");
        public static Gesture GestureRockNRoll = new Gesture(5, "RockNRoll");
        public static Gesture GestureGun = new Gesture(6, "Gun");
        public static Gesture GestureThumbsUp = new Gesture(7, "ThumbsUp");
        public static Gesture[] Gestures = {
            GestureNone,
            GestureFist,
            GestureOpen,
            GesturePoint,
            GesturePeace,
            GestureRockNRoll,
            GestureGun,
            GestureThumbsUp,
        };

        private void Create()
        {
            var my = (FXHands) target;
            var aac = AacFX.AnimatorAsCode(SystemName, my.avatar, my.assetContainer, my.assetKey, AacFX.Options().WriteDefaultsOff());
            
            var fx = aac.CreateMainFxLayer();
            
            // 目のアニメーションの定義
            var eyeAnims = new Animation[] {
                new Animation(1, "AnimEyeAngly", LoadMotion("AnimEyeAngly"), 0.05f, null),
                new Animation(2, "AnimEyeBou", LoadMotion("AnimEyeBou"), 0, null),
                new Animation(3, "AnimEyeEmotionless", LoadMotion("AnimEyeEmotionless"), 0.05f, null),
                new Animation(4, "AnimEyeHachu", LoadMotion("AnimEyeHachu"), 0, null),
                new Animation(5, "AnimEyeHachuH", LoadMotion("AnimEyeHachuH"), 0, null),
                new Animation(6, "AnimEyeHachuV", LoadMotion("AnimEyeHachuV"), 0, null),
                new Animation(7, "AnimEyeJito", LoadMotion("AnimEyeJito"), 0.05f, null),
                new Animation(8, "AnimEyeMyu", LoadMotion("AnimEyeMyu"), 0, null),
                new Animation(9, "AnimEyeRounded", LoadMotion("AnimEyeRounded"), 0.05f, null),
                new Animation(10, "AnimEyeSleepy", LoadMotion("AnimEyeSleepy"), 0.05f, null),
                new Animation(11, "AnimEyeSmile", LoadMotion("AnimEyeSmile"), 0.05f, null),
                new Animation(12, "AnimEyeSmileS", LoadMotion("AnimEyeSmileS"), 0.05f, null),
                new Animation(13, "AnimEyeTen", LoadMotion("AnimEyeTen"), 0, null),
                new Animation(14, "AnimEyeWorry", LoadMotion("AnimEyeWorry"), 0.05f, null),
            };

            // 目のアニメーション指の対応関係の定義
            var gestureEye = new int[,] {
                {0, 3,  0, 2, 11,  1, 0, 9}, // 0: None
                {0, 3, 14, 2, 11,  1, 0, 9}, // 1: Fist
                {0, 3, 14, 2, 11,  1, 0, 9}, // 2: Open
                {0, 3, 13, 2, 12,  1, 0, 0}, // 3: Point    
                {0, 3, 13, 2,  8,  1, 0, 0}, // 4: Peace
                {0, 3,  5, 2, 11, 14, 0, 0}, // 5: RockNRoll
                {0, 3, 13, 2, 12, 14, 0, 0}, // 6: Gun
                {0, 3,  5, 2, 11, 14, 0, 0}, // 7: ThumbsUp
            }; //0, 1,  2, 3,  4, 5, 6, 7
            
            // 目エフェクトのアニメーションの定義
            var eyeEffectAnims = new Animation[] {
                new Animation(1, "AnimEyeEffectGuruguru", LoadMotion("AnimEyeEffectGuruguru"), 0, null),
                new Animation(2, "AnimEyeEffectHeart", LoadMotion("AnimEyeEffectHeart"), 0, null),
                new Animation(3, "AnimEyeEffectShirome", LoadMotion("AnimEyeEffectShirome"), 0, null),
                new Animation(4, "AnimEyeEffectStar", LoadMotion("AnimEyeEffectStar"), 0, null),
                new Animation(5, "AnimEyeEffectUru", LoadMotion("AnimEyeEffectUru"), 0, null),
            };

            // 目エフェクトのアニメーション指の対応関係の定義
            var gestureEyeEffect = new int[,] {
                {0, 0, 3, 0, 0, 0, 3, 0}, // 0: None
                {0, 0, 0, 0, 0, 0, 3, 0}, // 1: Fist
                {0, 0, 0, 0, 0, 0, 3, 0}, // 2: Open
                {0, 0, 0, 0, 0, 0, 5, 4}, // 3: Point
                {0, 0, 0, 0, 0, 0, 5, 4}, // 4: Peace
                {0, 0, 0, 0, 0, 0, 1, 4}, // 5: RockNRoll
                {0, 0, 0, 0, 0, 0, 5, 4}, // 6: Gun
                {0, 0, 0, 0, 0, 5, 1, 2}, // 7: ThumbsUp
            }; //0, 1, 2, 3, 4, 5, 6, 7

            // 口のアニメーションの定義
            var mouthAnims = new Animation[] {
                new Animation(1, "AnimMouthAngly", LoadMotion("AnimMouthAngly"), 0.05f, fx.Av3().Voice),
                new Animation(2, "AnimMouthEmotionless", LoadMotion("AnimMouthEmotionless"), 0.05f, fx.Av3().Voice),
                new Animation(3, "AnimMouthGne", LoadMotion("AnimMouthGne"), 0.05f, fx.Av3().Voice),
                new Animation(4, "AnimMouthOpen", LoadMotion("AnimMouthOpen"), 0.05f, fx.Av3().Voice),
                new Animation(5, "AnimMouthOpenSmile", LoadMotion("AnimMouthOpenSmile"), 0.05f, fx.Av3().Voice),
                new Animation(6, "AnimMouthSmile", LoadMotion("AnimMouthSmile"), 0.05f, fx.Av3().Voice),
                new Animation(7, "AnimMouthTongue", LoadMotion("AnimMouthTongue"), 0.05f, fx.Av3().Voice),
                new Animation(8, "AnimMouthTongue2", LoadMotion("AnimMouthTongue2"), 0.05f, fx.Av3().Voice),
                new Animation(9, "AnimMouthV", LoadMotion("AnimMouthV"), 0.05f, fx.Av3().Voice),
                new Animation(10, "AnimMoouthWorry", LoadMotion("AnimMoouthWorry"), 0.05f, fx.Av3().Voice),
            };

            // 口のアニメーション指の対応関係の定義
            var gestureMouth = new int[,] {
                {0, 0, 0, 0, 6, 1,  3, 0}, // 0: None
                {0, 0, 0, 0, 5, 1,  3, 0}, // 1: Fist
                {0, 0, 0, 0, 5, 1,  3, 0}, // 2: Open
                {0, 9, 0, 3, 5, 1,  0, 0}, // 3: Point
                {0, 9, 0, 9, 5, 1,  0, 0}, // 4: Peace
                {8, 9, 0, 9, 5, 6, 10, 0}, // 5: RockNRoll
                {0, 9, 0, 3, 5, 6,  0, 0}, // 6: Gun
                {0, 7, 0, 9, 5, 6, 10, 0}, // 7: ThumbsUp
            }; //0, 1, 2, 3, 4, 5, 6, 7

            // 顔エフェクトのアニメーションの定義
            var faceEffectAnims = new Animation[] {
                new Animation(1, "AnimFaceEffectBlush", LoadMotion("AnimFaceEffectBlush"), 0, null),
                new Animation(2, "AnimFaceEffectSweat", LoadMotion("AnimFaceEffectSweat"), 0, null),
                new Animation(3, "AnimFaceEffectTear", LoadMotion("AnimFaceEffectTear"), 0, null),
            };

            // 顔エフェクトのアニメーション指の対応関係の定義
            var gestureFaceEffect = new int[,] {
                {0, 0, 0, 0, 0, 0, 0, 0}, // 0: None
                {0, 0, 0, 0, 0, 0, 0, 0}, // 1: Fist
                {0, 0, 0, 0, 0, 0, 0, 0}, // 2: Open
                {0, 0, 0, 0, 0, 0, 0, 0}, // 3: Point
                {0, 0, 0, 0, 1, 0, 0, 0}, // 4: Peace
                {0, 0, 0, 0, 0, 3, 0, 0}, // 5: RockNRoll
                {0, 0, 0, 0, 0, 2, 0, 0}, // 6: Gun
                {0, 0, 0, 0, 0, 0, 0, 1}, // 7: ThumbsUp
            }; //0, 1, 2, 3, 4, 5, 6, 7


            var eyeGesture = fx.IntParameter("EyeGesture");
            var eyeEffectGesture = fx.IntParameter("EyeEffectGesture");
            var mouthGesture = fx.IntParameter("MouthGesture");
            var faceEffectGesture = fx.IntParameter("FaceEffectGesture");

            var eye = fx.IntParameter("Eye");
            var eyeEffect = fx.IntParameter("EyeEffect");
            var mouth = fx.IntParameter("Mouth");
            var faceEffect = fx.IntParameter("FaceEffect");

            var paramTables = new ParamTable[] {
                new ParamTable(eyeGesture, gestureEye),
                new ParamTable(eyeEffectGesture, gestureEyeEffect),
                new ParamTable(mouthGesture, gestureMouth),
                new ParamTable(faceEffectGesture, gestureFaceEffect),
            };
            DefineGestureLayer(fx, paramTables);

            var eyeLayer = aac.CreateSupportingFxLayer("Eye");
            DefineAnimationLayer(eyeLayer, eyeAnims, eye, eyeGesture, AacFlState.TrackingElement.Eyes);

            var eyeEffectLayer = aac.CreateSupportingFxLayer("EyeEffect");
            DefineAnimationLayer(eyeEffectLayer, eyeEffectAnims, eyeEffect, eyeEffectGesture, null);

            var mouthLayer = aac.CreateSupportingFxLayer("Mouth");
            DefineAnimationLayer(mouthLayer, mouthAnims, mouth, mouthGesture, null);

            var faceEffectLayer = aac.CreateSupportingFxLayer("FaceEffect");
            DefineAnimationLayer(faceEffectLayer, faceEffectAnims, faceEffect, faceEffectGesture, null);
        }

        private Motion LoadMotion(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Motion>("Assets/Neko/Animations/" + path + ".anim");
        }

        private void DefineGestureLayer(AacFlLayer layer, ParamTable[] paramTables)
        {
            for (int x = 0; x < Gestures.Length; x++)
            {
                var leftHands = Gestures[x];
                for (int y = 0; y < Gestures.Length; y++)
                {
                    var rightHands = Gestures[y];
                    var state = layer.NewState("L:" + leftHands.label + " R:" + rightHands.label, x, y);
                    foreach (var paramTable in paramTables)
                    {
                        state.Drives(paramTable.param, paramTable.table[x, y]);   
                    }
                    layer.AnyTransitionsTo(state)
                        .When(layer.Av3().GestureLeft.IsEqualTo(leftHands.id))
                        .And(layer.Av3().GestureRight.IsEqualTo(rightHands.id));
                }
            }
        }

        private void DefineAnimationLayer(AacFlLayer layer, Animation[] animations, AacFlIntParameter exParam, AacFlIntParameter gestureParam, AacFlState.TrackingElement? trackingElement)
        {
            var entry = layer.NewState("Entry");
            if (trackingElement != null) {
                entry.TrackingTracks((AacFlState.TrackingElement)trackingElement);
            }

            var exStates = new List<AacFlState>();
            for (int i = 0; i < animations.Length; i++)
            {
                var anim = animations[i];
                var state = layer.NewState("Ex:"+anim.label)
                    .Shift(entry, -1, i);
                if (trackingElement != null) {
                    state.TrackingAnimates((AacFlState.TrackingElement)trackingElement);
                }
                if (anim.motionTime != null) {
                    state.MotionTime((AacFlFloatParameter) anim.motionTime);
                }
                state.WithAnimation(anim.clip);
                state.TransitionsTo(entry)
                    .WithTransitionDurationSeconds(anim.transitionDuration)
                    .When(exParam.IsEqualTo(0));
                entry.TransitionsTo(state)
                    .WithTransitionDurationSeconds(anim.transitionDuration)
                    .When(exParam.IsEqualTo(anim.id));
                for (int k = 0; k < i; k++)
                {
                    var transAnim = animations[k];
                    var transTarget = exStates[k];
                    state.TransitionsTo(transTarget)
                        .WithTransitionDurationSeconds(TransitionDuration(anim, transAnim))
                        .When(exParam.IsEqualTo(transAnim.id));
                    transTarget.TransitionsTo(state)
                        .WithTransitionDurationSeconds(TransitionDuration(anim, transAnim))
                        .When(exParam.IsEqualTo(anim.id));
                }
                exStates.Add(state);
            }

            var gestureStates = new List<AacFlState>();
            for (int i = 0; i < animations.Length; i++)
            {
                var anim = animations[i];
                var state = layer.NewState("Gesture:" + anim.label).Shift(entry, 1, i);
                if (trackingElement != null) {
                    state.TrackingAnimates((AacFlState.TrackingElement)trackingElement);
                }
                if (anim.motionTime != null) {
                    state.MotionTime((AacFlFloatParameter) anim.motionTime);
                }
                state.WithAnimation(anim.clip);
                state.TransitionsTo(entry)
                    .WithTransitionDurationSeconds(anim.transitionDuration)
                    .When(gestureParam.IsEqualTo(0));
                entry.TransitionsTo(state)
                    .WithTransitionDurationSeconds(anim.transitionDuration)
                    .When(gestureParam.IsEqualTo(anim.id));
                for (int k = 0; k < i; k++)
                {
                    var transAnim = animations[k];
                    var transTarget = gestureStates[k];
                    state.TransitionsTo(transTarget)
                        .WithTransitionDurationSeconds(TransitionDuration(anim, transAnim))
                        .When(gestureParam.IsEqualTo(transAnim.id));
                    transTarget.TransitionsTo(state)
                        .WithTransitionDurationSeconds(TransitionDuration(anim, transAnim))
                        .When(gestureParam.IsEqualTo(anim.id));
                }
                gestureStates.Add(state);
                for (int k = 0; k < exStates.Count; k++)
                {
                    var exAnim = animations[k];
                    state.TransitionsTo(exStates[k])
                        .WithTransitionDurationSeconds(TransitionDuration(anim, exAnim))
                        .When(exParam.IsEqualTo(exAnim.id));
                }
            }
        }

        private void DefineSimpleSwitchLayer(AacFlLayer layer, AacFlBoolParameter param, Motion clip)
        {
            var stateOff = layer.NewState("OFF", 1, 0);
            var stateOn = layer.NewState("ON", 1, 1).WithAnimation(clip);
            stateOff.TransitionsTo(stateOn).When(param.IsTrue());
            stateOn.TransitionsTo(stateOff).When(param.IsFalse());
        }

        private void Remove()
        {
            var my = (FXHands) target;
            var aac = AacFX.AnimatorAsCode(SystemName, my.avatar, my.assetContainer, my.assetKey);

            aac.RemoveAllMainLayers();
            aac.RemoveAllSupportingLayers("Eye");
            aac.RemoveAllSupportingLayers("EyeEffect");
        }

        private float TransitionDuration(Animation a, Animation b) {
            if (a.transitionDuration > b.transitionDuration) {
                return b.transitionDuration;
            }
            return a.transitionDuration;
        }
    }

    public class Animation : MonoBehaviour
    {
        public string label;
        public int id;
        public Motion clip;
        public float transitionDuration;
        public AacFlFloatParameter motionTime;
        public Animation(int _id, string _label, Motion _clip, float _transitionDuration, AacFlFloatParameter _motionTime)
        {
            id = _id;
            label = _label;
            clip = _clip;
            transitionDuration = _transitionDuration;
            motionTime = _motionTime;
        }
    }

    public class Gesture : MonoBehaviour
    {
        public string label;
        public int id;
        public Gesture(int _id, string _label)
        {
            id = _id;
            label = _label;
        }
    }

    public class FXHands : MonoBehaviour
    {
        public VRCAvatarDescriptor avatar;
        public AnimatorController assetContainer;
        public string assetKey;
        public GameObject item;
    }

    public class ParamTable : MonoBehaviour {
        public AacFlIntParameter param;
        public int[,] table;

        public ParamTable(AacFlIntParameter _param, int[,] _table)
        {
            param = _param;
            table = _table;
        }
    }
}
#endif