using System.Collections.Concurrent;

using Klipboard.Utils;

namespace Klipboard
{
    public class UxTaskScheduler : IDisposable
    {
        private Thread m_thread;
        private CancellationTokenSource m_cancellationTokenSource;
        private BlockingCollection<(TaskCompletionSource CompletionSource, Action Action)> m_tasks;

        public UxTaskScheduler()
        {
            m_cancellationTokenSource = new CancellationTokenSource();
            m_tasks = new BlockingCollection<(TaskCompletionSource TaskSource, Action Act)>();

            m_thread = new Thread(RunTasks);
            m_thread.SetApartmentState(ApartmentState.STA);
            m_thread.Start();
        }

        public void Dispose()
        {
            m_cancellationTokenSource?.Cancel();
            m_thread?.Join();
        }

        private void RunTasks()
        {
            try
            {
                foreach (var task in m_tasks.GetConsumingEnumerable(m_cancellationTokenSource.Token))
                {
                    try
                    {
                        task.Action.Invoke();
                        task.CompletionSource.SetResult();
                    }
                    catch (Exception ex)
                    {
                        task.CompletionSource.SetException(ex);
                        Logger.Log.Error(ex, "Exception happaned while running UX tasks");
                    }
                }
            }
            catch(OperationCanceledException) { }
            catch(Exception ex) 
            {
                Logger.Log.Error(ex, "Exception happaned while fecthing UX tasks");
                throw;
            }
        }

        public Task Run(Action action)
        {
            var completionSource = new TaskCompletionSource();

            m_tasks.Add((completionSource, action));
            return completionSource.Task;
        }
    }
}
