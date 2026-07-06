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
        //var nameSender = CacheManager.Instance.GetAccountData(chatPacket.idSender).accountCachedData.Username;

        //null ref exception
        var senderData = CacheManager.Instance.GetAccountData(chatPacket.idSender);
        if (senderData == null)
            return;

        string nameSender = senderData.accountCachedData.Username;

        int idReceiver = 0;

        bool isPrivateChanel = chatPacket.channel == 4; //ui(chat) t lam private channel = 4 trong hierarchy

        if (isPrivateChanel)
        {
            //if (CacheManager.Instance.GetAccountData(chatPacket.nameReceiver) != null)
            //{
            //    idReceiver = CacheManager.Instance.GetAccountData(chatPacket.nameReceiver).accountCachedData.Idaccount;
            //}

            //private msg check idreceiver ton tai (dung client/socket)
            var receiverData = CacheManager.Instance.GetAccountData(chatPacket.nameReceiver);

            if (receiverData == null)
            {
                Console.WriteLine("nguoi nhan " + chatPacket.nameReceiver + " khong ton tai:");
                return;
            }

            idReceiver = receiverData.accountCachedData.Idaccount;

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

            //if (idReceiver > 0)
            //{
            //    ClientConnection receiverClient = RaceManager.Instance.GetClientByAccountId(idReceiver);

            //    if (receiverClient != null)
            //    {
            //        await RaceManager.Instance.SendPacketToClient(receiverClient, writer.ToArray());
            //    }

            //    SaveMessage(chatPacket, idReceiver).Wait();
            //}
            //else
            //    await RaceManager.Instance.SendPacketToAllClients(writer.ToArray());

            byte[] data = writer.ToArray(); //giam tai cpu luc call sender vs receiver

            //check idreceiver/package co nam trong private channel ko
            if (isPrivateChanel)
            {
                ClientConnection receiverClient = RaceManager.Instance.GetClientByAccountId(idReceiver);
                await RaceManager.Instance.SendPacketToClient(client, data); //t cung can thay nhung gi t chat vs m tren UI chu

                //nguoi nhan onl thi gui, tu chat voi ban than thi ko resend msg (2 lan)
                if  (receiverClient != null && receiverClient != client)
                {
                    await RaceManager.Instance.SendPacketToClient(receiverClient, data);
                }

                await SaveMessage(chatPacket, idReceiver);
            }
            else
                await RaceManager.Instance.SendPacketToAllClients(data);
        }
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