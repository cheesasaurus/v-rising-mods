using Bloodstone.API;
using ProjectM.Network;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.Chat;

public static class ChatUtil {
    public static void ForgeMessage(UserModel userModel, string message, ChatMessageType messageType = ChatMessageType.Global) {
        var entityManager = VWorld.Server.EntityManager;

        var fromCharacter = new FromCharacter() {
			User = userModel.Entity,
			Character = userModel.User.LocalCharacter._Entity,
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
