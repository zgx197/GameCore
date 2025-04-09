using System;
using GameCore.ECS.Core;

namespace GameCore.ECS.Systems
{
    /// <summary>
    /// 系统基类，提供通用功能和依赖注入
    /// </summary>
    public abstract class SystemBase : ISystem
    {
        /// <summary>
        /// 系统所在的世界实例
        /// </summary>
        protected World? World { get; private set; }
        
        /// <summary>
        /// 系统的启用状态
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 系统初始化
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// 系统执行，子类必须实现此方法
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// 系统清理
        /// </summary>
        public virtual void Cleanup() { }

        /// <summary>
        /// 设置系统所在的世界
        /// </summary>
        internal void SetWorld(World world)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>
        /// 创建实体查询
        /// </summary>
        /// <param name="withComponents">必须包含的组件类型</param>
        /// <param name="withoutComponents">必须不包含的组件类型</param>
        /// <returns>新的查询实例</returns>
        protected Query CreateQuery(Type[] withComponents, Type[]? withoutComponents = null)
        {
            return World!.CreateQuery(withComponents, withoutComponents);
        }
    }
} 