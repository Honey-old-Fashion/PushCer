using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace VeryAnimation
{
    public partial class VeryAnimation
    {
        private static readonly string[] DofIndex2String =
        {
            ".x", ".y", ".z", ".w"
        };
        private static int[] QuaternionXMirrorSwapDof = new int[] { 2, 3, 0, 1 };
        public enum AnimatorIKIndex
        {
            None = -1,
            LeftHand,
            RightHand,
            LeftFoot,
            RightFoot,
            Total
        }
        public static readonly string[] AnimatorIKTIndexStrings =
        {
            AnimatorIKIndex.LeftHand.ToString() + "T.",
            AnimatorIKIndex.RightHand.ToString() + "T.",
            AnimatorIKIndex.LeftFoot.ToString() + "T.",
            AnimatorIKIndex.RightFoot.ToString() + "T.",
        };
        public static readonly string[] AnimatorIKQIndexStrings =
        {
            AnimatorIKIndex.LeftHand.ToString() + "Q.",
            AnimatorIKIndex.RightHand.ToString() + "Q.",
            AnimatorIKIndex.LeftFoot.ToString() + "Q.",
            AnimatorIKIndex.RightFoot.ToString() + "Q.",
        };
        public static readonly AnimatorIKIndex[] AnimatorIKMirrorIndexes =
        {
            AnimatorIKIndex.RightHand,
            AnimatorIKIndex.LeftHand,
            AnimatorIKIndex.RightFoot,
            AnimatorIKIndex.LeftFoot,
        };
        public static readonly HumanBodyBones[] AnimatorIKIndex2HumanBodyBones =
        {
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
        };
        public enum AnimatorTDOFIndex
        {
            None = -1,
            LeftUpperLeg,
            RightUpperLeg,
            Spine,
            Chest,
            Neck,
            LeftShoulder,
            RightShoulder,
            UpperChest,
#if UNITY_2017_3_OR_NEWER
            LeftLowerLeg,
            RightLowerLeg,
            LeftFoot,
            RightFoot,
            Head,
            LeftUpperArm,
            RightUpperArm,
            LeftLowerArm,
            RightLowerArm,
            LeftHand,
            RightHand,
            LeftToes,
            RightToes,
#endif
            Total
        }
        public static readonly string[] AnimatorTDOFIndexStrings =
        {
            AnimatorTDOFIndex.LeftUpperLeg.ToString(),
            AnimatorTDOFIndex.RightUpperLeg.ToString(),
            AnimatorTDOFIndex.Spine.ToString(),
            AnimatorTDOFIndex.Chest.ToString(),
            AnimatorTDOFIndex.Neck.ToString(),
            AnimatorTDOFIndex.LeftShoulder.ToString(),
            AnimatorTDOFIndex.RightShoulder.ToString(),
            AnimatorTDOFIndex.UpperChest.ToString(),
#if UNITY_2017_3_OR_NEWER
            AnimatorTDOFIndex.LeftLowerLeg.ToString(),
            AnimatorTDOFIndex.RightLowerLeg.ToString(),
            AnimatorTDOFIndex.LeftFoot.ToString(),
            AnimatorTDOFIndex.RightFoot.ToString(),
            AnimatorTDOFIndex.Head.ToString(),
            AnimatorTDOFIndex.LeftUpperArm.ToString(),
            AnimatorTDOFIndex.RightUpperArm.ToString(),
            AnimatorTDOFIndex.LeftLowerArm.ToString(),
            AnimatorTDOFIndex.RightLowerArm.ToString(),
            AnimatorTDOFIndex.LeftHand.ToString(),
            AnimatorTDOFIndex.RightHand.ToString(),
            AnimatorTDOFIndex.LeftToes.ToString(),
            AnimatorTDOFIndex.RightToes.ToString(),
#endif
        };
        public static readonly AnimatorTDOFIndex[] AnimatorTDOFMirrorIndexes =
        {
            AnimatorTDOFIndex.RightUpperLeg,
            AnimatorTDOFIndex.LeftUpperLeg,
            AnimatorTDOFIndex.None,
            AnimatorTDOFIndex.None,
            AnimatorTDOFIndex.None,
            AnimatorTDOFIndex.RightShoulder,
            AnimatorTDOFIndex.LeftShoulder,
            AnimatorTDOFIndex.None,
#if UNITY_2017_3_OR_NEWER
            AnimatorTDOFIndex.RightLowerLeg,
            AnimatorTDOFIndex.LeftLowerLeg,
            AnimatorTDOFIndex.RightFoot,
            AnimatorTDOFIndex.LeftFoot,
            AnimatorTDOFIndex.None,
            AnimatorTDOFIndex.RightUpperArm,
            AnimatorTDOFIndex.LeftUpperArm,
            AnimatorTDOFIndex.RightLowerArm,
            AnimatorTDOFIndex.LeftLowerArm,
            AnimatorTDOFIndex.RightHand,
            AnimatorTDOFIndex.LeftHand,
            AnimatorTDOFIndex.RightToes,
            AnimatorTDOFIndex.LeftToes,
#endif
        };
        public static readonly HumanBodyBones[] AnimatorTDOFIndex2HumanBodyBones =
        {
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.Neck,
            HumanBodyBones.LeftShoulder,
            HumanBodyBones.RightShoulder,
            HumanBodyBones.UpperChest,
#if UNITY_2017_3_OR_NEWER
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
            HumanBodyBones.Head,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftToes,
            HumanBodyBones.RightToes,
#endif
        };

        public static readonly HumanBodyBones[] HumanBodyMirrorBones =
        {
            (HumanBodyBones)(-1),           //Hips = 0,
            HumanBodyBones.RightUpperLeg,   //LeftUpperLeg = 1,
            HumanBodyBones.LeftUpperLeg,    //RightUpperLeg = 2,
            HumanBodyBones.RightLowerLeg,   //LeftLowerLeg = 3,
            HumanBodyBones.LeftLowerLeg,    //RightLowerLeg = 4,
            HumanBodyBones.RightFoot,       //LeftFoot = 5,
            HumanBodyBones.LeftFoot,        //RightFoot = 6,
            (HumanBodyBones)(-1),           //Spine = 7,
            (HumanBodyBones)(-1),           //Chest = 8,
            (HumanBodyBones)(-1),           //Neck = 9,
            (HumanBodyBones)(-1),           //Head = 10,
            HumanBodyBones.RightShoulder,   //LeftShoulder = 11,
            HumanBodyBones.LeftShoulder,    //RightShoulder = 12,
            HumanBodyBones.RightUpperArm,   //LeftUpperArm = 13,
            HumanBodyBones.LeftUpperArm,    //RightUpperArm = 14,
            HumanBodyBones.RightLowerArm,   //LeftLowerArm = 15,
            HumanBodyBones.LeftLowerArm,    //RightLowerArm = 16,
            HumanBodyBones.RightHand,       //LeftHand = 17,
            HumanBodyBones.LeftHand,        //RightHand = 18,
            HumanBodyBones.RightToes,       //LeftToes = 19,
            HumanBodyBones.LeftToes,        //RightToes = 20,
            HumanBodyBones.RightEye,        //LeftEye = 21,
            HumanBodyBones.LeftEye,         //RightEye = 22,
            (HumanBodyBones)(-1),           //Jaw = 23,
            HumanBodyBones.RightThumbProximal,      //LeftThumbProximal = 24,
            HumanBodyBones.RightThumbIntermediate,  //LeftThumbIntermediate = 25,
            HumanBodyBones.RightThumbDistal,        //LeftThumbDistal = 26,
            HumanBodyBones.RightIndexProximal,      //LeftIndexProximal = 27,
            HumanBodyBones.RightIndexIntermediate,  //LeftIndexIntermediate = 28,
            HumanBodyBones.RightIndexDistal,        //LeftIndexDistal = 29,
            HumanBodyBones.RightMiddleProximal,     //LeftMiddleProximal = 30,
            HumanBodyBones.RightMiddleIntermediate, //LeftMiddleIntermediate = 31,
            HumanBodyBones.RightMiddleDistal,       //LeftMiddleDistal = 32,
            HumanBodyBones.RightRingProximal,       //LeftRingProximal = 33,
            HumanBodyBones.RightRingIntermediate,   //LeftRingIntermediate = 34,
            HumanBodyBones.RightRingDistal,         //LeftRingDistal = 35,
            HumanBodyBones.RightLittleProximal,     //LeftLittleProximal = 36,
            HumanBodyBones.RightLittleIntermediate, //LeftLittleIntermediate = 37,
            HumanBodyBones.RightLittleDistal,       //LeftLittleDistal = 38,
            HumanBodyBones.LeftThumbProximal,       //RightThumbProximal = 39,
            HumanBodyBones.LeftThumbIntermediate,   //RightThumbIntermediate = 40,
            HumanBodyBones.LeftThumbDistal,         //RightThumbDistal = 41,
            HumanBodyBones.LeftIndexProximal,       //RightIndexProximal = 42,
            HumanBodyBones.LeftIndexIntermediate,   //RightIndexIntermediate = 43,
            HumanBodyBones.LeftIndexDistal,         //RightIndexDistal = 44,
            HumanBodyBones.LeftMiddleProximal,      //RightMiddleProximal = 45,
            HumanBodyBones.LeftMiddleIntermediate,  //RightMiddleIntermediate = 46,
            HumanBodyBones.LeftMiddleDistal,        //RightMiddleDistal = 47,
            HumanBodyBones.LeftRingProximal,        //RightRingProximal = 48,
            HumanBodyBones.LeftRingIntermediate,    //RightRingIntermediate = 49,
            HumanBodyBones.LeftRingDistal,          //RightRingDistal = 50,
            HumanBodyBones.LeftLittleProximal,      //RightLittleProximal = 51,
            HumanBodyBones.LeftLittleIntermediate,  //RightLittleIntermediate = 52,
            HumanBodyBones.LeftLittleDistal,        //RightLittleDistal = 53,
            (HumanBodyBones)(-1),                   //UpperChest = 54,
        };

        public class HumanVirtualBone
        {
            public HumanBodyBones boneA;
            public HumanBodyBones boneB;
            public float leap;
            public Quaternion addRotation = Quaternion.identity;
            public Vector3 limitSign = Vector3.one;
        }
        public static readonly HumanVirtualBone[][] HumanVirtualBones =
        {
            null, //Hips = 0,
            null, //LeftUpperLeg = 1,
            null, //RightUpperLeg = 2,
            null, //LeftLowerLeg = 3,
            null, //RightLowerLeg = 4,
            null, //LeftFoot = 5,
            null, //RightFoot = 6,
            null, //Spine = 7,
            new HumanVirtualBone[] { new HumanVirtualBone() { boneA = HumanBodyBones.Spine, boneB = HumanBodyBones.Head, leap = 0.15f } }, //Chest = 8,
            new HumanVirtualBone[] { new HumanVirtualBone() { boneA = HumanBodyBones.UpperChest, boneB = HumanBodyBones.Head, leap = 0.8f },
                                        new HumanVirtualBone() { boneA = HumanBodyBones.Chest, boneB = HumanBodyBones.Head, leap = 0.8f },
                                        new HumanVirtualBone() { boneA = HumanBodyBones.Spine, boneB = HumanBodyBones.Head, leap = 0.85f } }, //Neck = 9,
            null, //Head = 10,
            new HumanVirtualBone[] { new HumanVirtualBone() { boneA = HumanBodyBones.LeftUpperArm, boneB = HumanBodyBones.RightUpperArm, leap = 0.2f, limitSign = new Vector3(1f, 1f, -1f) } }, //LeftShoulder = 11,
            new HumanVirtualBone[] { new HumanVirtualBone() { boneA = HumanBodyBones.RightUpperArm, boneB = HumanBodyBones.LeftUpperArm, leap = 0.2f } }, //RightShoulder = 12,
            null, //LeftUpperArm = 13,
            null, //RightUpperArm = 14,
            null, //LeftLowerArm = 15,
            null, //RightLowerArm = 16,
            null, //LeftHand = 17,
            null, //RightHand = 18,
            null, //LeftToes = 19,
            null, //RightToes = 20,
            null, //LeftEye = 21,
            null, //RightEye = 22,
            null, //Jaw = 23,
            null, //LeftThumbProximal = 24,
            null, //LeftThumbIntermediate = 25,
            null, //LeftThumbDistal = 26,
            null, //LeftIndexProximal = 27,
            null, //LeftIndexIntermediate = 28,
            null, //LeftIndexDistal = 29,
            null, //LeftMiddleProximal = 30,
            null, //LeftMiddleIntermediate = 31,
            null, //LeftMiddleDistal = 32,
            null, //LeftRingProximal = 33,
            null, //LeftRingIntermediate = 34,
            null, //LeftRingDistal = 35,
            null, //LeftLittleProximal = 36,
            null, //LeftLittleIntermediate = 37,
            null, //LeftLittleDistal = 38,
            null, //RightThumbProximal = 39,
            null, //RightThumbIntermediate = 40,
            null, //RightThumbDistal = 41,
            null, //RightIndexProximal = 42,
            null, //RightIndexIntermediate = 43,
            null, //RightIndexDistal = 44,
            null, //RightMiddleProximal = 45,
            null, //RightMiddleIntermediate = 46,
            null, //RightMiddleDistal = 47,
            null, //RightRingProximal = 48,
            null, //RightRingIntermediate = 49,
            null, //RightRingDistal = 50,
            null, //RightLittleProximal = 51,
            null, //RightLittleIntermediate = 52,
            null, //RightLittleDistal = 53,
            new HumanVirtualBone[] { new HumanVirtualBone() { boneA = HumanBodyBones.Chest, boneB = HumanBodyBones.Head, leap = 0.2f },
                                        new HumanVirtualBone() { boneA = HumanBodyBones.Spine, boneB = HumanBodyBones.Head, leap = 0.3f } }, //UpperChest = 54,
        };

        public class AnimatorTDOF
        {
            public AnimatorTDOFIndex index;
            public HumanBodyBones parent;
            public Vector3 mirror = new Vector3(1f, 1f, -1f);
        }
#if UNITY_2017_3_OR_NEWER
        public static readonly AnimatorTDOF[] HumanBonesAnimatorTDOFIndex =
        {
            null, //Hips = 0,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftUpperLeg, parent = HumanBodyBones.Hips }, //LeftUpperLeg = 1,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightUpperLeg, parent = HumanBodyBones.Hips }, //RightUpperLeg = 2,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftLowerLeg, parent = HumanBodyBones.LeftUpperLeg }, //LeftLowerLeg = 3,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightLowerLeg, parent = HumanBodyBones.RightUpperLeg }, //RightLowerLeg = 4,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftFoot, parent = HumanBodyBones.LeftLowerLeg }, //LeftFoot = 5,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightFoot, parent = HumanBodyBones.RightLowerLeg }, //RightFoot = 6,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.Spine, parent = HumanBodyBones.Hips }, //Spine = 7,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.Chest, parent = HumanBodyBones.Spine }, //Chest = 8,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.Neck, parent = HumanBodyBones.UpperChest }, //Neck = 9,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.Head, parent = HumanBodyBones.Neck }, //Head = 10,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftShoulder, parent = HumanBodyBones.Chest }, //LeftShoulder = 11,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightShoulder, parent = HumanBodyBones.Chest }, //RightShoulder = 12,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftUpperArm, parent = HumanBodyBones.LeftShoulder, mirror = new Vector3(1f, -1f, 1f) }, //LeftUpperArm = 13,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightUpperArm, parent = HumanBodyBones.RightShoulder, mirror = new Vector3(1f, -1f, 1f) }, //RightUpperArm = 14,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftLowerArm, parent = HumanBodyBones.LeftUpperArm, mirror = new Vector3(1f, -1f, 1f)  }, //LeftLowerArm = 15,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightLowerArm, parent = HumanBodyBones.RightUpperArm, mirror = new Vector3(1f, -1f, 1f)  }, //RightLowerArm = 16,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftHand, parent = HumanBodyBones.LeftLowerArm, mirror = new Vector3(1f, -1f, 1f)  }, //LeftHand = 17,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightHand, parent = HumanBodyBones.RightLowerArm, mirror = new Vector3(1f, -1f, 1f)  }, //RightHand = 18,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftToes, parent = HumanBodyBones.LeftFoot }, //LeftToes = 19,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightToes, parent = HumanBodyBones.RightFoot }, //RightToes = 20,
            null, //LeftEye = 21,
            null, //RightEye = 22,
            null, //Jaw = 23,
            null, //LeftThumbProximal = 24,
            null, //LeftThumbIntermediate = 25,
            null, //LeftThumbDistal = 26,
            null, //LeftIndexProximal = 27,
            null, //LeftIndexIntermediate = 28,
            null, //LeftIndexDistal = 29,
            null, //LeftMiddleProximal = 30,
            null, //LeftMiddleIntermediate = 31,
            null, //LeftMiddleDistal = 32,
            null, //LeftRingProximal = 33,
            null, //LeftRingIntermediate = 34,
            null, //LeftRingDistal = 35,
            null, //LeftLittleProximal = 36,
            null, //LeftLittleIntermediate = 37,
            null, //LeftLittleDistal = 38,
            null, //RightThumbProximal = 39,
            null, //RightThumbIntermediate = 40,
            null, //RightThumbDistal = 41,
            null, //RightIndexProximal = 42,
            null, //RightIndexIntermediate = 43,
            null, //RightIndexDistal = 44,
            null, //RightMiddleProximal = 45,
            null, //RightMiddleIntermediate = 46,
            null, //RightMiddleDistal = 47,
            null, //RightRingProximal = 48,
            null, //RightRingIntermediate = 49,
            null, //RightRingDistal = 50,
            null, //RightLittleProximal = 51,
            null, //RightLittleIntermediate = 52,
            null, //RightLittleDistal = 53,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.UpperChest, parent = HumanBodyBones.Chest }, //UpperChest = 54,
        };
#else
        public static readonly AnimatorTDOF[] HumanBonesAnimatorTDOFIndex =
        {
            null, //Hips = 0,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftUpperLeg, parent = HumanBodyBones.Hips }, //LeftUpperLeg = 1,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightUpperLeg, parent = HumanBodyBones.Hips }, //RightUpperLeg = 2,
            null, //LeftLowerLeg = 3,
            null, //RightLowerLeg = 4,
            null, //LeftFoot = 5,
            null, //RightFoot = 6,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.Spine, parent = HumanBodyBones.Hips }, //Spine = 7,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.Chest, parent = HumanBodyBones.Spine }, //Chest = 8,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.Neck, parent = HumanBodyBones.UpperChest }, //Neck = 9,
            null, //Head = 10,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.LeftShoulder, parent = HumanBodyBones.Chest }, //LeftShoulder = 11,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.RightShoulder, parent = HumanBodyBones.Chest }, //RightShoulder = 12,
            null, //LeftUpperArm = 13,
            null, //RightUpperArm = 14,
            null, //LeftLowerArm = 15,
            null, //RightLowerArm = 16,
            null, //LeftHand = 17,
            null, //RightHand = 18,
            null, //LeftToes = 19,
            null, //RightToes = 20,
            null, //LeftEye = 21,
            null, //RightEye = 22,
            null, //Jaw = 23,
            null, //LeftThumbProximal = 24,
            null, //LeftThumbIntermediate = 25,
            null, //LeftThumbDistal = 26,
            null, //LeftIndexProximal = 27,
            null, //LeftIndexIntermediate = 28,
            null, //LeftIndexDistal = 29,
            null, //LeftMiddleProximal = 30,
            null, //LeftMiddleIntermediate = 31,
            null, //LeftMiddleDistal = 32,
            null, //LeftRingProximal = 33,
            null, //LeftRingIntermediate = 34,
            null, //LeftRingDistal = 35,
            null, //LeftLittleProximal = 36,
            null, //LeftLittleIntermediate = 37,
            null, //LeftLittleDistal = 38,
            null, //RightThumbProximal = 39,
            null, //RightThumbIntermediate = 40,
            null, //RightThumbDistal = 41,
            null, //RightIndexProximal = 42,
            null, //RightIndexIntermediate = 43,
            null, //RightIndexDistal = 44,
            null, //RightMiddleProximal = 45,
            null, //RightMiddleIntermediate = 46,
            null, //RightMiddleDistal = 47,
            null, //RightRingProximal = 48,
            null, //RightRingIntermediate = 49,
            null, //RightRingDistal = 50,
            null, //RightLittleProximal = 51,
            null, //RightLittleIntermediate = 52,
            null, //RightLittleDistal = 53,
            new AnimatorTDOF() { index = AnimatorTDOFIndex.UpperChest, parent = HumanBodyBones.Chest }, //UpperChest = 54,
        };
#endif
        public static readonly HumanBodyBones[] HumanPoseHaveMassBones =
        {
            HumanBodyBones.Hips,
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightFoot,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,
            HumanBodyBones.LeftShoulder,
            HumanBodyBones.RightShoulder,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftToes,
            HumanBodyBones.RightToes,
            HumanBodyBones.LeftEye,
            HumanBodyBones.RightEye,
            HumanBodyBones.Jaw,
        };

        #region WeightUpdateFrame
        private class WeightUpdateFrame
        {
            public WeightUpdateFrame()
            {
                frames = new Dictionary<int, float>();
            }
            public void Add(int frame, float weight)
            {
                float outWeight;
                if (frames.TryGetValue(frame, out outWeight))
                {
                    if (Mathf.Abs(outWeight) > Mathf.Abs(weight))
                        frames[frame] = weight;
                }
                else
                {
                    frames.Add(frame, weight);
                }
            }
            public void Clear()
            {
                frames.Clear();
            }
            public bool IsEmpty()
            {
                return frames.Count == 0;
            }

            public Dictionary<int, float> frames { get; private set; }
        }
        #endregion

        #region AnimatorRootCorrection
        private class AnimatorRootCorrection
        {
            public bool update;
            public bool disable;
            public int[] muscleIndexes;
            public AnimationCurve[] rootTCurves = new AnimationCurve[3];
            public AnimationCurve[] rootQCurves = new AnimationCurve[4];
            public AnimationCurve[] muscleCurves;
            //Save
            [Serializable, System.Diagnostics.DebuggerDisplay("Position({position}), Rotation({rotation})")]
            public struct TransformSave
            {
                public Vector3 position;
                public Quaternion rotation;
            }
            public List<TransformSave> hipSaves = new List<TransformSave>();
            public List<TransformSave> rootSaves = new List<TransformSave>();
            public List<float>[] muscleValueSaves;

            public TransformSave[] frameRootSaves;

            public HumanPose humanPose;

            public WeightUpdateFrame updateFrame = new WeightUpdateFrame();
        }
        private AnimatorRootCorrection updateAnimatorRootCorrection;
        private void InitializeAnimatorRootCorrection()
        {
            if (!isHuman) return;

            updateAnimatorRootCorrection = new AnimatorRootCorrection();

            {
                List<int> muscles = new List<int>();
                for (int i = 0; i < HumanPoseHaveMassBones.Length; i++)
                {
                    for (int dof = 0; dof < 3; dof++)
                    {
                        var muscleIndex = HumanTrait.MuscleFromBone((int)HumanPoseHaveMassBones[i], dof);
                        if (muscleIndex >= 0)
                            muscles.Add(muscleIndex);
                    }
                }
                updateAnimatorRootCorrection.muscleIndexes = muscles.ToArray();
            }
            updateAnimatorRootCorrection.muscleCurves = new AnimationCurve[updateAnimatorRootCorrection.muscleIndexes.Length];
            updateAnimatorRootCorrection.muscleValueSaves = new List<float>[updateAnimatorRootCorrection.muscleIndexes.Length];
            for (int i = 0; i < updateAnimatorRootCorrection.muscleValueSaves.Length; i++)
                updateAnimatorRootCorrection.muscleValueSaves[i] = new List<float>();
            updateAnimatorRootCorrection.humanPose.muscles = new float[HumanTrait.MuscleCount];
        }
        private void ReleaseAnimatorRootCorrection()
        {
            updateAnimatorRootCorrection = null;
        }
        private void EnableAnimatorRootCorrection(AnimationCurve curve, int keyIndex)
        {
            if (!isHuman) return;
            if (rootCorrectionMode == RootCorrectionMode.Disable) return;
            if (keyIndex < 0 || keyIndex >= curve.length) return;

            var currentTime = curve[keyIndex].time;
            var beforeTime = 0f;
            var afterTime = currentClip.length;
            if (rootCorrectionMode == RootCorrectionMode.Full)
            {
                if (keyIndex > 0)
                    beforeTime = curve[keyIndex - 1].time;
                if (keyIndex + 1 < curve.length)
                    afterTime = curve[keyIndex + 1].time;
            }
            EnableAnimatorRootCorrection(currentTime, beforeTime, afterTime);
        }
        private void EnableAnimatorRootCorrection(float currentTime, float beforeTime, float afterTime)
        {
            if (!isHuman) return;
            if (rootCorrectionMode == RootCorrectionMode.Disable) return;

            updateAnimatorRootCorrection.update = true;

            var currentFrame = uAw.TimeToFrameRound(currentTime);
            updateAnimatorRootCorrection.updateFrame.Add(currentFrame, 0f);

            if (rootCorrectionMode == RootCorrectionMode.Full)
            {
                var beforeFrame = uAw.TimeToFrameRound(beforeTime);
                var afterFrame = uAw.TimeToFrameRound(afterTime);
                updateAnimatorRootCorrection.updateFrame.Add(beforeFrame, 1f);
                updateAnimatorRootCorrection.updateFrame.Add(afterFrame, -1f);
                for (int frame = currentFrame - 1; frame > beforeFrame; frame--)
                {
                    updateAnimatorRootCorrection.updateFrame.Add(frame, 0f);
                }
                for (int frame = currentFrame + 1; frame < afterFrame; frame++)
                {
                    updateAnimatorRootCorrection.updateFrame.Add(frame, 0f);
                }
            }
        }
        private void DisableAnimatorRootCorrection()
        {
            if (!isHuman) return;
            updateAnimatorRootCorrection.disable = true;
        }
        private void ResetAnimatorRootCorrection()
        {
            if (!isHuman) return;
            updateAnimatorRootCorrection.update = false;
            updateAnimatorRootCorrection.disable = false;
            updateAnimatorRootCorrection.updateFrame.Clear();
        }
        private void SaveAnimatorRootCorrection()
        {
            if (!isHuman) return;

            UpdateSyncEditorCurveClip();

            #region Clear
            {
                updateAnimatorRootCorrection.hipSaves.Clear();
                updateAnimatorRootCorrection.rootSaves.Clear();
                foreach (var saves in updateAnimatorRootCorrection.muscleValueSaves)
                    saves.Clear();
            }
            #endregion

            if (!humanoidHasTDoF)
            {
                #region Not TDoF
                Assert.IsTrue(calcObject.isTransformOrigin);
                for (int i = 0; i < 3; i++)
                    updateAnimatorRootCorrection.rootTCurves[i] = GetEditorCurveCache(AnimationCurveBindingAnimatorRootT[i]);
                for (int i = 0; i < 4; i++)
                    updateAnimatorRootCorrection.rootQCurves[i] = GetEditorCurveCache(AnimationCurveBindingAnimatorRootQ[i]);
                for (int i = 0; i < updateAnimatorRootCorrection.muscleIndexes.Length; i++)
                    updateAnimatorRootCorrection.muscleCurves[i] = GetEditorCurveCache(AnimationCurveBindingAnimatorMuscle(updateAnimatorRootCorrection.muscleIndexes[i]));
                for (int frame = 0; frame <= GetLastFrame(); frame++)
                {
                    var time = GetFrameTime(frame);
                    var rootT = new Vector3(updateAnimatorRootCorrection.rootTCurves[0] != null ? updateAnimatorRootCorrection.rootTCurves[0].Evaluate(time) : 0f,
                                            updateAnimatorRootCorrection.rootTCurves[1] != null ? updateAnimatorRootCorrection.rootTCurves[1].Evaluate(time) : 0f,
                                            updateAnimatorRootCorrection.rootTCurves[2] != null ? updateAnimatorRootCorrection.rootTCurves[2].Evaluate(time) : 0f);
                    Quaternion rootQ = Quaternion.identity;
                    {
                        Vector4 result = new Vector4(0, 0, 0, 1);
                        for (int i = 0; i < 4; i++)
                        {
                            if (updateAnimatorRootCorrection.rootQCurves[i] != null)
                                result[i] = updateAnimatorRootCorrection.rootQCurves[i].Evaluate(time);
                        }
                        if (result.sqrMagnitude > 0f)
                        {
                            result.Normalize();
                            rootQ = new Quaternion(result.x, result.y, result.z, result.w);
                        }
                    }
                    updateAnimatorRootCorrection.rootSaves.Add(new AnimatorRootCorrection.TransformSave()
                    {
                        position = rootT,
                        rotation = rootQ,
                    });
                    for (int i = 0; i < updateAnimatorRootCorrection.muscleIndexes.Length; i++)
                    {
                        var curve = updateAnimatorRootCorrection.muscleCurves[i];
                        updateAnimatorRootCorrection.muscleValueSaves[i].Add(curve != null ? curve.Evaluate(time) : 0f);
                    }
                }
                #endregion
            }
            else
            {
                #region Has TDoF
                calcObject.gameObject.SetActive(true);
                calcObject.vaEdit.SetAnimationClip(currentClip);
                calcObject.AnimatorRebind();
                var tHip = calcObject.humanoidHipsTransform;
                for (int frame = 0; frame <= GetLastFrame(); frame++)
                {
                    var time = GetFrameTime(frame);
                    currentClip.SampleAnimation(calcObject.gameObject, time);
                    updateAnimatorRootCorrection.hipSaves.Add(new AnimatorRootCorrection.TransformSave()
                    {
                        position = tHip.position,
                        rotation = (tHip.rotation * humanoidPoseHipRotation) * humanoidPreHipRotationInverse,
                    });
                }
                calcObject.gameObject.SetActive(false);
                #endregion
            }
        }
        private void UpdateAnimatorRootCorrection()
        {
            if (isHuman &&
                rootCorrectionMode != RootCorrectionMode.Disable &&
                !updatePoseFixAnimation &&
                updateAnimatorRootCorrection.update &&
                !updateAnimatorRootCorrection.disable &&
                !updateAnimatorRootCorrection.updateFrame.IsEmpty())
            {
                Undo.RegisterCompleteObjectUndo(currentClip, "Change Root");

                UpdateSyncEditorCurveClip();

                #region Chache
                {
                    for (int i = 0; i < 3; i++)
                    {
                        updateAnimatorRootCorrection.rootTCurves[i] = GetAnimationCurveAnimatorRootT(i);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        updateAnimatorRootCorrection.rootQCurves[i] = GetAnimationCurveAnimatorRootQ(i);
                    }
                    #region FrameRootSaves
                    {
                        var lastFrame = GetLastFrame();
                        if (updateAnimatorRootCorrection.frameRootSaves == null || updateAnimatorRootCorrection.frameRootSaves.Length < lastFrame + 1)
                        {
                            updateAnimatorRootCorrection.frameRootSaves = new AnimatorRootCorrection.TransformSave[lastFrame + 1];
                        }
                        foreach (var pair in updateAnimatorRootCorrection.updateFrame.frames)
                        {
                            var frame = pair.Key;
                            if (frame > lastFrame)
                                frame = lastFrame;
                            var time = GetFrameTime(frame);

                            updateAnimatorRootCorrection.frameRootSaves[frame].position = new Vector3(updateAnimatorRootCorrection.rootTCurves[0].Evaluate(time),
                                                                                                        updateAnimatorRootCorrection.rootTCurves[1].Evaluate(time),
                                                                                                        updateAnimatorRootCorrection.rootTCurves[2].Evaluate(time));
                            {
                                Vector4 result = new Vector4(0, 0, 0, 1);
                                for (int i = 0; i < 4; i++)
                                    result[i] = updateAnimatorRootCorrection.rootQCurves[i].Evaluate(time);
                                if (result.sqrMagnitude > 0f)
                                {
                                    result.Normalize();
                                    updateAnimatorRootCorrection.frameRootSaves[frame].rotation = new Quaternion(result.x, result.y, result.z, result.w);
                                }
                                else
                                {
                                    updateAnimatorRootCorrection.frameRootSaves[frame].rotation = Quaternion.identity;
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion
                
                if (!humanoidHasTDoF)
                {
                    #region Not TDoF
                    #region Chache
                    {
                        for (int i = 0; i < updateAnimatorRootCorrection.muscleIndexes.Length; i++)
                        {
                            updateAnimatorRootCorrection.muscleCurves[i] = GetEditorCurveCache(AnimationCurveBindingAnimatorMuscle(updateAnimatorRootCorrection.muscleIndexes[i]));
                        }
                    }
                    #endregion
                    foreach (var pair in updateAnimatorRootCorrection.updateFrame.frames)
                    {
                        var frame = pair.Key;
                        var tGO = calcObject.gameObjectTransform;
                        var tHip = calcObject.humanoidHipsTransform;
                        var time = GetFrameTime(frame);
                        #region Before
                        {
                            var tframe = frame;
                            if (tframe >= updateAnimatorRootCorrection.rootSaves.Count)
                                tframe = updateAnimatorRootCorrection.rootSaves.Count - 1;
                            updateAnimatorRootCorrection.humanPose.bodyPosition = updateAnimatorRootCorrection.rootSaves[tframe].position;
                            updateAnimatorRootCorrection.humanPose.bodyRotation = updateAnimatorRootCorrection.rootSaves[tframe].rotation;
                            for (int i = 0; i < updateAnimatorRootCorrection.muscleIndexes.Length; i++)
                            {
                                var muscleIndex = updateAnimatorRootCorrection.muscleIndexes[i];
                                updateAnimatorRootCorrection.humanPose.muscles[muscleIndex] = updateAnimatorRootCorrection.muscleValueSaves[i][tframe];
                            }
                            calcObject.humanPoseHandler.SetHumanPose(ref updateAnimatorRootCorrection.humanPose);
                        }
                        var hipBeforeRot = (tHip.rotation * humanoidPoseHipRotation) * humanoidPreHipRotationInverse;
                        var hipBeforePos = tHip.position;
                        #endregion
                        #region RootQ
                        Quaternion rootQ;
                        {
                            updateAnimatorRootCorrection.humanPose.bodyPosition = updateAnimatorRootCorrection.frameRootSaves[frame].position;
                            updateAnimatorRootCorrection.humanPose.bodyRotation = updateAnimatorRootCorrection.frameRootSaves[frame].rotation;
                            for (int i = 0; i < updateAnimatorRootCorrection.muscleIndexes.Length; i++)
                            {
                                if (updateAnimatorRootCorrection.muscleCurves[i] == null) continue;
                                var muscleIndex = updateAnimatorRootCorrection.muscleIndexes[i];
                                updateAnimatorRootCorrection.humanPose.muscles[muscleIndex] = updateAnimatorRootCorrection.muscleCurves[i].Evaluate(time);
                            }
                            calcObject.humanPoseHandler.SetHumanPose(ref updateAnimatorRootCorrection.humanPose);
                            {
                                var hipNowRot = (tHip.rotation * humanoidPoseHipRotation) * humanoidPreHipRotationInverse;
                                var offset = hipBeforeRot * Quaternion.Inverse(hipNowRot);
                                rootQ = offset * updateAnimatorRootCorrection.humanPose.bodyRotation;
                                #region FixReverseRotation
                                {
                                    var rot = rootQ * Quaternion.Inverse(Quaternion.identity);
                                    if (rot.w < 0f)
                                    {
                                        for (int i = 0; i < 4; i++)
                                            rootQ[i] = -rootQ[i];
                                    }
                                }
                                #endregion
                            }
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            var curve = updateAnimatorRootCorrection.rootQCurves[i];
                            float value = rootQ[i];
                            SetKeyframe(curve, time, value, false);
                        }
                        updateAnimatorRootCorrection.humanPose.bodyRotation = rootQ;
                        calcObject.humanPoseHandler.SetHumanPose(ref updateAnimatorRootCorrection.humanPose);
                        #endregion
                        #region RootT
                        Vector3 rootT;
                        {
                            var hipNowPos = tHip.position;
                            var offset = ((hipNowPos - hipBeforePos)) * (1f / calcObject.animator.humanScale);
                            rootT = updateAnimatorRootCorrection.humanPose.bodyPosition - offset;
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            var curve = updateAnimatorRootCorrection.rootTCurves[i];
                            float value = rootT[i];
                            SetKeyframe(curve, time, value, false);
                        }
                        #endregion
                    }
                    #endregion
                }
                else
                {
                    #region Has TDoF
                    calcObject.gameObject.SetActive(true);
                    calcObject.vaEdit.SetAnimationClip(currentClip);
                    calcObject.AnimatorRebind();
                    foreach (var pair in updateAnimatorRootCorrection.updateFrame.frames)
                    {
                        var frame = pair.Key;
                        var tHip = calcObject.humanoidHipsTransform;
                        var time = GetFrameTime(frame);
                        currentClip.SampleAnimation(calcObject.gameObject, time);
                        {
                            #region Before
                            Vector3 hipBeforePos;
                            Quaternion hipBeforeRot;
                            {
                                var tframe = frame;
                                if (tframe >= updateAnimatorRootCorrection.hipSaves.Count)
                                    tframe = updateAnimatorRootCorrection.hipSaves.Count - 1;
                                hipBeforePos = updateAnimatorRootCorrection.hipSaves[tframe].position;
                                hipBeforeRot = updateAnimatorRootCorrection.hipSaves[tframe].rotation;
                            }
                            #endregion
                            var hipNowPos = tHip.position;
                            var hipNowRot = (tHip.rotation * humanoidPoseHipRotation) * humanoidPreHipRotationInverse;
                            #region RootQ
                            Quaternion rootQ;
                            Quaternion rotationOffset;
                            {
                                rotationOffset = hipBeforeRot * Quaternion.Inverse(hipNowRot);
                                updateAnimatorRootCorrection.humanPose.bodyRotation = updateAnimatorRootCorrection.frameRootSaves[frame].rotation;
                                rootQ = rotationOffset * updateAnimatorRootCorrection.humanPose.bodyRotation;
                                #region FixReverseRotation
                                {
                                    var rot = rootQ * Quaternion.Inverse(Quaternion.identity);
                                    if (rot.w < 0f)
                                    {
                                        for (int i = 0; i < 4; i++)
                                            rootQ[i] = -rootQ[i];
                                    }
                                }
                                #endregion
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                var curve = updateAnimatorRootCorrection.rootQCurves[i];
                                float value = rootQ[i];
                                SetKeyframe(curve, time, value, false);
                            }
                            #endregion
                            #region RootT
                            Vector3 rootT;
                            {
                                updateAnimatorRootCorrection.humanPose.bodyPosition = updateAnimatorRootCorrection.frameRootSaves[frame].position;
                                var bodyPosition = updateAnimatorRootCorrection.humanPose.bodyPosition * calcObject.animator.humanScale;
                                var worldRootPosition = calcObject.gameObjectTransform.localToWorldMatrix.MultiplyPoint3x4(bodyPosition);
                                hipNowPos = worldRootPosition + rotationOffset * (hipNowPos - worldRootPosition);
                                var offset = ((hipNowPos - hipBeforePos)) * (1f / calcObject.animator.humanScale);
                                rootT = updateAnimatorRootCorrection.humanPose.bodyPosition - offset;
                            }
                            for (int i = 0; i < 3; i++)
                            {
                                var curve = updateAnimatorRootCorrection.rootTCurves[i];
                                float value = rootT[i];
                                SetKeyframe(curve, time, value, false);
                            }
                            #endregion
                        }
                    }
                    calcObject.gameObject.SetActive(false);
                    #endregion
                }

                #region SmoothTangent
                {
                    foreach (var pair in updateAnimatorRootCorrection.updateFrame.frames)
                    {
                        var frame = pair.Key;
                        var weight = pair.Value;
                        var time = GetFrameTime(frame);
                        for (int i = 0; i < 4; i++)
                        {
                            var keyIndex = FindKeyframeAtTime(updateAnimatorRootCorrection.rootQCurves[i], time);
                            if (keyIndex >= 0)
                                updateAnimatorRootCorrection.rootQCurves[i].SmoothTangents(keyIndex, weight);
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            var keyIndex = FindKeyframeAtTime(updateAnimatorRootCorrection.rootTCurves[i], time);
                            if (keyIndex >= 0)
                                updateAnimatorRootCorrection.rootTCurves[i].SmoothTangents(keyIndex, weight);
                        }
                        AddHumanoidFootIK(time, weight);
                    }
                }
                #endregion

                #region Write
                {
                    for (int i = 0; i < 4; i++)
                    {
                        SetAnimationCurveAnimatorRootQ(i, updateAnimatorRootCorrection.rootQCurves[i]);
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        SetAnimationCurveAnimatorRootT(i, updateAnimatorRootCorrection.rootTCurves[i]);
                    }
                }
                #endregion
            }
        }
        #endregion

        #region FootIK
        private class HumanoidFootIK
        {
            public AnimationCurve[] rootT = new AnimationCurve[3];
            public AnimationCurve[] rootQ = new AnimationCurve[4];
            public class IkCurves
            {
                public AnimationCurve[] ikT = new AnimationCurve[3];
                public AnimationCurve[] ikQ = new AnimationCurve[4];
            }
            public IkCurves[] ikCurves;

            public WeightUpdateFrame updateFrame = new WeightUpdateFrame();

            public HumanoidFootIK()
            {
                ikCurves = new IkCurves[AnimatorIKIndex.RightFoot - AnimatorIKIndex.LeftFoot + 1];
                for (int i = 0; i < ikCurves.Length; i++)
                {
                    ikCurves[i] = new IkCurves();
                }
            }

            public void Clear()
            {
                for (int i = 0; i < 3; i++)
                {
                    rootT[i] = null;
                    for (int j = 0; j < ikCurves.Length; j++)
                        ikCurves[j].ikT[i] = null;
                }
                for (int i = 0; i < 4; i++)
                {
                    rootQ[i] = null;
                    for (int j = 0; j < ikCurves.Length; j++)
                        ikCurves[j].ikQ[i] = null;
                }
                updateFrame.Clear();
            }
        }
        private HumanoidFootIK humanoidFootIK;

        private void InitializeHumanoidFootIK()
        {
            if (!isHuman) return;
            humanoidFootIK = new HumanoidFootIK();
        }
        private void ReleaseHumanoidFootIK()
        {
            humanoidFootIK = null;
        }
        private bool IsEnableUpdateHumanoidFootIK()
        {
            if (!isHuman)
                return false;

#if UNITY_2017_1_OR_NEWER
            return (autoFootIK || uAw_2017_1.GetLinkedWithTimeline());
#else
            return (autoFootIK);
#endif
        }
        private void AddHumanoidFootIK(float time, float weight = 0f)
        {
            if (!isHuman) return;
            if (time < 0f || time > currentClip.length) return;

            var frame = uAw.TimeToFrameRound(time);
            humanoidFootIK.updateFrame.Add(frame, weight);
        }
        private bool UpdateHumanoidFootIK()
        {
            bool update = false;
            if (IsEnableUpdateHumanoidFootIK() &&
                !humanoidFootIK.updateFrame.IsEmpty())
            {
                UpdateSyncEditorCurveClip();

                var lastFrame = GetLastFrame();
                #region Tmp
                for (int i = 0; i < 3; i++)
                {
                    humanoidFootIK.rootT[i] = GetAnimationCurveAnimatorRootT(i);
                }
                for (int i = 0; i < 4; i++)
                {
                    humanoidFootIK.rootQ[i] = GetAnimationCurveAnimatorRootQ(i);
                }
                for (var ikIndex = AnimatorIKIndex.LeftFoot; ikIndex <= AnimatorIKIndex.RightFoot; ikIndex++)
                {
                    int index = ikIndex - AnimatorIKIndex.LeftFoot;
                    for (int i = 0; i < 3; i++)
                    {
                        humanoidFootIK.ikCurves[index].ikT[i] = GetAnimationCurveAnimatorIkT(ikIndex, i);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        humanoidFootIK.ikCurves[index].ikQ[i] = GetAnimationCurveAnimatorIkQ(ikIndex, i);
                    }
                }
                #endregion
                #region Set
                {
                    if (!editAnimator.isInitialized)
                        editAnimator.Rebind();
                    var root = editGameObject.transform;
                    var humanScale = editAnimator.humanScale;
                    var leftFeetBottomHeight = editAnimator.leftFeetBottomHeight;
                    var rightFeetBottomHeight = editAnimator.rightFeetBottomHeight;
                    var postLeftFoot = uAvatar.GetPostRotation(editAnimator.avatar, (int)HumanBodyBones.LeftFoot);
                    var postRightFoot = uAvatar.GetPostRotation(editAnimator.avatar, (int)HumanBodyBones.RightFoot);
                    foreach (var pair in humanoidFootIK.updateFrame.frames)
                    {
                        var frame = pair.Key;
                        if (frame > lastFrame)
                            frame = lastFrame;
                        var time = GetFrameTime(frame);
                        currentClip.SampleAnimation(editGameObject, time);
                        Vector3 rootT = Vector3.zero;
                        {
                            for (int i = 0; i < 3; i++)
                                rootT[i] = humanoidFootIK.rootT[i].Evaluate(time);
                        }
                        Quaternion rootQ = Quaternion.identity;
                        {
                            Vector4 result = new Vector4(0, 0, 0, 1);
                            for (int i = 0; i < 4; i++)
                                result[i] = humanoidFootIK.rootQ[i].Evaluate(time);
                            if (result.sqrMagnitude > 0f)
                                rootQ = new Quaternion(result.x, result.y, result.z, result.w);
                        }
                        for (var ikIndex = AnimatorIKIndex.LeftFoot; ikIndex <= AnimatorIKIndex.RightFoot; ikIndex++)
                        {
                            int index = ikIndex - AnimatorIKIndex.LeftFoot;
                            var humanoidIndex = AnimatorIKIndex2HumanBodyBones[(int)ikIndex];
                            var t = editHumanoidBones[(int)humanoidIndex].transform;
                            Vector3 ikT = t.position;
                            Quaternion ikQ = t.rotation;
                            {
                                Quaternion post = Quaternion.identity;
                                switch ((AnimatorIKIndex)ikIndex)
                                {
                                case AnimatorIKIndex.LeftFoot: post = postLeftFoot; break;
                                case AnimatorIKIndex.RightFoot: post = postRightFoot; break;
                                }
                                #region IkT
                                if (ikIndex == AnimatorIKIndex.LeftFoot || ikIndex == AnimatorIKIndex.RightFoot)
                                {
                                    Vector3 add = Vector3.zero;
                                    switch ((AnimatorIKIndex)ikIndex)
                                    {
                                    case AnimatorIKIndex.LeftFoot: add.x += leftFeetBottomHeight; break;
                                    case AnimatorIKIndex.RightFoot: add.x += rightFeetBottomHeight; break;
                                    }
                                    ikT += (t.rotation * post) * add;
                                }
                                ikT = root.worldToLocalMatrix.MultiplyPoint3x4(ikT) - (rootT * humanScale);
                                ikT = Quaternion.Inverse(rootQ) * ikT;
                                ikT *= 1f / humanScale;
                                #endregion
                                #region IkQ
                                ikQ = Quaternion.Inverse(root.rotation * rootQ) * (t.rotation * post);
                                #endregion
                            }
                            for (int i = 0; i < 3; i++)
                            {
                                var keyIndex = SetKeyframe(humanoidFootIK.ikCurves[index].ikT[i], time, ikT[i], false);
                                AnimationUtility.SetKeyLeftTangentMode(humanoidFootIK.ikCurves[index].ikT[i], keyIndex, AnimationUtility.TangentMode.ClampedAuto);
                                AnimationUtility.SetKeyRightTangentMode(humanoidFootIK.ikCurves[index].ikT[i], keyIndex, AnimationUtility.TangentMode.ClampedAuto);
                                AnimationUtility.SetKeyBroken(humanoidFootIK.ikCurves[index].ikT[i], keyIndex, false);
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                var keyIndex = SetKeyframe(humanoidFootIK.ikCurves[index].ikQ[i], time, ikQ[i], false);
                                AnimationUtility.SetKeyLeftTangentMode(humanoidFootIK.ikCurves[index].ikQ[i], keyIndex, AnimationUtility.TangentMode.ClampedAuto);
                                AnimationUtility.SetKeyRightTangentMode(humanoidFootIK.ikCurves[index].ikQ[i], keyIndex, AnimationUtility.TangentMode.ClampedAuto);
                                AnimationUtility.SetKeyBroken(humanoidFootIK.ikCurves[index].ikQ[i], keyIndex, false);
                            }
                        }
                    }
                }
                #endregion
                #region SmoothTangent
                {
                    foreach (var pair in updateAnimatorRootCorrection.updateFrame.frames)
                    {
                        var frame = pair.Key;
                        if (frame > lastFrame) continue;
                        var weight = pair.Value;
                        var time = GetFrameTime(frame);
                        for (var ikIndex = AnimatorIKIndex.LeftFoot; ikIndex <= AnimatorIKIndex.RightFoot; ikIndex++)
                        {
                            int index = ikIndex - AnimatorIKIndex.LeftFoot;
                            for (int i = 0; i < 3; i++)
                            {
                                var keyIndex = FindKeyframeAtTime(humanoidFootIK.ikCurves[index].ikT[i], time);
                                if (keyIndex >= 0)
                                    humanoidFootIK.ikCurves[index].ikT[i].SmoothTangents(keyIndex, weight);
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                var keyIndex = FindKeyframeAtTime(humanoidFootIK.ikCurves[index].ikQ[i], time);
                                if (keyIndex >= 0)
                                    humanoidFootIK.ikCurves[index].ikQ[i].SmoothTangents(keyIndex, weight);
                            }
                        }
                    }
                }
                #endregion
                #region Write
                for (var ikIndex = AnimatorIKIndex.LeftFoot; ikIndex <= AnimatorIKIndex.RightFoot; ikIndex++)
                {
                    int index = ikIndex - AnimatorIKIndex.LeftFoot;
                    for (int i = 0; i < 3; i++)
                    {
                        SetAnimationCurveAnimatorIkT(ikIndex, i, humanoidFootIK.ikCurves[index].ikT[i]);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        SetAnimationCurveAnimatorIkQ(ikIndex, i, humanoidFootIK.ikCurves[index].ikQ[i]);
                    }
                }
                #endregion
                ResampleAnimation();
            }
            humanoidFootIK.Clear();
            return update;
        }
        #endregion

        #region AnimationTool
        public HumanBodyBones GetHumanVirtualBoneParentBone(HumanBodyBones bone)
        {
            if (!isHuman) return (HumanBodyBones)(-1);
            var vbs = HumanVirtualBones[(int)bone];
            if (vbs != null)
            {
                foreach (var vb in vbs)
                {
                    if (humanoidBones[(int)vb.boneA] == null) continue;
                    return vb.boneA;
                }
            }
            return (HumanBodyBones)(-1);
        }
        public Vector3 GetHumanVirtualBoneLimitSign(HumanBodyBones bone)
        {
            if (!isHuman) return Vector3.one;
            var vbs = HumanVirtualBones[(int)bone];
            if (vbs != null)
            {
                foreach (var vb in vbs)
                {
                    if (humanoidBones[(int)vb.boneA] == null) continue;
                    return vb.limitSign;
                }
            }
            return Vector3.one;
        }

        public Vector3 GetHumanVirtualBonePosition(HumanBodyBones bone)
        {
            if (!isHuman) return Vector3.zero;
            var vbs = HumanVirtualBones[(int)bone];
            if (vbs != null)
            {
                foreach (var vb in vbs)
                {
                    if (editHumanoidBones[(int)vb.boneA] == null || editHumanoidBones[(int)vb.boneB] == null) continue;
                    var posA = editHumanoidBones[(int)vb.boneA].transform.position;
                    var posB = editHumanoidBones[(int)vb.boneB].transform.position;
                    return Vector3.Lerp(posA, posB, vb.leap);
                }
            }
            return Vector3.zero;
        }
        public Quaternion GetHumanVirtualBoneRotation(HumanBodyBones bone)
        {
            if (!isHuman) return Quaternion.identity;
            var vbs = HumanVirtualBones[(int)bone];
            if (vbs != null)
            {
                foreach (var vb in vbs)
                {
                    if (editHumanoidBones[(int)vb.boneA] == null) continue;
                    var vRotation = Vector3.zero;
                    for (int i = 0; i < 3; i++)
                    {
                        var mi = HumanTrait.MuscleFromBone((int)bone, i);
                        if (i >= 0)
                        {
                            var muscle = GetAnimationValueAnimatorMuscle(mi);
                            vRotation[i] = Mathf.Lerp(humanoidMuscleLimit[(int)bone].min[i], humanoidMuscleLimit[(int)bone].max[i], (muscle + 1f) / 2f);
                        }
                    }
                    var qRotation = Quaternion.Euler(vRotation);
                    var parentRotation = editHumanoidBones[(int)vb.boneA].transform.rotation * uAvatar.GetPostRotation(editAnimator.avatar, (int)vb.boneA);
                    return parentRotation * qRotation;
                }
            }
            return Quaternion.identity;
        }
        public Quaternion GetHumanVirtualBoneParentRotation(HumanBodyBones bone)
        {
            if (!isHuman) return Quaternion.identity;
            var vbs = HumanVirtualBones[(int)bone];
            if (vbs != null)
            {
                foreach (var vb in vbs)
                {
                    if (editHumanoidBones[(int)vb.boneA] == null) continue;
                    return editHumanoidBones[(int)vb.boneA].transform.rotation * uAvatar.GetPostRotation(editAnimator.avatar, (int)vb.boneA) * vb.addRotation;
                }
            }
            return Quaternion.identity;
        }

        public Vector3 GetHumanWorldRootPosition()
        {
            if (!isHuman) return Vector3.zero;
            var bodyPosition = GetAnimationValueAnimatorRootT() * editAnimator.humanScale;
            return editGameObject.transform.localToWorldMatrix.MultiplyPoint3x4(bodyPosition);
        }
        public Vector3 GetHumanLocalRootPosition(Vector3 pos)
        {
            if (!isHuman) return Vector3.zero;
            var bodyPosition = editGameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
            return bodyPosition / editAnimator.humanScale;
        }
        public Quaternion GetHumanWorldRootRotation()
        {
            if (!isHuman) return Quaternion.identity;
            return editGameObject.transform.rotation * GetAnimationValueAnimatorRootQ();
        }
        public Quaternion GetHumanLocalRootRotation(Quaternion rot)
        {
            if (!isHuman) return Quaternion.identity;
            return Quaternion.Inverse(editGameObject.transform.rotation) * rot;
        }

        public bool GetPlayingAnimationInfo(out AnimationClip dstClip, out float dstTime)
        {
            dstClip = null;
            dstTime = 0f;
            if (!EditorApplication.isPlaying) return false;
            if (vaw.animator != null && vaw.animator.runtimeAnimatorController != null && vaw.animator.isInitialized)
            {
                #region animator
                UnityEditor.Animations.AnimatorController ac = null;
                AnimatorOverrideController owc = null;
                if (vaw.animator.runtimeAnimatorController is AnimatorOverrideController)
                {
                    owc = vaw.animator.runtimeAnimatorController as AnimatorOverrideController;
                    ac = owc.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                }
                else
                {
                    ac = vaw.animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                }
                if (vaw.animator.layerCount > 0)
                {
                    var layerIndex = 0;
                    var currentAnimatorStateInfo = vaw.animator.GetCurrentAnimatorStateInfo(layerIndex);
                    AnimationClip resultClip = null;
                    float resultTime = 0f;
                    Func<AnimatorStateMachine, bool> FindStateMachine = null;
                    FindStateMachine = (stateMachine) =>
                    {
                        foreach (var state in stateMachine.states)
                        {
                            if (state.state.nameHash != currentAnimatorStateInfo.shortNameHash ||
                                currentAnimatorStateInfo.length <= 0f)
                                continue;

                            Func<Motion, AnimationClip> FindMotion = null;
                            FindMotion = (motion) =>
                            {
                                if (motion != null)
                                {
                                    if (motion is UnityEditor.Animations.BlendTree)
                                    {
                                        #region BlendTree
                                        var blendTree = motion as UnityEditor.Animations.BlendTree;
                                        switch (blendTree.blendType)
                                        {
                                        case BlendTreeType.Simple1D:
                                            #region 1D
                                            {
                                                var param = vaw.animator.GetFloat(blendTree.blendParameter);
                                                float near = float.MaxValue;
                                                int index = -1;
                                                for (int i = 0; i < blendTree.children.Length; i++)
                                                {
                                                    var offset = Mathf.Abs(blendTree.children[i].threshold - param);
                                                    if (offset < near)
                                                    {
                                                        index = i;
                                                        near = offset;
                                                    }
                                                }
                                                if (index >= 0)
                                                {
                                                    return FindMotion(blendTree.children[index].motion);
                                                }
                                            }
                                            #endregion
                                            break;
                                        case BlendTreeType.SimpleDirectional2D:
                                        case BlendTreeType.FreeformDirectional2D:
                                        case BlendTreeType.FreeformCartesian2D:
                                            #region 2D
                                            {
                                                var paramX = vaw.animator.GetFloat(blendTree.blendParameter);
                                                var paramY = vaw.animator.GetFloat(blendTree.blendParameterY);
                                                float near = float.MaxValue;
                                                int index = -1;
                                                for (int i = 0; i < blendTree.children.Length; i++)
                                                {
                                                    var offsetX = Mathf.Abs(blendTree.children[i].position.x - paramX);
                                                    var offsetY = Mathf.Abs(blendTree.children[i].position.y - paramY);
                                                    if (offsetX + offsetY < near)
                                                    {
                                                        index = i;
                                                        near = offsetX + offsetY;
                                                    }
                                                }
                                                if (index >= 0)
                                                {
                                                    return FindMotion(blendTree.children[index].motion);
                                                }
                                            }
                                            #endregion
                                            break;
                                        case BlendTreeType.Direct:
                                            #region Direct
                                            {
                                                float max = float.MinValue;
                                                int index = -1;
                                                for (int i = 0; i < blendTree.children.Length; i++)
                                                {
                                                    var param = vaw.animator.GetFloat(blendTree.children[i].directBlendParameter);
                                                    if (param >= max)
                                                    {
                                                        index = i;
                                                        max = param;
                                                    }
                                                }
                                                if (index >= 0)
                                                {
                                                    return FindMotion(blendTree.children[index].motion);
                                                }
                                            }
                                            #endregion
                                            break;
                                        default:
                                            Assert.IsTrue(false, "not support type");
                                            break;
                                        }
                                        #endregion
                                    }
                                    else if (motion is AnimationClip)
                                    {
                                        return motion as AnimationClip;
                                    }
                                }
                                return null;
                            };
                            var clip = FindMotion(state.state.motion);
                            if (clip == null)
                                continue;
                            if (owc != null)
                                clip = owc[clip];
                            resultClip = clip;
                            var time = currentAnimatorStateInfo.length * currentAnimatorStateInfo.normalizedTime;
                            resultTime = time;
                            if (resultClip.isLooping)
                            {
                                var loop = Mathf.FloorToInt(time / currentAnimatorStateInfo.length);
                                resultTime -= loop * currentAnimatorStateInfo.length;
                            }
                            else if (resultTime > currentAnimatorStateInfo.length)
                            {
                                resultTime = currentAnimatorStateInfo.length;
                            }
                            return true;
                        }
                        foreach (var cstateMachine in stateMachine.stateMachines)
                        {
                            if (FindStateMachine(cstateMachine.stateMachine))
                                return true;
                        }
                        return false;
                    };
                    if (FindStateMachine(ac.layers[layerIndex].stateMachine))
                    {
                        dstClip = resultClip;
                        dstTime = resultTime;
                        return true;
                    }
                }
                #endregion
            }
            else if (vaw.animation != null)
            {
                #region animation
                foreach (AnimationState state in vaw.animation)
                {
                    if (!state.enabled || state.length <= 0f) continue;
                    dstClip = state.clip;
                    var time = state.time;
                    dstTime = time;
                    switch (state.wrapMode)
                    {
                    case WrapMode.Loop:
                        {
                            var loop = Mathf.FloorToInt(time / state.length);
                            dstTime -= loop * state.length;
                        }
                        break;
                    case WrapMode.PingPong:
                        {
                            var loop = Mathf.FloorToInt(time / state.length);
                            dstTime -= loop * state.length;
                            if (loop % 2 != 0)
                                dstTime = state.length - dstTime;
                        }
                        break;
                    default:
                        dstTime = Mathf.Min(dstTime, state.length);
                        break;
                    }
                    return true;
                }
                #endregion
            }
            return false;
        }

        public enum HumanoidBoneAttribute
        {
            Center,
            Left,
            Right,
        }
        public HumanoidBoneAttribute GetHumanoidBoneAttribute(HumanBodyBones bone)
        {
            var text = bone.ToString();
            if (text.StartsWith("Left")) return HumanoidBoneAttribute.Left;
            else if (text.StartsWith("Right")) return HumanoidBoneAttribute.Right;
            else return HumanoidBoneAttribute.Center;
        }

        public int GetMirrorMuscleIndex(int muscleIndex)
        {
            if (muscleIndex < 0) return -1;
            var humanIndex = HumanTrait.BoneFromMuscle(muscleIndex);
            if (humanIndex < 0) return -1;
            if (HumanBodyMirrorBones[humanIndex] < 0) return -1;
            for (int i = 0; i < 3; i++)
            {
                if (muscleIndex == HumanTrait.MuscleFromBone(humanIndex, i))
                    return HumanTrait.MuscleFromBone((int)HumanBodyMirrorBones[humanIndex], i);
            }
            return -1;
        }
        private Vector3 GetMirrorBoneLocalPosition(int boneIndex, Vector3 localPosition)
        {
            var rootInv = Quaternion.Inverse(boneSaveTransforms[0].rotation);
            var local = localPosition - boneSaveTransforms[boneIndex].localPosition;
            var parentRot = (rootInv * boneSaveTransforms[boneIndex].rotation) * Quaternion.Inverse(boneSaveTransforms[boneIndex].localRotation);
            var world = parentRot * local;
            world.x = -world.x;
            if (mirrorBoneIndexes[boneIndex] >= 0)
            {
                var mparentRot = (rootInv * boneSaveTransforms[mirrorBoneIndexes[boneIndex]].rotation) * Quaternion.Inverse(boneSaveTransforms[mirrorBoneIndexes[boneIndex]].localRotation);
                var mlocal = Quaternion.Inverse(mparentRot) * world;
                return boneSaveTransforms[mirrorBoneIndexes[boneIndex]].localPosition + mlocal;
            }
            else
            {
                var mlocal = Quaternion.Inverse(parentRot) * world;
                return boneSaveTransforms[boneIndex].localPosition + mlocal;
            }
        }
        private Quaternion GetMirrorBoneLocalRotation(int boneIndex, Quaternion localRotation)
        {
            var rootInv = Quaternion.Inverse(boneSaveTransforms[0].rotation);
            var parentRot = (rootInv * boneSaveTransforms[boneIndex].rotation) * Quaternion.Inverse(boneSaveTransforms[boneIndex].localRotation);
            var wrot = parentRot * localRotation;
            if (mirrorBoneIndexes[boneIndex] >= 0)
            {
                var rootRot = rootInv * boneSaveTransforms[mirrorBoneRootIndexes[boneIndex]].rotation;
                wrot *= Quaternion.Inverse(Quaternion.Inverse(rootRot) * (rootInv * boneSaveTransforms[boneIndex].rotation));
                {
                    wrot *= Quaternion.Inverse(rootRot);
                    wrot = new Quaternion(wrot.x, -wrot.y, -wrot.z, wrot.w);
                    wrot *= rootRot;
                }
                wrot *= Quaternion.Inverse(rootRot) * (rootInv * boneSaveTransforms[mirrorBoneIndexes[boneIndex]].rotation);
                var mparentRot = (rootInv * boneSaveTransforms[mirrorBoneIndexes[boneIndex]].rotation) * Quaternion.Inverse(boneSaveTransforms[mirrorBoneIndexes[boneIndex]].localRotation);
                return Quaternion.Inverse(mparentRot) * wrot;
            }
            else
            {
                var rootRot = rootInv * boneSaveTransforms[boneIndex].rotation;
                wrot *= Quaternion.Inverse(rootRot);
                wrot = new Quaternion(wrot.x, -wrot.y, -wrot.z, wrot.w);
                wrot *= rootRot;
                return Quaternion.Inverse(parentRot) * wrot;
            }
        }
        private Vector3 GetMirrorBoneLocalScale(int boneIndex, Vector3 localRotation)
        {
            return localRotation;
        }

        public int GetLastFrame()
        {
            return uAw.GetLastFrame(currentClip);
        }
        public float GetFrameTime(int frame)
        {
            return uAw.GetFrameTime(frame, currentClip);
        }

        public int FindKeyframeAtTime(AnimationCurve curve, float time)
        {
            if (curve.length > 0)
            {
                int begin = 0, end = curve.length - 1;
                while (end - begin > 1)
                {
                    var index = begin + Mathf.FloorToInt((end - begin) / 2f);
                    if (time < curve[index].time)
                    {
                        if (end == index) break;
                        end = index;
                    }
                    else
                    {
                        if (begin == index) break;
                        begin = index;
                    }
                }
                if (Mathf.Abs(curve[begin].time - time) < 0.0001f)
                    return begin;
                if (Mathf.Abs(curve[end].time - time) < 0.0001f)
                    return end;
            }
            return -1;
        }
        public int FindKeyframeAtTime(Keyframe[] keys, float time)
        {
            if (keys.Length > 0)
            {
                int begin = 0, end = keys.Length - 1;
                while (end - begin > 1)
                {
                    var index = begin + Mathf.FloorToInt((end - begin) / 2f);
                    if (time < keys[index].time)
                    {
                        if (end == index) break;
                        end = index;
                    }
                    else
                    {
                        if (begin == index) break;
                        begin = index;
                    }
                }
                if (Mathf.Abs(keys[begin].time - time) < 0.0001f)
                    return begin;
                if (Mathf.Abs(keys[end].time - time) < 0.0001f)
                    return end;
            }
            return -1;
        }
        public int FindKeyframeAtTime(List<ObjectReferenceKeyframe> keys, float time)
        {
            if (keys.Count > 0)
            {
                int begin = 0, end = keys.Count - 1;
                while (end - begin > 1)
                {
                    var index = begin + Mathf.FloorToInt((end - begin) / 2f);
                    if (time < keys[index].time)
                    {
                        if (end == index) break;
                        end = index;
                    }
                    else
                    {
                        if (begin == index) break;
                        begin = index;
                    }
                }
                if (Mathf.Abs(keys[begin].time - time) < 0.0001f)
                    return begin;
                if (Mathf.Abs(keys[end].time - time) < 0.0001f)
                    return end;
            }
            return -1;
        }
        public int FindBeforeNearKeyframeAtTime(AnimationCurve curve, float time)
        {
            if (curve.length == 0)
                return -1;
            else if (curve[curve.length - 1].time < time)
                return curve.length - 1;
            else if (curve[0].time >= time)
                return -1;

            var result = -1;
            {
                int begin = 0, end = curve.length - 1;
                while (end - begin > 1)
                {
                    var index = begin + Mathf.FloorToInt((end - begin) / 2f);
                    if (time < curve[index].time)
                    {
                        if (end == index) break;
                        end = index;
                    }
                    else
                    {
                        if (begin == index) break;
                        begin = index;
                    }
                }
                if (Mathf.Abs(curve[begin].time - time) < 0.0001f)
                    result = begin - 1;
                else
                    result = begin;
            }
            Assert.IsTrue(curve[result].time < time && (result + 1 >= curve.length || curve[result + 1].time >= time));
            return result;
        }
        public int FindBeforeNearKeyframeAtTime(ObjectReferenceKeyframe[] keys, float time)
        {
            if (keys.Length == 0)
                return -1;
            else if (keys[keys.Length - 1].time < time)
                return keys.Length - 1;
            else if (keys[0].time >= time)
                return -1;

            var result = -1;
            {
                int begin = 0, end = keys.Length - 1;
                while (end - begin > 1)
                {
                    var index = begin + Mathf.FloorToInt((end - begin) / 2f);
                    if (time < keys[index].time)
                    {
                        if (end == index) break;
                        end = index;
                    }
                    else
                    {
                        if (begin == index) break;
                        begin = index;
                    }
                }
                if (Mathf.Abs(keys[begin].time - time) < 0.0001f)
                    result = begin - 1;
                else
                    result = begin;
            }
            Assert.IsTrue(keys[result].time < time && (result + 1 >= keys.Length || keys[result + 1].time >= time));
            return result;
        }
        public int FindKeyframeIndex(AnimationCurve curve, AnimationCurve findCurve, int findIndex)
        {
            var index = FindKeyframeAtTime(curve, findCurve[findIndex].time);
            if (index >= 0)
            {
                //if(curve[index].Equals(key))  GC Alloc...
                if (curve[index].time == findCurve[findIndex].time &&
                    curve[index].value == findCurve[findIndex].value &&
                    curve[index].inTangent == findCurve[findIndex].inTangent &&
                    curve[index].outTangent == findCurve[findIndex].outTangent &&
#if !UNITY_2018_1_OR_NEWER
                    curve[index].tangentMode == findCurve[findIndex].tangentMode)
#else
                    curve[index].inWeight == findCurve[findIndex].inWeight &&
                    curve[index].outWeight == findCurve[findIndex].outWeight &&
                    curve[index].weightedMode == findCurve[findIndex].weightedMode &&
                    AnimationUtility.GetKeyLeftTangentMode(curve, index) == AnimationUtility.GetKeyLeftTangentMode(findCurve, findIndex) &&
                    AnimationUtility.GetKeyRightTangentMode(curve, index) == AnimationUtility.GetKeyRightTangentMode(findCurve, findIndex))
#endif
                {
                    return index;
                }
            }
            return -1;
        }

        public int AddKeyframe(AnimationCurve curve, float time, float value, bool updateTangents = true)
        {
            var keyIndex = curve.AddKey(new Keyframe(time, value));
            if (keyIndex < 0) return -1;
            uCurveUtility.SetKeyModeFromContext(curve, keyIndex);
            if (updateTangents)
                uAnimationUtility.UpdateTangentsFromModeSurrounding(curve, keyIndex);
            return keyIndex;
        }
        public int AddKeyframe(AnimationCurve curve, Keyframe keyframe, bool updateTangents = true)
        {
            var keyIndex = curve.AddKey(keyframe);
            if (keyIndex < 0) return -1;
            uCurveUtility.SetKeyModeFromContext(curve, keyIndex);
            if (updateTangents)
                uAnimationUtility.UpdateTangentsFromModeSurrounding(curve, keyIndex);
            return keyIndex;
        }
        public int SetKeyframe(AnimationCurve curve, float time, float value, bool updateTangents = true)
        {
            var keyIndex = FindKeyframeAtTime(curve, time);
            if (keyIndex < 0)
            {
                keyIndex = AddKeyframe(curve, time, value, updateTangents);
                SetAnimationWindowRefresh(AnimationWindowStateRefreshType.CurvesOnly);
            }
            else
            {
                var keyframe = curve[keyIndex];
                keyframe.value = value;
                curve.MoveKey(keyIndex, keyframe);
            }
            return keyIndex;
        }
        public int SetKeyframe(AnimationCurve curve, Keyframe keyframe, bool updateTangents = true)
        {
            var keyIndex = FindKeyframeAtTime(curve, keyframe.time);
            if (keyIndex < 0)
            {
                keyIndex = AddKeyframe(curve, keyframe, updateTangents);
                SetAnimationWindowRefresh(AnimationWindowStateRefreshType.CurvesOnly);
            }
            else
            {
                curve.MoveKey(keyIndex, keyframe);
            }
            return keyIndex;
        }

        public void SetKeyframeTangentModeLinear(AnimationCurve curve, int keyIndex)
        {
            if (keyIndex < 0 || keyIndex >= curve.length) return;
            AnimationUtility.SetKeyRightTangentMode(curve, keyIndex, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyLeftTangentMode(curve, keyIndex, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyBroken(curve, keyIndex, false);
        }

        public Quaternion FixReverseRotationQuaternion(AnimationCurve[] curves, float time, Quaternion rotation)
        {
            Quaternion beforeRotation;
            {
                var beforeTime = 0f;
                for (int i = 0; i < 4; i++)
                {
                    var index = FindBeforeNearKeyframeAtTime(curves[i], time);
                    if (index >= 0)
                        beforeTime = Mathf.Max(beforeTime, curves[i][index].time);
                }
                {
                    Vector4 result = Vector4.zero;
                    for (int i = 0; i < 4; i++)
                        result[i] = curves[i].Evaluate(beforeTime);
                    result.Normalize();
                    beforeRotation = Quaternion.identity;
                    for (int i = 0; i < 4; i++)
                        beforeRotation[i] = result[i];
                }
            }
            var rot = rotation * Quaternion.Inverse(beforeRotation);
            if (rot.w < 0f)
            {
                for (int i = 0; i < 4; i++)
                    rotation[i] = -rotation[i];
            }
            return rotation;
        }
        public Vector3 FixReverseRotationEuler(AnimationCurve[] curves, float time, Vector3 eulerAngles)
        {
            for (int i = 0; i < 3; i++)
            {
                float euler = 0f;
                {
                    var beforeTime = 0f;
                    {
                        var index = FindBeforeNearKeyframeAtTime(curves[i], time);
                        if (index >= 0)
                            beforeTime = Mathf.Max(beforeTime, curves[i][index].time);
                    }
                    euler = curves[i].Evaluate(beforeTime);
                }
                while (Mathf.Abs(eulerAngles[i] - euler) > 180f)
                {
                    if (euler < eulerAngles[i])
                        eulerAngles[i] -= 360f;
                    else
                        eulerAngles[i] += 360f;
                }
            }
            return eulerAngles;
        }
        public bool FixReverseRotationEuler(AnimationCurve curve)
        {
            bool updated = false;
            for (int i = 1; i < curve.length; i++)
            {
                var keyframe = curve[i];
                if (Mathf.Abs(keyframe.value - curve[i - 1].value) <= 180f)
                    continue;
                while (Mathf.Abs(keyframe.value - curve[i - 1].value) > 180f)
                {
                    if (keyframe.value < curve[i - 1].value)
                        keyframe.value += 360f;
                    else
                        keyframe.value -= 360f;
                }
                curve.MoveKey(i, keyframe);
                updated = true;
            }
            return updated;
        }

        private UnityEditor.Animations.AnimatorController GetAnimatorController()
        {
            UnityEditor.Animations.AnimatorController ac = null;
            if (vaw.animator != null)
            {
                if (vaw.animator.runtimeAnimatorController is AnimatorOverrideController)
                {
                    var owc = vaw.animator.runtimeAnimatorController as AnimatorOverrideController;
                    ac = owc.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                }
                else
                {
                    ac = vaw.animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                }
            }
            return ac;
        }
        private void ActionAllAnimatorState(AnimationClip clip, Action<UnityEditor.Animations.AnimatorState> action)
        {
            var ac = GetAnimatorController();
            if (ac == null) return;

            foreach (UnityEditor.Animations.AnimatorControllerLayer layer in ac.layers)
            {
                Action<AnimatorStateMachine> ActionStateMachine = null;
                ActionStateMachine = (stateMachine) =>
                {
                    foreach (var state in stateMachine.states)
                    {
                        if (state.state.motion is UnityEditor.Animations.BlendTree)
                        {
                            Action<UnityEditor.Animations.BlendTree> ActionBlendTree = null;
                            ActionBlendTree = (blendTree) =>
                            {
                                if (blendTree.children == null) return;
                                var children = blendTree.children;
                                for (int j = 0; j < children.Length; j++)
                                {
                                    if (children[j].motion is UnityEditor.Animations.BlendTree)
                                    {
                                        ActionBlendTree(children[j].motion as UnityEditor.Animations.BlendTree);
                                    }
                                    else
                                    {
                                        if (children[j].motion == clip)
                                        {
                                            action(state.state);
                                        }
                                    }
                                }
                            };
                            ActionBlendTree(state.state.motion as UnityEditor.Animations.BlendTree);
                        }
                        else
                        {
                            if (state.state.motion == clip)
                            {
                                action(state.state);
                            }
                        }
                    }
                    foreach (var childStateMachine in stateMachine.stateMachines)
                    {
                        ActionStateMachine(childStateMachine.stateMachine);
                    }
                };
                ActionStateMachine(layer.stateMachine);
            }
        }

        private Transform GetTransformFromPath(string path)
        {
            var root = editGameObject.transform;
            if (!string.IsNullOrEmpty(path))
            {
                var splits = path.Split('/');
                for (int i = 0; i < splits.Length; i++)
                {
                    bool contains = false;
                    for (int j = 0; j < root.childCount; j++)
                    {
                        if (root.GetChild(j).name == splits[i])
                        {
                            root = root.GetChild(j);
                            contains = true;
                            break;
                        }
                    }
                    if (!contains) return null;
                }
            }
            return root;
        }
        #endregion

        private class OnCurveWasModifiedData
        {
            public EditorCurveBinding binding;
            public AnimationUtility.CurveModifiedType deleted;
            public AnimationCurve beforeCurve;
        }
        private Dictionary<int, OnCurveWasModifiedData> curvesWasModified = new Dictionary<int, OnCurveWasModifiedData>();
        private Dictionary<int, OnCurveWasModifiedData> curvesWasModifiedStopped = new Dictionary<int, OnCurveWasModifiedData>();
        private bool OnCurveWasModifiedStop = false;
        private void OnCurveWasModified(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType deleted)
        {
            if (isEditError) return;

            if (currentClip != clip || !IsCheckChangeClipClearEditorCurveCache(clip))
                return;

            if (deleted == AnimationUtility.CurveModifiedType.ClipModified)
            {
                ClearEditorCurveCache();
                return;
            }

            if (!IsVeryAnimationEditableCurveBinding(binding))
                return;

            AnimationCurve beforeCurve = null;
            if (IsContainsEditorCurveCache(binding))
            {
                beforeCurve = GetEditorCurveCache(binding);
            }
            if (deleted == AnimationUtility.CurveModifiedType.CurveModified || 
                deleted == AnimationUtility.CurveModifiedType.CurveDeleted)
            {
                if (editorCurveCacheDic != null)
                {
                    if (editorCurveCacheDic.ContainsKey(GetEditorCurveBindingHashCode(binding)))
                    {
                        RemoveEditorCurveCache(binding);
                    }
                    if (binding.type == typeof(Transform) &&
                        binding.propertyName.StartsWith(URotationCurveInterpolation.PrefixForInterpolation[(int)URotationCurveInterpolation.Mode.NonBaked]))
                    {
                        var bindingSub = binding;
                        foreach (var propertyName in EditorCurveBindingTransformRotationPropertyNames[(int)URotationCurveInterpolation.Mode.RawQuaternions])
                        {
                            bindingSub.propertyName = propertyName;
                            RemoveEditorCurveCache(bindingSub);
                        }
                    }
                }
            }

            #region Ignore undo
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                if (stackTrace.FrameCount >= 2 && stackTrace.GetFrame(1).GetMethod().Name == "Internal_CallAnimationClipAwake")
                    return;
            }
            #endregion

            AddOnCurveWasModified(binding, deleted, beforeCurve);
        }
        private void AddOnCurveWasModified(EditorCurveBinding binding, AnimationUtility.CurveModifiedType deleted, AnimationCurve beforeCurve)
        {
            var hash = GetEditorCurveBindingHashCode(binding);
            Dictionary<int, OnCurveWasModifiedData> dic = !OnCurveWasModifiedStop ? curvesWasModified : curvesWasModifiedStopped;
            OnCurveWasModifiedData data;
            if (dic.TryGetValue(hash, out data))
            {
                if (data.deleted == AnimationUtility.CurveModifiedType.CurveModified &&
                    data.deleted != deleted)
                {
                    data.deleted = deleted;
                }
                if (data.beforeCurve == null && beforeCurve != null)
                {
                    data.beforeCurve = beforeCurve;
                }
            }
            else
            {
                dic.Add(hash, new OnCurveWasModifiedData() { binding = binding, deleted = deleted, beforeCurve = beforeCurve });
            }
        }
        private void SetOnCurveWasModifiedStop(bool flag)
        {
            OnCurveWasModifiedStop = flag;
            if (!flag)
            {
                foreach (var pair in curvesWasModifiedStopped)
                {
                    AddOnCurveWasModified(pair.Value.binding, pair.Value.deleted, pair.Value.beforeCurve);
                }
            }
            curvesWasModifiedStopped.Clear();
        }

        #region EditorCurveBinding
        private void CreateEditorCurveBindingPropertyNames()
        {
            {
                EditorCurveBindingAnimatorIkTPropertyNames = new string[(int)AnimatorIKIndex.Total][];
                EditorCurveBindingAnimatorIkQPropertyNames = new string[(int)AnimatorIKIndex.Total][];
                for (int i = 0; i < (int)AnimatorIKIndex.Total; i++)
                {
                    EditorCurveBindingAnimatorIkTPropertyNames[i] = new string[3];
                    for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                        EditorCurveBindingAnimatorIkTPropertyNames[i][dofIndex] = string.Format("{0}T{1}", (AnimatorIKIndex)i, DofIndex2String[dofIndex]);
                    EditorCurveBindingAnimatorIkQPropertyNames[i] = new string[4];
                    for (int dofIndex = 0; dofIndex < 4; dofIndex++)
                        EditorCurveBindingAnimatorIkQPropertyNames[i][dofIndex] = string.Format("{0}Q{1}", (AnimatorIKIndex)i, DofIndex2String[dofIndex]);
                }
            }
            {
                EditorCurveBindingAnimatorTDOFPropertyNames = new string[(int)AnimatorTDOFIndex.Total][];
                for (int i = 0; i < (int)AnimatorTDOFIndex.Total; i++)
                {
                    EditorCurveBindingAnimatorTDOFPropertyNames[i] = new string[3];
                    for (int dofIndex = 0; dofIndex < 3; dofIndex++)
                        EditorCurveBindingAnimatorTDOFPropertyNames[i][dofIndex] = string.Format("{0}TDOF{1}", AnimatorTDOFIndex2HumanBodyBones[i], DofIndex2String[dofIndex]);
                }
            }
            {
                EditorCurveBindingTransformRotationPropertyNames = new string[(int)URotationCurveInterpolation.Mode.Total][];
                for (int i = 0; i < (int)URotationCurveInterpolation.Mode.Total; i++)
                {
                    int dofCount;
                    switch ((URotationCurveInterpolation.Mode)i)
                    {
                    case URotationCurveInterpolation.Mode.RawQuaternions: dofCount = 4; break;
                    default: dofCount = 3; break;
                    }
                    EditorCurveBindingTransformRotationPropertyNames[i] = new string[dofCount];
                    for (int dofIndex = 0; dofIndex < dofCount; dofIndex++)
                    {
                        if (URotationCurveInterpolation.PrefixForInterpolation[i] == null) continue;
                        EditorCurveBindingTransformRotationPropertyNames[i][dofIndex] = URotationCurveInterpolation.PrefixForInterpolation[i] + DofIndex2String[dofIndex].Remove(0, 1);
                    }
                }
            }
        }
        public readonly EditorCurveBinding[] AnimationCurveBindingAnimatorRootT =
        {
            EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.x"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.y"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.z"),
        };
        public readonly EditorCurveBinding[] AnimationCurveBindingAnimatorRootQ =
        {
            EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.x"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.y"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.z"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ.w"),
        };
        public readonly EditorCurveBinding[] AnimationCurveBindingAnimatorMotionT =
        {
            EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionT.x"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionT.y"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionT.z"),
        };
        public readonly EditorCurveBinding[] AnimationCurveBindingAnimatorMotionQ =
        {
            EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionQ.x"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionQ.y"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionQ.z"),
            EditorCurveBinding.FloatCurve("", typeof(Animator), "MotionQ.w"),
        };
        private string[][] EditorCurveBindingAnimatorIkTPropertyNames;
        public EditorCurveBinding AnimationCurveBindingAnimatorIkT(AnimatorIKIndex ikIndex, int dofIndex)
        {
            return EditorCurveBinding.FloatCurve("", typeof(Animator), EditorCurveBindingAnimatorIkTPropertyNames[(int)ikIndex][dofIndex]);
        }
        private string[][] EditorCurveBindingAnimatorIkQPropertyNames;
        public EditorCurveBinding AnimationCurveBindingAnimatorIkQ(AnimatorIKIndex ikIndex, int dofIndex)
        {
            return EditorCurveBinding.FloatCurve("", typeof(Animator), EditorCurveBindingAnimatorIkQPropertyNames[(int)ikIndex][dofIndex]);
        }
        private string[][] EditorCurveBindingAnimatorTDOFPropertyNames;
        public EditorCurveBinding AnimationCurveBindingAnimatorTDOF(AnimatorTDOFIndex tdofIndex, int dofIndex)
        {
            return EditorCurveBinding.FloatCurve("", typeof(Animator), EditorCurveBindingAnimatorTDOFPropertyNames[(int)tdofIndex][dofIndex]);
        }
        public EditorCurveBinding AnimationCurveBindingAnimatorMuscle(int muscleIndex)
        {
            return EditorCurveBinding.FloatCurve("", typeof(Animator), musclePropertyName.PropertyNames[muscleIndex]);
        }
        public EditorCurveBinding AnimationCurveBindingAnimatorCustom(string propertyName)
        {
            return EditorCurveBinding.FloatCurve("", typeof(Animator), propertyName);
        }
        private static readonly string[] EditorCurveBindingTransformPositionPropertyNames =
        {
            "m_LocalPosition.x",
            "m_LocalPosition.y",
            "m_LocalPosition.z",
        };
        public EditorCurveBinding AnimationCurveBindingTransformPosition(int boneIndex, int dofIndex)
        {
            return EditorCurveBinding.FloatCurve(bonePaths[boneIndex], typeof(Transform), EditorCurveBindingTransformPositionPropertyNames[dofIndex]);
        }
        private string[][] EditorCurveBindingTransformRotationPropertyNames;
        public EditorCurveBinding AnimationCurveBindingTransformRotation(int boneIndex, int dofIndex, URotationCurveInterpolation.Mode mode)
        {
            return EditorCurveBinding.FloatCurve(bonePaths[boneIndex], typeof(Transform), EditorCurveBindingTransformRotationPropertyNames[(int)mode][dofIndex]);
        }
        private static readonly string[] EditorCurveBindingTransformScalePropertyNames =
        {
            "m_LocalScale.x",
            "m_LocalScale.y",
            "m_LocalScale.z",
        };
        public EditorCurveBinding AnimationCurveBindingTransformScale(int boneIndex, int dofIndex)
        {
            return EditorCurveBinding.FloatCurve(bonePaths[boneIndex], typeof(Transform), EditorCurveBindingTransformScalePropertyNames[dofIndex]);
        }
        public EditorCurveBinding AnimationCurveBindingBlendShape(SkinnedMeshRenderer renderer, string name)
        {
            return EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(renderer.transform, editGameObject.transform), typeof(SkinnedMeshRenderer), string.Format("blendShape.{0}", name));
        }

        public int GetBoneIndexFromCurveBinding(EditorCurveBinding binding)
        {
            return GetBoneIndexFromPath(binding.path);
        }
        public int GetBoneIndexFromPath(string path)
        {
            int boneIndex;
            if (bonePathDic.TryGetValue(path, out boneIndex))
            {
                return boneIndex;
            }
            return -1;
        }
        public int GetRootTDofIndexFromCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator)) return -1;
            for (int dofIndex = 0; dofIndex < 3; dofIndex++)
            {
                if (binding == AnimationCurveBindingAnimatorRootT[dofIndex])
                    return dofIndex;
            }
            return -1;
        }
        public int GetRootQDofIndexFromCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator)) return -1;
            for (int dofIndex = 0; dofIndex < 4; dofIndex++)
            {
                if (binding == AnimationCurveBindingAnimatorRootQ[dofIndex])
                    return dofIndex;
            }
            return -1;
        }
        public int GetMuscleIndexFromCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator)) return -1;
            return GetMuscleIndexFromPropertyName(binding.propertyName);
        }
        public int GetMuscleIndexFromPropertyName(string propertyName)
        {
            int muscleIndex;
            if (musclePropertyName.PropertyNameDic.TryGetValue(propertyName, out muscleIndex))
            {
                return muscleIndex;
            }
            return -1;
        }
        public AnimatorIKIndex GetIkTIndexFromCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator)) return AnimatorIKIndex.None;
            for (var ikIndex = 0; ikIndex < AnimatorIKTIndexStrings.Length; ikIndex++)
            {
                if (binding.propertyName.StartsWith(AnimatorIKTIndexStrings[ikIndex]))
                    return (AnimatorIKIndex)ikIndex;
            }
            return AnimatorIKIndex.None;
        }
        public AnimatorIKIndex GetIkQIndexFromCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator)) return AnimatorIKIndex.None;
            for (var ikIndex = 0; ikIndex < AnimatorIKQIndexStrings.Length; ikIndex++)
            {
                if (binding.propertyName.StartsWith(AnimatorIKQIndexStrings[ikIndex]))
                    return (AnimatorIKIndex)ikIndex;
            }
            return AnimatorIKIndex.None;
        }
        public AnimatorTDOFIndex GetTDOFIndexFromCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator)) return AnimatorTDOFIndex.None;
            var indexOf = binding.propertyName.IndexOf("TDOF.");
            if (indexOf < 0) return AnimatorTDOFIndex.None;
            var name = binding.propertyName.Remove(indexOf);
            for (int tdofIndex = 0; tdofIndex < (int)AnimatorTDOFIndex.Total; tdofIndex++)
            {
                if (name == AnimatorTDOFIndexStrings[tdofIndex])
                    return (AnimatorTDOFIndex)tdofIndex;
            }
            return AnimatorTDOFIndex.None;
        }
        public int GetDOFIndexFromCurveBinding(EditorCurveBinding binding)
        {
            for (int i = 0; i < DofIndex2String.Length; i++)
            {
                if (binding.propertyName.EndsWith(DofIndex2String[i]))
                    return i;
            }
            return -1;
        }
        public EditorCurveBinding? GetMirrorAnimationCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type == typeof(Animator))
            {
                int muscleIndex;
                AnimatorIKIndex ikTIndex, ikQIndex;
                AnimatorTDOFIndex tdofIndex;
                if (IsAnimatorRootCurveBinding(binding))
                {
                    return null;
                }
                else if ((muscleIndex = GetMuscleIndexFromCurveBinding(binding)) >= 0)
                {
                    var mmuscleIndex = GetMirrorMuscleIndex(muscleIndex);
                    if (mmuscleIndex < 0) return null;
                    return AnimationCurveBindingAnimatorMuscle(mmuscleIndex);
                }
                else if ((ikTIndex = GetIkTIndexFromCurveBinding(binding)) != AnimatorIKIndex.None)
                {
                    var mikTIndex = AnimatorIKMirrorIndexes[(int)ikTIndex];
                    if (mikTIndex < 0) return null;
                    var dofIndex = GetDOFIndexFromCurveBinding(binding);
                    if (dofIndex < 0) return null;
                    return AnimationCurveBindingAnimatorIkT(mikTIndex, dofIndex);
                }
                else if ((ikQIndex = GetIkQIndexFromCurveBinding(binding)) != AnimatorIKIndex.None)
                {
                    var mikQIndex = AnimatorIKMirrorIndexes[(int)ikQIndex];
                    if (mikQIndex < 0) return null;
                    var dofIndex = GetDOFIndexFromCurveBinding(binding);
                    if (dofIndex < 0) return null;
                    return AnimationCurveBindingAnimatorIkQ(mikQIndex, dofIndex);
                }
                else if ((tdofIndex = GetTDOFIndexFromCurveBinding(binding)) != AnimatorTDOFIndex.None)
                {
                    var mtdofIndex = AnimatorTDOFMirrorIndexes[(int)tdofIndex];
                    if (mtdofIndex < 0) return null;
                    var dofIndex = GetDOFIndexFromCurveBinding(binding);
                    if (dofIndex < 0) return null;
                    return AnimationCurveBindingAnimatorTDOF(mtdofIndex, dofIndex);
                }
                else
                {
                    return null;
                }
            }
            else if (binding.type == typeof(Transform))
            {
                var boneIndex = GetBoneIndexFromCurveBinding(binding);
                if (boneIndex < 0) return null;
                if (mirrorBoneIndexes[boneIndex] < 0) return null;
                binding.path = bonePaths[mirrorBoneIndexes[boneIndex]];
                return binding;
            }
            else if (IsSkinnedMeshRendererBlendShapeCurveBinding(binding))
            {
                var boneIndex = GetBoneIndexFromCurveBinding(binding);
                if (boneIndex < 0) return null;
                var renderer = bones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                if (renderer == null) return null;
                Dictionary<string, string> nameTable;
                if (!mirrorBlendShape.TryGetValue(renderer, out nameTable)) return null;
                string mirrorName;
                var name = binding.propertyName.Remove(0, "blendShape.".Length);
                if (!nameTable.TryGetValue(name, out mirrorName)) return null;
                binding.propertyName = "blendShape." + mirrorName;
                return binding;
            }
            else
            {
                return null;
            }
        }

        public bool IsAnimatorRootCurveBinding(EditorCurveBinding binding)
        {
            return (GetRootTDofIndexFromCurveBinding(binding) >= 0 ||
                    GetRootQDofIndexFromCurveBinding(binding) >= 0);
        }
        public bool IsAnimatorReservedPropertyName(string propertyName)
        {
            for (int dof = 0; dof < 3; dof++)
            {
                if (propertyName == AnimationCurveBindingAnimatorRootT[dof].propertyName)
                    return true;
            }
            for (int dof = 0; dof < 4; dof++)
            {
                if (propertyName == AnimationCurveBindingAnimatorRootQ[dof].propertyName)
                    return true;
            }
            for (int dof = 0; dof < 3; dof++)
            {
                if (propertyName == AnimationCurveBindingAnimatorMotionT[dof].propertyName)
                    return true;
            }
            for (int dof = 0; dof < 4; dof++)
            {
                if (propertyName == AnimationCurveBindingAnimatorMotionQ[dof].propertyName)
                    return true;
            }
            for (var i = 0; i < (int)AnimatorIKIndex.Total; i++)
            {
                for (int dof = 0; dof < 3; dof++)
                {
                    if (propertyName == EditorCurveBindingAnimatorIkTPropertyNames[i][dof])
                        return true;
                }
                for (int dof = 0; dof < 4; dof++)
                {
                    if (propertyName == EditorCurveBindingAnimatorIkQPropertyNames[i][dof])
                        return true;
                }
            }
            for (var i = 0; i < (int)AnimatorTDOFIndex.Total; i++)
            {
                for (int dof = 0; dof < 3; dof++)
                {
                    if (propertyName == EditorCurveBindingAnimatorTDOFPropertyNames[i][dof])
                        return true;
                }
            }
            if (GetMuscleIndexFromPropertyName(propertyName) >= 0)
                return true;

            return false;
        }
        public bool IsTransformPositionCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Transform)) return false;
            return binding.propertyName.StartsWith("m_LocalPosition.");
        }
        public bool IsTransformRotationCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Transform)) return false;
            for (int i = 0; i < URotationCurveInterpolation.PrefixForInterpolation.Length; i++)
            {
                if (URotationCurveInterpolation.PrefixForInterpolation[i] == null) continue;
                if (binding.propertyName.StartsWith(URotationCurveInterpolation.PrefixForInterpolation[i]))
                    return true;
            }
            return false;
        }
        public bool IsTransformScaleCurveBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Transform)) return false;
            return binding.propertyName.StartsWith("m_LocalScale.");
        }
        public bool IsSkinnedMeshRendererBlendShapeCurveBinding(EditorCurveBinding binding)
        {
            return binding.type == typeof(SkinnedMeshRenderer) && binding.propertyName.StartsWith("blendShape.");
        }
        public bool IsVeryAnimationEditableCurveBinding(EditorCurveBinding binding)
        {
            return (binding.type == typeof(Animator) || binding.type == typeof(Transform) || IsSkinnedMeshRendererBlendShapeCurveBinding(binding));
        }
        #endregion

        #region HumanPose
        public void GetHumanPose(ref HumanPose humanPose)
        {
            if (!isHuman || editHumanPoseHandler == null) return;
            var t = editGameObject.transform;
            TransformPoseSave.SaveData save = new TransformPoseSave.SaveData(t);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            editHumanPoseHandler.GetHumanPose(ref humanPose);
            save.LoadLocal(t);
        }
        public void GetHumanPoseCurve(ref HumanPose humanPose, float time = -1f)
        {
            humanPose.bodyPosition = GetAnimationValueAnimatorRootT(time);
            humanPose.bodyRotation = GetAnimationValueAnimatorRootQ(time);
            humanPose.muscles = new float[HumanTrait.MuscleCount];
            for (int i = 0; i < humanPose.muscles.Length; i++)
            {
                humanPose.muscles[i] = GetAnimationValueAnimatorMuscle(i, time);
            }
        }
        #endregion

        #region EditorCurveCache
        #region EditorCurveBindingCache
        private Dictionary<string, Dictionary<Type, Dictionary<string, int>>> editorCurveBindingHashCacheDic;
        private void ClearEditorCurveBindingHashCode()
        {
            if (editorCurveBindingHashCacheDic == null)
                editorCurveBindingHashCacheDic = new Dictionary<string, Dictionary<Type, Dictionary<string, int>>>();
            else
                editorCurveBindingHashCacheDic.Clear();
        }
        public int GetEditorCurveBindingHashCode(EditorCurveBinding binding)
        {
            if (binding.path == null || binding.propertyName == null)
                return binding.GetHashCode();

            if (editorCurveBindingHashCacheDic == null)
                editorCurveBindingHashCacheDic = new Dictionary<string, Dictionary<Type, Dictionary<string, int>>>();

            int hashCode;
            Dictionary<Type, Dictionary<string, int>> typeNameDic;
            if (editorCurveBindingHashCacheDic.TryGetValue(binding.path, out typeNameDic))
            {
                Dictionary<string, int> propertyNameDic;
                if (typeNameDic.TryGetValue(binding.type, out propertyNameDic))
                {
                    if (propertyNameDic.TryGetValue(binding.propertyName, out hashCode))
                    {
                        return hashCode;
                    }
                    else
                    {
                        hashCode = binding.GetHashCode();
                        propertyNameDic.Add(binding.propertyName, hashCode);
                    }
                }
                else
                {
                    propertyNameDic = new Dictionary<string, int>();
                    hashCode = binding.GetHashCode();
                    propertyNameDic.Add(binding.propertyName, hashCode);
                    typeNameDic.Add(binding.type, propertyNameDic);
                }
            }
            else
            {
                typeNameDic = new Dictionary<Type, Dictionary<string, int>>();
                hashCode = binding.GetHashCode();
                {
                    var propertyNameDic = new Dictionary<string, int>();
                    propertyNameDic.Add(binding.propertyName, hashCode);
                    typeNameDic.Add(binding.type, propertyNameDic);
                }
                editorCurveBindingHashCacheDic.Add(binding.path, typeNameDic);
            }

            return hashCode;
        }
        #endregion

        private AnimationClip editorCurveCacheClip;
        private bool editorCurveCacheDirty;
        private class EditorCurveCacheDicData
        {
            public EditorCurveCacheDicData(AnimationCurve curve)
            {
                this.curve = curve;
                beforeKeys = new List<Keyframe>();
            }

            public AnimationCurve curve;
            public List<Keyframe> beforeKeys;
        }
        private Dictionary<int, EditorCurveCacheDicData> editorCurveCacheDic;

        private void ClearEditorCurveCache()
        {
            ClearEditorCurveBindingHashCode();

            editorCurveCacheClip = null;
            if (editorCurveCacheDic == null)
                editorCurveCacheDic = new Dictionary<int, EditorCurveCacheDicData>();
            else
                editorCurveCacheDic.Clear();

            editorCurveCacheDirty = true;
        }
        private void RemoveEditorCurveCache(EditorCurveBinding binding)
        {
            CheckChangeClipClearEditorCurveCache();
            if (editorCurveCacheDic == null) return;
            var hash = GetEditorCurveBindingHashCode(binding);
            if (editorCurveCacheDic.Remove(hash))
            {
                editorCurveCacheDirty = true;
            }
        }
        private bool IsCheckChangeClipClearEditorCurveCache(AnimationClip clip)
        {
            return clip == editorCurveCacheClip;
        }
        private void CheckChangeClipClearEditorCurveCache()
        {
            if (!IsCheckChangeClipClearEditorCurveCache(currentClip))
            {
                ClearEditorCurveCache();
                editorCurveCacheClip = currentClip;
            }
        }
        private bool IsContainsEditorCurveCache(EditorCurveBinding binding)
        {
            CheckChangeClipClearEditorCurveCache();
            var hash = GetEditorCurveBindingHashCode(binding);
            return editorCurveCacheDic.ContainsKey(hash);
        }
        private AnimationCurve GetEditorCurveCache(EditorCurveBinding binding)
        {
            CheckChangeClipClearEditorCurveCache();
            if (editorCurveCacheDic == null)
                return null;
            var hash = GetEditorCurveBindingHashCode(binding);
            EditorCurveCacheDicData data = null;
            if (!editorCurveCacheDic.TryGetValue(hash, out data))
            {
                var curve = AnimationUtility.GetEditorCurve(currentClip, binding);     //If an error occurs on this line, execute Tools/Fix Errors.
                data = new EditorCurveCacheDicData(curve);
                if (curve != null)
                {
                    if (data.beforeKeys.Capacity < curve.length)
                        data.beforeKeys.Capacity = curve.length;
                    for (int i = 0; i < curve.length; i++)
                        data.beforeKeys.Add(curve[i]);
                }
                editorCurveCacheDic.Add(hash, data);
            }
            return data.curve;
        }
        private void SetEditorCurveCache(EditorCurveBinding binding, AnimationCurve curve)
        {
            CheckChangeClipClearEditorCurveCache();
            uAnimationUtility.Internal_SetEditorCurve(currentClip, binding, curve, false);
            if (needSyncEditorCurveClip != null && needSyncEditorCurveClip != currentClip)
                UpdateSyncEditorCurveClip();
            needSyncEditorCurveClip = currentClip;
            var hash = GetEditorCurveBindingHashCode(binding);
            if (editorCurveCacheDic == null)
                editorCurveCacheDic = new Dictionary<int, EditorCurveCacheDicData>();
            EditorCurveCacheDicData data = null;
            if (!editorCurveCacheDic.TryGetValue(hash, out data))
                data = new EditorCurveCacheDicData(curve);
            else
                data.curve = curve;
            {
                var type = curve != null ? AnimationUtility.CurveModifiedType.CurveModified : AnimationUtility.CurveModifiedType.CurveDeleted;
                if (binding.type == typeof(Transform) &&
                    binding.propertyName.StartsWith(URotationCurveInterpolation.PrefixForInterpolation[(int)URotationCurveInterpolation.Mode.RawQuaternions]))
                {
                    var bindingSub = binding;
                    for (int dof = 0; dof < 3; dof++)
                    {
                        bindingSub.propertyName = EditorCurveBindingTransformRotationPropertyNames[(int)URotationCurveInterpolation.Mode.NonBaked][dof];
                        uAw.CurveWasModified(currentClip, bindingSub, type);
                    }
                }
                else
                {
                    uAw.CurveWasModified(currentClip, binding, type);
                }
                AddOnCurveWasModified(binding, type, new AnimationCurve(data.beforeKeys.ToArray()));
            }
            data.beforeKeys.Clear();
            if (curve != null)
            {
                if (data.beforeKeys.Capacity < curve.length)
                    data.beforeKeys.Capacity = curve.length;
                for (int i = 0; i < curve.length; i++)
                    data.beforeKeys.Add(curve[i]);
            }
            editorCurveCacheDic[hash] = data;
        }
        #endregion

        #region PoseTemplate
        public void SavePoseTemplate(PoseTemplate poseTemplate)
        {
            poseTemplate.Reset();
            poseTemplate.isHuman = isHuman;
            #region Human
            if (isHuman)
            {
                poseTemplate.haveRootT = true;
                poseTemplate.rootT = GetAnimationValueAnimatorRootT();
                poseTemplate.haveRootQ = true;
                poseTemplate.rootQ = GetAnimationValueAnimatorRootQ();
                {
                    Dictionary<string, float> muscleList = new Dictionary<string, float>();
                    for (int muscleIndex = 0; muscleIndex < musclePropertyName.PropertyNames.Length; muscleIndex++)
                        muscleList.Add(musclePropertyName.PropertyNames[muscleIndex], GetAnimationValueAnimatorMuscle(muscleIndex));
                    poseTemplate.musclePropertyNames = muscleList.Keys.ToArray();
                    poseTemplate.muscleValues = muscleList.Values.ToArray();
                }
                {
                    Dictionary<AnimatorTDOFIndex, Vector3> tdofIndices = new Dictionary<AnimatorTDOFIndex, Vector3>();
                    for (AnimatorTDOFIndex tdofIndex = 0; tdofIndex < AnimatorTDOFIndex.Total; tdofIndex++)
                        tdofIndices.Add(tdofIndex, GetAnimationValueAnimatorTDOF(tdofIndex));
                    poseTemplate.tdofIndices = tdofIndices.Keys.ToArray();
                    poseTemplate.tdofValues = tdofIndices.Values.ToArray();
                }
                {
                    Dictionary<AnimatorIKIndex, PoseTemplate.IKData> ikIndices = new Dictionary<AnimatorIKIndex, PoseTemplate.IKData>();
                    for (AnimatorIKIndex ikIndex = 0; ikIndex < AnimatorIKIndex.Total; ikIndex++)
                    {
                        ikIndices.Add(ikIndex, new PoseTemplate.IKData()
                        {
                            position = GetAnimationValueAnimatorIkT(ikIndex),
                            rotation = GetAnimationValueAnimatorIkQ(ikIndex),
                        });
                    }
                    poseTemplate.ikIndices = ikIndices.Keys.ToArray();
                    poseTemplate.ikValues = ikIndices.Values.ToArray();
                }
            }
            #endregion
            #region Generic
            {
                if (!isHuman && rootMotionBoneIndex >= 0)
                {
                    poseTemplate.haveRootT = true;
                    poseTemplate.rootT = GetAnimationValueAnimatorRootT();
                    poseTemplate.haveRootQ = true;
                    poseTemplate.rootQ = GetAnimationValueAnimatorRootQ();
                }
                Dictionary<string, PoseTemplate.TransformData> transformList = new Dictionary<string, PoseTemplate.TransformData>();
                {   //Root
                    var boneIndex = 0;
                    var t = editBones[boneIndex].transform;
                    transformList.Add(bonePaths[boneIndex], new PoseTemplate.TransformData()
                    {
                        position = t.localPosition - transformPoseSave.startPosition,
                        rotation = Quaternion.Inverse(transformPoseSave.startRotation) * t.localRotation,
                        scale = t.localScale - transformPoseSave.startScale,
                    });
                }
                for (int boneIndex = 1; boneIndex < bones.Length; boneIndex++)
                {
                    if (isHuman && humanoidConflict[boneIndex])
                    {
                        var t = editBones[boneIndex].transform;
                        transformList.Add(bonePaths[boneIndex], new PoseTemplate.TransformData()
                        {
                            position = t.localPosition,
                            rotation = t.localRotation,
                            scale = t.localScale,
                        });
                    }
                    else
                    {
                        transformList.Add(bonePaths[boneIndex], new PoseTemplate.TransformData()
                        {
                            position = GetAnimationValueTransformPosition(boneIndex),
                            rotation = GetAnimationValueTransformRotation(boneIndex),
                            scale = GetAnimationValueTransformScale(boneIndex),
                        });
                    }
                }
                poseTemplate.transformPaths = transformList.Keys.ToArray();
                poseTemplate.transformValues = transformList.Values.ToArray();
            }
            #endregion
            #region BlendShape
            {
                Dictionary<string, PoseTemplate.BlendShapeData> blendShapeList = new Dictionary<string, PoseTemplate.BlendShapeData>();
                foreach (var renderer in editGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount <= 0) continue;
                    var path = AnimationUtility.CalculateTransformPath(renderer.transform, editGameObject.transform);
                    var data = new PoseTemplate.BlendShapeData()
                    {
                        names = new string[renderer.sharedMesh.blendShapeCount],
                        weights = new float[renderer.sharedMesh.blendShapeCount],
                    };
                    for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                    {
                        data.names[i] = renderer.sharedMesh.GetBlendShapeName(i);
                        data.weights[i] = GetAnimationValueBlendShape(renderer, data.names[i]);
                    }
                    blendShapeList.Add(path, data);
                }
                poseTemplate.blendShapePaths = blendShapeList.Keys.ToArray();
                poseTemplate.blendShapeValues = blendShapeList.Values.ToArray();
            }
            #endregion
        }
        public void SaveSelectionPoseTemplate(PoseTemplate poseTemplate)
        {
            bool selectRoot = SelectionGameObjectsIndexOf(vaw.gameObject) >= 0;
            var selectHumanoidIndexes = SelectionGameObjectsHumanoidIndex();
            var selectMuscleIndexes = SelectionGameObjectsMuscleIndex();
            var selectAnimatorIKTargetsHumanoidIndexes = animatorIK.SelectionAnimatorIKTargetsHumanoidIndexes();
            var selectOriginalIKTargetsBoneIndexes = originalIK.SelectionOriginalIKTargetsBoneIndexes();
            //
            poseTemplate.Reset();
            poseTemplate.isHuman = isHuman;
            #region Human
            if (isHuman)
            {
                if (selectRoot)
                {
                    poseTemplate.haveRootT = true;
                    poseTemplate.rootT = GetAnimationValueAnimatorRootT();
                    poseTemplate.haveRootQ = true;
                    poseTemplate.rootQ = GetAnimationValueAnimatorRootQ();
                }
                {
                    Dictionary<string, float> muscleList = new Dictionary<string, float>();
                    for (int muscleIndex = 0; muscleIndex < musclePropertyName.PropertyNames.Length; muscleIndex++)
                    {
                        if (selectMuscleIndexes.Contains(muscleIndex) ||
                            selectAnimatorIKTargetsHumanoidIndexes.Contains((HumanBodyBones)HumanTrait.BoneFromMuscle(muscleIndex)))
                        {
                            muscleList.Add(musclePropertyName.PropertyNames[muscleIndex], GetAnimationValueAnimatorMuscle(muscleIndex));
                        }
                    }
                    poseTemplate.musclePropertyNames = muscleList.Keys.ToArray();
                    poseTemplate.muscleValues = muscleList.Values.ToArray();
                }
                {
                    Dictionary<AnimatorTDOFIndex, Vector3> tdofIndices = new Dictionary<AnimatorTDOFIndex, Vector3>();
                    for (AnimatorTDOFIndex tdofIndex = 0; tdofIndex < AnimatorTDOFIndex.Total; tdofIndex++)
                    {
                        if (selectHumanoidIndexes.Contains(AnimatorTDOFIndex2HumanBodyBones[(int)tdofIndex]) ||
                            selectAnimatorIKTargetsHumanoidIndexes.Contains(AnimatorTDOFIndex2HumanBodyBones[(int)tdofIndex]))
                        {
                            tdofIndices.Add(tdofIndex, GetAnimationValueAnimatorTDOF(tdofIndex));
                        }
                    }
                    poseTemplate.tdofIndices = tdofIndices.Keys.ToArray();
                    poseTemplate.tdofValues = tdofIndices.Values.ToArray();
                }
                {
                    Dictionary<AnimatorIKIndex, PoseTemplate.IKData> ikIndices = new Dictionary<AnimatorIKIndex, PoseTemplate.IKData>();
                    for (AnimatorIKIndex ikIndex = 0; ikIndex < AnimatorIKIndex.Total; ikIndex++)
                    {
                        if (selectHumanoidIndexes.Contains(AnimatorIKIndex2HumanBodyBones[(int)ikIndex]) ||
                            selectAnimatorIKTargetsHumanoidIndexes.Contains(AnimatorIKIndex2HumanBodyBones[(int)ikIndex]))
                        {
                            ikIndices.Add(ikIndex, new PoseTemplate.IKData()
                            {
                                position = GetAnimationValueAnimatorIkT(ikIndex),
                                rotation = GetAnimationValueAnimatorIkQ(ikIndex),
                            });
                        }
                    }
                    poseTemplate.ikIndices = ikIndices.Keys.ToArray();
                    poseTemplate.ikValues = ikIndices.Values.ToArray();
                }
            }
            #endregion
            #region Generic
            {
                if (!isHuman && rootMotionBoneIndex >= 0 && selectionBones.Contains(rootMotionBoneIndex))
                {
                    poseTemplate.haveRootT = true;
                    poseTemplate.rootT = GetAnimationValueAnimatorRootT();
                    poseTemplate.haveRootQ = true;
                    poseTemplate.rootQ = GetAnimationValueAnimatorRootQ();
                }
                Dictionary<string, PoseTemplate.TransformData> transformList = new Dictionary<string, PoseTemplate.TransformData>();
                {   //Root
                    var boneIndex = 0;
                    if (selectionBones.Contains(boneIndex) ||
                        selectOriginalIKTargetsBoneIndexes.Contains(boneIndex))
                    {
                        var t = editBones[boneIndex].transform;
                        transformList.Add(bonePaths[boneIndex], new PoseTemplate.TransformData()
                        {
                            position = t.localPosition - transformPoseSave.startPosition,
                            rotation = Quaternion.Inverse(transformPoseSave.startRotation) * t.localRotation,
                            scale = t.localScale - transformPoseSave.startScale,
                        });
                    }
                }
                for (int boneIndex = 1; boneIndex < bones.Length; boneIndex++)
                {
                    if (selectionBones.Contains(boneIndex) ||
                        selectOriginalIKTargetsBoneIndexes.Contains(boneIndex))
                    {
                        if (isHuman && humanoidConflict[boneIndex])
                        {
                            var t = editBones[boneIndex].transform;
                            transformList.Add(bonePaths[boneIndex], new PoseTemplate.TransformData()
                            {
                                position = t.localPosition,
                                rotation = t.localRotation,
                                scale = t.localScale,
                            });
                        }
                        else
                        {
                            transformList.Add(bonePaths[boneIndex], new PoseTemplate.TransformData()
                            {
                                position = GetAnimationValueTransformPosition(boneIndex),
                                rotation = GetAnimationValueTransformRotation(boneIndex),
                                scale = GetAnimationValueTransformScale(boneIndex),
                            });
                        }
                    }
                }
                poseTemplate.transformPaths = transformList.Keys.ToArray();
                poseTemplate.transformValues = transformList.Values.ToArray();
            }
            #endregion
            #region BlendShape
            {
                Dictionary<string, PoseTemplate.BlendShapeData> blendShapeList = new Dictionary<string, PoseTemplate.BlendShapeData>();
                foreach (var boneIndex in selectionBones)
                {
                    var renderer = editBones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                    if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount <= 0) continue;
                    var path = AnimationUtility.CalculateTransformPath(renderer.transform, editGameObject.transform);
                    var data = new PoseTemplate.BlendShapeData()
                    {
                        names = new string[renderer.sharedMesh.blendShapeCount],
                        weights = new float[renderer.sharedMesh.blendShapeCount],
                    };
                    for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                    {
                        data.names[i] = renderer.sharedMesh.GetBlendShapeName(i);
                        data.weights[i] = GetAnimationValueBlendShape(renderer, data.names[i]);
                    }
                    blendShapeList.Add(path, data);
                }
                poseTemplate.blendShapePaths = blendShapeList.Keys.ToArray();
                poseTemplate.blendShapeValues = blendShapeList.Values.ToArray();
            }
            #endregion
        }
        public void LoadPoseTemplate(PoseTemplate poseTemplate, bool updateAll = false)
        {
            if (updateAll)
            {
                ResetAllHaveAnimationCurve();
            }
            #region Human
            if (isHuman && poseTemplate.isHuman)
            {
                if (poseTemplate.haveRootT)
                {
                    SetAnimationValueAnimatorRootT(poseTemplate.rootT);
                }
                if (poseTemplate.haveRootQ)
                {
                    SetAnimationValueAnimatorRootQ(poseTemplate.rootQ);
                }
                if (poseTemplate.musclePropertyNames != null && poseTemplate.muscleValues != null)
                {
                    Assert.IsTrue(poseTemplate.musclePropertyNames.Length == poseTemplate.muscleValues.Length);
                    for (int i = 0; i < poseTemplate.musclePropertyNames.Length; i++)
                    {
                        var muscleIndex = GetMuscleIndexFromPropertyName(poseTemplate.musclePropertyNames[i]);
                        if (muscleIndex < 0) continue;
                        var value = poseTemplate.muscleValues[i];
                        if (!IsHaveAnimationCurveAnimatorMuscle(muscleIndex) && value == 0f) continue;
                        SetAnimationValueAnimatorMuscle(muscleIndex, poseTemplate.muscleValues[i]);
                    }
                }
                if (poseTemplate.tdofIndices != null && poseTemplate.tdofValues != null)
                {
                    Assert.IsTrue(poseTemplate.tdofIndices.Length == poseTemplate.tdofValues.Length);
                    for (int i = 0; i < poseTemplate.tdofIndices.Length; i++)
                    {
                        var tdofIndex = poseTemplate.tdofIndices[i];
                        var value = poseTemplate.tdofValues[i];
                        if (!IsHaveAnimationCurveAnimatorTDOF(tdofIndex) && value == Vector3.zero) continue;
                        SetAnimationValueAnimatorTDOF(tdofIndex, value);
                    }
                }
                if (poseTemplate.ikIndices != null && poseTemplate.ikValues != null)
                {
                    Assert.IsTrue(poseTemplate.ikIndices.Length == poseTemplate.ikValues.Length);
                    for (int i = 0; i < poseTemplate.ikIndices.Length; i++)
                    {
                        var ikIndex = poseTemplate.ikIndices[i];
                        var value = poseTemplate.ikValues[i];
                        if (IsHaveAnimationCurveAnimatorIkT(ikIndex) || value.position != Vector3.zero)
                        {
                            SetAnimationValueAnimatorIkT(ikIndex, value.position);
                        }
                        if (IsHaveAnimationCurveAnimatorIkQ(ikIndex) || value.rotation != Quaternion.identity)
                        {
                            SetAnimationValueAnimatorIkQ(ikIndex, value.rotation);
                        }
                    }
                }
            }
            #endregion
            #region Generic
            if (!isHuman && !poseTemplate.isHuman)
            {
                if (rootMotionBoneIndex >= 0)
                {
                    if (poseTemplate.haveRootT)
                    {
                        SetAnimationValueAnimatorRootT(poseTemplate.rootT);
                    }
                    if (poseTemplate.haveRootQ)
                    {
                        SetAnimationValueAnimatorRootQ(poseTemplate.rootQ);
                    }
                }
            }
            if (poseTemplate.transformPaths != null && poseTemplate.transformValues != null)
            {
                Assert.IsTrue(poseTemplate.transformPaths.Length == poseTemplate.transformValues.Length);
                for (int i = 0; i < poseTemplate.transformPaths.Length; i++)
                {
                    var boneIndex = GetBoneIndexFromPath(poseTemplate.transformPaths[i]);
                    if (boneIndex < 0 || (isHuman && humanoidConflict[boneIndex])) continue;
                    var position = poseTemplate.transformValues[i].position;
                    var rotation = poseTemplate.transformValues[i].rotation;
                    var scale = poseTemplate.transformValues[i].scale;
                    if (boneIndex == 0)
                    {   //Root
                        if (rootMotionBoneIndex >= 0) continue;
                        position = transformPoseSave.startPosition + position;
                        rotation = transformPoseSave.startRotation * rotation;
                        scale = transformPoseSave.startScale + scale;
                    }
                    else if (rootMotionBoneIndex >= 0 && boneIndex == rootMotionBoneIndex)
                    {
                        updateGenericRootMotion = true;
                        continue;
                    }
                    if (IsHaveAnimationCurveTransformPosition(boneIndex) || position != boneSaveTransforms[boneIndex].localPosition)
                        SetAnimationValueTransformPosition(boneIndex, position);
                    if (IsHaveAnimationCurveTransformRotation(boneIndex) != URotationCurveInterpolation.Mode.Undefined || rotation != boneSaveTransforms[boneIndex].localRotation)
                        SetAnimationValueTransformRotation(boneIndex, rotation);
                    if (IsHaveAnimationCurveTransformScale(boneIndex) || scale != boneSaveTransforms[boneIndex].localScale)
                        SetAnimationValueTransformScale(boneIndex, scale);
                }
            }
            #endregion
            #region BlendShape
            if (poseTemplate.blendShapePaths != null && poseTemplate.blendShapeValues != null)
            {
                foreach (var renderer in editGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount <= 0) continue;
                    var path = AnimationUtility.CalculateTransformPath(renderer.transform, editGameObject.transform);
                    var index = EditorCommon.ArrayIndexOf(poseTemplate.blendShapePaths, path);
                    if (index < 0) continue;
                    for (int i = 0; i < poseTemplate.blendShapeValues[index].names.Length; i++)
                    {
                        if (IsHaveAnimationCurveBlendShape(renderer, poseTemplate.blendShapeValues[index].names[i]) || poseTemplate.blendShapeValues[index].weights[i] != blendShapeWeightSave.GetOriginalWeight(renderer, poseTemplate.blendShapeValues[index].names[i]))
                            SetAnimationValueBlendShape(renderer, poseTemplate.blendShapeValues[index].names[i], poseTemplate.blendShapeValues[index].weights[i]);
                    }
                }
            }
            #endregion
            SetPoseAfter();
        }
        #endregion

        #region IK
        public void IKHandleGUI()
        {
            animatorIK.HandleGUI();
            originalIK.HandleGUI();
        }
        public void IKTargetGUI()
        {
            animatorIK.TargetGUI();
            originalIK.TargetGUI();
        }

        private void IKUpdateBones()
        {
            animatorIK.UpdateBones();
        }
        private void IKChangeSelection()
        {
            if (animatorIK.ChangeSelectionIK()) return;
            if (originalIK.ChangeSelectionIK()) return;
        }

        public void ClearIkTargetSelect()
        {
            animatorIK.ikTargetSelect = null;
            animatorIK.OnSelectionChange();
            originalIK.ikTargetSelect = null;
            originalIK.OnSelectionChange();
        }

        public void SetUpdateSelectionIKtarget()
        {
            if (animatorIK.ikTargetSelect != null)
            {
                foreach (var ikTarget in animatorIK.ikTargetSelect)
                {
                    animatorIK.SetUpdateIKtargetAnimatorIK(ikTarget);
                }
            }
            if (originalIK.ikTargetSelect != null)
            {
                foreach (var ikTarget in originalIK.ikTargetSelect)
                {
                    originalIK.SetUpdateIKtargetOriginalIK(ikTarget);
                }
            }
        }

        public bool IsIKBone(HumanBodyBones humanoidIndex)
        {
            return animatorIK.IsIKBone(humanoidIndex) != AnimatorIKCore.IKTarget.None ||
                    originalIK.IsIKBone(humanoidIndex) >= 0;
        }
        public bool IsIKBone(int boneIndex)
        {
            return animatorIK.IsIKBone(boneIndex2humanoidIndex[boneIndex]) != AnimatorIKCore.IKTarget.None ||
                    originalIK.IsIKBone(boneIndex) >= 0;
        }

        public void SetUpdateIKtargetBone(int boneIndex)
        {
            if (boneIndex < 0) return;
            originalIK.SetUpdateIKtargetBone(boneIndex);
        }
        public void SetUpdateIKtargetMuscle(int muscleIndex)
        {
            if (muscleIndex < 0) return;
            animatorIK.SetUpdateIKtargetMuscle(muscleIndex);
            originalIK.SetUpdateIKtargetMuscle(muscleIndex);
        }
        public void SetUpdateIKtargetHumanoidIndex(HumanBodyBones humanoidIndex)
        {
            if (humanoidIndex < 0) return;
            animatorIK.SetUpdateIKtargetHumanoidIndex(humanoidIndex);
            originalIK.SetUpdateIKtargetHumanoidIndex(humanoidIndex);
        }
        public void SetUpdateIKtargetTdofIndex(AnimatorTDOFIndex tdofIndex)
        {
            if (tdofIndex < 0) return;
            animatorIK.SetUpdateIKtargetTdofIndex(tdofIndex);
            originalIK.SetUpdateIKtargetTdofIndex(tdofIndex);
        }
        public void SetUpdateIKtargetAnimatorIK(AnimatorIKCore.IKTarget ikTarget)
        {
            if (ikTarget < 0) return;
            animatorIK.SetUpdateIKtargetAnimatorIK(ikTarget);
            for (int humanoidIndex = 0; humanoidIndex < AnimatorIKCore.HumanBonesUpdateAnimatorIK.Length; humanoidIndex++)
            {
                if (AnimatorIKCore.HumanBonesUpdateAnimatorIK[humanoidIndex] == ikTarget)
                {
                    originalIK.SetUpdateIKtargetBone(humanoidIndex2boneIndex[humanoidIndex]);
                }
            }
        }
        public void SetUpdateIKtargetOriginalIK(int ikTarget)
        {
            if (ikTarget < 0) return;
            originalIK.SetUpdateIKtargetOriginalIK(ikTarget);
        }
        public void SetUpdateIKtargetAll(bool flag)
        {
            animatorIK.SetUpdateIKtargetAll(flag);
            originalIK.SetUpdateIKtargetAll(flag);
        }
        public bool GetUpdateIKtargetAll()
        {
            return animatorIK.GetUpdateIKtargetAll() || originalIK.GetUpdateIKtargetAll();
        }

        public void SetSynchroIKtargetAll(bool flag)
        {
            animatorIK.SetSynchroIKtargetAll(flag);
            originalIK.SetSynchroIKtargetAll(flag);
        }
        public bool GetSynchroIKtargetAll()
        {
            return animatorIK.GetSynchroIKtargetAll() || originalIK.GetSynchroIKtargetAll();
        }
        public void UpdateSynchroIKSet()
        {
            animatorIK.UpdateSynchroIKSet();
            originalIK.UpdateSynchroIKSet();
        }
        #endregion

        #region AnimationCurve
        private class TmpCurves
        {
            public AnimationCurve[] curves = new AnimationCurve[4];

            public void Clear()
            {
                for (int i = 0; i < 4; i++)
                {
                    curves[i] = null;
                }
            }
        }
        private TmpCurves tmpCurves = new TmpCurves();

        private bool beginChangeAnimationCurve;
        private bool BeginChangeAnimationCurve(AnimationClip clip, string undoName)
        {
            SetUpdateResampleAnimation();
            if (!beginChangeAnimationCurve)
            {
                if (clip == null) return false;
                if ((clip.hideFlags & HideFlags.NotEditable) != HideFlags.None)
                {
                    EditorCommon.ShowNotification("Read-Only");
                    Debug.LogErrorFormat(Language.GetText(Language.Help.LogAnimationClipReadOnlyError), clip.name);
                    return false;
                }

                Undo.RegisterCompleteObjectUndo(clip, undoName);
                uAnimatorControllerTool.SetAnimatorController(null);
                uParameterControllerEditor.SetAnimatorController(null);
                beginChangeAnimationCurve = true;
            }
            return true;
        }
        private void EndChangeAnimationCurve()
        {
            if (!beginChangeAnimationCurve) return;
            if (vaw.animator != null)
            {
                var ac = GetAnimatorController();
                uAnimatorControllerTool.SetAnimatorController(ac);
                uParameterControllerEditor.SetAnimatorController(ac);
            }
            beginChangeAnimationCurve = false;
        }

        public void SetPoseHumanoidDefault()
        {
            ResetAllHaveAnimationCurve();
            SetPoseAfter();
        }
        public void SetPoseEditStart()
        {
            ResetAllHaveAnimationCurve();
            SetAllChangedAnimationCurve();
            SetPoseAfter();
        }
        public void SetPoseBind()
        {
            ResetAllHaveAnimationCurve();
            transformPoseSave.ResetBindTransform();
            SetAllChangedAnimationCurve();
            SetPoseAfter();
        }
        public void SetPosePrefab()
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(vaw.gameObject) as GameObject;
            if (prefab == null) return;

            ResetAllHaveAnimationCurve();
            transformPoseSave.ResetPrefabTransform();
            blendShapeWeightSave.ResetPrefabWeight();
            SetAllChangedAnimationCurve();
            SetPoseAfter();
        }
        public void SetPoseMirror()
        {
            #region Humanoid
            {
                var rootT = GetAnimationValueAnimatorRootT();
                if (IsHaveAnimationCurveAnimatorRootT())
                    SetAnimationValueAnimatorRootT(new Vector3(-rootT.x, rootT.y, rootT.z));
                var rootQ = GetAnimationValueAnimatorRootQ();
                if (IsHaveAnimationCurveAnimatorRootQ())
                    SetAnimationValueAnimatorRootQ(new Quaternion(rootQ.x, -rootQ.y, -rootQ.z, rootQ.w));
            }
            {
                var values = new float[HumanTrait.MuscleCount];
                for (int i = 0; i < values.Length; i++)
                {
                    var mmi = GetMirrorMuscleIndex(i);
                    if (mmi < 0)
                        values[i] = float.MaxValue;
                    else
                        values[i] = GetAnimationValueAnimatorMuscle(mmi);
                }
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == float.MaxValue)
                    {
                        var hi = HumanTrait.BoneFromMuscle(i);
                        if (i == HumanTrait.MuscleFromBone(hi, 0) || i == HumanTrait.MuscleFromBone(hi, 1))
                        {
                            if (!IsHaveAnimationCurveAnimatorMuscle(i)) continue;
                            SetAnimationValueAnimatorMuscle(i, -GetAnimationValueAnimatorMuscle(i));
                        }
                    }
                    else
                    {
                        var mmi = GetMirrorMuscleIndex(i);
                        if (!IsHaveAnimationCurveAnimatorMuscle(i) && !IsHaveAnimationCurveAnimatorMuscle(mmi)) continue;
                        SetAnimationValueAnimatorMuscle(i, values[i]);
                    }
                }
            }
            {
                Vector3[] saves = new Vector3[(int)AnimatorTDOFIndex.Total];
                for (var tdof = (AnimatorTDOFIndex)0; tdof < AnimatorTDOFIndex.Total; tdof++)
                {
                    saves[(int)tdof] = GetAnimationValueAnimatorTDOF(tdof);
                }
                for (var tdof = (AnimatorTDOFIndex)0; tdof < AnimatorTDOFIndex.Total; tdof++)
                {
                    if (!IsHaveAnimationCurveAnimatorTDOF(tdof)) continue;
                    var mmi = AnimatorTDOFMirrorIndexes[(int)tdof];
                    var vec = Vector3.zero;
                    if (mmi != AnimatorTDOFIndex.None)
                    {
                        vec = Vector3.Scale(saves[(int)mmi], HumanBonesAnimatorTDOFIndex[(int)AnimatorTDOFIndex2HumanBodyBones[(int)mmi]].mirror);
                    }
                    else
                    {
                        vec = saves[(int)tdof];
                        vec.z = -vec.z;
                    }
                    SetAnimationValueAnimatorTDOF(tdof, vec);
                }
            }
            #endregion
            var bindings = AnimationUtility.GetCurveBindings(currentClip);
            #region Generic
            {
                var values = new Dictionary<int, TransformPoseSave.SaveData>();
                foreach (var binding in bindings)
                {
                    var boneIndex = GetBoneIndexFromCurveBinding(binding);
                    if (boneIndex < 0) continue;
                    if (values.ContainsKey(boneIndex)) continue;
                    values.Add(boneIndex, new TransformPoseSave.SaveData());
                    var mbi = mirrorBoneIndexes[boneIndex];
                    if (mbi >= 0)
                    {
                        var mt = editBones[mbi].transform;
                        values[boneIndex].localPosition = GetMirrorBoneLocalPosition(mbi, mt.localPosition);
                        values[boneIndex].localRotation = GetMirrorBoneLocalRotation(mbi, mt.localRotation);
                        values[boneIndex].localScale = GetMirrorBoneLocalScale(mbi, mt.localScale);
                        if (!values.ContainsKey(mbi))
                        {
                            values.Add(mbi, new TransformPoseSave.SaveData());
                            var t = editBones[boneIndex].transform;
                            values[mbi].localPosition = GetMirrorBoneLocalPosition(boneIndex, t.localPosition);
                            values[mbi].localRotation = GetMirrorBoneLocalRotation(boneIndex, t.localRotation);
                            values[mbi].localScale = GetMirrorBoneLocalScale(boneIndex, t.localScale);
                        }
                    }
                    else
                    {
                        var t = editBones[boneIndex].transform;
                        values[boneIndex].localPosition = GetMirrorBoneLocalPosition(boneIndex, t.localPosition);
                        values[boneIndex].localRotation = GetMirrorBoneLocalRotation(boneIndex, t.localRotation);
                        values[boneIndex].localScale = GetMirrorBoneLocalScale(boneIndex, t.localScale);
                    }
                }
                foreach (var pair in values)
                {
                    var bi = pair.Key;
                    if (bi == rootMotionBoneIndex) continue;
                    var mbi = mirrorBoneIndexes[bi];
                    if (mbi >= 0)
                    {
                        if (IsHaveAnimationCurveTransformPosition(bi) || IsHaveAnimationCurveTransformPosition(mbi))
                            SetAnimationValueTransformPosition(bi, pair.Value.localPosition);
                        if (IsHaveAnimationCurveTransformRotation(bi) != URotationCurveInterpolation.Mode.Undefined || IsHaveAnimationCurveTransformRotation(mbi) != URotationCurveInterpolation.Mode.Undefined)
                            SetAnimationValueTransformRotation(bi, pair.Value.localRotation);
                        if (IsHaveAnimationCurveTransformScale(bi) || IsHaveAnimationCurveTransformScale(mbi))
                            SetAnimationValueTransformScale(bi, pair.Value.localScale);
                    }
                    else
                    {
                        if (IsHaveAnimationCurveTransformPosition(bi))
                            SetAnimationValueTransformPosition(bi, pair.Value.localPosition);
                        if (IsHaveAnimationCurveTransformRotation(bi) != URotationCurveInterpolation.Mode.Undefined)
                            SetAnimationValueTransformRotation(bi, pair.Value.localRotation);
                        if (IsHaveAnimationCurveTransformScale(bi))
                            SetAnimationValueTransformScale(bi, pair.Value.localScale);
                    }
                }
            }
            #endregion
            #region BlendShape
            {
                var values = new Dictionary<SkinnedMeshRenderer, Dictionary<string, float>>();
                foreach (var binding in bindings)
                {
                    if (!IsSkinnedMeshRendererBlendShapeCurveBinding(binding)) continue;
                    var boneIndex = GetBoneIndexFromPath(binding.path);
                    if (boneIndex < 0) continue;
                    var renderer = editBones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                    if (renderer == null) continue;
                    var name = binding.propertyName.Remove(0, "blendShape.".Length);
                    Dictionary<string, string> nameTable;
                    if (mirrorBlendShape.TryGetValue(renderer, out nameTable))
                    {
                        string mirrorName;
                        if (nameTable.TryGetValue(name, out mirrorName))
                        {
                            if (!values.ContainsKey(renderer))
                                values.Add(renderer, new Dictionary<string, float>());
                            values[renderer].Add(mirrorName, GetAnimationValueBlendShape(renderer, name));
                            #region NotHaveMirrorCurve
                            {
                                var mbinding = AnimationCurveBindingBlendShape(renderer, mirrorName);
                                if (!EditorCommon.ArrayContains(bindings, mbinding))
                                {
                                    values[renderer].Add(name, blendShapeWeightSave.GetOriginalWeight(renderer, mirrorName));
                                }
                            }
                            #endregion
                        }
                    }
                }
                foreach (var list in values)
                {
                    foreach (var pair in list.Value)
                    {
                        SetAnimationValueBlendShape(list.Key, pair.Key, pair.Value);
                    }
                }
            }
            #endregion
            SetPoseAfter();
        }
        public void SetPoseAfter()
        {
            SetUpdateResampleAnimation();
            SetSynchroIKtargetAll(true);
            updatePoseFixAnimation = true;
        }

        public void SelectionHumanoidMirror()
        {
            {
                var ikHumanoidIndexs = animatorIK.SelectionAnimatorIKTargetsHumanoidIndexes();
                var muscles = SelectionGameObjectsMuscleIndex();
                foreach (var humanoidIndex in ikHumanoidIndexs)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, j);
                        if (muscleIndex < 0) continue;
                        muscles.Add(muscleIndex);
                    }
                }
                int[] mirrorMuscles = new int[muscles.Count];
                var values = new float[muscles.Count];
                for (int i = 0; i < muscles.Count; i++)
                {
                    mirrorMuscles[i] = GetMirrorMuscleIndex(muscles[i]);
                    if (mirrorMuscles[i] >= 0)
                        values[i] = GetAnimationValueAnimatorMuscle(mirrorMuscles[i]);
                }
                for (int i = 0; i < muscles.Count; i++)
                {
                    if (mirrorMuscles[i] < 0)
                    {
                        var hi = HumanTrait.BoneFromMuscle(muscles[i]);
                        if (muscles[i] == HumanTrait.MuscleFromBone(hi, 0) || muscles[i] == HumanTrait.MuscleFromBone(hi, 1))
                        {
                            var value = -GetAnimationValueAnimatorMuscle(muscles[i]);
                            if (!IsHaveAnimationCurveAnimatorMuscle(muscles[i]) && value == 0f) continue;
                            SetAnimationValueAnimatorMuscle(muscles[i], value);
                        }
                    }
                    else
                    {
                        if (!IsHaveAnimationCurveAnimatorMuscle(muscles[i]) && values[i] == 0f) continue;
                        SetAnimationValueAnimatorMuscle(muscles[i], values[i]);
                    }
                }
                if (humanoidHasTDoF)
                {
                    var his = SelectionGameObjectsHumanoidIndex();
                    his.AddRange(ikHumanoidIndexs);
                    Vector3[] saves = new Vector3[(int)AnimatorTDOFIndex.Total];
                    foreach (var hi in his)
                    {
                        if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null) continue;
                        var tdof = VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index;
                        var mmi = AnimatorTDOFMirrorIndexes[(int)tdof];
                        if (mmi != AnimatorTDOFIndex.None)
                            saves[(int)mmi] = GetAnimationValueAnimatorTDOF(mmi);
                        else
                            saves[(int)tdof] = GetAnimationValueAnimatorTDOF(tdof);
                    }
                    foreach (var hi in his)
                    {
                        if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null) continue;
                        var tdof = VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi].index;
                        if (!IsHaveAnimationCurveAnimatorTDOF(tdof)) continue;
                        var mmi = AnimatorTDOFMirrorIndexes[(int)tdof];
                        var vec = Vector3.zero;
                        if (mmi != AnimatorTDOFIndex.None)
                        {
                            vec = Vector3.Scale(saves[(int)mmi], HumanBonesAnimatorTDOFIndex[(int)AnimatorTDOFIndex2HumanBodyBones[(int)mmi]].mirror);
                        }
                        else
                        {
                            vec = saves[(int)tdof];
                            vec.z = -vec.z;
                        }
                        SetAnimationValueAnimatorTDOF(tdof, vec);
                    }
                }
            }
            if (selectionBones.Contains(rootMotionBoneIndex))
            {
                var rootT = GetAnimationValueAnimatorRootT();
                SetAnimationValueAnimatorRootT(new Vector3(-rootT.x, rootT.y, rootT.z));
                var rootQ = GetAnimationValueAnimatorRootQ();
                SetAnimationValueAnimatorRootQ(new Quaternion(rootQ.x, -rootQ.y, -rootQ.z, rootQ.w));
            }
            #region BlendShape
            {
                var values = new Dictionary<SkinnedMeshRenderer, Dictionary<string, float>>();
                foreach (var boneIndex in selectionBones)
                {
                    var renderer = editBones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                    if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount == 0) continue;
                    blendShapeWeightSave.ActionOriginalWeights(renderer, (name, value) =>
                    {
                        Dictionary<string, string> nameTable;
                        if (mirrorBlendShape.TryGetValue(renderer, out nameTable))
                        {
                            string mirrorName;
                            if (nameTable.TryGetValue(name, out mirrorName))
                            {
                                if (!values.ContainsKey(renderer))
                                    values.Add(renderer, new Dictionary<string, float>());
                                values[renderer].Add(mirrorName, GetAnimationValueBlendShape(renderer, name));
                            }
                        }
                    });
                }
                foreach (var list in values)
                {
                    foreach (var pair in list.Value)
                    {
                        if (!IsHaveAnimationCurveBlendShape(list.Key, pair.Key) &&
                            GetAnimationValueBlendShape(list.Key, pair.Key) == pair.Value) continue;
                        SetAnimationValueBlendShape(list.Key, pair.Key, pair.Value);
                    }
                }
            }
            #endregion
        }
        public void SelectionHumanoidResetAll()
        {
            {
                var ikHumanoidIndexs = animatorIK.SelectionAnimatorIKTargetsHumanoidIndexes();
                {
                    var muscles = SelectionGameObjectsMuscleIndex();
                    foreach (var humanoidIndex in ikHumanoidIndexs)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var muscleIndex = HumanTrait.MuscleFromBone((int)humanoidIndex, j);
                            if (muscleIndex < 0) continue;
                            muscles.Add(muscleIndex);
                        }
                    }
                    foreach (var muscle in muscles)
                    {
                        if (IsHaveAnimationCurveAnimatorMuscle(muscle))
                        {
                            SetAnimationValueAnimatorMuscle(muscle, 0f);
                        }
                    }
                }
                {
                    var his = SelectionGameObjectsHumanoidIndex();
                    his.AddRange(ikHumanoidIndexs);
                    foreach (var hi in his)
                    {
                        if (VeryAnimation.HumanBonesAnimatorTDOFIndex[(int)hi] == null) continue;
                        if (IsHaveAnimationCurveAnimatorTDOF(HumanBonesAnimatorTDOFIndex[(int)hi].index))
                        {
                            SetAnimationValueAnimatorTDOF(HumanBonesAnimatorTDOFIndex[(int)hi].index, Vector3.zero);
                        }
                    }
                }
            }
            if (SelectionGameObjectsIndexOf(vaw.gameObject) >= 0)
            {
                if (IsHaveAnimationCurveAnimatorRootT())
                    SetAnimationValueAnimatorRootT(new Vector3(0, 1, 0));
                if (IsHaveAnimationCurveAnimatorRootQ())
                    SetAnimationValueAnimatorRootQ(Quaternion.identity);
            }
            #region BlendShape
            {
                foreach (var boneIndex in selectionBones)
                {
                    var renderer = editBones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                    if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount == 0) continue;
                    blendShapeWeightSave.ActionOriginalWeights(renderer, (name, value) =>
                    {
                        if (IsHaveAnimationCurveBlendShape(renderer, name))
                            SetAnimationValueBlendShape(renderer, name, blendShapeWeightSave.GetOriginalWeight(renderer, name));
                    });
                }
            }
            #endregion
        }
        public void SelectionGenericMirror()
        {
            {
                var selectOriginalIKTargetsBoneIndexes = originalIK.SelectionOriginalIKTargetsBoneIndexes();
                var bones = new List<int>(selectionBones);
                bones.AddRange(selectOriginalIKTargetsBoneIndexes);
                var values = new TransformPoseSave.SaveData[bones.Count];
                for (int i = 0; i < bones.Count; i++)
                {
                    var mbi = mirrorBoneIndexes[bones[i]];
                    if (mbi >= 0)
                    {
                        var mt = editBones[mbi].transform;
                        values[i] = new TransformPoseSave.SaveData()
                        {
                            localPosition = GetMirrorBoneLocalPosition(mbi, mt.localPosition),
                            localRotation = GetMirrorBoneLocalRotation(mbi, mt.localRotation),
                            localScale = GetMirrorBoneLocalScale(mbi, mt.localScale),
                        };
                    }
                    else
                    {
                        var bi = bones[i];
                        var t = editBones[bi].transform;
                        values[i] = new TransformPoseSave.SaveData()
                        {
                            localPosition = GetMirrorBoneLocalPosition(bi, t.localPosition),
                            localRotation = GetMirrorBoneLocalRotation(bi, t.localRotation),
                            localScale = GetMirrorBoneLocalScale(bi, t.localScale),
                        };
                    }
                }
                for (int i = 0; i < bones.Count; i++)
                {
                    var bi = bones[i];
                    if (rootMotionBoneIndex >= 0 && bi == rootMotionBoneIndex) continue;
                    if (IsHaveAnimationCurveTransformPosition(bi) || values[i].localPosition != boneSaveTransforms[bi].localPosition)
                    {
                        SetAnimationValueTransformPosition(bi, values[i].localPosition);
                    }
                    if (IsHaveAnimationCurveTransformRotation(bi) != URotationCurveInterpolation.Mode.Undefined || values[i].localRotation != boneSaveTransforms[bi].localRotation)
                    {
                        SetAnimationValueTransformRotation(bi, values[i].localRotation);
                    }
                    if (IsHaveAnimationCurveTransformScale(bi) || values[i].localScale != boneSaveTransforms[bi].localScale)
                    {
                        SetAnimationValueTransformScale(bi, values[i].localScale);
                    }
                }
            }
            if (rootMotionBoneIndex >= 0)
            {
                if (selectionBones.Contains(rootMotionBoneIndex))
                {
                    var rootT = GetAnimationValueAnimatorRootT();
                    SetAnimationValueAnimatorRootT(new Vector3(-rootT.x, rootT.y, rootT.z));
                    var rootQ = GetAnimationValueAnimatorRootQ();
                    SetAnimationValueAnimatorRootQ(new Quaternion(rootQ.x, -rootQ.y, -rootQ.z, rootQ.w));
                }
            }
            #region BlendShape
            {
                var values = new Dictionary<SkinnedMeshRenderer, Dictionary<string, float>>();
                foreach (var boneIndex in selectionBones)
                {
                    var renderer = editBones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                    if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount == 0) continue;
                    blendShapeWeightSave.ActionOriginalWeights(renderer, (name, value) =>
                    {
                        Dictionary<string, string> nameTable;
                        if (mirrorBlendShape.TryGetValue(renderer, out nameTable))
                        {
                            string mirrorName;
                            if (nameTable.TryGetValue(name, out mirrorName))
                            {
                                if (!values.ContainsKey(renderer))
                                    values.Add(renderer, new Dictionary<string, float>());
                                values[renderer].Add(mirrorName, GetAnimationValueBlendShape(renderer, name));
                            }
                        }
                    });
                }
                foreach (var list in values)
                {
                    foreach (var pair in list.Value)
                    {
                        if (!IsHaveAnimationCurveBlendShape(list.Key, pair.Key) &&
                            GetAnimationValueBlendShape(list.Key, pair.Key) == pair.Value) continue;
                        SetAnimationValueBlendShape(list.Key, pair.Key, pair.Value);
                    }
                }
            }
            #endregion
        }
        public void SelectionGenericResetAll()
        {
            {
                var selectOriginalIKTargetsBoneIndexes = originalIK.SelectionOriginalIKTargetsBoneIndexes();
                var bones = new List<int>(selectionBones);
                bones.AddRange(selectOriginalIKTargetsBoneIndexes);
                foreach (var bi in bones)
                {
                    if (rootMotionBoneIndex >= 0 && rootMotionBoneIndex == bi) continue;
                    if (IsHaveAnimationCurveTransformPosition(bi))
                    {
                        SetAnimationValueTransformPosition(bi, boneSaveTransforms[bi].localPosition);
                    }
                    if (IsHaveAnimationCurveTransformRotation(bi) != URotationCurveInterpolation.Mode.Undefined)
                    {
                        SetAnimationValueTransformRotation(bi, boneSaveTransforms[bi].localRotation);
                    }
                    if (IsHaveAnimationCurveTransformScale(bi))
                    {
                        SetAnimationValueTransformScale(bi, boneSaveTransforms[bi].localScale);
                    }
                }
            }
            if (rootMotionBoneIndex >= 0)
            {
                if (selectionBones.Contains(rootMotionBoneIndex))
                {
                    if (IsHaveAnimationCurveAnimatorRootT())
                    {
                        SetAnimationValueAnimatorRootT(boneSaveTransforms[rootMotionBoneIndex].localPosition);
                    }
                    if (IsHaveAnimationCurveAnimatorRootQ())
                    {
                        SetAnimationValueAnimatorRootQ(boneSaveTransforms[rootMotionBoneIndex].localRotation);
                    }
                }
            }
            #region BlendShape
            {
                foreach (var boneIndex in selectionBones)
                {
                    var renderer = editBones[boneIndex].GetComponent<SkinnedMeshRenderer>();
                    if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount == 0) continue;
                    blendShapeWeightSave.ActionOriginalWeights(renderer, (name, value) =>
                    {
                        if (IsHaveAnimationCurveBlendShape(renderer, name))
                            SetAnimationValueBlendShape(renderer, name, blendShapeWeightSave.GetOriginalWeight(renderer, name));
                    });
                }
            }
            #endregion
        }

        private void ResetAllHaveAnimationCurve()
        {
            transformPoseSave.ResetOriginalTransform();
            blendShapeWeightSave.ResetOriginalWeight();

            #region Humanoid
            if (isHuman)
            {
                SetAnimationValueAnimatorRootT(new Vector3(0, 1, 0));   //Always create
                SetAnimationValueAnimatorRootQ(Quaternion.identity);    //Always create
                for (int mi = 0; mi < HumanTrait.MuscleCount; mi++)
                {
                    if (!IsHaveAnimationCurveAnimatorMuscle(mi)) continue;
                    SetAnimationValueAnimatorMuscle(mi, 0f);
                }
                for (var tdof = (AnimatorTDOFIndex)0; tdof < AnimatorTDOFIndex.Total; tdof++)
                {
                    if (!IsHaveAnimationCurveAnimatorTDOF(tdof)) continue;
                    SetAnimationValueAnimatorTDOF(tdof, Vector3.zero);
                }
            }
            #endregion

            #region Generic
            for (int i = 0; i < editBones.Length; i++)
            {
                if (isHuman && humanoidConflict[i]) continue;
                if (!isHuman && rootMotionBoneIndex >= 0 && (i == rootMotionBoneIndex || i == 0)) continue;

                if (IsHaveAnimationCurveTransformPosition(i))
                    SetAnimationValueTransformPosition(i, boneSaveTransforms[i].localPosition);
                if (IsHaveAnimationCurveTransformRotation(i) != URotationCurveInterpolation.Mode.Undefined)
                    SetAnimationValueTransformRotation(i, boneSaveTransforms[i].localRotation);
                if (IsHaveAnimationCurveTransformScale(i))
                    SetAnimationValueTransformScale(i, boneSaveTransforms[i].localScale);
            }
            #endregion

            #region BlendShape
            foreach (var renderer in editGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount <= 0) continue;
                for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                {
                    var name = renderer.sharedMesh.GetBlendShapeName(i);
                    if (IsHaveAnimationCurveBlendShape(renderer, name))
                        SetAnimationValueBlendShape(renderer, name, blendShapeWeightSave.GetOriginalWeight(renderer, name));
                }
            }
            #endregion
        }
        private void SetAllChangedAnimationCurve()
        {
            #region Humanoid
            if (isHuman)
            {
                HumanPose hp = new HumanPose();
                GetHumanPose(ref hp);
                SetAnimationValueAnimatorRootT(hp.bodyPosition);    //Always create
                if (IsHaveAnimationCurveAnimatorRootQ())
                    SetAnimationValueAnimatorRootQ(hp.bodyRotation);
                for (int i = 0; i < hp.muscles.Length; i++)
                {
                    if (!IsHaveAnimationCurveAnimatorMuscle(i) && hp.muscles[i] == 0f) continue;
                    SetAnimationValueAnimatorMuscle(i, hp.muscles[i]);
                }
            }
            #endregion

            #region Generic
            if (!isHuman && rootMotionBoneIndex >= 0)
            {
                var t = editBones[rootMotionBoneIndex].transform;
                SetAnimationValueAnimatorRootT(t.localPosition);    //Always create
                if (IsHaveAnimationCurveAnimatorRootQ())
                    SetAnimationValueAnimatorRootQ(t.localRotation);
            }
            for (int i = 0; i < editBones.Length; i++)
            {
                if (isHuman && humanoidConflict[i]) continue;
                if (!isHuman && rootMotionBoneIndex >= 0 && (i == rootMotionBoneIndex || i == 0)) continue;

                var t = editBones[i].transform;
                if (IsHaveAnimationCurveTransformPosition(i) || t.localPosition != boneSaveTransforms[i].localPosition)
                    SetAnimationValueTransformPosition(i, t.localPosition);
                if (IsHaveAnimationCurveTransformRotation(i) != URotationCurveInterpolation.Mode.Undefined || t.localRotation != boneSaveTransforms[i].localRotation)
                    SetAnimationValueTransformRotation(i, t.localRotation);
                //There is an error only on the scale, so roughly check it.
                const float Threshold = 0.1f;
                if (IsHaveAnimationCurveTransformScale(i) ||
                    Mathf.Abs(t.localScale.x - boneSaveTransforms[i].localScale.x) >= Threshold ||
                    Mathf.Abs(t.localScale.y - boneSaveTransforms[i].localScale.y) >= Threshold ||
                    Mathf.Abs(t.localScale.z - boneSaveTransforms[i].localScale.z) >= Threshold)
                {
                    SetAnimationValueTransformScale(i, t.localScale);
                }
            }
            #endregion

            #region BlendShape
            foreach (var renderer in editGameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer == null || renderer.sharedMesh == null || renderer.sharedMesh.blendShapeCount <= 0) continue;
                for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                {
                    var name = renderer.sharedMesh.GetBlendShapeName(i);
                    var weight = renderer.GetBlendShapeWeight(i);
                    if (IsHaveAnimationCurveBlendShape(renderer, name) || weight != blendShapeWeightSave.GetOriginalWeight(renderer, name))
                        SetAnimationValueBlendShape(renderer, name, weight);
                }
            }
            #endregion
        }

        private float GetFrameSnapTime(float time = -1f)
        {
            if (time < 0f)
                return uAw.SnapToFrame(currentTime, currentClip.frameRate);
            else
                return uAw.SnapToFrame(time, currentClip.frameRate);
        }
        private bool IsHaveThisTimeRootAnimationCurveKeyframe(float time = -1f)
        {
            if (currentClip == null)
                return false;
            time = GetFrameSnapTime(time);
            for (int dofIndex = 0; dofIndex < 3; dofIndex++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootT[dofIndex]);
                if (curve == null) continue;
                if (FindKeyframeAtTime(curve, time) >= 0)
                    return true;
            }
            for (int dofIndex = 0; dofIndex < 4; dofIndex++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootQ[dofIndex]);
                if (curve == null) continue;
                if (FindKeyframeAtTime(curve, time) >= 0)
                    return true;
            }
            return false;
        }

        public bool IsHaveAnimationCurveAnimatorRootT()
        {
            if (currentClip == null)
                return false;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootT[i]);
                if (curve != null)
                    return true;
            }
            return false;
        }
        public Vector3 GetAnimationValueAnimatorRootT(float time = -1f)
        {
            if (currentClip == null)
                return Vector3.zero;
            time = GetFrameSnapTime(time);
            Vector3 result = isHuman || rootMotionBoneIndex < 0 ? Vector3.zero : boneSaveTransforms[rootMotionBoneIndex].localPosition;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootT[i]);
                if (curve != null)
                {
                    result[i] = curve.Evaluate(time);
                }
            }
            return result;
        }
        public void SetAnimationValueAnimatorRootT(Vector3 value3, float time = -1f)
        {
            if (!BeginChangeAnimationCurve(currentClip, "Change RootT"))
                return;
            time = GetFrameSnapTime(time);
            for (int i = 0; i < 3; i++)
            {
                var curve = GetAnimationCurveAnimatorRootT(i);
                SetKeyframe(curve, time, value3[i]);
                SetAnimationCurveAnimatorRootT(i, curve);
            }
            if (uAw.TimeToFrameRound(time) == uAw.GetCurrentFrame())
            {
                SetUpdateIKtargetAll(true);
            }
        }
        public AnimationCurve GetAnimationCurveAnimatorRootT(int dof)
        {
            var binding = AnimationCurveBindingAnimatorRootT[dof];
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = isHuman ? new Vector3(0f, 1f, 0f) : (rootMotionBoneIndex >= 0 ? boneSaveTransforms[rootMotionBoneIndex].localPosition : Vector3.zero);
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue[dof]);
                AddKeyframe(curve, currentClip.length, defaultValue[dof]);
                //Created
                AddAnimationWindowSynchroSelection(binding);
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveAnimatorRootT(int dof, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change RootT"))
            {
                SetEditorCurveCache(AnimationCurveBindingAnimatorRootT[dof], curve);
            }
        }

        public bool IsHaveAnimationCurveAnimatorRootQ()
        {
            if (currentClip == null)
                return false;
            for (int i = 0; i < 4; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootQ[i]);
                if (curve != null)
                    return true;
            }
            return false;
        }
        public Quaternion GetAnimationValueAnimatorRootQ(float time = -1f)
        {
            if (currentClip == null)
                return Quaternion.identity;
            time = GetFrameSnapTime(time);
            Vector4 result = isHuman || rootMotionBoneIndex < 0 ? new Vector4(0, 0, 0, 1) : new Vector4(boneSaveTransforms[rootMotionBoneIndex].localRotation.x, boneSaveTransforms[rootMotionBoneIndex].localRotation.y, boneSaveTransforms[rootMotionBoneIndex].localRotation.z, boneSaveTransforms[rootMotionBoneIndex].localRotation.w);
            for (int i = 0; i < 4; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorRootQ[i]);
                if (curve != null)
                {
                    result[i] = curve.Evaluate(time);
                }
            }
            if (result.sqrMagnitude > 0f)
            {
                result.Normalize();
                return new Quaternion(result.x, result.y, result.z, result.w);
            }
            else
            {
                return Quaternion.identity;
            }
        }
        public void SetAnimationValueAnimatorRootQ(Quaternion rotation, float time = -1f)
        {
            if (!BeginChangeAnimationCurve(currentClip, "Change RootQ"))
                return;
            time = GetFrameSnapTime(time);
            tmpCurves.Clear();
            for (int i = 0; i < 4; i++)
            {
                tmpCurves.curves[i] = GetAnimationCurveAnimatorRootQ(i);
            }
            rotation = FixReverseRotationQuaternion(tmpCurves.curves, time, rotation);
            for (int i = 0; i < 4; i++)
            {
                SetKeyframe(tmpCurves.curves[i], time, rotation[i]);
                SetAnimationCurveAnimatorRootQ(i, tmpCurves.curves[i]);
            }
            tmpCurves.Clear();
            if (uAw.TimeToFrameRound(time) == uAw.GetCurrentFrame())
            {
                SetUpdateIKtargetAll(true);
            }
        }
        public AnimationCurve GetAnimationCurveAnimatorRootQ(int dof)
        {
            var binding = AnimationCurveBindingAnimatorRootQ[dof];
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = isHuman ? Quaternion.identity : (rootMotionBoneIndex >= 0 ? boneSaveTransforms[rootMotionBoneIndex].localRotation : Quaternion.identity);
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue[dof]);
                AddKeyframe(curve, currentClip.length, defaultValue[dof]);
                //Created
                AddAnimationWindowSynchroSelection(binding);
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveAnimatorRootQ(int dof, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change RootQ"))
            {
                SetEditorCurveCache(AnimationCurveBindingAnimatorRootQ[dof], curve);
            }
        }

        public bool IsHaveAnimationCurveAnimatorIkT(AnimatorIKIndex ikIndex)
        {
            if (currentClip == null)
                return false;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorIkT(ikIndex, i));
                if (curve != null)
                    return true;
            }
            return false;
        }
        public Vector3 GetAnimationValueAnimatorIkT(AnimatorIKIndex ikIndex, float time = -1f)
        {
            if (currentClip == null)
                return Vector3.zero;
            time = GetFrameSnapTime(time);
            Vector3 result = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorIkT(ikIndex, i));
                if (curve != null)
                {
                    result[i] = curve.Evaluate(time);
                }
            }
            return result;
        }
        public void SetAnimationValueAnimatorIkT(AnimatorIKIndex ikIndex, Vector3 value3, float time = -1f)
        {
            if (!BeginChangeAnimationCurve(currentClip, "Change IK T"))
                return;
            time = GetFrameSnapTime(time);
            for (int i = 0; i < 3; i++)
            {
                var curve = GetAnimationCurveAnimatorIkT(ikIndex, i);
                SetKeyframe(curve, time, value3[i]);
                SetAnimationCurveAnimatorIkT(ikIndex, i, curve);
            }
        }
        public AnimationCurve GetAnimationCurveAnimatorIkT(AnimatorIKIndex ikIndex, int dof)
        {
            var binding = AnimationCurveBindingAnimatorIkT(ikIndex, dof);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = Vector3.zero;
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue[dof]);
                AddKeyframe(curve, currentClip.length, defaultValue[dof]);
            }
            return curve;
        }
        public void SetAnimationCurveAnimatorIkT(AnimatorIKIndex ikIndex, int dof, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change IK T"))
            {
                SetEditorCurveCache(AnimationCurveBindingAnimatorIkT(ikIndex, dof), curve);
            }
        }

        public bool IsHaveAnimationCurveAnimatorIkQ(AnimatorIKIndex ikIndex)
        {
            if (currentClip == null)
                return false;
            for (int i = 0; i < 4; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorIkQ(ikIndex, i));
                if (curve != null)
                    return true;
            }
            return false;
        }
        public Quaternion GetAnimationValueAnimatorIkQ(AnimatorIKIndex ikIndex, float time = -1f)
        {
            if (currentClip == null)
                return Quaternion.identity;
            time = GetFrameSnapTime(time);
            Vector4 result = new Vector4(0, 0, 0, 1);
            for (int i = 0; i < 4; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorIkQ(ikIndex, i));
                if (curve != null)
                {
                    result[i] = curve.Evaluate(time);
                }
            }
            if (result.sqrMagnitude > 0f)
            {
                result.Normalize();
                return new Quaternion(result.x, result.y, result.z, result.w);
            }
            else
            {
                return Quaternion.identity;
            }
        }
        public void SetAnimationValueAnimatorIkQ(AnimatorIKIndex ikIndex, Quaternion rotation, float time = -1f)
        {
            if (!BeginChangeAnimationCurve(currentClip, "Change IK Q"))
                return;
            time = GetFrameSnapTime(time);
            tmpCurves.Clear();
            for (int i = 0; i < 4; i++)
            {
                tmpCurves.curves[i] = GetAnimationCurveAnimatorIkQ(ikIndex, i);
            }
            rotation = FixReverseRotationQuaternion(tmpCurves.curves, time, rotation);
            for (int i = 0; i < 4; i++)
            {
                SetKeyframe(tmpCurves.curves[i], time, rotation[i]);
                SetAnimationCurveAnimatorIkQ(ikIndex, i, tmpCurves.curves[i]);
            }
            tmpCurves.Clear();
        }
        public AnimationCurve GetAnimationCurveAnimatorIkQ(AnimatorIKIndex ikIndex, int dof)
        {
            var binding = AnimationCurveBindingAnimatorIkQ(ikIndex, dof);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = Quaternion.identity;
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue[dof]);
                AddKeyframe(curve, currentClip.length, defaultValue[dof]);
            }
            return curve;
        }
        public void SetAnimationCurveAnimatorIkQ(AnimatorIKIndex ikIndex, int dof, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change IK Q"))
            {
                SetEditorCurveCache(AnimationCurveBindingAnimatorIkQ(ikIndex, dof), curve);
            }
        }

        public bool IsHaveAnimationCurveAnimatorTDOF(AnimatorTDOFIndex tdofIndex)
        {
            if (currentClip == null)
                return false;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorTDOF(tdofIndex, i));
                if (curve != null)
                    return true;
            }
            return false;
        }
        public Vector3 GetAnimationValueAnimatorTDOF(AnimatorTDOFIndex tdofIndex, float time = -1f)
        {
            if (currentClip == null)
                return Vector3.zero;
            time = GetFrameSnapTime(time);
            Vector3 result = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorTDOF(tdofIndex, i));
                if (curve != null)
                {
                    result[i] = curve.Evaluate(time);
                }
            }
            return result;
        }
        public void SetAnimationValueAnimatorTDOF(AnimatorTDOFIndex tdofIndex, Vector3 value3, float time = -1f)
        {
            if (!BeginChangeAnimationCurve(currentClip, "Change TDOF"))
                return;
            time = GetFrameSnapTime(time);
            for (int dof = 0; dof < 3; dof++)
            {
                var curve = GetAnimationCurveAnimatorTDOF(tdofIndex, dof);
                SetKeyframe(curve, time, value3[dof]);
                SetAnimationCurveAnimatorTDOF(tdofIndex, dof, curve);
            }
            if (uAw.TimeToFrameRound(time) == uAw.GetCurrentFrame())
            {
                SetUpdateIKtargetTdofIndex(tdofIndex);
            }
        }
        public AnimationCurve GetAnimationCurveAnimatorTDOF(AnimatorTDOFIndex tdofIndex, int dof)
        {
            var binding = AnimationCurveBindingAnimatorTDOF(tdofIndex, dof);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = Vector3.zero;
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue[dof]);
                AddKeyframe(curve, currentClip.length, defaultValue[dof]);
                //Created
                AddAnimationWindowSynchroSelection(binding);
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveAnimatorTDOF(AnimatorTDOFIndex tdofIndex, int dof, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change TDOF"))
            {
                SetEditorCurveCache(AnimationCurveBindingAnimatorTDOF(tdofIndex, dof), curve);
            }
        }

        public bool IsHaveAnimationCurveAnimatorMuscle(int muscleIndex)
        {
            if (muscleIndex < 0 || muscleIndex >= HumanTrait.MuscleCount)
                return false;
            if (currentClip == null)
                return false;
            return GetEditorCurveCache(AnimationCurveBindingAnimatorMuscle(muscleIndex)) != null;
        }
        public float GetAnimationValueAnimatorMuscle(int muscleIndex, float time = -1f)
        {
            if (muscleIndex < 0 || muscleIndex >= HumanTrait.MuscleCount)
                return 0f;
            if (currentClip == null)
                return 0f;
            time = GetFrameSnapTime(time);
            var curve = GetEditorCurveCache(AnimationCurveBindingAnimatorMuscle(muscleIndex));
            if (curve == null) return 0f;
            return curve.Evaluate(time);
        }
        public void SetAnimationValueAnimatorMuscle(int muscleIndex, float value, float time = -1f)
        {
            if (muscleIndex < 0 || muscleIndex >= HumanTrait.MuscleCount)
                return;
            if (!BeginChangeAnimationCurve(currentClip, "Change Muscle"))
                return;
            time = GetFrameSnapTime(time);
            {
                var curve = GetAnimationCurveAnimatorMuscle(muscleIndex);
                SetKeyframe(curve, time, value);
                SetAnimationCurveAnimatorMuscle(muscleIndex, curve);
            }
            if (uAw.TimeToFrameRound(time) == uAw.GetCurrentFrame())
            {
                SetUpdateIKtargetMuscle(muscleIndex);
            }
        }
        public AnimationCurve GetAnimationCurveAnimatorMuscle(int muscleIndex)
        {
            var binding = AnimationCurveBindingAnimatorMuscle(muscleIndex);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, 0f);
                AddKeyframe(curve, currentClip.length, 0f);
                //Created
                AddAnimationWindowSynchroSelection(binding);
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveAnimatorMuscle(int muscleIndex, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change Muscle"))
            {
                SetEditorCurveCache(AnimationCurveBindingAnimatorMuscle(muscleIndex), curve);
            }
        }

        public bool IsHaveAnimationCurveTransformPosition(int boneIndex)
        {
            if (currentClip == null || boneIndex < 0 || boneIndex >= editBones.Length)
                return false;
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingTransformPosition(boneIndex, 0));
                if (curve != null)
                    return true;
            }
            return false;
        }
        public Vector3 GetAnimationValueTransformPosition(int boneIndex, float time = -1f)
        {
            if (currentClip == null || boneIndex < 0 || boneIndex >= editBones.Length)
                return boneSaveTransforms[boneIndex].localPosition;
            time = GetFrameSnapTime(time);
            Vector3 result = boneSaveTransforms[boneIndex].localPosition;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingTransformPosition(boneIndex, i));
                if (curve != null)
                {
                    result[i] = curve.Evaluate(time);
                }
            }
            return result;
        }
        public void SetAnimationValueTransformPosition(int boneIndex, Vector3 position, float time = -1f)
        {
            if (boneIndex < 0 || boneIndex >= editBones.Length)
                return;
            if (!BeginChangeAnimationCurve(currentClip, "Change Transform Position"))
                return;
            time = GetFrameSnapTime(time);
            bool removeCurve = false;
            if (isHuman && humanoidConflict[boneIndex])
            {
                EditorCommon.ShowNotification("Conflict");
                Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveHumanoidConflictError), editBones[boneIndex].name);
                removeCurve = true;
            }
            else if (rootMotionBoneIndex >= 0 && boneIndex == 0)
            {
                EditorCommon.ShowNotification("Conflict");
                Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveRootConflictError), editBones[boneIndex].name);
                removeCurve = true;
            }
            else if (!isHuman && rootMotionBoneIndex >= 0 && boneIndex == rootMotionBoneIndex)
            {
                EditorCommon.ShowNotification("Conflict");
                Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveGenericRootConflictError), editBones[boneIndex].name);
                updateGenericRootMotion = true;
                return;
            }
            if (removeCurve)
            {
                for (int i = 0; i < 3; i++)
                {
                    SetEditorCurveCache(AnimationCurveBindingTransformPosition(boneIndex, i), null);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    var curve = GetAnimationCurveTransformPosition(boneIndex, i);
                    SetKeyframe(curve, time, position[i]);
                    SetAnimationCurveTransformPosition(boneIndex, i, curve);
                }
            }
            if (uAw.TimeToFrameRound(time) == uAw.GetCurrentFrame())
            {
                SetUpdateIKtargetBone(boneIndex);
            }
        }
        public AnimationCurve GetAnimationCurveTransformPosition(int boneIndex, int dof)
        {
            var binding = AnimationCurveBindingTransformPosition(boneIndex, dof);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = boneSaveTransforms[boneIndex].localPosition;
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue[dof]);
                AddKeyframe(curve, currentClip.length, defaultValue[dof]);
                //Created
                AddAnimationWindowSynchroSelection(binding);
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveTransformPosition(int boneIndex, int dof, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change Transform Position"))
            {
                SetEditorCurveCache(AnimationCurveBindingTransformPosition(boneIndex, dof), curve);
            }
        }

        public URotationCurveInterpolation.Mode IsHaveAnimationCurveTransformRotation(int boneIndex)
        {
            if (currentClip == null || boneIndex < 0 || boneIndex >= editBones.Length)
                return URotationCurveInterpolation.Mode.Undefined;

            if (GetEditorCurveCache(AnimationCurveBindingTransformRotation(boneIndex, 0, URotationCurveInterpolation.Mode.RawQuaternions)) != null)
                return URotationCurveInterpolation.Mode.RawQuaternions;

            if (GetEditorCurveCache(AnimationCurveBindingTransformRotation(boneIndex, 0, URotationCurveInterpolation.Mode.RawEuler)) != null)
                return URotationCurveInterpolation.Mode.RawEuler;

            return URotationCurveInterpolation.Mode.Undefined;
        }
        public Quaternion GetAnimationValueTransformRotation(int boneIndex, float time = -1f)
        {
            if (currentClip == null || boneIndex < 0 || boneIndex >= editBones.Length)
                return boneSaveTransforms[boneIndex].localRotation;
            time = GetFrameSnapTime(time);
            var binding = AnimationCurveBindingTransformRotation(boneIndex, 0, URotationCurveInterpolation.Mode.RawQuaternions);
            var curve = GetEditorCurveCache(binding);
            if (curve != null)
            {
                #region RawQuaternions
                Vector4 result = Vector4.zero;
                result[0] = curve.Evaluate(time);
                for (int i = 1; i < 4; i++)
                {
                    binding = AnimationCurveBindingTransformRotation(boneIndex, i, URotationCurveInterpolation.Mode.RawQuaternions);
                    curve = GetEditorCurveCache(binding);
                    if (curve != null)
                        result[i] = curve.Evaluate(time);
                }
                if (result.sqrMagnitude <= 0f)
                    return boneSaveTransforms[boneIndex].localRotation;
                result.Normalize();
                Quaternion resultQ = Quaternion.identity;
                for (int i = 0; i < 4; i++)
                    resultQ[i] = result[i];
                return resultQ;
                #endregion
            }
            else
            {
                binding = AnimationCurveBindingTransformRotation(boneIndex, 0, URotationCurveInterpolation.Mode.RawEuler);
                curve = GetEditorCurveCache(binding);
                if (curve != null)
                {
                    #region RawEuler
                    Vector3 result = Vector3.zero;
                    for (int i = 0; i < 3; i++)
                    {
                        binding = AnimationCurveBindingTransformRotation(boneIndex, i, URotationCurveInterpolation.Mode.RawEuler);
                        curve = GetEditorCurveCache(binding);
                        if (curve != null)
                            result[i] = curve.Evaluate(time);
                    }
                    return Quaternion.Euler(result);
                }
                #endregion
            }
            return boneSaveTransforms[boneIndex].localRotation;
        }
        public void SetAnimationValueTransformRotation(int boneIndex, Quaternion rotation, float time = -1f)
        {
            if (boneIndex < 0 || boneIndex >= editBones.Length)
                return;
            if (!BeginChangeAnimationCurve(currentClip, "Change Transform Rotation"))
                return;
            time = GetFrameSnapTime(time);
            bool removeCurve = false;
            if (isHuman && humanoidConflict[boneIndex])
            {
                EditorCommon.ShowNotification("Conflict");
                Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveHumanoidConflictError), editBones[boneIndex].name);
                removeCurve = true;
            }
            else if (rootMotionBoneIndex >= 0 && boneIndex == 0)
            {
                EditorCommon.ShowNotification("Conflict");
                Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveRootConflictError), editBones[boneIndex].name);
                removeCurve = true;
            }
            else if (!isHuman && rootMotionBoneIndex >= 0 && boneIndex == rootMotionBoneIndex)
            {
                EditorCommon.ShowNotification("Conflict");
                Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveGenericRootConflictError), editBones[boneIndex].name);
                updateGenericRootMotion = true;
                return;
            }
            var mode = IsHaveAnimationCurveTransformRotation(boneIndex);
            if (removeCurve)
            {
                if (mode == URotationCurveInterpolation.Mode.RawQuaternions)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        SetEditorCurveCache(AnimationCurveBindingTransformRotation(boneIndex, i, URotationCurveInterpolation.Mode.RawQuaternions), null);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        SetEditorCurveCache(AnimationCurveBindingTransformRotation(boneIndex, i, URotationCurveInterpolation.Mode.RawEuler), null);
                    }
                }
            }
            else
            {
                if (mode == URotationCurveInterpolation.Mode.Undefined)
                    mode = URotationCurveInterpolation.Mode.RawQuaternions;
                tmpCurves.Clear();
                if (mode == URotationCurveInterpolation.Mode.RawQuaternions)
                {
                    #region RawQuaternions
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            tmpCurves.curves[i] = GetAnimationCurveTransformRotation(boneIndex, i, mode);
                        }
                        rotation = FixReverseRotationQuaternion(tmpCurves.curves, time, rotation);
                        for (int i = 0; i < 4; i++)
                        {
                            float value = rotation[i];
                            SetKeyframe(tmpCurves.curves[i], time, value);
                            #region ErrorAvoidance  
                            {
                                //There must be at least two keyframes. If not, an error will occur.[AnimationUtility.GetEditorCurve]
                                while (tmpCurves.curves[i].length < 2)
                                {
                                    var addTime = 0f;
                                    if (time != 0f) addTime = 0f;
                                    else if (currentClip.length != 0f) addTime = currentClip.length;
                                    else addTime = 1f;
                                    AddKeyframe(tmpCurves.curves[i], addTime, tmpCurves.curves[i].Evaluate(addTime));
                                }
                            }
                            #endregion
                            SetAnimationCurveTransformRotation(boneIndex, i, mode, tmpCurves.curves[i]);
                        }
                    }
                    #endregion
                }
                else
                {
                    #region RawEuler
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            tmpCurves.curves[i] = GetAnimationCurveTransformRotation(boneIndex, i, mode);
                        }
                        var eulerAngles = FixReverseRotationEuler(tmpCurves.curves, time, rotation.eulerAngles);
                        for (int i = 0; i < 3; i++)
                        {
                            var value = eulerAngles[i];
                            SetKeyframe(tmpCurves.curves[i], time, value);
                            SetAnimationCurveTransformRotation(boneIndex, i, mode, tmpCurves.curves[i]);
                        }
                    }
                    #endregion
                }
            }
            tmpCurves.Clear();
            if (uAw.TimeToFrameRound(time) == uAw.GetCurrentFrame())
            {
                SetUpdateIKtargetBone(boneIndex);
            }
        }
        public AnimationCurve GetAnimationCurveTransformRotation(int boneIndex, int dof, URotationCurveInterpolation.Mode mode)
        {
            var binding = AnimationCurveBindingTransformRotation(boneIndex, dof, mode);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                curve = new AnimationCurve();
                if (mode == URotationCurveInterpolation.Mode.RawQuaternions)
                {
                    var defaultValue = boneSaveTransforms[boneIndex].localRotation;
                    AddKeyframe(curve, 0f, defaultValue[dof]);
                    AddKeyframe(curve, currentClip.length, defaultValue[dof]);
                    //Created
                    for (int i = 0; i < 3; i++)
                    {
                        var bindingSub = AnimationCurveBindingTransformRotation(boneIndex, i, URotationCurveInterpolation.Mode.NonBaked);
                        AddAnimationWindowSynchroSelection(bindingSub);
                    }
                }
                else
                {
                    var defaultValue = boneSaveTransforms[boneIndex].localRotation.eulerAngles;
                    AddKeyframe(curve, 0f, defaultValue[dof]);
                    AddKeyframe(curve, currentClip.length, defaultValue[dof]);
                    //Created
                    AddAnimationWindowSynchroSelection(binding);
                }
                //Created
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveTransformRotation(int boneIndex, int dof, URotationCurveInterpolation.Mode mode, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change Transform Rotation"))
            {
                SetEditorCurveCache(AnimationCurveBindingTransformRotation(boneIndex, dof, mode), curve);
            }
        }

        public bool IsHaveAnimationCurveTransformScale(int boneIndex)
        {
            if (currentClip == null || boneIndex < 0 || boneIndex >= editBones.Length)
                return false;
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingTransformScale(boneIndex, 0));
                if (curve != null)
                    return true;
            }
            return false;
        }
        public Vector3 GetAnimationValueTransformScale(int boneIndex, float time = -1f)
        {
            if (currentClip == null || boneIndex < 0 || boneIndex >= editBones.Length)
                return boneSaveTransforms[boneIndex].localScale;
            time = GetFrameSnapTime(time);
            Vector3 result = boneSaveTransforms[boneIndex].localScale;
            for (int i = 0; i < 3; i++)
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingTransformScale(boneIndex, i));
                if (curve != null)
                {
                    result[i] = curve.Evaluate(time);
                }
            }
            return result;
        }
        public void SetAnimationValueTransformScale(int boneIndex, Vector3 scale, float time = -1f)
        {
            if (boneIndex < 0 || boneIndex >= editBones.Length)
                return;
            if (!BeginChangeAnimationCurve(currentClip, "Change Transform Scale"))
                return;
            time = GetFrameSnapTime(time);
            bool removeCurve = false;
            if (isHuman && humanoidConflict[boneIndex])
            {
                EditorCommon.ShowNotification("Conflict");
                Debug.LogErrorFormat(Language.GetText(Language.Help.LogGenericCurveHumanoidConflictError), editBones[boneIndex].name);
                removeCurve = true;
            }
            if (removeCurve)
            {
                for (int i = 0; i < 3; i++)
                {
                    SetEditorCurveCache(AnimationCurveBindingTransformScale(boneIndex, i), null);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    var curve = GetAnimationCurveTransformScale(boneIndex, i);
                    SetKeyframe(curve, time, scale[i]);
                    SetAnimationCurveTransformScale(boneIndex, i, curve);
                }
            }
            if (uAw.TimeToFrameRound(time) == uAw.GetCurrentFrame())
            {
                SetUpdateIKtargetBone(boneIndex);
            }
        }
        public AnimationCurve GetAnimationCurveTransformScale(int boneIndex, int dof)
        {
            var binding = AnimationCurveBindingTransformScale(boneIndex, dof);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = boneSaveTransforms[boneIndex].localScale;
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue[dof]);
                AddKeyframe(curve, currentClip.length, defaultValue[dof]);
                //Created
                AddAnimationWindowSynchroSelection(binding);
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveTransformScale(int boneIndex, int dof, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change Transform Scale"))
            {
                SetEditorCurveCache(AnimationCurveBindingTransformScale(boneIndex, dof), curve);
            }
        }

        public bool IsHaveAnimationCurveBlendShape(SkinnedMeshRenderer renderer, string name)
        {
            if (currentClip == null || renderer == null || renderer.sharedMesh == null)
                return false;
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingBlendShape(renderer, name));
                if (curve != null)
                    return true;
            }
            return false;
        }
        public float GetAnimationValueBlendShape(SkinnedMeshRenderer renderer, string name, float time = -1f)
        {
            if (currentClip == null || renderer == null || renderer.sharedMesh == null)
                return 0f;
            time = GetFrameSnapTime(time);
            float result = blendShapeWeightSave.GetOriginalWeight(renderer, name);
            {
                var curve = GetEditorCurveCache(AnimationCurveBindingBlendShape(renderer, name));
                if (curve != null)
                {
                    result = curve.Evaluate(time);
                }
            }
            return result;
        }
        public void SetAnimationValueBlendShape(SkinnedMeshRenderer renderer, string name, float value, float time = -1f)
        {
            if (renderer == null || renderer.sharedMesh == null)
                return;
            if (!BeginChangeAnimationCurve(currentClip, "Change BlendShape"))
                return;
            time = GetFrameSnapTime(time);
            {
                var curve = GetAnimationCurveBlendShape(renderer, name);
                SetKeyframe(curve, time, value);
                SetAnimationCurveBlendShape(renderer, name, curve);
            }
        }
        public AnimationCurve GetAnimationCurveBlendShape(SkinnedMeshRenderer renderer, string name)
        {
            var binding = AnimationCurveBindingBlendShape(renderer, name);
            var curve = GetEditorCurveCache(binding);
            if (curve == null)
            {
                var defaultValue = blendShapeWeightSave.GetOriginalWeight(renderer, name);
                curve = new AnimationCurve();
                AddKeyframe(curve, 0f, defaultValue);
                AddKeyframe(curve, currentClip.length, defaultValue);
                //Created
                AddAnimationWindowSynchroSelection(binding);
                uAw.Repaint();
            }
            return curve;
        }
        public void SetAnimationCurveBlendShape(SkinnedMeshRenderer renderer, string name, AnimationCurve curve)
        {
            if (BeginChangeAnimationCurve(currentClip, "Change BlendShape"))
            {
                SetEditorCurveCache(AnimationCurveBindingBlendShape(renderer, name), curve);
            }
        }
        #endregion

        private void UndoRedoPerformed()
        {
            if (isEditError) return;

            UpdateSkeletonShowBoneList();
            ToolsParameterRelatedCurveReset();

            SetUpdateResampleAnimation();
            SetSynchroIKtargetAll(true);
            EditorApplication.delayCall += () =>
            {
                SetUpdateResampleAnimation();
                SetSynchroIKtargetAll(true);
            };

            InternalEditorUtility.RepaintAllViews();
        }
    }
}
