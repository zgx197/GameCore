using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameCore.ECS.Jobs
{
    /// <summary>
    /// 作业句柄，用于跟踪和等待作业完成
    /// </summary>
    public readonly struct JobHandle
    {
        /// <summary>
        /// 内部任务
        /// </summary>
        internal readonly Task Task;

        /// <summary>
        /// 作业ID
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// 检查作业是否已完成
        /// </summary>
        public bool IsCompleted => Task?.IsCompleted ?? true;

        /// <summary>
        /// 创建新的作业句柄
        /// </summary>
        internal JobHandle(Task task, int id)
        {
            Task = task;
            Id = id;
        }

        /// <summary>
        /// 表示无效或已完成的作业句柄
        /// </summary>
        public static readonly JobHandle Completed = new JobHandle(Task.CompletedTask, 0);
    }

    /// <summary>
    /// 任务系统，负责管理和调度并行任务
    /// </summary>
    public class JobSystem
    {
        /// <summary>
        /// 作业计数器，用于生成唯一ID
        /// </summary>
        private int _jobCounter = 0;
        
        /// <summary>
        /// 活动作业字典
        /// </summary>
        private readonly Dictionary<int, Task> _activeJobs = new Dictionary<int, Task>();
        
        /// <summary>
        /// 锁对象，保护活动作业字典
        /// </summary>
        private readonly object _jobsLock = new object();
        
        /// <summary>
        /// 并行选项，配置任务并行度
        /// </summary>
        private readonly ParallelOptions _parallelOptions;

        /// <summary>
        /// 创建新的任务系统
        /// </summary>
        /// <param name="maxDegreeOfParallelism">最大并行度，默认使用所有可用处理器</param>
        public JobSystem(int maxDegreeOfParallelism = -1)
        {
            _parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };
        }

        /// <summary>
        /// 调度任务执行
        /// </summary>
        /// <typeparam name="T">任务类型</typeparam>
        /// <param name="job">任务实例</param>
        /// <param name="itemCount">处理的元素总数</param>
        /// <param name="batchSize">每批处理的元素数量</param>
        /// <returns>作业句柄</returns>
        public JobHandle Schedule<T>(T job, int itemCount, int batchSize = 64) where T : IJob
        {
            if (itemCount <= 0)
            {
                return JobHandle.Completed;
            }

            // 确保批次大小合理
            batchSize = Math.Max(1, Math.Min(batchSize, itemCount));

            // 生成新的作业ID
            int jobId = Interlocked.Increment(ref _jobCounter);

            // 创建任务
            Task task = Task.Run(() =>
            {
                Parallel.For(0, (itemCount + batchSize - 1) / batchSize, _parallelOptions, batchIndex =>
                {
                    int start = batchIndex * batchSize;
                    int count = Math.Min(batchSize, itemCount - start);
                    job.Execute(start, count);
                });
            });

            // 添加到活动作业字典
            lock (_jobsLock)
            {
                _activeJobs[jobId] = task;
            }

            // 任务完成后自动清理
            task.ContinueWith(t =>
            {
                lock (_jobsLock)
                {
                    _activeJobs.Remove(jobId);
                }
            });

            return new JobHandle(task, jobId);
        }

        /// <summary>
        /// 等待作业完成
        /// </summary>
        public void Complete(JobHandle handle)
        {
            if (handle.Task == null)
            {
                return;
            }

            try
            {
                handle.Task.Wait();
            }
            catch (AggregateException ae)
            {
                // 展开异常并重新抛出第一个内部异常
                if (ae.InnerExceptions.Count > 0)
                {
                    throw ae.InnerExceptions[0];
                }
                throw;
            }
        }

        /// <summary>
        /// 等待所有作业完成
        /// </summary>
        public void CompleteAll()
        {
            Task[] tasks;
            
            lock (_jobsLock)
            {
                tasks = new Task[_activeJobs.Count];
                _activeJobs.Values.CopyTo(tasks, 0);
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                // 展开异常并重新抛出第一个内部异常
                if (ae.InnerExceptions.Count > 0)
                {
                    throw ae.InnerExceptions[0];
                }
                throw;
            }
        }
    }
} 