// このスクリプトによってFXレイヤーを作成しています
// スクリプトの使い方は下に記載しています

#if UNITY_EDITOR
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using AnimatorAsCode.V0;
using Mushus.AAC.FX;
using static VRC.SDKBase.VRC_AvatarDescriptor;

namespace Mushus.Candy.FX
{
    [CustomEditor(typeof(CandyFx), true)]
    public class CandyFxEditor : Editor
    {
        private const string SystemName = "Fx";

        public override void OnInspectorGUI()
        {
            AacInspector.InspectorTemplate(this, serializedObject, "assetKey", Create, Remove);
        }

        private void Create()
        {
            var my = (CandyFx)target;
            var aac = AacInspector.AnimatorAsCode(SystemName, my.avatar, my.assetContainer, my.assetKey);

            var fx = aac.CreateMainFxLayer();

            // 目のアニメーションの定義
            var eyeAnims = new Animation[] {
                new Animation(1, "EyeAngly", LoadMotion("Eyes/EyeAngly"), 0.05f, null),
                new Animation(2, "EyeClosed", LoadMotion("Eyes/EyeClosed"), 0.05f, null),
                new Animation(3, "EyeGuruguru", LoadMotion("Eyes/EyeGuruguru"), 0, null),
                new Animation(4, "EyeJitome", LoadMotion("Eyes/EyeJitome"), 0.05f, null),
                new Animation(5, "EyeMyu", LoadMotion("Eyes/EyeMyu"), 0, null),
                new Animation(6, "EyeSadness", LoadMotion("Eyes/EyeSadness"), 0.05f, null),
                new Animation(7, "EyeShirome", LoadMotion("Eyes/EyeShirome"), 0, null),
                new Animation(8, "EyeSmile", LoadMotion("Eyes/EyeSmile"), 0.05f, null),
                new Animation(9, "EyeSurprise", LoadMotion("Eyes/EyeSurprise"), 0.05f, null),
                new Animation(10, "EyeNagomi", LoadMotion("Eyes/EyeNagomi"), 0.05f, null),
                new Animation(20, "EyeDefault", LoadMotion("EmptyAnim"), 0, null),
            };

            // 目のアニメーション指の対応関係の定義
            var gestureEye = new int[,] {
                {0,  0, 2, 0, 8, 1, 4, 5}, // 0: None
                {0, 10, 2, 0, 8, 1, 4, 5}, // 1: Fist
                {0, 10, 9, 0, 8, 1, 4, 5}, // 2: Open
                {0, 10, 2, 0, 8, 1, 4, 5}, // 3: Point
                {0, 10, 2, 3, 5, 1, 4, 5}, // 4: Peace
                {0, 10, 2, 0, 8, 1, 4, 5}, // 5: RockNRoll
                {0, 10, 2, 0, 8, 6, 7, 5}, // 6: Gun
                {0, 10, 2, 0, 8, 6, 4, 5}, // 7: ThumbsUp
            }; //0, 1, 2, 3, 4, 5, 6, 7

            // 口のアニメーションの定義
            var mouthAnims = new Animation[] {
                new Animation(1, "MouthAngly", LoadMotion("Mouth/MouthAngly"), 0.05f, fx.Av3().Voice),
                new Animation(2, "MouthOpen", LoadMotion("Mouth/MouthOpen"), 0.05f, fx.Av3().Voice),
                new Animation(3, "MouthTongueOut", LoadMotion("Mouth/MouthTongueOut"), 0.05f, fx.Av3().Voice),
                new Animation(20, "Default", LoadMotion("EmptyAnim"), 0, null),
            };

            // 口のアニメーション指の対応関係の定義
            var gestureMouth = new int[,] {
                {0, 0, 0, 0, 0, 1, 0, 0}, // 0: None
                {0, 0, 0, 0, 0, 1, 0, 0}, // 1: Fist
                {0, 0, 0, 0, 0, 1, 0, 0}, // 2: Open
                {0, 0, 0, 0, 0, 1, 0, 0}, // 3: Point
                {0, 0, 0, 0, 0, 1, 0, 0}, // 4: Peace
                {0, 0, 0, 0, 0, 0, 0, 0}, // 5: RockNRoll
                {0, 0, 0, 0, 0, 1, 0, 0}, // 6: Gun
                {0, 0, 0, 0, 0, 0, 0, 2}, // 7: ThumbsUp
            }; //0, 1, 2, 3, 4, 5, 6, 7

            var handsIndex = fx.IntParameter("HandsIndex");
            var eyeGesture = fx.IntParameter("EyeGesture");
            var mouthGesture = fx.IntParameter("MouthGesture");

            var paramTables = new ParamTable[] {
                new ParamTable(eyeGesture, gestureEye),
                new ParamTable(mouthGesture, gestureMouth),
            };
            DefineGestureLayer(fx, handsIndex);

            var eyeLayer = aac.CreateSupportingFxLayer("Eye");
            DefineAnimationLayer(eyeLayer, eyeAnims, gestureEye, eyeGesture, handsIndex, AacFlState.TrackingElement.Eyes);

            var mouthLayer = aac.CreateSupportingFxLayer("Mouth");
            DefineAnimationLayer(mouthLayer, mouthAnims, gestureMouth, mouthGesture, handsIndex, null);
        }

        private Motion LoadMotion(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Motion>("Assets/Mushus/Candy/Animations/" + path + ".anim");
        }

        private void DefineGestureLayer(AacFlLayer layer, AacFlIntParameter handsIndex)
        {
            for (int y = 0; y < Gesture.Gestures.Length; y++)
            {
                var leftHands = Gesture.Gestures[y];
                for (int x = 0; x < Gesture.Gestures.Length; x++)
                {
                    var index = x + y * Gesture.Gestures.Length;
                    var rightHands = Gesture.Gestures[x];
                    var state = layer.NewState("L:" + leftHands.label + " R:" + rightHands.label, y, x);
                    state.Drives(handsIndex, index);
                    layer.AnyTransitionsTo(state)
                        .When(layer.Av3().GestureLeft.IsEqualTo(leftHands.id))
                        .And(layer.Av3().GestureRight.IsEqualTo(rightHands.id));
                }
            }
        }

        private void DefineAnimationLayer(AacFlLayer layer, Animation[] animations, int[,] gestureMap, AacFlIntParameter exParam, AacFlIntParameter handsIndex, AacFlState.TrackingElement? trackingElement)
        {
            var entry = layer.NewState("Entry");
            if (trackingElement != null)
            {
                entry.TrackingTracks((AacFlState.TrackingElement)trackingElement);
            }

            var yLength = gestureMap.GetLength(0);
            var xLength = gestureMap.GetLength(1);
            for (int i = 0; i < animations.Length; i++)
            {
                var animation = animations[i];
                var state = layer.NewState(animation.label, 1, i)
                    .WithAnimation(animation.clip);
                if (trackingElement != null)
                {
                    state.TrackingAnimates((AacFlState.TrackingElement)trackingElement);
                }
                if (animation.motionTime != null)
                {
                    state.MotionTime((AacFlFloatParameter)animation.motionTime);
                }

                var fromEntryTransition = entry.TransitionsTo(state)
                    .WithTransitionDurationSeconds(animation.transitionDuration)
                    .When(exParam.IsEqualTo(animation.id));
                var toEntryTransition = state.TransitionsTo(entry)
                    .WithTransitionDurationSeconds(animation.transitionDuration)
                    .When(exParam.IsNotEqualTo(animation.id));
                
                for (int y = 0; y < yLength; y++)
                {
                    for (int x = 0; x < xLength; x++)
                    {
                        if (animation.id != gestureMap[y, x])
                        {
                            continue;
                        }
                        var index = x + y * xLength;
                        fromEntryTransition.Or().When(handsIndex.IsEqualTo(index));
                        toEntryTransition.And(handsIndex.IsNotEqualTo(index));
                    }
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
            var my = (CandyFx)target;
            var aac = AacInspector.AnimatorAsCode(SystemName, my.avatar, my.assetContainer, my.assetKey);

            aac.RemoveAllMainLayers();
            aac.RemoveAllSupportingLayers("Eye");
            aac.RemoveAllSupportingLayers("Mouth");
        }

        private float TransitionDuration(Animation a, Animation b)
        {
            if (a.transitionDuration > b.transitionDuration)
            {
                return b.transitionDuration;
            }
            return a.transitionDuration;
        }
    }

    public class Animation
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


    public class CandyFx : MonoBehaviour
    {
        public VRCAvatarDescriptor avatar;
        public AnimatorController assetContainer;
        public string assetKey;
    }

    public class ParamTable
    {
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
