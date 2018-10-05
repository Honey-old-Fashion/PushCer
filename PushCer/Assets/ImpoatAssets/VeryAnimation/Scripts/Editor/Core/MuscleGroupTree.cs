using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VeryAnimation
{
    [Serializable]
    public class MuscleGroupTree
    {
        private VeryAnimationWindow vaw { get { return VeryAnimationWindow.instance; } }
        private VeryAnimation va { get { return VeryAnimation.instance; } }
        private VeryAnimationEditorWindow vae { get { return VeryAnimationEditorWindow.instance; } }

        public enum MuscleGroupMode
        {
            Category,
            Part,
            Total,
        }
        private static readonly string[] MuscleGroupModeString =
        {
            MuscleGroupMode.Category.ToString(),
            MuscleGroupMode.Part.ToString(),
        };

        public MuscleGroupMode muscleGroupMode;

        private class MuscleInfo
        {
            public HumanBodyBones hi;
            public int dof;
            public float scale = 1f;
        }
        private class MuscleGroupNode
        {
            public string name;
            public bool foldout;
            public int dof = -1;
            public MuscleInfo[] infoList;
            public MuscleGroupNode[] children;
        }
        private MuscleGroupNode[] muscleGroupNode;
        private Dictionary<MuscleGroupNode, int> muscleGroupTreeTable;

        [SerializeField]
        private float[] muscleGroupValues;

        public MuscleGroupTree()
        {
            #region MuscleGroupNode
            {
                muscleGroupNode = new MuscleGroupNode[]
                {
#region Category
                    new MuscleGroupNode() { name = MuscleGroupMode.Category.ToString(),
                        children = new MuscleGroupNode[]
                        {
#region Open Close
                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Head, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 2 },
                                },
                                children = new MuscleGroupNode[]
                                {
                                    new MuscleGroupNode() { name = "Head", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.Head, dof = 2 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Body", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 2 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Left Arm", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 2 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Arm", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 2 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Left Leg", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 2 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Leg", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 2 },
                                        },
                                    },
                                },
                            },
#endregion
#region Left Right
                            new MuscleGroupNode() { name = "Left Right", dof = 1,
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Head, dof = 1 },
                                },
                            },
#endregion
#region Roll Left Right
                            new MuscleGroupNode() { name = "Roll Left Right", dof = 0,
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.Head, dof = 0 },
                                },
                            },
#endregion
#region In Out
                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 1 },
                                },
                                children = new MuscleGroupNode[]
                                {
                                    new MuscleGroupNode() { name = "Left Arm", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 1 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Arm", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 1 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Left Leg", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 1 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Leg", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 1 },
                                        },
                                    },
                                },
                            },
#endregion
#region Roll In Out
                            new MuscleGroupNode() { name = "Roll In Out", dof = 0,
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 0 },
                                },
                                children = new MuscleGroupNode[]
                                {
                                    new MuscleGroupNode() { name = "Left Arm", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 0 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Arm", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 0 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Left Leg", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 0 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Leg", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 0 },
                                        },
                                    },
                                },
                            },
#endregion
#region Finger Open Close
                            new MuscleGroupNode() { name = "Finger Open Close", dof = 2,
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleDistal, dof = 2 },
                                },
                                children = new MuscleGroupNode[]
                                {
                                    new MuscleGroupNode() { name = "Left Finger", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Left Thumb", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Left Index", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Left Middle", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Left Ring", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Left Little", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleDistal, dof = 2 },
                                                },
                                            },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Finger", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingDistal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Right Thumb", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Right Index", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Right Middle", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Right Ring", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightRingIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightRingDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "Right Little", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleDistal, dof = 2 },
                                                },
                                            },
                                        },
                                    },
                                },
                            },
#endregion
#region Finger In Out
                            new MuscleGroupNode() { name = "Finger In Out", dof = 1,
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 1 },
                                },
                                children = new MuscleGroupNode[]
                                {
                                    new MuscleGroupNode() { name = "Left Finger", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 1 },
                                        },
                                    },
                                    new MuscleGroupNode() { name = "Right Finger", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 1 },
                                        },
                                    },
                                },
                            },
#endregion
                        },
                    },
#endregion
#region Part
                    new MuscleGroupNode() { name = MuscleGroupMode.Category.ToString(),
                        children = new MuscleGroupNode[]
                        {
#region Face
                            new MuscleGroupNode() { name = "Face",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftEye, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftEye, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightEye, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightEye, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Jaw, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Jaw, dof = 1 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Eyes Down Up
                                    new MuscleGroupNode() { name = "Eyes Down Up",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftEye, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightEye, dof = 2 },
                                        },
                                    },
#endregion
#region Eyes Left Right
                                    new MuscleGroupNode() { name = "Eyes Left Right",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftEye, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightEye, dof = 1, scale = -1f },
                                        },
                                    },
#endregion
#region Jaw
                                    new MuscleGroupNode() { name = "Jaw",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Jaw, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.Jaw, dof = 1 },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Head
                            new MuscleGroupNode() { name = "Head",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.Head, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Head, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Head, dof = 0 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Open Close
                                    new MuscleGroupNode() { name = "Open Close", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.Head, dof = 2 },
                                        },
                                    },
#endregion
#region Left Right
                                    new MuscleGroupNode() { name = "Left Right", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.Head, dof = 1 },
                                        },
                                    },
#endregion
#region Roll Left Right
                                    new MuscleGroupNode() { name = "Roll Left Right", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Neck, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.Head, dof = 0 },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Body
                            new MuscleGroupNode() { name = "Body",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 0 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Open Close
                                    new MuscleGroupNode() { name = "Open Close", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 2 },
                                        },
                                    },
#endregion
#region Left Right
                                    new MuscleGroupNode() { name = "Left Right", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 1 },
                                        },
                                    },
#endregion
#region Roll Left Right
                                    new MuscleGroupNode() { name = "Roll Left Right", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.Spine, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.Chest, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.UpperChest, dof = 0 },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Left Arm
                            new MuscleGroupNode() { name = "Left Arm",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 1 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Open Close
                                    new MuscleGroupNode() { name = "Open Close", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 2 },
                                        },
                                    },
#endregion
#region In Out
                                    new MuscleGroupNode() { name = "In Out", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftShoulder, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftHand, dof = 1 },
                                        },
                                    },
#endregion
#region Roll In Out
                                    new MuscleGroupNode() { name = "Roll In Out", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperArm, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerArm, dof = 0 },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Right Arm
                            new MuscleGroupNode() { name = "Right Arm",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 1 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Open Close
                                    new MuscleGroupNode() { name = "Open Close", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 2 },
                                        },
                                    },
#endregion
#region In Out
                                    new MuscleGroupNode() { name = "In Out", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightShoulder, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightHand, dof = 1 },
                                        },
                                    },
#endregion
#region Roll In Out
                                    new MuscleGroupNode() { name = "Roll In Out", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperArm, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerArm, dof = 0 },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Left Leg
                            new MuscleGroupNode() { name = "Left Leg",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftToes, dof = 2 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Open Close
                                    new MuscleGroupNode() { name = "Open Close", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 2 },
                                        },
                                    },
#endregion
#region In Out
                                    new MuscleGroupNode() { name = "In Out", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftFoot, dof = 1 },
                                        },
                                    },
#endregion
#region Roll In Out
                                    new MuscleGroupNode() { name = "Roll In Out", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftUpperLeg, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLowerLeg, dof = 0 },
                                        },
                                    },
#endregion
#region Toes
                                    new MuscleGroupNode() { name = "Toes", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftToes, dof = 2 },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Right Leg
                            new MuscleGroupNode() { name = "Right Leg",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 0 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightToes, dof = 2 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Open Close
                                    new MuscleGroupNode() { name = "Open Close", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 2 },
                                        },
                                    },
#endregion
#region In Out
                                    new MuscleGroupNode() { name = "In Out", dof = 1,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightFoot, dof = 1 },
                                        },
                                    },
#endregion
#region Roll In Out
                                    new MuscleGroupNode() { name = "Roll In Out", dof = 0,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightUpperLeg, dof = 0 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLowerLeg, dof = 0 },
                                        },
                                    },
#endregion
#region Toes
                                    new MuscleGroupNode() { name = "Toes", dof = 2,
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightToes, dof = 2 },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Left Finger
                            new MuscleGroupNode() { name = "Left Finger",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleDistal, dof = 2 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Left Thumb
                                    new MuscleGroupNode() { name = "Left Thumb",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftThumbDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftThumbProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Left Index
                                    new MuscleGroupNode() { name = "Left Index",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftIndexDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftIndexProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Left Middle
                                    new MuscleGroupNode() { name = "Left Middle",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftMiddleDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftMiddleProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Left Ring
                                    new MuscleGroupNode() { name = "Left Ring",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftRingDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftRingProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Left Little
                                    new MuscleGroupNode() { name = "Left Little",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.LeftLittleDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.LeftLittleProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
#region Right Finger
                            new MuscleGroupNode() { name = "Right Finger",
                                infoList = new MuscleInfo[]
                                {
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightRingDistal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 1 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleIntermediate, dof = 2 },
                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleDistal, dof = 2 },
                                },
                                children = new MuscleGroupNode[]
                                {
#region Right Thumb
                                    new MuscleGroupNode() { name = "Right Thumb",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightThumbDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightThumbProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Right Index
                                    new MuscleGroupNode() { name = "Right Index",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightIndexDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightIndexProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Right Middle
                                    new MuscleGroupNode() { name = "Right Middle",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightMiddleDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightMiddleProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Right Ring
                                    new MuscleGroupNode() { name = "Right Ring",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightRingDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightRingIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightRingDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightRingProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
#region Right Little
                                    new MuscleGroupNode() { name = "Right Little",
                                        infoList = new MuscleInfo[]
                                        {
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 1 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleIntermediate, dof = 2 },
                                            new MuscleInfo() { hi = HumanBodyBones.RightLittleDistal, dof = 2 },
                                        },
                                        children = new MuscleGroupNode[]
                                        {
                                            new MuscleGroupNode() { name = "Open Close", dof = 2,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleIntermediate, dof = 2 },
                                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleDistal, dof = 2 },
                                                },
                                            },
                                            new MuscleGroupNode() { name = "In Out", dof = 1,
                                                infoList = new MuscleInfo[]
                                                {
                                                    new MuscleInfo() { hi = HumanBodyBones.RightLittleProximal, dof = 1 },
                                                },
                                            },
                                        },
                                    },
#endregion
                                },
                            },
#endregion
                        },
                    },
#endregion
                };

                {
                    muscleGroupTreeTable = new Dictionary<MuscleGroupNode, int>();
                    int counter = 0;
                    Action<MuscleGroupNode> AddTable = null;
                    AddTable = (mg) =>
                    {
                        muscleGroupTreeTable.Add(mg, counter++);
                        if (mg.children != null)
                        {
                            foreach (var child in mg.children)
                            {
                                AddTable(child);
                            }
                        }
                    };
                    foreach (var node in muscleGroupNode)
                    {
                        AddTable(node);
                    }

                    muscleGroupValues = new float[muscleGroupTreeTable.Count];
                }
            }
            #endregion
        }

        public void MuscleGroupToolbarGUI()
        {
            EditorGUI.BeginChangeCheck();
            var m = (MuscleGroupTree.MuscleGroupMode)GUILayout.Toolbar((int)muscleGroupMode, MuscleGroupTree.MuscleGroupModeString, EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(vae, "Change Muscle Group Mode");
                muscleGroupMode = m;
            }
        }

        private struct MuscleValue
        {
            public int muscleIndex;
            public float value;
        }
        public void MuscleGroupTreeGUI()
        {
            RowCount = 0;

            var e = Event.current;

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                var mgRoot = muscleGroupNode[(int)muscleGroupMode].children;

                #region Reset All
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select All", GUILayout.Width(100)))
                    {
                        if (e.control || e.shift)
                        {
                            var combineGoList = new List<GameObject>(va.selectionGameObjects);
                            combineGoList.Add(vaw.gameObject);
                            combineGoList.AddRange(va.humanoidBones);
                            va.SelectGameObjects(combineGoList.ToArray());
                        }
                        else
                        {
                            var combineGoList = new List<GameObject>();
                            combineGoList.Add(vaw.gameObject);
                            combineGoList.AddRange(va.humanoidBones);
                            Selection.activeGameObject = vaw.gameObject;
                            va.SelectGameObjects(combineGoList.ToArray());
                        }
                    }
                    GUILayout.Space(10);
                    if (GUILayout.Button("Root", GUILayout.Width(100)))
                    {
                        if (e.control || e.shift)
                        {
                            var combineGoList = new List<GameObject>(va.selectionGameObjects);
                            combineGoList.Add(vaw.gameObject);
                            va.SelectGameObjects(combineGoList.ToArray());
                        }
                        else
                        {
                            va.SelectGameObject(vaw.gameObject);
                        }
                    }
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Reset All", GUILayout.Width(100)))
                    {
                        Undo.RecordObject(vae, "Reset All Muscle Group");
                        foreach (var root in mgRoot)
                        {
                            List<MuscleValue> muscles = new List<MuscleValue>();
                            SetMuscleGroupValue(root, 0f, muscles);
                            SetAnimationCurveMuscleValues(muscles);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                #endregion

                EditorGUILayout.Space();

                #region Muscle
                foreach (var root in mgRoot)
                {
                    MuscleGroupTreeNodeGUI(root, 0);
                }
                #endregion
            }
            EditorGUILayout.EndVertical();
        }
        #region MuscleGroupTreeGUI
        private int RowCount = 0;
        private const int IndentWidth = 15;
        private int GetTreeLevel(MuscleGroupNode mg, int level)
        {
            if (mg.foldout)
            {
                if (mg.children != null && mg.children.Length > 0)
                {
                    int tmp = level;
                    foreach (var child in mg.children)
                    {
                        tmp = Math.Max(tmp, GetTreeLevel(child, level + 1));
                    }
                    level = tmp;
                }
                else if (mg.infoList != null && mg.infoList.Length > 0)
                {
                    level++;
                }
            }
            return level;
        }
        private void SetMuscleGroupFoldout(MuscleGroupNode mg, bool foldout)
        {
            mg.foldout = foldout;
            if (mg.children != null)
            {
                foreach (var child in mg.children)
                {
                    SetMuscleGroupFoldout(child, foldout);
                }
            }
        }
        private bool ContainsMuscleGroup(MuscleGroupNode mg)
        {
            if (mg.infoList != null)
            {
                foreach (var info in mg.infoList)
                {
                    var muscleIndex = HumanTrait.MuscleFromBone((int)info.hi, info.dof);
                    if (va.humanoidMuscleContains[muscleIndex]) return true;
                }
            }
            if (mg.children != null && mg.children.Length > 0)
            {
                foreach (var child in mg.children)
                {
                    if (ContainsMuscleGroup(child)) return true;
                }
            }
            return false;
        }
        private void SetMuscleGroupValue(MuscleGroupNode mg, float value, List<MuscleValue> muscles)
        {
            muscleGroupValues[muscleGroupTreeTable[mg]] = value;
            if (mg.infoList != null)
            {
                foreach (var info in mg.infoList)
                {
                    var muscleIndex = HumanTrait.MuscleFromBone((int)info.hi, info.dof);
                    muscles.Add(new MuscleValue() { muscleIndex = muscleIndex, value = value * info.scale });
                }
            }
            if (mg.children != null && mg.children.Length > 0)
            {
                foreach (var child in mg.children)
                {
                    SetMuscleGroupValue(child, value, muscles);
                }
            }
        }
        private void SetAnimationCurveMuscleValues(List<MuscleValue> muscles)
        {
            bool[] doneFlags = null;
            for (int i = 0; i < muscles.Count; i++)
            {
                if (va.mirrorEnable)
                {
                    if (doneFlags == null) doneFlags = new bool[HumanTrait.MuscleCount];
                    var mmuscleIndex = va.GetMirrorMuscleIndex(muscles[i].muscleIndex);
                    if (mmuscleIndex >= 0 && doneFlags[mmuscleIndex])
                        continue;
                    doneFlags[muscles[i].muscleIndex] = true;
                }
                if (!va.IsHaveAnimationCurveAnimatorMuscle(muscles[i].muscleIndex) && muscles[i].value == 0f) continue;
                va.SetAnimationValueAnimatorMuscle(muscles[i].muscleIndex, muscles[i].value);
            }
        }
        private void MuscleGroupTreeNodeGUI(MuscleGroupNode mg, int level)
        {
            const int FoldoutWidth = 22;
            const int FoldoutSpace = 10;
            var e = Event.current;
            var mgContains = ContainsMuscleGroup(mg);
            EditorGUI.BeginDisabledGroup(!mgContains);
            EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
            {
                var rect = EditorGUILayout.GetControlRect();
                var indentSpace = IndentWidth * level;
                {
                    var r = rect;
                    r.width = indentSpace + FoldoutWidth;
                    EditorGUI.BeginChangeCheck();
                    mg.foldout = EditorGUI.Foldout(r, mg.foldout, "", true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (Event.current.alt)
                            SetMuscleGroupFoldout(mg, mg.foldout);
                    }
                }
                {
                    var r = rect;
                    r.x += FoldoutWidth + indentSpace;
                    r.y += 1;
                    r.width -= FoldoutWidth + indentSpace;
                    r.height += 1;
                    if (GUI.Button(r, new GUIContent(mg.name, muscleGroupValues[muscleGroupTreeTable[mg]].ToString())))
                    {
                        HashSet<HumanBodyBones> humanoidIndexex = new HashSet<HumanBodyBones>();
                        List<EditorCurveBinding> bindings = new List<EditorCurveBinding>();
                        if (mg.infoList != null && mg.infoList.Length > 0)
                        {
                            foreach (var info in mg.infoList)
                            {
                                humanoidIndexex.Add(info.hi);
                                var muscleIndex = HumanTrait.MuscleFromBone((int)info.hi, info.dof);
                                bindings.Add(va.AnimationCurveBindingAnimatorMuscle(muscleIndex));
                            }
                        }
                        if (e.control || e.shift)
                        {
                            var combineGoList = new List<GameObject>(va.selectionGameObjects);
                            var combineVirtualList = new List<HumanBodyBones>();
                            if (va.selectionHumanVirtualBones != null)
                                combineVirtualList.AddRange(va.selectionHumanVirtualBones);
                            foreach (var hi in humanoidIndexex)
                            {
                                if (va.humanoidBones[(int)hi] != null)
                                {
                                    combineGoList.Add(va.humanoidBones[(int)hi]);
                                }
                                else if(VeryAnimation.HumanVirtualBones[(int)hi] != null)
                                {
                                    combineVirtualList.Add(hi);
                                }
                            }
                            va.SelectGameObjects(combineGoList.ToArray(), combineVirtualList.ToArray());
                            var combineBindings = new List<EditorCurveBinding>(va.uAw.GetCurveSelection());
                            combineBindings.AddRange(bindings);
                            va.SetAnimationWindowSynchroSelection(combineBindings.ToArray());
                        }
                        else
                        {
                            if (humanoidIndexex.Count > 0)
                            {
                                foreach (var hi in humanoidIndexex)
                                {
                                    if (va.humanoidBones[(int)hi] != null)
                                    {
                                        Selection.activeGameObject = va.humanoidBones[(int)hi];
                                        break;
                                    }
                                }
                            }
                            va.SelectHumanoidBones(humanoidIndexex.ToArray());
                            va.SetAnimationWindowSynchroSelection(bindings.ToArray());
                        }
                    }
                }
                GUILayout.Space(FoldoutSpace);
            }
            {
                var saveBackgroundColor = GUI.backgroundColor;
                switch (mg.dof)
                {
                case 0: GUI.backgroundColor = Handles.xAxisColor; break;
                case 1: GUI.backgroundColor = Handles.yAxisColor; break;
                case 2: GUI.backgroundColor = Handles.zAxisColor; break;
                }
                EditorGUI.BeginChangeCheck();
                var value = GUILayout.HorizontalSlider(muscleGroupValues[muscleGroupTreeTable[mg]], -1f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(vae, "Change Muscle Group");
                    List<MuscleValue> muscles = new List<MuscleValue>();
                    SetMuscleGroupValue(mg, value, muscles);
                    SetAnimationCurveMuscleValues(muscles);
                }
                GUI.backgroundColor = saveBackgroundColor;
            }
            GUILayout.Space(IndentWidth * GetTreeLevel(mg, 0));
            if (GUILayout.Button("Reset", GUILayout.Width(44)))
            {
                Undo.RecordObject(vae, "Reset Muscle Group");
                muscleGroupValues[muscleGroupTreeTable[mg]] = 0f;
                List<MuscleValue> muscles = new List<MuscleValue>();
                if (mg.children != null && mg.children.Length > 0)
                {
                    foreach (var root in mg.children)
                    {
                        List<MuscleValue> sub = new List<MuscleValue>();
                        SetMuscleGroupValue(root, 0f, sub);
                        foreach (var i in sub)
                            muscles.Add(i);
                    }
                }
                else if (mg.infoList != null && mg.infoList.Length > 0)
                {
                    foreach (var info in mg.infoList)
                    {
                        var muscleIndex = HumanTrait.MuscleFromBone((int)info.hi, info.dof);
                        muscles.Add(new MuscleValue() { muscleIndex = muscleIndex, value = 0f });
                    }
                }
                SetAnimationCurveMuscleValues(muscles);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            if (mg.foldout)
            {
                EditorGUI.indentLevel++;
                if (mg.children != null && mg.children.Length > 0)
                {
                    foreach (var child in mg.children)
                    {
                        MuscleGroupTreeNodeGUI(child, level + 1);
                    }
                }
                else if (mg.infoList != null && mg.infoList.Length > 0)
                {
                    #region Muscle
                    foreach (var info in mg.infoList)
                    {
                        var muscleIndex = HumanTrait.MuscleFromBone((int)info.hi, info.dof);
                        var humanoidIndex = (HumanBodyBones)HumanTrait.BoneFromMuscle(muscleIndex);
                        var muscleValue = va.GetAnimationValueAnimatorMuscle(muscleIndex);
                        EditorGUILayout.BeginHorizontal(RowCount++ % 2 == 0 ? vaw.guiStyleAnimationRowEvenStyle : vaw.guiStyleAnimationRowOddStyle);
                        EditorGUI.indentLevel++;
                        {
                            var rect = EditorGUILayout.GetControlRect();
                            {
                                var offset = IndentWidth * EditorGUI.indentLevel + FoldoutWidth - IndentWidth;
                                rect.x += offset;
                                rect.width -= offset;
                                rect.y += 1;
                                rect.height += 1;
                            }
                            var contains = va.humanoidBones[(int)humanoidIndex] != null || VeryAnimation.HumanVirtualBones[(int)humanoidIndex] != null;
                            EditorGUI.BeginDisabledGroup(!contains);
                            if (GUI.Button(rect, new GUIContent(va.musclePropertyName.Names[muscleIndex], muscleValue.ToString())))
                            {
                                if (e.control || e.shift)
                                {
                                    var combineGoList = new List<GameObject>(va.selectionGameObjects);
                                    combineGoList.Add(va.humanoidBones[(int)humanoidIndex]);
                                    va.SelectGameObjects(combineGoList.ToArray());
                                    var combineBindings = new List<EditorCurveBinding>(va.uAw.GetCurveSelection());
                                    combineBindings.Add(va.AnimationCurveBindingAnimatorMuscle(muscleIndex));
                                    va.SetAnimationWindowSynchroSelection(combineBindings.ToArray());
                                }
                                else
                                {
                                    va.SelectHumanoidBones(new HumanBodyBones[] { humanoidIndex });
                                    va.SetAnimationWindowSynchroSelection(new EditorCurveBinding[] { va.AnimationCurveBindingAnimatorMuscle(muscleIndex) });
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                            GUILayout.Space(FoldoutSpace);
                        }
                        {
                            EditorGUI.BeginDisabledGroup(!va.humanoidMuscleContains[muscleIndex]);
                            var saveBackgroundColor = GUI.backgroundColor;
                            switch (info.dof)
                            {
                            case 0: GUI.backgroundColor = Handles.xAxisColor; break;
                            case 1: GUI.backgroundColor = Handles.yAxisColor; break;
                            case 2: GUI.backgroundColor = Handles.zAxisColor; break;
                            }
                            EditorGUI.BeginChangeCheck();
                            var value2 = GUILayout.HorizontalSlider(muscleValue, -1f, 1f, GUILayout.Width(vaw.editorSettings.settingEditorSliderSize));
                            if (EditorGUI.EndChangeCheck())
                            {
                                va.SetAnimationValueAnimatorMuscle(muscleIndex, value2);
                            }
                            GUI.backgroundColor = saveBackgroundColor;
                        }
                        if (GUILayout.Button("Reset", GUILayout.Width(44)))
                        {
                            if (va.IsHaveAnimationCurveAnimatorMuscle(muscleIndex))
                            {
                                va.SetAnimationValueAnimatorMuscle(muscleIndex, 0f);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion
                }
                EditorGUI.indentLevel--;
            }
        }
        #endregion
    }
}
