using System;
using System.Numerics;
using System.Threading.Tasks;
using GameCore.GameSystems.Navigation.Grids;
using GameCore.GameSystems.Navigation.Interfaces;
using GameCore.GameSystems.Navigation.Pathfinding;
using GameCore.Systems;

namespace GameCore.GameSystems.Navigation
{
    /// <summary>
    /// Main navigation system, providing pathfinding and grid query services.
    /// Can work with different grid implementations via the IGrid interface.
    /// </summary>
    public class NavigationSystem : IInitializableSystem
    {
        private IGrid _grid;
        private IPathfinder _pathfinder;
        private bool _isInitialized;
        
        /// <summary>
        /// The underlying navigation grid (implementing IGrid).
        /// </summary>
        public IGrid Grid => _grid;
        
        /// <summary>
        /// The pathfinder used by the system.
        /// </summary>
        public IPathfinder Pathfinder => _pathfinder;
        
        /// <summary>
        /// Initializes the navigation system with a specific grid implementation.
        /// </summary>
        /// <param name="grid">The grid system to use (must implement IGrid).</param>
        /// <param name="pathfinder">Optional pathfinder instance. If null, AStarPathfinder is used.</param>
        public void Initialize(IGrid grid, IPathfinder pathfinder = null)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Navigation system is already initialized.");
            }
            
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _pathfinder = pathfinder ?? new AStarPathfinder(_grid);
            _isInitialized = true;
            Console.WriteLine($"NavigationSystem initialized with grid type: {_grid.GetType().Name}");
        }
        
        /// <summary>
        /// Default initialization using a SquareGrid.
        /// Implements IInitializableSystem.
        /// </summary>
        public void Initialize()
        {
            // Default initialization creates a standard SquareGrid.
            // For other grid types, use the Initialize(IGrid, IPathfinder) overload.
            var defaultGrid = new SquareGrid(100, 100, 1.0f, new Vector3(-50, 0, -50));
            Initialize(defaultGrid);
        }
        
        /// <summary>
        /// System update loop (currently unused by NavigationSystem).
        /// </summary>
        public void Update(float deltaTime)
        {
            // NavigationSystem doesn't typically require per-frame updates itself.
            // Agents using the system will have their own update logic.
        }
        
        /// <summary>
        /// 寻找从起点到终点的路径
        /// </summary>
        /// <param name="start">起点世界位置</param>
        /// <param name="end">终点世界位置</param>
        /// <param name="options">寻路选项</param>
        /// <returns>寻路结果</returns>
        public PathResult FindPath(Vector3 start, Vector3 end, PathfindingOptions options = null)
        {
            CheckInitialization();
            return _pathfinder.FindPath(start, end, options);
        }
        
        /// <summary>
        /// 异步寻找从起点到终点的路径
        /// </summary>
        /// <param name="start">起点世界位置</param>
        /// <param name="end">终点世界位置</param>
        /// <param name="options">寻路选项</param>
        /// <returns>寻路结果任务</returns>
        public Task<PathResult> FindPathAsync(Vector3 start, Vector3 end, PathfindingOptions options = null)
        {
            CheckInitialization();
            return _pathfinder.FindPathAsync(start, end, options);
        }
        
        /// <summary>
        /// 检查位置是否可行走
        /// </summary>
        /// <param name="position">要检查的位置</param>
        /// <returns>如果位置可行走返回true，否则返回false</returns>
        public bool IsPositionWalkable(Vector3 position)
        {
            CheckInitialization();
            return _pathfinder.IsPositionWalkable(position);
        }
        
        /// <summary>
        /// 获取最接近的可行走位置
        /// </summary>
        /// <param name="position">参考位置</param>
        /// <returns>最接近的可行走位置</returns>
        public Vector3 GetClosestWalkablePosition(Vector3 position)
        {
            CheckInitialization();
            return _pathfinder.GetClosestWalkablePosition(position);
        }
        
        /// <summary>
        /// 从高度图数据更新网格高度
        /// </summary>
        /// <param name="heightMap">高度图数据</param>
        /// <param name="mapWidth">高度图宽度</param>
        /// <param name="mapDepth">高度图深度</param>
        public void UpdateFromHeightMap(float[] heightMap, int mapWidth, int mapDepth)
        {
            CheckInitialization();
            _grid.UpdateGridHeights(heightMap, mapWidth, mapDepth);
        }
        
        /// <summary>
        /// 设置区域可行走状态
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <param name="walkable">是否可行走</param>
        public void SetAreaWalkable(Vector3 center, float radius, bool walkable)
        {
            CheckInitialization();
            _grid.SetAreaWalkable(center, radius, walkable);
        }
        
        /// <summary>
        /// 设置矩形区域可行走状态
        /// </summary>
        /// <param name="min">最小点（左下角）</param>
        /// <param name="max">最大点（右上角）</param>
        /// <param name="walkable">是否可行走</param>
        public void SetRectWalkable(Vector3 min, Vector3 max, bool walkable)
        {
            CheckInitialization();
            _grid.SetRectWalkable(min, max, walkable);
        }
        
        /// <summary>
        /// 重置导航网格，将所有节点设置为可行走
        /// </summary>
        public void ResetGrid()
        {
            CheckInitialization();
            _grid.ResetGridWalkability();
        }
        
        /// <summary>
        /// 更改寻路算法实现
        /// </summary>
        /// <param name="pathfinder">新的寻路器实现</param>
        public void SetPathfinder(IPathfinder pathfinder)
        {
            CheckInitialization();
            _pathfinder = pathfinder ?? throw new ArgumentNullException(nameof(pathfinder));
            Console.WriteLine($"NavigationSystem pathfinder updated to: {_pathfinder.GetType().Name}");
        }
        
        private void CheckInitialization()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Navigation system has not been initialized. Call Initialize() first.");
            }
        }
    }
} 