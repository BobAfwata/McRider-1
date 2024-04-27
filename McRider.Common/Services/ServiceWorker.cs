using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace McRider.Common.Services;

/// <summary>
/// Represents a service worker that executes asynchronous actions concurrently.
/// </summary>
public class ServiceWorker
{
    private CancellationTokenSource cancellationTokenSource;
    private ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceWorker"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging messages.</param>
    public ServiceWorker(ILogger<ServiceWorker> logger)
    {
        _logger = logger;
        cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Gets a value indicating whether the service worker is currently running.
    /// </summary>
    public bool IsRunning { get; private set; } = false;

    /// <summary>
    /// Gets or sets the list of asynchronous actions to execute.
    /// </summary>
    public List<Func<IProgress<double>, Task<bool>>> Actions { get; protected set; } = new List<Func<IProgress<double>, Task<bool>>>();

    /// <summary>
    /// Removes an action from the list of actions.
    /// </summary>
    /// <param name="action">The action to remove.</param>
    /// <returns><c>true</c> if the action was removed; otherwise, <c>false</c>.</returns>
    public bool RemoveAction(Func<IProgress<double>, Task<bool>> action) => Actions.Remove(action);

    /// <summary>
    /// Adds an action to the list of actions.
    /// </summary>
    /// <param name="action">The action to add.</param>
    public void AddAction(Func<IProgress<double>, Task<bool>> action) => Actions.Add(action);

    /// <summary>
    /// Starts the service worker.
    /// </summary>
    /// <param name="progress">The progress reporter.</param>
    /// <returns>The total count of completed actions.</returns>
    public async Task<long> Start(IProgress<double> progress)
    {
        if (cancellationTokenSource?.IsCancellationRequested != false)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
        }

        var cancellationToken = cancellationTokenSource.Token;
        long count = 0;

        try
        {
            IsRunning = true;
            var tasks = new ConcurrentDictionary<int, Task<bool>>();
            var completedActions = new List<Func<IProgress<double>, Task<bool>>>();

            while (!cancellationToken.IsCancellationRequested)
            {
                completedActions.ForEach(func => RemoveAction(func));

                if (Actions.Count != 0)
                    Actions.ForEach(action =>
                    {
                        try
                        {
                            if (action == null)
                                return;

                            if (cancellationToken.IsCancellationRequested)
                                return;

                            if (tasks.ContainsKey(action.GetHashCode()))
                                return;

                            try
                            {
                                tasks[action.GetHashCode()] = action.Invoke(progress);

                                tasks[action.GetHashCode()]?.ContinueWith(task =>
                                {
                                    if (task.Result == true)
                                    {
                                        completedActions.Add(action);
                                        Interlocked.Increment(ref count);
                                        tasks.Remove(action.GetHashCode(), out _);
                                    }
                                }).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error while running backgound tasks");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing action.");
                        }
                    });

                if (!tasks.IsEmpty)
                    await Task.WhenAll(tasks.Values);
                else
                    await Task.Delay(500);
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation
        }
        finally
        {
            IsRunning = false;
        }

        return count;
    }

    /// <summary>
    /// Stop
    /// </summary>
    public async Task<bool> Stop()
    {
        if (IsRunning == false)
            return false;

        cancellationTokenSource?.Cancel();

        var count = 0;

        while (IsRunning)
        {
            await Task.Delay(1000);
            if (++count >= 1000)
                return false;
        }

        return true;
    }
}