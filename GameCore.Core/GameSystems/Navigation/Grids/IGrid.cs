using System.Collections.Generic;
using System.Numerics;

namespace GameCore.GameSystems.Navigation.Grids
{
    /// <summary>
    /// Interface representing a navigation grid system.
    /// Defines the common methods required by pathfinding algorithms 
    /// to interact with different types of grids (e.g., Square, Hex).
    /// </summary>
    public interface IGrid
    {
        /// <summary>
        /// Gets the grid node corresponding to the given world position.
        /// </summary>
        /// <param name="worldPosition">The world position to query.</param>
        /// <returns>The corresponding IGridNode, or null if the position is outside the grid or invalid.</returns>
        IGridNode? GetNodeFromWorldPosition(Vector3 worldPosition);

        /// <summary>
        /// Gets the list of neighboring nodes for a given node.
        /// The definition of "neighbor" depends on the grid type (e.g., 4/8 for square, 6 for hex).
        /// </summary>
        /// <param name="node">The node for which to find neighbors.</param>
        /// <param name="options">Pathfinding options that might influence neighbor selection (e.g., diagonal movement).</param>
        /// <returns>A list of neighboring IGridNode instances.</returns>
        List<IGridNode> GetNeighbors(IGridNode node, Pathfinding.PathfindingOptions options);

        /// <summary>
        /// Calculates the movement cost between two adjacent nodes.
        /// This should consider distance, node traversal costs, and potentially height differences.
        /// </summary>
        /// <param name="fromNode">The starting node.</param>
        /// <param name="toNode">The destination node.</param>
        /// <param name="options">Pathfinding options that might influence the cost.</param>
        /// <returns>The calculated cost of moving between the nodes.</returns>
        float GetMovementCost(IGridNode fromNode, IGridNode toNode, Pathfinding.PathfindingOptions options);

        /// <summary>
        /// Calculates the heuristic estimate (H cost) from one node to a target node.
        /// This is used by algorithms like A* to estimate the remaining distance.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <param name="targetNode">The target node.</param>
        /// <param name="options">Pathfinding options that might influence the heuristic.</param>
        /// <returns>The estimated heuristic cost.</returns>
        float GetHeuristicCost(IGridNode node, IGridNode targetNode, Pathfinding.PathfindingOptions options);
        
        /// <summary>
        /// Finds the closest walkable node to a given potentially non-walkable node.
        /// </summary>
        /// <param name="node">The starting node (might be non-walkable).</param>
        /// <returns>The nearest walkable IGridNode, or null if none is found within a reasonable search range.</returns>
        IGridNode? FindNearestWalkableNode(IGridNode node);

        /// <summary>
        /// Updates the walkability of nodes within a circular area.
        /// </summary>
        /// <param name="center">The center of the area in world coordinates.</param>
        /// <param name="radius">The radius of the circular area.</param>
        /// <param name="walkable">The new walkability status to set.</param>
        void SetAreaWalkable(Vector3 center, float radius, bool walkable);

        /// <summary>
        /// Updates the walkability of nodes within a rectangular area.
        /// </summary>
        /// <param name="min">The minimum corner (e.g., bottom-left) of the rectangle in world coordinates.</param>
        /// <param name="max">The maximum corner (e.g., top-right) of the rectangle in world coordinates.</param>
        /// <param name="walkable">The new walkability status to set.</param>
        void SetRectWalkable(Vector3 min, Vector3 max, bool walkable);
        
        /// <summary>
        /// Resets the entire grid, typically making all valid nodes walkable.
        /// </summary>
        void ResetGridWalkability();

        /// <summary>
        /// Updates the height of grid nodes based on provided height map data.
        /// The interpretation of heightMap depends on the grid implementation.
        /// </summary>
        /// <param name="heightMap">An array or structure containing height data.</param>
        /// <param name="mapWidth">The width dimension of the height map.</param>
        /// <param name="mapDepth">The depth dimension of the height map.</param>
        void UpdateGridHeights(float[] heightMap, int mapWidth, int mapDepth);
    }
} 