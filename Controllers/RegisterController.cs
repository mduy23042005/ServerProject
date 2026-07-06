using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Server.Models;

public class RegisterRequestPacket
{
    public EnumCmdCode cmd;
    public int idSchool;
    public string username;
    public string password;
}
public class RegisterResultPacket
{
    public EnumCmdCode cmd;
    public bool success;
}

class RegisterController
{
    public async Task ClickRegister(ClientConnection client, RegisterRequestPacket registerPacket)
    {
        // Tạo object để gửi lên API
        var registerData = new
        {
            Account = new Account
            {
                Username = registerPacket.username,
                Password = registerPacket.password,
                Role = "User",
                State = "Active"
            },
        };

        string json = JsonConvert.SerializeObject(registerData);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage result = await WebAPIManager.Instance.GetHttpClient().PostAsync($"{WebAPIManager.Instance.GetApiUrl()}/api/account/register", content);

        RegisterResultPacket registerResult;

        if (result.IsSuccessStatusCode)
        {
            Console.WriteLine("Đăng ký thành công!");

            registerResult = new RegisterResultPacket
            {
                cmd = EnumCmdCode.register,
                success = result.IsSuccessStatusCode
            };
        }
        else
        {
            string errorMsg = await result.Content.ReadAsStringAsync();
            Console.WriteLine($"Đăng ký thất bại: {errorMsg}");

            registerResult = new RegisterResultPacket
            {
                cmd = EnumCmdCode.register,
                success = result.IsSuccessStatusCode
            };
        }

        PacketWriterManager writer = new PacketWriterManager();
        writer.WriteInt((int)registerResult.cmd);
        writer.WriteBool(registerResult.success);

        await RaceManager.Instance.SendPacketToClient(client, writer.ToArray());
    }
}