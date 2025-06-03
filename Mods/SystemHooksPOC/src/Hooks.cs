namespace SystemHooksPOC.Hooks;


// todo: what to pass in, and what NOT to pass in. do we want to avoid unsafe contexts?

public delegate bool Hook_System_OnUpdate_Prefix();

public delegate void Hook_System_OnUpdate_Postfix();