using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace VeryAnimation
{
    public class UAvatar
    {
        private Func<int, float> dg_GetAxisLength;
        private Func<int, Quaternion> dg_GetPreRotation;
        private Func<int, Quaternion> dg_GetPostRotation;
        private Func<int, Quaternion, Quaternion, Quaternion> dg_GetZYPostQ;
        private Func<int, Vector3, Quaternion> dg_GetZYRoll;
        private Func<int, Vector3> dg_GetLimitSign;

        public float GetAxisLength(Avatar avatar, int humanId)
        {
            if (dg_GetAxisLength == null || (UnityEngine.Object)dg_GetAxisLength.Target != avatar)
                dg_GetAxisLength = (Func<int, float>)Delegate.CreateDelegate(typeof(Func<int, float>), avatar, avatar.GetType().GetMethod("GetAxisLength", BindingFlags.NonPublic | BindingFlags.Instance));
            return dg_GetAxisLength(humanId);
        }

        public Quaternion GetPreRotation(Avatar avatar, int humanId)
        {
            if (dg_GetPreRotation == null || (UnityEngine.Object)dg_GetPreRotation.Target != avatar)
                dg_GetPreRotation = (Func<int, Quaternion>)Delegate.CreateDelegate(typeof(Func<int, Quaternion>), avatar, avatar.GetType().GetMethod("GetPreRotation", BindingFlags.NonPublic | BindingFlags.Instance));
            return dg_GetPreRotation(humanId);
        }

        public Quaternion GetPostRotation(Avatar avatar, int humanId)
        {
            if (dg_GetPostRotation == null || (UnityEngine.Object)dg_GetPostRotation.Target != avatar)
                dg_GetPostRotation = (Func<int, Quaternion>)Delegate.CreateDelegate(typeof(Func<int, Quaternion>), avatar, avatar.GetType().GetMethod("GetPostRotation", BindingFlags.NonPublic | BindingFlags.Instance));
            return dg_GetPostRotation(humanId);
        }

        public Quaternion GetZYPostQ(Avatar avatar, int humanId, Quaternion parentQ, Quaternion q)
        {
            if (dg_GetZYPostQ == null || (UnityEngine.Object)dg_GetZYPostQ.Target != avatar)
                dg_GetZYPostQ = (Func<int, Quaternion, Quaternion, Quaternion>)Delegate.CreateDelegate(typeof(Func<int, Quaternion, Quaternion, Quaternion>), avatar, avatar.GetType().GetMethod("GetZYPostQ", BindingFlags.NonPublic | BindingFlags.Instance));
            return dg_GetZYPostQ(humanId, parentQ, q);
        }

        public Quaternion GetZYRoll(Avatar avatar, int humanId, Vector3 uvw)
        {
            if (dg_GetZYRoll == null || (UnityEngine.Object)dg_GetZYRoll.Target != avatar)
                dg_GetZYRoll = (Func<int, Vector3, Quaternion>)Delegate.CreateDelegate(typeof(Func<int, Vector3, Quaternion>), avatar, avatar.GetType().GetMethod("GetZYRoll", BindingFlags.NonPublic | BindingFlags.Instance));
            return dg_GetZYRoll(humanId, uvw);
        }

        public Vector3 GetLimitSign(Avatar avatar, int humanId)
        {
            if (dg_GetLimitSign == null || (UnityEngine.Object)dg_GetLimitSign.Target != avatar)
                dg_GetLimitSign = (Func<int, Vector3>)Delegate.CreateDelegate(typeof(Func<int, Vector3>), avatar, avatar.GetType().GetMethod("GetLimitSign", BindingFlags.NonPublic | BindingFlags.Instance));
            return dg_GetLimitSign(humanId);
        }

        #region MuscleLimit
        public class MuscleLimit
        {
            public Vector3 min;
            public Vector3 max;
        }
        public MuscleLimit GetMuscleLimit(Avatar avatar, HumanBodyBones humanoidIndex)
        {
            if (humanoidIndex < 0)
                return null;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            int skeletonIndex = -1;
            if (humanoidIndex <= HumanBodyBones.Jaw || humanoidIndex == HumanBodyBones.UpperChest)
            {
                int humanId = -1;
                if (humanoidIndex <= HumanBodyBones.Chest) humanId = (int)humanoidIndex;
                else if (humanoidIndex <= HumanBodyBones.Jaw) humanId = (int)humanoidIndex + 1;
                else humanId = 9;
                var pHumanBoneIndexArray = serializedObject.FindProperty("m_Avatar.m_Human.data.m_HumanBoneIndex");
                if (pHumanBoneIndexArray == null || !pHumanBoneIndexArray.isArray || humanId < 0 || humanId >= pHumanBoneIndexArray.arraySize)
                    return null;
                skeletonIndex = pHumanBoneIndexArray.GetArrayElementAtIndex(humanId).intValue;
            }
            else if (humanoidIndex <= HumanBodyBones.LeftLittleDistal)
            {
                int handId = (int)humanoidIndex - (int)HumanBodyBones.LeftThumbProximal;
                var pHandBoneIndexArray = serializedObject.FindProperty("m_Avatar.m_Human.data.m_LeftHand.data.m_HandBoneIndex");
                if (pHandBoneIndexArray == null || !pHandBoneIndexArray.isArray || handId < 0 || handId >= pHandBoneIndexArray.arraySize)
                    return null;
                skeletonIndex = pHandBoneIndexArray.GetArrayElementAtIndex(handId).intValue;
            }
            else if (humanoidIndex <= HumanBodyBones.RightLittleDistal)
            {
                int handId = (int)humanoidIndex - (int)HumanBodyBones.RightThumbProximal;
                var pHandBoneIndexArray = serializedObject.FindProperty("m_Avatar.m_Human.data.m_RightHand.data.m_HandBoneIndex");
                if (pHandBoneIndexArray == null || !pHandBoneIndexArray.isArray || handId < 0 || handId >= pHandBoneIndexArray.arraySize)
                    return null;
                skeletonIndex = pHandBoneIndexArray.GetArrayElementAtIndex(handId).intValue;
            }
            var pNodeArray = serializedObject.FindProperty("m_Avatar.m_Human.data.m_Skeleton.data.m_Node");
            if (pNodeArray == null || !pNodeArray.isArray || skeletonIndex < 0 || skeletonIndex >= pNodeArray.arraySize)
                return null;
            var pNode = pNodeArray.GetArrayElementAtIndex(skeletonIndex);
            if (pNode == null)
                return null;
            var axedId = pNode.FindPropertyRelative("m_AxesId").intValue;
            if (axedId < 0)
                return null;
            var pAxesArray = serializedObject.FindProperty("m_Avatar.m_Human.data.m_Skeleton.data.m_AxesArray");
            if (pAxesArray == null || !pAxesArray.isArray || axedId < 0 || axedId >= pAxesArray.arraySize)
                return null;
            var pAxes = pAxesArray.GetArrayElementAtIndex(axedId);
            if (pAxes == null)
                return null;
            Vector3 min = new Vector3(pAxes.FindPropertyRelative("m_Limit.m_Min.x").floatValue,
                                        pAxes.FindPropertyRelative("m_Limit.m_Min.y").floatValue,
                                        pAxes.FindPropertyRelative("m_Limit.m_Min.z").floatValue) * Mathf.Rad2Deg;
            Vector3 max = new Vector3(pAxes.FindPropertyRelative("m_Limit.m_Max.x").floatValue,
                                        pAxes.FindPropertyRelative("m_Limit.m_Max.y").floatValue,
                                        pAxes.FindPropertyRelative("m_Limit.m_Max.z").floatValue) * Mathf.Rad2Deg;
            return new MuscleLimit() { min = min, max = max };
        }
        public MuscleLimit GetMuscleLimitNonError(Avatar avatar, HumanBodyBones humanoidIndex)
        {
            var ml = GetMuscleLimit(avatar, humanoidIndex);
            if (ml != null) return ml;

            var muscleX = HumanTrait.MuscleFromBone((int)humanoidIndex, 0);
            var muscleY = HumanTrait.MuscleFromBone((int)humanoidIndex, 1);
            var muscleZ = HumanTrait.MuscleFromBone((int)humanoidIndex, 2);
            return new MuscleLimit()
            {
                min = new Vector3(muscleX >= 0 ? HumanTrait.GetMuscleDefaultMin(muscleX) : 0f,
                                    muscleY >= 0 ? HumanTrait.GetMuscleDefaultMin(muscleY) : 0f,
                                    muscleZ >= 0 ? HumanTrait.GetMuscleDefaultMin(muscleZ) : 0f),
                max = new Vector3(muscleX >= 0 ? HumanTrait.GetMuscleDefaultMax(muscleX) : 0f,
                                    muscleY >= 0 ? HumanTrait.GetMuscleDefaultMax(muscleY) : 0f,
                                    muscleZ >= 0 ? HumanTrait.GetMuscleDefaultMax(muscleZ) : 0f)
            };
        }
        #endregion

        public float GetArmStretch(Avatar avatar)
        {
            if (avatar == null) return 0f;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            var pArmStretch = serializedObject.FindProperty("m_Avatar.m_Human.data.m_ArmStretch");
            if (pArmStretch == null)
                return 0f;
            return pArmStretch.floatValue;
        }
        public void SetArmStretch(Avatar avatar, float value)
        {
            if (avatar == null) return;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            var pArmStretch = serializedObject.FindProperty("m_Avatar.m_Human.data.m_ArmStretch");
            if (pArmStretch == null) return;
            pArmStretch.floatValue = value;
            serializedObject.ApplyModifiedProperties();
        }
        public float GetLegStretch(Avatar avatar)
        {
            if (avatar == null) return 0f;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            var pLegStretch = serializedObject.FindProperty("m_Avatar.m_Human.data.m_LegStretch");
            if (pLegStretch == null)
                return 0f;
            return pLegStretch.floatValue;
        }
        public void SetLegStretch(Avatar avatar, float value)
        {
            if (avatar == null) return;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            var pLegStretch = serializedObject.FindProperty("m_Avatar.m_Human.data.m_LegStretch");
            if (pLegStretch == null) return;
            pLegStretch.floatValue = value;
            serializedObject.ApplyModifiedProperties();
        }
        public bool GetHasLeftHand(Avatar avatar)
        {
            if (avatar == null) return false;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            var pHasLeftHand = serializedObject.FindProperty("m_Avatar.m_Human.data.m_HasLeftHand");
            if (pHasLeftHand == null)
                return false;
            return pHasLeftHand.boolValue;
        }
        public bool GetHasRightHand(Avatar avatar)
        {
            if (avatar == null) return false;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            var pHasRightHand = serializedObject.FindProperty("m_Avatar.m_Human.data.m_HasRightHand");
            if (pHasRightHand == null)
                return false;
            return pHasRightHand.boolValue;
        }
        public bool GetHasTDoF(Avatar avatar)
        {
            if (avatar == null) return false;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            var pHasTDoF = serializedObject.FindProperty("m_Avatar.m_Human.data.m_HasTDoF");
            if (pHasTDoF == null)
                return false;
            return pHasTDoF.boolValue;
        }
        public string GetGenericRootMotionBonePath(Avatar avatar)
        {
            if (avatar == null) return null;
            var serializedObject = new UnityEditor.SerializedObject(avatar);
            int index;
            {
                var pRootMotionBoneIndex = serializedObject.FindProperty("m_Avatar.m_RootMotionBoneIndex");
                if (pRootMotionBoneIndex == null)
                    return null;
                index = pRootMotionBoneIndex.intValue;
                if (index < 0)
                    return null;
            }
            long id;
            {
                var pID = serializedObject.FindProperty("m_Avatar.m_AvatarSkeleton.data.m_ID");
                if (pID == null || !pID.isArray || index >= pID.arraySize)
                    return null;
                id = pID.GetArrayElementAtIndex(index).longValue;
            }
            {
                var pTOS = serializedObject.FindProperty("m_TOS");
                if (pTOS == null || !pTOS.isArray)
                    return null;
                for (int i = 0; i < pTOS.arraySize; i++)
                {
                    var pElement = pTOS.GetArrayElementAtIndex(i);
                    if (pElement == null) continue;
                    var pFirst = pElement.FindPropertyRelative("first");
                    if (pFirst == null) continue;
                    if (id != pFirst.longValue) continue;
                    var pSecond = pElement.FindPropertyRelative("second");
                    if (pSecond == null) continue;
                    return pSecond.stringValue;
                }
            }
            return null;
        }
    }
}
