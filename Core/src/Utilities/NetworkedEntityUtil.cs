using Bloodstone.API;
using ProjectM.Network;
using Unity.Entities;

namespace VRisingMods.Core.Utilities;


public static class NetworkedEntityUtil {

    private static NetworkIdSystem _NetworkIdSystem = VWorld.Server.GetExistingSystemManaged<NetworkIdSystem>();

    public static bool TryFindEntity(NetworkId networkId, out Entity entity) {
        return _NetworkIdSystem._NetworkIdToEntityMap.TryGetValue(networkId, out entity);
    }

}
