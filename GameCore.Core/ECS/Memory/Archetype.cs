using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.ECS.Components;
using GameCore.ECS.Core;

namespace GameCore.ECS.Memory
{
    /// <summary>
    /// 原型，表示具有相同组件组合的实体集合
    /// 使用原型可以优化内存布局和查询性能
    /// </summary>
    public class Archetype
    {
        // 原型唯一ID
        private readonly int _id;
        
        // 包含的组件类型
        private readonly Type[] _componentTypes;
        
        // 组件类型哈希集，用于快速查询
        private readonly HashSet<Type> _componentTypeSet;
        
        // 属于此原型的实体列表
        private readonly List<EntityId> _entities = new List<EntityId>();
        
        // 组件数据数组，按类型索引
        private readonly Dictionary<Type, Array> _componentArrays = new Dictionary<Type, Array>();
        
        // 实体到索引的映射
        private readonly Dictionary<EntityId, int> _entityToIndex = new Dictionary<EntityId, int>();
        
        // 实体数量
        private int _count;

        /// <summary>
        /// 原型ID
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// 包含的组件类型
        /// </summary>
        public IReadOnlyList<Type> ComponentTypes => _componentTypes;

        /// <summary>
        /// 实体数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 创建新的原型
        /// </summary>
        public Archetype(int id, IEnumerable<Type> componentTypes)
        {
            _id = id;
            
            // 确保组件类型有序，保证唯一性
            _componentTypes = componentTypes.OrderBy(t => t.FullName).ToArray();
            _componentTypeSet = new HashSet<Type>(_componentTypes);
            
            // 验证组件类型
            foreach (var type in _componentTypes)
            {
                if (!typeof(IComponent).IsAssignableFrom(type))
                {
                    throw new ArgumentException($"Type {type.Name} is not a valid component type");
                }
                
                // 初始化组件数组
                _componentArrays[type] = Array.CreateInstance(type, 64);
            }
        }

        /// <summary>
        /// 检查原型是否包含指定组件类型
        /// </summary>
        public bool HasComponent(Type componentType)
        {
            return _componentTypeSet.Contains(componentType);
        }

        /// <summary>
        /// 检查是否与指定组件查询匹配
        /// </summary>
        public bool Matches(IEnumerable<Type> withComponents, IEnumerable<Type> withoutComponents)
        {
            // 必须包含所有指定组件
            foreach (var type in withComponents)
            {
                if (!_componentTypeSet.Contains(type))
                {
                    return false;
                }
            }
            
            // 必须不包含指定组件
            foreach (var type in withoutComponents)
            {
                if (_componentTypeSet.Contains(type))
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 添加实体到原型
        /// </summary>
        public void AddEntity(EntityId entity, Dictionary<Type, object> components)
        {
            // 确保容量
            if (_count >= _componentArrays[_componentTypes[0]].Length)
            {
                GrowArrays();
            }
            
            // 记录实体
            _entities.Add(entity);
            _entityToIndex[entity] = _count;
            
            // 添加组件数据
            foreach (var type in _componentTypes)
            {
                if (components.TryGetValue(type, out var component))
                {
                    _componentArrays[type].SetValue(component, _count);
                }
                else
                {
                    // 使用默认值
                    _componentArrays[type].SetValue(Activator.CreateInstance(type), _count);
                }
            }
            
            _count++;
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        public bool RemoveEntity(EntityId entity)
        {
            if (!_entityToIndex.TryGetValue(entity, out int index))
            {
                return false;
            }
            
            int lastIndex = _count - 1;
            
            // 如果不是最后一个，将最后一个实体移到要删除的位置
            if (index < lastIndex)
            {
                EntityId lastEntity = _entities[lastIndex];
                
                // 更新最后一个实体的索引
                _entityToIndex[lastEntity] = index;
                _entities[index] = lastEntity;
                
                // 移动组件数据
                foreach (var type in _componentTypes)
                {
                    var array = _componentArrays[type];
                    array.SetValue(array.GetValue(lastIndex), index);
                }
            }
            
            // 移除实体
            _entityToIndex.Remove(entity);
            _entities.RemoveAt(lastIndex);
            _count--;
            
            return true;
        }

        /// <summary>
        /// 获取实体的组件
        /// </summary>
        public object GetComponent(EntityId entity, Type componentType)
        {
            if (!_componentTypeSet.Contains(componentType))
            {
                throw new ArgumentException($"Archetype does not contain component type {componentType.Name}");
            }
            
            if (!_entityToIndex.TryGetValue(entity, out int index))
            {
                throw new KeyNotFoundException($"Entity {entity} not found in archetype");
            }
            
            var value = _componentArrays[componentType].GetValue(index);
            if (value == null)
            {
                throw new InvalidOperationException($"Component value at index {index} is null");
            }
            return value;
        }

        /// <summary>
        /// 获取实体的所有组件
        /// </summary>
        public Dictionary<Type, object> GetComponents(EntityId entity)
        {
            if (!_entityToIndex.TryGetValue(entity, out int index))
            {
                throw new KeyNotFoundException($"Entity {entity} not found in archetype");
            }
            
            var result = new Dictionary<Type, object>();
            
            foreach (var type in _componentTypes)
            {
                var value = _componentArrays[type].GetValue(index);
                if (value == null)
                {
                    throw new InvalidOperationException($"Component value of type {type.Name} at index {index} is null");
                }
                result[type] = value;
            }
            
            return result;
        }

        /// <summary>
        /// 获取特定类型的所有组件
        /// </summary>
        public T[] GetComponentArray<T>() where T : IComponent
        {
            var type = typeof(T);
            
            if (!_componentTypeSet.Contains(type))
            {
                throw new ArgumentException($"Archetype does not contain component type {type.Name}");
            }
            
            // 创建新数组以避免修改内部状态
            var result = new T[_count];
            Array.Copy(_componentArrays[type], result, _count);
            return result;
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        public EntityId[] GetEntities()
        {
            var result = new EntityId[_count];
            _entities.CopyTo(0, result, 0, _count);
            return result;
        }

        /// <summary>
        /// 增长内部数组容量
        /// </summary>
        private void GrowArrays()
        {
            int currentCapacity = _componentArrays[_componentTypes[0]].Length;
            int newCapacity = currentCapacity * 2;
            
            foreach (var type in _componentTypes)
            {
                var newArray = Array.CreateInstance(type, newCapacity);
                Array.Copy(_componentArrays[type], newArray, currentCapacity);
                _componentArrays[type] = newArray;
            }
        }
    }
} 