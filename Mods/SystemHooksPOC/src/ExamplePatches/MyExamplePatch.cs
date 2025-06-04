using System;
using ProjectM;
using SystemHooksPOC.Attributes;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC.ExamplePatches;

public class MyExamplePatch
{
    private static TimeSpan twoSeconds = new TimeSpan(hours: 0, minutes: 0, seconds: 2);
    private static DateTime nextTime = DateTime.MinValue;

    [EcsSystemUpdatePrefix(typeof(TakeDamageInSunSystem_Server), onlyWhenSystemRuns: true)]
    public static bool ExamplePrefix() //note: must be public
    {
        // return false, to skip further prefixes and the original
        bool shouldSkipFurtherPrefixesAndTheOriginal = true;
        bool returnVal = !shouldSkipFurtherPrefixesAndTheOriginal;

        if (DateTime.Now < nextTime)
        {
            return returnVal;
        }
        nextTime = DateTime.Now.Add(twoSeconds);
        LogUtil.LogInfo($"[{DateTime.Now}] ExamplePrefix executing. (debounce 2 seconds)");
        return returnVal;
    }

}