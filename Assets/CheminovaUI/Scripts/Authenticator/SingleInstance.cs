using System.Threading;
using System.IO.Pipes;

public class SingleInstance
{
    private static Mutex mutex;

    public static bool Initialize()
    {
        mutex = new Mutex(true, "ChemiAnalysisUnityAppMutex", out var createdNew);
        return createdNew;
    }
}