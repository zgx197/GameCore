using System;
using System.Numerics;
using System.Linq;
using Xunit;
using GameCore.ECS.Core;
using GameCore.ECS.Components;
using GameCore.ECS.Systems;

namespace GameCore.Tests
{
    // 测试用的可见组件
    public struct TestTransformComponent : IComponent
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }
    
    public struct TestVelocityComponent : IComponent
    {
        public Vector3 Linear;
        public Vector3 Angular;
    }
    
    public struct TestLifetimeComponent : IComponent
    {
        public float RemainingTime;
        public bool DestroyOnExpire;
    }
    
    // 测试用系统
    public class TestMovementSystem : SystemBase
    {
        private Query? _query;
        
        public override void Initialize()
        {
            _query = CreateQuery(new[] { typeof(TestTransformComponent), typeof(TestVelocityComponent) });
        }
        
        public override void Execute()
        {
            if (_query == null) return;
            
            var entities = _query.GetMatchingEntities() ?? Array.Empty<EntityId>();
            if (entities.Count == 0) return;
            
            float deltaTime = World?.Time.DeltaTime ?? 0.01f;
            
            foreach (var entity in entities)
            {
                ref var transform = ref World!.GetComponent<TestTransformComponent>(entity);
                ref var velocity = ref World!.GetComponent<TestVelocityComponent>(entity);
                
                transform.Position += velocity.Linear * deltaTime;
            }
        }
    }
    
    public class ECSCoreTests
    {
        [Fact]
        public void Initialize_CreatesValidWorld()
        {
            // 重置GameCore状态
            if (GameCore.IsInitialized)
                GameCore.Shutdown();
            
            // 初始化
            GameCore.Initialize();
            
            // 验证
            Assert.NotNull(GameCore.World);
            Assert.NotNull(GameCore.Events);
            Assert.NotNull(GameCore.Jobs);
            Assert.True(GameCore.IsInitialized);
            
            // 清理
            GameCore.Shutdown();
        }
        
        [Fact]
        public void EntityCreation_AddsAndRetrievesComponents()
        {
            // 设置
            var world = new World();
            var entityId = world.CreateEntity();
            
            // 测试添加和获取组件
            var transform = new TestTransformComponent
            {
                Position = new Vector3(1, 2, 3),
                Rotation = Quaternion.Identity,
                Scale = Vector3.One
            };
            
            world.AddComponent(entityId, transform);
            
            // 验证
            Assert.True(world.HasComponent<TestTransformComponent>(entityId));
            var retrievedTransform = world.GetComponent<TestTransformComponent>(entityId);
            Assert.Equal(1, retrievedTransform.Position.X);
            Assert.Equal(2, retrievedTransform.Position.Y);
            Assert.Equal(3, retrievedTransform.Position.Z);
        }
        
        [Fact]
        public void MovementSystem_UpdatesPositionCorrectly()
        {
            // 设置
            var world = new World();
            world.Initialize(); // 初始化世界
            
            var entityId = world.CreateEntity();
            
            // 添加所需组件
            world.AddComponent(entityId, new TestTransformComponent
            {
                Position = new Vector3(0, 0, 0),
                Rotation = Quaternion.Identity,
                Scale = Vector3.One
            });
            
            world.AddComponent(entityId, new TestVelocityComponent
            {
                Linear = new Vector3(1, 2, 3),
                Angular = Vector3.Zero
            });
            
            // 注册测试系统
            var movementSystem = new TestMovementSystem();
            world.RegisterSystem(movementSystem);
            
            // 执行系统
            world.Update(1.0f); // 更新1秒
            
            // 验证位置变化
            var transform = world.GetComponent<TestTransformComponent>(entityId);
            Assert.Equal(1, transform.Position.X);
            Assert.Equal(2, transform.Position.Y);
            Assert.Equal(3, transform.Position.Z);
        }
        
        [Fact]
        public void ComponentSystemInteraction_BasicTest()
        {
            // 设置
            var world = new World();
            world.Initialize(); // 初始化世界
            
            var entityId = world.CreateEntity();
            
            // 添加组件
            world.AddComponent(entityId, new TestTransformComponent
            {
                Position = Vector3.Zero
            });
            
            // 验证实体存在并有正确的组件
            Assert.True(world.IsEntityAlive(entityId));
            Assert.True(world.HasComponent<TestTransformComponent>(entityId));
        }
    }
} 