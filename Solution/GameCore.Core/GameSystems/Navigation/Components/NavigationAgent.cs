using System;
using System.Collections.Generic;
using System.Numerics;
using GameCore.GameSystems.Navigation.Pathfinding;

namespace GameCore.GameSystems.Navigation.Components
{
    /// <summary>
    /// 导航代理组件，用于实体使用导航系统进行寻路和移动
    /// </summary>
    public class NavigationAgent
    {
        private readonly NavigationSystem _navigationSystem;
        private Vector3 _currentPosition;
        private Vector3 _targetPosition;
        private List<Vector3>? _currentPath;
        private int _currentPathIndex;
        private bool _isFollowingPath;
        private PathfindingOptions _pathfindingOptions;
        private float _speed = 3.0f;
        private float _stoppingDistance = 0.1f;
        private float _pathReplanTime = 0.5f;
        private float _timeSinceLastRepath;
        private bool _isPathStale;
        private Action<PathResult>? _onPathComplete;
        
        /// <summary>
        /// 当前位置
        /// </summary>
        public Vector3 Position
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;
                // 如果位置改变，当前路径可能不再有效
                _isPathStale = _isFollowingPath;
            }
        }
        
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 Destination
        {
            get => _targetPosition;
            set
            {
                if (_targetPosition != value)
                {
                    _targetPosition = value;
                    _isPathStale = _isFollowingPath;
                }
            }
        }
        
        /// <summary>
        /// 移动速度（单位/秒）
        /// </summary>
        public float Speed
        {
            get => _speed;
            set => _speed = Math.Max(0.1f, value);
        }
        
        /// <summary>
        /// 停止距离，小于此距离视为已到达目标
        /// </summary>
        public float StoppingDistance
        {
            get => _stoppingDistance;
            set => _stoppingDistance = Math.Max(0.01f, value);
        }
        
        /// <summary>
        /// 路径重新计算时间（秒）
        /// </summary>
        public float PathReplanTime
        {
            get => _pathReplanTime;
            set => _pathReplanTime = Math.Max(0.1f, value);
        }
        
        /// <summary>
        /// 寻路选项
        /// </summary>
        public PathfindingOptions PathfindingOptions
        {
            get => _pathfindingOptions;
            set => _pathfindingOptions = value ?? PathfindingOptions.Default();
        }
        
        /// <summary>
        /// 是否正在跟随路径
        /// </summary>
        public bool IsFollowingPath => _isFollowingPath;
        
        /// <summary>
        /// 是否到达目标
        /// </summary>
        public bool HasReachedDestination => !_isFollowingPath || (_currentPath != null && _currentPathIndex >= _currentPath.Count);
        
        /// <summary>
        /// 当前路径
        /// </summary>
        public IReadOnlyList<Vector3>? Path => _currentPath;
        
        /// <summary>
        /// 创建一个新的导航代理
        /// </summary>
        /// <param name="navigationSystem">导航系统</param>
        public NavigationAgent(NavigationSystem navigationSystem)
        {
            _navigationSystem = navigationSystem ?? throw new ArgumentNullException(nameof(navigationSystem));
            _pathfindingOptions = PathfindingOptions.Default();
            _currentPath = null;
        }
        
        /// <summary>
        /// 设置当路径计算完成时的回调
        /// </summary>
        /// <param name="callback">回调方法</param>
        public void SetPathCompleteCallback(Action<PathResult>? callback)
        {
            _onPathComplete = callback;
        }
        
        /// <summary>
        /// 请求移动到指定位置
        /// </summary>
        /// <param name="destination">目标位置</param>
        /// <param name="immediately">是否立即计算路径（否则将在下一次更新时计算）</param>
        public void MoveTo(Vector3 destination, bool immediately = true)
        {
            _targetPosition = destination;
            _isPathStale = true;
            if (immediately) CalculatePath();
        }
        
        /// <summary>
        /// 停止移动
        /// </summary>
        public void Stop()
        {
            _isFollowingPath = false;
            _currentPath = null;
            _currentPathIndex = 0;
        }
        
        /// <summary>
        /// 更新导航代理
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        public void Update(float deltaTime)
        {
            if (!_isFollowingPath && _isPathStale)
            {
                CalculatePath();
                return;
            }
            
            if (!_isFollowingPath || _currentPath == null || _currentPath.Count == 0 || _currentPathIndex >= _currentPath.Count)
            {
                return;
            }
            
            _timeSinceLastRepath += deltaTime;
            if (_timeSinceLastRepath >= _pathReplanTime && _isPathStale)
            {
                CalculatePath();
                return;
            }
            
            Vector3 targetPoint = _currentPath[_currentPathIndex];
            Vector3 direction = targetPoint - _currentPosition;
            float distanceToTarget = direction.Length();
            
            if (distanceToTarget <= _stoppingDistance)
            {
                _currentPathIndex++;
                if (_currentPathIndex >= _currentPath.Count)
                {
                    _currentPosition = targetPoint;
                    Stop();
                    return;
                }
                targetPoint = _currentPath[_currentPathIndex];
                direction = targetPoint - _currentPosition;
                distanceToTarget = direction.Length();
                if (distanceToTarget < 0.001f) return;
            }
            
            float moveDistance = _speed * deltaTime;
            if (moveDistance >= distanceToTarget)
            {
                _currentPosition = targetPoint;
            }
            else
            {
                direction /= distanceToTarget;
                _currentPosition += direction * moveDistance;
            }
        }
        
        /// <summary>
        /// 计算从当前位置到目标的路径
        /// </summary>
        private void CalculatePath()
        {
            _timeSinceLastRepath = 0;
            _isPathStale = false;
            
            if (Vector3.DistanceSquared(_currentPosition, _targetPosition) <= _stoppingDistance * _stoppingDistance)
            {
                List<Vector3> shortPath = new List<Vector3> { _currentPosition, _targetPosition };
                PathResult result = PathResult.Success(shortPath, 0, 0);
                _onPathComplete?.Invoke(result);
                Stop();
                return;
            }
            
            PathResult pathResult = _navigationSystem.FindPath(_currentPosition, _targetPosition, _pathfindingOptions);
            
            _onPathComplete?.Invoke(pathResult);
            
            if (pathResult.IsPathFound && pathResult.Waypoints.Count > 0)
            {
                _currentPath = new List<Vector3>(pathResult.Waypoints);
                _currentPathIndex = 0;
                _isFollowingPath = true;
                _isPathStale = false;
            }
            else
            {
                Stop();
            }
        }
        
        /// <summary>
        /// 找到并返回路径中最近的点的索引
        /// </summary>
        /// <returns>最近点的索引</returns>
        public int GetClosestPathPointIndex()
        {
            if (_currentPath == null || _currentPath.Count == 0 || _currentPathIndex >= _currentPath.Count) return -1;
            
            int closestIndex = _currentPathIndex;
            float closestDistSqr = Vector3.DistanceSquared(_currentPosition, _currentPath[_currentPathIndex]);
            
            for (int i = _currentPathIndex + 1; i < _currentPath.Count; i++)
            {
                float distSqr = Vector3.DistanceSquared(_currentPosition, _currentPath[i]);
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }
        
        /// <summary>
        /// 设置新的路径
        /// </summary>
        /// <param name="path">路径点列表</param>
        public void SetPath(List<Vector3>? path)
        {
            if (path == null || path.Count == 0)
            {
                Stop();
                return;
            }
            
            _currentPath = new List<Vector3>(path);
            _currentPathIndex = 0;
            _isFollowingPath = true;
            _targetPosition = path[path.Count - 1];
            _isPathStale = false;
        }
    }
} 