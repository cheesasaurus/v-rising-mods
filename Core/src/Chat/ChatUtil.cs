using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.Chat;

public static class ChatUtil {
    public static void ForgeMessage(string senderName, string message, ChatMessageType messageType = ChatMessageType.Global) {
        var entityManager = VWorld.Server.EntityManager;

        var foundSender = UserUtil.TryFindUserByName(senderName, out var user);
        if (!foundSender) {
            return;
        }
        
        // todo: better way to get user+charactrer combo
        var character = user.Value.LocalCharacter;
        var playerCharacter = VWorld.Server.EntityManager.GetComponentData<PlayerCharacter>(character._Entity);

        var fromCharacter = new FromCharacter() {
			User = playerCharacter.UserEntity,
			Character = character._Entity,
		};

        var messageEvent = new ChatMessageEvent() {
            MessageType = messageType,
            MessageText = message,
        };

        var entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, fromCharacter);
        AotWorkaroundUtil.AddComponentData(entity, messageEvent);
    }
}
