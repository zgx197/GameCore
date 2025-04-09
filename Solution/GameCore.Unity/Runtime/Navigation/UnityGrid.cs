using UnityEngine;
using System.Collections.Generic;
using SysVec3 = System.Numerics.Vector3;
using UnityVec3 = UnityEngine.Vector3;
using GameCore.GameSystems.Navigation.Grids;
using GameCore.GameSystems.Navigation.Pathfinding;
using GameCore.Unity.Adapters;

namespace GameCore.Unity.Navigation
{
    /// <summary>
    /// Unity环境下的网格实现
    /// </summary>
    public class UnityGrid : MonoBehaviour, IGrid
    {
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private int _gridWidth = 10;
        [SerializeField] private int _gridDepth = 10;
        
        private Dictionary<Vector3Int, UnityGridNode> _nodes = new Dictionary<Vector3Int, UnityGridNode>();
        
        public float CellSize => _cellSize;
        
        private void Awake()
        {
            InitializeGrid();
        }
        
        private void InitializeGrid()
        {
            _nodes.Clear();
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridDepth; z++)
                {
                    var position = new Vector3Int(x, 0, z);
                    _nodes[position] = new UnityGridNode(position, true);
                }
            }
        }
        
        public IGridNode GetNodeFromWorldPosition(SysVec3 worldPosition)
        {
            var gridPosition = WorldToGridPosition(Vector3Adapter.ToUnity(worldPosition));
            return _nodes.TryGetValue(gridPosition, out var node) ? node : null;
        }
        
        public IGridNode GetNode(int x, int z)
        {
            var position = new Vector3Int(x, 0, z);
            return _nodes.TryGetValue(position, out var node) ? node : null;
        }
        
        public List<IGridNode> GetNeighbors(IGridNode node, PathfindingOptions options)
        {
            var unityNode = node as UnityGridNode;
            if (unityNode == null) return new List<IGridNode>();
            
            var neighbors = new List<IGridNode>();
            var directions = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 0, 1),
                new Vector3Int(0, 0, -1)
            };
            
            foreach (var direction in directions)
            {
                var neighborPos = unityNode.Position + direction;
                if (_nodes.TryGetValue(neighborPos, out var neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }
        
        public float GetMovementCost(IGridNode fromNode, IGridNode toNode, PathfindingOptions options)
        {
            return toNode.Cost;
        }
        
        public float GetHeuristicCost(IGridNode node, IGridNode targetNode, PathfindingOptions options)
        {
            var dx = UnityEngine.Mathf.Abs(node.WorldPosition.X - targetNode.WorldPosition.X);
            var dz = UnityEngine.Mathf.Abs(node.WorldPosition.Z - targetNode.WorldPosition.Z);
            return dx + dz;
        }
        
        public IGridNode FindNearestWalkableNode(IGridNode node)
        {
            if (node.Walkable) return node;
            
            var queue = new Queue<IGridNode>();
            var visited = new HashSet<IGridNode>();
            queue.Enqueue(node);
            visited.Add(node);
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Walkable) return current;
                
                foreach (var neighbor in GetNeighbors(current, new PathfindingOptions()))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            
            return null;
        }
        
        public void SetAreaWalkable(SysVec3 center, float radius, bool walkable)
        {
            var unityCenter = Vector3Adapter.ToUnity(center);
            foreach (var node in _nodes.Values)
            {
                var worldPos = Vector3Adapter.ToUnity(node.WorldPosition);
                if (UnityEngine.Vector3.Distance(worldPos, unityCenter) <= radius)
                {
                    node.Walkable = walkable;
                }
            }
        }
        
        public void SetRectWalkable(SysVec3 min, SysVec3 max, bool walkable)
        {
            var unityMin = Vector3Adapter.ToUnity(min);
            var unityMax = Vector3Adapter.ToUnity(max);
            foreach (var node in _nodes.Values)
            {
                var worldPos = Vector3Adapter.ToUnity(node.WorldPosition);
                if (worldPos.x >= unityMin.x && worldPos.x <= unityMax.x &&
                    worldPos.z >= unityMin.z && worldPos.z <= unityMax.z)
                {
                    node.Walkable = walkable;
                }
            }
        }
        
        public void ResetGridWalkability()
        {
            foreach (var node in _nodes.Values)
            {
                node.Walkable = true;
            }
        }
        
        public void UpdateGridHeights(float[] heightMap, int mapWidth, int mapDepth)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapDepth; z++)
                {
                    var position = new Vector3Int(x, 0, z);
                    if (_nodes.TryGetValue(position, out var node))
                    {
                        node.Height = heightMap[x + z * mapWidth];
                    }
                }
            }
        }
        
        public IEnumerable<IGridNode> GetAllNodes()
        {
            return _nodes.Values;
        }
        
        private Vector3Int WorldToGridPosition(UnityVec3 worldPosition)
        {
            var localPosition = transform.InverseTransformPoint(worldPosition);
            return new Vector3Int(
                UnityEngine.Mathf.RoundToInt(localPosition.x / _cellSize),
                0,
                UnityEngine.Mathf.RoundToInt(localPosition.z / _cellSize)
            );
        }
    }
} 