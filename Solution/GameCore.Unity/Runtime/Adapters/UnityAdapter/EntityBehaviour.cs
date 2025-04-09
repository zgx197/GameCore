using UnityEngine;
using GameCore.ECS.Core;
using GameCore.ECS.Components;
using System;

namespace GameCore.Unity.Adapters
{
    /// <summary>
    /// 实体行为组件，将Unity游戏对象与ECS实体关联
    /// </summary>
    public class EntityBehaviour : MonoBehaviour
    {
        // 关联的ECS实体ID
        [SerializeField, HideInInspector]
        private uint _entityIndex;
        
        [SerializeField, HideInInspector]
        private uint _entityVersion;
        
        // 自动同步变换
        [SerializeField]
        private bool _syncTransform = true;

        /// <summary>
        /// 关联的ECS实体ID
        /// </summary>
        public EntityId EntityId
        {
            get => new EntityId(_entityIndex, _entityVersion);
            internal set
            {
                _entityIndex = value.Index;
                _entityVersion = value.Version;
            }
        }

        /// <summary>
        /// 是否拥有有效的ECS实体
        /// </summary>
        public bool HasValidEntity => EntityId.IsValid && 
                                     (GameCore.World?.IsEntityAlive(EntityId) ?? false);

        /// <summary>
        /// Unity Awake生命周期方法
        /// </summary>
        protected virtual void Awake()
        {
            // 在Awake时不创建实体，由适配器统一管理
        }

        /// <summary>
        /// Unity OnDestroy生命周期方法
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 尝试找到适配器
            var adapter = FindObjectOfType<UnityWorldAdapter>();
            if (adapter != null && HasValidEntity)
            {
                adapter.UnregisterEntityBehaviour(this);
            }
        }

        /// <summary>
        /// 将组件数据同步到ECS
        /// </summary>
        public virtual void SyncComponentsToECS(World world)
        {
            if (!HasValidEntity || world == null)
            {
                return;
            }

            // 同步变换
            if (_syncTransform)
            {
                var transform = this.transform;
                
                var transformComponent = new TransformComponent
                {
                    Position = new System.Numerics.Vector3(
                        transform.position.x,
                        transform.position.y,
                        transform.position.z
                    ),
                    Rotation = new System.Numerics.Quaternion(
                        transform.rotation.x,
                        transform.rotation.y,
                        transform.rotation.z,
                        transform.rotation.w
                    ),
                    Scale = new System.Numerics.Vector3(
                        transform.localScale.x,
                        transform.localScale.y,
                        transform.localScale.z
                    )
                };
                
                world.AddComponent(EntityId, transformComponent);
            }
        }

        /// <summary>
        /// 将ECS数据同步到组件
        /// </summary>
        public virtual void SyncComponentsFromECS(World world)
        {
            if (!HasValidEntity || world == null)
            {
                return;
            }

            // 同步变换
            if (_syncTransform && world.HasComponent<TransformComponent>(EntityId))
            {
                var transformComponent = world.GetComponent<TransformComponent>(EntityId);
                
                var transform = this.transform;
                
                // 仅当数据发生变化时才更新，避免不必要的Unity变换操作
                var position = new Vector3(
                    transformComponent.Position.X,
                    transformComponent.Position.Y,
                    transformComponent.Position.Z
                );
                
                var rotation = new Quaternion(
                    transformComponent.Rotation.X,
                    transformComponent.Rotation.Y,
                    transformComponent.Rotation.Z,
                    transformComponent.Rotation.W
                );
                
                var scale = new Vector3(
                    transformComponent.Scale.X,
                    transformComponent.Scale.Y,
                    transformComponent.Scale.Z
                );
                
                if (Vector3.Distance(transform.position, position) > 0.001f)
                {
                    transform.position = position;
                }
                
                if (Quaternion.Angle(transform.rotation, rotation) > 0.1f)
                {
                    transform.rotation = rotation;
                }
                
                if (Vector3.Distance(transform.localScale, scale) > 0.001f)
                {
                    transform.localScale = scale;
                }
            }
        }

        /// <summary>
        /// 添加ECS组件到关联实体
        /// </summary>
        public void AddComponent<T>(T component) where T : struct, IComponent
        {
            if (!HasValidEntity)
            {
                Debug.LogWarning("Cannot add component to invalid entity");
                return;
            }
            
            GameCore.World.AddComponent(EntityId, component);
        }

        /// <summary>
        /// 获取关联实体的ECS组件
        /// </summary>
        public T GetECSComponent<T>() where T : struct, IComponent
        {
            if (!HasValidEntity)
            {
                throw new InvalidOperationException("Cannot get component from invalid entity");
            }
            
            if (GameCore.World == null)
            {
                throw new InvalidOperationException("GameCore.World is null");
            }
            
            if (!GameCore.World.HasComponent<T>(EntityId))
            {
                throw new InvalidOperationException($"Entity does not have component of type {typeof(T).Name}");
            }
            
            return GameCore.World.GetComponent<T>(EntityId);
        }

        /// <summary>
        /// 检查关联实体是否拥有ECS组件
        /// </summary>
        public bool HasComponent<T>() where T : struct, IComponent
        {
            if (GameCore.World == null)
            {
                return false;
            }
            return HasValidEntity && GameCore.World.HasComponent<T>(EntityId);
        }

        /// <summary>
        /// 移除关联实体的ECS组件
        /// </summary>
        public void RemoveComponent<T>() where T : struct, IComponent
        {
            if (!HasValidEntity)
            {
                return;
            }
            
            if (GameCore.World == null)
            {
                Debug.LogWarning("Cannot remove component: GameCore.World is null");
                return;
            }
            
            GameCore.World.RemoveComponent<T>(EntityId);
        }
    }
} 