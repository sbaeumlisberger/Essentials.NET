namespace Essentials.NET;

public class AsyncEventArgs
{
    private readonly List<Task> tasks = new List<Task>();

    public Task CompletionTask => Task.WhenAll(tasks);

    public void AddTask(Task task)
    {
        tasks.Add(task);
    }

}
