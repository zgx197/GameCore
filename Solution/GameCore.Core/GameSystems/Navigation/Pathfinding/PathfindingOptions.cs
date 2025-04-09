using System;
using System.Numerics;

namespace GameCore.GameSystems.Navigation.Pathfinding
{
    /// <summary>
    /// 寻路选项，用于配置寻路算法行为
    /// </summary>
    public class PathfindingOptions
    {
        /// <summary>
        /// 路径平滑程度（0-1），值越大平滑度越高
        /// </summary>
        public float SmoothingFactor { get; set; } = 0.2f;
        
        /// <summary>
        /// 最大寻路时间（毫秒），超过此时间将返回已找到的最佳路径
        /// </summary>
        public int TimeoutMs { get; set; } = 500;
        
        /// <summary>
        /// 最大寻路节点数，超过此数量将返回已找到的最佳路径
        /// </summary>
        public int MaxNodes { get; set; } = 10000;
        
        /// <summary>
        /// 路径简化容差，相近点距离小于此值将被合并
        /// </summary>
        public float SimplificationTolerance { get; set; } = 0.1f;
        
        /// <summary>
        /// 单位半径，用于计算通行性
        /// </summary>
        public float AgentRadius { get; set; } = 0.5f;
        
        /// <summary>
        /// 单位高度，用于计算通行性
        /// </summary>
        public float AgentHeight { get; set; } = 2.0f;
        
        /// <summary>
        /// 单位最大坡度（度），用于计算通行性
        /// </summary>
        public float MaxSlopeAngle { get; set; } = 45.0f;
        
        /// <summary>
        /// 单位最大高度差，用于计算通行性
        /// </summary>
        public float MaxHeightDifference { get; set; } = 0.5f;
        
        /// <summary>
        /// 是否允许对角线移动（适用于网格寻路）
        /// </summary>
        public bool AllowDiagonalMovement { get; set; } = true;
        
        /// <summary>
        /// 是否避开动态障碍物
        /// </summary>
        public bool AvoidDynamicObstacles { get; set; } = true;
        
        /// <summary>
        /// 高度权重，影响高度差异对路径选择的影响
        /// </summary>
        public float HeightWeight { get; set; } = 1.0f;
        
        /// <summary>
        /// 如果终点不可达，是否返回最接近的可达点
        /// </summary>
        public bool FindNearestIfUnreachable { get; set; } = true;
        
        /// <summary>
        /// 创建默认寻路选项
        /// </summary>
        /// <returns>默认选项</returns>
        public static PathfindingOptions Default()
        {
            return new PathfindingOptions();
        }
        
        /// <summary>
        /// 创建高性能寻路选项（更小的搜索空间，更快但可能不是最优）
        /// </summary>
        /// <returns>高性能选项</returns>
        public static PathfindingOptions HighPerformance()
        {
            return new PathfindingOptions
            {
                TimeoutMs = 100,
                MaxNodes = 2000,
                SimplificationTolerance = 0.5f,
                SmoothingFactor = 0.1f
            };
        }
        
        /// <summary>
        /// 创建高质量寻路选项（更大的搜索空间，更精确但更慢）
        /// </summary>
        /// <returns>高质量选项</returns>
        public static PathfindingOptions HighQuality()
        {
            return new PathfindingOptions
            {
                TimeoutMs = 2000,
                MaxNodes = 50000,
                SimplificationTolerance = 0.05f,
                SmoothingFactor = 0.5f
            };
        }
    }
} 