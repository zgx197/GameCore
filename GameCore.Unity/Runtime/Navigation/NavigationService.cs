#if UNITY
using UnityEngine;
using GameCore.GameSystems.Navigation;
using GameCore.GameSystems.Navigation.Grids;
using System;
using System.Diagnostics;
using SysVec3 = System.Numerics.Vector3;

namespace GameCore.Unity.Navigation
{
    /// <summary>
    /// Manages the core NavigationSystem within the Unity environment.
    /// Provides access to the system and handles grid generation based on the scene.
    /// </summary>
    [AddComponentMenu("GameCore/Navigation/Navigation Service")]
    public class NavigationService : MonoBehaviour
    {
        public static NavigationService Instance { get; private set; }

        [Header("Grid Settings")]
        [Tooltip("The width of the grid in number of cells.")]
        [SerializeField] private int _gridWidth = 100;
        [Tooltip("The depth of the grid in number of cells.")]
        [SerializeField] private int _gridDepth = 100;
        [Tooltip("The size of each square grid cell in world units.")]
        [SerializeField] private float _cellSize = 1.0f;
        [Tooltip("The world position corresponding to the bottom-left corner of the grid (cell 0,0).")]
        [SerializeField] private Vector3 _gridOrigin = new Vector3(-50, 0, -50);
        
        [Header("Grid Baking Settings")]
        [Tooltip("Layers considered walkable ground.")]
        [SerializeField] private LayerMask _walkableLayers = 1; // Default layer
        [Tooltip("Maximum height difference between adjacent cells to be considered walkable.")]
        [SerializeField] private float _maxStepHeight = 0.4f;
        [Tooltip("Maximum slope angle (in degrees) considered walkable.")]
        [SerializeField] private float _maxSlopeAngle = 45.0f;
        [Tooltip("How high above the cell center to start the ground check raycast.")]
        [SerializeField] private float _groundCheckRayOffsetY = 1.0f;
        [Tooltip("How far down from the offset to cast the ground check ray.")]
        [SerializeField] private float _groundCheckRayDistance = 2.0f;
        [Tooltip("Radius of the sphere cast used to check for obstructions near the cell center.")]
        [SerializeField] private float _obstructionCheckRadius = 0.4f;
        [Tooltip("Layers considered obstacles.")]
        [SerializeField] private LayerMask _obstacleLayers = 0;

        private NavigationSystem _navigationSystem;
        public NavigationSystem System => _navigationSystem;

        private IGrid _grid; // Keep a direct reference to the grid for baking

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                UnityEngine.Debug.LogWarning("Multiple NavigationService instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeNavigationSystem();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            // Potentially add cleanup for the navigation system if needed
        }

        private void InitializeNavigationSystem()
        {
            // Currently hardcoded to SquareGrid, could be made configurable
            _grid = new SquareGrid(_gridWidth, _gridDepth, _cellSize, Adapters.Vector3Adapter.ToSystemNumerics(_gridOrigin));
            _navigationSystem = new NavigationSystem();
            _navigationSystem.Initialize(_grid); 
            UnityEngine.Debug.Log($"NavigationService initialized with {_gridWidth}x{_gridDepth} SquareGrid.");
        }

        /// <summary>
        /// Generates or updates the navigation grid based on the current scene geometry and settings.
        /// This can be a potentially expensive operation.
        /// </summary>
        [ContextMenu("Bake Navigation Grid")]
        public void BakeGrid()
        {
            if (_grid == null || !(_grid is SquareGrid squareGrid))
            {
                UnityEngine.Debug.LogError("Cannot Bake Grid: Grid is not initialized or is not a SquareGrid.");
                return;
            }

            UnityEngine.Debug.Log("Starting Navigation Grid Bake...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            int width = squareGrid.Width;
            int depth = squareGrid.Depth;
            float cellSize = squareGrid.CellSize;
            SysVec3 origin = squareGrid.Origin;
            Vector3 unityOrigin = Adapters.Vector3Adapter.ToUnity(origin); // Unity version of origin for calculations
            float halfCell = cellSize * 0.5f;
            float maxSlopeCos = UnityEngine.Mathf.Cos(_maxSlopeAngle * UnityEngine.Mathf.Deg2Rad);

            int bakedNodes = 0;
            int unwalkableNodes = 0;

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // Calculate world position for the center of the cell
                    Vector3 cellCenter = new Vector3(
                        _gridOrigin.x + x * cellSize + halfCell,
                        _gridOrigin.y + _groundCheckRayOffsetY, // Start raycast high up
                        _gridOrigin.z + z * cellSize + halfCell
                    );

                    bool isWalkable = false;
                    float groundHeight = _gridOrigin.y; // Default height if no ground found

                    RaycastHit hitInfo;
                    if (Physics.Raycast(cellCenter, Vector3.down, out hitInfo, _groundCheckRayDistance, _walkableLayers))
                    {
                        // Ground hit. Check slope.
                        if (hitInfo.normal.y >= maxSlopeCos)
                        {
                            // Slope is acceptable. Check for obstructions near the hit point.
                            groundHeight = hitInfo.point.y;
                            Vector3 checkPos = hitInfo.point + Vector3.up * _obstructionCheckRadius; // Check slightly above ground
                            if (!Physics.CheckSphere(checkPos, _obstructionCheckRadius, _obstacleLayers))
                            {
                                // No obstructions found
                                isWalkable = true;
                            }
                        }
                    }

                    // Update the core grid node (if it exists - SquareGrid initializes all)
                    var node = squareGrid.GetNode(x, z);
                    if (node != null)
                    {
                        node.Walkable = isWalkable;
                        node.Height = groundHeight;
                        bakedNodes++;
                        if (!isWalkable) unwalkableNodes++;
                    }
                }
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"Navigation Grid Bake complete. Baked {bakedNodes} nodes ({unwalkableNodes} unwalkable). Time: {stopwatch.ElapsedMilliseconds}ms");

            // Optional: Implement neighbor walkability checks (e.g., based on maxStepHeight)
            // This would be a second pass after initial walkability/height is set.
            // CheckStepHeight(squareGrid);
        }

        // Optional second pass for step height check
        /*
        private void CheckStepHeight(SquareGrid squareGrid)
        {
            Debug.Log("Checking step heights...");
            for (int x = 0; x < squareGrid.Width; x++)
            {
                for (int z = 0; z < squareGrid.Depth; z++)
                {
                    var node = squareGrid.GetNode(x, z);
                    if (node == null || !node.Walkable) continue;

                    var neighbors = squareGrid.GetNeighbors(node, new PathfindingOptions { AllowDiagonalMovement = true }); // Check all 8 neighbors
                    foreach (var neighborNode in neighbors)
                    {
                        if (!neighborNode.Walkable) continue;

                        if (Mathf.Abs(node.Height - neighborNode.Height) > _maxStepHeight)
                        {
                             // Mark connection (or node) as unwalkable due to step height
                             // This requires more complex logic - either modify node costs dynamically,
                             // or store connection validity separately, or mark nodes involved.
                             // For simplicity, could mark the higher node as unwalkable if step is too large:
                             // if(neighborNode.Height > node.Height) neighborNode.Walkable = false;
                             // else node.Walkable = false; 
                             // A better approach involves modifying GetNeighbors or GetMovementCost in SquareGrid
                        }
                    }
                }
            }
            Debug.Log("Step height check finished.");
        }
        */

        // --- Gizmos for Visualization ---
        void OnDrawGizmosSelected()
        {
            if (!(_grid is SquareGrid squareGrid))
            {
                return; // Only draw gizmos if we have a square grid
            }

            float cellSize = squareGrid.CellSize;
            Vector3 size = new Vector3(cellSize, 0.1f, cellSize);
            Color walkableColor = new Color(0, 1, 0, 0.3f);
            Color unwalkableColor = new Color(1, 0, 0, 0.3f);

            for (int x = 0; x < squareGrid.Width; x++)
            {
                for (int z = 0; z < squareGrid.Depth; z++)
                {
                    var node = squareGrid.GetNode(x, z);
                    if (node != null)
                    {
                        Gizmos.color = node.Walkable ? walkableColor : unwalkableColor;
                        // Draw slightly above the node's height
                        Vector3 pos = Adapters.Vector3Adapter.ToUnity(node.WorldPosition) + Vector3.up * 0.05f; 
                        Gizmos.DrawCube(pos, size);
                    }
                }
            }
        }
    }
}
#endif 