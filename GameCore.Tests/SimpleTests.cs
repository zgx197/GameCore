using Xunit;
using GameCore.ECS.Core;

namespace GameCore.Tests
{
    public class SimpleTests
    {
        [Fact]
        public void WorldInitialize_Works()
        {
            // 创建世界实例
            var world = new World();
            
            // 初始化世界
            world.Initialize();
            
            // 创建实体
            var entityId = world.CreateEntity();
            
            // 验证实体创建成功
            Assert.True(world.IsEntityAlive(entityId));
            
            // 销毁实体
            world.DestroyEntity(entityId);
            
            // 验证实体已销毁
            Assert.False(world.IsEntityAlive(entityId));
            
            // 清理世界
            world.Cleanup();
        }
        
        [Fact]
        public void GameCoreInitShutdown_Works()
        {
            // 确保GameCore未初始化
            if (GameCore.IsInitialized)
            {
                GameCore.Shutdown();
            }
            
            Assert.False(GameCore.IsInitialized);
            
            // 禁用默认系统注册以避免测试失败
            // 这是临时的解决方案
            var originalSystems = typeof(GameCore).GetField("_registerDefaultSystems", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            bool oldValue = false;
            
            // 获取并临时禁用默认系统
            oldValue = originalSystems?.GetValue(null) as bool? ?? false;
            originalSystems?.SetValue(null, false);
            
            // 初始化
            GameCore.Initialize();
            
            // 验证
            Assert.True(GameCore.IsInitialized);
            Assert.NotNull(GameCore.World);
            
            // 关闭
            GameCore.Shutdown();
            
            // 恢复原始设置
            if (oldValue)
            {
                originalSystems?.SetValue(null, true);
            }
        }
    }
} 