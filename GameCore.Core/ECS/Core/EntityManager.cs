using System;
using System.Collections.Generic;
using GameCore.ECS.Components;

namespace GameCore.ECS.Core
{
    /// <summary>
    /// 实体管理器，负责创建、销毁和管理实体
    /// </summary>
    public class EntityManager
    {
        /// <summary>
        /// 最大实体数量
        /// </summary>
        private const int MAX_ENTITIES = 1_000_000;
        
        /// <summary>
        /// 实体版本号，用于处理实体重用
        /// </summary>
        private readonly uint[] _entityVersions = new uint[MAX_ENTITIES];
        
        /// <summary>
        /// 空闲实体ID队列，用于重用已销毁的实体ID
        /// </summary>
        private readonly Queue<uint> _freeEntities = new Queue<uint>();
        
        /// <summary>
        /// 实体计数器，用于生成下一个实体ID
        /// </summary>
        private uint _nextEntityId = 1; // 从1开始，0保留给无效实体
        
        /// <summary>
        /// 每种类型的组件存储，按组件类型索引
        /// </summary>
        private readonly Dictionary<Type, ComponentStore> _componentStores = new Dictionary<Type, ComponentStore>();
        
        /// <summary>
        /// 实体与组件类型的关联映射
        /// </summary>
        private readonly Dictionary<uint, HashSet<Type>> _entityComponents = new Dictionary<uint, HashSet<Type>>();

        /// <summary>
        /// 创建一个新实体
        /// </summary>
        public EntityId CreateEntity()
        {
            uint index;
            
            // 尝试复用已销毁的实体ID
            if (_freeEntities.Count > 0)
            {
                index = _freeEntities.Dequeue();
            }
            else
            {
                // 分配新的实体ID
                if (_nextEntityId >= MAX_ENTITIES)
                {
                    throw new InvalidOperationException($"Entity limit reached ({MAX_ENTITIES} entities)");
                }
                
                index = _nextEntityId++;
            }
            
            // 确保实体组件集合存在
            if (!_entityComponents.ContainsKey(index))
            {
                _entityComponents[index] = new HashSet<Type>();
            }
            
            return new EntityId(index, _entityVersions[index]);
        }

        /// <summary>
        /// 销毁实体及其所有组件
        /// </summary>
        public void DestroyEntity(EntityId entity)
        {
            if (!IsEntityAlive(entity))
            {
                return;
            }

            // 移除所有组件
            if (_entityComponents.TryGetValue(entity.Index, out var componentTypes))
            {
                foreach (Type type in componentTypes)
                {
                    if (_componentStores.TryGetValue(type, out var store))
                    {
                        store.Remove(entity);
                    }
                }
                
                componentTypes.Clear();
            }
            
            // 增加版本号，使旧引用失效
            _entityVersions[entity.Index]++;
            
            // 回收实体ID以便复用
            _freeEntities.Enqueue(entity.Index);
        }

        /// <summary>
        /// 判断实体是否存在且有效
        /// </summary>
        public bool IsEntityAlive(EntityId entity)
        {
            return entity.IsValid && 
                   entity.Index < _nextEntityId && 
                   _entityVersions[entity.Index] == entity.Version && 
                   !_freeEntities.Contains(entity.Index);
        }

        /// <summary>
        /// 添加组件到实体
        /// </summary>
        public void AddComponent<T>(EntityId entity, T component) where T : struct, IComponent
        {
            if (!IsEntityAlive(entity))
            {
                throw new ArgumentException($"Entity {entity} is not alive", nameof(entity));
            }

            // 获取或创建组件存储
            var store = GetComponentStore<T>();
            
            // 添加组件
            store.Add(entity, component);
            
            // 记录实体拥有此组件类型
            _entityComponents[entity.Index].Add(typeof(T));
        }

        /// <summary>
        /// 获取实体的组件
        /// </summary>
        public ref T GetComponent<T>(EntityId entity) where T : struct, IComponent
        {
            if (!IsEntityAlive(entity))
            {
                throw new ArgumentException($"Entity {entity} is not alive", nameof(entity));
            }

            var store = GetComponentStore<T>();
            return ref store.Get(entity);
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的组件
        /// </summary>
        public bool HasComponent<T>(EntityId entity) where T : struct, IComponent
        {
            if (!IsEntityAlive(entity))
            {
                return false;
            }

            if (!_componentStores.TryGetValue(typeof(T), out var store))
            {
                return false;
            }

            return store.Has(entity);
        }

        /// <summary>
        /// 移除实体的指定类型组件
        /// </summary>
        public void RemoveComponent<T>(EntityId entity) where T : struct, IComponent
        {
            if (!IsEntityAlive(entity))
            {
                return;
            }

            Type componentType = typeof(T);
            
            if (!_componentStores.TryGetValue(componentType, out var store))
            {
                return;
            }
            
            store.Remove(entity);
            _entityComponents[entity.Index].Remove(componentType);
        }

        /// <summary>
        /// 获取指定类型的组件存储
        /// 如果不存在则创建新的存储
        /// </summary>
        public ComponentStore<T> GetComponentStore<T>() where T : struct, IComponent
        {
            Type componentType = typeof(T);
            
            if (!_componentStores.TryGetValue(componentType, out var store))
            {
                store = new ComponentStore<T>();
                _componentStores[componentType] = store;
            }
            
            return (ComponentStore<T>)store;
        }

        /// <summary>
        /// 获取实体拥有的所有组件类型
        /// </summary>
        public IReadOnlyCollection<Type> GetEntityComponentTypes(EntityId entity)
        {
            if (!IsEntityAlive(entity) || !_entityComponents.TryGetValue(entity.Index, out var types))
            {
                return Array.Empty<Type>();
            }
            
            return types;
        }

        /// <summary>
        /// 清空所有实体和组件数据
        /// </summary>
        public void Clear()
        {
            _nextEntityId = 1;
            _freeEntities.Clear();
            _entityComponents.Clear();
            
            // 清空所有版本号
            Array.Clear(_entityVersions, 0, _entityVersions.Length);
            
            // 清空所有组件存储
            foreach (var store in _componentStores.Values)
            {
                store.Clear();
            }
        }
    }
} 