using System;
using System.Collections.Generic;
using GameCore.ECS.Components;

namespace GameCore.ECS.Core
{
    /// <summary>
    /// 组件存储基类，提供组件的抽象存储和管理功能
    /// </summary>
    public abstract class ComponentStore
    {
        /// <summary>
        /// 获取该存储管理的组件类型
        /// </summary>
        public abstract Type ComponentType { get; }

        /// <summary>
        /// 从存储中移除指定实体的组件
        /// </summary>
        public abstract void Remove(EntityId entity);

        /// <summary>
        /// 检查存储中是否存在指定实体的组件
        /// </summary>
        public abstract bool Has(EntityId entity);

        /// <summary>
        /// 清空所有组件数据
        /// </summary>
        public abstract void Clear();
    }

    /// <summary>
    /// 泛型组件存储，管理特定类型组件的数据
    /// 优化内存布局以提高性能
    /// </summary>
    public class ComponentStore<T> : ComponentStore where T : struct, IComponent
    {
        // 实体ID到组件数组索引的映射
        private readonly Dictionary<uint, int> _entityToIndex = new Dictionary<uint, int>();
        
        // 版本号映射，用于验证实体有效性
        private readonly Dictionary<uint, uint> _entityVersions = new Dictionary<uint, uint>();
        
        // 组件数据数组，连续存储以优化缓存命中率
        private T[] _components = new T[64];
        
        // 实体ID数组，与组件数组平行存储
        private EntityId[] _entities = new EntityId[64];
        
        // 当前使用的组件数量
        private int _count = 0;

        /// <summary>
        /// 获取该存储管理的组件类型
        /// </summary>
        public override Type ComponentType => typeof(T);

        /// <summary>
        /// 添加或更新实体的组件
        /// </summary>
        public void Add(EntityId entity, in T component)
        {
            if (!entity.IsValid)
            {
                throw new ArgumentException("Cannot add component to invalid entity", nameof(entity));
            }

            // 如果已存在则更新
            if (_entityToIndex.TryGetValue(entity.Index, out int index))
            {
                // 验证版本号
                if (_entityVersions[entity.Index] != entity.Version)
                {
                    throw new InvalidOperationException($"Entity version mismatch: expected {_entityVersions[entity.Index]}, got {entity.Version}");
                }

                _components[index] = component;
                return;
            }

            // 容量检查
            EnsureCapacity(_count + 1);

            // 添加新组件
            _components[_count] = component;
            _entities[_count] = entity;
            _entityToIndex[entity.Index] = _count;
            _entityVersions[entity.Index] = entity.Version;
            _count++;
        }

        /// <summary>
        /// 获取实体的组件
        /// </summary>
        public ref T Get(EntityId entity)
        {
            if (!Has(entity))
            {
                throw new KeyNotFoundException($"Entity {entity} does not have component {typeof(T).Name}");
            }

            int index = _entityToIndex[entity.Index];
            return ref _components[index];
        }

        /// <summary>
        /// 检查实体是否拥有此类型的组件
        /// </summary>
        public override bool Has(EntityId entity)
        {
            return entity.IsValid && 
                   _entityToIndex.TryGetValue(entity.Index, out _) && 
                   _entityVersions.TryGetValue(entity.Index, out uint version) && 
                   version == entity.Version;
        }

        /// <summary>
        /// 移除实体的组件
        /// </summary>
        public override void Remove(EntityId entity)
        {
            if (!Has(entity))
            {
                return;
            }

            int indexToRemove = _entityToIndex[entity.Index];
            int lastIndex = _count - 1;

            // 如果不是最后一个元素，将最后一个元素移到被删除的位置
            if (indexToRemove < lastIndex)
            {
                _components[indexToRemove] = _components[lastIndex];
                _entities[indexToRemove] = _entities[lastIndex];
                _entityToIndex[_entities[lastIndex].Index] = indexToRemove;
            }

            // 移除映射
            _entityToIndex.Remove(entity.Index);
            _entityVersions.Remove(entity.Index);
            _count--;
        }

        /// <summary>
        /// 清空所有组件数据
        /// </summary>
        public override void Clear()
        {
            _entityToIndex.Clear();
            _entityVersions.Clear();
            _count = 0;
        }

        /// <summary>
        /// 获取原始组件数据Span，用于高效批量处理
        /// </summary>
        public Span<T> GetRawData()
        {
            return new Span<T>(_components, 0, _count);
        }

        /// <summary>
        /// 获取实体ID Span，与组件数据平行存储
        /// </summary>
        public Span<EntityId> GetEntities()
        {
            return new Span<EntityId>(_entities, 0, _count);
        }

        /// <summary>
        /// 确保数组容量足够
        /// </summary>
        private void EnsureCapacity(int capacity)
        {
            if (capacity <= _components.Length)
            {
                return;
            }

            int newCapacity = Math.Max(_components.Length * 2, capacity);
            Array.Resize(ref _components, newCapacity);
            Array.Resize(ref _entities, newCapacity);
        }

        /// <summary>
        /// 获取组件数量
        /// </summary>
        public int Count => _count;
    }
} 