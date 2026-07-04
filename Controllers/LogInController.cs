using Newtonsoft.Json;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

//class hứng dữ liệu từ client gửi lên
public class LogInRequestPacket
{
    public EnumCmdCode cmd;
    public string username;
    public string password;
}
//class trả dữ liệu về client
public class LogInResultPacket
{
    public EnumCmdCode cmd;
    public bool success;
    public int idAccount;
    public string username;
    public string message;
}

public class LogOutRequestPacket
{
    public EnumCmdCode cmd;
    public int idAccount;
}

public class LogInController
{
    private TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
    private DateTime time;

    public async Task ClickLogIn(ClientConnection client, LogInRequestPacket loginPacket)
    {
        LogInResultPacket loginResult;

        var acc = WebSocketServerManager.db.Accounts.FirstOrDefault(a => a.Username == loginPacket.username && a.Password == loginPacket.password);
        bool isValid = acc.State == "Active";

        if (acc != null)
        {
            if (!isValid)
            {
               loginResult = new LogInResultPacket
                {
                    cmd = EnumCmdCode.login,
                    success = false,
                    idAccount = 0,
                    username = null,
                    message = $"Tài khoản {acc.Username} đã bị khóa."
                };
            }
            else
            {
                if (CacheManager.Instance.IsAccountOnline(acc.Idaccount))
                {
                    loginResult = new LogInResultPacket
                    {
                        cmd = EnumCmdCode.login,
                        success = false,
                        idAccount = 0,
                        username = null,
                        message = $"Tài khoản {acc.Username} đang online."
                    };
                }
                else
                {
                    loginResult = new LogInResultPacket
                    {
                        cmd = EnumCmdCode.login,
                        success = acc != null,
                        idAccount = acc.Idaccount,
                        username = acc.Username,
                        message = $"Đăng nhập {acc.Username} thành công."
                    };

                    await LoadAccountData(acc);

                    time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                    Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Welcome back {loginResult.username}.");
                    RaceManager.Instance.BindAccountToClient(client, acc.Idaccount);
                }
            }
        }
        else
        {
            loginResult = new LogInResultPacket
            {
                cmd = EnumCmdCode.login,
                success = false,
                idAccount = 0,
                username = null,
                message = "Username hoặc Password không đúng."
            };
            return;
        }

        PacketWriterManager writer = new PacketWriterManager();
        writer.WriteInt((int)loginResult.cmd);
        writer.WriteBool(loginResult.success);
        writer.WriteInt(loginResult.idAccount);
        writer.WriteString(loginResult.username);
        writer.WriteString(loginResult.message);

        await RaceManager.Instance.SendPacketToClient(client, writer.ToArray());
    }
    private async Task LoadAccountData(Account acc)
    {
        AccountData accountData = new AccountData();
        accountData.account = acc;
        accountData.accountCachedData = acc;
        CacheManager.Instance.AddAccountData(accountData);
    }
}
