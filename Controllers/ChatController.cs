using Server.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ChatRequestPacket
{
    public int channel;
    public int idSender;
    public string nameReceiver;
    public string message;
}
public class ChatResultPacket
{
    public EnumCmdCode cmd;
    public int channel;
    public string nameSender;
    public int idReceiver;
    public string message;
}


public class ChatController
{
    public async Task SendChatMessage(ClientConnection client, ChatRequestPacket chatPacket)
    {
        var nameSender = CacheManager.Instance.GetAccountData(chatPacket.idSender).accountCachedData.Username;
        int idReceiver = 0;
        if (CacheManager.Instance.GetAccountData(chatPacket.nameReceiver) != null)
        {
            idReceiver = CacheManager.Instance.GetAccountData(chatPacket.nameReceiver).accountCachedData.Idaccount;
        }

        ChatResultPacket chatResultPacket = new ChatResultPacket
        {
            cmd = EnumCmdCode.chat,
            channel = chatPacket.channel,
            nameSender = nameSender,
            idReceiver = idReceiver,
            message = chatPacket.message
        };

        PacketWriterManager writer = new PacketWriterManager();
        writer.WriteInt((int)chatResultPacket.cmd);
        writer.WriteInt(chatResultPacket.channel);
        writer.WriteString(chatResultPacket.nameSender);
        writer.WriteInt(chatResultPacket.idReceiver);
        writer.WriteString(chatResultPacket.message);

        if (idReceiver > 0)
        {
            ClientConnection receiverClient = RaceManager.Instance.GetClientByAccountId(idReceiver);

            if (receiverClient != null)
            {
                await RaceManager.Instance.SendPacketToClient(receiverClient, writer.ToArray());
            }

            SaveMessage(chatPacket, idReceiver).Wait();
        }
        else
            await RaceManager.Instance.SendPacketToAllClients(writer.ToArray());
    }
    private async Task SaveMessage(ChatRequestPacket chatPacket, int idReceiver)
    {
        Message message = new Message
        {
            IdaccountFrom = chatPacket.idSender,
            IdaccountTo = idReceiver,
            Contents = chatPacket.message
        };

        WebSocketServerManager.db.Messages.Add(message);
        await WebSocketServerManager.db.SaveChangesAsync();
    }
}