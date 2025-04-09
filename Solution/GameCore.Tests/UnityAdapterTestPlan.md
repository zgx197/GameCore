# Unity适配器测试计划

由于Unity测试需要在Unity编辑器环境中运行，以下是一个手动测试计划，用于验证GameCore与Unity的集成。

## 1. 基本设置测试

### 测试步骤:
1. 创建一个新的Unity场景
2. 添加一个空游戏对象，命名为"GameCoreAdapter"
3. 将`UnityWorldAdapter`组件添加到此游戏对象
4. 运行场景
5. 检查控制台是否有错误

### 预期结果:
- 适配器初始化没有错误
- `GameCore.World`成功创建
- 控制台没有显示异常

## 2. 实体同步测试

### 测试步骤:
1. 创建一个新的Unity场景
2. 添加UnityWorldAdapter组件到场景中
3. 创建3个立方体游戏对象
4. 为每个立方体添加`EntityBehaviour`组件
5. 运行场景
6. 通过Inspector检查每个EntityBehaviour的EntityId字段

### 预期结果:
- 每个EntityBehaviour都应该有一个有效的EntityId
- UnityWorldAdapter的实体映射字典应包含3个条目

## 3. 变换同步测试

### 测试步骤:
1. 创建一个带有UnityWorldAdapter的场景
2. 添加一个带有EntityBehaviour的立方体
3. 确保EntityBehaviour上的"同步变换"选项已启用
4. 运行场景
5. 在运行时修改立方体的位置
6. 检查ECS中的TransformComponent是否更新

### 检查方式:
```csharp
// 在EntityBehaviour组件上添加一个测试方法
public void TestTransformSync()
{
    if (HasComponent<TransformComponent>())
    {
        var ecsTransform = GetECSComponent<TransformComponent>();
        Debug.Log($"ECS Position: {ecsTransform.Position}");
        Debug.Log($"Unity Position: {transform.position}");
    }
}
```

### 预期结果:
- Unity变换更改应该反映在ECS的TransformComponent中
- ECS中的变化应该反映到Unity变换中

## 4. 系统交互测试

### 测试步骤:
1. 创建一个带有UnityWorldAdapter的场景
2. 添加一个带有EntityBehaviour的立方体
3. 通过脚本为实体添加VelocityComponent:

```csharp
// 测试脚本
public class TestVelocity : MonoBehaviour
{
    private EntityBehaviour _entityBehaviour;
    
    void Start()
    {
        _entityBehaviour = GetComponent<EntityBehaviour>();
        
        // 添加速度组件
        if (_entityBehaviour.HasValidEntity)
        {
            _entityBehaviour.AddComponent(new VelocityComponent
            {
                Linear = new System.Numerics.Vector3(1, 0, 0),
                Angular = System.Numerics.Vector3.Zero
            });
        }
    }
    
    void Update()
    {
        // 输出当前位置
        Debug.Log($"Position: {transform.position}");
    }
}
```

4. 运行场景
5. 观察立方体是否自动移动

### 预期结果:
- 立方体应沿X轴移动
- 移动应由ECS的MovementSystem处理
- Unity变换应该自动更新

## 5. 实体生命周期测试

### 测试步骤:
1. 创建一个带有UnityWorldAdapter的场景
2. 添加一个带有EntityBehaviour的立方体
3. 通过脚本为实体添加LifetimeComponent:

```csharp
// 测试脚本
public class TestLifetime : MonoBehaviour
{
    private EntityBehaviour _entityBehaviour;
    
    void Start()
    {
        _entityBehaviour = GetComponent<EntityBehaviour>();
        
        // 添加生命周期组件 - 2秒后销毁
        if (_entityBehaviour.HasValidEntity)
        {
            _entityBehaviour.AddComponent(new LifetimeComponent
            {
                RemainingTime = 2.0f,
                DestroyOnExpire = true
            });
        }
    }
}
```

4. 运行场景
5. 观察立方体是否在约2秒后被销毁

### 预期结果:
- 立方体应在大约2秒后被销毁
- Unity游戏对象应该被销毁
- 实体应从ECS世界中移除

## 6. 错误处理测试

### 测试步骤:
1. 创建一个带有UnityWorldAdapter的场景
2. 添加一个没有EntityBehaviour的游戏对象
3. 尝试通过脚本手动访问此游戏对象的ECS功能:

```csharp
// 测试脚本
public class TestErrorHandling : MonoBehaviour
{
    void Start()
    {
        try
        {
            // 尝试获取不存在的EntityBehaviour
            var entityBehaviour = GetComponent<EntityBehaviour>();
            if (entityBehaviour == null)
            {
                Debug.Log("EntityBehaviour为空，这是预期的");
            }
            
            // 尝试直接访问世界
            if (GameCore.World != null)
            {
                var entity = GameCore.World.CreateEntity();
                Debug.Log($"成功创建实体: {entity}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"捕获到异常: {e.Message}");
        }
    }
}
```

4. 运行场景
5. 检查控制台日志

### 预期结果:
- 应该优雅地处理缺少EntityBehaviour的情况
- 直接访问GameCore.World应该成功
- 不应该有未处理的异常 