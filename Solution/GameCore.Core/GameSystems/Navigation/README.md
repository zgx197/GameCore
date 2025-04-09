# 导航系统（Navigation System）

这是一个基于网格的导航系统，提供寻路和导航功能。系统使用A*算法实现高效的路径查找，支持不同的地形类型和动态障碍物。

## 特性

- 基于网格的导航（NavGrid）
- A*寻路算法实现
- 支持异步路径查找
- 自定义寻路选项
- 路径平滑和简化
- 动态障碍物支持
- 高度差异处理

## 目录结构

```
GameCore.Core/GameSystems/Navigation/
├── Components/            # 导航组件
│   ├── NavigationAgent.cs # 导航代理
│   └── NavigationObstacle.cs # 导航障碍物
├── Grids/                 # 网格实现
│   └── NavGrid.cs         # 导航网格
├── Interfaces/            # 接口定义
│   └── IPathfinder.cs     # 寻路器接口
├── Pathfinding/           # 寻路算法
│   ├── AStarPathfinder.cs # A*寻路实现
│   ├── PathResult.cs      # 寻路结果
│   └── PathfindingOptions.cs # 寻路选项
├── Examples/              # 使用示例
│   └── NavigationExample.cs # 示例代码
├── NavigationSystem.cs    # 导航系统主类
└── README.md              # 文档说明
```

## 快速入门

### 创建导航系统

```csharp
// 创建并初始化导航系统
NavigationSystem navSystem = new NavigationSystem();
navSystem.Initialize(100, 100, 1.0f, new Vector3(-50, 0, -50));
```

### 使用导航代理

```csharp
// 创建导航代理
NavigationAgent agent = new NavigationAgent(navSystem);
agent.Position = new Vector3(0, 0, 0);
agent.Speed = 3.0f;

// 请求移动到目标位置
agent.MoveTo(new Vector3(10, 0, 10));

// 在游戏循环中更新代理
void Update(float deltaTime)
{
    agent.Update(deltaTime);
    
    if (agent.HasReachedDestination)
    {
        Console.WriteLine("到达目标位置！");
    }
}
```

### 添加障碍物

```csharp
// 添加圆形障碍物
NavigationObstacle obstacle1 = new NavigationObstacle(navSystem, new Vector3(5, 0, 5), 2.0f);

// 添加矩形障碍物
NavigationObstacle obstacle2 = new NavigationObstacle(navSystem, new Vector3(-5, 0, -5), new Vector3(3, 1, 4));
```

### 直接寻路

```csharp
// 使用默认选项寻找路径
PathResult result = navSystem.FindPath(new Vector3(0, 0, 0), new Vector3(10, 0, 10));

if (result.IsPathFound)
{
    Console.WriteLine($"找到路径，长度: {result.TotalLength}，路径点数: {result.Waypoints.Count}");
    
    // 使用路径...
    foreach (Vector3 waypoint in result.Waypoints)
    {
        Console.WriteLine($"路径点: ({waypoint.X}, {waypoint.Y}, {waypoint.Z})");
    }
}
else
{
    Console.WriteLine($"无法找到路径: {result.ErrorMessage}");
}
```

### 异步寻路

```csharp
// 异步寻找路径
async Task FindPathAsync()
{
    PathResult result = await navSystem.FindPathAsync(
        new Vector3(0, 0, 0), 
        new Vector3(10, 0, 10), 
        PathfindingOptions.HighQuality());
        
    // 处理结果...
}
```

### 自定义寻路选项

```csharp
// 创建自定义寻路选项
PathfindingOptions options = new PathfindingOptions
{
    SmoothingFactor = 0.5f,
    SimplificationTolerance = 0.2f,
    TimeoutMs = 1000,
    MaxNodes = 20000,
    AllowDiagonalMovement = true,
    MaxHeightDifference = 1.0f
};

// 使用自定义选项寻路
PathResult result = navSystem.FindPath(start, end, options);
```

## 高级功能

### 高度图更新

```csharp
// 创建高度图数据
float[] heightMap = new float[100 * 100];
// 填充高度数据...

// 更新导航网格高度
navSystem.UpdateFromHeightMap(heightMap);
```

### 区域标记

```csharp
// 将圆形区域标记为不可行走
navSystem.SetAreaWalkable(new Vector3(0, 0, 0), 5.0f, false);

// 将矩形区域标记为可行走
navSystem.SetRectWalkable(
    new Vector3(-10, 0, -10), 
    new Vector3(10, 0, 10), 
    true);
```

### 路径回调

```csharp
// 设置路径完成回调
agent.SetPathCompleteCallback(pathResult => {
    if (pathResult.IsPathFound)
    {
        Console.WriteLine("路径找到！");
    }
    else
    {
        Console.WriteLine($"寻路失败: {pathResult.ErrorMessage}");
    }
});
```

## 性能考虑

- 网格大小和单元格尺寸会影响性能和精度
- 使用 `PathfindingOptions.HighPerformance()` 可以提高性能（但可能降低路径质量）
- 对于大型场景，考虑使用异步寻路以避免阻塞主线程
- 适当调整 `TimeoutMs` 和 `MaxNodes` 参数以防止长时间搜索

## 扩展

系统设计支持扩展，可以通过以下方式自定义：

1. 实现自己的 `IPathfinder` 接口来创建新的寻路算法
2. 继承 `NavGrid` 来支持更复杂的网格结构
3. 创建自定义组件与导航系统交互 