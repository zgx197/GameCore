using UnityEngine;
using System.Numerics;
using GameCore.GameSystems.Navigation.Grids;
using GameCore.Unity.Adapters;

namespace GameCore.Unity.Navigation
{
    /// <summary>
    /// Unity环境下的网格节点实现
    /// </summary>
    public class UnityGridNode : IGridNode
    {
        public Vector3Int Position { get; }
        public bool Walkable { get; set; }
        public float Cost { get; set; }
        public float Height { get; set; }
        
        public UnityGridNode(Vector3Int position, bool walkable)
        {
            Position = position;
            Walkable = walkable;
            Cost = 1f;
            Height = 0f;
        }
        
        public System.Numerics.Vector3 WorldPosition => Vector3Adapter.ToSystem(new UnityEngine.Vector3(Position.x, Height, Position.z));
    }
} 