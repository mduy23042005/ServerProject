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

        string urlAccount = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/login?username={loginPacket.username}&password={loginPacket.password}";
        HttpResponseMessage res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlAccount);
        string json = await res.Content.ReadAsStringAsync();
        var acc = JsonConvert.DeserializeObject<Account>(json);

        if (acc != null)
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

                //await LoadAccountData(acc);

                time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
                Console.WriteLine($"[Server] {time.ToString("hh:mm:ss tt")} Welcome back {loginResult.username}.");
                RaceManager.Instance.BindAccountToClient(client, acc.Idaccount);
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
    /*
    private async Task LoadAccountData(Account acc)
    {
        HttpResponseMessage res;
        string json;

        string urlEquipment = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/equipment?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlEquipment);
        json = await res.Content.ReadAsStringAsync();
        var equipmentData = JsonConvert.DeserializeObject<List<EquipmentData>>(json);

        string urlInventoryItem0 = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/inventoryItem0?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlInventoryItem0);
        json = await res.Content.ReadAsStringAsync();
        var inventoryItem0Data = JsonConvert.DeserializeObject<List<InventoryItem0Data>>(json);

        string urlInventoryItem1 = $"{WebAPIManager.Instance.GetApiUrl()}/api/account/{acc.IDAccount}/inventoryItem1?idAccount={acc.IDAccount}";
        res = await WebAPIManager.Instance.GetHttpClient().GetAsync(urlInventoryItem1);
        json = await res.Content.ReadAsStringAsync();
        var inventoryItem1Data = JsonConvert.DeserializeObject<List<InventoryItem1Data>>(json);


        AccountData accountData = new AccountData();
        accountData.account = acc;
        accountData.equipments = equipmentData;
        if (inventoryItem0Data != null)
        {
            accountData.inventoryItem0s = inventoryItem0Data;
        }
        if (inventoryItem2Data != null)
        {
            accountData.inventoryItem2s = inventoryItem2Data;
        }
        if (inventoryItem3Data != null)
        {
            accountData.inventoryItem3s = inventoryItem3Data;
        }
        if (inventoryItem4Data != null)
        {
            accountData.inventoryItem4s = inventoryItem4Data;
        }

        accountData.playerData = new PlayerData();
        var playerController = new PlayerController(accountData.account.IDAccount, acc.Point0, acc.Point1, acc.Point2, acc.Point3);
        accountData.playerData.maxHP = playerController.GetMaxHP();
        accountData.playerData.maxMP = playerController.GetMaxMP();
        accountData.playerData.hp = playerController.GetHP();
        accountData.playerData.mp = playerController.GetMP();
        CacheManager.Instance.AddAccountData(accountData);
    }
    */
}
