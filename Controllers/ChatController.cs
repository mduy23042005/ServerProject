using Microsoft.EntityFrameworkCore;
using Server.Models;

public class ChatRequestPacket
{
    public int channel;
    public int idSender;
    public string nameReceiver = string.Empty;
    public string message = string.Empty;
}

public class ChatResultPacket
{
    public EnumCmdCode cmd;
    public int channel;
    public string nameSender = string.Empty;
    public int idReceiver;
    public string message = string.Empty;
}

public class ChatController
{
    private const int AllServerChannel = 0;
    private const int PartyChannel = 3;
    private const int PrivateChannel = 4;
    private const int SystemChannel = 5;
    private const int MaxMessageLength = 180;

    public async Task SendChatMessage(ClientConnection client, ChatRequestPacket chatPacket)
    {
        int senderId = RaceManager.Instance.GetIDAccount(client);
        if (senderId <= 0)
        {
            await SendError(client, 0, "You must log in before sending chat messages.");
            return;
        }

        AccountData senderData = CacheManager.Instance.GetAccountData(senderId);
        if (senderData?.accountCachedData == null)
        {
            await SendError(client, senderId, "The sender account is not available.");
            return;
        }

        string message = chatPacket.message?.Trim() ?? string.Empty;
        if (message.Length == 0)
        {
            await SendError(client, senderId, "The message cannot be empty.");
            return;
        }

        if (message.Length > MaxMessageLength)
        {
            await SendError(client, senderId, $"The message cannot exceed {MaxMessageLength} characters.");
            return;
        }

        string senderName = senderData.accountCachedData.Username;

        switch (chatPacket.channel)
        {
            case AllServerChannel:
                await RaceManager.Instance.SendPacketToAllClients(
                    BuildChatPacket(AllServerChannel, senderName, 0, message));
                break;

            case PrivateChannel:
                await SendPrivateMessage(client, senderId, senderName, chatPacket.nameReceiver, message);
                break;

            case PartyChannel:
                await SendError(client, senderId, "Party chat is not available yet.");
                break;

            default:
                await SendError(client, senderId, "Unsupported chat channel.");
                break;
        }
    }

    private async Task SendPrivateMessage(
        ClientConnection senderClient,
        int senderId,
        string senderName,
        string rawReceiverName,
        string message)
    {
        string receiverName = (rawReceiverName ?? string.Empty).Trim().TrimStart('@');
        if (receiverName.Length == 0)
        {
            await SendError(senderClient, senderId, "Choose a private-message recipient.");
            return;
        }

        AccountData? cachedReceiver = CacheManager.Instance.GetAccountData(receiverName);
        Account? receiverAccount = cachedReceiver?.accountCachedData;

        if (receiverAccount == null)
        {
            await using ServerDbprojectContext db = new ServerDbprojectContext();
            receiverAccount = await db.Accounts.AsNoTracking()
                .FirstOrDefaultAsync(account => account.Username == receiverName);
        }

        if (receiverAccount == null)
        {
            await SendError(senderClient, senderId, $"User '{receiverName}' does not exist.");
            return;
        }

        int receiverId = receiverAccount.Idaccount;
        byte[] data = BuildChatPacket(PrivateChannel, senderName, receiverId, message);

        await RaceManager.Instance.SendPacketToClient(senderClient, data);

        ClientConnection receiverClient = RaceManager.Instance.GetClientByAccountId(receiverId);
        if (receiverClient != null && receiverClient != senderClient)
            await RaceManager.Instance.SendPacketToClient(receiverClient, data);

        await SavePrivateMessage(senderId, receiverId, message);
    }

    private async Task SendError(ClientConnection client, int senderId, string message)
    {
        byte[] data = BuildChatPacket(SystemChannel, "Server", senderId, message);
        await RaceManager.Instance.SendPacketToClient(client, data);
    }

    private static byte[] BuildChatPacket(int channel, string senderName, int receiverId, string message)
    {
        PacketWriterManager writer = new PacketWriterManager();
        writer.WriteInt((int)EnumCmdCode.chat);
        writer.WriteInt(channel);
        writer.WriteString(senderName);
        writer.WriteInt(receiverId);
        writer.WriteString(message);
        return writer.ToArray();
    }

    private async Task SavePrivateMessage(int senderId, int receiverId, string message)
    {
        Message savedMessage = new Message
        {
            IdaccountFrom = senderId,
            IdaccountTo = receiverId,
            Contents = message
        };

        await using ServerDbprojectContext db = new ServerDbprojectContext();
        db.Messages.Add(savedMessage);
        await db.SaveChangesAsync();
    }
}