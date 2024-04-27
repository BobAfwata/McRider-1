using Microsoft.Extensions.Logging;

namespace McRider.Common.Services;

public class RetryExecutionService
{
    ILogger Logger;

    public RetryExecutionService(ILogger<RetryExecutionService> logger)
    {
        Logger = logger;
    }

    /// <summary>
    ///     The retry methods are used for local retry cycles of any functionality that can be retried.  
    ///     The action delegate should return a boolean value specifying if the action should be retried.  
    ///     If it should be retried, return true.  If it should NOT retry then return false (for instance in cases where the action succeeded).
    /// </summary>
    /// <param name="cycles"></param>
    /// <param name="waitFor"></param>
    /// <param name="scale"></param>
    /// <param name="action"></param>
    /// <param name="errorHandler"></param>
    /// <return>true: If it should be retried, false: If it should NOT retry (for instance in cases where the action succeeded)</return>
    public void Retry(int cycles, int waitFor, int scale, Func<bool> action, Action errorHandler)
    {
        //  Invoke the action imediatly
        bool shouldRetry = action();

        //  Initialize cycle count to 1
        int cycleCount = 1;

        //  Initialize wait period 
        int millisWait = waitFor;

        //  Process while should retry and cycle count is less then specified cycles
        while (shouldRetry && cycleCount < cycles)
        {
            if (cycleCount > 1)
                Logger.LogInformation($"Retry {cycleCount} of {cycles}.");

            //  Sleep for the wait period
            Thread.Sleep(millisWait);

            //  Retry the action
            shouldRetry = action();

            //  Increment cycle count
            cycleCount++;

            //  Multiply wait period by scale
            millisWait *= scale;
        }

        //  Check if alotted cycles have been met and the action was not yet successful (should retry)
        if (cycleCount == cycles && shouldRetry)
            //  If cycles met and action not successful, call the Can't Retry action handle
            errorHandler?.Invoke();
    }

    /// <summary>
    /// Retries the passed action if any Exception is thrown
    /// </summary>
    /// <param name="cycles"></param>
    /// <param name="waitFor"></param>
    /// <param name="scale"></param>
    /// <param name="action"></param>
    public T RetryOnException<T>(int cycles, int waitFor, int scale, Func<T> action, Action<Exception> errorHandler = null)
    {
        var exception = default(Exception);
        var result = default(T);

        Retry(cycles, waitFor, scale, () =>
        {
            try
            {
                result = action();
                exception = null;
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Something went wrong while executing and action! Retrying..");
                exception = ex;
                return true;
            }
        }, () =>
        {
            Logger.LogError(exception, $"RetryOnException failed after {cycles} retries!");
            if (errorHandler != null)
                errorHandler(exception);
            else if (exception != null)
                throw new Exception($"RetryOnException failed after {cycles} retries!", exception);
        });

        return result;
    }

    /// <summary>
    /// Retries the passed action if any Exception is thrown
    /// </summary>
    /// <param name="cycles"></param>
    /// <param name="waitFor"></param>
    /// <param name="scale"></param>
    /// <param name="action"></param>
    public void RetryOnException(int cycles, int waitFor, int scale, Action action, Action<Exception> errorHandler = null) =>
        RetryOnException<object>(cycles, waitFor, scale, () => { action?.Invoke(); return string.Empty; }, errorHandler);

    /// <summary>
    /// Retries the passed action 10 times if any Exception is thrown with 1sec between eac retry
    /// </summary>
    /// <param name="action"></param>
    public T RetryOnException<T>(Func<T> action, Action<Exception> errorHandler = null) =>
        RetryOnException(5, 1000, 2, action, errorHandler);

    /// <summary>
    /// Retries the passed action 10 times if any Exception is thrown with 1sec between eac retry
    /// </summary>
    /// <param name="action"></param>
    public void RetryOnException(Action action, Action<Exception> errorHandler = null) =>
        RetryOnException<object>(5, 1000, 2, () => { action?.Invoke(); return string.Empty; }, errorHandler);

    /// <summary>
    /// Retries the passed action 10 times if any Exception is thrown with 1sec between eac retry
    /// </summary>
    /// <param name="action"></param>
    public T RetryOnExceptionAsync<T>(Func<Task<T>> action, Action<Exception> errorHandler = null) =>
        RetryOnException(3, 1000, 2, () =>
        {
            var task = action();
            
            task.ConfigureAwait(false);
            task.Wait();

            return task.Result;
        }, errorHandler);

}
