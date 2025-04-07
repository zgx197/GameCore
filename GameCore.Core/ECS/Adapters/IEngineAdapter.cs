using GameCore.ECS.Core;

namespace GameCore.ECS.Adapters
{
    /// <summary>
    /// 游戏引擎适配器接口，定义ECS与游戏引擎集成的契约
    /// </summary>
    public interface IEngineAdapter
    {
        /// <summary>
        /// 初始化适配器
        /// </summary>
        /// <param name="world">ECS世界实例</param>
        void Initialize(World world);
        
        /// <summary>
        /// 更新适配器
        /// </summary>
        void Update();
        
        /// <summary>
        /// 关闭适配器并释放资源
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// 将游戏引擎数据同步到ECS
        /// </summary>
        void SyncToECS();
        
        /// <summary>
        /// 将ECS数据同步到游戏引擎
        /// </summary>
        void SyncFromECS();
    }
} 