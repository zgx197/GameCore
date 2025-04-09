using System;
using System.Numerics;
using GameCore.GameSystems.Navigation;

namespace GameCore.GameSystems.Navigation.Components
{
    /// <summary>
    /// 导航障碍物类型枚举
    /// </summary>
    public enum ObstacleType
    {
        /// <summary>
        /// 圆形障碍物
        /// </summary>
        Circle,
        
        /// <summary>
        /// 矩形障碍物
        /// </summary>
        Rectangle
    }
    
    /// <summary>
    /// 导航障碍物组件，用于在导航网格上创建动态障碍
    /// </summary>
    public class NavigationObstacle
    {
        private NavigationSystem _navigationSystem;
        private ObstacleType _obstacleType;
        private Vector3 _position;
        private Vector3 _size; // 用于矩形，x和z分别表示宽和长
        private float _radius; // 用于圆形
        private bool _isActive;
        
        /// <summary>
        /// 获取或设置障碍物在世界中的位置
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    // 如果障碍物是活动的，先移除旧位置的障碍
                    if (_isActive)
                    {
                        RemoveFromNavigation();
                    }
                    
                    _position = value;
                    
                    // 然后更新到新位置
                    if (_isActive)
                    {
                        AddToNavigation();
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取或设置矩形障碍物的大小 (仅当ObstacleType为Rectangle时有效)
        /// </summary>
        public Vector3 Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    // 如果障碍物是活动的，先移除旧大小的障碍
                    if (_isActive && _obstacleType == ObstacleType.Rectangle)
                    {
                        RemoveFromNavigation();
                    }
                    
                    _size = value;
                    
                    // 然后更新新大小的障碍
                    if (_isActive && _obstacleType == ObstacleType.Rectangle)
                    {
                        AddToNavigation();
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取或设置圆形障碍物的半径 (仅当ObstacleType为Circle时有效)
        /// </summary>
        public float Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    // 如果障碍物是活动的，先移除旧半径的障碍
                    if (_isActive && _obstacleType == ObstacleType.Circle)
                    {
                        RemoveFromNavigation();
                    }
                    
                    _radius = Math.Max(0.01f, value);
                    
                    // 然后更新新半径的障碍
                    if (_isActive && _obstacleType == ObstacleType.Circle)
                    {
                        AddToNavigation();
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取或设置障碍物是否活动
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    
                    if (_isActive)
                    {
                        AddToNavigation();
                    }
                    else
                    {
                        RemoveFromNavigation();
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取障碍物类型
        /// </summary>
        public ObstacleType ObstacleType => _obstacleType;
        
        /// <summary>
        /// 创建一个圆形障碍物
        /// </summary>
        /// <param name="navigationSystem">导航系统</param>
        /// <param name="position">位置</param>
        /// <param name="radius">半径</param>
        /// <param name="isActive">是否立即激活</param>
        public NavigationObstacle(NavigationSystem navigationSystem, Vector3 position, float radius, bool isActive = true)
        {
            _navigationSystem = navigationSystem ?? throw new ArgumentNullException(nameof(navigationSystem));
            _obstacleType = ObstacleType.Circle;
            _position = position;
            _radius = Math.Max(0.01f, radius);
            _isActive = isActive;
            
            if (_isActive)
            {
                AddToNavigation();
            }
        }
        
        /// <summary>
        /// 创建一个矩形障碍物
        /// </summary>
        /// <param name="navigationSystem">导航系统</param>
        /// <param name="position">位置（中心点）</param>
        /// <param name="size">大小（x和z分别表示宽和长）</param>
        /// <param name="isActive">是否立即激活</param>
        public NavigationObstacle(NavigationSystem navigationSystem, Vector3 position, Vector3 size, bool isActive = true)
        {
            _navigationSystem = navigationSystem ?? throw new ArgumentNullException(nameof(navigationSystem));
            _obstacleType = ObstacleType.Rectangle;
            _position = position;
            _size = size;
            _isActive = isActive;
            
            if (_isActive)
            {
                AddToNavigation();
            }
        }
        
        /// <summary>
        /// 在导航网格上添加障碍
        /// </summary>
        private void AddToNavigation()
        {
            if (_navigationSystem == null)
            {
                return;
            }
            
            switch (_obstacleType)
            {
                case ObstacleType.Circle:
                    _navigationSystem.SetAreaWalkable(_position, _radius, false);
                    break;
                    
                case ObstacleType.Rectangle:
                    Vector3 halfSize = _size * 0.5f;
                    Vector3 min = new Vector3(_position.X - halfSize.X, _position.Y, _position.Z - halfSize.Z);
                    Vector3 max = new Vector3(_position.X + halfSize.X, _position.Y, _position.Z + halfSize.Z);
                    _navigationSystem.SetRectWalkable(min, max, false);
                    break;
            }
        }
        
        /// <summary>
        /// 从导航网格上移除障碍
        /// </summary>
        private void RemoveFromNavigation()
        {
            if (_navigationSystem == null)
            {
                return;
            }
            
            switch (_obstacleType)
            {
                case ObstacleType.Circle:
                    _navigationSystem.SetAreaWalkable(_position, _radius, true);
                    break;
                    
                case ObstacleType.Rectangle:
                    Vector3 halfSize = _size * 0.5f;
                    Vector3 min = new Vector3(_position.X - halfSize.X, _position.Y, _position.Z - halfSize.Z);
                    Vector3 max = new Vector3(_position.X + halfSize.X, _position.Y, _position.Z + halfSize.Z);
                    _navigationSystem.SetRectWalkable(min, max, true);
                    break;
            }
        }
        
        /// <summary>
        /// 设置为一个新的圆形障碍物
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="radius">半径</param>
        public void SetAsCircle(Vector3 position, float radius)
        {
            if (_isActive) RemoveFromNavigation();
            _obstacleType = ObstacleType.Circle;
            _position = position;
            _radius = Math.Max(0.01f, radius);
            if (_isActive) AddToNavigation();
        }
        
        /// <summary>
        /// 设置为一个新的矩形障碍物
        /// </summary>
        /// <param name="position">位置（中心点）</param>
        /// <param name="size">大小（x和z分别表示宽和长）</param>
        public void SetAsRectangle(Vector3 position, Vector3 size)
        {
            if (_isActive) RemoveFromNavigation();
            _obstacleType = ObstacleType.Rectangle;
            _position = position;
            _size = size;
            if (_isActive) AddToNavigation();
        }
        
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isActive) RemoveFromNavigation();
            _isActive = false;
            _navigationSystem = null;
        }
    }
} 