using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.ECS.Components;
using GameCore.ECS.Core;

namespace GameCore.ECS.Systems
{
    /// <summary>
    /// 实体查询系统，用于筛选具有特定组件组合的实体
    /// </summary>
    public class Query
    {
        private readonly World _world;
        
        // 必须包含的组件类型
        private readonly Type[] _withComponents;
        
        // 必须不包含的组件类型
        private readonly Type[] _withoutComponents;
        
        // 缓存的查询结果
        private List<EntityId> _cachedEntities = new List<EntityId>();
        
        // 缓存是否有效
        private bool _isCacheDirty = true;

        /// <summary>
        /// 创建新的查询
        /// </summary>
        /// <param name="world">ECS世界实例</param>
        /// <param name="withComponents">必须包含的组件类型</param>
        /// <param name="withoutComponents">必须不包含的组件类型</param>
        internal Query(World world, Type[] withComponents, Type[]? withoutComponents)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _withComponents = withComponents ?? Array.Empty<Type>();
            _withoutComponents = withoutComponents ?? Array.Empty<Type>();
            
            // 验证组件类型
            foreach (var type in _withComponents.Concat(_withoutComponents))
            {
                if (!IsComponentType(type))
                {
                    throw new ArgumentException($"Type {type.Name} is not a valid component type");
                }
            }
        }

        /// <summary>
        /// 刷新缓存，重新查找匹配的实体
        /// </summary>
        public void Refresh()
        {
            _isCacheDirty = true;
        }

        /// <summary>
        /// 获取匹配查询条件的所有实体
        /// </summary>
        public IReadOnlyList<EntityId> GetMatchingEntities()
        {
            // 如果缓存有效，直接返回
            if (!_isCacheDirty)
            {
                return _cachedEntities;
            }
            
            // 清空缓存并重新查询
            _cachedEntities.Clear();
            
            // 如果没有过滤条件，则不可能有匹配的实体
            if (_withComponents.Length == 0)
            {
                _isCacheDirty = false;
                return _cachedEntities;
            }
            
            // 获取第一个必需组件的所有实体，作为基础集合
            var firstType = _withComponents[0];
            var firstStore = _world.GetComponentStore(firstType);
            if (firstStore == null)
            {
                _isCacheDirty = false;
                return _cachedEntities;
            }
            
            // 收集第一个组件的所有实体
            var getEntitiesMethod = firstType.GetMethod("GetEntities") ?? 
                throw new InvalidOperationException($"Type {firstType.Name} does not have GetEntities method");
            var baseEntities = getEntitiesMethod.Invoke(firstStore, null) as IEnumerable<EntityId>;
            
            if (baseEntities == null)
            {
                _isCacheDirty = false;
                return _cachedEntities;
            }
            
            // 筛选符合所有条件的实体
            foreach (var entity in baseEntities)
            {
                bool matches = true;
                
                // 检查必须包含的组件
                for (int i = 1; i < _withComponents.Length; i++)
                {
                    if (!_world.HasComponent(entity, _withComponents[i]))
                    {
                        matches = false;
                        break;
                    }
                }
                
                // 检查必须不包含的组件
                if (matches)
                {
                    foreach (var type in _withoutComponents)
                    {
                        if (_world.HasComponent(entity, type))
                        {
                            matches = false;
                            break;
                        }
                    }
                }
                
                // 如果匹配所有条件，添加到结果集
                if (matches)
                {
                    _cachedEntities.Add(entity);
                }
            }
            
            _isCacheDirty = false;
            return _cachedEntities;
        }

        /// <summary>
        /// 获取匹配查询的实体拥有的指定类型组件数据
        /// </summary>
        public Span<T> GetComponentData<T>() where T : struct, IComponent
        {
            var store = _world.GetComponentStore<T>();
            if (store == null)
            {
                return Span<T>.Empty;
            }
            
            // 确保匹配实体列表已更新
            GetMatchingEntities();
            
            // 如果没有匹配实体，返回空集合
            if (_cachedEntities.Count == 0)
            {
                return Span<T>.Empty;
            }
            
            // 获取组件数据
            // 注意：这假设ComponentStore为每个匹配的实体保存组件
            // 对于不连续的组件数据，需要更复杂的处理
            return store.GetRawData();
        }

        /// <summary>
        /// 检查类型是否为有效的组件类型
        /// </summary>
        private bool IsComponentType(Type type)
        {
            return typeof(IComponent).IsAssignableFrom(type);
        }
    }
} 