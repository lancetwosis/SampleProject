using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    }
}
