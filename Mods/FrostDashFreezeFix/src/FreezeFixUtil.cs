namespace FrostDashFreezeFix;

public static class FreezeFixUtil
{
    public static int TickCount = 0;
    public static int CurrentTick_CallCount = 0;

    public static void NewTickStarted()
    {
        TickCount++;
        CurrentTick_CallCount = 0;
    }

}