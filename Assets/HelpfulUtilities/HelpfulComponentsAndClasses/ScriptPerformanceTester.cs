using System.Diagnostics;
using System;

public class ScriptPerformanceTester {

    /// <summary>
    /// Runs an action for a number of iterations and returns the total time spent in ticks
    /// </summary>
    /// <param name="inActionToTest"></param>
    /// <param name="inActionRunIterations"></param>
    /// <returns></returns>
    public static TimeSpan GetTimeSpanOfAction(Action inActionToTest, int inActionRunIterations)
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();
        for (int i = 0; i < inActionRunIterations; i++)
        {
            inActionToTest();
        }
        sw.Stop();

        return sw.Elapsed;
    }

    /// <summary>
    /// Does a test mulitple times: Runs an action for a number of iterations and returns the total time spent in ticks
    /// And then finds the average time for the tests.
    /// </summary>
    /// <param name="inActionToTest"></param>
    /// <param name="inActionRunIterations"></param>
    /// <param name="inNumOfRuns"></param>
    /// <returns></returns>
    public static long TestAnActionAndGetTheAverageTicksTaken(Action inActionToTest, int inActionRunIterations, int inNumOfRuns)
    {
        long totalTicks = 0;

        for (int i = 0; i < inNumOfRuns; i++)
        {
            totalTicks += GetTimeSpanOfAction(inActionToTest, inActionRunIterations).Ticks;
        }

        return totalTicks / inNumOfRuns;
    }
}
