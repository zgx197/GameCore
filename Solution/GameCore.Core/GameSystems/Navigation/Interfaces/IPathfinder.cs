using System;
using System.Collections.Generic;
using System.Numerics;
using GameCore.GameSystems.Navigation.Pathfinding;

namespace GameCore.GameSystems.Navigation.Interfaces
{
    /// <summary>
    /// 寻路算法接口，用于抽象不同的寻路实现
    /// </summary>
    public interface IPathfinder
    {
        /// <summary>
        /// 寻找从起点到终点的路径
        /// </summary>
        /// <param name="start">起点世界位置</param>
        /// <param name="end">终点世界位置</param>
        /// <param name="options">寻路选项</param>
        /// <returns>寻路结果</returns>
        PathResult FindPath(Vector3 start, Vector3 end, PathfindingOptions options = null);
        
        /// <summary>
        /// 异步寻找从起点到终点的路径
        /// </summary>
        /// <param name="start">起点世界位置</param>
        /// <param name="end">终点世界位置</param>
        /// <param name="options">寻路选项</param>
        /// <returns>寻路结果任务</returns>
        System.Threading.Tasks.Task<PathResult> FindPathAsync(Vector3 start, Vector3 end, PathfindingOptions options = null);
        
        /// <summary>
        /// 检查位置是否可到达
        /// </summary>
        /// <param name="position">要检查的位置</param>
        /// <returns>如果位置可到达返回true，否则返回false</returns>
        bool IsPositionWalkable(Vector3 position);
        
        /// <summary>
        /// 获取最接近的可行走位置
        /// </summary>
        /// <param name="position">参考位置</param>
        /// <returns>最接近的可行走位置</returns>
        Vector3 GetClosestWalkablePosition(Vector3 position);
    }
} 