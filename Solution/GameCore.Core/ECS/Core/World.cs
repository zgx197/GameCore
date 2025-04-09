using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.ECS.Components;
using GameCore.ECS.Systems;

namespace GameCore.ECS.Core
{
    /// <summary>
    /// ECS世界，作为实体、组件和系统的主容器
    /// </summary>
    public class World
    {
        // 实体管理器
        private readonly EntityManager _entityManager;
        
        // 注册的系统列表
        private readonly List<ISystem> _systems = new List<ISystem>();
        
        // 是否已初始化
        private bool _isInitialized = false;
        
        // 世界是否正在运行
        private bool _isRunning = false;
        
        // 共享的时间信息
        private readonly GameTime _gameTime = new GameTime();

        /// <summary>
        /// 当前游戏时间信息
        /// </summary>
        public GameTime Time => _gameTime;

        /// <summary>
        /// 创建新的ECS世界
        /// </summary>
        public World()
        {
            _entityManager = new EntityManager();
        }

        /// <summary>
        /// 创建新实体
        /// </summary>
        public EntityId CreateEntity()
        {
            return _entityManager.CreateEntity();
        }

        /// <summary>
        /// 销毁实体及其所有组件
        /// </summary>
        public void DestroyEntity(EntityId entity)
        {
            _entityManager.DestroyEntity(entity);
        }

        /// <summary>
        /// 判断实体是否存在且有效
        /// </summary>
        public bool IsEntityAlive(EntityId entity)
        {
            return _entityManager.IsEntityAlive(entity);
        }

        /// <summary>
        /// 添加组件到实体
        /// </summary>
        public void AddComponent<T>(EntityId entity, T component) where T : struct, IComponent
        {
            _entityManager.AddComponent(entity, component);
        }

        /// <summary>
        /// 获取实体的组件
        /// </summary>
        public ref T GetComponent<T>(EntityId entity) where T : struct, IComponent
        {
            return ref _entityManager.GetComponent<T>(entity);
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的组件
        /// </summary>
        public bool HasComponent<T>(EntityId entity) where T : struct, IComponent
        {
            return _entityManager.HasComponent<T>(entity);
        }

        /// <summary>
        /// 检查实体是否拥有指定类型的组件
        /// </summary>
        public bool HasComponent(EntityId entity, Type componentType)
        {
            var method = typeof(EntityManager).GetMethod("HasComponent");
            if (method == null)
            {
                throw new InvalidOperationException($"Method 'HasComponent' not found in EntityManager");
            }
            
            var genericMethod = method.MakeGenericMethod(componentType);
            var result = genericMethod.Invoke(_entityManager, new object[] { entity });
            if (result == null)
            {
                throw new InvalidOperationException($"Method 'HasComponent' returned null");
            }
            
            return (bool)result;
        }

        /// <summary>
        /// 移除实体的指定类型组件
        /// </summary>
        public void RemoveComponent<T>(EntityId entity) where T : struct, IComponent
        {
            _entityManager.RemoveComponent<T>(entity);
        }

        /// <summary>
        /// 获取指定类型的组件存储
        /// </summary>
        public ComponentStore<T> GetComponentStore<T>() where T : struct, IComponent
        {
            return _entityManager.GetComponentStore<T>();
        }

        /// <summary>
        /// 获取指定类型的组件存储
        /// </summary>
        internal ComponentStore GetComponentStore(Type componentType)
        {
            var method = typeof(EntityManager).GetMethod("GetComponentStore");
            if (method == null)
            {
                throw new InvalidOperationException($"Method 'GetComponentStore' not found in EntityManager");
            }
            
            var genericMethod = method.MakeGenericMethod(componentType);
            var result = genericMethod.Invoke(_entityManager, null);
            if (result == null)
            {
                throw new InvalidOperationException($"Method 'GetComponentStore' returned null");
            }
            
            return (ComponentStore)result;
        }

        /// <summary>
        /// 注册系统到世界
        /// </summary>
        public void RegisterSystem(ISystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (_systems.Contains(system))
            {
                return;
            }

            _systems.Add(system);

            // 如果世界已初始化，则立即初始化系统
            if (_isInitialized)
            {
                if (system is SystemBase baseSystem)
                {
                    baseSystem.SetWorld(this);
                }
                system.Initialize();
            }
        }

        /// <summary>
        /// 移除系统
        /// </summary>
        public bool RemoveSystem(ISystem system)
        {
            if (system == null)
            {
                return false;
            }

            bool removed = _systems.Remove(system);
            if (removed && _isInitialized)
            {
                system.Cleanup();
            }

            return removed;
        }

        /// <summary>
        /// 创建实体查询
        /// </summary>
        internal Query CreateQuery(Type[] withComponents, Type[]? withoutComponents = null)
        {
            return new Query(this, withComponents, withoutComponents);
        }

        /// <summary>
        /// 初始化世界和所有已注册的系统
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            // 初始化所有系统
            foreach (var system in _systems)
            {
                if (system is SystemBase baseSystem)
                {
                    baseSystem.SetWorld(this);
                }
                system.Initialize();
            }

            _isInitialized = true;
            _isRunning = true;
        }

        /// <summary>
        /// 更新世界和所有已注册的系统
        /// </summary>
        /// <param name="deltaTime">自上次更新以来的时间（秒）</param>
        public void Update(float deltaTime = 0.016f)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (!_isRunning)
            {
                return;
            }

            // 更新时间信息
            _gameTime.Update(deltaTime);

            // 执行所有启用的系统
            foreach (var system in _systems)
            {
                if (system is SystemBase baseSystem && !baseSystem.Enabled)
                {
                    continue;
                }

                system.Execute();
            }
        }

        /// <summary>
        /// 清理世界和所有系统
        /// </summary>
        public void Cleanup()
        {
            if (!_isInitialized)
            {
                return;
            }

            _isRunning = false;

            // 清理所有系统
            foreach (var system in _systems)
            {
                system.Cleanup();
            }

            // 清空系统列表
            _systems.Clear();

            // 清空实体管理器
            _entityManager.Clear();

            _isInitialized = false;
        }
    }

    /// <summary>
    /// 游戏时间信息
    /// </summary>
    public class GameTime
    {
        /// <summary>
        /// 自上次更新以来的时间（秒）
        /// </summary>
        public float DeltaTime { get; private set; } = 0.016f;

        /// <summary>
        /// 自游戏开始以来的总时间（秒）
        /// </summary>
        public float TotalTime { get; private set; } = 0f;

        /// <summary>
        /// 自游戏开始以来的帧数
        /// </summary>
        public ulong FrameCount { get; private set; } = 0;

        /// <summary>
        /// 更新时间信息
        /// </summary>
        internal void Update(float deltaTime)
        {
            DeltaTime = deltaTime;
            TotalTime += deltaTime;
            FrameCount++;
        }
    }
} 