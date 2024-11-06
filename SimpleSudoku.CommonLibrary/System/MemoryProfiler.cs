using System.Diagnostics;

namespace SimpleSudoku.CommonLibrary.System
{
    public class MemoryProfiler
    {
        public static void ProfileMethodMemoryUsage<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            // Force a garbage collection to get a clean memory snapshot.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Get the memory usage before the method execution
            long beforeMemory = GC.GetTotalMemory(true);

            // Execute the method with argument
            method(arg1, arg2, arg3, arg4);

            // Get the memory usage after the method execution
            long afterMemory = GC.GetTotalMemory(true);

            // Calculate the difference in memory usage
            long memoryUsed = afterMemory - beforeMemory;

            // Print to Debug console
            Debug.WriteLine($"Memory used by method: {memoryUsed} bytes");
        }
    }
}
