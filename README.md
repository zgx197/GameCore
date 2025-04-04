# GameCore

跨引擎游戏开发核心框架 - 提供高性能、引擎无关的游戏系统实现。

## 特点

- 纯C#实现，完全引擎无关
- 同时支持Unity和Godot
- 高性能六边形网格系统
- 可扩展的模块化设计

## 快速开始

### Unity集成

1. 将GameCore添加到你的Unity项目:
   - 复制`GameCore.Core.dll`和`GameCore.Unity.dll`到你的`Assets/Plugins`文件夹

2. 基本使用:
```csharp
using GameCore.HexGrid;
using GameCore.Unity.Adapters;

// 创建六边形坐标
var hexCoord = new HexCoord(3, 4);

// 转换为Unity坐标
Vector3 worldPos = UnityVectorAdapter.HexToWorld(hexCoord);
```

### Godot集成

详见Godot集成文档

## 构建和开发

- `build.ps1`: 构建所有项目
- `integrate-unity.ps1`: 集成到Unity示例项目

## 许可证

Apache-2.0
