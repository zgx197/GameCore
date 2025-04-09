using System;
using System.Collections.Generic;
using System.Numerics;
using GameCore.GameSystems.Navigation.Pathfinding;

namespace GameCore.GameSystems.Navigation.Grids
{
    /// <summary>
    /// Represents a navigation grid composed of square cells in a 3D space.
    /// Implements the IGrid interface for compatibility with pathfinders.
    /// </summary>
    public class SquareGrid : IGrid
    {
        /// <summary>
        /// Represents a single node (cell) within the square grid.
        /// Implements the IGridNode interface.
        /// </summary>
        public class Node : IGridNode
        {
            private Vector3 _basePosition;
            
            /// <summary>
            /// The node's index in the X-dimension of the grid array.
            /// </summary>
            public int X { get; }
            
            /// <summary>
            /// The node's index in the Z-dimension of the grid array.
            /// </summary>
            public int Z { get; }
            
            /// <summary>
            /// The node's world position, including its current height.
            /// </summary>
            public Vector3 WorldPosition 
            { 
                get { return new Vector3(_basePosition.X, Height, _basePosition.Z); }
            }
            
            /// <summary>
            /// Whether the node is walkable.
            /// </summary>
            public bool Walkable { get; set; }
            
            /// <summary>
            /// The cost of the node for pathfinding algorithms.
            /// </summary>
            public float Cost { get; set; } = 1.0f;
            
            /// <summary>
            /// The height of the node (Y coordinate).
            /// </summary>
            public float Height { get; set; }
            
            /// <summary>
            /// Initializes a new instance of the Node class.
            /// </summary>
            /// <param name="x">X coordinate</param>
            /// <param name="z">Z coordinate</param>
            /// <param name="worldPosition">World position</param>
            /// <param name="walkable">Whether the node is walkable</param>
            /// <param name="height">Height of the node</param>
            public Node(int x, int z, Vector3 worldPosition, bool walkable, float height)
            {
                X = x;
                Z = z;
                _basePosition = worldPosition;
                Walkable = walkable;
                Height = height;
            }
        }
        
        private readonly Node[,] _grid;
        private readonly Vector3 _origin;
        private readonly int _width;
        private readonly int _depth;
        private readonly float _cellSize;
        
        /// <summary>
        /// Gets the width of the grid (number of nodes in the X direction).
        /// </summary>
        public int Width => _width;
        
        /// <summary>
        /// Gets the depth of the grid (number of nodes in the Z direction).
        /// </summary>
        public int Depth => _depth;
        
        /// <summary>
        /// Gets the size of a grid cell.
        /// </summary>
        public float CellSize => _cellSize;
        
        /// <summary>
        /// Gets the origin (bottom-left corner) of the grid.
        /// </summary>
        public Vector3 Origin => _origin;
        
        /// <summary>
        /// Initializes a new instance of the SquareGrid class.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="depth">Depth of the grid</param>
        /// <param name="cellSize">Size of a grid cell</param>
        /// <param name="origin">Origin (bottom-left corner) of the grid</param>
        public SquareGrid(int width, int depth, float cellSize, Vector3 origin)
        {
            _width = Math.Max(1, width);
            _depth = Math.Max(1, depth);
            _cellSize = Math.Max(0.01f, cellSize);
            _origin = origin;
            _grid = new Node[_width, _depth];
            
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    Vector3 worldPos = new Vector3(
                        _origin.X + x * _cellSize + _cellSize * 0.5f,
                        _origin.Y, // Initial height is at origin's Y
                        _origin.Z + z * _cellSize + _cellSize * 0.5f);
                    
                    // Create the node, initially walkable with origin height
                    _grid[x, z] = new Node(x, z, worldPos, true, _origin.Y);
                }
            }
        }
        
        /// <summary>
        /// Gets the specific Node instance at the given grid coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>The Node, or null if coordinates are out of bounds.</returns>
        public Node? GetNode(int x, int z)
        {
            if (x >= 0 && x < _width && z >= 0 && z < _depth)
            {
                return _grid[x, z];
            }
            return null;
        }
        
        /// <summary>
        /// Gets the Node from a world position.
        /// </summary>
        /// <param name="worldPosition">World position</param>
        /// <returns>The Node, or null if the position is out of bounds.</returns>
        public IGridNode? GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.X - _origin.X) / _cellSize);
            int z = Mathf.FloorToInt((worldPosition.Z - _origin.Z) / _cellSize);
            
            return GetNode(x, z);
        }
        
        /// <summary>
        /// Gets all neighboring nodes of a given node.
        /// </summary>
        /// <param name="node">Center node</param>
        /// <param name="options">Pathfinding options</param>
        /// <returns>List of neighboring nodes</returns>
        public List<IGridNode> GetNeighbors(IGridNode node, PathfindingOptions options)
        {
            List<IGridNode> neighbors = new List<IGridNode>();
            if (!(node is Node squareNode)) // Ensure it's the correct node type for this grid
            {
                return neighbors; 
            }

            int x = squareNode.X;
            int z = squareNode.Z;

            // Add horizontal/vertical neighbors
            CheckAndAddNeighbor(neighbors, x + 1, z);
            CheckAndAddNeighbor(neighbors, x - 1, z);
            CheckAndAddNeighbor(neighbors, x, z + 1);
            CheckAndAddNeighbor(neighbors, x, z - 1);
            
            // Add diagonal neighbors if allowed
            if (options.AllowDiagonalMovement)
            {
                CheckAndAddNeighbor(neighbors, x + 1, z + 1);
                CheckAndAddNeighbor(neighbors, x - 1, z + 1);
                CheckAndAddNeighbor(neighbors, x + 1, z - 1);
                CheckAndAddNeighbor(neighbors, x - 1, z - 1);
            }
            
            return neighbors;
        }
        
        private void CheckAndAddNeighbor(List<IGridNode> neighbors, int x, int z)
        {
            Node? neighbor = GetNode(x, z);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        /// <summary>
        /// Gets the movement cost between two nodes.
        /// </summary>
        /// <param name="fromNode">Starting node</param>
        /// <param name="toNode">Ending node</param>
        /// <param name="options">Pathfinding options</param>
        /// <returns>Movement cost between the nodes</returns>
        public float GetMovementCost(IGridNode fromNode, IGridNode toNode, PathfindingOptions options)
        {
            float baseCost = Vector3.Distance(fromNode.WorldPosition, toNode.WorldPosition);
            float terrainCost = toNode.Cost; // Cost multiplier for the destination node's terrain
            float heightCost = Math.Abs(toNode.Height - fromNode.Height) * options.HeightWeight;

            // Ensure terrainCost isn't zero or negative, minimum cost is base distance
            return Math.Max(baseCost, baseCost * Math.Max(0.1f, terrainCost) + heightCost);
        }

        /// <summary>
        /// Gets the heuristic cost between two nodes.
        /// </summary>
        /// <param name="node">Starting node</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="options">Pathfinding options</param>
        /// <returns>Heuristic cost between the nodes</returns>
        public float GetHeuristicCost(IGridNode node, IGridNode targetNode, PathfindingOptions options)
        {
            // Using Euclidean distance in 3D as the heuristic
            float dx = Math.Abs(node.WorldPosition.X - targetNode.WorldPosition.X);
            float dz = Math.Abs(node.WorldPosition.Z - targetNode.WorldPosition.Z);
            float dy = Math.Abs(node.Height - targetNode.Height) * options.HeightWeight;
            
            // Using Euclidean distance, slightly cheaper than sqrt for comparison purposes often
            // Could use Manhattan distance (dx + dz + dy) for potentially faster but less accurate heuristic
            return (float)Math.Sqrt(dx * dx + dz * dz + dy * dy);
        }

        /// <summary>
        /// Finds the nearest walkable node to a given node.
        /// </summary>
        /// <param name="startNode">Starting node</param>
        /// <returns>The nearest walkable node, or null if no walkable node is found</returns>
        public IGridNode? FindNearestWalkableNode(IGridNode startNode)
        {
            if (!(startNode is Node squareStartNode) || startNode.Walkable)
            {
                return startNode; // Already walkable or wrong node type
            }

            Queue<Node> queue = new Queue<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            
            queue.Enqueue(squareStartNode);
            visited.Add(squareStartNode);
            
            int searchLimit = 100; // Limit search to prevent infinite loops in edge cases
            int searched = 0;

            while (queue.Count > 0 && searched < searchLimit)
            {
                Node current = queue.Dequeue();
                searched++;
                
                // Check neighbors (using the internal Node type)
                int x = current.X;
                int z = current.Z;

                // Check 8 directions for square grid
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {   
                        if (dx == 0 && dz == 0) continue; // Skip self
                        
                        Node? neighbor = GetNode(x + dx, z + dz);
                        
                        if (neighbor != null && !visited.Contains(neighbor))
                        {
                            if (neighbor.Walkable)
                            {
                                return neighbor; // Found the nearest walkable node
                            }
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
            
            return null; // No walkable node found within the search limit
        }
        
        /// <summary>
        /// Updates the walkability of a specific node.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="walkable">Whether the node is walkable</param>
        public void SetWalkable(int x, int z, bool walkable)
        {
            Node? node = GetNode(x, z);
            if (node != null)
            {
                node.Walkable = walkable;
            }
        }
        
        /// <summary>
        /// Updates the height of a specific node.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="height">Height value</param>
        public void SetHeight(int x, int z, float height)
        {
            Node? node = GetNode(x, z);
            if (node != null)
            {
                node.Height = height;
            }
        }
        
        /// <summary>
        /// Updates the cost of a specific node.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="cost">Cost value</param>
        public void SetCost(int x, int z, float cost)
        {
            Node? node = GetNode(x, z);
            if (node != null)
            {
                node.Cost = cost;
            }
        }

        /// <summary>
        /// Sets the walkability of an area around a given center.
        /// </summary>
        /// <param name="center">Center of the area</param>
        /// <param name="radius">Radius of the area</param>
        /// <param name="walkable">Whether the area is walkable</param>
        public void SetAreaWalkable(Vector3 center, float radius, bool walkable)
        {
            IGridNode? centerNode = GetNodeFromWorldPosition(center);
            if (!(centerNode is Node squareCenterNode))
            {
                return;
            }
            
            int nodeRadius = (int)Math.Ceiling(radius / _cellSize);
            int startX = Math.Max(0, squareCenterNode.X - nodeRadius);
            int endX = Math.Min(_width - 1, squareCenterNode.X + nodeRadius);
            int startZ = Math.Max(0, squareCenterNode.Z - nodeRadius);
            int endZ = Math.Min(_depth - 1, squareCenterNode.Z + nodeRadius);
            
            float radiusSqr = radius * radius;
            
            for (int x = startX; x <= endX; x++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    Node? node = GetNode(x, z);
                    if (node != null)
                    {
                        float dx = node.WorldPosition.X - center.X;
                        float dz = node.WorldPosition.Z - center.Z;
                        // Check distance in XZ plane
                        if (dx * dx + dz * dz <= radiusSqr)
                        {
                            node.Walkable = walkable;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the walkability of a rectangular area.
        /// </summary>
        /// <param name="min">Minimum corner of the rectangle</param>
        /// <param name="max">Maximum corner of the rectangle</param>
        /// <param name="walkable">Whether the area is walkable</param>
        public void SetRectWalkable(Vector3 min, Vector3 max, bool walkable)
        {
            IGridNode? minNode = GetNodeFromWorldPosition(min);
            IGridNode? maxNode = GetNodeFromWorldPosition(max);
            
            if (!(minNode is Node squareMinNode) || !(maxNode is Node squareMaxNode))
            {
                return;
            }
            
            // Ensure min/max are correctly ordered
            int startX = Math.Min(squareMinNode.X, squareMaxNode.X);
            int endX = Math.Max(squareMinNode.X, squareMaxNode.X);
            int startZ = Math.Min(squareMinNode.Z, squareMaxNode.Z);
            int endZ = Math.Max(squareMinNode.Z, squareMaxNode.Z);

            // Clamp to grid bounds
            startX = Math.Max(0, startX);
            endX = Math.Min(_width - 1, endX);
            startZ = Math.Max(0, startZ);
            endZ = Math.Min(_depth - 1, endZ);
            
            for (int x = startX; x <= endX; x++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    SetWalkable(x, z, walkable); // Use internal SetWalkable
                }
            }
        }

        /// <summary>
        /// Resets the walkability of the entire grid.
        /// </summary>
        public void ResetGridWalkability()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    if (_grid[x, z] != null)
                    {
                         _grid[x, z].Walkable = true; // Reset to walkable
                    }
                }
            }
        }

        /// <summary>
        /// Updates the heights of all nodes in the grid from a height map.
        /// </summary>
        /// <param name="heightMap">Height map data, length should be width*depth</param>
        /// <param name="mapWidth">Width of the height map</param>
        /// <param name="mapDepth">Depth of the height map</param>
        public void UpdateGridHeights(float[] heightMap, int mapWidth, int mapDepth)
        {
            if (heightMap.Length != mapWidth * mapDepth || mapWidth != _width || mapDepth != _depth)
            {
                // Consider throwing a more specific exception or logging a warning
                throw new ArgumentException("Height map dimensions do not match grid dimensions.");
            }
            
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    int index = z * mapWidth + x; // Standard row-major order assumed
                    float height = heightMap[index];
                    SetHeight(x, z, height); // Use internal SetHeight
                }
            }
        }
    }
    
    /// <summary>
    /// Math utility class (consider moving to a common utility namespace if used elsewhere)
    /// </summary>
    public static class Mathf
    {
        /// <summary>
        /// Converts a float to an integer by rounding down.
        /// </summary>
        /// <param name="value">Float value</param>
        /// <returns>Floored integer</returns>
        public static int FloorToInt(float value)
        {
            return (int)Math.Floor(value);
        }
    }
} 