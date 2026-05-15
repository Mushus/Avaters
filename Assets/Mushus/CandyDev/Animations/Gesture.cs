using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mushus.AAC.FX
{
    public class Gesture
    {
        // ハンドサインの定義
        public static Gesture GestureNone = new Gesture(0, "None");
        public static Gesture GestureFist = new Gesture(1, "Fist");
        public static Gesture GestureOpen = new Gesture(2, "Open");
        public static Gesture GesturePoint = new Gesture(3, "Point");
        public static Gesture GesturePeace = new Gesture(4, "Peace");
        public static Gesture GestureRockNRoll = new Gesture(5, "RockNRoll");
        public static Gesture GestureGun = new Gesture(6, "Gun");
        public static Gesture GestureThumbsUp = new Gesture(7, "ThumbsUp");
        public static readonly Gesture[] Gestures = {
            GestureNone,
            GestureFist,
            GestureOpen,
            GesturePoint,
            GesturePeace,
            GestureRockNRoll,
            GestureGun,
            GestureThumbsUp,
        };

        public string label;
        public int id;
        private Gesture(int _id, string _label)
        {
            id = _id;
            label = _label;
        }
    }
}
