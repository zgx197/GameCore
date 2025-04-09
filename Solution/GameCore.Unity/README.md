# GameCore Unity Adapter

Unity引擎适配器，用于GameCore框架的六边形网格系统。

## 安装方式

### 通过Git URL安装(推荐)
1. 打开Unity项目
2. 选择Window > Package Manager
3. 点击左上角的"+"按钮
4. 选择"Add package from git URL"
5. 输入 `https://github.com/zgx197/GameCore.git#upm`
6. 点击Add

### 通过手动下载
1. 从Releases下载最新版本
2. 解压到你的Unity项目的Packages文件夹

## 使用方法

```csharp
// 使用HexCoord
using GameCore.HexGrid;
using GameCore.Unity.Adapters;

// 创建六边形坐标
var hexCoord = new HexCoord(3, -2);

// 转换为Unity世界坐标
Vector3 worldPos = UnityVectorAdapter.HexToWorld(hexCoord, 1.0f);
```
```