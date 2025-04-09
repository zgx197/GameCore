using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameCore.GameSystems.Navigation.Pathfinding
{
    /// <summary>
    /// 路径查找状态
    /// </summary>
    public enum PathfindingStatus
    {
        /// <summary>
        /// 成功找到路径
        /// </summary>
        Success,
        
        /// <summary>
        /// 部分路径找到（例如由于超时），但是没有到达最终目的地。
        /// </summary>
        PartialPathFound,
        
        /// <summary>
        /// 无法找到路径
        /// </summary>
        NoPathFound,
        
        /// <summary>
        /// 起点无效
        /// </summary>
        InvalidStart,
        
        /// <summary>
        /// 终点无效
        /// </summary>
        InvalidEnd,
        
        /// <summary>
        /// 寻路过程中超时
        /// </summary>
        Timeout,
        
        /// <summary>
        /// 寻路过程中出错
        /// </summary>
        Error
    }
    
    /// <summary>
    /// 寻路结果，包含路径和状态信息
    /// </summary>
    public class PathResult
    {
        /// <summary>
        /// 路径点列表
        /// </summary>
        public IReadOnlyList<Vector3> Waypoints { get; }
        
        /// <summary>
        /// 路径状态
        /// </summary>
        public PathfindingStatus Status { get; }
        
        /// <summary>
        /// 路径总长度
        /// </summary>
        public float TotalLength { get; }
        
        /// <summary>
        /// 寻路耗时（毫秒）
        /// </summary>
        public float ComputationTimeMs { get; }
        
        /// <summary>
        /// 如果寻路失败，包含错误信息
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// 创建成功的寻路结果
        /// </summary>
        /// <param name="waypoints">路径点</param>
        /// <param name="totalLength">路径总长度</param>
        /// <param name="computationTimeMs">计算耗时</param>
        /// <param name="status">寻路状态</param>
        /// <returns>寻路结果</returns>
        public static PathResult Success(List<Vector3> waypoints, float totalLength, long computationTimeMs, PathfindingStatus status = PathfindingStatus.Success)
        {
            return new PathResult(waypoints ?? new List<Vector3>(), status, totalLength, computationTimeMs, null);
        }
        
        /// <summary>
        /// 创建失败的寻路结果
        /// </summary>
        /// <param name="status">失败状态</param>
        /// <param name="errorMessage">错误信息</param>
        /// <param name="computationTimeMs">计算耗时</param>
        /// <returns>寻路结果</returns>
        public static PathResult Failure(PathfindingStatus status, string errorMessage, long computationTimeMs)
        {
            if (status == PathfindingStatus.Success || status == PathfindingStatus.PartialPathFound)
            {
                status = PathfindingStatus.Error;
            }
            return new PathResult(new List<Vector3>(), status, 0, computationTimeMs, errorMessage);
        }
        
        private PathResult(IReadOnlyList<Vector3> waypoints, PathfindingStatus status, float totalLength, float computationTimeMs, string errorMessage)
        {
            Waypoints = waypoints;
            Status = status;
            TotalLength = totalLength;
            ComputationTimeMs = computationTimeMs;
            ErrorMessage = errorMessage;
        }
        
        /// <summary>
        /// 路径是否找到
        /// </summary>
        public bool IsPathFound => Status == PathfindingStatus.Success;
    }
} 