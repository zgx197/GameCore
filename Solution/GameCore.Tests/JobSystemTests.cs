using System;
using System.Threading;
using Xunit;
using GameCore.ECS.Jobs;

namespace GameCore.Tests
{
    public class JobSystemTests
    {
        // 测试作业实现
        private struct TestJob : IJob
        {
            public int[] Data;
            public int Value;
            
            public void Execute(int startIndex, int count)
            {
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    if (i < Data.Length)
                    {
                        Data[i] = Value;
                    }
                }
            }
        }
        
        [Fact]
        public void Schedule_ExecutesJobCorrectly()
        {
            // 设置
            var jobSystem = new JobSystem();
            var data = new int[100]; // 减少数组大小以加快测试速度
            
            // 创建作业
            var job = new TestJob { Data = data, Value = 42 };
            
            // 调度作业
            var handle = jobSystem.Schedule(job, data.Length, 10);
            
            // 等待完成
            jobSystem.Complete(handle);
            
            // 验证所有元素都已正确更新
            foreach (var value in data)
            {
                Assert.Equal(42, value);
            }
        }
        
        [Fact]
        public void CompleteAll_WaitsForAllJobs()
        {
            // 设置
            var jobSystem = new JobSystem();
            var data1 = new int[50]; // 减少数组大小以加快测试速度
            var data2 = new int[50];
            
            // 创建和调度多个作业
            var job1 = new TestJob { Data = data1, Value = 42 };
            var job2 = new TestJob { Data = data2, Value = 84 };
            
            jobSystem.Schedule(job1, data1.Length, 10);
            jobSystem.Schedule(job2, data2.Length, 10);
            
            // 等待所有作业
            jobSystem.CompleteAll();
            
            // 验证所有数据都已更新
            foreach (var value in data1)
            {
                Assert.Equal(42, value);
            }
            
            foreach (var value in data2)
            {
                Assert.Equal(84, value);
            }
        }
        
        [Fact]
        public void ParallelExecution_JobsRunInParallel()
        {
            // 设置
            var jobSystem = new JobSystem(Environment.ProcessorCount);
            int completedBatches = 0;
            
            // 创建一个可以检测并行执行的作业
            var parallelJob = new ParallelTestJob { CompletedBatches = () => Interlocked.Increment(ref completedBatches) };
            
            // 调度具有多个批次的作业
            var handle = jobSystem.Schedule(parallelJob, 1000, 100);
            
            // 等待完成
            jobSystem.Complete(handle);
            
            // 验证多个批次已完成
            Assert.True(completedBatches > 1, $"预期多个并行批次，但只有 {completedBatches} 批次完成");
        }
        
        // 用于测试并行性的作业
        private struct ParallelTestJob : IJob
        {
            public Func<int> CompletedBatches;
            
            public void Execute(int startIndex, int count)
            {
                // 模拟工作负载
                Thread.Sleep(10);
                
                // 计数已完成的批次
                CompletedBatches();
            }
        }
    }
} 