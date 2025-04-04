namespace GameCore
{
    /// <summary>
    /// GameCore框架主入口点和配置类
    /// </summary>
    public static class GameCore
    {
        /// <summary>
        /// 框架版本
        /// </summary>
        public static readonly string Version = typeof(GameCore).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        /// <summary>
        /// 初始化GameCore框架
        /// </summary>
        public static void Initialize()
        {

        }   
    }
}