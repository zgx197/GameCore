using System;

namespace GameCore.ECS.Core
{
    /// <summary>
    /// 实体唯一标识符
    /// 包含索引和版本号，用于处理实体复用时的引用问题
    /// </summary>
    public readonly struct EntityId : IEquatable<EntityId>
    {
        /// <summary>
        /// 无效实体标识符
        /// </summary>
        public static readonly EntityId Invalid = new EntityId(0, 0);

        /// <summary>
        /// 实体索引，用于在存储中定位实体
        /// </summary>
        public readonly uint Index;

        /// <summary>
        /// 版本号，用于确保引用有效性
        /// 当实体被销毁再复用时，版本号会递增
        /// </summary>
        public readonly uint Version;

        /// <summary>
        /// 创建新的实体ID
        /// </summary>
        public EntityId(uint index, uint version)
        {
            Index = index;
            Version = version;
        }

        /// <summary>
        /// 判断实体ID是否有效
        /// </summary>
        public bool IsValid => Index != 0 || Version != 0;

        /// <summary>
        /// 判断两个实体ID是否相等
        /// </summary>
        /// <param name="other">要比较的实体ID</param>
        /// <returns>如果两个实体ID相等，返回true；否则返回false</returns>
        public bool Equals(EntityId other)
        {
            return Index == other.Index && Version == other.Version;
        }

        /// <summary>
        /// 判断当前实体ID是否与另一个对象相等
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>如果当前实体ID与另一个对象相等，返回true；否则返回false</returns>  
        public override bool Equals(object? obj)
        {
            return obj is EntityId other && Equals(other);
        }

        /// <summary>
        /// 获取当前实体ID的哈希值
        /// </summary>
        /// <returns>当前实体ID的哈希值</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Index, Version);
        }

        /// <summary>
        /// 判断两个实体ID是否相等
        /// </summary>
        /// <param name="left">左边的实体ID</param>
        /// <param name="right">右边的实体ID</param>
        /// <returns>如果两个实体ID相等，返回true；否则返回false</returns>
        public static bool operator ==(EntityId left, EntityId right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 判断两个实体ID是否不相等
        /// </summary>
        /// <param name="left">左边的实体ID</param>
        /// <param name="right">右边的实体ID</param>
        /// <returns>如果两个实体ID不相等，返回true；否则返回false</returns>
        public static bool operator !=(EntityId left, EntityId right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 将实体ID转换为字符串表示
        /// </summary>
        /// <returns>实体ID的字符串表示</returns>
        public override string ToString()
        {
            return $"Entity(Index={Index}, Version={Version})";
        }
    }
} 