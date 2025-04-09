#if UNITY
using UnityEngine;
using GameCore.GameSystems.Navigation.Grids;
using GameCore.GameSystems.Navigation.Pathfinding;
using GameCore.Unity.Adapters;

namespace GameCore.Unity.Navigation
{
    /// <summary>
    /// 寻路系统测试控制器
    /// </summary>
    public class NavigationTestController : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private UnityNavigationAgent _testAgent;
        [SerializeField] private bool _drawGrid = true;
        [SerializeField] private bool _drawPath = true;
        [SerializeField] private Color _gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color _pathColor = Color.yellow;
        [SerializeField] private float _gridLineWidth = 0.1f;
        [SerializeField] private float _pathLineWidth = 0.2f;
        
        [Header("测试目标")]
        [SerializeField] private Transform _targetPoint;
        
        private IGrid _grid;
        private PathResult _currentPath;

        private void Start()
        {
            if (_testAgent != null)
            {
                _grid = _testAgent.Grid;
                // 测试移动到随机点
                var randomPoint = new Vector3(
                    Random.Range(-10f, 10f),
                    0f,
                    Random.Range(-10f, 10f)
                );
                _testAgent.MoveTo(randomPoint);
            }
        }
        
        private void Update()
        {
            // 点击测试
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (_testAgent != null)
                    {
                        _testAgent.MoveTo(hit.point);
                    }
                }
            }
            
            // 空格键停止
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _testAgent.Stop();
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _grid == null)
                return;

            if (_drawGrid)
            {
                Gizmos.color = _gridColor;
                foreach (var node in _grid.GetAllNodes())
                {
                    var worldPos = Vector3Adapter.ToUnity(node.WorldPosition);
                    var size = _grid.CellSize;
                    Gizmos.DrawWireCube(worldPos, new Vector3(size, 0.1f, size));
                }
            }

            if (_drawPath && _currentPath != null && _currentPath.IsPathFound)
            {
                Gizmos.color = _pathColor;
                for (int i = 0; i < _currentPath.Waypoints.Count - 1; i++)
                {
                    var start = Vector3Adapter.ToUnity(_currentPath.Waypoints[i]);
                    var end = Vector3Adapter.ToUnity(_currentPath.Waypoints[i + 1]);
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}
#endif 