using System;
using System.Numerics;

namespace GameCore.ECS.Components
{
    /// <summary>
    /// 变换组件，存储实体的位置、旋转和缩放
    /// </summary>
    public struct TransformComponent : IComponent
    {
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// 旋转（四元数）
        /// </summary>
        public Quaternion Rotation;
        
        /// <summary>
        /// 缩放
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// 创建默认变换组件
        /// </summary>
        public static TransformComponent Default => new TransformComponent
        {
            Position = Vector3.Zero,
            Rotation = Quaternion.Identity,
            Scale = Vector3.One
        };
    }

    /// <summary>
    /// 速度组件，用于运动系统
    /// </summary>
    public struct VelocityComponent : IComponent
    {
        /// <summary>
        /// 线性速度
        /// </summary>
        public Vector3 Linear;
        
        /// <summary>
        /// 角速度
        /// </summary>
        public Vector3 Angular;
    }

    /// <summary>
    /// 标记组件，标识实体的特殊状态或类型
    /// </summary>
    public struct TagComponent : IComponent
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public string Name;
    }

    /// <summary>
    /// 生命周期组件，管理实体的生命周期
    /// </summary>
    public struct LifetimeComponent : IComponent
    {
        /// <summary>
        /// 剩余生命时间（秒）
        /// </summary>
        public float RemainingTime;
        
        /// <summary>
        /// 生命周期结束时是否自动销毁实体
        /// </summary>
        public bool DestroyOnExpire;
    }

    /// <summary>
    /// 层级组件，定义实体间的父子关系
    /// </summary>
    public struct HierarchyComponent : IComponent
    {
        /// <summary>
        /// 父实体ID
        /// </summary>
        public Core.EntityId Parent;
        
        /// <summary>
        /// 子实体ID列表
        /// </summary>
        public Core.EntityId[] Children;
        
        /// <summary>
        /// 局部变换（相对于父实体）
        /// </summary>
        public TransformComponent LocalTransform;
    }

    /// <summary>
    /// 用户数据组件，存储任意用户定义的数据
    /// </summary>
    public struct UserDataComponent : IComponent
    {
        /// <summary>
        /// 用户数据对象
        /// </summary>
        public object Data;
        
        /// <summary>
        /// 数据类型
        /// </summary>
        public Type DataType;
    }
} 