using Server.Models;
using System;
using System.Collections.Generic;


public class AccountData
{
    public Account account;

    public Account accountCachedData;
}

public class CacheManager
{
    private static readonly Lazy<CacheManager> lazyInstance = new Lazy<CacheManager>(() => new CacheManager());
    public static CacheManager Instance => lazyInstance.Value;

    private Dictionary<int, AccountData> accountsFindByIDAccount;
    private Dictionary<string, AccountData> accountsFindByUsername;

    public void InitCache()
    {
        accountsFindByIDAccount = new Dictionary<int, AccountData>();
        accountsFindByUsername = new Dictionary<string, AccountData>();
    }

    //Account
    public void AddAccountData(AccountData data)
    {
        accountsFindByIDAccount[data.account.Idaccount] = data;
        accountsFindByUsername[data.account.Username] = data;
    }
    public AccountData GetAccountData(int accountId)
    {
        accountsFindByIDAccount.TryGetValue(accountId, out var data);
        return data;
    }
    public AccountData GetAccountData(string username)
    {
        accountsFindByUsername.TryGetValue(username, out var data);
        return data;
    }
    public Dictionary<int, AccountData> GetAllAccountData()
    {
        return accountsFindByIDAccount;
    }

    public void RemoveAccountData(int accountId)
    {
        accountsFindByIDAccount.Remove(accountId);
    }
    public void RemoveAccountData(string username)
    {
        accountsFindByUsername.Remove(username);
    }

    public bool IsAccountOnline(int accountId)
    {
        return accountsFindByIDAccount.ContainsKey(accountId);
    }
    public bool IsAccountOnline(string username)
    {
        return accountsFindByUsername.ContainsKey(username);
    }

    public void ClearAccounts()
    {
        accountsFindByIDAccount.Clear();
        accountsFindByUsername.Clear();
    }
}