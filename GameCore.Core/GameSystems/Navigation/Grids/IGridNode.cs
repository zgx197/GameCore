using System.Numerics;

namespace GameCore.GameSystems.Navigation.Grids
{
    /// <summary>
    /// Interface representing a node within a navigation grid.
    /// Defines the common properties required for pathfinding.
    /// </summary>
    public interface IGridNode
    {
        /// <summary>
        /// The world position of this node.
        /// The Y component should represent the actual height for pathfinding calculations.
        /// </summary>
        Vector3 WorldPosition { get; }

        /// <summary>
        /// Indicates whether this node is walkable.
        /// Pathfinding algorithms will typically ignore non-walkable nodes.
        /// </summary>
        bool Walkable { get; set; }

        /// <summary>
        /// The inherent cost of traversing this node.
        /// Can be used to represent different terrain types (e.g., road vs. swamp).
        /// A standard cost is 1.0.
        /// </summary>
        float Cost { get; set; }

        /// <summary>
        /// The height (Y-coordinate) of the node in world space.
        /// Used for calculating slope limits and vertical distance heuristics.
        /// </summary>
        float Height { get; set; }
    }
} 