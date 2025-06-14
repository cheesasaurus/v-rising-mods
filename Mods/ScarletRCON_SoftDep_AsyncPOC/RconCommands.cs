using System.Threading.Tasks;
using ScarletRCON.Shared;

namespace ScarletRCON_SoftDep_AsyncPOC;

[RconCommandCategory("Debug")]
public static class RconCommands
{

    [RconCommand("echo", "responds with the sent message", "<message>")]
    public async static Task<string> EchoAsync(string message)
    {
        await Task.Delay(200);
        return message;
    }

}
