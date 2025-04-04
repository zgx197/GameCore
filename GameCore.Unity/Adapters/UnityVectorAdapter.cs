using UnityEngine;
using GameCore.HexGrid;

namespace GameCore.Unity.Adapters
{
    /// <summary>
    /// Unity的Vector3适配器
    /// </summary>
    public static class UnityVectorAdapter
    {
        /// <summary>
        /// 将六边形坐标转换为Unity的Vector3
        /// </summary>
        /// <param name="hexCoord">六边形坐标</param>
        /// <param name="hexSize">六边形大小</param>
        /// <returns>Unity的Vector3</returns>
        public static Vector3 HexToWorld(HexCoord hexCoord, float hexSize = 1.0f) {
            // 使用pointy-top布局
            float x = hexSize * (Mathf.Sqrt(3) * hexCoord.Q + Mathf.Sqrt(3) / 2 * hexCoord.R);
            float z = hexSize * (3.0f / 2 * hexCoord.R);
            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// 将Unity的Vector3转换为六边形坐标
        /// </summary>
        /// <param name="worldPos">Unity的Vector3</param>
        /// <param name="hexSize">六边形大小</param>
        /// <returns>六边形坐标</returns>
        public static HexCoord WorldToHex(Vector3 worldPos, float hexSize = 1.0f) {
            // 逆转换
            float q = (Mathf.Sqrt(3)/3 * worldPos.x - 1.0f/3 * worldPos.z) / hexSize;
            float r = (2.0f/3 * worldPos.z) / hexSize;
            return HexRound(q, r);
        }

        /// <summary>
        /// 四舍五入到最近的六边形坐标
        /// </summary>
        /// <param name="q">Q轴坐标</param>
        /// <param name="r">R轴坐标</param>
        /// <returns>六边形坐标</returns>   
        private static HexCoord HexRound(float q, float r)
        {
            float s = -q - r;
            
            int qi = Mathf.RoundToInt(q);
            int ri = Mathf.RoundToInt(r);
            int si = Mathf.RoundToInt(s);
            
            float qDiff = Mathf.Abs(qi - q);
            float rDiff = Mathf.Abs(ri - r);
            float sDiff = Mathf.Abs(si - s);
            
            if (qDiff > rDiff && qDiff > sDiff)
                qi = -ri - si;
            else if (rDiff > sDiff)
                ri = -qi - si;
            
            return new HexCoord(qi, ri);
        }
    }
}
