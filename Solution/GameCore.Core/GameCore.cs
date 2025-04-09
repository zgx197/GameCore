using GameCore.ECS.Core;
using GameCore.ECS.Events;
using GameCore.ECS.Jobs;
using GameCore.ECS.Systems;

namespace GameCore
{
    /// <summary>
    /// GameCore框架主入口点和配置类
    /// </summary>
    public static class GameCore
    {
        /// <summary>
        /// 框架版本
        /// </summary>
        public static readonly string Version = typeof(GameCore).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        /// <summary>
        /// ECS世界实例
        /// </summary>
        public static World? World { get; private set; }

        /// <summary>
        /// 事件系统
        /// </summary>
        public static EventSystem? Events { get; private set; }

        /// <summary>
        /// 任务系统
        /// </summary>
        public static JobSystem? Jobs { get; private set; }

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public static bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 是否注册默认系统，默认为true，可用于测试
        /// </summary>
        private static bool _registerDefaultSystems = true;

        /// <summary>
        /// 初始化GameCore框架
        /// </summary>
        public static void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            // 创建ECS世界
            World = new World();
            
            // 创建事件系统
            Events = new EventSystem(World);
            
            // 创建任务系统
            Jobs = new JobSystem();
            
            // 注册默认系统
            RegisterDefaultSystems();
            
            // 初始化ECS世界
            World.Initialize();
            
            IsInitialized = true;
        }

        /// <summary>
        /// 注册默认系统
        /// </summary>
        private static void RegisterDefaultSystems()
        {
            if (!_registerDefaultSystems)
            {
                return;
            }
            
            // 注册运动系统
            World?.RegisterSystem(new MovementSystem());
            
            // 注册生命周期系统
            World?.RegisterSystem(new LifetimeSystem());
        }

        /// <summary>
        /// 更新GameCore框架
        /// </summary>
        /// <param name="deltaTime">时间增量（秒）</param>
        public static void Update(float deltaTime = 0.016f)
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            // 更新ECS世界
            World?.Update(deltaTime);
            
            // 处理事件
            Events?.ProcessEvents();
        }

        /// <summary>
        /// 关闭GameCore框架并释放资源
        /// </summary>
        public static void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            // 等待所有任务完成
            Jobs?.CompleteAll();
            
            // 清理ECS世界
            World?.Cleanup();
            
            IsInitialized = false;
        }
    }
}