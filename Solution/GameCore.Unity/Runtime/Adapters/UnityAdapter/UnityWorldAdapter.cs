#if UNITY
using UnityEngine;
using GameCore.ECS.Adapters;
using GameCore.ECS.Core;
using System;

namespace GameCore.Unity.Adapters
{
    /// <summary>
    /// Unity世界适配器，连接Unity和GameCore ECS
    /// </summary>
    public class UnityWorldAdapter : MonoBehaviour, IEngineAdapter
    {
        /// <summary>
        /// ECS世界实例
        /// </summary>
        private World? _ecsWorld;
        
        /// <summary>
        /// 实体映射字典，Unity Instance ID到ECS实体ID
        /// </summary>
        private System.Collections.Generic.Dictionary<int, EntityId> _entityMap = 
            new System.Collections.Generic.Dictionary<int, EntityId>();

        /// <summary>
        /// Unity Awake生命周期方法
        /// </summary>
        void Awake()
        {
            // 使用共享的GameCore世界实例
            if (GameCore.World == null)
            {
                GameCore.Initialize();
            }
            
            _ecsWorld = GameCore.World ?? throw new InvalidOperationException("Failed to initialize GameCore World");
            
            // 初始化适配器
            Initialize(_ecsWorld);
        }

        /// <summary>
        /// Unity Update生命周期方法
        /// </summary>
        public void Update()
        {
            // 同步Unity数据到ECS
            SyncToECS();
            
            // 更新ECS世界
            GameCore.Update(UnityEngine.Time.deltaTime);
            
            // 同步ECS数据到Unity
            SyncFromECS();
        }

        /// <summary>
        /// Unity OnDestroy生命周期方法
        /// </summary>
        void OnDestroy()
        {
            Shutdown();
        }

        /// <summary>
        /// 初始化适配器
        /// </summary>
        public void Initialize(World world)
        {
            _ecsWorld = world ?? throw new System.ArgumentNullException(nameof(world));
            
            // 查找场景中的所有EntityBehaviour
            var entityBehaviours = UnityEngine.Object.FindObjectsOfType<EntityBehaviour>();
            
            // 为每个实体行为创建ECS实体
            foreach (var behaviour in entityBehaviours)
            {
                RegisterEntityBehaviour(behaviour);
            }
        }

        /// <summary>
        /// 注册实体行为，创建对应的ECS实体
        /// </summary>
        public void RegisterEntityBehaviour(EntityBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return;
            }
            
            int instanceId = behaviour.gameObject.GetInstanceID();
            
            // 如果已注册，直接返回
            if (_entityMap.ContainsKey(instanceId))
            {
                return;
            }
            
            // 创建ECS实体
            EntityId entity = _ecsWorld!.CreateEntity();
            
            // 记录映射
            _entityMap[instanceId] = entity;
            
            // 设置实体行为的实体ID
            behaviour.EntityId = entity;
            
            // 同步初始组件
            behaviour.SyncComponentsToECS(_ecsWorld!);
        }

        /// <summary>
        /// 注销实体行为，销毁对应的ECS实体
        /// </summary>
        public void UnregisterEntityBehaviour(EntityBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return;
            }
            
            int instanceId = behaviour.gameObject.GetInstanceID();
            
            // 如果未注册，直接返回
            if (!_entityMap.TryGetValue(instanceId, out EntityId entity))
            {
                return;
            }
            
            // 销毁ECS实体
            _ecsWorld!.DestroyEntity(entity);
            
            // 移除映射
            _entityMap.Remove(instanceId);
            
            // 清除实体行为的实体ID
            behaviour.EntityId = EntityId.Invalid;
        }

        /// <summary>
        /// 将Unity数据同步到ECS
        /// </summary>
        public void SyncToECS()
        {
            // 查找所有实体行为
            var entityBehaviours = UnityEngine.Object.FindObjectsOfType<EntityBehaviour>();
            
            foreach (var behaviour in entityBehaviours)
            {
                // 如果还未注册，注册新实体
                if (!behaviour.HasValidEntity)
                {
                    RegisterEntityBehaviour(behaviour);
                }
                
                // 同步组件数据
                behaviour.SyncComponentsToECS(_ecsWorld!);
            }
        }

        /// <summary>
        /// 将ECS数据同步到Unity
        /// </summary>
        public void SyncFromECS()
        {
            // 查找所有实体行为
            var entityBehaviours = UnityEngine.Object.FindObjectsOfType<EntityBehaviour>();
            
            foreach (var behaviour in entityBehaviours)
            {
                // 如果有有效实体，同步组件数据
                if (behaviour.HasValidEntity)
                {
                    behaviour.SyncComponentsFromECS(_ecsWorld!);
                }
            }
        }

        /// <summary>
        /// 关闭适配器
        /// </summary>
        public void Shutdown()
        {
            // 注销所有实体行为
            foreach (var entry in _entityMap)
            {
                _ecsWorld!.DestroyEntity(entry.Value);
            }
            
            _entityMap.Clear();
        }
    }
}
#endif 