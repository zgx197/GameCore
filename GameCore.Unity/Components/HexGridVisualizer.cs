using UnityEngine;
using GameCore.HexGrid;
using GameCore.Unity.Adapters;

namespace GameCore.Unity.Components
{
    /// <summary>
    /// 六边形网格可视化组件
    /// </summary>
    public class HexGridVisualizer : MonoBehaviour
    {
        [SerializeField] private float hexSize = 1.0f;
        [SerializeField] private int gridRadius = 5;
        [SerializeField] private GameObject? hexPrefab;
        
        private void Start()
        {
            CreateGrid();
        }
        
        private void CreateGrid()
        {
            for (int q = -gridRadius; q <= gridRadius; q++)
            {
                int r1 = Mathf.Max(-gridRadius, -q - gridRadius);
                int r2 = Mathf.Min(gridRadius, -q + gridRadius);
                
                for (int r = r1; r <= r2; r++)
                {
                    var hexCoord = new HexCoord(q, r);
                    Vector3 position = UnityVectorAdapter.HexToWorld(hexCoord, hexSize);
                    
                    if (hexPrefab != null)
                    {
                        GameObject hexTile = Instantiate(hexPrefab, position, Quaternion.identity, transform);
                        hexTile.name = $"Hex_{q}_{r}";
                    }
                }
            }
        }
    }
}