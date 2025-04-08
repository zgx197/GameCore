#if UNITY
using UnityEngine;
using GameCore.GameSystems.Navigation.Components; // Core NavigationObstacle
using GameCore.Unity.Adapters; // Vector3Adapter
using System;
using SysVec3 = System.Numerics.Vector3; // Add missing using alias

namespace GameCore.Unity.Navigation
{
    /// <summary>
    /// Unity MonoBehaviour component that wraps the core NavigationObstacle.
    /// Attach this to GameObjects that should dynamically block navigation.
    /// Automatically updates the navigation grid when the object moves or its state changes.
    /// </summary>
    [AddComponentMenu("GameCore/Navigation/Navigation Obstacle")]
    public class UnityNavigationObstacle : MonoBehaviour
    {
        [Header("Obstacle Shape")]
        [SerializeField] private ObstacleType _obstacleType = ObstacleType.Circle;
        [Tooltip("Radius of the obstacle if type is Circle.")]
        [SerializeField] private float _radius = 0.5f;
        [Tooltip("Size (width, height, length) of the obstacle if type is Rectangle. Height is ignored.")]
        [SerializeField] private Vector3 _size = Vector3.one;

        [Header("State")]
        [Tooltip("Is the obstacle currently active and blocking navigation?")]
        [SerializeField] private bool _isActive = true;
        
        [Header("Debugging")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private Color _gizmoColor = Color.red;

        private NavigationObstacle _coreObstacle;
        private Vector3 _lastPosition; // To detect movement
        private float _lastRadius;     // To detect radius change
        private Vector3 _lastSize;       // To detect size change
        private bool _lastIsActive;    // To detect state change

        public NavigationObstacle CoreObstacle => _coreObstacle;

        void Start()
        {
            if (NavigationService.Instance == null)
            {
                UnityEngine.Debug.LogError("NavigationService not found in the scene. UnityNavigationObstacle requires a NavigationService.", this);
                enabled = false;
                return;
            }

            InitializeCoreObstacle();
            _lastPosition = transform.position;
            _lastRadius = _radius;
            _lastSize = _size;
            _lastIsActive = _isActive;
        }

        void Update()
        {
            if (_coreObstacle == null) return;

            bool changed = false;

            // Check for position change
            if (transform.position != _lastPosition)
            {
                _coreObstacle.Position = Vector3Adapter.ToSystemNumerics(transform.position);
                _lastPosition = transform.position;
                changed = true; // Position changes require re-applying to grid
            }

            // Check for radius change (if circle)
            if (_obstacleType == ObstacleType.Circle && _radius != _lastRadius)
            {
                _coreObstacle.Radius = _radius;
                _lastRadius = _radius;
                changed = true;
            }
            
            // Check for size change (if rectangle)
             if (_obstacleType == ObstacleType.Rectangle && _size != _lastSize)
            {
                _coreObstacle.Size = Vector3Adapter.ToSystemNumerics(_size);
                _lastSize = _size;
                changed = true;
            }

            // Check for active state change from Inspector
            if (_isActive != _lastIsActive)
            {
                _coreObstacle.IsActive = _isActive;
                _lastIsActive = _isActive;
                // No need to set changed=true here, IsActive setter handles grid update
            }
            
            // Note: Shape type change during runtime is handled by the Inspector logic below
            // because it requires recreating the core obstacle.
        }

        // Called when the script is enabled/disabled in the Inspector or via code
        void OnEnable()
        { 
            if (_coreObstacle != null) 
            {
                _coreObstacle.IsActive = true; 
                _isActive = true;
                _lastIsActive = true;
            }
        }

        void OnDisable()
        { 
             if (_coreObstacle != null) 
             {
                _coreObstacle.IsActive = false; 
                 _isActive = false;
                 _lastIsActive = false;
             }
        }

        // Called when the component is destroyed
        void OnDestroy()
        {
            _coreObstacle?.Dispose(); // Crucial: remove obstacle from grid
        }

        // Initialize or re-initialize the core obstacle based on current settings
        private void InitializeCoreObstacle()
        {
            // Dispose previous if exists (e.g., when changing type in inspector)
            _coreObstacle?.Dispose(); 

            if (NavigationService.Instance?.System == null) 
            {
                 UnityEngine.Debug.LogError("Cannot initialize core obstacle: NavigationService not ready.");
                 return;
            }

            Vector3 currentPosition = transform.position;
            SysVec3 sysPosition = Vector3Adapter.ToSystemNumerics(currentPosition);

            if (_obstacleType == ObstacleType.Circle)
            {
                _coreObstacle = new NavigationObstacle(NavigationService.Instance.System, sysPosition, _radius, _isActive);
            }
            else // Rectangle
            {
                SysVec3 sysSize = Vector3Adapter.ToSystemNumerics(_size);
                _coreObstacle = new NavigationObstacle(NavigationService.Instance.System, sysPosition, sysSize, _isActive);
            }
            
            _lastPosition = currentPosition;
            _lastRadius = _radius;
            _lastSize = _size;
            _lastIsActive = _isActive;
        }

        // Called in the editor when a value is changed in the Inspector
        void OnValidate()
        {
            // Clamp values
            _radius = Mathf.Max(0.01f, _radius);
            _size = new Vector3(Mathf.Max(0.01f, _size.x), Mathf.Max(0.01f, _size.y), Mathf.Max(0.01f, _size.z));

            // If the application is playing, changing type or critical parameters requires reinitialization
            if (Application.isPlaying && _coreObstacle != null)
            {
                // Check if type changed
                if (_coreObstacle.ObstacleType != _obstacleType)
                {
                     UnityEngine.Debug.Log("Obstacle type changed, reinitializing...");
                     InitializeCoreObstacle(); // Recreate with new type
                }
                else
                {
                    // Update parameters directly if type is the same
                    // Note: Radius/Size/Active changes are handled in Update or OnEnable/Disable
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            Gizmos.color = _gizmoColor;
            Vector3 position = transform.position;

            if (_obstacleType == ObstacleType.Circle)
            {
                Gizmos.DrawWireSphere(position, _radius);
            }
            else // Rectangle
            {
                // Draw wireframe cube based on size, ignoring rotation for simplicity
                Gizmos.DrawWireCube(position, _size); 
            }
        }
    }
}
#endif 