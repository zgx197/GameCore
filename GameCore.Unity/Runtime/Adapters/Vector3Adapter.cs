#if UNITY
using SysVec3 = System.Numerics.Vector3;
using UnityVec3 = UnityEngine.Vector3;

namespace GameCore.Unity.Adapters
{
    /// <summary>
    /// Provides static methods for converting between System.Numerics.Vector3 and UnityEngine.Vector3.
    /// </summary>
    public static class Vector3Adapter
    {
        /// <summary>
        /// Converts a UnityEngine.Vector3 to System.Numerics.Vector3.
        /// </summary>
        /// <param name="v">The UnityEngine vector.</param>
        /// <returns>The System.Numerics vector.</returns>
        public static SysVec3 ToSystemNumerics(UnityVec3 v)
        {
            return new SysVec3(v.x, v.y, v.z);
        }

        /// <summary>
        /// Converts a System.Numerics.Vector3 to UnityEngine.Vector3.
        /// </summary>
        /// <param name="v">The System.Numerics vector.</param>
        /// <returns>The UnityEngine vector.</returns>
        public static UnityVec3 ToUnity(SysVec3 v)
        {
            return new UnityVec3(v.X, v.Y, v.Z);
        }
        
        /// <summary>
        /// Converts a UnityEngine.Vector3 array to System.Numerics.Vector3 list.
        /// </summary>
        public static System.Collections.Generic.List<SysVec3> ToSystemNumericsList(UnityVec3[] unityVectors)
        {
            if (unityVectors == null) return new System.Collections.Generic.List<SysVec3>();
            var list = new System.Collections.Generic.List<SysVec3>(unityVectors.Length);
            foreach (var v in unityVectors)
            {
                list.Add(ToSystemNumerics(v));
            }
            return list;
        }
        
        /// <summary>
        /// Converts a System.Numerics.Vector3 list/readonly list to UnityEngine.Vector3 array.
        /// </summary>
        public static UnityVec3[] ToUnityArray(System.Collections.Generic.IReadOnlyList<SysVec3> systemVectors)
        {
             if (systemVectors == null) return System.Array.Empty<UnityVec3>();
            var array = new UnityVec3[systemVectors.Count];
            for (int i = 0; i < systemVectors.Count; i++)
            {
                array[i] = ToUnity(systemVectors[i]);
            }
            return array;
        }
    }
}
#endif 