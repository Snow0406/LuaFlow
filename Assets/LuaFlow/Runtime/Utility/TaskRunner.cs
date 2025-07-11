using Cysharp.Threading.Tasks;

namespace LuaFlow.Runtime.Utility
{
    public static class TaskRunner
    {
        /// <summary>
        /// Proceed to the next step if any task is complete.
        /// </summary>
        /// <param name="tasks"></param>
        public static async UniTask ExecuteAny(params UniTask[] tasks)
        {
            await UniTask.WhenAny(tasks);
        }

        /// <summary>
        /// Proceed to the next step once the first task is complete.
        /// </summary>
        /// <param name="tasks"></param>
        public static async UniTask ExecuteFirst(params UniTask[] tasks)
        {
            if (tasks.Length == 0) return;
            _ = UniTask.WhenAll(tasks[1..]);
            await tasks[0];
        }

        /// <summary>
        /// Proceed to the next step once all tasks are complete.
        /// </summary>
        /// <param name="tasks"></param>
        public static async UniTask ExecuteAll(params UniTask[] tasks)
        {
            await UniTask.WhenAll(tasks);
        }
    }
}