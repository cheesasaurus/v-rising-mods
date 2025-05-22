using Bloodstone.API;
using ProjectM.Network;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.Chat;

public static class ChatUtil
{
    public static void ForgeMessage(UserModel userModel, string message, ChatMessageType messageType = ChatMessageType.Global)
    {
        var entityManager = VWorld.Server.EntityManager;

        var fromCharacter = new FromCharacter()
        {
            User = userModel.Entity,
            Character = userModel.User.LocalCharacter._Entity,
        };

        var messageEvent = new ChatMessageEvent()
        {
            MessageType = messageType,
            MessageText = message,
        };

        var entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, fromCharacter);
        AotWorkaroundUtil.AddComponentData(entity, messageEvent);
    }

    public static void SendSystemMessageToClient(User user, string message)
    {
        var entityManager = VWorld.Server.EntityManager;
        var messageString512Bytes = new Unity.Collections.FixedString512Bytes(message.ToString());
        ProjectM.ServerChatUtils.SendSystemMessageToClient(entityManager, user, ref messageString512Bytes);
    }

    public static void SendSystemMessageToAllClients(string message)
    {
        var entityManager = VWorld.Server.EntityManager;
        var messageString512Bytes = new Unity.Collections.FixedString512Bytes(message.ToString());
        ProjectM.ServerChatUtils.SendSystemMessageToAllClients(entityManager, ref messageString512Bytes);
    }
    
}
