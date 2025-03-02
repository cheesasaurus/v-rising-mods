using System;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace SystemsPOC;

public class MySystem : SystemBase
{
    private TimeSpan fiveSeconds = new TimeSpan(hours: 0, minutes: 0, seconds: 5);

    private DateTime nextTime = DateTime.MinValue;

    public override void OnCreate()
    {
        LogUtil.LogInfo($"MySystem.OnCreate called");
    }

    public override void OnStartRunning()
    {
        LogUtil.LogInfo($"MySystem.OnStartRunning called");
    }

    public override void OnUpdate()
    {
        if (DateTime.Now < nextTime)
        {
            return;
        }
        nextTime = DateTime.Now.Add(fiveSeconds);
        LogUtil.LogInfo($"[{DateTime.Now}] MySystem is updating. (next update in 5 seconds)");
    }

    public override void OnStopRunning()
    {
        LogUtil.LogInfo($"MySystem.OnStartRunning called");
    }
    
    public override void OnDestroy()
    {
        LogUtil.LogInfo($"MySystem.OnDestroy called");
    }

}