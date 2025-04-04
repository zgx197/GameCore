using System;

namespace GameCore.HexGrid
{
    /// <summary>
    /// 六边形坐标
    /// </summary>
    public readonly struct HexCoord: IEquatable<HexCoord>
    {
        /// <summary>
        /// Q轴坐标(从西南到东北)
        /// </summary>
        public readonly int Q;
        /// <summary>
        /// R轴坐标(从西北到东南)
        /// </summary>
        public readonly int R;
        /// <summary>
        /// S轴坐标, -Q - R
        /// </summary>
        public int S => -Q - R;

        /// <summary>
        /// 创建新的六边形坐标
        /// </summary>
        /// <param name="q">Q轴坐标</param>
        /// <param name="r">R轴坐标</param>
        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }
        
        /// <summary>
        /// 计算两个六边形坐标之间的距离
        /// </summary>
        /// <param name="a">六边形坐标A</param>
        /// <param name="b">六边形坐标B</param>
        /// <returns>距离</returns>
        public static int Distance(HexCoord a, HexCoord b)
        {
            return Math.Max(
                Math.Max(
                    Math.Abs(a.Q - b.Q),
                    Math.Abs(a.R - b.R)
                ),
                Math.Abs(a.S - b.S)
            );
        }

        /// <summary>
        /// 实现相等比较方法
        /// </summary>
        /// <param name="other">其他六边形坐标</param>
        /// <returns>是否相等</returns>
        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;

        /// <summary>
        /// 重写Equals方法
        /// </summary>
        /// <param name="obj">其他对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object? obj) => obj is HexCoord other && Equals(other);

        /// <summary>
        /// 重写GetHashCode方法
        /// </summary>
        /// <returns>哈希值</returns>
        public override int GetHashCode() => HashCode.Combine(Q, R);

        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="a">六边形坐标A</param>
        /// <param name="b">六边形坐标B</param>
        /// <returns>是否相等</returns>
        public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);

        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="a">六边形坐标A</param>
        /// <param name="b">六边形坐标B</param>
        /// <returns>是否不相等</returns>
        public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);

        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="a">六边形坐标A</param>
        /// <param name="b">六边形坐标B</param>
        public static HexCoord operator +(HexCoord a, HexCoord b) => new HexCoord(a.Q + b.Q, a.R + b.R);

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString() => $"HexCoord({Q}, {R}, {S})";
    }
}
