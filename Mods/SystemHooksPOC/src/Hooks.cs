namespace SystemHooksPOC.Hooks;


// todo: what to pass in, and what NOT to pass in. do we want to avoid unsafe contexts?

public delegate bool Hook_System_OnUpdate_Prefix();
public struct HookOptions_System_OnUpdate_Prefix(bool onlyWhenSystemRuns = true)
{
    public bool OnlyWhenSystemRuns = onlyWhenSystemRuns;
    public static HookOptions_System_OnUpdate_Prefix Default => new HookOptions_System_OnUpdate_Prefix();
}

public delegate void Hook_System_OnUpdate_Postfix();
public struct HookOptions_System_OnUpdate_Postfix(bool onlyWhenSystemRuns = true)
{
    public bool OnlyWhenSystemRuns = onlyWhenSystemRuns;
    public static HookOptions_System_OnUpdate_Postfix Default => new HookOptions_System_OnUpdate_Postfix();
}
