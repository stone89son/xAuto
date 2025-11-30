using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApSetting
{
    public static class RetryHelperAsync
    {
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        // Lock helper
        private static async Task<T> RunWithLockAsync<T>(Func<Task<T>> action)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                return await action().ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        private static async Task RunWithLockAsync(Func<Task> action)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await action().ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        // Retry cho Task không trả về giá trị
        public static async Task DoAsync(
            Func<int, Task> action,
            int retryCount = 3,
            int delayMs = 500,
            CancellationToken cancellationToken = default)
        {
            await RunWithLockAsync(async () =>
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                Exception lastEx = null;

                for (int attempt = 1; attempt <= retryCount; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await action(attempt).ConfigureAwait(false);
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastEx = ex;
                        if (attempt == retryCount) throw;
                        await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                    }
                }

                throw lastEx ?? new Exception("RetryAsync failed");
            });
        }

        // Retry cho Task trả về bool (true = OK, false = retry)
        public static async Task<bool> DoAsync(
            Func<int, Task<bool>> action,
            int retryCount = 3,
            int delayMs = 500,
            CancellationToken cancellationToken = default)
        {
            return await RunWithLockAsync(async () =>
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                for (int attempt = 1; attempt <= retryCount; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    bool ok = false;

                    try
                    {
                        ok = await action(attempt).ConfigureAwait(false);
                    }
                    catch
                    {
                        if (attempt == retryCount) throw;
                    }

                    if (ok) return true;

                    if (attempt < retryCount)
                        await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                }

                return false;
            });
        }

        // Retry cho Task trả về T
        public static async Task<T> DoAsync<T>(
            Func<int, Task<T>> action,
            int retryCount = 3,
            int delayMs = 500,
            CancellationToken cancellationToken = default)
        {
            return await RunWithLockAsync(async () =>
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                Exception lastEx = null;

                for (int attempt = 1; attempt <= retryCount; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        return await action(attempt).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        lastEx = ex;
                        if (attempt == retryCount) throw;
                        await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                    }
                }

                throw lastEx ?? new Exception("RetryAsync failed");
            });
        }

        // Retry với timeout cho mỗi lần thử
        public static async Task<T> DoWithTimeoutAsync<T>(
            Func<int, CancellationToken, Task<T>> actionWithToken,
            int retryCount = 3,
            int attemptTimeoutMs = 5000,
            int delayMs = 500)
        {
            return await RunWithLockAsync(async () =>
            {
                if (actionWithToken == null) throw new ArgumentNullException(nameof(actionWithToken));

                for (int attempt = 1; attempt <= retryCount; attempt++)
                {
                    using (var cts = new CancellationTokenSource(attemptTimeoutMs))
                    {
                        try
                        {
                            return await actionWithToken(attempt, cts.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            if (attempt == retryCount)
                                throw new TimeoutException("Retry operation timed out.");
                        }
                        catch
                        {
                            if (attempt == retryCount)
                                throw;
                        }
                    }

                    await Task.Delay(delayMs).ConfigureAwait(false);
                }

                throw new Exception("RetryWithTimeoutAsync failed");
            });
        }
    }
}
