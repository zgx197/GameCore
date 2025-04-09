using UnityEngine;
using SysVec3 = System.Numerics.Vector3;
using UnityVec3 = UnityEngine.Vector3;

namespace GameCore.Unity.Adapters
{
    /// <summary>
    /// Unity和System.Numerics之间的Vector3转换适配器
    /// </summary>
    public static class Vector3Adapter
    {
        public static UnityVec3 ToUnity(SysVec3 systemVector)
        {
            return new UnityVec3(systemVector.X, systemVector.Y, systemVector.Z);
        }
        
        public static SysVec3 ToSystem(UnityVec3 unityVector)
        {
            return new SysVec3(unityVector.x, unityVector.y, unityVector.z);
        }
        
        public static UnityVec3[] ToUnityArray(SysVec3[] systemVectors)
        {
            if (systemVectors == null) return null;
            var array = new UnityVec3[systemVectors.Length];
            for (int i = 0; i < systemVectors.Length; i++)
            {
                array[i] = ToUnity(systemVectors[i]);
            }
            return array;
        }
        
        public static SysVec3[] ToSystemArray(UnityVec3[] unityVectors)
        {
            if (unityVectors == null) return null;
            var array = new SysVec3[unityVectors.Length];
            for (int i = 0; i < unityVectors.Length; i++)
            {
                array[i] = ToSystem(unityVectors[i]);
            }
            return array;
        }
    }
} 