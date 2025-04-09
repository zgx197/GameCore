using System.Numerics;
using GameCore.GameSystems.Navigation.Grids;

namespace GameCore.GameSystems.Navigation
{
    /// <summary>
    /// 导航代理接口
    /// </summary>
    public interface INavigationAgent
    {
        /// <summary>
        /// 获取导航代理使用的网格
        /// </summary>
        IGrid Grid { get; }
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        void MoveTo(Vector3 position);
        
        /// <summary>
        /// 停止移动
        /// </summary>
        void Stop();
    }
} 