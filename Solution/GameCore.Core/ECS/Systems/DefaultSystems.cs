using System;
using GameCore.ECS.Components;
using GameCore.ECS.Core;

namespace GameCore.ECS.Systems
{
    /// <summary>
    /// 运动系统，更新具有变换和速度组件的实体位置
    /// </summary>
    public class MovementSystem : SystemBase
    {
        /// <summary>
        /// 查询具有变换和速度组件的实体
        /// </summary>
        public Query? _query;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MovementSystem()
        {
            _query = CreateQuery(new[] { typeof(TransformComponent), typeof(VelocityComponent) });
        }

        /// <summary>
        /// 执行系统
        /// </summary>
        public override void Execute()
        {
            // 获取匹配实体
            var entities = _query?.GetMatchingEntities() ?? Array.Empty<EntityId>();
            
            // 跳过如果没有实体
            if (entities.Count == 0)
            {
                return;
            }
            
            // 获取当前帧的时间增量
            float deltaTime = World?.Time.DeltaTime ?? 0f;
            
            // 更新每个实体的位置
            foreach (var entity in entities)
            {
                // 获取组件引用
                ref var transform = ref World!.GetComponent<TransformComponent>(entity);
                ref var velocity = ref World!.GetComponent<VelocityComponent>(entity);
                
                // 更新位置
                transform.Position += new System.Numerics.Vector3(
                    velocity.Linear.X * deltaTime,
                    velocity.Linear.Y * deltaTime,
                    velocity.Linear.Z * deltaTime
                );
                
                // 应用角速度（简化版，实际应该使用四元数计算）
                transform.Rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(
                    velocity.Angular.Y * deltaTime,
                    velocity.Angular.X * deltaTime,
                    velocity.Angular.Z * deltaTime
                ) * transform.Rotation;
            }
        }
    }

    /// <summary>
    /// 生命周期系统，管理实体的生命周期
    /// </summary>
    public class LifetimeSystem : SystemBase
    {
        // 查询具有生命周期组件的实体
        private Query? _query;

        /// <summary>
        /// 初始化系统
        /// </summary>
        public override void Initialize()
        {
            // 创建查询，筛选具有LifetimeComponent的实体
            _query = CreateQuery(new[] { typeof(LifetimeComponent) });
        }

        /// <summary>
        /// 执行系统
        /// </summary>
        public override void Execute()
        {
            // 获取匹配实体
            var entities = _query?.GetMatchingEntities() ?? Array.Empty<EntityId>();
            
            // 跳过如果没有实体
            if (entities.Count == 0)
            {
                return;
            }
            
            // 获取当前帧的时间增量
            float deltaTime = World?.Time.DeltaTime ?? 0f;
            
            // 更新每个实体的生命周期
            foreach (var entity in entities)
            {
                // 获取生命周期组件引用
                ref var lifetime = ref World!.GetComponent<LifetimeComponent>(entity);
                
                // 减少剩余时间
                lifetime.RemainingTime -= deltaTime;
                
                // 检查生命周期是否结束
                if (lifetime.RemainingTime <= 0)
                {
                    if (lifetime.DestroyOnExpire)
                    {
                        // 销毁实体
                        World!.DestroyEntity(entity);
                    }
                    else
                    {
                        // 移除生命周期组件
                        World!.RemoveComponent<LifetimeComponent>(entity);
                    }
                }
            }
        }
    }
} 