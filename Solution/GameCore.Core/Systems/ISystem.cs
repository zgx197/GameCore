namespace GameCore.Systems
{
    /// <summary>
    /// 系统接口，所有游戏系统的基础接口
    /// </summary>
    public interface ISystem
    {
        /// <summary>
        /// 更新系统
        /// </summary>
        /// <param name="deltaTime">自上次更新以来的时间增量（秒）</param>
        void Update(float deltaTime);
    }
    
    /// <summary>
    /// 可初始化系统接口
    /// </summary>
    public interface IInitializableSystem : ISystem
    {
        /// <summary>
        /// 初始化系统
        /// </summary>
        void Initialize();
    }
} 