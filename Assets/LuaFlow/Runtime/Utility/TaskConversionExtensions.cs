using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace LuaFlow.Runtime.Utility
{
    /// <summary>
    /// A static class that provides extension methods to convert from ValueTask to UniTask
    /// </summary>
    public static class TaskConversionExtensions
    {
        public static UniTask AsUniTask(this ValueTask valueTask)
        {
            return UniTask.Create(async () => await valueTask);
        }
    }
}