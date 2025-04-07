namespace GameCore.ECS.Jobs
{
    /// <summary>
    /// 任务接口，定义可并行执行的工作单元
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="startIndex">处理的起始索引</param>
        /// <param name="count">处理的元素数量</param>
        void Execute(int startIndex, int count);
    }
} 