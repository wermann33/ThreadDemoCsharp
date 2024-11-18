using System.Diagnostics;

namespace ThreadDemo;

class Program
{
    private static int _counter;
    private static readonly object LockObject = new();

    static void Main(string[] args)
    {
        _counter = 0;
        var stopwatch = Stopwatch.StartNew();
        Run(useLock: false);
        stopwatch.Stop();
        Console.WriteLine($"Final result without lock: {_counter}");
        Console.WriteLine($"Time taken without lock: {stopwatch.ElapsedMilliseconds} ms\n");

        _counter = 0;
        stopwatch.Restart();
        Run(useLock: true);
        stopwatch.Stop();
        Console.WriteLine($"Final result with lock: {_counter}");
        Console.WriteLine($"Time taken with lock: {stopwatch.ElapsedMilliseconds} ms\n");

        _counter = 0;
        stopwatch.Restart();
        RunWithThreadPool(useLock: true);
        stopwatch.Stop();
        Console.WriteLine($"Final result using ThreadPool with lock: {_counter}");
        Console.WriteLine($"Time taken using ThreadPool with lock: {stopwatch.ElapsedMilliseconds} ms");
    }

    private static void Run(bool useLock)
    {
        Thread.Sleep(1500);
        var threads = new Thread[3];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() => IncrementCounter(useLock));
            threads[i].Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }
    }

    private static void RunWithThreadPool(bool useLock)
    {
        int taskCount = 3;
        // done-signal to the main thread when all tasks are complete
        var doneEvent = new ManualResetEvent(false);
        int threadsCompleted = 0;

        for (int i = 0; i < taskCount; i++)
        {
            // Add each task to the ThreadPool for execution
            ThreadPool.QueueUserWorkItem(_ =>
            {
                IncrementCounter(useLock);

                // Increment the completed task count safely
                if (Interlocked.Increment(ref threadsCompleted) == taskCount)
                {
                    // Signal the main thread that all tasks are complete
                    doneEvent.Set();
                }
            });
        }

        // Wait for all tasks to complete
        doneEvent.WaitOne();
    }


    private static void IncrementCounter(bool useLock)
    {
        for (var cnt = 0; cnt < 100000; cnt++)
        {
            if (useLock)
            {
                lock (LockObject)
                {
                    _counter++;
                }
            } else
            {
                _counter++;
            }
        }
    }
}