namespace GameCore.ECS.Systems
{
    /// <summary>
    /// 系统接口，定义所有ECS系统必须实现的契约
    /// 系统负责处理游戏逻辑，操作具有特定组件的实体
    /// </summary>
    public interface ISystem
    {
        /// <summary>
        /// 系统初始化，在系统首次添加到世界时调用
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 系统执行，每次更新循环中调用
        /// </summary>
        void Execute();
        
        /// <summary>
        /// 系统清理，在系统从世界中移除时调用
        /// </summary>
        void Cleanup();
    }
} 