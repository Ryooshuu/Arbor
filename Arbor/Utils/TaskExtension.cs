namespace Arbor.Utils;

public static class TaskExtension
{
    public static void WaitSafely(this Task task)
    {
        if (!isWaitingValid(task))
            throw new InvalidOperationException($"Can't use {nameof(WaitSafely)} from inside an async operation.");

        task.Wait();
    }
    
    private static bool isWaitingValid(Task task)
    {
        if (task.CreationOptions.HasFlag(TaskCreationOptions.LongRunning))
            return true;

        return !Thread.CurrentThread.IsThreadPoolThread;
    }
}
