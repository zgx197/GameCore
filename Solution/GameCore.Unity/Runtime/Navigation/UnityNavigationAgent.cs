#if UNITY
using UnityEngine;
using GameCore.GameSystems.Navigation.Components; // Core NavigationAgent
using GameCore.GameSystems.Navigation.Pathfinding; // PathResult, PathfindingOptions
using GameCore.Unity.Adapters; // Vector3Adapter
using System.Collections.Generic; // List
using SysVec3 = System.Numerics.Vector3;
using GameCore.GameSystems.Navigation.Grids;
using GameCore.GameSystems.Navigation; // Add this for INavigationAgent

namespace GameCore.Unity.Navigation
{
    /// <summary>
    /// Unity MonoBehaviour component that wraps the core NavigationAgent.
    /// Attach this to GameObjects that need pathfinding capabilities.
    /// Handles synchronization between Unity's Transform and the core agent's position.
    /// </summary>
    [AddComponentMenu("GameCore/Navigation/Navigation Agent")]
    [RequireComponent(typeof(Rigidbody))] // Recommend using Rigidbody for movement
    public class UnityNavigationAgent : MonoBehaviour, INavigationAgent
    {
        [Header("Movement Settings")]
        [Tooltip("Movement speed in world units per second.")]
        [SerializeField] private float _speed = 5.0f;
        [Tooltip("Distance from the destination waypoint at which the agent is considered to have arrived.")]
        [SerializeField] private float _stoppingDistance = 0.2f;
        [Tooltip("How often (in seconds) the agent should check if its path needs recalculating.")]
        [SerializeField] private float _pathReplanTime = 0.5f;
        [Tooltip("Maximum acceleration for smoother movement (units/sec^2). Set to 0 for instant velocity change.")]
        [SerializeField] private float _acceleration = 8.0f; // Added for smoother movement
        [Tooltip("Angular speed for turning towards the next waypoint (degrees/sec).")]
        [SerializeField] private float _angularSpeed = 120.0f; // Added for smoother rotation

        [Header("Pathfinding Options")]
        [Tooltip("Options controlling the pathfinding algorithm (e.g., smoothing, tolerance).")]
        [SerializeField] private PathfindingOptions _pathfindingOptions = PathfindingOptions.Default();
        
        [Header("Debugging")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private Color _pathColor = Color.green;
        [SerializeField] private float _waypointRadius = 0.1f;

        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private UnityGrid _grid;
        
        private NavigationAgent _coreAgent;
        private Rigidbody _rigidbody;
        private Vector3 _velocity = Vector3.zero; // Added for acceleration-based movement
        private Vector3 _targetPosition;
        private bool _isMoving;
        private PathResult _currentPath;

        public NavigationAgent CoreAgent => _coreAgent;
        public bool IsFollowingPath => _coreAgent?.IsFollowingPath ?? false;
        public bool HasReachedDestination => _coreAgent?.HasReachedDestination ?? true;
        public IReadOnlyList<SysVec3> CorePath => _coreAgent?.Path;
        public IGrid Grid => _grid;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                UnityEngine.Debug.LogError("UnityNavigationAgent requires a Rigidbody component.", this);
                enabled = false; // Disable script if no Rigidbody
                return;
            }
             _rigidbody.isKinematic = true; // Recommended for direct velocity control
             _rigidbody.useGravity = false; // Disable gravity if controlling vertically too
        }

        void Start()
        {
            if (NavigationService.Instance == null)
            {
                UnityEngine.Debug.LogError("NavigationService not found in the scene. UnityNavigationAgent requires a NavigationService.", this);
                enabled = false;
                return;
            }

            // Create the core agent instance
            _coreAgent = new NavigationAgent(NavigationService.Instance.System);

            // Apply serialized settings to the core agent
            SyncSettingsToCoreAgent();

            // Set initial position
            _coreAgent.Position = Vector3Adapter.ToSystem(transform.position);

            // Optional: Set up callback for path results
            // _coreAgent.SetPathCompleteCallback(HandlePathResult);
        }

        void Update()
        {
            if (_coreAgent == null) return;

            // Sync Unity position TO core agent before its update
            _coreAgent.Position = Vector3Adapter.ToSystem(transform.position);

            // Update the core agent logic (path following state, replanning)
            _coreAgent.Update(Time.deltaTime);

            // No direct movement application here, FixedUpdate handles physics
        }

        void FixedUpdate()
        {
             if (_coreAgent == null || !_coreAgent.IsFollowingPath || _coreAgent.Path == null || _coreAgent.Path.Count == 0)
            {
                // Not moving or no path, decelerate to zero velocity
                _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _acceleration * Time.fixedDeltaTime);
                _rigidbody.velocity = _velocity;
                return;
            }

            // Get current target waypoint
            int currentWaypointIndex = _coreAgent.GetClosestPathPointIndex(); // Use core logic to find where we are on path
             if (currentWaypointIndex < 0 || currentWaypointIndex >= _coreAgent.Path.Count)
            {
                Stop(); // Invalid index, stop movement
                return;
            }
            Vector3 targetWaypoint = Vector3Adapter.ToUnity(_coreAgent.Path[currentWaypointIndex]);
            
            // --- Movement Logic --- 
            Vector3 currentPosition = _rigidbody.position; // Use Rigidbody position
            Vector3 directionToTarget = (targetWaypoint - currentPosition);
            // Ignore Y difference for direction calculation on flat ground movement
            directionToTarget.y = 0; 
            float distance = directionToTarget.magnitude;

            // If close enough to the *final* destination, stop precisely
            if (_coreAgent.HasReachedDestination && distance < _stoppingDistance)
            {
                 _velocity = Vector3.zero;
            }
            else
            {
                // Calculate desired velocity towards the target waypoint
                Vector3 desiredVelocity = directionToTarget.normalized * _speed;

                // Apply acceleration
                _velocity = Vector3.MoveTowards(_velocity, desiredVelocity, _acceleration * Time.fixedDeltaTime);
                
                 // --- Rotation Logic --- 
                if (_velocity.sqrMagnitude > 0.01f) // Only rotate if moving
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
                    Quaternion newRotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, _angularSpeed * Time.fixedDeltaTime);
                    _rigidbody.MoveRotation(newRotation);
                }
            }

            // Apply velocity to Rigidbody
            _rigidbody.velocity = _velocity;
        }

        /// <summary>
        /// Requests the agent to move to the specified destination.
        /// </summary>
        /// <param name="destination">World space destination point.</param>
        public void MoveTo(SysVec3 destination)
        {
            SyncSettingsToCoreAgent(); // Ensure latest settings are used
            _coreAgent?.MoveTo(destination);
        }

        /// <summary>
        /// Overload for MoveTo using UnityEngine.Vector3 for convenience in Unity Editor.
        /// </summary>
        public void MoveTo(Vector3 destination)
        {
            MoveTo(Vector3Adapter.ToSystem(destination));
        }

        /// <summary>
        /// Stops the agent's current movement and clears its path.
        /// </summary>
        public void Stop()
        {
            _coreAgent?.Stop();
            _velocity = Vector3.zero; // Immediately stop velocity
            if(_rigidbody != null) _rigidbody.velocity = Vector3.zero;
        }
        
        /// <summary>
        /// Sets a new path for the agent to follow directly.
        /// </summary>
        /// <param name="unityPath">List of world space waypoints.</param>
        public void SetPath(List<Vector3> unityPath)
        {
            if (unityPath == null) 
            {
                _coreAgent?.SetPath(null);
                return;
            }
            var systemPath = Vector3Adapter.ToSystemArray(unityPath.ToArray());
            _coreAgent?.SetPath(new List<SysVec3>(systemPath));
        }

        // Sync inspector settings to core agent
        private void SyncSettingsToCoreAgent()
        {
            if (_coreAgent == null) return;
            _coreAgent.Speed = _speed;
            _coreAgent.StoppingDistance = _stoppingDistance;
            _coreAgent.PathReplanTime = _pathReplanTime;
            _coreAgent.PathfindingOptions = _pathfindingOptions ?? PathfindingOptions.Default();
        }

        // Example handler for path results (optional)
        private void HandlePathResult(PathResult result)
        {
            if (!result.IsPathFound)
            {
                UnityEngine.Debug.LogWarning($"Pathfinding failed for {gameObject.name}: {result.ErrorMessage} ({result.Status})", this);
            }
            else
            {
                 UnityEngine.Debug.Log($"Path found for {gameObject.name}. Waypoints: {result.Waypoints.Count}", this);
            }
        }

        void OnDrawGizmos()
        {
            if (!_drawGizmos || _coreAgent == null || _coreAgent.Path == null || !Application.isPlaying) return;

            var path = _coreAgent.Path;
            if (path.Count > 1)
            {
                Gizmos.color = _pathColor;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Vector3 p1 = Vector3Adapter.ToUnity(path[i]);
                    Vector3 p2 = Vector3Adapter.ToUnity(path[i + 1]);
                    Gizmos.DrawLine(p1 + Vector3.up * 0.1f, p2 + Vector3.up * 0.1f); // Draw slightly above ground
                    Gizmos.DrawSphere(p2 + Vector3.up * 0.1f, _waypointRadius);
                }
                 Gizmos.DrawSphere(Vector3Adapter.ToUnity(path[0]) + Vector3.up * 0.1f, _waypointRadius); // Draw start waypoint
            }
        }
    }
}
#endif 