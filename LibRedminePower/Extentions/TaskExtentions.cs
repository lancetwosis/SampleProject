using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LibRedminePower.Extentions
{
    public static class TaskExtentions
    {
        public static Task<T> WithCancel<T>(this Task<T> task, CancellationToken ct)
        {
            return task.IsCompleted ? 
                task :
                task.ContinueWith(completedTask => completedTask.GetAwaiter().GetResult(),
                ct,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        /// <summary>
        /// 非同期処理中に発生した例外を集約例外に委ねる。
        /// await する場合、本メソッドは不要なため、意図的に戻り値は void とする。
        /// </summary>
        public static void WithErrHandleAsync(this Task task)
        {
            var _ = task.ContinueWith(t =>
            {
                if (t.Exception != null)
                    ErrorHandler.Instance.HandleError(t.Exception);
            });
        }
    }
}
