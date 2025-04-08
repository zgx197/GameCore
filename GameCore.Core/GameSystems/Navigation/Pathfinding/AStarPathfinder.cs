using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using GameCore.GameSystems.Navigation.Grids;
using GameCore.GameSystems.Navigation.Interfaces;

namespace GameCore.GameSystems.Navigation.Pathfinding
{
    /// <summary>
    /// A pathfinder implementation using the A* algorithm.
    /// Works with any grid system implementing the IGrid interface.
    /// </summary>
    public class AStarPathfinder : IPathfinder
    {
        private readonly IGrid _grid;
        
        /// <summary>
        /// Internal helper node class for the A* algorithm.
        /// </summary>
        private class PathNode : IComparable<PathNode>
        {
            public IGridNode GridNode;
            public float GCost;      
            public float HCost;      
            public float FCost => GCost + HCost;
            public PathNode Parent;
            
            public int CompareTo(PathNode other)
            {
                int comparison = FCost.CompareTo(other.FCost);
                if (comparison == 0)
                {
                    comparison = HCost.CompareTo(other.HCost);
                }
                return comparison;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the AStarPathfinder class.
        /// </summary>
        /// <param name="grid">The navigation grid (must implement IGrid).</param>
        public AStarPathfinder(IGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
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
            options ??= PathfindingOptions.Default();
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            // Use IGrid interface to get nodes
            IGridNode? startNode = _grid.GetNodeFromWorldPosition(start);
            IGridNode? endNode = _grid.GetNodeFromWorldPosition(end);
            
            // Validation checks
            if (startNode == null)
            {
                stopwatch.Stop();
                return PathResult.Failure(PathfindingStatus.InvalidStart, "Invalid start position (outside grid?)", stopwatch.ElapsedMilliseconds);
            }
            
            if (endNode == null)
            {
                stopwatch.Stop();
                return PathResult.Failure(PathfindingStatus.InvalidEnd, "Invalid end position (outside grid?)", stopwatch.ElapsedMilliseconds);
            }
            
            // Check start node walkability and potentially find nearest
            if (!startNode.Walkable)
            {
                if (options.FindNearestIfUnreachable)
                {
                    // Use IGrid interface to find nearest walkable
                    startNode = _grid.FindNearestWalkableNode(startNode);
                    if (startNode == null)
                    {
                        stopwatch.Stop();
                        return PathResult.Failure(PathfindingStatus.InvalidStart, "Start node is unwalkable and no nearby walkable node found.", stopwatch.ElapsedMilliseconds);
                    }
                }
                else
                {
                    stopwatch.Stop();
                    return PathResult.Failure(PathfindingStatus.InvalidStart, "Start node is not walkable.", stopwatch.ElapsedMilliseconds);
                }
            }
            
            // Check end node walkability and potentially find nearest
            if (!endNode.Walkable)
            {
                if (options.FindNearestIfUnreachable)
                {
                    // Use IGrid interface to find nearest walkable
                    endNode = _grid.FindNearestWalkableNode(endNode);
                    if (endNode == null)
                    {
                        stopwatch.Stop();
                        return PathResult.Failure(PathfindingStatus.InvalidEnd, "End node is unwalkable and no nearby walkable node found.", stopwatch.ElapsedMilliseconds);
                    }
                }
                else
                {
                    stopwatch.Stop();
                    return PathResult.Failure(PathfindingStatus.InvalidEnd, "End node is not walkable.", stopwatch.ElapsedMilliseconds);
                }
            }
            
            // Start and end are the same node
            if (startNode == endNode)
            {
                List<Vector3> path = new List<Vector3> { startNode.WorldPosition, endNode.WorldPosition };
                stopwatch.Stop();
                return PathResult.Success(path, 0, stopwatch.ElapsedMilliseconds);
            }
            
            // A* algorithm setup
            List<PathNode> openSet = new List<PathNode>();
            HashSet<IGridNode> closedSet = new HashSet<IGridNode>();
            Dictionary<IGridNode, PathNode> pathNodeLookup = new Dictionary<IGridNode, PathNode>();
            
            PathNode startPathNode = new PathNode
            {
                GridNode = startNode,
                GCost = 0,
                HCost = _grid.GetHeuristicCost(startNode, endNode, options)
            };
            openSet.Add(startPathNode);
            pathNodeLookup[startNode] = startPathNode;
            
            int nodesProcessed = 0;
            
            while (openSet.Count > 0)
            {
                // Timeout / Node limit check
                if (stopwatch.ElapsedMilliseconds > options.TimeoutMs || nodesProcessed > options.MaxNodes)
                {
                    stopwatch.Stop();
                    // Attempt to return best path found so far if timeout occurs
                    PathNode? bestNode = GetBestNodeInOpenSet(openSet);
                    if (bestNode != null && options.FindNearestIfUnreachable)
                    {
                        // If we time out but want nearest, return path to the best H-cost node
                        List<Vector3> partialPath = ReconstructPath(bestNode);
                        // Potentially smooth/simplify partial path if needed
                        float length = CalculatePathLength(partialPath);
                        return PathResult.Success(partialPath, length, stopwatch.ElapsedMilliseconds, PathfindingStatus.PartialPathFound);
                    }
                    
                    return PathResult.Failure(
                        PathfindingStatus.Timeout,
                        $"Pathfinding timed out or exceeded node limit ({nodesProcessed} nodes, {stopwatch.ElapsedMilliseconds}ms).",
                        stopwatch.ElapsedMilliseconds);
                }
                
                // Get node with lowest F cost
                openSet.Sort();
                PathNode currentNode = openSet[0];
                openSet.RemoveAt(0);
                
                // Path found
                if (currentNode.GridNode == endNode)
                {
                    List<Vector3> path = ReconstructPath(currentNode);
                    stopwatch.Stop();
                    
                    // Optional path post-processing
                    if (options.SmoothingFactor > 0) path = SmoothPath(path, options.SmoothingFactor);
                    if (options.SimplificationTolerance > 0) path = SimplifyPath(path, options.SimplificationTolerance);
                    
                    float pathLength = CalculatePathLength(path);
                    return PathResult.Success(path, pathLength, stopwatch.ElapsedMilliseconds);
                }
                
                // Move node to closed set
                closedSet.Add(currentNode.GridNode);
                nodesProcessed++;

                // Process neighbors
                List<IGridNode> neighbors = _grid.GetNeighbors(currentNode.GridNode, options);
                foreach (IGridNode neighbor in neighbors)
                {
                    // Skip non-walkable or already processed nodes
                    if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    
                    // Check height difference constraint (already implicitly handled in GetMovementCost by IGrid?)
                    // Explicit check remains useful if GetMovementCost doesn't guarantee infinite cost for invalid moves.
                    if (Math.Abs(neighbor.Height - currentNode.GridNode.Height) > options.MaxHeightDifference)
                    {
                        continue;
                    }
                    
                    // Calculate cost to reach neighbor through current node
                    float movementCostToNeighbor = _grid.GetMovementCost(currentNode.GridNode, neighbor, options);
                    float tentativeGCost = currentNode.GCost + movementCostToNeighbor;
                    
                    // Check if neighbor is in open set
                    if (!pathNodeLookup.TryGetValue(neighbor, out PathNode neighborPathNode))
                    {
                        neighborPathNode = new PathNode
                        {
                            GridNode = neighbor,
                            GCost = tentativeGCost,
                            HCost = _grid.GetHeuristicCost(neighbor, endNode, options),
                            Parent = currentNode
                        };
                        openSet.Add(neighborPathNode);
                        pathNodeLookup[neighbor] = neighborPathNode;
                    }
                    // If neighbor is in open set, check if this path is better
                    else if (tentativeGCost < neighborPathNode.GCost)
                    {
                        neighborPathNode.Parent = currentNode;
                        neighborPathNode.GCost = tentativeGCost;
                    }
                }
            }
            
            // No path found
            stopwatch.Stop();
            return PathResult.Failure(
                PathfindingStatus.NoPathFound,
                $"No path found after processing {nodesProcessed} nodes ({stopwatch.ElapsedMilliseconds}ms).",
                stopwatch.ElapsedMilliseconds);
        }
        
        /// <summary>
        /// 异步寻找从起点到终点的路径
        /// </summary>
        /// <param name="start">起点世界位置</param>
        /// <param name="end">终点世界位置</param>
        /// <param name="options">寻路选项</param>
        /// <returns>寻路结果任务</returns>
        public async Task<PathResult> FindPathAsync(Vector3 start, Vector3 end, PathfindingOptions options = null)
        {
            return await Task.Run(() => FindPath(start, end, options));
        }
        
        /// <summary>
        /// 检查位置是否可到达
        /// </summary>
        /// <param name="position">要检查的位置</param>
        /// <returns>如果位置可到达返回true，否则返回false</returns>
        public bool IsPositionWalkable(Vector3 position)
        {
            IGridNode? node = _grid.GetNodeFromWorldPosition(position);
            return node != null && node.Walkable;
        }
        
        /// <summary>
        /// 获取最接近的可行走位置
        /// </summary>
        /// <param name="position">参考位置</param>
        /// <returns>最接近的可行走位置</returns>
        public Vector3 GetClosestWalkablePosition(Vector3 position)
        {
            IGridNode? node = _grid.GetNodeFromWorldPosition(position);
            if (node == null) return position; // Outside grid
            if (node.Walkable) return node.WorldPosition; // Already walkable
            
            // Use IGrid interface method
            IGridNode? walkableNode = _grid.FindNearestWalkableNode(node);
            return walkableNode?.WorldPosition ?? position; // Return nearest or original if none found
        }
        
        /// <summary>
        /// Gets the node with the best (lowest) H cost from the open set.
        /// Used for finding the most promising node when a timeout occurs.
        /// </summary>
        private PathNode? GetBestNodeInOpenSet(List<PathNode> openSet)
        {
            if (openSet.Count == 0) return null;
            
            PathNode bestNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].HCost < bestNode.HCost)
                {
                    bestNode = openSet[i];
                }
            }
            return bestNode;
        }
        
        /// <summary>
        /// Reconstructs the path from the end node back to the start node.
        /// </summary>
        private List<Vector3> ReconstructPath(PathNode endNode)
        {
            List<Vector3> path = new List<Vector3>();
            PathNode? current = endNode;
            while (current != null)
            {
                path.Add(current.GridNode.WorldPosition);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }
        
        /// <summary>
        /// Calculates the total length of a path.
        /// </summary>
        private float CalculatePathLength(List<Vector3> path)
        {
            if (path.Count < 2) return 0;
            float length = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                length += Vector3.Distance(path[i], path[i + 1]);
            }
            return length;
        }
        
        /// <summary>
        /// Path smoothing (remains the same, operates on Vector3 list)
        /// </summary>
        private List<Vector3> SmoothPath(List<Vector3> path, float smoothingFactor)
        {
            if (path.Count < 3 || smoothingFactor <= 0) return path;
            
            List<Vector3> smoothedPath = new List<Vector3>(path);
            int iterations = 5; // Number of smoothing iterations

            for (int i = 0; i < iterations; i++)
            {
                for (int j = 1; j < smoothedPath.Count - 1; j++)
                {
                    Vector3 prev = smoothedPath[j - 1];
                    Vector3 current = smoothedPath[j];
                    Vector3 next = smoothedPath[j + 1];
                    
                    Vector3 target = Vector3.Lerp(prev, next, 0.5f); // Midpoint
                    smoothedPath[j] = Vector3.Lerp(current, target, smoothingFactor);
                }
            }
            return smoothedPath;
        }
        
        /// <summary>
        /// Path simplification (remains the same, operates on Vector3 list)
        /// Uses Ramer-Douglas-Peucker algorithm concept
        /// </summary>
        private List<Vector3> SimplifyPath(List<Vector3> path, float tolerance)
        {
            if (path.Count < 3 || tolerance <= 0) return path;
            
            List<Vector3> simplified = new List<Vector3>();
            simplified.Add(path[0]); // Always keep the start point

            SimplifyRecursive(path, 0, path.Count - 1, tolerance * tolerance, simplified);

            simplified.Add(path[path.Count - 1]); // Always keep the end point
            return simplified;
        }

        private void SimplifyRecursive(List<Vector3> points, int startIndex, int endIndex, float toleranceSqr, List<Vector3> simplified)
        {
            if (startIndex + 1 >= endIndex) return; // Base case: segment has only 2 points

            float maxDistSqr = 0;
            int index = startIndex;

            Vector3 start = points[startIndex];
            Vector3 end = points[endIndex];

            for (int i = startIndex + 1; i < endIndex; i++)
            {
                float distSqr = PerpendicularDistanceSqr(points[i], start, end);
                if (distSqr > maxDistSqr)
                {
                    maxDistSqr = distSqr;
                    index = i;
                }
            }

            // If max distance is greater than tolerance, recursively simplify
            if (maxDistSqr > toleranceSqr)
            {
                SimplifyRecursive(points, startIndex, index, toleranceSqr, simplified);
                simplified.Add(points[index]); // Add the significant point
                SimplifyRecursive(points, index, endIndex, toleranceSqr, simplified);
            }
            // Otherwise, the segment can be simplified (points between start and end are removed)
        }

        // Calculates squared perpendicular distance from a point to a line segment
        private float PerpendicularDistanceSqr(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineVec = lineEnd - lineStart;
            float lineLenSqr = lineVec.LengthSquared();
            if (lineLenSqr == 0.0f) return Vector3.DistanceSquared(point, lineStart);

            // Project point onto the line defined by lineStart and lineEnd
            float t = Vector3.Dot(point - lineStart, lineVec) / lineLenSqr;
            t = Math.Clamp(t, 0.0f, 1.0f); // Clamp to segment

            Vector3 projection = lineStart + t * lineVec;
            return Vector3.DistanceSquared(point, projection);
        }
    }
} 