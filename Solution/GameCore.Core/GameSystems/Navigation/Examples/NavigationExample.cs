using System;
using System.Collections.Generic;
using System.Numerics;
using GameCore.GameSystems.Navigation;
using GameCore.GameSystems.Navigation.Components;
using GameCore.GameSystems.Navigation.Grids;
using GameCore.GameSystems.Navigation.Pathfinding;

namespace GameCore.GameSystems.Navigation.Examples
{
    /// <summary>
    /// 导航系统使用示例，展示基本的寻路和导航功能
    /// </summary>
    public class NavigationExample
    {
        private NavigationSystem _navigationSystem;
        private NavigationAgent _agent;
        private List<NavigationObstacle> _obstacles;
        
        /// <summary>
        /// 初始化示例
        /// </summary>
        public void Initialize()
        {
            Console.WriteLine("Initializing Navigation System Example...");
            
            // Create the specific grid implementation (SquareGrid in this case)
            var grid = new SquareGrid(50, 50, 1.0f, new Vector3(-25, 0, -25));
            
            // Initialize the NavigationSystem with the grid
            _navigationSystem = new NavigationSystem();
            _navigationSystem.Initialize(grid); // Pass the IGrid implementation
            
            // Create obstacles (NavigationObstacle now interacts with NavigationSystem, which uses IGrid)
            CreateObstacles();
            
            // Create agent
            _agent = new NavigationAgent(_navigationSystem);
            _agent.Position = new Vector3(0, 0, 0);
            _agent.Speed = 5.0f;
            _agent.StoppingDistance = 0.1f;
            
            // 设置路径完成回调
            _agent.SetPathCompleteCallback(OnPathComplete);
            
            Console.WriteLine("Navigation System Example Initialized.");
        }
        
        /// <summary>
        /// 创建一些障碍物作为演示
        /// </summary>
        private void CreateObstacles()
        {
            _obstacles = new List<NavigationObstacle>();
            
            // Obstacles are created using the NavigationSystem, which handles grid updates via IGrid
            _obstacles.Add(new NavigationObstacle(_navigationSystem, new Vector3(5, 0, 5), 2.0f));
            _obstacles.Add(new NavigationObstacle(_navigationSystem, new Vector3(-5, 0, 8), 3.0f));
            _obstacles.Add(new NavigationObstacle(_navigationSystem, new Vector3(10, 0, -5), 2.5f));
            _obstacles.Add(new NavigationObstacle(_navigationSystem, new Vector3(0, 0, -10), new Vector3(10, 1, 2)));
            _obstacles.Add(new NavigationObstacle(_navigationSystem, new Vector3(-8, 0, -5), new Vector3(3, 1, 8)));
            
            Console.WriteLine($"Created {_obstacles.Count} obstacles.");
        }
        
        /// <summary>
        /// 寻路完成回调
        /// </summary>
        private void OnPathComplete(PathResult pathResult)
        {
            if (pathResult.IsPathFound)
            {
                Console.WriteLine($"Path Found! Status: {pathResult.Status}, Length: {pathResult.TotalLength:F2}, Time: {pathResult.ComputationTimeMs}ms, Waypoints: {pathResult.Waypoints.Count}");
            }
            else
            {
                Console.WriteLine($"Path Not Found! Status: {pathResult.Status}, Error: {pathResult.ErrorMessage}");
            }
        }
        
        /// <summary>
        /// 更新示例，应该在游戏循环中调用
        /// </summary>
        /// <param name="deltaTime">时间增量（秒）</param>
        public void Update(float deltaTime)
        {
            _agent?.Update(deltaTime);
            
            // Check destination reached (optional logging)
            // if (_agent != null && _agent.HasReachedDestination) { ... }
        }
        
        /// <summary>
        /// 发出移动请求
        /// </summary>
        /// <param name="destination">目标位置</param>
        public void RequestMove(Vector3 destination)
        {
            Console.WriteLine($"Requesting move to ({destination.X:F1}, {destination.Y:F1}, {destination.Z:F1})");
            _agent?.MoveTo(destination);
        }
        
        /// <summary>
        /// 在随机位置添加一个新的障碍物
        /// </summary>
        public void AddRandomObstacle()
        {
            Random random = new Random();
            float x = (float)(random.NextDouble() * 40 - 20);
            float z = (float)(random.NextDouble() * 40 - 20);
            Vector3 position = new Vector3(x, 0, z);
            
            if (random.Next(2) == 0)
            {
                float radius = (float)(random.NextDouble() * 2 + 1);
                _obstacles.Add(new NavigationObstacle(_navigationSystem, position, radius));
                Console.WriteLine($"Added Circle Obstacle: Pos({position.X:F1},{position.Z:F1}), R({radius:F1})");
            }
            else
            {
                float width = (float)(random.NextDouble() * 4 + 2);
                float length = (float)(random.NextDouble() * 4 + 2);
                Vector3 size = new Vector3(width, 1, length);
                _obstacles.Add(new NavigationObstacle(_navigationSystem, position, size));
                 Console.WriteLine($"Added Rect Obstacle: Pos({position.X:F1},{position.Z:F1}), Size({size.X:F1},{size.Z:F1})");
            }
        }
        
        /// <summary>
        /// 显示当前代理位置和路径信息
        /// </summary>
        public void DisplayAgentInfo()
        {
            if (_agent == null) return;
            Console.WriteLine($"Agent: Pos({_agent.Position.X:F1},{_agent.Position.Z:F1}), Dest({_agent.Destination.X:F1},{_agent.Destination.Z:F1}), Speed({_agent.Speed:F1}), FollowingPath({_agent.IsFollowingPath}), PathPoints({_agent.Path?.Count ?? 0})");
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            if (_obstacles != null)
            {
                foreach (var obstacle in _obstacles)
                {
                    obstacle.Dispose(); // Ensure obstacles are removed from the grid
                }
                _obstacles.Clear();
            }
            _navigationSystem = null; // Allow GC
            _agent = null;
            Console.WriteLine("Navigation System Example Cleaned Up.");
        }
    }
    
    /// <summary>
    /// 演示导航系统使用的主程序
    /// </summary>
    public static class NavigationDemo
    {
        /// <summary>
        /// 运行导航系统演示
        /// </summary>
        public static void Run()
        {
            Console.WriteLine("===== Navigation System Demo ====");
            
            NavigationExample example = new NavigationExample();
            example.Initialize();
            
            Vector3 startPosition = new Vector3(0, 0, 0);
            Vector3 endPosition = new Vector3(15, 0, 15);
            
            int frameCount = 100;
            float deltaTime = 0.033f; 
            
            Console.WriteLine($"Moving from ({startPosition.X:F1},{startPosition.Z:F1}) to ({endPosition.X:F1},{endPosition.Z:F1})");
            example.RequestMove(endPosition);
            
            for (int i = 0; i < frameCount; i++)
            {
                example.Update(deltaTime);
                
                if (i % 10 == 0) example.DisplayAgentInfo();
                if (i == 30) example.AddRandomObstacle();

                // Basic simulation delay
                System.Threading.Thread.Sleep((int)(deltaTime * 1000)); 
                 if (Console.KeyAvailable) { break; } // Allow early exit
            }
            
            example.Cleanup();
            Console.WriteLine("===== Demo End ====");
        }
    }
} 